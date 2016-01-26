using System;
using System.Collections.Generic;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using MySql.Data.MySqlClient;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
{
    public sealed class MapView : IInformationSchemaView
    {
        private readonly IInformationSchemaTable _mapTabela;

        public MapView(IInformationSchemaTable mapTabela)
        {
            _mapTabela = mapTabela;
        }

        /// <summary>
        /// Retorna todas as Stored Procedures do banco de dados selecionado
        /// </summary>
        /// <param name="dataBaseName">Nome do banco de dados alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<DadosObjeto> ListaAllViews(string dataBaseName, DadosUsuario usr)
        {
            var clsTables = new List<DadosObjeto>();

            using (var cnn = Conexao.RetornaConexaoBase(usr))
            {
                cnn.Open();

                var cmd = new MySqlCommand(string.Format("set global innodb_stats_on_metadata=0;SELECT TABLE_NAME, CURRENT_TIMESTAMP as CREATE_TIME, CURRENT_TIMESTAMP as UPDATE_TIME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{0}' ORDER BY TABLE_NAME ASC;", dataBaseName), cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new DadosObjeto
                        {
                            Nome = dr[0].ToString(),
                            DataCriacao = Convert.ToDateTime(dr[1]),
                            DataAlteracao = Convert.ToDateTime(dr[2]),
                            Tipo = "view"
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
        /// <param name="viewsName">Nome da Stored Procedure alvo</param>
        /// <param name="usr">Dados do usuario</param>
        /// <returns></returns>
        public List<DadosColunas> ListAllFieldsFromViews(string dataBaseName, string viewsName, DadosUsuario usr)
        {
            return _mapTabela.ListAllFieldsFromTable(dataBaseName, viewsName, usr);
        }
    }
}
