using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;
using MySql.Data.MySqlClient;

namespace DataDrain.Databases.MySql
{
    public sealed class MapProcedure : IInformationSchemaProcedure
    {
        private readonly ISupport _rorinas;

        public MapProcedure(ISupport rorinas)
        {
            _rorinas = rorinas;
        }

        public List<DatabaseObjectMap> ListAllStoredProcedures(string dataBaseName, DatabaseUser usr)
        {
            using (var cnn = Connection.DefaultConnection(usr))
            {
                var clsTables = new List<DatabaseObjectMap>();

                cnn.Open();
                var cmd = new MySqlCommand(string.Format("SELECT ROUTINE_NAME, CREATED, LAST_ALTERED FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '{0}' AND ROUTINE_TYPE='PROCEDURE' ORDER BY ROUTINE_NAME ASC;", dataBaseName), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new DatabaseObjectMap
                        {
                            Name = dr[0].ToString(),
                            CreationDate = Convert.ToDateTime(dr[1]),
                            ChangeDate = Convert.ToDateTime(dr[2]),
                            Type = EDatabaseObjectType.Procedure
                        });
                    }
                }
                return clsTables;
            }
        }

        /// <summary>
        /// Lista os parametros de entrada da Procedure selecionada
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <param name="procedureName">Nome da Stored Procedure alvo</param>
        /// <returns></returns>
        public List<StoredProcedureParameter> ListAllStoredProceduresParameters(string dataBaseName, DatabaseUser usr, string procedureName)
        {
            using (var cnn=Connection.DefaultConnection(usr))
            {
                var clsTables = new List<StoredProcedureParameter>();
    
                cnn.Open();

                var sbSql = new System.Text.StringBuilder();
                if (cnn.ServerVersion != null && cnn.ServerVersion.Contains("5."))
                {
                    sbSql.AppendLine("SELECT TRIM(CAST(param_list AS CHAR(10000) CHARACTER SET utf8)) AS parametros FROM mysql.proc ");
                    sbSql.AppendLine(string.Format("WHERE name = '{0}' AND db='{1}';", procedureName, dataBaseName));

                    var cmd = new MySqlCommand(sbSql.ToString(), cnn);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var strParametros = dr[0];
                            var arrayParam = strParametros.ToString().Trim().Split(',');

                            clsTables.AddRange(arrayParam.Select(p => p.Trim().Split(' ')).Select(ajustaParam => new StoredProcedureParameter
                            {
                                ParameterName = ajustaParam[1],
                                ParameterDataType = ajustaParam[2],
                                ParameterMaxBytes = RetornaBytesParametro(ajustaParam[2].ToLower()),
                                IsOutPutParameter = ajustaParam[0] != "IN",
                                ParameterDotNetType = _rorinas.DataTypeNetFramework(ajustaParam[2])
                            }));
                        }                        
                    }
                }
                else
                {
                    sbSql.AppendLine("SELECT PARAMETER_NAME, '' AS ParameterValue, DATA_TYPE AS ParameterDataType, CHARACTER_MAXIMUM_LENGTH AS ParameterMaxBytes, CASE PARAMETER_MODE WHEN 'IN' THEN 0 else 1 END AS IsOutPutParameter ");
                    sbSql.Append("FROM INFORMATION_SCHEMA.PARAMETERS ");
                    sbSql.AppendFormat("WHERE SPECIFIC_NAME='{0}' ", procedureName);
                    sbSql.Append("ORDER BY ORDINAL_POSITION;");

                    var cmd = new MySqlCommand(sbSql.ToString(), cnn);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            clsTables.Add(new StoredProcedureParameter
                            {
                                ParameterName = dr["PARAMETER_NAME"].ToString(),
                                ParameterDataType = dr["ParameterDataType"].ToString(),
                                ParameterMaxBytes = dr["ParameterMaxBytes"].ToString(),
                                IsOutPutParameter = Convert.ToBoolean(dr["IsOutPutParameter"] ?? false),
                                ParameterDotNetType = _rorinas.DataTypeNetFramework(dr["ParameterDataType"].ToString())
                            });
                        }                        
                    }
                }

                return clsTables;                
            }
        }

        private static string RetornaBytesParametro(string paramTipo)
        {
            if (paramTipo.Contains("char"))
            {
                try
                {
                    var parte1 = paramTipo.Split('(');
                    var parte2 = parte1[1].Split(')');
                    return parte2[0];
                }
                catch
                {
                    return "0";
                }
            }
            return "0";
        }

        /// <summary>
        /// Lista todos os campos da tabela
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="procedureName">Nome da Stored Procedure alvo</param>
        /// <param name="parametros"></param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<ColumnInfo> ListAllFieldsFromStoredProcedure(string dataBaseName, string procedureName, List<StoredProcedureParameter> parametros, DatabaseUser usr)
        {
            var clsTables = new List<ColumnInfo>();

            using (var cnn = Connection.DefaultConnection(usr))
            {

                cnn.Open();

                var cmd = new MySqlCommand(procedureName, cnn) {CommandType = CommandType.StoredProcedure};

                foreach (var param in parametros.Select(parametro => new MySqlParameter
                {
                    ParameterName = parametro.ParameterName,
                    Value = parametro.ParameterValue,
                    Direction = parametro.IsOutPutParameter ? ParameterDirection.Output : ParameterDirection.Input
                }))
                {
                    cmd.Parameters.Add(param);
                }

                using (var dr = cmd.ExecuteReader())
                {
                    var schemaTable = dr.GetSchemaTable();

                    if (schemaTable != null)
                    {
                        clsTables.AddRange(from DataRow myField in schemaTable.Rows
                            select new ColumnInfo
                            {
                                ColumnName = myField.ItemArray[0].ToString(),
                                Type = _rorinas.DataTypeNetFramework(_rorinas.ConvertTypeClrToSql(myField.ItemArray[11].ToString().Replace("System.", ""))),
                                IsNullability = false,
                                Size = Convert.ToInt32(myField.ItemArray[2].ToString())
                            });
                    }
                }
            }
            return clsTables;
        }
    }
}
