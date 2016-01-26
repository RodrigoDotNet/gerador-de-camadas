// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DataDrain.ORM.Data.Common.Expressions;

namespace DataDrain.ORM.Data.Common.Language
{
    /// <summary>
    /// Formats a query expression into common SQL language syntax
    /// </summary>
    public class SqlFormatter : DbExpressionVisitor
    {
        StringBuilder sb;
        QueryLanguage language;
        int indent = 2;
        int depth;
        bool hideColumnAliases;
        bool hideTableAliases;
        bool isNested;
        bool forDebug;

        public List<DbParameter> Parametros { get; set; }

        private SqlFormatter(QueryLanguage language, bool forDebug)
        {
            this.language = language;
            this.sb = new StringBuilder();
            this.forDebug = forDebug;
        }

        protected SqlFormatter(QueryLanguage language)
            : this(language, true)
        {
            Parametros = new List<DbParameter>();
        }

        public static string Format(Expression expression, bool forDebug)
        {
            var formatter = new SqlFormatter(null, forDebug);
            formatter.Visit(expression);
            return formatter.ToString();
        }

        public static string Format(Expression expression)
        {
            var formatter = new SqlFormatter(null, false);
            formatter.Visit(expression);
            return formatter.ToString();
        }

        public override string ToString()
        {
            return this.sb.ToString();
        }

        public List<DbParameter> RetornaParametros()
        {
            return Parametros;
        }

        protected virtual QueryLanguage Language
        {
            get { return this.language; }
        }

        protected bool HideColumnAliases
        {
            get { return this.hideColumnAliases; }
            set { this.hideColumnAliases = value; }
        }

        protected bool HideTableAliases
        {
            get { return this.hideTableAliases; }
            set { this.hideTableAliases = value; }
        }

        protected bool IsNested
        {
            get { return this.isNested; }
            set { this.isNested = value; }
        }

        protected bool ForDebug
        {
            get { return this.forDebug; }
        }

        protected enum Indentation
        {
            Same,
            Inner,
            Outer
        }

        public int IndentationWidth
        {
            get { return this.indent; }
            set { this.indent = value; }
        }

        protected void Write(object value)
        {
            this.sb.Append(value);
        }

        protected virtual void WriteParameterName(string name)
        {
            this.Write("@" + name);
        }

        protected virtual void WriteVariableName(string name)
        {
            this.WriteParameterName(name);
        }

        protected virtual void WriteAsAliasName(string aliasName)
        {
            this.Write("AS ");
            this.WriteAliasName(aliasName);
        }

        protected virtual void WriteAliasName(string aliasName)
        {
            this.Write(aliasName);
        }

        protected virtual void WriteAsColumnName(string columnName)
        {
            this.Write("AS ");
            this.WriteColumnName(columnName);
        }

        protected virtual void WriteColumnName(string columnName)
        {
            string name = (this.Language != null) ? this.Language.Quote(columnName) : columnName;
            this.Write(name);
        }

        protected virtual void WriteTableName(string tableName)
        {
            string name = (this.Language != null) ? this.Language.Quote(tableName) : tableName;
            this.Write(name);
        }

        protected void WriteLine(Indentation style)
        {
            sb.AppendLine();
            this.Indent(style);
            for (int i = 0, n = this.depth * this.indent; i < n; i++)
            {
                this.Write(" ");
            }
        }

        protected void Indent(Indentation style)
        {
            if (style == Indentation.Inner)
            {
                this.depth++;
            }
            else if (style == Indentation.Outer)
            {
                this.depth--;
                System.Diagnostics.Debug.Assert(this.depth >= 0);
            }
        }


        protected override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

