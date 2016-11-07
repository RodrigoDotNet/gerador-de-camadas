using System;
using System.Collections.Generic;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;
using MySql.Data.MySqlClient;

namespace DataDrain.Databases.MySql
{
    public sealed class MapTabela : IInformationSchemaTable
    {
        private readonly ISupport _funcoes;

        public MapTabela(ISupport funcoes)
        {
            _funcoes = funcoes;
        }

        /// <summary>
        /// Retorna todas as tabelas do banco de dados selecionado
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<DatabaseObjectMap> ListAllTables(string dataBaseName, DatabaseUser usr)
        {
            var clsTables = new List<DatabaseObjectMap>();

            using (var cnn = Connection.DefaultConnection(usr))
            {
                cnn.Open();
                var cmd = new MySqlCommand(string.Format("set global innodb_stats_on_metadata=0;SELECT `TABLES`.TABLE_NAME, `TABLES`.CREATE_TIME, ifnull(`TABLES`.UPDATE_TIME,`TABLES`.CREATE_TIME) as DataAlteracao,table_rows AS QtdRegistros FROM INFORMATION_SCHEMA.`TABLES` WHERE (`TABLES`.TABLE_SCHEMA = '{0}') AND (`TABLES`.TABLE_TYPE = 'BASE TABLE') ORDER BY `TABLES`.TABLE_NAME ASC;", dataBaseName), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new DatabaseObjectMap
                        {
                            Name = dr[0].ToString(),
                            CreationDate = Convert.ToDateTime(dr[1]),
                            ChangeDate = Convert.ToDateTime(dr[2]),
                            Type = EDatabaseObjectType.Tabela,
                            Records = Convert.ToInt32(dr[3])
                        });
                    }
                }
            }
            return clsTables;
        }

        public List<ColumnInfo> ListAllFieldsFromTable(string dataBaseName, string tableName, DatabaseUser usr)
        {
            using (var cnn = Connection.DefaultConnection(usr))
            {
                cnn.Open();

                var sql = new System.Text.StringBuilder("SELECT COLUMNS.COLUMN_NAME AS Coluna, COLUMNS.DATA_TYPE AS Tipo, COLUMNS.IS_NULLABLE AS AceitaNull, COLUMNS.CHARACTER_MAXIMUM_LENGTH AS Tamanho, COLUMNS.COLUMN_KEY AS PK, COLUMNS.EXTRA AS is_identity, COLUMNS.COLUMN_DEFAULT AS DefaultValue, COLUMNS.privileges as Sync  ");
                sql.Append("FROM INFORMATION_SCHEMA.COLUMNS COLUMNS ");
                sql.Append(string.Format("WHERE (COLUMNS.TABLE_SCHEMA = '{0}') AND (COLUMNS.TABLE_NAME = '{1}');", dataBaseName, tableName));

                var cmd = new MySqlCommand(sql.ToString(), cnn);
                var dadosTabela = new List<ColumnInfo>();


                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dadosTabela.Add(new ColumnInfo
                        {
                            ColumnName = dr["Coluna"].ToString(),
                            Type = _funcoes.DataTypeNetFramework(dr["Tipo"].ToString()),
                            IsNullability = dr["AceitaNull"].ToString() != "NO",
                            Size = dr["Tamanho"] != DBNull.Value
                                                    ? Convert.ToInt32(dr["Tamanho"])
                                                    : 0,
                            IsPrimaryKey = dr["PK"].ToString() == "PRI",
                            IsIdentity = dr["is_identity"].ToString().Contains("auto_increment"),
                            DefaultValue = dr["DefaultValue"] != null ? dr["DefaultValue"].ToString() : "",
                            IsSync = dr["Sync"].ToString().Contains("insert,update"),

                        });
                    }
                }

                return dadosTabela;
            }
        }

        public List<ForeignKey> ListForeignKeysTable(string nomeTabela, string dataBaseName, DatabaseUser usr)
        {
            var relacionamentos = new List<ForeignKey>();

            using (var cnn = Connection.DefaultConnection(usr))
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
                        relacionamentos.Add(new ForeignKey
                        {
                            TableName = dr[0].ToString(),
                            Column = dr[1].ToString(),
                            TableFk = dr[3].ToString(),
                            ColumnFk = dr[4].ToString(),
                            Constraint = dr[2].ToString()
                        });
                    }
                }
                return relacionamentos;
            }
        }
    }
}
