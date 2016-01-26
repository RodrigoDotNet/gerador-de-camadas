using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain;
using DataDrain.Factories;
using MySql.Data.MySqlClient;

namespace CorpDAL.DataDrain
{
    internal class FuncoesCrud
    {
        /// <summary>
        /// Lista de parametros do predicado
        /// </summary>
        public static List<MySqlParameter> Parametros { get; set; }

        /// <summary>
        /// Retorna a string do where
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static string RetornaStringWhere<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                ISqlFormatter visitor = new MySqlFormatter();
                Parametros = new List<MySqlParameter>();

                var sql = visitor.Format(predicate, new SqlLanguage());
                GeraParametros(visitor.Parametros);
                return string.Format(" WHERE {0};", sql.Replace("?Parameter?().", ""));
            }
            else
            {
                Parametros = new List<MySqlParameter>();
                return "";
            }
        }

        private static void GeraParametros(IEnumerable<DbParameter> parametros)
        {

            foreach (var param in parametros)
            {
                Parametros.Add(new MySqlParameter
                {
                    ParameterName = RetornaNomeParametro(param.ParameterName),
                    DbType = param.DbType,
                    Value = param.Value
                });
            }

        }

        private static string RetornaNomeParametro(string p)
        {
            var nomeParametro = p;

            var i = 2;
            foreach (var param in Parametros.Where(param => param.ParameterName == p))
            {
                nomeParametro = string.Format("{0}{1}", param.ParameterName, i);
                RetornaNomeParametro(nomeParametro);
                i++;
            }

            return nomeParametro;
        }




        private static void HandlePrimitive(Expression e)
        {
            var be = e as BinaryExpression;

            if (be != null)
            {
                switch (e.NodeType)
                {
                    case ExpressionType.Not:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        RetornaValorParametro(be.Right);
                        break;

                    case ExpressionType.OrElse:
                    case ExpressionType.Or:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Call:
                        GeraParametros(e);
                        break;
                }
            }
            else
            {
                GeraParametros(e);
            }
        }

        private static object RetornaValorParametro(Expression value)
        {
            var innerMember = value as MemberExpression;

            if (innerMember != null)
            {
                var ce = innerMember.Expression as ConstantExpression;
                if (ce != null)
                {
                    var innerObj = ce.Value;
                    return string.Format("'{0}'", innerObj.GetType().GetFields()[0].GetValue(innerObj));
                }
            }
            else
            {
                var m = value as MethodCallExpression;
                object retorno;
                if (m != null)
                {
                    if (m.Arguments.Count == 0)
                    {
                        if (!m.ToString().Contains("ToString()"))
                        {
                            retorno = m.Method.Invoke(m, null);
                        }
                        else
                        {
                            var ce = m.Object as ConstantExpression;
                            if (ce != null)
                            {
                                retorno = ce.Value;
                            }
                            else
                            {
                                retorno = m.ToString().Replace(".ToString()", "");
                            }
                        }
                    }
                    else
                    {
                        var parametros = new List<object>();
                        for (int i = 0; i < m.Arguments.Count; i++)
                        {
                            var ce = m.Arguments[i] as ConstantExpression;
                            if (ce != null)
                            {
                                parametros.Add(ce.Value);
                            }
                            else
                            {
                                var me = m.Arguments[i] as MethodCallExpression;

                                if (me != null)
                                {
                                    parametros.Add(RetornaValorParametro(me));
                                }
                                else
                                {
                                    var mb = m.Arguments[i] as MemberExpression;
                                    if (mb != null)
                                    {
                                        var p = mb.Member as PropertyInfo;

                                        if (p != null)
                                        {
                                            parametros.Add(p.GetValue(p, null));
                                        }
                                    }
                                }
                            }
                        }

                        if (m.ToString().Contains("Equals"))
                        {
                            retorno = string.Format("'{0}'", parametros[0].ToString().Replace("True", "1").Replace("False", "0"));
                        }
                        else if (m.ToString().Contains("Contains("))
                        {
                            var mb = m.Object as MemberExpression;

                            if (mb != null)
                            {
                                var p = mb.Member as PropertyInfo;

                                if (p != null)
                                {
                                    var valorComparar = p.GetValue(mb, null);
                                    return valorComparar.ToString().Contains(m.Arguments.ToString());
                                }
                            }

                            retorno = "";
                        }
                        else
                        {

                            retorno = string.Format("'{0}'", m.Method.Invoke(m, parametros.ToArray()));
                        }
                    }
                    return retorno;
                }
                else
                {
                    var ce = value as ConditionalExpression;

                    if (ce != null)
                    {
                        var valorTrue = ce.IfTrue as ConstantExpression;
                        var valorFalse = ce.IfFalse as ConstantExpression;
                        var metodo = ce.Test as MethodCallExpression;

                        if (metodo != null && valorTrue != null && valorFalse != null)
                        {
                            var valorComparado = metodo.Object as ConstantExpression;
                            var valorComparadoCom = metodo.Arguments[0] as ConstantExpression;

                            if (valorComparado != null && valorComparadoCom != null)
                            {
                                return string.Format("'{0}'", (valorComparado.Equals(valorComparadoCom) ? valorTrue.Value : valorFalse.Value));
                            }
                        }

                    }
                }
            }

            return null;
        }

        private static void GeraParametros(Expression e)
        {
            switch (e.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    var be = e as BinaryExpression;
                    if (be != null)
                    {
                        HandlePrimitive(be.Left);
                        GeraParametros(be.Right);
                    }
                    break;
                case ExpressionType.Call:
                    var mc = e as MethodCallExpression;
                    if (mc != null)
                    {
                        RetornaValorParametro(mc);
                    }
                    break;
                default:
                    HandlePrimitive(e);
                    break;
            }
        }
    }
}
