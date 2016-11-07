using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Apoio.Configura
{
    public static class GridView
    {

        private const DadosColunas.ETipoSync tipoSyncCollumn = DadosColunas.ETipoSync.Always;

        /// <summary>
        /// Prepara o DataGridView para ser usado
        /// </summary>
        /// <param name="gvCampos">DataGridView alvo</param>
        public static void PreparaGridViewColunas(DataGridView gvCampos)
        {
            var colunas = new List<DataGridViewColumn>();
            

            colunas.Add(new DataGridViewImageColumn { HeaderText = "PK", Image = null, Name = "Tipo", Width = 50, ReadOnly = true });
            colunas.Add(new DataGridViewImageColumn { HeaderText = "AI", Image = null, Name = "Tipo", Width = 50, ReadOnly = true });
            colunas.Add(new DataGridViewTextBoxColumn { HeaderText = "Coluna", Name = "dataCadastro", Width = 130, ReadOnly = true });
            colunas.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", Name = "dataAlteracao", Width = 100, ReadOnly = true });
            colunas.Add(new DataGridViewTextBoxColumn { HeaderText = "Tamanho", Name = "Mapear", Width = 70, ReadOnly = true });
            colunas.Add(new DataGridViewTextBoxColumn { HeaderText = "Aceita Null", Name = "TipoStr", Width = 90, ReadOnly = true });
            colunas.Add(new DataGridViewTextBoxColumn { HeaderText = "Val. Padrão", Name = "TipoStr", Width = 100, ReadOnly = true });
            //colunas.Add(new DataGridViewCheckBoxColumn { HeaderText = "Mapear", Name = "TipoStr", Width = 50, ReadOnly = true });
            colunas.Add(new DataGridViewTextBoxColumn { HeaderText = "PkStr", Name = "PkStr", Width = 10, Visible = false });

            if (RegExpression.RegularExpressionStatus())
            {
                colunas.Add(new DataGridViewComboBoxColumn { HeaderText = "Reg. Expression", Name = "cbRegExp", Width = 200, Visible = true, DataSource = RegExpression.RetornaRegularExpressions(), DisplayMember = "Nome", ValueMember = "Expression", ValueType = typeof(DadosRegExpression), DataPropertyName = "DadosRegExpression", SortMode = DataGridViewColumnSortMode.NotSortable });
            }

            colunas.Add(new DataGridViewComboBoxColumn { HeaderText = "Tipo de Atualização", Name = "cbTipoSync", Width = 200, Visible = true, DataSource = tipoSyncCollumn.ToKeyPar(), DisplayMember = "Value", ValueMember = "Key", ValueType = typeof(DadosColunas.ETipoSync), DataPropertyName = "DadosTipoSync", SortMode = DataGridViewColumnSortMode.NotSortable });


            gvCampos.AutoGenerateColumns = false;
            gvCampos.Columns.AddRange(colunas.ToArray());
        }

        /// <summary>
        /// Carrega os dados das colunas
        /// </summary>
        /// <param name="gvCampos">DataGridView alvo</param>
        /// <param name="objetosMapeados">Objetos a ser mapeados</param>
        /// <param name="il">Imagelist com as imagens usadas no DataGridView</param>
        public static void CarregaGridViewColunas(DataGridView gvCampos, List<KeyValuePair<TipoObjetoBanco, List<DadosColunas>>> objetosMapeados, ImageList il)
        {
            try
            {
                //http://stackoverflow.com/questions/740581/applying-a-datagridviewcomboboxcell-selection-change-immediately
                gvCampos.Rows.Clear();

                //carrega as colunas do primeiro objeto
                var colunas = objetosMapeados.FirstOrDefault().Value;

                foreach (var coluna in colunas)
                {
                    var celulas = new List<DataGridViewCell>();

                    var row = new DataGridViewRow();

                    celulas.Add(new DataGridViewImageCell { Value = coluna.Pk ? il.Images[0] : il.Images[2], ToolTipText = coluna.Pk ? "Chave primaria" : "" });
                    celulas.Add(new DataGridViewImageCell { Value = coluna.Identity ? il.Images[1] : il.Images[2], ToolTipText = coluna.Identity ? "Auto Incremento" : "" });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Coluna });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Tipo });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Tamanho });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.AceitaNull });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.DefaultValue });
                    //celulas.Add(new DataGridViewCheckBoxCell { Value = true });
                    celulas.Add(new DataGridViewTextBoxCell { Value = "T" });
                    if (RegExpression.RegularExpressionStatus())
                    {
                        celulas.Add(new DataGridViewComboBoxCell {DataSource = RegExpression.RetornaRegularExpressions(), DisplayMember = "Nome", ValueMember = "Expression", Value = "sem"});
                    }
                    celulas.Add(new DataGridViewComboBoxCell { DataSource = tipoSyncCollumn.ToKeyPar(), DisplayMember = "Value", ValueMember = "Key", Value = 0 });

                    row.Cells.AddRange(celulas.ToArray());
                    gvCampos.Rows.Add(row);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        /// <summary>
        /// Carrega os dados das colunas
        /// </summary>
        /// <param name="gvCampos">DataGridView alvo</param>
        /// <param name="objetosMapeados">Objetos a ser mapeados</param>
        /// <param name="il">Imagelist com as imagens usadas no DataGridView</param>
        public static void CarregaGridViewColunas(DataGridView gvCampos, KeyValuePair<TipoObjetoBanco, List<DadosColunas>> objetoMapeados, ImageList il)
        {
            try
            {
                //http://stackoverflow.com/questions/740581/applying-a-datagridviewcomboboxcell-selection-change-immediately
                gvCampos.Rows.Clear();

                //carrega as colunas do primeiro objeto
                var colunas = objetoMapeados.Value;

                foreach (var coluna in colunas)
                {
                    var celulas = new List<DataGridViewCell>();

                    var row = new DataGridViewRow();

                    celulas.Add(new DataGridViewImageCell { Value = coluna.Pk ? il.Images[0] : il.Images[2], ToolTipText = coluna.Pk ? "Chave primaria" : "" });
                    celulas.Add(new DataGridViewImageCell { Value = coluna.Identity ? il.Images[1] : il.Images[2], ToolTipText = coluna.Identity ? "Auto Incremento" : "" });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Coluna });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Tipo });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.Tamanho });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.AceitaNull });
                    celulas.Add(new DataGridViewTextBoxCell { Value = coluna.DefaultValue });
                    //celulas.Add(new DataGridViewCheckBoxCell { Value = true });
                    celulas.Add(new DataGridViewTextBoxCell { Value = "T" });

                    if (RegExpression.RegularExpressionStatus())
                    {
                        celulas.Add(new DataGridViewComboBoxCell { DataSource = RegExpression.RetornaRegularExpressions(), DisplayMember = "Nome", ValueMember = "Expression", Value = coluna.RegExp == "" ? "sem" : coluna.RegExp });
                    }
                    celulas.Add(new DataGridViewComboBoxCell { DataSource = tipoSyncCollumn.ToKeyPar(), DisplayMember = "Value", ValueMember = "Key", Value = (int)coluna.TipoSync });

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
