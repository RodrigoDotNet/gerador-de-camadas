using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DataDrain.Databases.SqlServer.Template;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.SqlServer
{
    public sealed class Map : IInformationSchema
    {
        public Map()
        {
            InfoConnection = new MapInfoConexao();
            Support = new SupportMap();
            TableMapping = new MapTabela(Support);
            ViewMapping = new MapView(TableMapping);
            StoredProcedureMapping = new MapProcedure(Support);
            DictionaryOfTemplates = new TemplatesSqlServer();

        }

        /// <summary>
        /// Lista todas as databases do servidor atual
        /// </summary>
        /// <param name="usr">Dados do usuario</param>
        /// <returns>lista com o nome dos bancos</returns>
        public List<string> ListAllDatabases(DatabaseUser usr)
        {
            using (var cnn = Connection.DefaultConnection(usr))
            {
                var clsDatabases = new List<string>();
                cnn.Open();

                var cmd = new SqlCommand("sp_databases", cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.HasRows) return clsDatabases.OrderBy(d => d).ToList();

                    while (dr.Read())
                    {
                        clsDatabases.Add(dr[0].ToString());
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
        public KeyValuePair<bool, string> TestConnection(DatabaseUser usr)
        {
            try
            {
                using (var cnn = Connection.DefaultConnection(usr))
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

        public ISupport Support { get; private set; }

        public IInformationSchemaTable TableMapping { get; private set; }

        public IInformationSchemaView ViewMapping { get; private set; }

        public IInformationSchemaProcedure StoredProcedureMapping { get; private set; }

        public IInfoConnection InfoConnection { get; private set; }

        public ITemplateText DictionaryOfTemplates { get; private set; }

        public bool IsTableMapping { get { return TableMapping != null; } }

        public bool IsViewMapping { get { return ViewMapping != null; } }

        public bool IsStoredProcedureMapping { get { return StoredProcedureMapping != null; } }

        /// <summary>
        /// Lista dos objetos de conexão do provider
        /// </summary>
        public Dictionary<string, string> AdoNetConnectionObjects
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

        public List<ColumnInfo> MapQuery(string sql, List<StoredProcedureParameter> parametros, DatabaseUser dadosLogin)
        {
            var retorno = new List<ColumnInfo>();
            DataTable dt;

            using (var cnn = Connection.DefaultConnection(dadosLogin))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = sql;

                foreach (var parametro in parametros)
                {
                    cmd.Parameters.AddWithValue(parametro.ParameterName, parametro.DefaultNull ? null : parametro.ParameterValue);
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
                                 select new ColumnInfo
                                 {
                                     ColumnName = row["ColumnName"].ToString(),
                                     IsPrimaryKey = row["IsKey"] != DBNull.Value && (bool)row["IsKey"],
                                     IsNullability = row["AllowDBNull"] != DBNull.Value && (bool)row["AllowDBNull"],
                                     Type = row["DataType"].ToString().Replace("System.", "").Replace("U", ""),
                                     ColumnSync = EColumnSync.Never
                                 });
            }

            return retorno;
        }
    }


}
