using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Formularios
{
    public partial class frmParametros : Form
    {

        public List<DadosStoredProceduresParameters> Parametros { get; set; }

        #region .: Load :.

        public frmParametros()
        {
            InitializeComponent();
        }

        private void frmParametros_Load(object sender, EventArgs e)
        {
            PreparaGridViewParametros();
            CarregaGridViewParametros();

            for (var i = 0; i < dgParametros.Rows.Count; i++)
            {
                dgParametros.Rows[i].Cells[0].ReadOnly = true;
                dgParametros.Rows[i].Cells[1].ReadOnly = true;
            }
        }

        #endregion

        #region .: Métodos :.

        private void CarregaGridViewParametros()
        {
            dgParametros.Rows.Clear();
            foreach (var param in Parametros.Where(p => !p.IsOutPutParameter))
            {
                var row = new DataGridViewRow();

                var nome = new DataGridViewTextBoxCell { Value = param.ParameterName };
                var tipo = new DataGridViewTextBoxCell { Value = param.ParameterDotNetType };
                var valor = new DataGridViewTextBoxCell { Value = param.ParameterValue };

                row.Cells.Add(nome);
                row.Cells.Add(tipo);
                row.Cells.Add(valor);

                dgParametros.Rows.Add(row);
            }
        }

        private void PreparaGridViewParametros()
        {
            var nome = new DataGridViewTextBoxColumn { HeaderText = "Nome", Name = "Nome", Width = 120 };
            var tipo = new DataGridViewTextBoxColumn { HeaderText = "Tipo", Name = "Tipo", Width = 80 };
            var valor = new DataGridViewTextBoxColumn { HeaderText = "Valor", Name = "Valor", Width = 120 };

            dgParametros.AutoGenerateColumns = false;

            dgParametros.Columns.Add(nome);
            dgParametros.Columns.Add(tipo);
            dgParametros.Columns.Add(valor);
        }

        #endregion

        #region .: Eventos :.

        private void bntGerar_Click(object sender, EventArgs e)
        {
            try
            {
                for (var i = 0; i < dgParametros.Rows.Count; i++)
                {
                    foreach (var parametro in Parametros.Where(parametro => parametro.ParameterName == dgParametros.Rows[i].Cells[0].Value.ToString()))
                    {
                        parametro.ParameterValue = dgParametros.Rows[i].Cells[2].Value.ConverteParaTipo();
                    }
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
