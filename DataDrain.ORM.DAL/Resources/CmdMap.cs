using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using Apoio.Enumeradores;
using DataDrain.Caching;
using MySql.Data.MySqlClient;

namespace Apoio.CommandMap
{
    internal static class CmdMap
    {
        private static readonly CachingMannager Cache = new CachingMannager(new TimeSpan(0, 0, 20, 0));

        internal static MySqlCommand CreateDbCommand<T>(T tipoObjeto, ETipoConsulta sqlType)
        {
            return AjustaCommando(tipoObjeto, RetornaDadosMap<T>(), sqlType);
        }

        internal static MySqlCommand CreateDbCommand<T>(Type typeObject, string cWhere, IEnumerable<MySqlParameter> parametros)
        {
            var mapObj = RetornaDadosMap<T>();
            var cmd = new MySqlCommand { CommandText = string.Format("SELECT {0} FROM {1} {2};", string.Join(",", mapObj.Select(p => p.Value.Storage).ToArray()), NomeTabela(typeObject), cWhere) };
            cmd.Parameters.AddRange(parametros.ToArray());

            return cmd;
        }

        private static List<KeyValuePair<PropertyInfo, ColumnAttribute>> RetornaDadosMap<T>()
        {
            var map = Cache.Recuperar<Dictionary<string, List<KeyValuePair<PropertyInfo, ColumnAttribute>>>>("mapCmd").Value ?? new Dictionary<string, List<KeyValuePair<PropertyInfo, ColumnAttribute>>>();

            if (map.ContainsKey(typeof(T).FullName))
            {
                return map[typeof(T).FullName];
            }

            var mapObj = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() != null
                ? typeof(T).GetProperties().Select(p => new KeyValuePair<PropertyInfo, ColumnAttribute>(p, p.GetCustomAttributes(true).Where(att => att is ColumnAttribute).Cast<ColumnAttribute>().FirstOrDefault())).ToList()
                : typeof(T).GetProperties().Select(p => new KeyValuePair<PropertyInfo, ColumnAttribute>(p, new ColumnAttribute { Storage = p.Name, AutoSync = AutoSync.Always })).ToList();

            map.Add(typeof(T).FullName, mapObj);
            Cache.Adicionar("mapCmd", map);

            return mapObj;
        }


        #region .: Metodos :.

        private static MySqlCommand AjustaCommando<T>(T genericObj, List<KeyValuePair<PropertyInfo, ColumnAttribute>> colunas, ETipoConsulta tipoConsulta)
        {
            var cmd = new MySqlCommand();

            foreach (var coluna in colunas)
            {
                coluna.Value.Storage = coluna.Value.Storage.Replace("[", "").Replace("]", "").Trim();
            }

            string sqlSelect;

            switch (tipoConsulta)
            {
                case ETipoConsulta.SelectAll:
                    cmd.CommandText = string.Format("SELECT {0} FROM {1};", string.Join(", ", colunas.Select(c => c.Value.Storage).ToArray()), NomeTabela(typeof(T)));
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
                        if (coluna.Value.IsDbGenerated)
                        {
                            whereIr.Add(string.Format("{0} = LAST_INSERT_ID() ", coluna.Value.Storage));
                        }
                        else
                        {
                            whereIr.Add(string.Format("{0} = @{0} ", coluna.Value.Storage));
                            ConfiguraParametro(genericObj, coluna.Key, cmd, coluna.Value.Storage);
                        }
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


        private static void ConfiguraParametro<T>(T genericObj, PropertyInfo property, MySqlCommand cmd, string nomeCampo)
        {
            if (cmd.Parameters.IndexOf(nomeCampo) < 0)
            {
                cmd.Parameters.AddWithValue(nomeCampo, property.GetValue(genericObj, null));
            }
        }

        internal static MySqlCommand CarregaValoresDbCommand<T>(T objetoAlvo, ETipoConsulta sqlType, MySqlCommand cmd)
        {
            var mapObj = RetornaDadosMap<T>();

            try
            {

                foreach (var daoProperty in mapObj)
                    {
                        daoProperty.Value.Storage = daoProperty.Value.Storage.Replace("[", "").Replace("]", "").Trim();
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
                                throw new ArgumentOutOfRangeException("sqlType");
                        }
                    }
                

                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Não foi possivel gerar o comando SQL de {0}", Enum.GetName(typeof(ETipoConsulta), sqlType)), ex);
            }
        }

        private static void ConfiguraValorParametro(object objetoAlvo, PropertyInfo property, MySqlCommand cmd, DataAttribute daoProperty)
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
