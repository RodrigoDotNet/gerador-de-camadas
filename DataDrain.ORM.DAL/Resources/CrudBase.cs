using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Apoio.CommandMap;
using Apoio.Conexao;
using Apoio.Enumeradores;
using Apoio.Mapping;
using CorpDAL.DataDrain;
using DataDrain.Mapping;
using MySql.Data.MySqlClient;

namespace CorpDAL.Apoio.Base
{
    public abstract class CrudBase<T>  where T : class, new()
    {
        public virtual List<T> SelectAll()
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {

                    var cmd = CmdMap.CreateDbCommand(new T(), ETipoConsulta.SelectAll);
                    cmd.Connection = cnn;
                    cnn.Open();

                    using (var drGeneric = cmd.ExecuteReader())
                    {
                        return drGeneric.MapToEntities<T>();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual int QuantidadeRegistros()
        {
            int retorno;
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = Singleton.RetornaConexao().CreateCommand();
                    cmd.CommandText = string.Format("SELECT COUNT(*) FROM {0};", CmdMap.NomeTabela(typeof(T)));
                    cmd.Connection = cnn;

                    cnn.Open();
                    retorno = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return retorno;
        }

        public virtual int QuantidadeRegistros(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate", "Expressão não pode ser nula");
            }

            int retorno;
            var cWhere = FuncoesCrud.RetornaStringWhere(predicate);

            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = Singleton.RetornaConexao().CreateCommand();
                    cmd.CommandText = string.Format("SELECT COUNT(*) FROM {0} {1};", CmdMap.NomeTabela(typeof(T)), cWhere);
                    cmd.Connection = cnn;

                    foreach (var param in FuncoesCrud.Parametros)
                    {
                        cmd.Parameters.Add(param);
                    }

                    cnn.Open();

                    retorno = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return retorno;
        }

        public virtual T SelectFirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate", "Expressão não pode ser nula");
            }

            var cWhere = FuncoesCrud.RetornaStringWhere(predicate);
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = CmdMap.CreateDbCommand<T>(typeof(T), cWhere, FuncoesCrud.Parametros);
                    cmd.Connection = cnn;
                    cnn.Open();

                    using (var drGeneric = cmd.ExecuteReader())
                    {
                        return drGeneric.MapToEntities<T>().FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual List<T> Select(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate", "Expressão não pode ser nula");
            }

            var cWhere = FuncoesCrud.RetornaStringWhere(predicate);
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {

                    var cmd = CmdMap.CreateDbCommand<T>(typeof(T), cWhere, FuncoesCrud.Parametros);
                    cmd.Connection = cnn;
                    cnn.Open();

                    using (var drGeneric = cmd.ExecuteReader())
                    {
                        return drGeneric.MapToEntities<T>();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual List<T> Select(Expression<Func<T, bool>> predicate, MySqlConnection cnn, MySqlTransaction trans)
        {
            if (predicate==null)
            {
                throw new ArgumentNullException("predicate","Expressão não pode ser nula");
            }

            if (cnn==null)
            {
                throw new ArgumentNullException("cnn", "Conexão não pode ser nula");
            }

            if (trans == null)
            {
                throw new ArgumentNullException("trans", "Transação não pode ser nula");
            }

            var cWhere = FuncoesCrud.RetornaStringWhere(predicate);
            try
            {
                var cmd = CmdMap.CreateDbCommand<T>(typeof(T), cWhere, FuncoesCrud.Parametros);
                cmd.Connection = cnn;
                cmd.Transaction = trans;
                cnn.Open();

                using (var drGeneric = cmd.ExecuteReader())
                {
                    return drGeneric.MapToEntities<T>();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual T Insert(T objGeneric)
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.InsertWithReturn);
                    cmd.Connection = cnn;

                    cnn.Open();

                    using (var drGeneric = cmd.ExecuteReader())
                    {
                        return drGeneric.MapToEntities<T>().FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Insert(T objGeneric, MySqlConnection cnn, MySqlTransaction trans)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn", "Conexão não pode ser nula");
            }

            if (trans == null)
            {
                throw new ArgumentNullException("trans", "Transação não pode ser nula");
            }

            try
            {
                var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Insert);
                cmd.Connection = cnn;
                cmd.Transaction = trans;
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Insert(List<T> clsGeneric)
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    cnn.Open();
                    Transaction.ExecutaListaComandosSemRetorno(clsGeneric, cnn, ETipoConsulta.Insert);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual T Update(T objGeneric)
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.UpdateWithReturn);
                    cmd.Connection = cnn;
                    cnn.Open();

                    using (var drGeneric = cmd.ExecuteReader())
                    {
                        return drGeneric.MapToEntities<T>().FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Update(T objGeneric, MySqlConnection cnn, MySqlTransaction trans)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn", "Conexão não pode ser nula");
            }

            if (trans == null)
            {
                throw new ArgumentNullException("trans", "Transação não pode ser nula");
            }

            try
            {
                var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Update);
                cmd.Connection = cnn;
                cmd.Transaction = trans;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Update(List<T> clsGeneric)
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    cnn.Open();
                    Transaction.ExecutaListaComandosSemRetorno(clsGeneric, cnn, ETipoConsulta.Update);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Delete(T objGeneric)
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Delete);
                    cmd.Connection = cnn;
                    cnn.Open();

                    cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Delete(T objGeneric, MySqlConnection cnn, MySqlTransaction trans)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn", "Conexão não pode ser nula");
            }

            if (trans == null)
            {
                throw new ArgumentNullException("trans", "Transação não pode ser nula");
            }

            try
            {
                var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Delete);
                cmd.Connection = cnn;
                cmd.Transaction = trans;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public virtual void Delete(List<T> clsGeneric)
        {
            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    cnn.Open();
                    Transaction.ExecutaListaComandosSemRetorno(clsGeneric, cnn, ETipoConsulta.Delete);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}