using System.Collections.Generic;
using System.Linq;
using DataDrain.Rules.Interfaces;

namespace DataDrain.Databases.MySql.Template
{
    public sealed class TemplatesMySql : ITemplateText
    {
        private readonly Dictionary<string, string> _dictionary;

        public TemplatesMySql()
        {
            _dictionary = new Dictionary<string, string> {{"CacheChangedEventArgs", @"using System;
using DataDrain.Caching.Enuns;

namespace DataDrain.Caching.Events
{
    public class CacheChangedEventArgs : EventArgs
    {
        public CacheChangedEventArgs(string chave, ECacheAcao acao)
        {
            Chave = chave;
            Status = acao;
        }

        public string Chave { get; private set; }

        public ECacheAcao Status { get; private set; }
    }
}"}, {"CacheChangedEventHandler", @"
namespace DataDrain.Caching.Events
{
    public delegate void CacheChangedEventHandler(object sender, CacheChangedEventArgs e);
}
"}, {"CachingMannager", @"using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using DataDrain.Caching.Enuns;
using DataDrain.Caching.Events;
using DataDrain.Caching.Interfaces;

namespace DataDrain.Caching
{
    public sealed class CachingMannager : ICachingProvider
    {
        #region Variaveis e Propriedades

        private readonly MemoryCache _cache;
        private readonly CacheItemPolicy _policy;
        private static readonly object Padlock = new object();

        #endregion

        /// <summary>
        /// Seviço de Cache em memoria 
        /// </summary>
        /// <param name='tempoCache'>Tempo que os itens permanecerão em cache a partir do ultimo acesso </param>
        public CachingMannager(TimeSpan tempoCache)
        {
            _cache = new MemoryCache('CachingProvider', new NameValueCollection
            {
                {'pollingInterval', '00:10:00'}, 
                {'physicalMemoryLimitPercentage', '0'},
                {'cacheMemoryLimitMegabytes', '10'}
            });

            _policy = new CacheItemPolicy { SlidingExpiration = tempoCache };
            _policy.RemovedCallback += RemovedCallback;
        }


        #region Métodos base de caching

        /// <summary>
        /// Salva um item em cache
        /// </summary>
        /// <typeparam name='T'>Tipo de objeto</typeparam>
        /// <param name='chave'>Chave de identificação</param>
        /// <param name='valor'>Objeto a ser salvo</param>
        public void Adicionar<T>(string chave, T valor)
        {
            lock (Padlock)
            {
                _cache.Add(chave, valor, _policy);
                OnPropertyChanged(chave, ECacheAcao.Adicionado);
            }
        }

        /// <summary>
        /// Remove um item da cache
        /// </summary>
        /// <param name='chave'>Chave de identificação</param>
        public void Remover(string chave)
        {
            lock (Padlock)
            {
                _cache.Remove(chave);
                OnPropertyChanged(chave, ECacheAcao.Removido);
            }
        }

        /// <summary>
        /// Verifica se um item esta salvo na cache
        /// </summary>
        /// <param name='chave'>Chave de identificação</param>
        /// <returns>Boleano de confirmação</returns>
        public bool Existe(string chave)
        {
            lock (Padlock)
            {
                var res = _cache[chave];

                return res != null;
            }
        }

        /// <summary>
        /// Recupera um item da Cache
        /// </summary>
        /// <typeparam name='T'>Tipo do objeto recuperado</typeparam>
        /// <param name='chave'>Chave de identificação</param>
        /// <param name='removerAposRecuperar'>Indica se deve remover da cache ao recuperar, por padrão é false</param>
        /// <returns>Conjunto chave valor [boleano,Objeto] onde boleano indica se foi possivel recuperar</returns>
        public KeyValuePair<bool, T> Recuperar<T>(string chave, bool removerAposRecuperar = false)
        {
            var retorno = Recuperar(chave, removerAposRecuperar);

            return new KeyValuePair<bool, T>(retorno.Key, retorno.Value != null ? (T)retorno.Value : default(T));
        }

        /// <summary>
        /// Recupera um item da Cache
        /// </summary>
        /// <param name='chave'>Chave de identificação</param>
        /// <param name='removerAposRecuperar'>Indica se deve remover da cache ao recuperar, por padrão é false</param>
        /// <returns>Conjunto chave valor [boleano,Objeto] onde boleano indica se foi possivel recuperar</returns>
        public KeyValuePair<bool, object> Recuperar(string chave, bool removerAposRecuperar = false)
        {
            lock (Padlock)
            {
                var res = _cache[chave];

                if (res != null)
                {
                    if (removerAposRecuperar)
                    {
                        _cache.Remove(chave);
                    }
                }

                return new KeyValuePair<bool, object>(res != null, res);
            }
        }

        /// <summary>
        /// Limpa todas as chaves da cache
        /// </summary>
        public void Clear()
        {
            lock (Padlock)
            {
                foreach (var cacheKey in _cache.Select(kvp => kvp.Key))
                {
                    _cache.Remove(cacheKey);
                }
            }
        }

        #endregion

        #region Evento de status

        /// <summary>
        /// Informa eventos com os itens da cache
        /// </summary>
        public event CacheChangedEventHandler CacheChanged;

        private void OnPropertyChanged(string propertyName, ECacheAcao acao)
        {
            var handler = CacheChanged;
            if (handler != null) handler(this, new CacheChangedEventArgs(propertyName, acao));
        }

        private void RemovedCallback(CacheEntryRemovedArguments arguments)
        {
            OnPropertyChanged(arguments.CacheItem.Key, ECacheAcao.Expirou);
        }

        #endregion

        /// <summary>
        /// Libera o serviço de cache
        /// </summary>
        public void Dispose()
        {
            lock (Padlock)
            {
                _cache.Dispose();
            }
        }
    }
}
"}, {"CmdMap", @"using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using Apoio.Enumeradores;
using DataDrain.Caching;
using usingIDb;

namespace Apoio.CommandMap
{
    internal static class CmdMap
    {
        private static readonly CachingMannager Cache = new CachingMannager(new TimeSpan(0, 0, 20, 0));

        internal static IDbCommand CreateDbCommand<T>(T tipoObjeto, ETipoConsulta sqlType)
        {
            return AjustaCommando(tipoObjeto, RetornaDadosMap<T>(), sqlType);
        }

        internal static IDbCommand CreateDbCommand<T>(Type typeObject, string cWhere, IEnumerable<IDbDataParameter> parametros)
        {
            var mapObj = RetornaDadosMap<T>();
            var cmd = new IDbCommand { CommandText = string.Format('SELECT {0} FROM {1} {2};', string.Join(',', mapObj.Select(p => p.Value.Storage).ToArray()), NomeTabela(typeObject), cWhere) };
            cmd.Parameters.AddRange(parametros.ToArray());

            return cmd;
        }

        private static List<KeyValuePair<PropertyInfo, ColumnAttribute>> RetornaDadosMap<T>()
        {
            var map = Cache.Recuperar<Dictionary<string, List<KeyValuePair<PropertyInfo, ColumnAttribute>>>>('mapCmd').Value ?? new Dictionary<string, List<KeyValuePair<PropertyInfo, ColumnAttribute>>>();

            if (map.ContainsKey(typeof(T).FullName))
            {
                return map[typeof(T).FullName];
            }

            var mapObj = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() != null
                ? typeof(T).GetProperties().Select(p => new KeyValuePair<PropertyInfo, ColumnAttribute>(p, p.GetCustomAttributes(true).Where(att => att is ColumnAttribute).Cast<ColumnAttribute>().FirstOrDefault())).ToList()
                : typeof(T).GetProperties().Select(p => new KeyValuePair<PropertyInfo, ColumnAttribute>(p, new ColumnAttribute { Storage = p.Name, AutoSync = AutoSync.Always })).ToList();

            map.Add(typeof(T).FullName, mapObj);
            Cache.Adicionar('mapCmd', map);

            return mapObj;
        }


        #region .: Metodos :.

        private static IDbCommand AjustaCommando<T>(T genericObj, List<KeyValuePair<PropertyInfo, ColumnAttribute>> colunas, ETipoConsulta tipoConsulta)
        {
            var cmd = new IDbCommand();

            foreach (var coluna in colunas)
            {
                coluna.Value.Storage = coluna.Value.Storage.Replace('[', '').Replace(']', '').Trim();
            }

            string sqlSelect;

            switch (tipoConsulta)
            {
                case ETipoConsulta.SelectAll:
                    cmd.CommandText = string.Format('SELECT {0} FROM {1};', string.Join(', ', colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)));
                    break;

                case ETipoConsulta.Insert:

                    var camposInsert = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => !coluna.Value.IsDbGenerated && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnInsert)))
                    {
                        camposInsert.Add(coluna.Value.Storage);
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    cmd.CommandText = string.Format('INSERT INTO {0} ({1}) VALUES ({2});', NomeTabela(typeof(T)), string.Join(', ', camposInsert.ToArray()), string.Join(' ,@', camposInsert.ToArray()));

                    break;
                case ETipoConsulta.Update:

                    var camposUpdate = new List<string>();
                    var whereUpdate = new List<string>();

                    foreach (var coluna in colunas.OrderByDescending(c => c.Value.IsPrimaryKey))
                    {
                        if (!coluna.Value.IsPrimaryKey && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnUpdate))
                        {
                            camposUpdate.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }

                        if (coluna.Value.IsPrimaryKey)
                        {
                            whereUpdate.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    cmd.CommandText = string.Format('UPDATE {0} SET {1} WHERE {2} ;', NomeTabela(typeof(T)), string.Join(',', camposUpdate.ToArray()), string.Join(' AND ', whereUpdate.ToArray()));

                    break;
                case ETipoConsulta.Delete:

                    var whereDel = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereDel.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    cmd.CommandText = string.Format('DELETE FROM {0} WHERE {1} ;', NomeTabela(typeof(T)), string.Join(' AND ', whereDel.ToArray()));

                    break;
                case ETipoConsulta.InsertWithReturn:

                    var whereIr = new List<string>();
                    var camposIr = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => !coluna.Value.IsDbGenerated && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnInsert)))
                    {
                        camposIr.Add(coluna.Value.Storage);
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    var sqlInsert = string.Format('INSERT INTO {0} ({1}) VALUES ({2});', NomeTabela(typeof(T)), string.Join(', ', camposIr.ToArray()), string.Join(' ,@', camposIr.ToArray()));

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        if (coluna.Value.IsDbGenerated)
                        {
                            whereIr.Add(string.Format('{0} = LAST_INSERT_ID() ', coluna.Value.Storage));
                        }
                        else
                        {
                            whereIr.Add(string.Format('{0} = @{0} ', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    sqlSelect = string.Format('SELECT {0} FROM {1} {2};', string.Join(',', colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)), (whereIr.Count > 0 ? string.Join(' AND ', whereIr.ToArray()) : ' '));

                    cmd.CommandText = sqlInsert + sqlSelect;

                    break;
                case ETipoConsulta.UpdateWithReturn:

                    var whereUp = new List<string>();
                    var camposUp = new List<string>();

                    foreach (var coluna in colunas.OrderByDescending(c => c.Value.IsPrimaryKey))
                    {
                        if (!coluna.Value.IsPrimaryKey && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnUpdate))
                        {
                            camposUp.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }

                        if (coluna.Value.IsPrimaryKey)
                        {
                            camposUp.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    var sqlUpdate = string.Format('UPDATE {0} SET {1} WHERE {2} ;', NomeTabela(typeof(T)), string.Join(',', camposUp.ToArray()), string.Join(' AND ', camposUp.ToArray()));

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereUp.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    sqlSelect = string.Format('SELECT {0} FROM {1} {2};', string.Join(',', colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)), (whereUp.Count > 0 ? string.Join(' AND ', whereUp.ToArray()) : ' '));

                    cmd.CommandText = sqlUpdate + sqlSelect;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return cmd;
        }


        private static void ConfiguraParametro<T>(T genericObj, PropertyInfo property, IDbCommand cmd, string nomeCampo)
        {
            if (cmd.Parameters.IndexOf(nomeCampo) < 0)
            {
                cmd.Parameters.AddWithValue(nomeCampo, property.GetValue(genericObj, null));
            }
        }

        internal static IDbCommand CarregaValoresDbCommand<T>(T objetoAlvo, ETipoConsulta sqlType, IDbCommand cmd)
        {
            var mapObj = RetornaDadosMap<T>();

            try
            {

                foreach (var daoProperty in mapObj)
                    {
                        daoProperty.Value.Storage = daoProperty.Value.Storage.Replace('[', '').Replace(']', '').Trim();
                        switch (sqlType)
                        {
                            case ETipoConsulta.SelectAll:
                                return cmd;

                            case ETipoConsulta.Insert:
                            case ETipoConsulta.Update:
                            case ETipoConsulta.InsertWithReturn:
                            case ETipoConsulta.UpdateWithReturn:
                                if (!daoProperty.Value.IsDbGenerated && (daoProperty.Value.AutoSync != AutoSync.Never))
                                {
                                    ConfiguraValorParametro(objetoAlvo, daoProperty.Key, cmd, daoProperty.Value);
                                }
                                break;
                            case ETipoConsulta.Delete:
                                if (daoProperty.Value.IsPrimaryKey)
                                {
                                    ConfiguraValorParametro(objetoAlvo, daoProperty.Key, cmd, daoProperty.Value);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException('sqlType');
                        }
                    }
                

                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format('Não foi possivel gerar o comando SQL de {0}', Enum.GetName(typeof(ETipoConsulta), sqlType)), ex);
            }
        }

        private static void ConfiguraValorParametro(object objetoAlvo, PropertyInfo property, IDbCommand cmd, DataAttribute daoProperty)
        {
            if (cmd.Parameters.IndexOf(daoProperty.Storage) >= 0)
            {
                cmd.Parameters[daoProperty.Storage].Value = property.GetValue(objetoAlvo, null);
            }
        }

        internal static string NomeTabela(Type typeObject)
        {
            var attribute = (TableAttribute)typeObject.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();

            return attribute == null ? typeObject.Name : attribute.Name;
        }

        #endregion
    }
}
"}, {"CrudBase", @"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Apoio.CommandMap;
using Apoio.Conexao;
using Apoio.Enumeradores;
using Apoio.Mapping;
using {namespace}.DataDrain;
using {namespace}Interfaces;
using DataDrain.Mapping;
using usingIDb;

namespace {namespace}DAL.Apoio.Base
{
    public class CrudBase<T> : IFactory<T> where T : class, new()
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
                    cmd.CommandText = string.Format('SELECT COUNT(*) FROM {0};', CmdMap.NomeTabela(typeof(T)));
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
                throw new ArgumentNullException('predicate', 'Expressão não pode ser nula');
            }

            int retorno;
            var cWhere = FuncoesCrud.RetornaStringWhere(predicate);

            try
            {
                using (var cnn = Singleton.RetornaConexao())
                {
                    var cmd = Singleton.RetornaConexao().CreateCommand();
                    cmd.CommandText = string.Format('SELECT COUNT(*) FROM {0} {1};', CmdMap.NomeTabela(typeof(T)), cWhere);
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
                throw new ArgumentNullException('predicate', 'Expressão não pode ser nula');
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
                throw new ArgumentNullException('predicate', 'Expressão não pode ser nula');
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

        public virtual List<T> Select(Expression<Func<T, bool>> predicate, IDbConnection cnn, IDbTransaction trans)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException('predicate', 'Expressão não pode ser nula');
            }

            if (cnn == null)
            {
                throw new ArgumentNullException('cnn', 'Conexão não pode ser nula');
            }

            if (trans == null)
            {
                throw new ArgumentNullException('trans', 'Transação não pode ser nula');
            }

            var cWhere = FuncoesCrud.RetornaStringWhere(predicate);
            try
            {
                var cmd = CmdMap.CreateDbCommand<T>(typeof(T), cWhere, FuncoesCrud.Parametros);
                cmd.Connection = ({IDbConnection})cnn;
                cmd.Transaction = ({IDbTransaction})trans;
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

        public virtual void Insert(T objGeneric, IDbConnection cnn, IDbTransaction trans)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException('cnn', 'Conexão não pode ser nula');
            }

            if (trans == null)
            {
                throw new ArgumentNullException('trans', 'Transação não pode ser nula');
            }

            try
            {
                var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Insert);
                cmd.Connection = ({IDbConnection})cnn;
                cmd.Transaction = ({IDbTransaction})trans;
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

        public virtual void Update(T objGeneric, IDbConnection cnn, IDbTransaction trans)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException('cnn', 'Conexão não pode ser nula');
            }

            if (trans == null)
            {
                throw new ArgumentNullException('trans', 'Transação não pode ser nula');
            }

            try
            {
                var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Update);
                cmd.Connection = ({IDbConnection})cnn;
                cmd.Transaction = ({IDbTransaction})trans;
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

        public virtual void Delete(T objGeneric, IDbConnection cnn, IDbTransaction trans)
        {
            if (cnn == null)
            {
                throw new ArgumentNullException('cnn', 'Conexão não pode ser nula');
            }

            if (trans == null)
            {
                throw new ArgumentNullException('trans', 'Transação não pode ser nula');
            }

            try
            {
                var cmd = CmdMap.CreateDbCommand(objGeneric, ETipoConsulta.Delete);
                cmd.Connection = ({IDbConnection})cnn;
                cmd.Transaction = ({IDbTransaction})trans;
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
}"}, {"DbExpressions", @"// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DataDrain.ORM.Data.Common.Language;
using DataDrain.ORM.Data.Common.Mapping;

namespace DataDrain.ORM.Data.Common.Expressions
{
    /// <summary>
    /// Extended node types for custom expressions
    /// </summary>
    public enum DbExpressionType
    {
        Table = 1000, // make sure these don't overlap with ExpressionType
        ClientJoin,
        Column,
        Select,
        Projection,
        Entity,
        Join,
        Aggregate,
        Scalar,
        Exists,
        In,
        Grouping,
        AggregateSubquery,
        IsNull,
        Between,
        RowCount,
        NamedValue,
        OuterJoined,
        Insert,
        Update,
        Delete,
        Batch,
        Function,
        Block,
        If,
        Declaration,
        Variable
    }

    public static class DbExpressionTypeExtensions
    {
        public static bool IsDbExpression(this ExpressionType et)
        {
            return ((int)et) >= 1000;
        }
    }

    public abstract class DbExpression : Expression
    {
        protected DbExpression(DbExpressionType eType, Type type)
            : base((ExpressionType)eType, type)
        {
        }

        public override string ToString()
        {
            return '';
        }
    }

    public abstract class AliasedExpression : DbExpression
    {
        TableAlias alias;
        protected AliasedExpression(DbExpressionType nodeType, Type type, TableAlias alias)
            : base(nodeType, type)
        {
            this.alias = alias;
        }
        public TableAlias Alias
        {
            get { return this.alias; }
        }
    }


    /// <summary>
    /// A custom expression node that represents a table reference in a SQL query
    /// </summary>
    public class TableExpression : AliasedExpression
    {
        MappingEntity entity;
        string name;

        public TableExpression(TableAlias alias, MappingEntity entity, string name)
            : base(DbExpressionType.Table, typeof(void), alias)
        {
            this.entity = entity;
            this.name = name;
        }

        public MappingEntity Entity
        {
            get { return this.entity; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public override string ToString()
        {
            return 'T(' + this.Name + ')';
        }
    }

    public class EntityExpression : DbExpression
    {
        MappingEntity entity;
        Expression expression;

        public EntityExpression(MappingEntity entity, Expression expression)
            : base(DbExpressionType.Entity, expression.Type)
        {
            this.entity = entity;
            this.expression = expression;
        }

        public MappingEntity Entity
        {
            get { return this.entity; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }
    }

    /// <summary>
    /// A custom expression node that represents a reference to a column in a SQL query
    /// </summary>
    public class ColumnExpression : DbExpression, IEquatable<ColumnExpression>
    {
        TableAlias alias;
        string name;
        QueryType queryType;

        public ColumnExpression(Type type, QueryType queryType, TableAlias alias, string name)
            : base(DbExpressionType.Column, type)
        {
            if (queryType == null)
                throw new ArgumentNullException('queryType');
            if (name == null)
                throw new ArgumentNullException('name');
            this.alias = alias;
            this.name = name;
            this.queryType = queryType;
        }

        public TableAlias Alias
        {
            get { return this.alias; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public QueryType QueryType
        {
            get { return this.queryType; }
        }

        public override string ToString()
        {
            return this.Alias.ToString() + '.C(' + this.name + ')';
        }

        public override int GetHashCode()
        {
            return alias.GetHashCode() + name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ColumnExpression);
        }

        public bool Equals(ColumnExpression other)
        {
            return other != null
                && ((object)this) == (object)other
                 || (alias == other.alias && name == other.Name);
        }
    }

    public class TableAlias
    {
        public TableAlias()
        {
        }

        public override string ToString()
        {
            return 'A:' + this.GetHashCode();
        }
    }

    /// <summary>
    /// A declaration of a column in a SQL SELECT expression
    /// </summary>
    public class ColumnDeclaration
    {
        string name;
        Expression expression;
        QueryType queryType;

        public ColumnDeclaration(string name, Expression expression, QueryType queryType)
        {
            if (name == null)
                throw new ArgumentNullException('name');
            if (expression == null)
                throw new ArgumentNullException('expression');
            if (queryType == null)
                throw new ArgumentNullException('queryType');
            this.name = name;
            this.expression = expression;
            this.queryType = queryType;
        }

        public string Name
        {
            get { return this.name; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }

        public QueryType QueryType
        {
            get { return this.queryType; }
        }
    }

    /// <summary>
    /// An SQL OrderBy order type 
    /// </summary>
    public enum OrderType
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// A pairing of an expression and an order type for use in a SQL Order By clause
    /// </summary>
    public class OrderExpression
    {
        OrderType orderType;
        Expression expression;
        public OrderExpression(OrderType orderType, Expression expression)
        {
            this.orderType = orderType;
            this.expression = expression;
        }
        public OrderType OrderType
        {
            get { return this.orderType; }
        }
        public Expression Expression
        {
            get { return this.expression; }
        }
    }



    /// <summary>
    /// A kind of SQL join
    /// </summary>
    public enum JoinType
    {
        CrossJoin,
        InnerJoin,
        CrossApply,
        OuterApply,
        LeftOuter,
        SingletonLeftOuter
    }

    /// <summary>
    /// A custom expression node representing a SQL join clause
    /// </summary>
    public class JoinExpression : DbExpression
    {
        JoinType joinType;
        Expression left;
        Expression right;
        Expression condition;

        public JoinExpression(JoinType joinType, Expression left, Expression right, Expression condition)
            : base(DbExpressionType.Join, typeof(void))
        {
            this.joinType = joinType;
            this.left = left;
            this.right = right;
            this.condition = condition;
        }
        public JoinType Join
        {
            get { return this.joinType; }
        }
        public Expression Left
        {
            get { return this.left; }
        }
        public Expression Right
        {
            get { return this.right; }
        }
        public new Expression Condition
        {
            get { return this.condition; }
        }
    }

    public class OuterJoinedExpression : DbExpression
    {
        Expression test;
        Expression expression;

        public OuterJoinedExpression(Expression test, Expression expression)
            : base(DbExpressionType.OuterJoined, expression.Type)
        {
            this.test = test;
            this.expression = expression;
        }

        public Expression Test
        {
            get { return this.test; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }
    }



    public class AggregateExpression : DbExpression
    {
        string aggregateName;
        Expression argument;
        bool isDistinct;
        public AggregateExpression(Type type, string aggregateName, Expression argument, bool isDistinct)
            : base(DbExpressionType.Aggregate, type)
        {
            this.aggregateName = aggregateName;
            this.argument = argument;
            this.isDistinct = isDistinct;
        }
        public string AggregateName
        {
            get { return this.aggregateName; }
        }
        public Expression Argument
        {
            get { return this.argument; }
        }
        public bool IsDistinct
        {
            get { return this.isDistinct; }
        }
    }


    /// <summary>
    /// Allows is-null tests against value-types like int and float
    /// </summary>
    public class IsNullExpression : DbExpression
    {
        Expression expression;
        public IsNullExpression(Expression expression)
            : base(DbExpressionType.IsNull, typeof(bool))
        {
            this.expression = expression;
        }
        public Expression Expression
        {
            get { return this.expression; }
        }
    }

    public class BetweenExpression : DbExpression
    {
        Expression expression;
        Expression lower;
        Expression upper;
        public BetweenExpression(Expression expression, Expression lower, Expression upper)
            : base(DbExpressionType.Between, expression.Type)
        {
            this.expression = expression;
            this.lower = lower;
            this.upper = upper;
        }
        public Expression Expression
        {
            get { return this.expression; }
        }
        public Expression Lower
        {
            get { return this.lower; }
        }
        public Expression Upper
        {
            get { return this.upper; }
        }
    }

    public class NamedValueExpression : DbExpression
    {
        string name;
        QueryType queryType;
        Expression value;

        public NamedValueExpression(string name, QueryType queryType, Expression value)
            : base(DbExpressionType.NamedValue, value.Type)
        {
            if (name == null)
                throw new ArgumentNullException('name');
            //if (queryType == null)
            //throw new ArgumentNullException('queryType');
            if (value == null)
                throw new ArgumentNullException('value');
            this.name = name;
            this.queryType = queryType;
            this.value = value;
        }

        public string Name
        {
            get { return this.name; }
        }

        public QueryType QueryType
        {
            get { return this.queryType; }
        }

        public Expression Value
        {
            get { return this.value; }
        }
    }

    public class BatchExpression : Expression
    {
        Expression input;
        LambdaExpression operation;
        Expression batchSize;
        Expression stream;

        public BatchExpression(Expression input, LambdaExpression operation, Expression batchSize, Expression stream)
            : base((ExpressionType)DbExpressionType.Batch, typeof(IEnumerable<>).MakeGenericType(operation.Body.Type))
        {
            this.input = input;
            this.operation = operation;
            this.batchSize = batchSize;
            this.stream = stream;
        }

        public Expression Input
        {
            get { return this.input; }
        }

        public LambdaExpression Operation
        {
            get { return this.operation; }
        }

        public Expression BatchSize
        {
            get { return this.batchSize; }
        }

        public Expression Stream
        {
            get { return this.stream; }
        }
    }

    public abstract class CommandExpression : DbExpression
    {
        protected CommandExpression(DbExpressionType eType, Type type)
            : base(eType, type)
        {
        }
    }

    public class ColumnAssignment
    {
        ColumnExpression column;
        Expression expression;

        public ColumnAssignment(ColumnExpression column, Expression expression)
        {
            this.column = column;
            this.expression = expression;
        }

        public ColumnExpression Column
        {
            get { return this.column; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }
    }



    public class IFCommand : CommandExpression
    {
        Expression check;
        Expression ifTrue;
        Expression ifFalse;

        public IFCommand(Expression check, Expression ifTrue, Expression ifFalse)
            : base(DbExpressionType.If, ifTrue.Type)
        {
            this.check = check;
            this.ifTrue = ifTrue;
            this.ifFalse = ifFalse;
        }

        public Expression Check
        {
            get { return this.check; }
        }

        public Expression IfTrue
        {
            get { return this.ifTrue; }
        }

        public Expression IfFalse
        {
            get { return this.ifFalse; }
        }
    }

    public class VariableDeclaration
    {
        string name;
        QueryType type;
        Expression expression;

        public VariableDeclaration(string name, QueryType type, Expression expression)
        {
            this.name = name;
            this.type = type;
            this.expression = expression;
        }

        public string Name
        {
            get { return this.name; }
        }

        public QueryType QueryType
        {
            get { return this.type; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }
    }

    public class VariableExpression : Expression
    {
        string name;
        QueryType queryType;

        public VariableExpression(string name, Type type, QueryType queryType)
            : base((ExpressionType)DbExpressionType.Variable, type)
        {
            this.name = name;
            this.queryType = queryType;
        }

        public string Name
        {
            get { return this.name; }
        }

        public QueryType QueryType
        {
            get { return this.queryType; }
        }
    }
}
"}, {"DbExpressionVisitor", @"// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using DataDrain.ORM.Toolkit;
using ExpressionVisitor = DataDrain.ORM.Toolkit.ExpressionVisitor;

namespace DataDrain.ORM.Data.Common.Expressions
{
    /// <summary>
    /// An extended expression visitor including custom DbExpression nodes
    /// </summary>
    public abstract class DbExpressionVisitor : ExpressionVisitor
    {
        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch ((DbExpressionType)exp.NodeType)
            {
                case DbExpressionType.Table:
                    return this.VisitTable((TableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((ColumnExpression)exp);
                case DbExpressionType.Join:
                    return this.VisitJoin((JoinExpression)exp);
                case DbExpressionType.OuterJoined:
                    return this.VisitOuterJoined((OuterJoinedExpression)exp);
                case DbExpressionType.Aggregate:
                    return this.VisitAggregate((AggregateExpression)exp);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.IsNull:
                    return this.VisitIsNull((IsNullExpression)exp);
                case DbExpressionType.Between:
                    return this.VisitBetween((BetweenExpression)exp);
                case DbExpressionType.NamedValue:
                    return this.VisitNamedValue((NamedValueExpression)exp);
                case DbExpressionType.Insert:
                case DbExpressionType.Update:
                case DbExpressionType.Delete:
                case DbExpressionType.If:
                case DbExpressionType.Block:
                case DbExpressionType.Batch:
                    return this.VisitBatch((BatchExpression)exp);
                case DbExpressionType.Variable:
                    return this.VisitVariable((VariableExpression)exp);
                case DbExpressionType.Entity:
                    return this.VisitEntity((EntityExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }

        protected virtual Expression VisitEntity(EntityExpression entity)
        {
            var exp = this.Visit(entity.Expression);
            return this.UpdateEntity(entity, exp);
        }

        protected EntityExpression UpdateEntity(EntityExpression entity, Expression expression)
        {
            if (expression != entity.Expression)
            {
                return new EntityExpression(entity.Entity, expression);
            }
            return entity;
        }

        protected virtual Expression VisitTable(TableExpression table)
        {
            return table;
        }

        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            return column;
        }

        protected virtual Expression VisitJoin(JoinExpression join)
        {
            var left = this.VisitSource(join.Left);
            var right = this.VisitSource(join.Right);
            var condition = this.Visit(join.Condition);
            return this.UpdateJoin(join, join.Join, left, right, condition);
        }

        protected JoinExpression UpdateJoin(JoinExpression join, JoinType joinType, Expression left, Expression right, Expression condition)
        {
            if (joinType != join.Join || left != join.Left || right != join.Right || condition != join.Condition)
            {
                return new JoinExpression(joinType, left, right, condition);
            }
            return join;
        }

        protected virtual Expression VisitOuterJoined(OuterJoinedExpression outer)
        {
            var test = this.Visit(outer.Test);
            var expression = this.Visit(outer.Expression);
            return this.UpdateOuterJoined(outer, test, expression);
        }

        protected OuterJoinedExpression UpdateOuterJoined(OuterJoinedExpression outer, Expression test, Expression expression)
        {
            if (test != outer.Test || expression != outer.Expression)
            {
                return new OuterJoinedExpression(test, expression);
            }
            return outer;
        }

        protected virtual Expression VisitAggregate(AggregateExpression aggregate)
        {
            var arg = this.Visit(aggregate.Argument);
            return this.UpdateAggregate(aggregate, aggregate.Type, aggregate.AggregateName, arg, aggregate.IsDistinct);
        }

        protected AggregateExpression UpdateAggregate(AggregateExpression aggregate, Type type, string aggType, Expression arg, bool isDistinct)
        {
            if (type != aggregate.Type || aggType != aggregate.AggregateName || arg != aggregate.Argument || isDistinct != aggregate.IsDistinct)
            {
                return new AggregateExpression(type, aggType, arg, isDistinct);
            }
            return aggregate;
        }

        protected virtual Expression VisitIsNull(IsNullExpression isnull)
        {
            var expr = this.Visit(isnull.Expression);
            return this.UpdateIsNull(isnull, expr);
        }

        protected IsNullExpression UpdateIsNull(IsNullExpression isnull, Expression expression)
        {
            if (expression != isnull.Expression)
            {
                return new IsNullExpression(expression);
            }
            return isnull;
        }

        protected virtual Expression VisitBetween(BetweenExpression between)
        {
            var expr = this.Visit(between.Expression);
            var lower = this.Visit(between.Lower);
            var upper = this.Visit(between.Upper);
            return this.UpdateBetween(between, expr, lower, upper);
        }

        protected BetweenExpression UpdateBetween(BetweenExpression between, Expression expression, Expression lower, Expression upper)
        {
            if (expression != between.Expression || lower != between.Lower || upper != between.Upper)
            {
                return new BetweenExpression(expression, lower, upper);
            }
            return between;
        }

        protected virtual Expression VisitNamedValue(NamedValueExpression value)
        {
            return value;
        }


        protected virtual Expression VisitSource(Expression source)
        {
            return this.Visit(source);
        }


        protected virtual Expression VisitBatch(BatchExpression batch)
        {
            var operation = (LambdaExpression)this.Visit(batch.Operation);
            var batchSize = this.Visit(batch.BatchSize);
            var stream = this.Visit(batch.Stream);
            return this.UpdateBatch(batch, batch.Input, operation, batchSize, stream);
        }

        protected BatchExpression UpdateBatch(BatchExpression batch, Expression input, LambdaExpression operation, Expression batchSize, Expression stream)
        {
            if (input != batch.Input || operation != batch.Operation || batchSize != batch.BatchSize || stream != batch.Stream)
            {
                return new BatchExpression(input, operation, batchSize, stream);
            }
            return batch;
        }

        protected virtual Expression VisitIf(IFCommand ifx)
        {
            var check = this.Visit(ifx.Check);
            var ifTrue = this.Visit(ifx.IfTrue);
            var ifFalse = this.Visit(ifx.IfFalse);
            return this.UpdateIf(ifx, check, ifTrue, ifFalse);
        }

        protected IFCommand UpdateIf(IFCommand ifx, Expression check, Expression ifTrue, Expression ifFalse)
        {
            if (check != ifx.Check || ifTrue != ifx.IfTrue || ifFalse != ifx.IfFalse)
            {
                return new IFCommand(check, ifTrue, ifFalse);
            }
            return ifx;
        }

        protected virtual Expression VisitVariable(VariableExpression vex)
        {
            return vex;
        }


        protected virtual ColumnAssignment VisitColumnAssignment(ColumnAssignment ca)
        {
            ColumnExpression c = (ColumnExpression)this.Visit(ca.Column);
            Expression e = this.Visit(ca.Expression);
            return this.UpdateColumnAssignment(ca, c, e);
        }

        protected ColumnAssignment UpdateColumnAssignment(ColumnAssignment ca, ColumnExpression c, Expression e)
        {
            if (c != ca.Column || e != ca.Expression)
            {
                return new ColumnAssignment(c, e);
            }
            return ca;
        }

        protected virtual ReadOnlyCollection<ColumnAssignment> VisitColumnAssignments(ReadOnlyCollection<ColumnAssignment> assignments)
        {
            List<ColumnAssignment> alternate = null;
            for (int i = 0, n = assignments.Count; i < n; i++)
            {
                ColumnAssignment assignment = this.VisitColumnAssignment(assignments[i]);
                if (alternate == null && assignment != assignments[i])
                {
                    alternate = assignments.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(assignment);
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return assignments;
        }

        protected virtual ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                ColumnDeclaration column = columns[i];
                Expression e = this.Visit(column.Expression);
                if (alternate == null && e != column.Expression)
                {
                    alternate = columns.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new ColumnDeclaration(column.Name, e, column.QueryType));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return columns;
        }

        protected virtual ReadOnlyCollection<VariableDeclaration> VisitVariableDeclarations(ReadOnlyCollection<VariableDeclaration> decls)
        {
            List<VariableDeclaration> alternate = null;
            for (int i = 0, n = decls.Count; i < n; i++)
            {
                VariableDeclaration decl = decls[i];
                Expression e = this.Visit(decl.Expression);
                if (alternate == null && e != decl.Expression)
                {
                    alternate = decls.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new VariableDeclaration(decl.Name, decl.QueryType, e));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return decls;
        }

        protected virtual ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions != null)
            {
                List<OrderExpression> alternate = null;
                for (int i = 0, n = expressions.Count; i < n; i++)
                {
                    OrderExpression expr = expressions[i];
                    Expression e = this.Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression)
                    {
                        alternate = expressions.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new OrderExpression(expr.OrderType, e));
                    }
                }
                if (alternate != null)
                {
                    return alternate.AsReadOnly();
                }
            }
            return expressions;
        }
    }
}"}, {"DomainValidator", @"using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Este validador verifica se o valor é um dos valores especificados em um conjunto. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DomainValidator : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        private readonly object[] _domain;
        public DomainValidator(object[] domain)
        {
            _domain = domain;
        }

        public override bool IsValid(object value)
        {
            var domainArray = _domain;

            if (value != null)
            {
                if (domainArray.Any(value.Equals))
                {
                    return true;
                }
                var campos = _domain.Select(i => i.ToString()).ToList();
                ErrorMessage = string.Format('O valor '{0}' não foi encontrado no dominio '{1}'', value, string.Join(',', campos.ToArray()));
            }
            ErrorMessage = 'O valor não pode ser nulo';
            return false;
        }
    }
}
"}, {"ECacheAcao", @"
namespace DataDrain.Caching.Enuns
{
    public enum ECacheAcao
    {
        Adicionado,
        Removido,
        Expirou,
        Atualizado
    }
}
"}, {"ETipoConsulta", @"namespace Apoio.Enumeradores
{
    public enum ETipoConsulta
    {
        SelectAll,
        Insert,
        Update,
        Delete,
        InsertWithReturn,
        UpdateWithReturn
    }
}"}, {"ExpiringDictionary", @"using System;
using System.Collections.Generic;
using System.Linq;

namespace TesteDAL.Apoio.Cache
{
    public class ExpiringDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Variaveis

        private readonly Dictionary<TKey, ExpiringValueHolder<TValue>> _innerDictionary;
        private readonly TimeSpan _expiryTimeSpan;
        private System.Timers.Timer timer;

        #endregion

        private void DestoryExpiredItems(TKey key)
        {
            if (_innerDictionary.ContainsKey(key))
            {
                var value = _innerDictionary[key];

                if (value.Expiry < DateTime.Now)
                {
                    _innerDictionary.Remove(key);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name='minutosCache'>tempo em minutos que o objeto ficaria na memooria</param>
        public ExpiringDictionary(Int32 minutosCache)
        {
            if (minutosCache <= 0)
            {
                minutosCache = 5;
            }

            _expiryTimeSpan = new TimeSpan(0, 0, minutosCache, 0);
            _innerDictionary = new Dictionary<TKey, ExpiringValueHolder<TValue>>();

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                foreach (var item in _innerDictionary.Where(item => item.Value.Expiry < DateTime.Now))
                {
                    _innerDictionary.Remove(item.Key);
                }
            };
            timer.Start();
        }

        public void Add(TKey key, TValue value)
        {
            DestoryExpiredItems(key);

            _innerDictionary.Add(key, new ExpiringValueHolder<TValue>(value, _expiryTimeSpan));
        }

        public bool ContainsKey(TKey key)
        {
            DestoryExpiredItems(key);

            return _innerDictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            DestoryExpiredItems(key);

            return _innerDictionary.Remove(key);
        }

        public ICollection<TKey> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var returnval = false;
            DestoryExpiredItems(key);

            if (_innerDictionary.ContainsKey(key))
            {
                value = _innerDictionary[key].Value;
                returnval = true;
            }
            else { value = default(TValue); }

            return returnval;
        }

        public ICollection<TValue> Values
        {
            get { return _innerDictionary.Values.Select(vals => vals.Value).ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                DestoryExpiredItems(key);
                return _innerDictionary[key].Value;
            }
            set
            {
                DestoryExpiredItems(key);
                _innerDictionary[key] = new ExpiringValueHolder<TValue>(value, _expiryTimeSpan);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            DestoryExpiredItems(item.Key);

            _innerDictionary.Add(item.Key, new ExpiringValueHolder<TValue>(item.Value, _expiryTimeSpan));
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
"}, {"ExpiringValueHolder", @"using System;

namespace TesteDAL.Apoio.Cache
{
    internal class ExpiringValueHolder<T>
    {
        public T Value { get; private set; }

        public DateTime Expiry { get; private set; }

        public ExpiringValueHolder(T value, TimeSpan expiresAfter)
        {
            Value = value;
            Expiry = DateTime.Now.Add(expiresAfter);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
"}, {"ExpressionVisitor", @"


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DataDrain.ORM.Toolkit 
{
    public abstract class ExpressionVisitor
    {
        protected ExpressionVisitor()
        {
        }

        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    return this.VisitUnary((UnaryExpression)exp);
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
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                default:
                    return this.VisitUnknown(exp);
            }
        }

        protected virtual Expression VisitUnknown(Expression expression)
        {
            throw new Exception(string.Format('Unhandled expression type: '{0}'', expression.NodeType));
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format('Unhandled binding type '{0}'', binding.BindingType));
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = this.Visit(u.Operand);
            return this.UpdateUnary(u, operand, u.Type, u.Method);
        }

        protected UnaryExpression UpdateUnary(UnaryExpression u, Expression operand, Type resultType, MethodInfo method)
        {
            if (u.Operand != operand || u.Type != resultType || u.Method != method)
            {
                return Expression.MakeUnary(u.NodeType, operand, resultType, method);
            }
            return u;
        }

        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            Expression conversion = this.Visit(b.Conversion);
            return this.UpdateBinary(b, left, right, conversion, b.IsLiftedToNull, b.Method);
        }

        protected BinaryExpression UpdateBinary(BinaryExpression b, Expression left, Expression right, Expression conversion, bool isLiftedToNull, MethodInfo method)
        {
            if (left != b.Left || right != b.Right || conversion != b.Conversion || method != b.Method || isLiftedToNull != b.IsLiftedToNull)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                {
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                }
                else
                {
                    return Expression.MakeBinary(b.NodeType, left, right, isLiftedToNull, method);
                }
            }
            return b;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = this.Visit(b.Expression);
            return this.UpdateTypeIs(b, expr, b.TypeOperand);
        }

        protected TypeBinaryExpression UpdateTypeIs(TypeBinaryExpression b, Expression expression, Type typeOperand)
        {
            if (expression != b.Expression || typeOperand != b.TypeOperand)
            {
                return Expression.TypeIs(expression, typeOperand);
            }
            return b;
        }

        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = this.Visit(c.Test);
            Expression ifTrue = this.Visit(c.IfTrue);
            Expression ifFalse = this.Visit(c.IfFalse);
            return this.UpdateConditional(c, test, ifTrue, ifFalse);
        }

        protected ConditionalExpression UpdateConditional(ConditionalExpression c, Expression test, Expression ifTrue, Expression ifFalse)
        {
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = this.Visit(m.Expression);
            return this.UpdateMemberAccess(m, exp, m.Member);
        }

        protected MemberExpression UpdateMemberAccess(MemberExpression m, Expression expression, MemberInfo member)
        {
            if (expression != m.Expression || member != m.Member)
            {
                return Expression.MakeMemberAccess(expression, member);
            }
            return m;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = this.Visit(m.Object);
            IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
            return this.UpdateMethodCall(m, obj, m.Method, args);
        }

        protected MethodCallExpression UpdateMethodCall(MethodCallExpression m, Expression obj, MethodInfo method, IEnumerable<Expression> args)
        {
            if (obj != m.Object || method != m.Method || args != m.Arguments)
            {
                return Expression.Call(obj, method, args);
            }
            return m;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original != null)
            {
                List<Expression> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    Expression p = this.Visit(original[i]);
                    if (list != null)
                    {
                        list.Add(p);
                    }
                    else if (p != original[i])
                    {
                        list = new List<Expression>(n);
                        for (int j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }
                        list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return original;
        }

        protected virtual ReadOnlyCollection<Expression> VisitMemberAndExpressionList(ReadOnlyCollection<MemberInfo> members, ReadOnlyCollection<Expression> original)
        {
            if (original != null)
            {
                List<Expression> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    Expression p = this.VisitMemberAndExpression(members != null ? members[i] : null, original[i]);
                    if (list != null)
                    {
                        list.Add(p);
                    }
                    else if (p != original[i])
                    {
                        list = new List<Expression>(n);
                        for (int j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }
                        list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return original;
        }

        protected virtual Expression VisitMemberAndExpression(MemberInfo member, Expression expression)
        {
            return this.Visit(expression);
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = this.Visit(assignment.Expression);
            return this.UpdateMemberAssignment(assignment, assignment.Member, e);
        }

        protected MemberAssignment UpdateMemberAssignment(MemberAssignment assignment, MemberInfo member, Expression expression)
        {
            if (expression != assignment.Expression || member != assignment.Member)
            {
                return Expression.Bind(member, expression);
            }
            return assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
            return this.UpdateMemberMemberBinding(binding, binding.Member, bindings);
        }

        protected MemberMemberBinding UpdateMemberMemberBinding(MemberMemberBinding binding, MemberInfo member, IEnumerable<MemberBinding> bindings)
        {
            if (bindings != binding.Bindings || member != binding.Member)
            {
                return Expression.MemberBind(member, bindings);
            }
            return binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
            return this.UpdateMemberListBinding(binding, binding.Member, initializers);
        }

        protected MemberListBinding UpdateMemberListBinding(MemberListBinding binding, MemberInfo member, IEnumerable<ElementInit> initializers)
        {
            if (initializers != binding.Initializers || member != binding.Member)
            {
                return Expression.ListBind(member, initializers);
            }
            return binding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = this.VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = this.VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            return this.UpdateLambda(lambda, lambda.Type, body, lambda.Parameters);
        }

        protected LambdaExpression UpdateLambda(LambdaExpression lambda, Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
        {
            if (body != lambda.Body || parameters != lambda.Parameters || delegateType != lambda.Type)
            {
                return Expression.Lambda(delegateType, body, parameters);
            }
            return lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = this.VisitMemberAndExpressionList(nex.Members, nex.Arguments);
            return this.UpdateNew(nex, nex.Constructor, args, nex.Members);
        }

        protected NewExpression UpdateNew(NewExpression nex, ConstructorInfo constructor, IEnumerable<Expression> args, IEnumerable<MemberInfo> members)
        {
            if (args != nex.Arguments || constructor != nex.Constructor || members != nex.Members)
            {
                if (nex.Members != null)
                {
                    return Expression.New(constructor, args, members);
                }
                else
                {
                    return Expression.New(constructor, args);
                }
            }
            return nex;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
            return this.UpdateMemberInit(init, n, bindings);
        }

        protected MemberInitExpression UpdateMemberInit(MemberInitExpression init, NewExpression nex, IEnumerable<MemberBinding> bindings)
        {
            if (nex != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(nex, bindings);
            }
            return init;
        }

        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = this.VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
            return this.UpdateListInit(init, n, initializers);
        }

        protected ListInitExpression UpdateListInit(ListInitExpression init, NewExpression nex, IEnumerable<ElementInit> initializers)
        {
            if (nex != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(nex, initializers);
            }
            return init;
        }

        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
            return this.UpdateNewArray(na, na.Type, exprs);
        }

        protected NewArrayExpression UpdateNewArray(NewArrayExpression na, Type arrayType, IEnumerable<Expression> expressions)
        {
            if (expressions != na.Expressions || na.Type != arrayType)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(arrayType.GetElementType(), expressions);
                }
                else
                {
                    return Expression.NewArrayBounds(arrayType.GetElementType(), expressions);
                }
            }
            return na;
        }

        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
            Expression expr = this.Visit(iv.Expression);
            return this.UpdateInvocation(iv, expr, args);
        }

        protected InvocationExpression UpdateInvocation(InvocationExpression iv, Expression expression, IEnumerable<Expression> args)
        {
            if (args != iv.Arguments || expression != iv.Expression)
            {
                return Expression.Invoke(expression, args);
            }
            return iv;
        }
    }
}"}, {"FastInvoke", @"using System;
using System.Linq.Expressions;
using System.Reflection;


namespace DataDrain.Mapping
{
public class FastInvoke
    {
        public static Func<T, TReturn> BuildTypedGetter<T, TReturn>(PropertyInfo propertyInfo)
        {
            var reflGet = (Func<T, TReturn>) Delegate.CreateDelegate(typeof(Func<T, TReturn>), propertyInfo.GetGetMethod());
            return reflGet;
        }
 
        public static Action<T, TProperty> BuildTypedSetter<T, TProperty>(PropertyInfo propertyInfo)
        {
            var reflSet = (Action<T, TProperty>)Delegate.CreateDelegate(typeof(Action<T, TProperty>), propertyInfo.GetSetMethod());
            return reflSet;
        }
 
        public static Action<T, object> BuildUntypedSetter<T>(PropertyInfo propertyInfo)
        {
            var targetType = propertyInfo.DeclaringType;
            var methodInfo = propertyInfo.GetSetMethod();
            var exTarget = Expression.Parameter(targetType, 't');
            var exValue = Expression.Parameter(typeof(object), 'p');
            // wir betreiben ein anObject.SetPropertyValue(object)
            var exBody = Expression.Call(exTarget, methodInfo,Expression.Convert(exValue, propertyInfo.PropertyType));
            var lambda = Expression.Lambda<Action<T, object>>(exBody, exTarget, exValue);
            // (t, p) => t.set_StringValue(Convert(p))
 
            var action = lambda.Compile();
            return action;
        }
 
        public static Func<T, object> BuildUntypedGetter<T>(PropertyInfo propertyInfo)
        {
            var targetType = propertyInfo.DeclaringType;
            var methodInfo = propertyInfo.GetGetMethod();
 
            var exTarget = Expression.Parameter(targetType, 't');
            var exBody = Expression.Call(exTarget, methodInfo);
            var exBody2 = Expression.Convert(exBody, typeof(object));
 
            var lambda = Expression.Lambda<Func<T, object>>(exBody2, exTarget);
            // t => Convert(t.get_Foo())
 
            var action = lambda.Compile();
            return action;
        }
    }
}
"}, {"FuncoesCrud", @"using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain;
using DataDrain.Factories;
using usingIDb;

namespace {namespace}DAL.DataDrain
{
    internal class FuncoesCrud
    {
        /// <summary>
        /// Lista de parametros do predicado
        /// </summary>
        public static List<IDbDataParameter> Parametros { get; set; }

        /// <summary>
        /// Retorna a string do where
        /// </summary>
        /// <param name='predicate'></param>
        /// <returns></returns>
        public static string RetornaStringWhere<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                ISqlFormatter visitor = new MySqlFormatter();
                Parametros = new List<IDbDataParameter>();

                var sql = visitor.Format(predicate, new SqlLanguage());
                GeraParametros(visitor.Parametros);
                return string.Format(' WHERE {0};', sql.Replace('?Parameter?().', ''));
            }
            else
            {
                Parametros = new List<IDbDataParameter>();
                return '';
            }
        }

        private static void GeraParametros(IEnumerable<DbParameter> parametros)
        {

            foreach (var param in parametros)
            {
                Parametros.Add(new IDbDataParameter
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
                nomeParametro = string.Format('{0}{1}', param.ParameterName, i);
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
                    return string.Format(''{0}'', innerObj.GetType().GetFields()[0].GetValue(innerObj));
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
                        if (!m.ToString().Contains('ToString()'))
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
                                retorno = m.ToString().Replace('.ToString()', '');
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

                        if (m.ToString().Contains('Equals'))
                        {
                            retorno = string.Format(''{0}'', parametros[0].ToString().Replace('True', '1').Replace('False', '0'));
                        }
                        else if (m.ToString().Contains('Contains('))
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

                            retorno = '';
                        }
                        else
                        {

                            retorno = string.Format(''{0}'', m.Method.Invoke(m, parametros.ToArray()));
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
                                return string.Format(''{0}'', (valorComparado.Equals(valorComparadoCom) ? valorTrue.Value : valorFalse.Value));
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
"}, {"ICachingProvider", @"using System;
using System.Collections.Generic;
using DataDrain.Caching.Events;

namespace DataDrain.Caching.Interfaces
{
    public interface ICachingProvider : IDisposable
    {
        void Adicionar<T>(string chave, T valor);
        void Remover(string chave);
        bool Existe(string chave);
        KeyValuePair<bool, T> Recuperar<T>(string chave, bool removerAposRecuperar = false);
        void Clear();
        event CacheChangedEventHandler CacheChanged;
    }
}"}, {"ISqlFormatter", @"using System.Collections.Generic;
using System.Data.Common;
using DataDrain.ORM.Data.Common.Language;
using System.Linq.Expressions;

namespace DataDrain.Factories
{
    internal interface ISqlFormatter
    {
        string Format(Expression expression, QueryLanguage language);

        List<DbParameter> Parametros { get; set; }
    }
}
"}, {"Map", @"using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using TesteDAL.Apoio.Cache;
using TesteDAL.Apoio.Conexao;
using TesteDAL.Apoio.Enumeradores;

namespace TesteDAL.Apoio.Mapping
{
    internal static class Map
    {
        /// <summary>
        /// Cache Model Mapping dos objetos
        /// </summary>
        private static ExpiringDictionary<Type, ObjectMap> _dictionaryOrm;

        /// <summary>
        /// Cache Object Mapping dos objetos
        /// </summary>
        private static ExpiringDictionary<Type, List<KeyValuePair<PropertyInfo, ColumnAttribute>>> _dictionaryMap;

        /// <summary>
        /// Retorna uma instancia do IDbCommand já configurada
        /// </summary>
        /// <typeparam name='T'>Tipo do Objeto</typeparam>
        /// <param name='dataObject'>Objeto a ser analisado</param>
        /// <param name='sqlType'>Tipo consulta</param>
        /// <returns></returns>
        internal static IDbCommand CreateDbCommand<T>(T dataObject, ETipoConsulta sqlType)
        {
            if (ValidaTableAttribute(dataObject))
            {
                return AjustaCommando(dataObject, RetornaDadosMap<T>(typeof(T)), sqlType);
            }

            throw new NullReferenceException('Objeto não implementa o atributo 'TableAttribute'');
        }

        /// <summary>
        /// Retorna uma instancia do IDbCommand
        /// </summary>
        /// <param name='typeObject'>Objeto a ser analisado</param>
        /// <param name='cWhere'>clausula where</param>
        /// <param name='parametros'>parametros de filtragem</param>
        /// <returns></returns>
        internal static IDbCommand CreateDbCommand<T>(Type typeObject, string cWhere, IEnumerable<IDbDataParameter> parametros)
        {
            if (ValidaTableAttribute(typeObject))
            {
                var tableName = NomeTabela(typeObject);
                var mapObj = RetornaDadosMap<T>(typeObject);

                var cmd = Singleton.RetornaCommando();
                cmd.CommandText = string.Format('SELECT {0} FROM {1} {2};', string.Join(',', mapObj.Select(p => p.Value.Storage).ToArray()), tableName, cWhere);

                foreach (var param in parametros)
                {
                    cmd.Parameters.Add(param);
                }

                return cmd;
            }

            throw new NullReferenceException('Objeto não implementa o atributo 'TableAttribute'');
        }

        /// <summary>
        /// Retorna uma instancia do IDbCommand
        /// </summary>
        /// <typeparam name='T'>Tipo do Objeto</typeparam>
        /// <param name='dataObject'>Objeto a ser analisado</param>
        /// <returns></returns>
        internal static IDbCommand CreateDbCommandProcedure<T>(T dataObject)
        {
            var cmd = Singleton.RetornaCommando();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = NomeTabela(typeof(T));

            return cmd;
        }

        /// <summary>
        /// Carrega a entidade do DataReader para o objeto
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='dr'></param>
        /// <returns></returns>
        internal static T CarregarEntidade<T>(IDataReader dr) where T : new()
        {
            try
            {
                var dadosObjeto = RetornaDadosObjeto<T>(dr);

                #region [ Varre as propriedades atribuindo os valores do DataReader ]

                foreach (var p in dadosObjeto.Propriedades)
                {
                    if (dadosObjeto.NomeCampos.FirstOrDefault(c => p.Value.Storage.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                    {
                        var valorDr = dr[p.Value.Storage.ToLower()];

                        if (valorDr != DBNull.Value)
                        {
                            if (dr[p.Value.Storage.ToLower()] is TimeSpan)
                            {
                                var ts = dr[p.Value.Storage.ToLower()];

                                var valorConvertido = Convert.ChangeType(ts.ToString(), p.Key.PropertyType);
                                p.Key.SetValue(dadosObjeto.ObjetoAlvo, valorConvertido, null);
                            }
                            else if (p.Key.PropertyType.IsGenericType && p.Key.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && p.Key.PropertyType.GetGenericArguments()[0].IsEnum)
                            {
                                p.Key.SetValue(dadosObjeto.ObjetoAlvo, Enum.Parse(p.Key.PropertyType.GetGenericArguments()[0], valorDr.ToString()), null);
                            }
                            else if (!p.Key.PropertyType.IsEnum)
                            {
                                if (Nullable.GetUnderlyingType(p.Key.PropertyType) != null)
                                    p.Key.SetValue(dadosObjeto.ObjetoAlvo, Convert.ChangeType(valorDr, Nullable.GetUnderlyingType(p.Key.PropertyType)), null);
                                else
                                {
                                    var valorConvertido = Convert.ChangeType(valorDr, p.Key.PropertyType);
                                    p.Key.SetValue(dadosObjeto.ObjetoAlvo, valorConvertido, null);
                                }
                            }
                            else
                            {
                                if (valorDr == null || string.IsNullOrEmpty(valorDr.ToString()))
                                {
                                    var myState = Activator.CreateInstance(p.Key.PropertyType);
                                    p.Key.SetValue(dadosObjeto.ObjetoAlvo, Enum.Parse(p.Key.PropertyType, myState.ToString()), null);
                                }
                                else
                                {
                                    p.Key.SetValue(dadosObjeto.ObjetoAlvo, Enum.Parse(p.Key.PropertyType, valorDr.ToString()), null);
                                }
                            }
                        }
                        else
                        {
                            switch (p.Key.PropertyType.FullName)
                            {
                                case 'System.String':
                                    {
                                        var valorConvertido = Convert.ChangeType('', p.Key.PropertyType);
                                        p.Key.SetValue(dadosObjeto.ObjetoAlvo, valorConvertido, null);
                                    }
                                    break;
                                case 'System.DateTime':
                                    {
                                        var valorConvertido = Convert.ChangeType((DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, p.Key.PropertyType);
                                        p.Key.SetValue(dadosObjeto.ObjetoAlvo, valorConvertido, null);
                                    }
                                    break;
                            }
                        }
                    }

                }
                #endregion

                return (T)dadosObjeto.ObjetoAlvo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Verifica se existe uma instancia do Model Mapping do objeto na cache
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='dr'></param>
        /// <returns></returns>
        private static ObjectMap RetornaDadosObjeto<T>(IDataRecord dr) where T : new()
        {
            if (_dictionaryOrm == null)
            {
                _dictionaryOrm = new ExpiringDictionary<Type, ObjectMap>(10);
            }

            if (_dictionaryOrm.ContainsKey(typeof(T)))
            {
                var objMap = _dictionaryOrm[typeof(T)];
                objMap.ObjetoAlvo = new T();
                return objMap;
            }

            var map = new ObjectMap
            {
                Propriedades = typeof(T).GetProperties().Where(p => p.GetCustomAttributes(true).Any(att => att is ColumnAttribute)).Select(p => new KeyValuePair<PropertyInfo, ColumnAttribute>(p, (ColumnAttribute)p.GetCustomAttributes(true).FirstOrDefault(att => att is ColumnAttribute))).ToList(),
                ObjetoAlvo = new T(),
                NomeCampos = new List<string>()
            };

            var numCampos = dr.FieldCount;

            for (var i = 0; i < numCampos; i++)
            {
                map.NomeCampos.Add(dr.GetName(i).ToLower());
            }

            _dictionaryOrm.Add(typeof(T), map);

            return map;
        }

        /// <summary>
        /// Verifica se existe uma instancia do Object Mapping do objeto na cache
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='type'></param>
        /// <returns></returns>
        private static List<KeyValuePair<PropertyInfo, ColumnAttribute>> RetornaDadosMap<T>(Type type)
        {
            List<KeyValuePair<PropertyInfo, ColumnAttribute>> mapObj;

            if (_dictionaryMap == null)
            {
                _dictionaryMap = new ExpiringDictionary<Type, List<KeyValuePair<PropertyInfo, ColumnAttribute>>>(10);
            }

            if (_dictionaryMap.ContainsKey(typeof(T)))
            {
                mapObj = _dictionaryMap[typeof(T)];
            }
            else
            {
                mapObj = type.GetProperties().Select(p => new KeyValuePair<PropertyInfo, ColumnAttribute>(p, p.GetCustomAttributes(true).Where(att => att is ColumnAttribute).Cast<ColumnAttribute>().FirstOrDefault())).ToList();

                _dictionaryMap.Add(new KeyValuePair<Type, List<KeyValuePair<PropertyInfo, ColumnAttribute>>>(typeof(T), mapObj));
            }
            return mapObj;
        }


        #region .: Metodos :.

        /// <summary>
        /// Prepara o IDbCommad gerando a instrução SQL
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='genericObj'>objeto que seta mapeado</param>
        /// <param name='colunas'>Mapa ORM do objeto</param>
        /// <param name='tipoConsulta'>Tipo de consulta que sera gerada</param>
        /// <returns></returns>
        private static IDbCommand AjustaCommando<T>(T genericObj, List<KeyValuePair<PropertyInfo, ColumnAttribute>> colunas, ETipoConsulta tipoConsulta)
        {
            var cmd = Singleton.RetornaCommando();

            foreach (var coluna in colunas)
            {
                coluna.Value.Storage = coluna.Value.Storage.Replace('[', '').Replace(']', '');
            }

            var sqlSelect = string.Empty;

            switch (tipoConsulta)
            {
                case ETipoConsulta.SelectAll:
                    cmd.CommandText = string.Format('SELECT {0} FROM {1};', string.Join(',', colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)));
                    break;

                case ETipoConsulta.Insert:

                    var camposInsert = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => !coluna.Value.IsDbGenerated && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnInsert)))
                    {
                        camposInsert.Add(coluna.Value.Storage);
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    cmd.CommandText = string.Format('INSERT INTO {0} ({1}) VALUES ({2});', NomeTabela(typeof(T)), string.Join(', ', camposInsert.ToArray()), string.Join(' ,@', camposInsert.ToArray()));

                    break;
                case ETipoConsulta.Update:

                    var camposUpdate = new List<string>();
                    var whereUpdate = new List<string>();

                    foreach (var coluna in colunas.OrderByDescending(c => c.Value.IsPrimaryKey))
                    {
                        if (!coluna.Value.IsPrimaryKey && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnUpdate))
                        {
                            camposUpdate.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }

                        if (coluna.Value.IsPrimaryKey)
                        {
                            whereUpdate.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    cmd.CommandText = string.Format('UPDATE {0} SET {1} WHERE {2} ;', NomeTabela(typeof(T)), string.Join(',', camposUpdate.ToArray()), string.Join(' AND ', whereUpdate.ToArray()));

                    break;
                case ETipoConsulta.Delete:

                    var whereDel = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereDel.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    cmd.CommandText = string.Format('DELETE FROM {0} WHERE {1} ;', NomeTabela(typeof(T)), string.Join(' AND ', whereDel.ToArray()));

                    break;
                case ETipoConsulta.InsertWithReturn:

                    var whereIr = new List<string>();
                    var camposIr = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => !coluna.Value.IsDbGenerated && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnInsert)))
                    {
                        camposIr.Add(coluna.Value.Storage);
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    var sqlInsert = string.Format('INSERT INTO {0} ({1}) VALUES ({2});', NomeTabela(typeof(T)), string.Join(', ', camposIr.ToArray()), string.Join(' ,@', camposIr.ToArray()));

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereIr.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage, cmd.GetType().Name, coluna.Value.IsDbGenerated);
                    }

                    sqlSelect = string.Format('SELECT {0} FROM {1} {2};', string.Join(',', colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)), (whereIr.Count > 0 ? string.Join(' AND ', whereIr.ToArray()) : ' '));

                    cmd.CommandText = sqlInsert + sqlSelect;

                    break;
                case ETipoConsulta.UpdateWithReturn:

                    var whereUp = new List<string>();
                    var camposUp = new List<string>();

                    foreach (var coluna in colunas.OrderByDescending(c => c.Value.IsPrimaryKey))
                    {
                        if (!coluna.Value.IsPrimaryKey && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnUpdate))
                        {
                            camposUp.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }

                        if (coluna.Value.IsPrimaryKey)
                        {
                            camposUp.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    var sqlUpdate = string.Format('UPDATE {0} SET {1} WHERE {2} ;', NomeTabela(typeof(T)), string.Join(',', camposUp.ToArray()), string.Join(' AND ', camposUp.ToArray()));

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereUp.Add(string.Format('{0} = @{0}', coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    sqlSelect = string.Format('SELECT {0} FROM {1} {2};', string.Join(',', colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)), (whereUp.Count > 0 ? string.Join(' AND ', whereUp.ToArray()) : ' '));

                    cmd.CommandText = sqlUpdate + sqlSelect;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return cmd;
        }


        private static void ConfiguraParametro<T>(T genericObj, PropertyInfo property, IDbCommand cmd, string nomeCampo, string nomeBanco, bool identity)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = string.Format('@{0}', nomeCampo);

            if (cmd.Parameters.IndexOf(param.ParameterName) < 0)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsEnum)
                {
                    param.DbType = DbType.String;
                }
                else
                {
                    param.DbType = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                                ? (DbType)(propertyType.GetGenericArguments()[0].IsEnum ?
                                        DbType.String :
                                        Enum.Parse(typeof(DbType), propertyType.GetGenericArguments()[0].Name))
                                : (DbType)Enum.Parse(typeof(DbType), property.PropertyType.Name);
                }

                cmd.Parameters.Add(param);

                if (string.IsNullOrEmpty(nomeBanco))
                {
                    param.Value = property.GetValue(genericObj, null) ?? DBNull.Value;
                }
                else
                {
                    switch (nomeCampo)
                    {
                        case 'SqlCommad':
                            param.Value = identity ? 'SCOPE_IDENTITY()' : (property.GetValue(genericObj, null) ?? DBNull.Value);
                            break;

                        case 'MySqlCommand':
                            param.Value = identity ? 'LAST_INSERT_ID()' : (property.GetValue(genericObj, null) ?? DBNull.Value);
                            break;

                        case 'FbCommand':

                            break;


                    }
                }
            }
        }

        private static void ConfiguraParametro<T>(T genericObj, PropertyInfo property, IDbCommand cmd, string nomeCampo)
        {
            ConfiguraParametro(genericObj, property, cmd, nomeCampo, null, false);
        }

        /// <summary>
        /// Carrega os valos em um IDbCommand pre compilado
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='dataObject'></param>
        /// <param name='sqlType'></param>
        /// <param name='commando'></param>
        /// <returns></returns>
        internal static IDbCommand CarregaValoresDbCommand<T>(T dataObject, ETipoConsulta sqlType, IDbCommand commando)
        {
            var cmd = commando;
            //cmd.Parameters.Clear();
            try
            {
                foreach (var property in dataObject.GetType().GetProperties())
                {
                    foreach (var daoProperty in property.GetCustomAttributes(true).Where(att => att is ColumnAttribute).Cast<ColumnAttribute>())
                    {
                        daoProperty.Storage = daoProperty.Storage.Replace('[', '').Replace(']', '');
                        switch (sqlType)
                        {
                            case ETipoConsulta.SelectAll:
                                return commando;

                            case ETipoConsulta.Insert:
                            case ETipoConsulta.Update:
                            case ETipoConsulta.InsertWithReturn:
                            case ETipoConsulta.UpdateWithReturn:
                                if (!daoProperty.IsDbGenerated && (daoProperty.AutoSync != AutoSync.Never))
                                {
                                    ConfiguraValorParametro(dataObject, property, cmd, daoProperty);
                                }
                                break;
                            case ETipoConsulta.Delete:
                                if (daoProperty.IsPrimaryKey)
                                {
                                    ConfiguraValorParametro(dataObject, property, cmd, daoProperty);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException('sqlType');
                        }
                    }
                }

                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format('Não foi possivel gerar o comando SQL de {0}', Enum.GetName(typeof(ETipoConsulta), sqlType)), ex);
            }
        }

        private static void ConfiguraValorParametro(object dataObject, PropertyInfo property, IDbCommand cmd, DataAttribute daoProperty)
        {
            var paramName = string.Format('@{0}', daoProperty.Storage);
            if (cmd.Parameters.IndexOf(paramName) >= 0)
            {
                ((IDbDataParameter)cmd.Parameters[paramName]).Value = property.GetValue(dataObject, null) ?? DBNull.Value;
            }
        }

        internal static string NomeTabela(Type typeObject)
        {
            var attribute = (TableAttribute)typeObject.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            if (attribute == null)
            {
                throw new CustomAttributeFormatException('Classe não se adequada aos atributos DataDrain.ORM');
            }

            return attribute.Name;
        }

        private static bool ValidaTableAttribute<T>(T dataObject)
        {
            var type = dataObject.GetType();
            var attributes = type.GetCustomAttributes(typeof(TableAttribute), true);


            return attributes.Length > 0;
        }

        #endregion
    }
}
"}, {"MapExtension", @"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DataDrain.Caching;


namespace DataDrain.Mapping
{
    internal static class Map
    {
        private static readonly CachingMannager Cache = new CachingMannager(new TimeSpan(0, 0, 5, 0));


        /// <summary>
        /// Mapeia os campos do DataTable para o objeto alvo
        /// </summary>
        /// <typeparam name='T'>Objeto a ser mapeado</typeparam>
        /// <param name='dt'>dados a serem mapeados</param>
        /// <returns>Lista de objetos mapeados</returns>
        public static List<T> MapToEntities<T>(this DataTable dt) where T : class, new()
        {
            var dr = dt.CreateDataReader();

            return dr.MapToEntities<T>();
        }

        /// <summary>
        /// Mapeia os campos do DataView para o objeto alvo
        /// </summary>
        /// <typeparam name='T'>Objeto a ser mapeado</typeparam>
        /// <param name='dv'>dados a serem mapeados</param>
        /// <returns>Lista de objetos mapeados</returns>
        public static List<T> MapToEntities<T>(this DataView dv) where T : class, new()
        {
            var dt = dv.ToTable('Tabela');

            return dt.MapToEntities<T>();
        }

        /// <summary>
        /// Mapeia os campos do dataReader para o objeto alvo
        /// </summary>
        /// <typeparam name='T'>Objeto a ser mapeado</typeparam>
        /// <param name='dr'>dados a serem mapeados</param>
        /// <returns>Lista de objetos mapeados</returns>
        public static List<T> MapToEntities<T>(this IDataReader dr) where T : class ,new()
        {
            try
            {
                var listaNovosObjetos = new List<T>();
                var camposValidos = RetornaMapObjeto<T>(dr);

                var setters = RetornaSetters<T>(camposValidos);

                while (dr.Read())
                {
                    var novoObjeto = new T();

                    foreach (var p in camposValidos)
                    {
                        var valorDr = dr[p.Name];

                        var setter = setters[p.Name];

                        if (valorDr != DBNull.Value)
                        {
                            if (valorDr is TimeSpan && p.PropertyType == typeof(DateTime))
                            {

                                setter(novoObjeto, Convert.ChangeType(valorDr.ToString(), p.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            else if (valorDr is byte[] && p.PropertyType == typeof(string))
                            {
                                setter(novoObjeto, Convert.ChangeType(System.Text.Encoding.Default.GetString((byte[])valorDr), p.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            else if (valorDr is Guid && p.PropertyType == typeof(string))
                            {
                                setter(novoObjeto, Convert.ChangeType(valorDr.ToString(), p.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            else if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && p.PropertyType.GetGenericArguments()[0].IsEnum)
                            {
                                setter(novoObjeto, Enum.Parse(p.PropertyType.GetGenericArguments()[0], valorDr.ToString()));
                            }
                            else if (!p.PropertyType.IsEnum)
                            {
                                setter(novoObjeto, Nullable.GetUnderlyingType(p.PropertyType) != null
                                        ? Convert.ChangeType(valorDr, Nullable.GetUnderlyingType(p.PropertyType), System.Globalization.CultureInfo.InvariantCulture)
                                        : Convert.ChangeType(valorDr, p.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            else if (p.PropertyType.IsEnum)
                            {
                                if (valorDr is string)
                                {
                                    setter(novoObjeto, Enum.Parse(p.PropertyType, valorDr.ToString()));
                                }
                                else
                                {
                                    setter(novoObjeto, Enum.ToObject(p.PropertyType, valorDr));
                                }
                            }
                        }
                        else
                        {
                            if ((p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                            {
                                setter(novoObjeto, null);
                            }
                            else
                            {
                                setter(novoObjeto, !p.PropertyType.IsValueType
                                    ? null
                                    : Activator.CreateInstance(p.PropertyType));
                            }
                        }
                    }
                    listaNovosObjetos.Add(novoObjeto);
                }

                return listaNovosObjetos;

            }
            catch
            {
                throw new Exception();
            }
        }


        private static Dictionary<string, Action<T, object>> RetornaSetters<T>(IEnumerable<PropertyInfo> camposValidos)
        {
            var map = Cache.Recuperar<Dictionary<string, Dictionary<string, Action<T, object>>>>('map').Value ?? new Dictionary<string, Dictionary<string, Action<T, object>>>();

            var mapObjAtual = camposValidos.ToDictionary(camposValido => camposValido.Name, FastInvoke.BuildUntypedSetter<T>);

            if (map.ContainsKey(typeof(T).FullName))
            {
                return map[typeof(T).FullName];
            }

            map.Add(typeof(T).FullName, mapObjAtual);
            Cache.Adicionar('map', map);
            return mapObjAtual;
        }

        private static List<PropertyInfo> RetornaMapObjeto<T>(IDataRecord dr) where T : new()
        {
            if (dr != null)
            {
                return typeof(T).GetProperties().Where(p => p.CanWrite && dr.HasColumn(p.Name)).ToList();
            }

            throw new ArgumentNullException('dr', 'DataReader não pode ser nulo');
        }

        private static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
"}, {"MySqlFormatter", @"using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using DataDrain.Factories;
using DataDrain.ORM.Data.Common.Language;


namespace DataDrain
{
    /// <summary>
    /// Formats a query expression into TSQL language syntax
    /// </summary>
    internal class MySqlFormatter : SqlFormatter, ISqlFormatter
    {
        public List<DbParameter> Parametros { get; set; }

        public MySqlFormatter()
            : base(new SqlLanguage())
        {
        }

        public string Format(Expression expression, QueryLanguage language)
        {
            var formatter = new MySqlFormatter();
            formatter.Visit(expression);
            Parametros = formatter.RetornaParametros();
            return formatter.ToString();
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case 'Length':
                        this.Write('CHAR_LENGTH(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                switch (m.Member.Name)
                {
                    case 'Day':
                        this.Write('DAY(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Month':
                        this.Write('MONTH(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Year':
                        this.Write('YEAR(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Hour':
                        this.Write('HOUR(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Minute':
                        this.Write('MINUTE(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Second':
                        this.Write('SECOND(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Millisecond':
                        this.Write('(MICROSECOND(');
                        this.Visit(m.Expression);
                        this.Write(')/1000)');
                        return m;
                    case 'DayOfWeek':
                        this.Write('(DAYOFWEEK(');
                        this.Visit(m.Expression);
                        this.Write(') - 1)');
                        return m;
                    case 'DayOfYear':
                        this.Write('(DAYOFYEAR(');
                        this.Visit(m.Expression);
                        this.Write(') - 1)');
                        return m;
                }
            }
            return base.VisitMemberAccess(m);
        }

        protected Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case 'StartsWith':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE CONCAT(');
                        this.Visit(m.Arguments[0]);
                        this.Write(','%'))');
                        return m;
                    case 'EndsWith':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE CONCAT('%',');
                        this.Visit(m.Arguments[0]);
                        this.Write('))');
                        return m;
                    case 'Contains':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE CONCAT('%',');
                        this.Visit(m.Arguments[0]);
                        this.Write(','%'))');
                        return m;
                    case 'Concat':
                        IList<Expression> args = m.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        this.Write('CONCAT(');
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) this.Write(', ');
                            this.Visit(args[i]);
                        }
                        this.Write(')');
                        return m;
                    case 'IsNullOrEmpty':
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(' IS NULL OR ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' = '')');
                        return m;
                    case 'ToUpper':
                        this.Write('UPPER(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                    case 'ToLower':
                        this.Write('LOWER(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                    case 'Replace':
                        this.Write('REPLACE(');
                        this.Visit(m.Object);
                        this.Write(', ');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Substring':
                        this.Write('SUBSTRING(');
                        this.Visit(m.Object);
                        this.Write(', ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' + 1');
                        if (m.Arguments.Count == 2)
                        {
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                        }
                        this.Write(')');
                        return m;
                    case 'Remove':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('LEFT(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                        }
                        else
                        {
                            this.Write('CONCAT(');
                            this.Write('LEFT(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write('), SUBSTRING(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write(' + ');
                            this.Visit(m.Arguments[1]);
                            this.Write('))');
                        }
                        return m;
                    case 'IndexOf':
                        this.Write('(LOCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Object);
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(' + 1');
                        }
                        this.Write(') - 1)');
                        return m;
                    case 'Trim':
                        this.Write('TRIM(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case 'op_Subtract':
                        if (m.Arguments[1].Type == typeof(DateTime))
                        {
                            this.Write('DATEDIFF(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'AddYears':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' YEAR)');
                        return m;
                    case 'AddMonths':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' MONTH)');
                        return m;
                    case 'AddDays':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' DAY)');
                        return m;
                    case 'AddHours':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' HOUR)');
                        return m;
                    case 'AddMinutes':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' MINUTE)');
                        return m;
                    case 'AddSeconds':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' SECOND)');
                        return m;
                    case 'AddMilliseconds':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL (');
                        this.Visit(m.Arguments[0]);
                        this.Write('* 1000) MICROSECOND)');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Decimal))
            {
                switch (m.Method.Name)
                {
                    case 'Add':
                    case 'Subtract':
                    case 'Multiply':
                    case 'Divide':
                    case 'Remainder':
                        this.Write('(');
                        this.VisitValue(m.Arguments[0]);
                        this.Write(' ');
                        this.Write(GetOperator(m.Method.Name));
                        this.Write(' ');
                        this.VisitValue(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Negate':
                        this.Write('-');
                        this.Visit(m.Arguments[0]);
                        this.Write('');
                        return m;
                    case 'Ceiling':
                    case 'Floor':
                        this.Write(m.Method.Name.ToUpper());
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Round':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'Truncate':
                        this.Write('TRUNCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(',0)');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                switch (m.Method.Name)
                {
                    case 'Abs':
                    case 'Acos':
                    case 'Asin':
                    case 'Atan':
                    case 'Atan2':
                    case 'Cos':
                    case 'Exp':
                    case 'Log10':
                    case 'Sin':
                    case 'Tan':
                    case 'Sqrt':
                    case 'Sign':
                    case 'Ceiling':
                    case 'Floor':
                        this.Write(m.Method.Name.ToUpper());
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Log':
                        if (m.Arguments.Count == 1)
                        {
                            goto case 'Log10';
                        }
                        break;
                    case 'Pow':
                        this.Write('POWER(');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Round':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'Truncate':
                        this.Write('TRUNCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(',0)');
                        return m;
                }
            }
            if (m.Method.Name == 'ToString')
            {
                if (m.Object.Type != typeof(string))
                {
                    this.Write('CAST(');
                    this.Visit(m.Object);
                    this.Write(' AS CHAR)');
                }
                else
                {
                    this.Visit(m.Object);
                }
                return m;
            }
            else if (!m.Method.IsStatic && m.Method.Name == 'CompareTo' && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 1)
            {
                this.Write('(CASE WHEN ');
                this.Visit(m.Object);
                this.Write(' = ');
                this.Visit(m.Arguments[0]);
                this.Write(' THEN 0 WHEN ');
                this.Visit(m.Object);
                this.Write(' < ');
                this.Visit(m.Arguments[0]);
                this.Write(' THEN -1 ELSE 1 END)');
                return m;
            }
            else if (m.Method.IsStatic && m.Method.Name == 'Compare' && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 2)
            {
                this.Write('(CASE WHEN ');
                this.Visit(m.Arguments[0]);
                this.Write(' = ');
                this.Visit(m.Arguments[1]);
                this.Write(' THEN 0 WHEN ');
                this.Visit(m.Arguments[0]);
                this.Write(' < ');
                this.Visit(m.Arguments[1]);
                this.Write(' THEN -1 ELSE 1 END)');
                return m;
            }
            return base.VisitMethodCall(m);
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Constructor.DeclaringType == typeof(DateTime))
            {
                if (nex.Arguments.Count == 3)
                {
                    this.Write('CAST(CONCAT(');
                    this.Visit(nex.Arguments[0]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[1]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[2]);
                    this.Write(') AS DATETIME)');
                    return nex;
                }
                else if (nex.Arguments.Count == 6)
                {
                    this.Write('CAST(CONCAT(');
                    this.Visit(nex.Arguments[0]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[1]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[2]);
                    this.Write(',' ',');
                    this.Visit(nex.Arguments[3]);
                    this.Write(',':',');
                    this.Visit(nex.Arguments[4]);
                    this.Write(',':',');
                    this.Visit(nex.Arguments[5]);
                    this.Write(') AS DATETIME)');
                    return nex;
                }
            }
            return base.VisitNew(nex);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.Power)
            {
                this.Write('POWER(');
                this.VisitValue(b.Left);
                this.Write(', ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Coalesce)
            {
                this.Write('COALESCE(');
                this.VisitValue(b.Left);
                this.Write(', ');
                Expression right = b.Right;
                while (right.NodeType == ExpressionType.Coalesce)
                {
                    BinaryExpression rb = (BinaryExpression)right;
                    this.VisitValue(rb.Left);
                    this.Write(', ');
                    right = rb.Right;
                }
                this.VisitValue(right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.LeftShift)
            {
                this.Write('(');
                this.VisitValue(b.Left);
                this.Write(' << ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.RightShift)
            {
                this.Write('(');
                this.VisitValue(b.Left);
                this.Write(' >> ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Add && b.Type == typeof(string))
            {
                this.Write('CONCAT(');
                int n = 0;
                this.VisitConcatArg(b.Left, ref n);
                this.VisitConcatArg(b.Right, ref n);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Divide && this.IsInteger(b.Type))
            {
                this.Write('TRUNCATE(');
                base.VisitBinary(b);
                this.Write(',0)');
                return b;
            }
            return base.VisitBinary(b);
        }

        private void VisitConcatArg(Expression e, ref int n)
        {
            if (e.NodeType == ExpressionType.Add && e.Type == typeof(string))
            {
                BinaryExpression b = (BinaryExpression)e;
                VisitConcatArg(b.Left, ref n);
                VisitConcatArg(b.Right, ref n);
            }
            else
            {
                if (n > 0)
                    this.Write(', ');
                this.Visit(e);
                n++;
            }
        }

        protected override Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                this.Write('CASE WHEN (');
                this.Visit(expr);
                this.Write(') THEN 1 ELSE 0 END');
                return expr;
            }
            return base.VisitValue(expr);
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (this.IsPredicate(c.Test))
            {
                this.Write('(CASE WHEN ');
                this.VisitPredicate(c.Test);
                this.Write(' THEN ');
                this.VisitValue(c.IfTrue);
                Expression ifFalse = c.IfFalse;
                while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                {
                    ConditionalExpression fc = (ConditionalExpression)ifFalse;
                    this.Write(' WHEN ');
                    this.VisitPredicate(fc.Test);
                    this.Write(' THEN ');
                    this.VisitValue(fc.IfTrue);
                    ifFalse = fc.IfFalse;
                }
                if (ifFalse != null)
                {
                    this.Write(' ELSE ');
                    this.VisitValue(ifFalse);
                }
                this.Write(' END)');
            }
            else
            {
                this.Write('(CASE ');
                this.VisitValue(c.Test);
                this.Write(' WHEN 0 THEN ');
                this.VisitValue(c.IfFalse);
                this.Write(' ELSE ');
                this.VisitValue(c.IfTrue);
                this.Write(' END)');
            }
            return c;
        }
    }
}

"}, {"ObjectMap", @"using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace TesteDAL.Apoio.Mapping
{
    internal class ObjectMap
    {
        public List<KeyValuePair<PropertyInfo, ColumnAttribute>> Propriedades { get; set; }

        public Object ObjetoAlvo { get; set; }

        public List<string> NomeCampos { get; set; }
    }
}
"}, {"OpcoesParametro", @"using System.Data;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Text;
using TesteDAL.Apoio.Enumeradores;

namespace TesteDAL.Apoio.Mapping
{
    internal class OpcoesParametro
    {
        public PropertyInfo Prop { get; set; }

        public IDbCommand Cmd { get; set; }

        public ColumnAttribute DaoProperty { get; set; }

        public bool HasCondition { get; set; }

        public StringBuilder SbWhere { get; set; }

        public ETipoConsulta TipoConsulta { get; set; }
    }
}
"}, {"QueryExecutor", @"using System;

namespace DataDrain.ORM.Data.Common
{
    public interface ICreateExecutor
    {
        QueryExecutor CreateExecutor();
    }

    public abstract class QueryExecutor
    {
        // called from compiled execution plan
        public abstract int RowsAffected { get; }
        public abstract object Convert(object value, Type type);

    }
}"}, {"QueryLanguage", @"// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain.ORM.Data.Common.Expressions;

namespace DataDrain.ORM.Data.Common.Language
{
    /// <summary>
    /// Defines the language rules for the query provider
    /// </summary>
    public abstract class QueryLanguage
    {
        public abstract QueryTypeSystem TypeSystem { get; }
        public abstract Expression GetGeneratedIdExpression(MemberInfo member);

        public virtual string Quote(string name)
        {
            return name;
        }

        public virtual bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public virtual bool AllowSubqueryInSelectWithoutFrom
        {
            get { return false; }
        }

        public virtual bool AllowDistinctInAggregates
        {
            get { return false; }
        }


        /// <summary>
        /// Determines whether the CLR type corresponds to a scalar data type in the query language
        /// </summary>
        /// <param name='type'></param>
        /// <returns></returns>


        public virtual bool IsAggregate(MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                if (method.DeclaringType == typeof(Queryable)
                    || method.DeclaringType == typeof(Enumerable))
                {
                    switch (method.Name)
                    {
                        case 'Count':
                        case 'LongCount':
                        case 'Sum':
                        case 'Min':
                        case 'Max':
                        case 'Average':
                            return true;
                    }
                }
            }
            var property = member as PropertyInfo;
            if (property != null
                && property.Name == 'Count'
                && typeof(IEnumerable).IsAssignableFrom(property.DeclaringType))
            {
                return true;
            }
            return false;
        }

        public virtual bool AggregateArgumentIsPredicate(string aggregateName)
        {
            return aggregateName == 'Count' || aggregateName == 'LongCount';
        }

        /// <summary>
        /// Determines whether the given expression can be represented as a column in a select expressionss
        /// </summary>
        /// <param name='expression'></param>
        /// <returns></returns>
        public virtual bool CanBeColumn(Expression expression)
        {
            // by default, push all work in projection to client
            return this.MustBeColumn(expression);
        }

        public virtual bool MustBeColumn(Expression expression)
        {
            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

    }

}"}, {"QueryMapping", @"// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain.ORM.Data.Common.Expressions;

namespace DataDrain.ORM.Data.Common.Mapping
{
    public abstract class MappingEntity
    {
        public abstract string TableId { get; }
        public abstract Type ElementType { get; }
        public abstract Type EntityType { get; }
    }

    public struct EntityInfo
    {
        object instance;
        MappingEntity mapping;

        public EntityInfo(object instance, MappingEntity mapping)
        {
            this.instance = instance;
            this.mapping = mapping;
        }

        public object Instance
        {
            get { return this.instance; }
        }

        public MappingEntity Mapping
        {
            get { return this.mapping; }
        }
    }

    public interface IHaveMappingEntity
    {
        MappingEntity Entity { get; }
    }

    /// <summary>
    /// Defines mapping information & rules for the query provider
    /// </summary>
    public abstract class QueryMapping
    {
        /// <summary>
        /// Determines the entity Id based on the type of the entity alone
        /// </summary>
        /// <param name='type'></param>
        /// <returns></returns>
        public virtual string GetTableId(Type type)
        {
            return type.Name;
        }

        /// <summary>
        /// Get the meta entity directly corresponding to the CLR type
        /// </summary>
        /// <param name='type'></param>
        /// <returns></returns>
        public virtual MappingEntity GetEntity(Type type)
        {
            return this.GetEntity(type, this.GetTableId(type));
        }

        /// <summary>
        /// Get the meta entity that maps between the CLR type 'entityType' and the database table, yet
        /// is represented publicly as 'elementType'.
        /// </summary>
        /// <param name='elementType'></param>
        /// <param name='entityID'></param>
        /// <returns></returns>
        public abstract MappingEntity GetEntity(Type elementType, string entityID);

        /// <summary>
        /// Get the meta entity represented by the IQueryable context member
        /// </summary>
        /// <param name='contextMember'></param>
        /// <returns></returns>
        public abstract MappingEntity GetEntity(MemberInfo contextMember);

        public abstract IEnumerable<MemberInfo> GetMappedMembers(MappingEntity entity);

        public abstract bool IsPrimaryKey(MappingEntity entity, MemberInfo member);

        public virtual IEnumerable<MemberInfo> GetPrimaryKeyMembers(MappingEntity entity)
        {
            return this.GetMappedMembers(entity).Where(m => this.IsPrimaryKey(entity, m));
        }

        /// <summary>
        /// Determines if a property is mapped as a relationship
        /// </summary>
        /// <param name='entity'></param>
        /// <param name='member'></param>
        /// <returns></returns>
        public abstract bool IsRelationship(MappingEntity entity, MemberInfo member);


        public abstract object GetPrimaryKey(MappingEntity entity, object instance);
        public abstract Expression GetPrimaryKeyQuery(MappingEntity entity, Expression source, Expression[] keys);
        public abstract IEnumerable<EntityInfo> GetDependentEntities(MappingEntity entity, object instance);
        public abstract IEnumerable<EntityInfo> GetDependingEntities(MappingEntity entity, object instance);
        public abstract object CloneEntity(MappingEntity entity, object instance);
        public abstract bool IsModified(MappingEntity entity, object instance, object original);

        public abstract QueryMapper CreateMapper(object translator);
    }

    public abstract class QueryMapper
    {
        public abstract QueryMapping Mapping { get; }
        public abstract object Translator { get; }

        /// <summary>
        /// Gets an expression that constructs an entity instance relative to a root.
        /// The root is most often a TableExpression, but may be any other experssion such as
        /// a ConstantExpression.
        /// </summary>
        /// <param name='root'></param>
        /// <param name='entity'></param>
        /// <returns></returns>
        public abstract EntityExpression GetEntityExpression(Expression root, MappingEntity entity);

        /// <summary>
        /// Get an expression for a mapped property relative to a root expression. 
        /// The root is either a TableExpression or an expression defining an entity instance.
        /// </summary>
        /// <param name='root'></param>
        /// <param name='entity'></param>
        /// <param name='member'></param>
        /// <returns></returns>
        public abstract Expression GetMemberExpression(Expression root, MappingEntity entity, MemberInfo member);

        /// <summary>
        /// Get an expression that represents the insert operation for the specified instance.
        /// </summary>
        /// <param name='entity'></param>
        /// <param name='instance'>The instance to insert.</param>
        /// <param name='selector'>A lambda expression that computes a return value from the operation.</param>
        /// <returns></returns>
        public abstract Expression GetInsertExpression(MappingEntity entity, Expression instance, LambdaExpression selector);

        /// <summary>
        /// Get an expression that represents the update operation for the specified instance.
        /// </summary>
        /// <param name='entity'></param>
        /// <param name='instance'></param>
        /// <param name='updateCheck'></param>
        /// <param name='selector'></param>
        /// <param name='else'></param>
        /// <returns></returns>
        public abstract Expression GetUpdateExpression(MappingEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression selector, Expression @else);

        /// <summary>
        /// Get an expression that represents the insert-or-update operation for the specified instance.
        /// </summary>
        /// <param name='entity'></param>
        /// <param name='instance'></param>
        /// <param name='updateCheck'></param>
        /// <param name='resultSelector'></param>
        /// <returns></returns>
        public abstract Expression GetInsertOrUpdateExpression(MappingEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression resultSelector);

        /// <summary>
        /// Get an expression that represents the delete operation for the specified instance.
        /// </summary>
        /// <param name='entity'></param>
        /// <param name='instance'></param>
        /// <param name='deleteCheck'></param>
        /// <returns></returns>
        public abstract Expression GetDeleteExpression(MappingEntity entity, Expression instance, LambdaExpression deleteCheck);

        /// <summary>
        /// Recreate the type projection with the additional members included
        /// </summary>
        /// <param name='entity'></param>
        /// <param name='fnIsIncluded'></param>
        /// <returns></returns>
        public abstract EntityExpression IncludeMembers(EntityExpression entity, Func<MemberInfo, bool> fnIsIncluded);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public abstract bool HasIncludedMembers(EntityExpression entity);

        /// <summary>
        /// Apply mapping to a sub query expression
        /// </summary>
        /// <param name='expression'></param>
        /// <returns></returns>
        public virtual Expression ApplyMapping(Expression expression)
        {
            return null;
        }

        /// <summary>
        /// Apply mapping translations to this expression
        /// </summary>
        /// <param name='expression'></param>
        /// <returns></returns>
        public virtual Expression Translate(Expression expression)
        {
            //// convert references to LINQ operators into query specific nodes
            //expression = QueryBinder.Bind(this, expression);

            //// move aggregate computations so they occur in same select as group-by
            //expression = AggregateRewriter.Rewrite(this.Translator.Linguist.Language, expression);

            //// do reduction so duplicate association's are likely to be clumped together
            //expression = UnusedColumnRemover.Remove(expression);
            //expression = RedundantColumnRemover.Remove(expression);
            //expression = RedundantSubqueryRemover.Remove(expression);
            //expression = RedundantJoinRemover.Remove(expression);

            //// convert references to association properties into correlated queries
            //var bound = RelationshipBinder.Bind(this, expression);
            //if (bound != expression)
            //{
            //    expression = bound;
            //    // clean up after ourselves! (multiple references to same association property)
            //    expression = RedundantColumnRemover.Remove(expression);
            //    expression = RedundantJoinRemover.Remove(expression);
            //}

            //// rewrite comparision checks between entities and multi-valued constructs
            //expression = ComparisonRewriter.Rewrite(this.Mapping, expression);

            return null;
        }
    }
}
"}, {"QueryTypeSystem", @"// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace DataDrain.ORM.Data.Common.Language
{
    public abstract class QueryType
    {
        public abstract bool NotNull { get; }
        public abstract int Length { get; }
        public abstract short Precision { get; }
        public abstract short Scale { get; }
    }

    public abstract class QueryTypeSystem 
    {
        public abstract QueryType Parse(string typeDeclaration);
        public abstract QueryType GetColumnType(Type type);
        public abstract string GetVariableDeclaration(QueryType type, bool suppressSize);
    }
}"}, {"Singleton", @"using System;
using System.Configuration;
using usingIDb;

namespace Apoio.Conexao
{
    internal static class Singleton
    {
        public static {IDbConnection} RetornaConexao()
        {
            return new {IDbConnection}(RetornaConnectionString());
        }

        private static string RetornaConnectionString()
        {
            try
            {
                var strConexao = ConfigurationManager.ConnectionStrings['STRING_CONNECTION'].ConnectionString;

                return strConexao;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
"}, {"SqlLanguage", @"using System;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain.ORM.Data.Common.Language;

namespace DataDrain
{

    internal class SqlLanguage : QueryLanguage
    {
        //DbTypeSystem typeSystem = new DbTypeSystem();

        public override QueryTypeSystem TypeSystem
        {
            get { return null; }
        }

        public override bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public override bool AllowDistinctInAggregates
        {
            get { return true; }
        }

        public override Expression GetGeneratedIdExpression(MemberInfo member)
        {
            return null;
        }

        public override string Quote(string name)
        {
            return name;
        }

        private static readonly char[] splitChars = new char[] { '.' };



        public static Type GetMemberType(MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null) return fi.FieldType;
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            EventInfo ei = mi as EventInfo;
            if (ei != null) return ei.EventHandlerType;
            MethodInfo meth = mi as MethodInfo;  // property getters really
            if (meth != null) return meth.ReturnType;
            return null;
        }

        public static readonly QueryLanguage Default = new SqlLanguage();
    }
}"}, {"SqlLiteFormatter", @"using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using DataDrain.ORM.Data.Common.Language;
using TesteDAL.DataDrain;
using TesteDAL.DataDrain.Factories;


namespace DataDrain.ORM.DAL.SQLServer.ExpressionVisitor
{
    /// <summary>
    /// Formats a query expression into TSQL language syntax
    /// </summary>
    internal class SqlLiteFormatter : SqlFormatter, ISqlFormatter
    {
        public List<DbParameter> Parametros { get; set; }

        public SqlLiteFormatter()
            : base(new SqlLanguage())
        {
        }

        public string Format(Expression expression, QueryLanguage language)
        {
            var formatter = new SqlLiteFormatter();
            formatter.Visit(expression);
            Parametros = formatter.RetornaParametros();
            return formatter.ToString();
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case 'Length':
                        this.Write('CHAR_LENGTH(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                switch (m.Member.Name)
                {
                    case 'Day':
                        this.Write('DAY(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Month':
                        this.Write('MONTH(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Year':
                        this.Write('YEAR(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Hour':
                        this.Write('HOUR(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Minute':
                        this.Write('MINUTE(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Second':
                        this.Write('SECOND(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Millisecond':
                        this.Write('(MICROSECOND(');
                        this.Visit(m.Expression);
                        this.Write(')/1000)');
                        return m;
                    case 'DayOfWeek':
                        this.Write('(DAYOFWEEK(');
                        this.Visit(m.Expression);
                        this.Write(') - 1)');
                        return m;
                    case 'DayOfYear':
                        this.Write('(DAYOFYEAR(');
                        this.Visit(m.Expression);
                        this.Write(') - 1)');
                        return m;
                }
            }
            return base.VisitMemberAccess(m);
        }

        protected Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case 'StartsWith':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' || '%')');
                        return m;
                    case 'EndsWith':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE '%' || ');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Contains':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE '%' || ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' || '%'))');
                        return m;
                    case 'Concat':
                        IList<Expression> args = m.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        this.Write('');
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) this.Write(' || ');
                            this.Visit(args[i]);
                        }
                        this.Write(')');
                        return m;
                    case 'IsNullOrEmpty':
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(' IFNULL( ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' ,''))');
                        return m;
                    case 'ToUpper':
                        this.Write('UPPER(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                    case 'ToLower':
                        this.Write('LOWER(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                    case 'Replace':
                        this.Write('REPLACE(');
                        this.Visit(m.Object);
                        this.Write(', ');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Substring':
                        this.Write('substr(');
                        this.Visit(m.Object);
                        this.Write(', ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' + 1');
                        if (m.Arguments.Count == 2)
                        {
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                        }
                        this.Write(')');
                        return m;
                    case 'Remove':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('LEFT(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                        }
                        else
                        {
                            this.Write('');
                            this.Write('LEFT(');
                            this.Visit(m.Object);
                            this.Write(' || ');
                            this.Visit(m.Arguments[0]);
                            this.Write('), SUBSTRING(');
                            this.Visit(m.Object);
                            this.Write(' || ');
                            this.Visit(m.Arguments[0]);
                            this.Write(' + ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                        }
                        return m;
                    case 'IndexOf':
                        this.Write('(LOCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Object);
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(' + 1');
                        }
                        this.Write(') - 1)');
                        return m;
                    case 'Trim':
                        this.Write('TRIM(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case 'op_Subtract':
                        if (m.Arguments[1].Type == typeof(DateTime))
                        {
                            this.Write('DATEDIFF(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'AddYears':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' YEAR)');
                        return m;
                    case 'AddMonths':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' MONTH)');
                        return m;
                    case 'AddDays':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' DAY)');
                        return m;
                    case 'AddHours':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' HOUR)');
                        return m;
                    case 'AddMinutes':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' MINUTE)');
                        return m;
                    case 'AddSeconds':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' SECOND)');
                        return m;
                    case 'AddMilliseconds':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL (');
                        this.Visit(m.Arguments[0]);
                        this.Write('* 1000) MICROSECOND)');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Decimal))
            {
                switch (m.Method.Name)
                {
                    case 'Add':
                    case 'Subtract':
                    case 'Multiply':
                    case 'Divide':
                    case 'Remainder':
                        this.Write('(');
                        this.VisitValue(m.Arguments[0]);
                        this.Write(' ');
                        this.Write(GetOperator(m.Method.Name));
                        this.Write(' ');
                        this.VisitValue(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Negate':
                        this.Write('-');
                        this.Visit(m.Arguments[0]);
                        this.Write('');
                        return m;
                    case 'Ceiling':
                    case 'Floor':
                        this.Write(m.Method.Name.ToUpper());
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Round':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'Truncate':
                        this.Write('TRUNCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(',0)');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                switch (m.Method.Name)
                {
                    case 'Abs':
                    case 'Acos':
                    case 'Asin':
                    case 'Atan':
                    case 'Atan2':
                    case 'Cos':
                    case 'Exp':
                    case 'Log10':
                    case 'Sin':
                    case 'Tan':
                    case 'Sqrt':
                    case 'Sign':
                    case 'Ceiling':
                    case 'Floor':
                        this.Write(m.Method.Name.ToUpper());
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Log':
                        if (m.Arguments.Count == 1)
                        {
                            goto case 'Log10';
                        }
                        break;
                    case 'Pow':
                        this.Write('POWER(');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Round':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'Truncate':
                        this.Write('TRUNCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(',0)');
                        return m;
                }
            }
            if (m.Method.Name == 'ToString')
            {
                if (m.Object.Type != typeof(string))
                {
                    this.Write('CAST(');
                    this.Visit(m.Object);
                    this.Write(' AS CHAR)');
                }
                else
                {
                    this.Visit(m.Object);
                }
                return m;
            }
            else if (!m.Method.IsStatic && m.Method.Name == 'CompareTo' && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 1)
            {
                this.Write('(CASE WHEN ');
                this.Visit(m.Object);
                this.Write(' = ');
                this.Visit(m.Arguments[0]);
                this.Write(' THEN 0 WHEN ');
                this.Visit(m.Object);
                this.Write(' < ');
                this.Visit(m.Arguments[0]);
                this.Write(' THEN -1 ELSE 1 END)');
                return m;
            }
            else if (m.Method.IsStatic && m.Method.Name == 'Compare' && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 2)
            {
                this.Write('(CASE WHEN ');
                this.Visit(m.Arguments[0]);
                this.Write(' = ');
                this.Visit(m.Arguments[1]);
                this.Write(' THEN 0 WHEN ');
                this.Visit(m.Arguments[0]);
                this.Write(' < ');
                this.Visit(m.Arguments[1]);
                this.Write(' THEN -1 ELSE 1 END)');
                return m;
            }
            return base.VisitMethodCall(m);
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Constructor.DeclaringType == typeof(DateTime))
            {
                if (nex.Arguments.Count == 3)
                {
                    this.Write('CAST(CONCAT(');
                    this.Visit(nex.Arguments[0]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[1]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[2]);
                    this.Write(') AS DATETIME)');
                    return nex;
                }
                else if (nex.Arguments.Count == 6)
                {
                    this.Write('CAST(CONCAT(');
                    this.Visit(nex.Arguments[0]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[1]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[2]);
                    this.Write(',' ',');
                    this.Visit(nex.Arguments[3]);
                    this.Write(',':',');
                    this.Visit(nex.Arguments[4]);
                    this.Write(',':',');
                    this.Visit(nex.Arguments[5]);
                    this.Write(') AS DATETIME)');
                    return nex;
                }
            }
            return base.VisitNew(nex);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.Power)
            {
                this.Write('POWER(');
                this.VisitValue(b.Left);
                this.Write(', ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Coalesce)
            {
                this.Write('COALESCE(');
                this.VisitValue(b.Left);
                this.Write(', ');
                Expression right = b.Right;
                while (right.NodeType == ExpressionType.Coalesce)
                {
                    BinaryExpression rb = (BinaryExpression)right;
                    this.VisitValue(rb.Left);
                    this.Write(', ');
                    right = rb.Right;
                }
                this.VisitValue(right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.LeftShift)
            {
                this.Write('(');
                this.VisitValue(b.Left);
                this.Write(' << ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.RightShift)
            {
                this.Write('(');
                this.VisitValue(b.Left);
                this.Write(' >> ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Add && b.Type == typeof(string))
            {

                int n = 0;
                this.Write('(');
                this.VisitConcatArg(b.Left, ref n);
                this.Write(' || ');
                this.VisitConcatArg(b.Right, ref n);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Divide && this.IsInteger(b.Type))
            {
                this.Write('TRUNCATE(');
                base.VisitBinary(b);
                this.Write(',0)');
                return b;
            }
            return base.VisitBinary(b);
        }

        private void VisitConcatArg(Expression e, ref int n)
        {
            if (e.NodeType == ExpressionType.Add && e.Type == typeof(string))
            {
                BinaryExpression b = (BinaryExpression)e;
                VisitConcatArg(b.Left, ref n);
                VisitConcatArg(b.Right, ref n);
            }
            else
            {
                if (n > 0)
                    this.Write(', ');
                this.Visit(e);
                n++;
            }
        }

        protected override Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                this.Write('CASE WHEN (');
                this.Visit(expr);
                this.Write(') THEN 1 ELSE 0 END');
                return expr;
            }
            return base.VisitValue(expr);
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (this.IsPredicate(c.Test))
            {
                this.Write('(CASE WHEN ');
                this.VisitPredicate(c.Test);
                this.Write(' THEN ');
                this.VisitValue(c.IfTrue);
                Expression ifFalse = c.IfFalse;
                while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                {
                    ConditionalExpression fc = (ConditionalExpression)ifFalse;
                    this.Write(' WHEN ');
                    this.VisitPredicate(fc.Test);
                    this.Write(' THEN ');
                    this.VisitValue(fc.IfTrue);
                    ifFalse = fc.IfFalse;
                }
                if (ifFalse != null)
                {
                    this.Write(' ELSE ');
                    this.VisitValue(ifFalse);
                }
                this.Write(' END)');
            }
            else
            {
                this.Write('(CASE ');
                this.VisitValue(c.Test);
                this.Write(' WHEN 0 THEN ');
                this.VisitValue(c.IfFalse);
                this.Write(' ELSE ');
                this.VisitValue(c.IfTrue);
                this.Write(' END)');
            }
            return c;
        }
    }
}

"}, {"SqlServerFormatter", @"using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using DataDrain.Factories;
using DataDrain.ORM.Data.Common.Language;

namespace DataDrain.ORM.DAL.SQLServer.ExpressionVisitor
{
    /// <summary>
    /// Formats a query expression into TSQL language syntax
    /// </summary>
    internal class SqlServerFormatter : SqlFormatter, ISqlFormatter
    {
        public List<DbParameter> Parametros { get; set; }

        public SqlServerFormatter()
            : base(new SqlLanguage())
        {
        }

        public string Format(Expression expression, QueryLanguage language)
        {
            var formatter = new SqlServerFormatter();
            formatter.Visit(expression);
            Parametros = formatter.RetornaParametros();
            return formatter.ToString();
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case 'Length':
                        this.Write('CHAR_LENGTH(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                switch (m.Member.Name)
                {
                    case 'Day':
                        this.Write('DAY(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Month':
                        this.Write('MONTH(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Year':
                        this.Write('YEAR(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Hour':
                        this.Write('HOUR(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Minute':
                        this.Write('MINUTE(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Second':
                        this.Write('SECOND(');
                        this.Visit(m.Expression);
                        this.Write(')');
                        return m;
                    case 'Millisecond':
                        this.Write('(MICROSECOND(');
                        this.Visit(m.Expression);
                        this.Write(')/1000)');
                        return m;
                    case 'DayOfWeek':
                        this.Write('(DAYOFWEEK(');
                        this.Visit(m.Expression);
                        this.Write(') - 1)');
                        return m;
                    case 'DayOfYear':
                        this.Write('(DAYOFYEAR(');
                        this.Visit(m.Expression);
                        this.Write(') - 1)');
                        return m;
                }
            }
            return base.VisitMemberAccess(m);
        }

        protected Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case 'StartsWith':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE CONCAT(');
                        this.Visit(m.Arguments[0]);
                        this.Write(','%'))');
                        return m;
                    case 'EndsWith':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE CONCAT('%',');
                        this.Visit(m.Arguments[0]);
                        this.Write('))');
                        return m;
                    case 'Contains':
                        this.Write('(');
                        this.Visit(m.Object);
                        this.Write(' LIKE CONCAT('%',');
                        this.Visit(m.Arguments[0]);
                        this.Write(','%'))');
                        return m;
                    case 'Concat':
                        IList<Expression> args = m.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        this.Write('CONCAT(');
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) this.Write(', ');
                            this.Visit(args[i]);
                        }
                        this.Write(')');
                        return m;
                    case 'IsNullOrEmpty':
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(' IS NULL OR ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' = '')');
                        return m;
                    case 'ToUpper':
                        this.Write('UPPER(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                    case 'ToLower':
                        this.Write('LOWER(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                    case 'Replace':
                        this.Write('REPLACE(');
                        this.Visit(m.Object);
                        this.Write(', ');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Substring':
                        this.Write('SUBSTRING(');
                        this.Visit(m.Object);
                        this.Write(', ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' + 1');
                        if (m.Arguments.Count == 2)
                        {
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                        }
                        this.Write(')');
                        return m;
                    case 'Remove':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('LEFT(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                        }
                        else
                        {
                            this.Write('CONCAT(');
                            this.Write('LEFT(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write('), SUBSTRING(');
                            this.Visit(m.Object);
                            this.Write(', ');
                            this.Visit(m.Arguments[0]);
                            this.Write(' + ');
                            this.Visit(m.Arguments[1]);
                            this.Write('))');
                        }
                        return m;
                    case 'IndexOf':
                        this.Write('(LOCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Object);
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(' + 1');
                        }
                        this.Write(') - 1)');
                        return m;
                    case 'Trim':
                        this.Write('TRIM(');
                        this.Visit(m.Object);
                        this.Write(')');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case 'op_Subtract':
                        if (m.Arguments[1].Type == typeof(DateTime))
                        {
                            this.Write('DATEDIFF(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'AddYears':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' YEAR)');
                        return m;
                    case 'AddMonths':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' MONTH)');
                        return m;
                    case 'AddDays':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' DAY)');
                        return m;
                    case 'AddHours':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' HOUR)');
                        return m;
                    case 'AddMinutes':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' MINUTE)');
                        return m;
                    case 'AddSeconds':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL ');
                        this.Visit(m.Arguments[0]);
                        this.Write(' SECOND)');
                        return m;
                    case 'AddMilliseconds':
                        this.Write('DATE_ADD(');
                        this.Visit(m.Object);
                        this.Write(', INTERVAL (');
                        this.Visit(m.Arguments[0]);
                        this.Write('* 1000) MICROSECOND)');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Decimal))
            {
                switch (m.Method.Name)
                {
                    case 'Add':
                    case 'Subtract':
                    case 'Multiply':
                    case 'Divide':
                    case 'Remainder':
                        this.Write('(');
                        this.VisitValue(m.Arguments[0]);
                        this.Write(' ');
                        this.Write(GetOperator(m.Method.Name));
                        this.Write(' ');
                        this.VisitValue(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Negate':
                        this.Write('-');
                        this.Visit(m.Arguments[0]);
                        this.Write('');
                        return m;
                    case 'Ceiling':
                    case 'Floor':
                        this.Write(m.Method.Name.ToUpper());
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Round':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'Truncate':
                        this.Write('TRUNCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(',0)');
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                switch (m.Method.Name)
                {
                    case 'Abs':
                    case 'Acos':
                    case 'Asin':
                    case 'Atan':
                    case 'Atan2':
                    case 'Cos':
                    case 'Exp':
                    case 'Log10':
                    case 'Sin':
                    case 'Tan':
                    case 'Sqrt':
                    case 'Sign':
                    case 'Ceiling':
                    case 'Floor':
                        this.Write(m.Method.Name.ToUpper());
                        this.Write('(');
                        this.Visit(m.Arguments[0]);
                        this.Write(')');
                        return m;
                    case 'Log':
                        if (m.Arguments.Count == 1)
                        {
                            goto case 'Log10';
                        }
                        break;
                    case 'Pow':
                        this.Write('POWER(');
                        this.Visit(m.Arguments[0]);
                        this.Write(', ');
                        this.Visit(m.Arguments[1]);
                        this.Write(')');
                        return m;
                    case 'Round':
                        if (m.Arguments.Count == 1)
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(')');
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write('ROUND(');
                            this.Visit(m.Arguments[0]);
                            this.Write(', ');
                            this.Visit(m.Arguments[1]);
                            this.Write(')');
                            return m;
                        }
                        break;
                    case 'Truncate':
                        this.Write('TRUNCATE(');
                        this.Visit(m.Arguments[0]);
                        this.Write(',0)');
                        return m;
                }
            }
            if (m.Method.Name == 'ToString')
            {
                if (m.Object.Type != typeof(string))
                {
                    this.Write('CAST(');
                    this.Visit(m.Object);
                    this.Write(' AS CHAR)');
                }
                else
                {
                    this.Visit(m.Object);
                }
                return m;
            }
            else if (!m.Method.IsStatic && m.Method.Name == 'CompareTo' && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 1)
            {
                this.Write('(CASE WHEN ');
                this.Visit(m.Object);
                this.Write(' = ');
                this.Visit(m.Arguments[0]);
                this.Write(' THEN 0 WHEN ');
                this.Visit(m.Object);
                this.Write(' < ');
                this.Visit(m.Arguments[0]);
                this.Write(' THEN -1 ELSE 1 END)');
                return m;
            }
            else if (m.Method.IsStatic && m.Method.Name == 'Compare' && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 2)
            {
                this.Write('(CASE WHEN ');
                this.Visit(m.Arguments[0]);
                this.Write(' = ');
                this.Visit(m.Arguments[1]);
                this.Write(' THEN 0 WHEN ');
                this.Visit(m.Arguments[0]);
                this.Write(' < ');
                this.Visit(m.Arguments[1]);
                this.Write(' THEN -1 ELSE 1 END)');
                return m;
            }
            return base.VisitMethodCall(m);
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Constructor.DeclaringType == typeof(DateTime))
            {
                if (nex.Arguments.Count == 3)
                {
                    this.Write('CAST(CONCAT(');
                    this.Visit(nex.Arguments[0]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[1]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[2]);
                    this.Write(') AS DATETIME)');
                    return nex;
                }
                else if (nex.Arguments.Count == 6)
                {
                    this.Write('CAST(CONCAT(');
                    this.Visit(nex.Arguments[0]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[1]);
                    this.Write(','-',');
                    this.Visit(nex.Arguments[2]);
                    this.Write(',' ',');
                    this.Visit(nex.Arguments[3]);
                    this.Write(',':',');
                    this.Visit(nex.Arguments[4]);
                    this.Write(',':',');
                    this.Visit(nex.Arguments[5]);
                    this.Write(') AS DATETIME)');
                    return nex;
                }
            }
            return base.VisitNew(nex);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.Power)
            {
                this.Write('POWER(');
                this.VisitValue(b.Left);
                this.Write(', ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Coalesce)
            {
                this.Write('COALESCE(');
                this.VisitValue(b.Left);
                this.Write(', ');
                Expression right = b.Right;
                while (right.NodeType == ExpressionType.Coalesce)
                {
                    BinaryExpression rb = (BinaryExpression)right;
                    this.VisitValue(rb.Left);
                    this.Write(', ');
                    right = rb.Right;
                }
                this.VisitValue(right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.LeftShift)
            {
                this.Write('(');
                this.VisitValue(b.Left);
                this.Write(' << ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.RightShift)
            {
                this.Write('(');
                this.VisitValue(b.Left);
                this.Write(' >> ');
                this.VisitValue(b.Right);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Add && b.Type == typeof(string))
            {
                this.Write('CONCAT(');
                int n = 0;
                this.VisitConcatArg(b.Left, ref n);
                this.VisitConcatArg(b.Right, ref n);
                this.Write(')');
                return b;
            }
            else if (b.NodeType == ExpressionType.Divide && this.IsInteger(b.Type))
            {
                this.Write('TRUNCATE(');
                base.VisitBinary(b);
                this.Write(',0)');
                return b;
            }
            return base.VisitBinary(b);
        }

        private void VisitConcatArg(Expression e, ref int n)
        {
            if (e.NodeType == ExpressionType.Add && e.Type == typeof(string))
            {
                BinaryExpression b = (BinaryExpression)e;
                VisitConcatArg(b.Left, ref n);
                VisitConcatArg(b.Right, ref n);
            }
            else
            {
                if (n > 0)
                    this.Write(', ');
                this.Visit(e);
                n++;
            }
        }

        protected override Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                this.Write('CASE WHEN (');
                this.Visit(expr);
                this.Write(') THEN 1 ELSE 0 END');
                return expr;
            }
            return base.VisitValue(expr);
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (this.IsPredicate(c.Test))
            {
                this.Write('(CASE WHEN ');
                this.VisitPredicate(c.Test);
                this.Write(' THEN ');
                this.VisitValue(c.IfTrue);
                Expression ifFalse = c.IfFalse;
                while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                {
                    ConditionalExpression fc = (ConditionalExpression)ifFalse;
                    this.Write(' WHEN ');
                    this.VisitPredicate(fc.Test);
                    this.Write(' THEN ');
                    this.VisitValue(fc.IfTrue);
                    ifFalse = fc.IfFalse;
                }
                if (ifFalse != null)
                {
                    this.Write(' ELSE ');
                    this.VisitValue(ifFalse);
                }
                this.Write(' END)');
            }
            else
            {
                this.Write('(CASE ');
                this.VisitValue(c.Test);
                this.Write(' WHEN 0 THEN ');
                this.VisitValue(c.IfFalse);
                this.Write(' ELSE ');
                this.VisitValue(c.IfTrue);
                this.Write(' END)');
            }
            return c;
        }
    }
}

"}, {"StrongNamePermission", @"using System;
using System.Linq;
using System.Reflection;

namespace {namespace}Interfaces
{
    /// <summary>
    /// Classe responsavel pela permissão de uso do sistema
    /// </summary>
    public class StrongNamePermission
    {
        /// <summary>
        /// Valida se o Assembly que esta usando a classe é assinado como o mesmo StrongName
        /// </summary>
        /// <param name='callerAssembly'>Assembly que esta chamando o projeto</param>
        /// <param name='currentAssembly'>Assembly do projeto</param>
        protected StrongNamePermission(Assembly callerAssembly, Assembly currentAssembly)
        {
            if (!currentAssembly.GetName().GetPublicKey().SequenceEqual(callerAssembly.GetName().GetPublicKey()))
            {
                throw new Exception(string.Format('O Assembly '{0}' não possui permissão de uso.', callerAssembly.GetName().FullName));
            }
        }
    }
}
"}, {"Transaction", @"using System.Collections.Generic;
using System.Linq;
using Apoio.CommandMap;
using Apoio.Enumeradores;
using usingIDb;

namespace Apoio.Mapping
{
    internal static class Transaction
    {
        #region .: Métodos :.

        /// <summary>
        /// Executa lista de comando sem retorno gerando um comando pré compilado
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='clsGeneric'>lista generica</param>
        /// <param name='cnn'>conexão com o BD</param>
        /// <param name='tipoComando'>Tipo de consulta</param>
        public static void ExecutaListaComandosSemRetorno<T>(List<T> clsGeneric, {IDbConnection} cnn, ETipoConsulta tipoComando)
        {
            {IDbTransaction} trans = null;

            try
            {
                if (clsGeneric.Count > 0)
                {

                    var cmdMaster = CmdMap.CreateDbCommand(clsGeneric.FirstOrDefault(), tipoComando);

                    cmdMaster.Transaction = cnn.BeginTransaction();

                    trans = cmdMaster.Transaction;

                    cmdMaster.ExecuteNonQuery();

                    foreach (var cmd in clsGeneric.Skip(1).Select(generic => CmdMap.CarregaValoresDbCommand(generic, tipoComando, cmdMaster)))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }
            catch
            {
                if (trans != null) trans.Rollback();
                throw;
            }
        }

        #endregion
    }
}
"}, {"Validar", @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TesteBLL.Validacao
{
    /// <summary>
    /// Classe responsavel pela validação dos DataAnnotations
    /// </summary>
    public class Validar
    {

        /// <summary>
        /// Verifica se o objeto é valido para ser enviado ao banco de dados
        /// </summary>
        /// <typeparam name='T'></typeparam>
        /// <param name='generic'>Classe a ser validada</param>
        /// <param name='formatar'>indica se deve exibir a quebra de linha após cada erro em html</param>
        /// <returns>boleano de confirmação</returns>
        public static bool Validate<T>(T generic, bool formatar)
        {
            var retorno = from prop in generic.GetType().GetProperties() let err = ValidateProperty(generic, prop.Name) where !String.IsNullOrEmpty(err) select string.Format('{0}: {1}{2}', prop.Name, err, ((formatar) ? '<br>' : ''));

            var erros = retorno as string[] ?? retorno.ToArray();
            if (!erros.Any())
            {
                return true;
            }

            var errMessage = new System.Text.StringBuilder();
            foreach (var erro in erros)
            {
                errMessage.AppendLine(erro);
            }
            throw new ValidationException(errMessage.ToString());
        }

        private static string ValidateProperty<T>(T generic, string propertyName)
        {
            var info = generic.GetType().GetProperty(propertyName);
            if (!info.CanWrite)
            {
                return null;
            }

            var value = info.GetValue(generic, null);
            IEnumerable<string> errorInfos = (from va in info.GetCustomAttributes(true).OfType<ValidationAttribute>() where !va.IsValid(value) select va.FormatErrorMessage(string.Empty)).ToList();

            return errorInfos.Any() ? errorInfos.FirstOrDefault() : null;
        }
    }
}
"}, {"interfaceCRUD", @"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace {namespace}Interfaces
{
    public interface IPersistence<T>
    {
        List<T> SelectAll();
        int QuantidadeRegistros();
        int QuantidadeRegistros(Expression<Func<T, bool>> predicate);

        T SelectFirstOrDefault(Expression<Func<T, bool>> predicate);
        List<T> Select(Expression<Func<T, bool>> predicate);
        List<T> Select(Expression<Func<T, bool>> predicate, IDbConnection cnn, IDbTransaction trans);

        T Insert(T objGeneric);
        void Insert(T objGeneric, IDbConnection cnn, IDbTransaction trans);
        void Insert(List<T> clsGeneric);

        T Update(T objGeneric);
        void Update(T objGeneric, IDbConnection cnn, IDbTransaction trans);
        void Update(List<T> clsGeneric);

        void Delete(T objGeneric);
        void Delete(T objGeneric, IDbConnection cnn, IDbTransaction trans);
        void Delete(List<T> clsGeneric);
    }
}"}, {"LADetalpmet", @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion='4.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition='Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')' />
  <PropertyGroup>
    <Configuration Condition=' '$(Configuration)' == '' '>Debug</Configuration>
    <Platform Condition=' '$(Platform)' == '' '>AnyCPU</Platform>
    <ProjectGuid>{[guid]}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>[namespace]DAL</RootNamespace>
    <AssemblyName>[namespace]DAL</AssemblyName>
    <TargetFrameworkVersion>[versao]</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' '>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Release|AnyCPU' '>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='System' />
    <Reference Include='System.Configuration' />
    <Reference Include='System.Core' />
    <Reference Include='System.Data.Linq' />
    <Reference Include='System.Runtime.Caching' />
    <Reference Include='System.Xml.Linq' />
    <Reference Include='System.Data.DataSetExtensions' />
    <Reference Include='System.Data' />
    <Reference Include='System.Xml' />
  </ItemGroup>
  <ItemGroup>
    [log]
    [arquivos]
    <Compile Include='ORM\Caching\CachingMannager.cs' />
    <Compile Include='ORM\Caching\Enuns\ECacheAcao.cs' />
    <Compile Include='ORM\Caching\Events\CacheChangedEventArgs.cs' />
    <Compile Include='ORM\Caching\Events\CacheChangedEventHandler.cs' />
    <Compile Include='ORM\Caching\Interfaces\ICachingProvider.cs' />
    <Compile Include='ORM\FastInvoke.cs' />
    <Compile Include='ORM\MapExtension.cs' />
    <Compile Include='Apoio\CommandMap\Transaction.cs' />
    <Compile Include='DataDrain\FuncoesCrud.cs' />
    <Compile Include='Apoio\CommandMap\CmdMap.cs' />
    <Compile Include='Apoio\Conexao\Singleton.cs' />
    <Compile Include='Apoio\Enumeradores\ETipoConsulta.cs' />
    <Compile Include='Apoio\Base\CrudBase.cs' />
    <Compile Include='DataDrain\Factories\MySqlFormatter.cs' />
    <Compile Include='DataDrain\SqlLanguage.cs' />
    <Compile Include='DataDrain\TSqlORM\DbExpressions.cs' />
    <Compile Include='DataDrain\TSqlORM\DbExpressionVisitor.cs' />
    <Compile Include='DataDrain\TSqlORM\ExpressionVisitor.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryExecutor.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryLanguage.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryMapping.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryTypeSystem.cs' />
    <Compile Include='DataDrain\TSqlORM\SqlFormatter.cs' />
    <Compile Include='DataDrain\Factories\ISqlFormatter.cs' />
    <Compile Include='Properties\AssemblyInfo.cs' />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include='..\[namespace]Interfaces\[namespace]Interfaces.csproj'>
      <Project>{[guidInterface]}</Project>
      <Name>[namespace]Interfaces</Name>
    </ProjectReference>
    <ProjectReference Include='..\[namespace]TO\[namespace]TO.csproj'>
      <Project>{[guidTO]}</Project>
      <Name>[namespace]TO</Name>
    </ProjectReference>
  </ItemGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name='BeforeBuild'>
  </Target>
  <Target Name='AfterBuild'>
  </Target>
  -->
</Project>"}, {"LLBetalpmet", @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion='4.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition='Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')' />
  <PropertyGroup>
    <Configuration Condition=' '$(Configuration)' == '' '>Debug</Configuration>
    <Platform Condition=' '$(Platform)' == '' '>AnyCPU</Platform>
    <ProjectGuid>{[guid]}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>[namespace]BLL</RootNamespace>
    <AssemblyName>[namespace]BLL</AssemblyName>
    <TargetFrameworkVersion>[versao]</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' '>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Release|AnyCPU' '>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='System' />
    <Reference Include='System.ComponentModel.DataAnnotations' />
    <Reference Include='System.Core' />
    <Reference Include='System.Xml.Linq' />
    <Reference Include='System.Data.DataSetExtensions' />
    <Reference Include='System.Data' />
    <Reference Include='System.Xml' />
  </ItemGroup>
  <ItemGroup>
    [log]
    [arquivos]
    <Compile Include='Base\CrudBaseBLL.cs' />    
    <Compile Include='Properties\AssemblyInfo.cs' />
    <Compile Include='Validacao\Validar.cs' />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include='..\[namespace]DAL\[namespace]DAL.csproj'>
      <Project>{[guidDAL]}</Project>
      <Name>[namespace]DAL</Name>
    </ProjectReference>
    <ProjectReference Include='..\[namespace]Interfaces\[namespace]Interfaces.csproj'>
      <Project>{[guidInterface]}</Project>
      <Name>[namespace]Interfaces</Name>
    </ProjectReference>
    <ProjectReference Include='..\[namespace]TO\[namespace]TO.csproj'>
      <Project>{[guidTO]}</Project>
      <Name>[namespace]TO</Name>
    </ProjectReference>
  </ItemGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name='BeforeBuild'>
  </Target>
  <Target Name='AfterBuild'>
  </Target>
  -->
</Project>"}, {"noituloS", @"Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010
Project('{[guid]}') = '[namespace]TO', '[namespace]TO\[namespace]TO.csproj', '{[guidTO]}'
EndProject
Project('{[guid]}') = '[namespace]DAL', '[namespace]DAL\[namespace]DAL.csproj', '{[guidDAL]}'
EndProject
Project('{[guid]}') = '[namespace]Interfaces', '[namespace]Interfaces\[namespace]Interfaces.csproj', '{[guidInterface]}'
EndProject
Project('{[guid]}') = '[namespace]BLL', '[namespace]BLL\[namespace]BLL.csproj', '{[guidBLL]}'
EndProject
[guidWcf]
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{[guidTO]}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{[guidTO]}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{[guidTO]}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{[guidTO]}.Release|Any CPU.Build.0 = Release|Any CPU
		{[guidDAL]}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{[guidDAL]}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{[guidDAL]}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{[guidDAL]}.Release|Any CPU.Build.0 = Release|Any CPU
		{[guidInterface]}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{[guidInterface]}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{[guidInterface]}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{[guidInterface]}.Release|Any CPU.Build.0 = Release|Any CPU
		{[guidBLL]}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{[guidBLL]}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{[guidBLL]}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{[guidBLL]}.Release|Any CPU.Build.0 = Release|Any CPU
        [guidGlobalWcf]
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal"}, {"ofnIylbmessA", @"using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle('[namespace][camada]')]
[assembly: AssemblyDescription('Camada de acesso a dados')]
[assembly: AssemblyConfiguration('')]
[assembly: AssemblyCompany('Data Drain ORM')]
[assembly: AssemblyProduct('[namespace][camada]')]
[assembly: AssemblyCopyright('Copyright ©  2014')]
[assembly: AssemblyTrademark('')]
[assembly: AssemblyCulture('')]
{sing}
[log]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid('[guid]')]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion('1.0.*')]
[assembly: AssemblyVersion('1.0.0.0')]
[assembly: AssemblyFileVersion('1.0.0.0')]"}, {"OTetalpmet", @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion='4.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition='Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')' />
  <PropertyGroup>
    <Configuration Condition=' '$(Configuration)' == '' '>Debug</Configuration>
    <Platform Condition=' '$(Platform)' == '' '>AnyCPU</Platform>
    <ProjectGuid>{[guid]}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>[namespace]TO</RootNamespace>
    <AssemblyName>[namespace]TO</AssemblyName>
    <TargetFrameworkVersion>[versao]</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <TargetFrameworkProfile />
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' '>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Release|AnyCPU' '>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='System' />
    <Reference Include='System.ComponentModel.DataAnnotations' />
    <Reference Include='System.Core' />
    <Reference Include='System.Data.Linq' />
    <Reference Include='System.Xml' />
    <Reference Include='System.Runtime.Serialization' />    
  </ItemGroup>
  <ItemGroup>
    <Compile Include='AttributeValidators\DomainValidator.cs' />
    <Compile Include='Properties\AssemblyInfo.cs' />
	  [arquivos]
  </ItemGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name='BeforeBuild'>
  </Target>
  <Target Name='AfterBuild'>
  </Target>
  -->
</Project>"}, {"padraoBLLNativo", @"using {namespace}BLL.Base;
using {namespace}TO;


namespace {namespace}BLL
{
    public class {classe}BLL : CrudBaseBLL<{classe}TO>
    {
        
    }
}"}, {"padraoBLLProc", @"using System;
using System.Collections.Generic;
using {namespace}DAL;
using {namespace}TO;

namespace {namespace}BLL
{
    public class {classe}BLL
    {
        private readonly {classe}DAL _repository;

        public {classe}BLL()
        {
            _repository = new {classe}DAL();
        }

        public List<{classe}TO> Select({parametros})
        {
            try
            {
                return _repository.Select({parametros2});
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}"}, {"padraoDALNativo", @"using {namespace}DAL.Apoio.Base;
using {namespace}TO;

namespace {namespace}DAL
{
    /// <summary>
    /// Responsavel pelo CRUD basico das entidades
    /// </summary>
    public sealed class {classe}DAL : CrudBase<{classe}TO>
    {
    }
}"}, {"padraoDALProc", @"using System;
using System.Collections.Generic;
using System.Data;
using Apoio.Conexao;
using {namespace}TO;
using DataDrain.Mapping;

namespace {namespace}DAL
{
    public sealed class {classe}DAL
    {
        public List<{classe}TO> Select({parametros})
        {
            using (var cnn = Singleton.RetornaConexao())
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = @'{query}';
                {proc}
                {carregaParametros}

                cnn.Open();

                using (var drGeneric = cmd.ExecuteReader())
                {
                    return drGeneric.MapToEntities<{classe}TO>();
                }
            }
        }
    }
}"}, {"secafretnIetalpmeT", @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion='4.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition='Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')' />
  <PropertyGroup>
    <Configuration Condition=' '$(Configuration)' == '' '>Debug</Configuration>
    <Platform Condition=' '$(Platform)' == '' '>AnyCPU</Platform>
    <ProjectGuid>[guid]</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>[namespace]Interfaces</RootNamespace>
    <AssemblyName>[namespace]Interfaces</AssemblyName>
    <TargetFrameworkVersion>[versao]</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <TargetFrameworkProfile />
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' '>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Release|AnyCPU' '>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='System.Core' />
	<Reference Include='System.Data' />
  </ItemGroup>
  <ItemGroup>
	<Compile Include='StrongNamePermission.cs' />
    <Compile Include='IFactory.cs' />
    <Compile Include='IPersistence.cs' />
    <Compile Include='IUnityOfWork.cs' />
    <Compile Include='Properties\AssemblyInfo.cs' />
  </ItemGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name='BeforeBuild'>
  </Target>
  <Target Name='AfterBuild'>
  </Target>
  -->
</Project>"}, {"CrudBaseBLL", @"using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using {namespace}BLL.Validacao;
using {namespace}DAL.Apoio.Base;
using {namespace}Interfaces;


namespace {namespace}BLL.Base
{
    public abstract class CrudBaseBLL<T> : IPersistence<T> where T : class, new()
    {
        private readonly IPersistence<T> _persistence;

        protected CrudBaseBLL()
        {
            _persistence = new CrudBase<T>();
        }

        /// <summary>
        /// Verifica se o objeto Cliente esta valido
        /// </summary>
        /// <param name='generic'>Objeto a ser validado</param>
        /// <param name='msgErro'>Retonar mensagem de erro se o mesmo existir</param>
        /// <returns></returns>
        public bool IsValid(T generic, out string msgErro)
        {
            try
            {
                msgErro = string.Empty;
                return Validar.Validate(generic, false);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                msgErro = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Valida se a lista de objetos Cliente esta valida
        /// </summary>
        /// <param name='lista'>Lista a ser validada</param>
        /// <param name='msgErro'>Retonar mensagem de erro se o mesmo existir</param>
        /// <returns></returns>
        public bool IsValid(List<T> lista, out string msgErro)
        {
            var posicao = 0;

            try
            {
                foreach (var item in lista)
                {
                    posicao++;
                    Validar.Validate(item, false);
                }
                msgErro = string.Empty;
                return true;
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                msgErro = string.Format('Erro no {0}º item da lista.\nErro:\n{1}', posicao, ex.Message);
                return false;
            }
        }

        public List<T> SelectAll()
        {
            return _persistence.SelectAll();
        }

        public int QuantidadeRegistros()
        {
            return _persistence.QuantidadeRegistros();
        }

        public int QuantidadeRegistros(Expression<Func<T, bool>> predicate)
        {
            return _persistence.QuantidadeRegistros(predicate);
        }

        public T SelectFirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _persistence.SelectFirstOrDefault(predicate);
        }

        public List<T> Select(Expression<Func<T, bool>> predicate)
        {
            return _persistence.Select(predicate);
        }

        public T Insert(T objGeneric)
        {
            return _persistence.Insert(objGeneric);
        }

        public void Insert(List<T> clsGeneric)
        {
            _persistence.Insert(clsGeneric);
        }

        public T Update(T objGeneric)
        {
            return _persistence.Update(objGeneric);
        }

        public void Update(List<T> clsGeneric)
        {
            _persistence.Update(clsGeneric);
        }

        public void Delete(T objGeneric)
        {
            _persistence.Delete(objGeneric);
        }

        public void Delete(List<T> clsGeneric)
        {
            _persistence.Delete(clsGeneric);
        }
    }
}
"}, {"interfaceView", @"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace {namespace}Interfaces
{
    public interface IPersistenceView<T>
    {
        List<T> SelectAll();
        int QuantidadeRegistros();
        int QuantidadeRegistros(Expression<Func<T, bool>> predicate);

        T SelectFirstOrDefault(Expression<Func<T, bool>> predicate);
        List<T> Select(Expression<Func<T, bool>> predicate);
        List<T> Select(Expression<Func<T, bool>> predicate, IDbConnection cnn, IDbTransaction trans);
    }
}"}, {"IFactory", @"
namespace {namespace}Interfaces
{
    public interface IFactory<T> : IPersistence<T>, IUnityOfWork<T>
    {

    }
}"}, {"IPersistence", @"using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace {namespace}Interfaces
{
    public interface IPersistence<T>
    {
        List<T> SelectAll();
        int QuantidadeRegistros();
        int QuantidadeRegistros(Expression<Func<T, bool>> predicate);

        T SelectFirstOrDefault(Expression<Func<T, bool>> predicate);
        List<T> Select(Expression<Func<T, bool>> predicate);

        T Insert(T objGeneric);
        void Insert(List<T> clsGeneric);

        T Update(T objGeneric);
        void Update(List<T> clsGeneric);

        void Delete(T objGeneric);
        void Delete(List<T> clsGeneric);
    }
}"}, {"IUnityOfWork", @"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace {namespace}Interfaces
{
    public interface IUnityOfWork<T>
    {
        List<T> Select(Expression<Func<T, bool>> predicate, IDbConnection cnn, IDbTransaction trans);

        void Insert(T objGeneric, IDbConnection cnn, IDbTransaction trans);

        void Update(T objGeneric, IDbConnection cnn, IDbTransaction trans);

        void Delete(T objGeneric, IDbConnection cnn, IDbTransaction trans);
    }
}"}, {"AssemblyInfoWcf", @"using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle('{namespace}Wcf')]
[assembly: AssemblyDescription('')]
[assembly: AssemblyConfiguration('')]
[assembly: AssemblyCompany('')]
[assembly: AssemblyProduct('{namespace}Wcf')]
[assembly: AssemblyCopyright('Copyright ©  2016')]
[assembly: AssemblyTrademark('')]
[assembly: AssemblyCulture('')]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid('{guid}')]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion('1.0.*')]
[assembly: AssemblyVersion('1.0.0.0')]
[assembly: AssemblyFileVersion('1.0.0.0')]
"}, {"corpoServico", @"using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using {namespace}BLL;
using {namespace}TO;
using {namespace}Wcf.Interface;

namespace {namespace}Wcf
{
    public class {classe} : I{classe}
    {
        private readonly {classe}BLL _aceite;

        public {classe}()
        {
            _aceite = new {classe}BLL();
        }

        public List<{classe}TO> SelectAll()
        {
            return _aceite.SelectAll();
        }

        public int QuantidadeRegistros()
        {
            return _aceite.QuantidadeRegistros();
        }

        public int QuantidadeRegistros(Expression<Func<{classe}TO, bool>> predicate)
        {
            return _aceite.QuantidadeRegistros(predicate);
        }

        public {classe}TO SelectFirstOrDefault(Expression<Func<{classe}TO, bool>> predicate)
        {
            return _aceite.SelectFirstOrDefault(predicate);
        }

        public List<{classe}TO> Select(Expression<Func<{classe}TO, bool>> predicate)
        {
            return _aceite.Select(predicate);
        }

        public {classe}TO Insert({classe}TO objGeneric)
        {
            return _aceite.Insert(objGeneric);
        }

        public void Insert(List<{classe}TO> clsGeneric)
        {
            _aceite.Insert(clsGeneric);
        }

        public {classe}TO Update({classe}TO objGeneric)
        {
            return _aceite.Update(objGeneric);
        }

        public void Update(List<{classe}TO> clsGeneric)
        {
            _aceite.Update(clsGeneric);
        }

        public void Delete({classe}TO objGeneric)
        {
            _aceite.Delete(objGeneric);
        }

        public void Delete(List<{classe}TO> clsGeneric)
        {
            _aceite.Delete(clsGeneric);
        }
    }
}
"}, {"corpoInterfaceServico", @"using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ServiceModel;
using {namespace}TO;

namespace {namespace}Wcf.Interface
{
    [ServiceContract]
    public interface I{classe}
    {
        [OperationContract]
        List<{classe}TO> SelectAll();

        [OperationContract]
        int QuantidadeRegistros();

        [OperationContract]
        int QuantidadeRegistros(Expression<Func<{classe}TO, bool>> predicate);

        [OperationContract]
        {classe}TO SelectFirstOrDefault(Expression<Func<{classe}TO, bool>> predicate);

        [OperationContract]
        List<{classe}TO> Select(Expression<Func<{classe}TO, bool>> predicate);

        [OperationContract]
        {classe}TO Insert({classe}TO objGeneric);

        [OperationContract]
        void Insert(List<{classe}TO> clsGeneric);

        [OperationContract]
        {classe}TO Update({classe}TO objGeneric);

        [OperationContract]
        void Update(List<{classe}TO> clsGeneric);

        [OperationContract]
        void Delete({classe}TO objGeneric);

        [OperationContract]
        void Delete(List<{classe}TO> clsGeneric);
    }
}
"}, {"corpoProjetoWcf", @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion='12.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition='Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')' />
  <PropertyGroup>
    <Configuration Condition=' '$(Configuration)' == '' '>Debug</Configuration>
    <Platform Condition=' '$(Platform)' == '' '>AnyCPU</Platform>
    <ProductVersion>
    </ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{[guid]}</ProjectGuid>
    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>{namespace}Wcf</RootNamespace>
    <AssemblyName>{namespace}Wcf</AssemblyName>
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <WcfConfigValidationEnabled>True</WcfConfigValidationEnabled>
    <UseIISExpress>true</UseIISExpress>
    <IISExpressSSLPort />
    <IISExpressAnonymousAuthentication />
    <IISExpressWindowsAuthentication />
    <IISExpressUseClassicPipelineMode />
    <TargetFrameworkProfile />
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' '>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <Prefer32Bit>false</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Release|AnyCPU' '>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <Prefer32Bit>false</Prefer32Bit>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='System' />
    <Reference Include='System.ServiceModel' />
    <Reference Include='System.ServiceModel.Web' />
  </ItemGroup>
  <ItemGroup>
    {servicos}
    <Content Include='Web.config' />
  </ItemGroup>
  <ItemGroup>
    {servicosCode}
    {servicosInterface}
    <Compile Include='Properties\AssemblyInfo.cs' />
  </ItemGroup>
  <ItemGroup />
  <ItemGroup>
    <ProjectReference Include='..\{namespace}BLL\{namespace}BLL.csproj'>
      <Project>{776bf1ac-b6b6-4d02-9e1c-bd232d626034}</Project>
      <Name>{namespace}BLL</Name>
    </ProjectReference>
    <ProjectReference Include='..\{namespace}Interfaces\{namespace}Interfaces.csproj'>
      <Project>{e76ebcc2-1fd9-4239-8959-98e44f017dea}</Project>
      <Name>{namespace}Interfaces</Name>
    </ProjectReference>
    <ProjectReference Include='..\{namespace}TO\{namespace}TO.csproj'>
      <Project>{ae07e894-0fc6-4d1b-b450-cc44b696a5f8}</Project>
      <Name>{namespace}TO</Name>
    </ProjectReference>
  </ItemGroup>
  <PropertyGroup>
    <VisualStudioVersion Condition=''$(VisualStudioVersion)' == '''>10.0</VisualStudioVersion>
    <VSToolsPath Condition=''$(VSToolsPath)' == '''>$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
  </PropertyGroup>
  <Import Project='$(MSBuildBinPath)\Microsoft.CSharp.targets' />
  <Import Project='$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets' Condition=''$(VSToolsPath)' != ''' />
  <Import Project='$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v10.0\WebApplications\Microsoft.WebApplication.targets' Condition='false' />
  <ProjectExtensions>
    <VisualStudio>
      <FlavorProperties GUID='{349c5851-65df-11da-9384-00065b846f21}'>
        <WebProjectProperties>
          <UseIIS>True</UseIIS>
          <AutoAssignPort>True</AutoAssignPort>
          <DevelopmentServerPort>59343</DevelopmentServerPort>
          <DevelopmentServerVPath>/</DevelopmentServerVPath>
          <IISUrl>http://localhost:59343/</IISUrl>
          <NTLMAuthentication>False</NTLMAuthentication>
          <UseCustomServer>False</UseCustomServer>
          <CustomServerUrl>
          </CustomServerUrl>
          <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
        </WebProjectProperties>
      </FlavorProperties>
    </VisualStudio>
  </ProjectExtensions>
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name='BeforeBuild'>
  </Target>
  <Target Name='AfterBuild'>
  </Target>
  -->
</Project>"}, {"corpoWebConfig", @"<?xml version='1.0'?>
<configuration>
  <!--
    For a description of web.config changes see http://go.microsoft.com/fwlink/?LinkId=235367.

    The following attributes can be set on the <httpRuntime> tag.
      <system.Web>
        <httpRuntime targetFramework='4.5' />
      </system.Web>
  -->
  <system.web>
    <compilation debug='true' targetFramework='4.5'/>
    <pages controlRenderingCompatibilityVersion='4.0'/>
  </system.web>
  <system.serviceModel>
    <behaviors>
      <serviceBehaviors>
        <behavior>
          <!-- To avoid disclosing metadata information, set the value below to false before deployment -->
          <serviceMetadata httpGetEnabled='true'/>
          <!-- To receive exception details in faults for debugging purposes, set the value below to true.  Set to false before deployment to avoid disclosing exception information -->
          <serviceDebug includeExceptionDetailInFaults='false'/>
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <serviceHostingEnvironment aspNetCompatibilityEnabled='true'
      multipleSiteBindingsEnabled='true' />
  </system.serviceModel>
  <system.webServer>
    <modules runAllManagedModulesForAllRequests='true'/>
    <!--
        To browse web app root directory during debugging, set the value below to true.
        Set to false before deployment to avoid disclosing web app folder information.
      -->
    <directoryBrowse enabled='true'/>
  </system.webServer>
</configuration>"}, {"SSLADetalpmet", @"<?xml version='1.0' encoding='utf-8'?>
<Project ToolsVersion='4.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition='Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')' />
  <PropertyGroup>
    <Configuration Condition=' '$(Configuration)' == '' '>Debug</Configuration>
    <Platform Condition=' '$(Platform)' == '' '>AnyCPU</Platform>
    <ProjectGuid>{[guid]}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>[namespace]DAL</RootNamespace>
    <AssemblyName>[namespace]DAL</AssemblyName>
    <TargetFrameworkVersion>[versao]</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' '>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=' '$(Configuration)|$(Platform)' == 'Release|AnyCPU' '>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='System' />
    <Reference Include='System.Configuration' />
    <Reference Include='System.Core' />
    <Reference Include='System.Data.Linq' />
    <Reference Include='System.Runtime.Caching' />
    <Reference Include='System.Xml.Linq' />
    <Reference Include='System.Data.DataSetExtensions' />
    <Reference Include='System.Data' />
    <Reference Include='System.Xml' />
  </ItemGroup>
  <ItemGroup>
    [log]
    [arquivos]
    <Compile Include='ORM\Caching\CachingMannager.cs' />
    <Compile Include='ORM\Caching\Enuns\ECacheAcao.cs' />
    <Compile Include='ORM\Caching\Events\CacheChangedEventArgs.cs' />
    <Compile Include='ORM\Caching\Events\CacheChangedEventHandler.cs' />
    <Compile Include='ORM\Caching\Interfaces\ICachingProvider.cs' />
    <Compile Include='ORM\FastInvoke.cs' />
    <Compile Include='ORM\MapExtension.cs' />
    <Compile Include='Apoio\CommandMap\Transaction.cs' />
    <Compile Include='DataDrain\FuncoesCrud.cs' />
    <Compile Include='Apoio\CommandMap\CmdMap.cs' />
    <Compile Include='Apoio\Conexao\Singleton.cs' />
    <Compile Include='Apoio\Enumeradores\ETipoConsulta.cs' />
    <Compile Include='Apoio\Base\CrudBase.cs' />
    <Compile Include='DataDrain\Factories\SqlServerFormatter.cs' />
    <Compile Include='DataDrain\SqlLanguage.cs' />
    <Compile Include='DataDrain\TSqlORM\DbExpressions.cs' />
    <Compile Include='DataDrain\TSqlORM\DbExpressionVisitor.cs' />
    <Compile Include='DataDrain\TSqlORM\ExpressionVisitor.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryExecutor.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryLanguage.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryMapping.cs' />
    <Compile Include='DataDrain\TSqlORM\QueryTypeSystem.cs' />
    <Compile Include='DataDrain\TSqlORM\SqlFormatter.cs' />
    <Compile Include='DataDrain\Factories\ISqlFormatter.cs' />
    <Compile Include='Properties\AssemblyInfo.cs' />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include='..\[namespace]Interfaces\[namespace]Interfaces.csproj'>
      <Project>{[guidInterface]}</Project>
      <Name>[namespace]Interfaces</Name>
    </ProjectReference>
    <ProjectReference Include='..\[namespace]TO\[namespace]TO.csproj'>
      <Project>{[guidTO]}</Project>
      <Name>[namespace]TO</Name>
    </ProjectReference>
  </ItemGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name='BeforeBuild'>
  </Target>
  <Target Name='AfterBuild'>
  </Target>
  -->
</Project>"}, {"SSFuncoesCrud", @"using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain;
using DataDrain.Factories;
using System.Data.SqlClient;
using DataDrain.ORM.DAL.SQLServer.ExpressionVisitor;

namespace {namespace}.DataDrain
{
    internal class FuncoesCrud
    {
        /// <summary>
        /// Lista de parametros do predicado
        /// </summary>
        public static List<SqlParameter> Parametros { get; set; }

        /// <summary>
        /// Retorna a string do where
        /// </summary>
        /// <param name='predicate'></param>
        /// <returns></returns>
        public static string RetornaStringWhere<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                ISqlFormatter visitor = new SqlServerFormatter();
                Parametros = new List<SqlParameter>();

                var sql = visitor.Format(predicate, new SqlLanguage());
                GeraParametros(visitor.Parametros);
                return string.Format(' WHERE {0};', sql.Replace('?Parameter?().', ''));
            }
            else
            {
                Parametros = new List<SqlParameter>();
                return '';
            }
        }

        private static void GeraParametros(IEnumerable<DbParameter> parametros)
        {

            foreach (var param in parametros)
            {
                Parametros.Add(new SqlParameter
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
                nomeParametro = string.Format('{0}{1}', param.ParameterName, i);
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
                    return string.Format(''{0}'', innerObj.GetType().GetFields()[0].GetValue(innerObj));
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
                        if (!m.ToString().Contains('ToString()'))
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
                                retorno = m.ToString().Replace('.ToString()', '');
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

                        if (m.ToString().Contains('Equals'))
                        {
                            retorno = string.Format(''{0}'', parametros[0].ToString().Replace('True', '1').Replace('False', '0'));
                        }
                        else if (m.ToString().Contains('Contains('))
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

                            retorno = '';
                        }
                        else
                        {

                            retorno = string.Format(''{0}'', m.Method.Invoke(m, parametros.ToArray()));
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
                                return string.Format(''{0}'', (valorComparado.Equals(valorComparadoCom) ? valorTrue.Value : valorFalse.Value));
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
"}};

        }

        public Dictionary<string, string> ListAllValues()
        {
            return _dictionary.ToDictionary(item => item.Key, item => item.Value.Replace("'", "\""));
        }

        public KeyValuePair<string, string> GelValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new KeyValuePair<string, string>(key, "");
            }

            if (_dictionary.ContainsKey(key))
            {
                return new KeyValuePair<string, string>(key, _dictionary[key]);
            }
            return new KeyValuePair<string, string>(key, "");
        }
    }
}
