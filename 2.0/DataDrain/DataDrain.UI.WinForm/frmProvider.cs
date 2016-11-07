using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Reflection;
using System.Windows.Forms;
using DataDrain.BusinessLayer;
using DataDrain.Rules.SuportObjects;


namespace DataDrain.UI.WinForm
{
    public partial class frmProvider : Form
    {
        private List<ProviderInfo> _providers;

        #region Load

        public frmProvider()
        {
            InitializeComponent();
        }

        private void frmProvider_Load(object sender, EventArgs e)
        {
            _providers = InformationSchemaBL.GetAllProviders();

            pbLogo.DataBindings.Add("Image", "", "Logo");
            pbLogo.DoubleClick += pbLogo_DoubleClick;
            lblVersao.DataBindings.Add("Text", "", "Versao");
            lblBDMinimo.DataBindings.Add("Text", "", "BancoMinimo");
            lblTabela.DataBindings.Add("Text", "", "MapeamentoTabela");
            lblView.DataBindings.Add("Text", "", "MapeamentoView");
            lblProcedure.DataBindings.Add("Text", "", "MapeamentoProcedure");

            rptProviders.DataSource = _providers;

            if (_providers.Count == 1)
            {
                rptProviders_ItemTemplate_DoubleClick(rptProviders, EventArgs.Empty);
            }

            labelControl1.Text = RetornaVersao();
        }

        private static string RetornaVersao()
        {
            return string.Format("Versão: {0}", ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.CurrentVersion :
                Assembly.GetExecutingAssembly().GetName().Version);
        }

        void pbLogo_DoubleClick(object sender, EventArgs e)
        {
            var pb = sender as PictureBox;

            if (pb != null)
            {
                rptProviders_ItemTemplate_DoubleClick(rptProviders, EventArgs.Empty);
            }
        }

        #endregion

        #region Eventos

        private void rptProviders_ItemTemplate_DoubleClick(object sender, EventArgs e)
        {
            var imgLogo = rptProviders.CurrentItem.Controls["pbLogo"] as PictureBox;

            if (imgLogo == null) return;

            var frm = new frmGerador { Gerador = _providers[rptProviders.CurrentItemIndex].Provider, Logo = imgLogo.Image };
            Hide();
            frm.Closed += (s, args) => Close();
            frm.ShowDialog();
            frm.Dispose();
        }

        private void bntSelecionar_Click(object sender, EventArgs e)
        {
            rptProviders_ItemTemplate_DoubleClick(rptProviders, EventArgs.Empty);
        }

        #endregion

    }
}