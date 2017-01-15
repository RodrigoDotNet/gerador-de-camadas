using System;
using System.Collections.Generic;
using System.Linq;
using DataDrain.Mapping;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using MySql.Data.MySqlClient;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
{
    public sealed class MapTabela : IInformationSchemaTable
    {
        private readonly IRotinasApoio _funcoes;

        public MapTabela(IRotinasApoio funcoes)
        {
            _funcoes = funcoes;
        }

        /// <summary>
        /// Retorna todas as tabelas do banco de dados selecionado
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<DadosObjeto> ListaAllTables(string dataBaseName, DadosUsuario usr)
        {
            var clsTables = new List<DadosObjeto>();

            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                cnn.Open();
                var cmd = new MySqlCommand(string.Format("set global innodb_stats_on_metadata=0;SELECT `TABLES`.TABLE_NAME, `TABLES`.CREATE_TIME, ifnull(`TABLES`.UPDATE_TIME,`TABLES`.CREATE_TIME) as DataAlteracao,table_rows AS QtdRegistros FROM INFORMATION_SCHEMA.`TABLES` WHERE (`TABLES`.TABLE_SCHEMA = '{0}') AND (`TABLES`.TABLE_TYPE = 'BASE TABLE') ORDER BY `TABLES`.TABLE_NAME ASC;", dataBaseName), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new DadosObjeto
                        {
                            Nome = dr[0].ToString(),
                            DataCriacao = Convert.ToDateTime(dr[1]),
                            DataAlteracao = Convert.ToDateTime(dr[2]),
                            Tipo = "tabela",
                            QtdRegistros = Convert.ToInt32(dr[3])
                        });
                    }
                }
            }
            return clsTables;
        }

        /// <summary>
        /// Lista todos os campos da tabela
        /// </summary>
        /// <param name="dataBaseName"></param>
        /// <param name="tableName"></param>
        /// <param name="usr"></param>
        /// <returns></returns>
        public List<DadosColunas> ListAllFieldsFromTable(string dataBaseName, string tableName, DadosUsuario usr)
        {
            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                cnn.Open();

                var sql = new System.Text.StringBuilder("SELECT COLUMNS.COLUMN_NAME AS Coluna, COLUMNS.DATA_TYPE AS Tipo, COLUMNS.IS_NULLABLE AS AceitaNull, COLUMNS.CHARACTER_MAXIMUM_LENGTH AS Tamanho, COLUMNS.COLUMN_KEY AS PK, COLUMNS.EXTRA AS is_identity, COLUMNS.COLUMN_DEFAULT AS DefaultValue, COLUMNS.privileges as Sync  ");
                sql.Append("FROM INFORMATION_SCHEMA.COLUMNS COLUMNS ");
                sql.Append(string.Format("WHERE (COLUMNS.TABLE_SCHEMA = '{0}') AND (COLUMNS.TABLE_NAME = '{1}');", dataBaseName, tableName));

                var cmd = new MySqlCommand(sql.ToString(), cnn);
                var dadosTabela = new List<DadosColunas>();


                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dadosTabela.Add(new DadosColunas
                        {
                            Coluna = dr["Coluna"].ToString(),
                            Tipo = _funcoes.RetornaTipoDadosDotNet(dr["Tipo"].ToString()),
                            AceitaNull = dr["AceitaNull"].ToString() != "NO",
                            Tamanho = dr["Tamanho"] != DBNull.Value
                                                    ? Convert.ToInt32(dr["Tamanho"])
                                                    : 0,
                            Pk = dr["PK"].ToString() == "PRI",
                            Identity = dr["is_identity"].ToString().Contains("auto_increment"),
                            DefaultValue = dr["DefaultValue"] != null ? dr["DefaultValue"].ToString() : "",
                            Sync = dr["Sync"].ToString().Contains("insert,update"),

                        });
                    }
                }

                return dadosTabela;
            }
        }

        public string RetornaEnum(string dataBaseName, string tableName, string colName, DadosUsuario usr)
        {
            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                cnn.Open();

                var sql = new System.Text.StringBuilder("SELECT COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS ");
                sql.AppendLine(string.Format("WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}';", dataBaseName, tableName, colName));

                var cmd = new MySqlCommand(sql.ToString(), cnn);

                var strEnum = "";

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if (dr[0].ToString().Contains("enum"))
                        {
                            var strTmp = dr[0].ToString().Replace("enum(", "").Replace(")", "").Split(',');
                            var valores = strTmp.Where(t => t.Replace("'", "").Length > 0).Cast<object>().ToList();

                            strEnum = string.Format("{0}|{1}", colName, string.Join(",", valores.Select(i => i.ToString()).ToArray()));
                        }
                    }
                }
                return strEnum;
            }
        }

        /// <summary>
        /// Lista todos os relacionamentos de chave estrangeira que a tabela possua
        /// </summary>
        /// <param name="nomeTabela">Nome da tabela a ser mapeada</param>
        /// <param name="dataBaseName">Nome do banco de dados </param>
        /// <param name="usr">Dados do usário para conexão </param>
        /// <returns></returns>
        public List<DadosAssociation> RetornaMapeamentoFK(string nomeTabela, string dataBaseName, DadosUsuario usr)
        {
            var relacionamentos = new List<DadosAssociation>();

            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                var sbSql = new System.Text.StringBuilder("select distinct TABLE_NAME,COLUMN_NAME,CONSTRAINT_NAME,REFERENCED_TABLE_NAME,REFERENCED_COLUMN_NAME  ");
                sbSql.Append("from INFORMATION_SCHEMA.KEY_COLUMN_USAGE ");
                sbSql.AppendFormat("where TABLE_NAME = '{0}' and referenced_table_name is not null;", nomeTabela);

                cnn.Open();
                var cmd = new MySqlCommand(sbSql.ToString(), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        relacionamentos.Add(new DadosAssociation
                        {
                            Tabela = dr[0].ToString(),
                            Coluna = dr[1].ToString(),
                            TabelaFK = dr[3].ToString(),
                            ColunaFK = dr[4].ToString(),
                            Constraint = dr[2].ToString()
                        });
                    }
                }
                return relacionamentos;
            }
        }
    }
}
