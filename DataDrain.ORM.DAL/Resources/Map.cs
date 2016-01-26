using System;
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
        /// <typeparam name="T">Tipo do Objeto</typeparam>
        /// <param name="dataObject">Objeto a ser analisado</param>
        /// <param name="sqlType">Tipo consulta</param>
        /// <returns></returns>
        internal static IDbCommand CreateDbCommand<T>(T dataObject, ETipoConsulta sqlType)
        {
            if (ValidaTableAttribute(dataObject))
            {
                return AjustaCommando(dataObject, RetornaDadosMap<T>(typeof(T)), sqlType);
            }

            throw new NullReferenceException("Objeto não implementa o atributo 'TableAttribute'");
        }

        /// <summary>
        /// Retorna uma instancia do IDbCommand
        /// </summary>
        /// <param name="typeObject">Objeto a ser analisado</param>
        /// <param name="cWhere">clausula where</param>
        /// <param name="parametros">parametros de filtragem</param>
        /// <returns></returns>
        internal static IDbCommand CreateDbCommand<T>(Type typeObject, string cWhere, IEnumerable<IDbDataParameter> parametros)
        {
            if (ValidaTableAttribute(typeObject))
            {
                var tableName = NomeTabela(typeObject);
                var mapObj = RetornaDadosMap<T>(typeObject);

                var cmd = Singleton.RetornaCommando();
                cmd.CommandText = string.Format("SELECT {0} FROM {1} {2};", string.Join(",", mapObj.Select(p => p.Value.Storage).ToArray()), tableName, cWhere);

                foreach (var param in parametros)
                {
                    cmd.Parameters.Add(param);
                }

                return cmd;
            }

            throw new NullReferenceException("Objeto não implementa o atributo 'TableAttribute'");
        }

        /// <summary>
        /// Retorna uma instancia do IDbCommand
        /// </summary>
        /// <typeparam name="T">Tipo do Objeto</typeparam>
        /// <param name="dataObject">Objeto a ser analisado</param>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
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
                                case "System.String":
                                    {
                                        var valorConvertido = Convert.ChangeType("", p.Key.PropertyType);
                                        p.Key.SetValue(dadosObjeto.ObjetoAlvo, valorConvertido, null);
                                    }
                                    break;
                                case "System.DateTime":
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
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="genericObj">objeto que seta mapeado</param>
        /// <param name="colunas">Mapa ORM do objeto</param>
        /// <param name="tipoConsulta">Tipo de consulta que sera gerada</param>
        /// <returns></returns>
        private static IDbCommand AjustaCommando<T>(T genericObj, List<KeyValuePair<PropertyInfo, ColumnAttribute>> colunas, ETipoConsulta tipoConsulta)
        {
            var cmd = Singleton.RetornaCommando();

            foreach (var coluna in colunas)
            {
                coluna.Value.Storage = coluna.Value.Storage.Replace("[", "").Replace("]", "");
            }

            var sqlSelect = string.Empty;

            switch (tipoConsulta)
            {
                case ETipoConsulta.SelectAll:
                    cmd.CommandText = string.Format("SELECT {0} FROM {1};", string.Join(",", colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)));
                    break;

                case ETipoConsulta.Insert:

                    var camposInsert = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => !coluna.Value.IsDbGenerated && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnInsert)))
                    {
                        camposInsert.Add(coluna.Value.Storage);
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", NomeTabela(typeof(T)), string.Join(", ", camposInsert.ToArray()), string.Join(" ,@", camposInsert.ToArray()));

                    break;
                case ETipoConsulta.Update:

                    var camposUpdate = new List<string>();
                    var whereUpdate = new List<string>();

                    foreach (var coluna in colunas.OrderByDescending(c => c.Value.IsPrimaryKey))
                    {
                        if (!coluna.Value.IsPrimaryKey && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnUpdate))
                        {
                            camposUpdate.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }

                        if (coluna.Value.IsPrimaryKey)
                        {
                            whereUpdate.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} ;", NomeTabela(typeof(T)), string.Join(",", camposUpdate.ToArray()), string.Join(" AND ", whereUpdate.ToArray()));

                    break;
                case ETipoConsulta.Delete:

                    var whereDel = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereDel.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    cmd.CommandText = string.Format("DELETE FROM {0} WHERE {1} ;", NomeTabela(typeof(T)), string.Join(" AND ", whereDel.ToArray()));

                    break;
                case ETipoConsulta.InsertWithReturn:

                    var whereIr = new List<string>();
                    var camposIr = new List<string>();

                    foreach (var coluna in colunas.Where(coluna => !coluna.Value.IsDbGenerated && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnInsert)))
                    {
                        camposIr.Add(coluna.Value.Storage);
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    var sqlInsert = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", NomeTabela(typeof(T)), string.Join(", ", camposIr.ToArray()), string.Join(" ,@", camposIr.ToArray()));

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereIr.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage, cmd.GetType().Name, coluna.Value.IsDbGenerated);
                    }

                    sqlSelect = string.Format("SELECT {0} FROM {1} {2};", string.Join(",", colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)), (whereIr.Count > 0 ? string.Join(" AND ", whereIr.ToArray()) : " "));

                    cmd.CommandText = sqlInsert + sqlSelect;

                    break;
                case ETipoConsulta.UpdateWithReturn:

                    var whereUp = new List<string>();
                    var camposUp = new List<string>();

                    foreach (var coluna in colunas.OrderByDescending(c => c.Value.IsPrimaryKey))
                    {
                        if (!coluna.Value.IsPrimaryKey && (coluna.Value.AutoSync == AutoSync.Always || coluna.Value.AutoSync == AutoSync.OnUpdate))
                        {
                            camposUp.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }

                        if (coluna.Value.IsPrimaryKey)
                        {
                            camposUp.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
                    }

                    var sqlUpdate = string.Format("UPDATE {0} SET {1} WHERE {2} ;", NomeTabela(typeof(T)), string.Join(",", camposUp.ToArray()), string.Join(" AND ", camposUp.ToArray()));

                    foreach (var coluna in colunas.Where(coluna => coluna.Value.IsPrimaryKey))
                    {
                        whereUp.Add(string.Format("{0} = @{0}", coluna.Value.Storage));
                        ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                    }

                    sqlSelect = string.Format("SELECT {0} FROM {1} {2};", string.Join(",", colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)), (whereUp.Count > 0 ? string.Join(" AND ", whereUp.ToArray()) : " "));

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
            param.ParameterName = string.Format("@{0}", nomeCampo);

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
                        case "SqlCommad":
                            param.Value = identity ? "SCOPE_IDENTITY()" : (property.GetValue(genericObj, null) ?? DBNull.Value);
                            break;

                        case "MySqlCommand":
                            param.Value = identity ? "LAST_INSERT_ID()" : (property.GetValue(genericObj, null) ?? DBNull.Value);
                            break;

                        case "FbCommand":

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
        /// <typeparam name="T"></typeparam>
        /// <param name="dataObject"></param>
        /// <param name="sqlType"></param>
        /// <param name="commando"></param>
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
                        daoProperty.Storage = daoProperty.Storage.Replace("[", "").Replace("]", "");
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
                                throw new ArgumentOutOfRangeException("sqlType");
                        }
                    }
                }

                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Não foi possivel gerar o comando SQL de {0}", Enum.GetName(typeof(ETipoConsulta), sqlType)), ex);
            }
        }

        private static void ConfiguraValorParametro(object dataObject, PropertyInfo property, IDbCommand cmd, DataAttribute daoProperty)
        {
            var paramName = string.Format("@{0}", daoProperty.Storage);
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
                throw new CustomAttributeFormatException("Classe não se adequada aos atributos DataDrain.ORM");
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
