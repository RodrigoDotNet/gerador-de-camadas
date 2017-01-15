using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using DataDrain.ORM.Generator.Apoio;
using DataDrain.ORM.Interfaces;



namespace DataDrain.ORM.Generator
{
    public partial class frmProvider : Form
    {
        private List<Provider> _providers;

        #region Load

        public frmProvider()
        {
            InitializeComponent();
        }

        private void frmProvider_Load(object sender, EventArgs e)
        {
            _providers = RetornaProvidersReferenciados().OrderByDescending(p => p.Versao).ToList();

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

            labelControl1.Text = Versao.RetornaVersao();
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

            var frm = new frmGerador { Gerador = _providers[rptProviders.CurrentItemIndex].Prov, Logo = imgLogo.Image };
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

        #region Métodos

        private static IEnumerable<Provider> RetornaProvidersReferenciados()
        {
            return new List<Provider>
                {
                    RetornaProvider(new DAL.MySQL.InformationSchema.Map()), 
                    RetornaProvider(new DAL.SqlServer.InformationSchema.Map()), 
                };
        }

        private static Provider RetornaProvider(IInformationSchema obj)
        {
            var assembly = obj.GetType().Assembly;


            return new Provider
                {
                    BancoMinimo = string.Format("Versão minima Banco: {0}", obj.InfoConexao.VersaoMinima),
                    Versao = string.Format("Versão Provider: {0}", assembly.GetName().Version),
                    MapeamentoTabela = string.Format("Mapeamento de Tabelas: {0}", (obj.CompativelMapeamentoTabela ? "Sim" : "Não")),
                    MapeamentoView = string.Format("Mapeamento de Views: {0}", (obj.CompativelMapeamentoView ? "Sim" : "Não")),
                    MapeamentoProcedure = string.Format("Mapeamento de Procedure: {0}", (obj.CompativelMapeamentoProcedure ? "Sim" : "Não")),
                    Logo = RetornaLogo(assembly),
                    Prov = obj
                };
        }

        private static Image RetornaLogo(Assembly assembly)
        {
            var nomesResources = assembly.GetManifestResourceNames();
            var rm = new ResourceManager(nomesResources[0].Replace(".resources", string.Empty), assembly);
            return ((Image)(rm.GetObject("logo")));
        }

        #endregion

    }
}
