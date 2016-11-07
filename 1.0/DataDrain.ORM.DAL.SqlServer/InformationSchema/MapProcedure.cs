using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL.SqlServer.InformationSchema
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
                var cmd = new SqlCommand("select name, create_date,modify_date from sys.procedures order by name;", cnn);

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
            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                var clsTables = new List<DadosStoredProceduresParameters>();

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
                        clsTables.Add(new DadosStoredProceduresParameters
                        {
                            ParameterName = dr[4].ToString(),
                            ParameterDataType = dr[5].ToString(),
                            ParameterMaxBytes = dr[6].ToString(),
                            IsOutPutParameter = Convert.ToBoolean(dr[7] ?? false),
                            ParameterDotNetType = _rorinas.RetornaTipoDadosDotNet(dr[5].ToString())
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
        public List<DadosColunas> ListAllFieldsFromStoredProcedure(string dataBaseName, string procedureName, List<DadosStoredProceduresParameters> parametros, DadosUsuario usr)
        {
            var clsTables = new List<DadosColunas>();

            using (var cnn = Conexao.RetornaConexaoBase(usr))
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
                                           select new DadosColunas
                                           {
                                               Coluna = myField.ItemArray[0].ToString(),
                                               Tipo = _rorinas.RetornaTipoDadosDotNet(myField.ItemArray[24].ToString()),
                                               AceitaNull = false,
                                               Tamanho = Convert.ToInt32(myField.ItemArray[2].ToString()),
                                               Pk = false,
                                               Identity = false,
                                               DefaultValue = ""
                                           });
                    }
                }
                return clsTables;
            }

        }
    }
}
