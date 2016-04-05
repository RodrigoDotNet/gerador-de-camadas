using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using JThomas.Controls;


namespace DataDrain.ORM.Generator.Formularios
{
    public partial class frmPesquisaManual : Form
    {
        private BindingList<DadosStoredProceduresParameters> _parametros;

        public IInformationSchema Gerador { get; set; }

        public KeyValuePair<TipoObjetoBanco, List<DadosColunas>> ObjetoSelecionado { get; private set; }

        public DadosUsuario DadosLogin { get; set; }

        public string BancoSelecionado { get; set; }

        public ImageList IlObjetos { get; set; }

        private List<KeyValuePair<string, string>> _tiposDadosParametros;

        public frmPesquisaManual()
        {
            InitializeComponent();
        }

        private void frmPesquisaManual_Load(object sender, EventArgs e)
        {
            _tiposDadosParametros = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("bool", "System.Boolean|"),
                new KeyValuePair<string, string>("Int32", "System.Int32|0000000"), 
                new KeyValuePair<string, string>("Int64", "System.Int64|0000000"),
                new KeyValuePair<string, string>("Float", "System.Single|"),
                new KeyValuePair<string, string>("Double", "System.Double|"), 
                new KeyValuePair<string, string>("Date", "System.DateTime|00/00/0000"),
                new KeyValuePair<string, string>("Time", "System.DateTime|00:00"), 
                new KeyValuePair<string, string>("DateTime", "System.DateTime|00/00/0000 00:00"),
                new KeyValuePair<string, string>("String", "System.String|"),
                new KeyValuePair<string, string>("Object", "System.Object|")
            };

            _parametros = new BindingList<DadosStoredProceduresParameters>();
            PreparaGridViewParametros();
        }

        #region Eventos

        private void txtConsulta_TextChanged(object sender, EventArgs e)
        {
            if (txtConsulta.Text.Contains(":"))
            {
                txtConsulta.Text = txtConsulta.Text.Replace(":", "@");
            }


            if (txtConsulta.Text.Contains("@"))
            {
                var consulta = txtConsulta.Text.Trim();
                var possicoes = txtConsulta.Text.AllIndexesOf("@");

                _parametros = new BindingList<DadosStoredProceduresParameters>();

                foreach (var posicao in possicoes)
                {
                    var nomeParametro = consulta.Substring(posicao, RetornaPosicaoFinalNomeParametro((posicao + 1), consulta));

                    if (_parametros.Count(p => p.ParameterName == nomeParametro)==0)
                    {
                        _parametros.Add(new DadosStoredProceduresParameters
                        {
                            ParameterName = nomeParametro,
                            DefineNull = true
                        });                        
                    }
                }
            }
            CarregaGridViewParametros();
        }

        private int RetornaPosicaoFinalNomeParametro(int posicao, string consulta)
        {
            if (consulta.IndexOf(" ", posicao, StringComparison.Ordinal) > -1)
            {
                return consulta.IndexOf(" ", posicao, StringComparison.Ordinal) - (posicao + 1);
            }
            if (consulta.IndexOf(";", posicao, StringComparison.Ordinal) > -1)
            {
                return consulta.IndexOf(";", posicao, StringComparison.Ordinal) - posicao;
            }
            return 0;
        }

        private void txtConsulta_Validating(object sender, CancelEventArgs e)
        {
            var palavrasRestritas = new List<string> { "insert", "update", "delete", "drop", "truncate", "alter", "create", "table" };

            foreach (var palavra in palavrasRestritas.Where(palavra => txtConsulta.Text.ToLower().Contains(palavra)))
            {
                errPadrao.SetError(txtConsulta, string.Format("Não é possivel usar a palavra chave '{0}'", palavra));
                btnMapear.Enabled = false;
                return;
            }
            errPadrao.SetError(txtConsulta, "");
            btnMapear.Enabled = true;
        }

        private void dgParametros_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (((bool)dgParametros.Rows[e.RowIndex].Cells[1].Value))
            {
                dgParametros.Rows[e.RowIndex].Cells[3].Value = null;
            }

            var tipoDados = dgParametros.Rows[e.RowIndex].Cells[2].Value.ToString().Split('|')[0];
            var mascara = dgParametros.Rows[e.RowIndex].Cells[2].Value.ToString().Split('|')[1];

            var tipo = Type.GetType(tipoDados);

            if (tipo != null)
            {
                var celula = dgParametros.Rows[e.RowIndex].Cells[3] as DataGridViewMaskedTextCell;

                if (celula != null)
                {
                    celula.Mask = mascara;
                }
            }

