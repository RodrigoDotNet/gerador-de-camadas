using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.SqlServer
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
        public List<DatabaseObjectMap> ListAllViews(string dataBaseName, DatabaseUser usr)
        {
            var clsTables = new List<DatabaseObjectMap>();

            using (var cnn = Connection.DefaultConnection(usr))
            {
                cnn.Open();

                var cmd = new SqlCommand("SELECT name as Nome, create_date AS DataCriacao, modify_date AS DataAlteracao FROM sys.views", cnn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        clsTables.Add(new DatabaseObjectMap
                        {
                            Name = dr[0].ToString(),
                            CreationDate = Convert.ToDateTime(dr[1]),
                            ChangeDate = Convert.ToDateTime(dr[2]),
                            Type = EDatabaseObjectType.View
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
        public List<ColumnInfo> ListAllFieldsFromViews(string dataBaseName, string viewsName, DatabaseUser usr)
        {
            return _mapTabela.ListAllFieldsFromTable(dataBaseName, viewsName, usr);
        }
    }
}
