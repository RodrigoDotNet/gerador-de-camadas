using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL.SqlServer.InformationSchema
{
    public sealed class Map : IInformationSchema
    {
        public Map()
        {
            InfoConexao = new MapInfoConexao();
            RotinasApoio = new MapRotinasApoio();
            MapeamentoTabela = new MapTabela(RotinasApoio);
            MapeamentoView = new MapView(MapeamentoTabela);
            MapeamentoProcedure = new MapProcedure(RotinasApoio);

        }

        /// <summary>
        /// Lista todas as databases do servidor atual
        /// </summary>
        /// <param name="usr">Dados do usuario</param>
        /// <returns>lista com o nome dos bancos</returns>
        public List<string> ListAllDatabases(DadosUsuario usr)
        {
            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                var clsDatabases = new List<string>();
                cnn.Open();

                var cmd = new SqlCommand("sp_databases", cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            clsDatabases.Add(dr[0].ToString());
                        }
                    }
                    return clsDatabases.OrderBy(d => d).ToList();
                }
            }
        }

        /// <summary>
        /// Testa se é possivel iniciar uma conexão com o banco de dados
        /// </summary>
        /// <param name="usr">dados do usuario necessarios para a conexão</param>
        /// <returns></returns>
        public KeyValuePair<bool, string> TestarConexao(DadosUsuario usr)
        {
            try
            {
                using (var cnn = Conexao.RetornaConexaoBase(usr))
                {
                    var cmd = new SqlCommand("SELECT @@VERSION AS 'SQL Server Version';", cnn);
                    cnn.Open();
                    return new KeyValuePair<bool, string>(true, cmd.ExecuteScalar().ToString());
                }
            }
            catch
            {
                return new KeyValuePair<bool, string>(false, "Não foi possivel conectar ao banco.");
            }
        }

        public IInfoConexao InfoConexao { get; private set; }
        public IRotinasApoio RotinasApoio { get; private set; }

        public IInformationSchemaTable MapeamentoTabela { get; private set; }
        public IInformationSchemaView MapeamentoView { get; private set; }
        public IInformationSchemaProcedure MapeamentoProcedure { get; private set; }

        /// <summary>
        /// Indica se é compativel com mapeamento de tabelas
        /// </summary>
        /// <returns></returns>
        public bool CompativelMapeamentoTabela
        {
            get { return MapeamentoTabela != null; }
        }


        /// <summary>
        /// Indica se é compativel com mapeamento de views
        /// </summary>
        /// <returns></returns>
        public bool CompativelMapeamentoView
        {
            get { return MapeamentoView != null; }
        }

        /// <summary>
        /// Indica se é compativel com mapeamento de procedures
        /// </summary>
        /// <returns></returns>
        public bool CompativelMapeamentoProcedure
        {
            get { return MapeamentoProcedure != null; }
        }

        /// <summary>
        /// Lista dos objetos de conexão do provider
        /// </summary>
        public Dictionary<string, string> TiposObjetosAcaoBanco
        {
            get
            {
                return new Dictionary<string, string>
            {
                {"{IDbConnection}", "SqlConnection"},
                {"{IDbTransaction}", "SqlTransaction"},
                {"IDbCommand", "SqlCommand"},
                {"IDbDataParameter", "SqlParameter"},
                {"usingIDb","System.Data.SqlClient"}
            };
            }
        }

        public List<DadosColunas> MapQuery(string sql, List<DadosStoredProceduresParameters> parametros, DadosUsuario dadosLogin)
        {
            var retorno = new List<DadosColunas>();
            DataTable dt;

            using (var cnn = Conexao.RetornaConexaoBase(dadosLogin))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = sql;

                foreach (var parametro in parametros)
                {
                    cmd.Parameters.AddWithValue(parametro.ParameterName, parametro.DefineNull ? null : parametro.ParameterValue);
                }

                cnn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    dt = dr.GetSchemaTable();
                }
            }

            if (dt != null)
            {
                retorno.AddRange(from DataRow row in dt.Rows
                                 select new DadosColunas
                                 {
                                     Coluna = row["ColumnName"].ToString(),
                                     Pk = row["IsKey"] != DBNull.Value && (bool)row["IsKey"],
                                     AceitaNull = row["AllowDBNull"] != DBNull.Value && (bool)row["AllowDBNull"],
                                     Tipo = row["DataType"].ToString().Replace("System.", "").Replace("U", ""),
                                     TipoSync = DadosColunas.ETipoSync.Never
                                 });
            }

            return retorno;
        }
    }


}