            // check for supported node types first 
            // non-supported ones should not be visited (as they would produce bad SQL)
            switch (exp.NodeType)
            {
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.MemberAccess:
                case ExpressionType.New:
                case ExpressionType.Lambda:
                    return base.Visit(exp);

                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.ArrayIndex:
                case ExpressionType.TypeIs:
                case ExpressionType.Parameter:

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Invoke:
                case ExpressionType.MemberInit:
                case ExpressionType.ListInit:
                default:
                    if (!forDebug)
                    {
                        throw new NotSupportedException(string.Format("The LINQ expression node of type {0} is not supported", exp.NodeType));
                    }

                    if (IsPredicate(exp))
                    {
                        this.Write("CASE WHEN (");
                        this.Visit(exp);
                        this.Write(") THEN 1 ELSE 0 END");
                        return exp;
                    }
                    else
                    {
                        this.Write(string.Format("?{0}?(", exp.NodeType));
                        base.Visit(exp);
                        this.Write(")");
                        return exp;
                    }
            }
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (this.forDebug)
            {
                this.Visit(m.Expression);

                if (m.Expression != null)
                {
                    if (!m.ToString().Contains("c__DisplayClass"))
                    {
                        this.Write(".");
                        this.Write(m.Member.Name);
                        Parametros.Add(new SqlParameter { ParameterName = RetornaNomeParametro(m.Member.Name) });
                    }
                }
                else
                {
                    WriteValueExpression(m);
                }

                return m;
            }
            else
            {
                throw new NotSupportedException(string.Format("The member access '{0}' is not supported", m.Member));
            }
        }


        private string RetornaNomeParametro(string p)
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