            dgParametros.Rows[e.RowIndex].Cells[3].ReadOnly = ((bool)dgParametros.Rows[e.RowIndex].Cells[1].Value);
        }

        private void btnMapear_Click(object sender, EventArgs e)
        {
            if (!txtConsulta.Text.ContainsInsensitive("select"))
            {
                MessageBox.Show("O comando SQL deve iniciar com SELECT", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_parametros.Count > 0)
            {
                foreach (var parametro in _parametros.Where(parametro => parametro.ParameterValue == null && !parametro.DefineNull))
                {
                    MessageBox.Show(string.Format("Parâmetro {0} não pode ser nulo", parametro.ParameterName), "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    for (var i = 0; i < dgParametros.Rows.Count; i++)
                    {
                        foreach (var parametro in _parametros.Where(parametro => parametro.ParameterName == dgParametros.Rows[i].Cells[0].Value.ToString()))
                        {
                            parametro.ParameterValue = ((bool)dgParametros.Rows[i].Cells[1].Value) ? null : Convert.ChangeType(dgParametros.Rows[i].Cells[3].Value, dgParametros.Rows[i].Cells[3].ValueType);
                            parametro.ParameterDotNetType = ((bool)dgParametros.Rows[i].Cells[1].Value) ? "object" : dgParametros.Rows[i].Cells[3].ValueType.FullName.Replace("System.", "");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            DadosLogin.DataBase = BancoSelecionado;

            List<DadosColunas> colunas;

            try
            {
                colunas = Gerador.MapQuery(txtConsulta.Text.Trim(), _parametros.ToList(), DadosLogin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var frm = new frmMapeamentoUnitario())
            {
                frm.Gerador = Gerador;
                frm.DadosLogin = DadosLogin;
                frm.BancoSelecionado = BancoSelecionado;
                frm.Parametros = _parametros.ToList();
                frm.IlObjetos = IlObjetos;
                frm.TipoObjeto = TipoObjetoBanco.ETipoObjeto.Query;
                frm.NomeObjeto = "Query";
                frm.Consulta = txtConsulta.Text.Trim();
                frm.Colunas = colunas;
                frm.button1.Enabled = false;
                frm.ShowDialog(this);

                ObjetoSelecionado = frm.ObjetosSelecionado;
            }
            Close();
        }

        #endregion


        #region Métodos

        private void PreparaGridViewParametros()
        {
            var nome = new DataGridViewTextBoxColumn { HeaderText = "Nome", Name = "Nome", Width = 120, ReadOnly = true };
            var setarNull = new DataGridViewCheckBoxColumn { HeaderText = "Valor nulo", Name = "DefineNull", Width = 100 };
            var lista = new DataGridViewComboBoxColumn { HeaderText = "Tipo de Parâmetro", Name = "cbTipoParan", Width = 100, Visible = true, DataSource = _tiposDadosParametros, DisplayMember = "Key", ValueMember = "Value", ValueType = typeof(KeyValuePair<string, string>), DataPropertyName = "ParameterDotNetType" };
            var valor = new DataGridViewMaskedTextColumn { HeaderText = "Valor", Name = "Valor", Width = 120, ReadOnly = true };


            dgParametros.AutoGenerateColumns = false;

            dgParametros.Columns.Add(nome);
            dgParametros.Columns.Add(setarNull);
            dgParametros.Columns.Add(lista);
            dgParametros.Columns.Add(valor);
        }

        private void CarregaGridViewParametros()
        {
            dgParametros.Rows.Clear();
            foreach (var param in _parametros)
            {
                var row = new DataGridViewRow();

                var nome = new DataGridViewTextBoxCell { Value = param.ParameterName };
                var valor = new DataGridViewMaskedTextCell { Value = param.ParameterValue };
                var defineNull = new DataGridViewCheckBoxCell { Value = param.DefineNull };
                var lista = new DataGridViewComboBoxCell { DataSource = _tiposDadosParametros, DisplayMember = "Key", ValueMember = "Value", Value = "System.Object|" };
                row.Cells.Add(nome);
                row.Cells.Add(defineNull);
                row.Cells.Add(lista);
                row.Cells.Add(valor);

                dgParametros.Rows.Add(row);
            }
        }

        #endregion

        private void dgParametros_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 3 && !((bool)dgParametros.Rows[e.RowIndex].Cells[1].Value))
            {
                var tipoDados = Type.GetType(dgParametros.Rows[e.RowIndex].Cells[2].Value.ToString().Split('|')[0]);
                try
                {
                    var celula = dgParametros.Rows[e.RowIndex].Cells[3] as DataGridViewMaskedTextCell;

                    var valor = Convert.ChangeType(celula.Value, tipoDados);
                    dgParametros.Rows[e.RowIndex].ErrorText = "";
                }
                catch (Exception ex)
                {
                    e.Cancel = true;
                    dgParametros.Rows[e.RowIndex].ErrorText = ex.Message;
                }
            }
        }


    }
}
