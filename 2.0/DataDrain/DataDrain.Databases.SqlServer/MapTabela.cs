using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.SqlServer
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
                var cmd = new SqlCommand("SELECT name AS Nome, create_date AS DataCriacao, modify_date AS DataAlteracao,(SELECT TOP 1 st.row_count FROM sys.dm_db_partition_stats st WHERE OBJECT_NAME(OBJECT_ID)=sys.tables.name) as Qtd FROM sys.tables ORDER BY name", cnn);

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

        /// <summary>
        /// Lista todos os campos da tabela
        /// </summary>
        /// <param name="dataBaseName"></param>
        /// <param name="tableName"></param>
        /// <param name="usr"></param>
        /// <returns></returns>
        public List<ColumnInfo> ListAllFieldsFromTable(string dataBaseName, string tableName, DatabaseUser usr)
        {
            using (var cnn = Connection.DefaultConnection(usr))
            {
                cnn.Open();

                var sql = @"SELECT C.name as Coluna, TY.name as Tipo, C.max_length AS Tamanho, c.is_nullable AS AceitaNull, 
                                C.is_identity ,(SELECT K.COLUMN_NAME as Coluna 
                                                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T 
                                                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K ON T.CONSTRAINT_NAME = K.CONSTRAINT_NAME  
                                            WHERE T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.TABLE_NAME = '" + tableName +
                          @"' and K.COLUMN_NAME=C.name) AS PK, 
                                object_definition(C.default_object_id) AS [DefaultValue]  
                            FROM sys.columns C 
                            INNER JOIN sys.tables T ON T.object_id = C.object_id 
                            INNER JOIN sys.types TY ON TY.user_type_id = C.user_type_id 
                            INNER JOIN sys.objects O ON O.object_id = C.object_id 
                            WHERE T.name='" + tableName + "';";

                var cmd = new SqlCommand(sql, cnn);
                var dadosTabela = new List<ColumnInfo>();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dadosTabela.Add(new ColumnInfo
                        {
                            ColumnName = dr["Coluna"].ToString(),
                            Type = _funcoes.DataTypeNetFramework(dr["Tipo"].ToString()),
                            IsNullability = Convert.ToBoolean(dr["AceitaNull"]),
                            Size = Convert.ToInt32(dr["Tamanho"]),
                            IsPrimaryKey = dr["PK"].ToString() != "",
                            IsIdentity = Convert.ToBoolean(dr["is_identity"]),
                            DefaultValue = dr["DefaultValue"] != null ? dr["DefaultValue"].ToString() : "",
                            IsSync = (!(dr["Coluna"].ToString().Contains("Alteracao") || dr["Coluna"].ToString().Contains("Cadastro") || dr["Coluna"].ToString().Contains("Criacao")))

                        });
                    }
                }

                return dadosTabela;
            }
        }

        /// <summary>
        /// Lista todos os relacionamentos de chave estrangeira que a tabela possua
        /// </summary>
        /// <param name="nomeTabela">Nome da tabela a ser mapeada</param>
        /// <param name="dataBaseName">Nome do banco de dados </param>
        /// <param name="usr">Dados do usário para conexão </param>
        /// <returns></returns>
        public List<ForeignKey> ListForeignKeysTable(string nomeTabela, string dataBaseName, DatabaseUser usr)
        {
            var relacionamentos = new List<ForeignKey>();

            using (var cnn = Connection.DefaultConnection(usr))
            {
                var sbSql = new System.Text.StringBuilder("SELECT FK.TABLE_NAME AS FK_Table, CU.COLUMN_NAME AS FK_Column, PK.TABLE_NAME AS PK_Table, PT.COLUMN_NAME AS PK_Column, C.CONSTRAINT_NAME AS Constraint_Name ");
                sbSql.Append("FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS C ");
                sbSql.Append("INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME  ");
                sbSql.Append("INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME ");
                sbSql.Append("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME ");
                sbSql.Append("INNER JOIN (SELECT i1.TABLE_NAME, i2.COLUMN_NAME ");
                sbSql.Append("  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS i1 ");
                sbSql.Append("  INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME ");
                sbSql.Append("  WHERE (i1.CONSTRAINT_TYPE = 'PRIMARY KEY')) AS PT ON PT.TABLE_NAME = PK.TABLE_NAME ");
                sbSql.AppendFormat("WHERE FK.TABLE_NAME='{0}' ", nomeTabela);
                sbSql.Append("ORDER BY FK_Table, FK_Column; ");

                cnn.Open();
                var cmd = new SqlCommand(sbSql.ToString(), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        relacionamentos.Add(new ForeignKey
                        {
                            TableName = dr[0].ToString(),
                            Column = dr[1].ToString(),
                            TableFk = dr[2].ToString(),
                            ColumnFk = dr[3].ToString(),
                            Constraint = dr[4].ToString()
                        });
                    }
                }
                return relacionamentos;
            }
        }
    }
}
