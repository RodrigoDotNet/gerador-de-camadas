using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using MySql.Data.MySqlClient;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
{
    public sealed class MapProcedure : IInformationSchemaProcedure
    {
        private readonly IRotinasApoio _rorinas;

        public MapProcedure(IRotinasApoio rorinas)
        {
            _rorinas = rorinas;
        }

        /// <summary>
        /// Retorna todas as Stored Procedures do banco de dados selecionado
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<DadosObjeto> ListaAllStoredProcedures(string dataBaseName, DadosUsuario usr)
        {
            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                var clsTables = new List<DadosObjeto>();

                cnn.Open();
                var cmd = new MySqlCommand(string.Format("SELECT ROUTINE_NAME, CREATED, LAST_ALTERED FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '{0}' AND ROUTINE_TYPE='PROCEDURE' ORDER BY ROUTINE_NAME ASC;", dataBaseName), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new DadosObjeto { Nome = dr[0].ToString(), DataCriacao = Convert.ToDateTime(dr[1]), DataAlteracao = Convert.ToDateTime(dr[2]), Tipo = "procedure" });
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
        public List<DadosStoredProceduresParameters> ListaAllStoredProceduresParameters(string dataBaseName, DadosUsuario usr, string procedureName)
        {
            using (var cnn=Conexao.RetornaConexaoBase(usr))
            {
                var clsTables = new List<DadosStoredProceduresParameters>();
    
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

                            clsTables.AddRange(arrayParam.Select(p => p.Trim().Split(' ')).Select(ajustaParam => new DadosStoredProceduresParameters
                            {
                                ParameterName = ajustaParam[1],
                                ParameterDataType = ajustaParam[2],
                                ParameterMaxBytes = RetornaBytesParametro(ajustaParam[2].ToLower()),
                                IsOutPutParameter = ajustaParam[0] != "IN",
                                ParameterDotNetType = _rorinas.RetornaTipoDadosDotNet(ajustaParam[2])
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
                            clsTables.Add(new DadosStoredProceduresParameters { ParameterName = dr["PARAMETER_NAME"].ToString(), ParameterDataType = dr["ParameterDataType"].ToString(), ParameterMaxBytes = dr["ParameterMaxBytes"].ToString(), IsOutPutParameter = Convert.ToBoolean(dr["IsOutPutParameter"] ?? false), ParameterDotNetType = _rorinas.RetornaTipoDadosDotNet(dr["ParameterDataType"].ToString()) });
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
        public List<DadosColunas> ListAllFieldsFromStoredProcedure(string dataBaseName, string procedureName, List<DadosStoredProceduresParameters> parametros, DadosUsuario usr)
        {
            var clsTables = new List<DadosColunas>();

            using (var cnn = Conexao.RetornaConexaoBase(usr))
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
                            select new DadosColunas
                            {
                                Coluna = myField.ItemArray[0].ToString(),
                                Tipo =_rorinas.RetornaTipoDadosDotNet(_rorinas.ConvertTipoClrToSql(myField.ItemArray[11].ToString().Replace("System.", ""))),
                                AceitaNull = false,
                                Tamanho = Convert.ToInt32(myField.ItemArray[2].ToString()),
                                Pk = false,
                                Identity = false,
                                DefaultValue = ""
                            });
                    }
                }
            }
            return clsTables;
        }
    }
}