        protected Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" LIKE CONCAT(");
                        this.Visit(m.Arguments[0]);
                        this.Write(",'%'))");
                        this.sb.Replace("=", "");
                        return m;
                    case "EndsWith":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" LIKE CONCAT('%',");
                        this.Visit(m.Arguments[0]);
                        this.Write("))");
                        this.sb.Replace("=", "");
                        return m;
                    case "Contains":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" LIKE CONCAT('%',");
                        this.Visit(m.Arguments[0]);
                        this.Write(",'%'))");
                        this.sb.Replace("=", "");
                        return m;
                    case "Concat":
                        IList<Expression> args = m.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        this.Write("CONCAT(");
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) this.Write(", ");
                            this.Visit(args[i]);
                        }
                        this.Write(")");
                        this.sb.Replace("=", "");
                        return m;
                    case "IsNullOrEmpty":
                        this.Write("(");
                        this.Visit(m.Arguments[0]);
                        this.Write(" IS NULL OR ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" = '')");
                        this.sb.Replace("=", "");
                        return m;
                    case "ToUpper":
                        this.Write("UPPER(");
                        this.Visit(m.Object);
                        this.Write(")");
                        this.sb.Replace("=", "");
                        return m;
                    case "ToLower":
                        this.Write("LOWER(");
                        this.Visit(m.Object);
                        this.Write(")");
                        this.sb.Replace("=", "");
                        return m;
                    case "Replace":
                        this.Write("REPLACE(");
                        this.Visit(m.Object);
                        this.Write(", ");
                        this.Visit(m.Arguments[0]);
                        this.Write(", ");
                        this.Visit(m.Arguments[1]);
                        this.Write(")");
                        this.sb.Replace("=", "");
                        return m;
                    case "Substring":
                        this.Write("SUBSTRING(");
                        this.Visit(m.Object);
                        this.Write(", ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" + 1");
                        if (m.Arguments.Count == 2)
                        {
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                        }
                        this.Write(")");
                        this.sb.Replace("=", "");
                        return m;
                    case "Remove":
                        if (m.Arguments.Count == 1)
                        {
                            this.Write("LEFT(");
                            this.Visit(m.Object);
                            this.Write(", ");
                            this.Visit(m.Arguments[0]);
                            this.Write(")");
                        }
                        else
                        {
                            this.Write("CONCAT(");
                            this.Write("LEFT(");
                            this.Visit(m.Object);
                            this.Write(", ");
                            this.Visit(m.Arguments[0]);
                            this.Write("), SUBSTRING(");
                            this.Visit(m.Object);
                            this.Write(", ");
                            this.Visit(m.Arguments[0]);
                            this.Write(" + ");
                            this.Visit(m.Arguments[1]);
                            this.Write("))");
                        }
                        this.sb.Replace("=", "");
                        return m;
                    case "IndexOf":
                        this.Write("(LOCATE(");
                        this.Visit(m.Arguments[0]);
                        this.Write(", ");
                        this.Visit(m.Object);
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                            this.Write(" + 1");
                        }
                        this.Write(") - 1)");
                        this.sb.Replace("=", "");
                        return m;
                    case "Trim":
                        this.Write("TRIM(");
                        this.Visit(m.Object);
                        this.Write(")");
                        this.sb.Replace("=", "");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case "op_Subtract":
                        if (m.Arguments[1].Type == typeof(DateTime))
                        {
                            this.Write("DATEDIFF(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        }
                        break;
                    case "AddYears":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" YEAR)");
                        return m;
                    case "AddMonths":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" MONTH)");
                        return m;
                    case "AddDays":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" DAY)");
                        return m;
                    case "AddHours":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" HOUR)");
                        return m;
                    case "AddMinutes":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" MINUTE)");
                        return m;
                    case "AddSeconds":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" SECOND)");
                        return m;
                    case "AddMilliseconds":
                        this.Write("DATE_ADD(");
                        this.Visit(m.Object);
                        this.Write(", INTERVAL (");
                        this.Visit(m.Arguments[0]);
                        this.Write("* 1000) MICROSECOND)");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Decimal))
            {
                switch (m.Method.Name)
                {
                    case "Add":
                    case "Subtract":
                    case "Multiply":
                    case "Divide":
                    case "Remainder":
                        this.Write("(");
                        this.VisitValue(m.Arguments[0]);
                        this.Write(" ");
                        this.Write(GetOperator(m.Method.Name));
                        this.Write(" ");
                        this.VisitValue(m.Arguments[1]);
                        this.Write(")");
                        return m;
                    case "Negate":
                        this.Write("-");
                        this.Visit(m.Arguments[0]);
                        this.Write("");
                        return m;
                    case "Ceiling":
                    case "Floor":
                        this.Write(m.Method.Name.ToUpper());
                        this.Write("(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Round":
                        if (m.Arguments.Count == 1)
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(")");
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        }
                        break;
                    case "Truncate":
                        this.Write("TRUNCATE(");
                        this.Visit(m.Arguments[0]);
                        this.Write(",0)");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                switch (m.Method.Name)
                {
                    case "Abs":
                    case "Acos":
                    case "Asin":
                    case "Atan":
                    case "Atan2":
                    case "Cos":
                    case "Exp":
                    case "Log10":
                    case "Sin":
                    case "Tan":
                    case "Sqrt":
                    case "Sign":
                    case "Ceiling":
                    case "Floor":
                        this.Write(m.Method.Name.ToUpper());
                        this.Write("(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Log":
                        if (m.Arguments.Count == 1)
                        {
                            goto case "Log10";
                        }
                        break;
                    case "Pow":
                        this.Write("POWER(");
                        this.Visit(m.Arguments[0]);
                        this.Write(", ");
                        this.Visit(m.Arguments[1]);
                        this.Write(")");
                        return m;
                    case "Round":
                        if (m.Arguments.Count == 1)
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(")");
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        }
                        break;
                    case "Truncate":
                        this.Write("TRUNCATE(");
                        this.Visit(m.Arguments[0]);
                        this.Write(",0)");
                        return m;
                }
            }
            if (m.Method.Name == "ToString")
            {
                if (m.Object.Type != typeof(string))
                {
                    this.Write("CAST(");
                    this.Visit(m.Object);
                    this.Write(" AS CHAR)");
                }
                else
                {
                    this.Visit(m.Object);
                }
                return m;
            }
            else if (!m.Method.IsStatic && m.Method.Name == "CompareTo" && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 1)
            {
                this.Write("(CASE WHEN ");
                this.Visit(m.Object);
                this.Write(" = ");
                this.Visit(m.Arguments[0]);
                this.Write(" THEN 0 WHEN ");
                this.Visit(m.Object);
                this.Write(" < ");
                this.Visit(m.Arguments[0]);
                this.Write(" THEN -1 ELSE 1 END)");
                return m;
            }
            else if (m.Method.IsStatic && m.Method.Name == "Compare" && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 2)
            {
                this.Write("(CASE WHEN ");
                this.Visit(m.Arguments[0]);
                this.Write(" = ");
                this.Visit(m.Arguments[1]);
                this.Write(" THEN 0 WHEN ");
                this.Visit(m.Arguments[0]);
                this.Write(" < ");
                this.Visit(m.Arguments[1]);
                this.Write(" THEN -1 ELSE 1 END)");
                return m;
            }
            return base.VisitMethodCall(m);
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
                    return innerObj.GetType().GetFields()[0].GetValue(innerObj);
                }
                else
                {
                    var mb = value as MemberExpression;
                    var p = mb.Member as PropertyInfo;

                    if (p != null)
                    {
                        return (p.GetValue(p, null));
                    }
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

                        retorno = m.Method.Invoke(m, parametros.ToArray());
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
                                return (valorComparado.Equals(valorComparadoCom) ? valorTrue.Value : valorFalse.Value);
                            }
                        }

                    }
                }
            }

            return null;
        }

        private static Type GetNonNullableType(Type type)
        {
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        protected virtual bool IsInteger(Type type)
        {
            Type nnType = GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (this.forDebug)
            {
                this.Write("?new?");
                this.Write(nex.Type.Name);
                this.Write("(");
                for (int i = 0; i < nex.Arguments.Count; i++)
                {
                    if (i > 0)
                        this.Write(", ");
                    this.Visit(nex.Arguments[i]);
                }
                this.Write(")");
                return nex;
            }
            else
            {
                throw new NotSupportedException(string.Format("The construtor for '{0}' is not supported", nex.Constructor.DeclaringType));
            }
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            string op = this.GetOperator(u);
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    if (IsBoolean(u.Operand.Type) || op.Length > 1)
                    {
                        this.Write(op);
                        this.Write(" ");
                        this.VisitPredicate(u.Operand);
                    }
                    else
                    {
                        this.Write(op);
                        this.VisitValue(u.Operand);
                    }
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    this.Write(op);
                    this.VisitValue(u.Operand);
                    break;
                case ExpressionType.UnaryPlus:
                    this.VisitValue(u.Operand);
                    break;
                case ExpressionType.Convert:
                    // ignore conversions for now
                    this.Visit(u.Operand);
                    break;
                default:
                    if (this.forDebug)
                    {
                        this.Write(string.Format("?{0}?", u.NodeType));
                        this.Write("(");
                        this.Visit(u.Operand);
                        this.Write(")");
                        return u;
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
                    }
            }
            return u;
        }

        //protected override Expression VisitBinary(BinaryExpression b)
        //{
        //    if (b.NodeType == ExpressionType.Power)
        //    {
        //        this.Write("POWER(");
        //        this.VisitValue(b.Left);
        //        this.Write(", ");
        //        this.VisitValue(b.Right);
        //        this.Write(")");
        //        return b;
        //    }
        //    else if (b.NodeType == ExpressionType.Coalesce)
        //    {
        //        this.Write("COALESCE(");
        //        this.VisitValue(b.Left);
        //        this.Write(", ");
        //        Expression right = b.Right;
        //        while (right.NodeType == ExpressionType.Coalesce)
        //        {
        //            BinaryExpression rb = (BinaryExpression)right;
        //            this.VisitValue(rb.Left);
        //            this.Write(", ");
        //            right = rb.Right;
        //        }
        //        this.VisitValue(right);
        //        this.Write(")");
        //        return b;
        //    }
        //    else if (b.NodeType == ExpressionType.LeftShift)
        //    {
        //        this.Write("(");
        //        this.VisitValue(b.Left);
        //        this.Write(" * POWER(2, ");
        //        this.VisitValue(b.Right);
        //        this.Write("))");
        //        return b;
        //    }
        //    else if (b.NodeType == ExpressionType.RightShift)
        //    {
        //        this.Write("(");
        //        this.VisitValue(b.Left);
        //        this.Write(" / POWER(2, ");
        //        this.VisitValue(b.Right);
        //        this.Write("))");
        //        return b;
        //    }
        //    return base.VisitBinary(b);
        //}

        protected override Expression VisitBinary(BinaryExpression b)
        {
            string op = this.GetOperator(b);
            Expression left = b.Left;
            Expression right = b.Right;

            this.Write("(");
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (this.IsBoolean(left.Type))
                    {
                        this.VisitPredicate(left);
                        this.Write(" ");
                        this.Write(op);
                        this.Write(" ");
                        this.VisitPredicate(right);
                    }
                    else
                    {
                        this.VisitValue(left);
                        this.Write(" ");
                        this.Write(op);
                        this.Write(" ");
                        this.VisitValue(right);
                    }
                    break;
                case ExpressionType.Equal:
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)right;
                        if (ce.Value == null)
                        {
                            this.Visit(left);
                            this.Write(" IS NULL");
                            break;
                        }
                    }
                    else if (left.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)left;
                        if (ce.Value == null)
                        {
                            this.Visit(right);
                            this.Write(" IS NULL");
                            break;
                        }
                    }
                    goto case ExpressionType.LessThan;
                case ExpressionType.NotEqual:
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)right;
                        if (ce.Value == null)
                        {
                            this.Visit(left);
                            this.Write(" IS NOT NULL");
                            break;
                        }
                    }
                    else if (left.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression)left;
                        if (ce.Value == null)
                        {
                            this.Visit(right);
                            this.Write(" IS NOT NULL");
                            break;
                        }
                    }
                    goto case ExpressionType.LessThan;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    // check for special x.CompareTo(y) && type.Compare(x,y)
                    if (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Constant)
                    {
                        MethodCallExpression mc = (MethodCallExpression)left;
                        ConstantExpression ce = (ConstantExpression)right;
                        if (ce.Value != null && ce.Value.GetType() == typeof(int) && ((int)ce.Value) == 0)
                        {
                            if (mc.Method.Name == "CompareTo" && !mc.Method.IsStatic && mc.Arguments.Count == 1)
                            {
                                left = mc.Object;
                                right = mc.Arguments[0];
                            }
                            else if (
                                (mc.Method.DeclaringType == typeof(string) || mc.Method.DeclaringType == typeof(decimal))
                                  && mc.Method.Name == "Compare" && mc.Method.IsStatic && mc.Arguments.Count == 2)
                            {
                                left = mc.Arguments[0];
                                right = mc.Arguments[1];
                            }
                        }
                    }
                    goto case ExpressionType.Add;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    this.VisitValue(left);
                    this.Write(" ");
                    this.Write(op);
                    this.Write(" ");
                    //this.VisitValue(right);
                    WriteValueExpression(right);
                    break;
                default:
                    if (this.forDebug)
                    {
                        this.Write(string.Format("?{0}?", b.NodeType));
                        this.Write("(");
                        this.Visit(b.Left);
                        this.Write(", ");
                        this.Visit(b.Right);
                        this.Write(")");
                        return b;
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
                    }
            }
            this.Write(")");
            return b;
        }

        protected virtual string GetOperator(string methodName)
        {
            switch (methodName)
            {
                case "Add": return "+";
                case "Subtract": return "-";
                case "Multiply": return "*";
                case "Divide": return "/";
                case "Negate": return "-";
                case "Remainder": return "%";
                default: return null;
            }
        }

        protected virtual string GetOperator(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return "-";
                case ExpressionType.UnaryPlus:
                    return "+";
                case ExpressionType.Not:
                    return IsBoolean(u.Operand.Type) ? "NOT" : "~";
                default:
                    return "";
            }
        }

        protected virtual string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return (IsBoolean(b.Left.Type)) ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return (IsBoolean(b.Left.Type) ? "OR" : "|");
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    return "";
            }
        }

        protected virtual bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        protected virtual bool IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(((BinaryExpression)expr).Type);
                case ExpressionType.Not:
                    return IsBoolean(((UnaryExpression)expr).Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case (ExpressionType)DbExpressionType.IsNull:
                case (ExpressionType)DbExpressionType.Between:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.In:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(((MethodCallExpression)expr).Type);
                default:
                    return false;
            }
        }

        protected virtual Expression VisitPredicate(Expression expr)
        {
            this.Visit(expr);
            if (!IsPredicate(expr))
            {
                this.Write(" <> 0");
            }
            return expr;
        }

        protected virtual Expression VisitValue(Expression expr)
        {
            return this.Visit(expr);
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (this.IsPredicate(c.Test))
            {
                this.Write("(CASE WHEN ");
                this.VisitPredicate(c.Test);
                this.Write(" THEN ");
                this.VisitValue(c.IfTrue);
                Expression ifFalse = c.IfFalse;
                while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                {
                    ConditionalExpression fc = (ConditionalExpression)ifFalse;
                    this.Write(" WHEN ");
                    this.VisitPredicate(fc.Test);
                    this.Write(" THEN ");
                    this.VisitValue(fc.IfTrue);
                    ifFalse = fc.IfFalse;
                }
                if (ifFalse != null)
                {
                    this.Write(" ELSE ");
                    this.VisitValue(ifFalse);
                }
                this.Write(" END)");
            }
            else
            {
                this.Write("(CASE ");
                this.VisitValue(c.Test);
                this.Write(" WHEN 0 THEN ");
                this.VisitValue(c.IfFalse);
                this.Write(" ELSE ");
                this.VisitValue(c.IfTrue);
                this.Write(" END)");
            }
            return c;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            this.WriteValue(c.Value);
            return c;
        }

        protected virtual void WriteValue(object value)
        {
            if (value == null)
            {
                //this.Write("NULL");
                this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                Parametros[Parametros.Count - 1].Value = null;
            }
            else if (value.GetType().IsEnum)
            {
                //this.Write(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
                this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                Parametros[Parametros.Count - 1].Value = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        //this.Write("'");
                        //this.Write(((bool)value) ? 1 : 0);
                        this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                        //this.Write("'");
                        Parametros[Parametros.Count - 1].Value = ((bool)value) ? 1 : 0;
                        break;
                    case TypeCode.String:
                        //this.Write("'");
                        //this.Write(value);
                        this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                        //this.Write("'");
                        Parametros[Parametros.Count - 1].Value = value;
                        break;
                    case TypeCode.Object:
                        if (value.GetType().IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
                        {
                            //this.Write("'");
                            //this.Write(value.GetType().GetFields()[0].GetValue(value));
                            this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                            //this.Write("'");
                            Parametros[Parametros.Count - 1].Value = value.GetType().GetFields()[0].GetValue(value);
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", value));
                        }
                        break;
                    case TypeCode.Single:
                    case TypeCode.Double:
                        string str = value.ToString();
                        if (!str.Contains('.'))
                        {
                            str += ".0";
                        }
                        //this.Write("'");
                        //this.Write(str);
                        //this.Write("'");
                        this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                        Parametros[Parametros.Count - 1].Value = str;
                        break;
                    default:
                        //this.Write("'");
                        //this.Write(value);
                        //.Write("'");
                        this.Write(string.Format("=@{0}", Parametros[Parametros.Count - 1].ParameterName));
                        Parametros[Parametros.Count - 1].Value = value;
                        break;
                }
            }
            if (value != null)
            {
                Parametros[Parametros.Count - 1].DbType = (DbType)Enum.Parse(typeof(DbType), Parametros[Parametros.Count - 1].Value.GetType().Name);
            }
        }


        private void WriteValueExpression(Expression right)
        {
            try
            {
                var value = Expression.Lambda(right).Compile().DynamicInvoke();

                if (value == null)
                {
                    //this.Write("NULL");
                    this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                    Parametros[Parametros.Count - 1].Value = null;
                }
                else if (value.GetType().IsEnum)
                {
                    //this.Write(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
                    this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                    Parametros[Parametros.Count - 1].Value = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                }
                else
                {
                    switch (Type.GetTypeCode(value.GetType()))
                    {
                        case TypeCode.Boolean:
                            //this.Write("'");
                            //this.Write(((bool)value) ? 1 : 0);
                            this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                            //this.Write("'");
                            Parametros[Parametros.Count - 1].Value = ((bool)value) ? 1 : 0;
                            break;
                        case TypeCode.String:
                            //this.Write("'");
                            //this.Write(value);
                            this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                            //this.Write("'");
                            Parametros[Parametros.Count - 1].Value = value;
                            break;
                        case TypeCode.Object:
                            if (value.GetType().IsDefined(typeof(CompilerGeneratedAttribute), false))
                            {
                                //this.Write("'");
                                //this.Write(value.GetType().GetFields()[0].GetValue(value));
                                this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                                //this.Write("'");
                                Parametros[Parametros.Count - 1].Value = value.GetType().GetFields()[0].GetValue(value);
                            }
                            else
                            {
                                throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", value));
                            }
                            break;
                        case TypeCode.Single:
                        case TypeCode.Double:
                            string str = value.ToString();
                            if (!str.Contains('.'))
                            {
                                str += ".0";
                            }
                            //this.Write("'");
                            //this.Write(str);
                            //this.Write("'");
                            this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                            Parametros[Parametros.Count - 1].Value = str;
                            break;
                        default:
                            //this.Write("'");
                            //this.Write(value);
                            //.Write("'");
                            this.Write(string.Format("@{0}", Parametros[Parametros.Count - 1].ParameterName));
                            Parametros[Parametros.Count - 1].Value = value;
                            break;
                    }
                }
                if (value != null)
                {
                    Parametros[Parametros.Count - 1].DbType = (DbType)Enum.Parse(typeof(DbType), Parametros[Parametros.Count - 1].Value.GetType().Name);
                }
            }
            catch (Exception e)
            {
                Visit(right);
            }
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (column.Alias != null && !this.HideColumnAliases)
            {
                //this.WriteAliasName(column.Alias);
                this.Write(".");
            }
            this.WriteColumnName(column.Name);
            return column;
        }

        protected virtual void WriteTopClause(Expression expression)
        {
            this.Write("TOP (");
            this.Visit(expression);
            this.Write(") ");
        }

        protected virtual void WriteColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            if (columns.Count > 0)
            {
                for (int i = 0, n = columns.Count; i < n; i++)
                {
                    ColumnDeclaration column = columns[i];
                    if (i > 0)
                    {
                        this.Write(", ");
                    }
                    ColumnExpression c = this.VisitValue(column.Expression) as ColumnExpression;
                    if (!string.IsNullOrEmpty(column.Name) && (c == null || c.Name != column.Name))
                    {
                        this.Write(" ");
                        this.WriteAsColumnName(column.Name);
                    }
                }
            }
            else
            {
                this.Write("NULL ");
                if (this.isNested)
                {
                    this.WriteAsColumnName("tmp");
                    this.Write(" ");
                }
            }
        }

        protected virtual void WriteAggregateName(string aggregateName)
        {
            switch (aggregateName)
            {
                case "Average":
                    this.Write("AVG");
                    break;
                case "LongCount":
                    this.Write("COUNT");
                    break;
                default:
                    this.Write(aggregateName.ToUpper());
                    break;
            }
        }

        protected virtual bool RequiresAsteriskWhenNoArgument(string aggregateName)
        {
            return aggregateName == "Count" || aggregateName == "LongCount";
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            this.WriteAggregateName(aggregate.AggregateName);
            this.Write("(");
            if (aggregate.IsDistinct)
            {
                this.Write("DISTINCT ");
            }
            if (aggregate.Argument != null)
            {
                this.VisitValue(aggregate.Argument);
            }
            else if (RequiresAsteriskWhenNoArgument(aggregate.AggregateName))
            {
                this.Write("*");
            }
            this.Write(")");
            return aggregate;
        }

        protected override Expression VisitIsNull(IsNullExpression isnull)
        {
            this.VisitValue(isnull.Expression);
            this.Write(" IS NULL");
            return isnull;
        }

        protected override Expression VisitBetween(BetweenExpression between)
        {
            this.VisitValue(between.Expression);
            this.Write(" BETWEEN ");
            this.VisitValue(between.Lower);
            this.Write(" AND ");
            this.VisitValue(between.Upper);
            return between;
        }


        protected override Expression VisitNamedValue(NamedValueExpression value)
        {
            this.WriteParameterName(value.Name);
            return value;
        }

        protected override Expression VisitIf(IFCommand ifx)
        {
            throw new NotSupportedException();
        }

        protected override Expression VisitVariable(VariableExpression vex)
        {
            this.WriteVariableName(vex.Name);
            return vex;
        }

    }
}
