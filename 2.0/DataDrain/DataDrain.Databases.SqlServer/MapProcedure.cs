using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.SqlServer
{
    public sealed class MapProcedure : IInformationSchemaProcedure
    {
        private readonly ISupport _rorinas;

        public MapProcedure(ISupport rorinas)
        {
            _rorinas = rorinas;
        }

        /// <summary>
        /// Retorna todas as Stored Procedures do banco de dados selecionado
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<DatabaseObjectMap> ListAllStoredProcedures(string dataBaseName, DatabaseUser usr)
        {
            using (var cnn = Connection.DefaultConnection(usr))
            {
                var clsTables = new List<DatabaseObjectMap>();

                cnn.Open();
                var cmd = new SqlCommand("select name, create_date,modify_date from sys.procedures order by name;", cnn);

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
            if (string.IsNullOrWhiteSpace(dataBaseName) || string.IsNullOrWhiteSpace(procedureName))
            {
                return new List<StoredProcedureParameter>();
            }

            using (var cnn = Connection.DefaultConnection(usr))
            {
                var clsTables = new List<StoredProcedureParameter>();

                cnn.Open();


                var sbSql = new System.Text.StringBuilder("SELECT SCHEMA_NAME(SCHEMA_ID) AS [Schema], SO.name AS [ObjectName], SO.Type_Desc AS [ObjectType (UDF/SP)], P.parameter_id AS [ParameterID], P.name AS [ParameterName], TYPE_NAME(P.user_type_id) AS [ParameterDataType], P.max_length AS [ParameterMaxBytes], P.is_output AS [IsOutPutParameter] ");
                sbSql.Append("FROM sys.objects AS SO ");
                sbSql.Append("INNER JOIN sys.parameters AS P ON SO.OBJECT_ID = P.OBJECT_ID ");
                sbSql.Append("WHERE SO.OBJECT_ID IN ( SELECT OBJECT_ID FROM sys.objects WHERE TYPE IN ('P')) and (SO.name=@ProceduresName) ");
                sbSql.Append("ORDER BY [Schema], SO.name, P.parameter_id;");

                var cmd = new SqlCommand(sbSql.ToString(), cnn);
                cmd.Parameters.AddWithValue("proceduresName", procedureName);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new StoredProcedureParameter
                        {
                            ParameterName = dr[4].ToString(),
                            ParameterDataType = dr[5].ToString(),
                            ParameterMaxBytes = dr[6].ToString(),
                            IsOutPutParameter = Convert.ToBoolean(dr[7] ?? false),
                            ParameterDotNetType = _rorinas.DataTypeNetFramework(dr[5].ToString())
                        });
                    }
                }

                return clsTables;
            }
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

                var cmd = new SqlCommand(procedureName, cnn) { CommandType = CommandType.StoredProcedure };

                if (parametros != null)
                {
                    foreach (var param in parametros.Select(parametro => new SqlParameter
                    {
                        ParameterName = parametro.ParameterName,
                        Value = parametro.ParameterValue,
                        Direction = parametro.IsOutPutParameter ? ParameterDirection.Output : ParameterDirection.Input
                    }))
                    {
                        cmd.Parameters.Add(param);
                    }
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
                                               Type = _rorinas.DataTypeNetFramework(myField.ItemArray[24].ToString()),
                                               IsNullability = false,
                                               Size = Convert.ToInt32(myField.ItemArray[2].ToString()),
                                               IsPrimaryKey = false,
                                               IsIdentity = false
                                           });
                    }
                }
                return clsTables;
            }

        }
    }
}
