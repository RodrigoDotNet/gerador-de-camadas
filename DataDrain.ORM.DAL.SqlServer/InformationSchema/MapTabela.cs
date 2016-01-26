using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL.SqlServer.InformationSchema
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
            var clsTables=new List<DadosObjeto>();

            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                cnn.Open();
                var cmd = new SqlCommand("SELECT name AS Nome, create_date AS DataCriacao, modify_date AS DataAlteracao,(SELECT TOP 1 st.row_count FROM sys.dm_db_partition_stats st WHERE OBJECT_NAME(OBJECT_ID)=sys.tables.name) as Qtd FROM sys.tables ORDER BY name", cnn);

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

                var sql = new System.Text.StringBuilder(string.Format("SELECT C.name as Coluna, TY.name as Tipo, C.max_length AS Tamanho, c.is_nullable AS AceitaNull, C.is_identity ,(SELECT K.COLUMN_NAME as Coluna FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K ON T.CONSTRAINT_NAME = K.CONSTRAINT_NAME  WHERE T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.TABLE_NAME = '{0}' and K.COLUMN_NAME=C.name) AS PK, object_definition(C.default_object_id) AS [DefaultValue]  ", tableName));
                sql.Append("FROM sys.columns C ");
                sql.Append("INNER JOIN sys.tables T ON T.object_id = C.object_id ");
                sql.Append("INNER JOIN sys.types TY ON TY.user_type_id = C.user_type_id ");
                sql.Append("INNER JOIN sys.objects O ON O.object_id = C.object_id ");
                sql.Append(string.Format("WHERE T.name='{0}' ", tableName));

                var cmd = new SqlCommand(sql.ToString(), cnn);
                var dadosTabela = new List<DadosColunas>();


                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dadosTabela.Add(new DadosColunas
                        {
                            Coluna = dr["Coluna"].ToString(),
                            Tipo = _funcoes.RetornaTipoDadosDotNet(dr["Tipo"].ToString()),
                            AceitaNull = Convert.ToBoolean(dr["AceitaNull"]),
                            Tamanho = Convert.ToInt32(dr["Tamanho"]),
                            Pk = dr["PK"].ToString() != "",
                            Identity = Convert.ToBoolean(dr["is_identity"]),
                            DefaultValue = dr["DefaultValue"] != null ? dr["DefaultValue"].ToString() : "",
                            Sync = (!(dr["Coluna"].ToString().Contains("Alteracao") || dr["Coluna"].ToString().Contains("Cadastro") || dr["Coluna"].ToString().Contains("Criacao")))

                        });
                    }
                }

                return dadosTabela;
            }
        }

        public string RetornaEnum(string dataBaseName, string tableName, string colName, DadosUsuario usr)
        {
            return null;
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
                        relacionamentos.Add(new DadosAssociation
                        {
                            Tabela = dr[0].ToString(),
                            Coluna = dr[1].ToString(),
                            TabelaFK = dr[2].ToString(),
                            ColunaFK = dr[3].ToString(),
                            Constraint = dr[4].ToString()
                        });
                    }
                }
                return relacionamentos;
            }
        }
    }
}
