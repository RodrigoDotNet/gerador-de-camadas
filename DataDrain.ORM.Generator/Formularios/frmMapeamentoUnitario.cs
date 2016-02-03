using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataDrain.ORM.Generator.Apoio;
using DataDrain.ORM.Generator.Apoio.Configura;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Formularios
{
    public partial class frmMapeamentoUnitario : Form
    {

        #region Propriedades

        public IInformationSchema Gerador { get; set; }

        public DadosUsuario DadosLogin { get; set; }

        public string BancoSelecionado { get; set; }

        public List<DadosStoredProceduresParameters> Parametros { get; set; }

        public KeyValuePair<TipoObjetoBanco, List<DadosColunas>> ObjetosSelecionado { get; set; }

        public ImageList IlObjetos { get; set; }

        public string NomeObjeto { get; set; }

        public TipoObjetoBanco.ETipoObjeto TipoObjeto { get; set; }

        public List<DadosColunas> Colunas { get; set; }

        public string Consulta { get; set; }

        #endregion

        #region Load

        public frmMapeamentoUnitario()
        {
            InitializeComponent();
        }

        private void frmMapeamentoUnitario_Load(object sender, EventArgs e)
        {
            GridView.PreparaGridViewColunas(gvColunasObjeto);
            CarregaCamposObjeto(NomeObjeto, TipoObjeto);
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Carrega os campos do objeto alvo
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto</param>
        /// <param name="tipo">tipo do objeto (tabela,view,procedure)</param>
        private void CarregaCamposObjeto(string nomeObjeto, TipoObjetoBanco.ETipoObjeto tipo)
        {
            try
            {
                if (ObjetosSelecionado.Key == null)
                {
                    var colunasObjeto = new List<DadosColunas>();

                    switch (tipo)
                    {
                        case TipoObjetoBanco.ETipoObjeto.Tabela:
                            colunasObjeto = Gerador.MapeamentoTabela.ListAllFieldsFromTable(BancoSelecionado, nomeObjeto, DadosLogin);
                            break;

                        case TipoObjetoBanco.ETipoObjeto.View:
                            colunasObjeto = Gerador.MapeamentoView.ListAllFieldsFromViews(BancoSelecionado, nomeObjeto, DadosLogin);
                            break;

                        case TipoObjetoBanco.ETipoObjeto.Procedure:
                            Parametros = Gerador.MapeamentoProcedure.ListaAllStoredProceduresParameters(BancoSelecionado, DadosLogin, nomeObjeto);

                            if (Parametros.Count > 0)
                            {
                                var frm = new frmParametros { Parametros = Parametros };

                                if (frm.ShowDialog(this) == DialogResult.Yes)
                                {
                                    Parametros = frm.Parametros;
                                }
                                frm.Dispose();
                            }

                            if (MessageBox.Show(string.Format("Algumas procedures podem desencadear uma sequencia de insert's, update's e delete's.\nExecute apenas procedures que você conheça o funcionamento e que retornem dados.\nDeseja executar a procedure '{0}' ?", nomeObjeto), "ATENÇÃO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                            {
                                colunasObjeto = Gerador.MapeamentoProcedure.ListAllFieldsFromStoredProcedure(BancoSelecionado, nomeObjeto, Parametros, DadosLogin);
                            }

                            break;

                        case TipoObjetoBanco.ETipoObjeto.Query:
                            colunasObjeto = Colunas;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("tipo", "Tipo inválido");
                    }

                    ObjetosSelecionado = tipo == TipoObjetoBanco.ETipoObjeto.Query ?
                        new KeyValuePair<TipoObjetoBanco, List<DadosColunas>>(new TipoObjetoBanco(nomeObjeto, tipo.ToString(), Parametros, Consulta), colunasObjeto)
                        : new KeyValuePair<TipoObjetoBanco, List<DadosColunas>>(new TipoObjetoBanco(nomeObjeto, tipo.ToString(), Parametros), colunasObjeto);

                }

                GridView.CarregaGridViewColunas(gvColunasObjeto, ObjetosSelecionado, IlObjetos);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Erro:\n{0}", ex.Message), ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RecuperaRegularExpressionColuna()
        {
            if (RegExpression.RegularExpressionStatus())
            {
                foreach (var coluna in ObjetosSelecionado.Value)
                {
                    for (var i = 0; i < gvColunasObjeto.Rows.Count; i++)
                    {
                        if (gvColunasObjeto.Rows[i].Cells[2].Value.ToString() == coluna.Coluna)
                        {
                            coluna.RegExp = gvColunasObjeto.Rows[i].Cells[8].Value.ToString() != "sem"
                                ? gvColunasObjeto.Rows[i].Cells[8].Value.ToString()
                                : "";

                            coluna.TipoSync = (DadosColunas.ETipoSync)gvColunasObjeto.Rows[i].Cells[9].Value;

                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Eventos

        #region Eventos Gerais

        private void btnAvancar2_Click(object sender, EventArgs e)
        {
            RecuperaRegularExpressionColuna();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RecuperaRegularExpressionColuna();
        }

        #endregion

        #region Eventos Grid

        private void gvColunasObjeto_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (gvColunasObjeto.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "manual")
            {
                using (var frmReg = new frmRegExpression())
                {
                    frmReg.ShowDialog();

                    if (frmReg.DialogResult == DialogResult.Yes)
                    {
                        var teste = gvColunasObjeto.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;

                        if (teste != null)
                        {
                            var dados = teste.DataSource as List<DadosRegExpression>;

                            if (dados != null)
                            {
                                dados.Add(new DadosRegExpression { Nome = frmReg.txtRegexValidator.Text, Expression = frmReg.txtRegexValidator.Text });
                            }
                            teste.DataSource = dados;
                            gvColunasObjeto.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = frmReg.txtRegexValidator.Text;
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

    }
}
