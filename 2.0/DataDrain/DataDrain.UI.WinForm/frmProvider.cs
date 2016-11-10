using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataDrain.BusinessLayer;
using DataDrain.Library.Helpers;
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

            pbLogo.DataBindings.Add("Image", "", "ImageLogo");
            pbLogo.DoubleClick += pbLogo_DoubleClick;
            lblVersao.DataBindings.Add("Text", "", "ProviderVersion");
            lblBDMinimo.DataBindings.Add("Text", "", "MinimalDatabaseVersion");
            lblTabela.DataBindings.Add("Text", "", "TableMapping");
            lblView.DataBindings.Add("Text", "", "ViewMapping");
            lblProcedure.DataBindings.Add("Text", "", "StoredProcedureMapping");

            rptProviders.DataSource = _providers;

            if (_providers.Count == 1)
            {
                rptProviders_ItemTemplate_DoubleClick(rptProviders, EventArgs.Empty);
            }

            labelControl1.Text = AssemblyHelper.RetornaVersao();
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