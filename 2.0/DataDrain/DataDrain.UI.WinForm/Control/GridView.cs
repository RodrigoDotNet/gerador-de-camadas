using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataDrain.Library.ExtensionMethods;
using DataDrain.Library.RegExp;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.UI.WinForm.Control
{
    public static class GridView
    {

        private const EColumnSync TipoSyncCollumn = EColumnSync.Always;

        /// <summary>
        /// Prepara o DataGridView para ser usado
        /// </summary>
        /// <param name="gvCampos">DataGridView alvo</param>
        public static void PreparaGridViewColunas(DataGridView gvCampos)
        {
            var colunas = new List<DataGridViewColumn>
            {
                new DataGridViewImageColumn
                {
                    HeaderText = "PK",
                    Image = null,
                    Name = "Tipo",
                    Width = 50,
                    ReadOnly = true
                },
                new DataGridViewImageColumn
                {
                    HeaderText = "AI",
                    Image = null,
                    Name = "Tipo",
                    Width = 50,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Coluna",
                    Name = "dataCadastro",
                    Width = 130,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Tipo",
                    Name = "dataAlteracao",
                    Width = 100,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {HeaderText = "Tamanho", Name = "Mapear", Width = 70, ReadOnly = true},
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Aceita Null",
                    Name = "TipoStr",
                    Width = 90,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Val. Padrão",
                    Name = "TipoStr",
                    Width = 100,
                    ReadOnly = true
                },
                new DataGridViewTextBoxColumn {HeaderText = "PkStr", Name = "PkStr", Width = 10, Visible = false},
                new DataGridViewComboBoxColumn
                {
                    HeaderText = "Reg. Expression",
                    Name = "cbRegExp",
                    Width = 200,
                    Visible = true,
                    DataSource = RegExpression.RetornaRegularExpressions(),
                    DisplayMember = "Nome",
                    ValueMember = "Expression",
                    ValueType = typeof (DadosRegExpression),
                    DataPropertyName = "DadosRegExpression",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewComboBoxColumn
                {
                    HeaderText = "Tipo de Atualização",
                    Name = "cbTipoSync",
                    Width = 200,
                    Visible = true,
                    DataSource = TipoSyncCollumn.ToKeyPar(),
                    DisplayMember = "Value",
                    ValueMember = "Key",
                    ValueType = typeof (EColumnSync),
                    DataPropertyName = "DadosTipoSync",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                }
            };




            gvCampos.AutoGenerateColumns = false;
            gvCampos.Columns.AddRange(colunas.ToArray());
        }

        /// <summary>
        /// Carrega os dados das colunas
        /// </summary>
        /// <param name="gvCampos">DataGridView alvo</param>
        /// <param name="objetosMapeados">Objetos a ser mapeados</param>
        /// <param name="il">Imagelist com as imagens usadas no DataGridView</param>
        public static void CarregaGridViewColunas(DataGridView gvCampos, DatabaseObjectInfo objetosMapeados, ImageList il)
        {
            try
            {
                //http://stackoverflow.com/questions/740581/applying-a-datagridviewcomboboxcell-selection-change-immediately
                gvCampos.Rows.Clear();

                //carrega as colunas do primeiro objeto
                var colunas = objetosMapeados.Columns;

                foreach (var coluna in colunas)
                {
                    var celulas = new List<DataGridViewCell>();

                    var row = new DataGridViewRow();

                    celulas.Add(new DataGridViewImageCell { Value = coluna.IsPrimaryKey ? il.Images[0] : il.Images[2], ToolTipText = coluna.IsPrimaryKey ? "Chave primaria" : "" });
                    celulas.Add(new DataGridViewImageCell { Value = coluna.IsIdentity ? il.Images[1] : il.Images[2], ToolTipText = coluna.IsIdentity ? "Auto Incremento" : "" });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.ColumnName });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Type });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Size });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.IsNullability });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.DefaultValue });
                    celulas.Add(new DataGridViewTextBoxCell { Value = "T" });
                    celulas.Add(new DataGridViewComboBoxCell { DataSource = RegExpression.RetornaRegularExpressions(), DisplayMember = "Nome", ValueMember = "Expression", Value = "sem" });
                    celulas.Add(new DataGridViewComboBoxCell { DataSource = TipoSyncCollumn.ToKeyPar(), DisplayMember = "Value", ValueMember = "Key", Value = 0 });

                    row.Cells.AddRange(celulas.ToArray());
                    gvCampos.Rows.Add(row);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
    }
}
