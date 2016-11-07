using System;
using System.Collections.Generic;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;
using MySql.Data.MySqlClient;

namespace DataDrain.Databases.MySql
{
    public sealed class MapView : IInformationSchemaView
    {
        private readonly IInformationSchemaTable _mapTabela;

        public MapView(IInformationSchemaTable mapTabela)
        {
            _mapTabela = mapTabela;
        }

        public List<DatabaseObjectMap> ListAllViews(string dataBaseName, DatabaseUser usr)
        {
            var clsTables = new List<DatabaseObjectMap>();

            using (var cnn = Connection.DefaultConnection(usr))
            {
                cnn.Open();

                var cmd = new MySqlCommand(string.Format("set global innodb_stats_on_metadata=0;SELECT TABLE_NAME, CURRENT_TIMESTAMP as CREATE_TIME, CURRENT_TIMESTAMP as UPDATE_TIME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{0}' ORDER BY TABLE_NAME ASC;", dataBaseName), cnn);

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
