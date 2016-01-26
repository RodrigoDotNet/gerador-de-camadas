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

        #region Propriedades e variaveis

        private List<Provider> providers;

        #endregion

        #region Load

        public frmProvider()
        {
            InitializeComponent();
        }

        private void frmProvider_Load(object sender, EventArgs e)
        {
            //providers = Directory.GetFiles(Directory.GetCurrentDirectory(), "DataDrain.ORM.DAL.*.dll").Select(RetornaProviderInfo).Where(a => a != null).OrderBy(a => a.Prov.GetType().FullName).ToList();

            providers = RetornaProvidersReferenciados().OrderByDescending(p => p.Versao).ToList();

            pbLogo.DataBindings.Add("Image", "", "Logo");
            lblVersao.DataBindings.Add("Text", "", "Versao");
            lblBDMinimo.DataBindings.Add("Text", "", "BancoMinimo");
            lblTabela.DataBindings.Add("Text", "", "MapeamentoTabela");
            lblView.DataBindings.Add("Text", "", "MapeamentoView");
            lblProcedure.DataBindings.Add("Text", "", "MapeamentoProcedure");

            rptProviders.DataSource = providers;

            //Caso tenha apenas um provider no repositorio carrega automatico o provider
            if (providers.Count == 1)
            {
                rptProviders_ItemTemplate_DoubleClick(rptProviders, EventArgs.Empty);
            }

            labelControl1.Text = Versao.RetornaVersao();
        }

        #endregion

        #region Eventos

        private void rptProviders_ItemTemplate_DoubleClick(object sender, EventArgs e)
        {
            var imgLogo = rptProviders.CurrentItem.Controls["pbLogo"] as PictureBox;

            if (imgLogo != null)
            {
                try
                {
                    var frm = new frmGerador { Gerador = providers[rptProviders.CurrentItemIndex].Prov, Logo = imgLogo.Image };
                    Hide();
                    frm.ShowDialog();
                    frm.Dispose();
                }
                catch
                {
                }
                finally
                {
                    Show();
                }
            }
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

        private static Provider RetornaProviderInfo(string arquivo)
        {
            try
            {
                var assemblyReflection = Assembly.ReflectionOnlyLoadFrom(arquivo);
                foreach (var assembly in assemblyReflection.GetReferencedAssemblies())
                {
                    Assembly.ReflectionOnlyLoad(assembly.FullName);
                }

                if (ValidaPublicKey(assemblyReflection))
                {
                    var classeReflection = assemblyReflection.GetTypes().FirstOrDefault(t => t.IsClass && t.GetInterfaces().Any(i => i.Name.Contains("IInformationSchema")));

                    if (classeReflection != null)
                    {
                        var assembly = Assembly.LoadFrom(arquivo);
                        var classe = assembly.GetTypes().FirstOrDefault(t => t.IsClass && t.GetInterfaces().Any(i => i.Name.Contains("IInformationSchema")));

                        if (classe != null)
                        {
                            var obj = Activator.CreateInstance(classe) as IInformationSchema;

                            if (obj != null)
                            {
                                return new Provider
                                           {
                                               BancoMinimo = string.Format("Versão minima Banco: {0}", obj.InfoConexao.VersaoMinima),
                                               Versao = string.Format("Versão Provider: {0}", assembly.GetName().Version),
                                               Logo = RetornaLogo(assembly),
                                               Prov = obj
                                           };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                //if (ex is ReflectionTypeLoadException)
                //{
                //    var typeLoadException = ex as ReflectionTypeLoadException;
                //    var loaderExceptions = typeLoadException.LoaderExceptions;
                //    throw new Exception(loaderExceptions[0].Message, ex);
                //}
                //throw;
                return null;
            }
        }

        /// <summary>
        /// Valida o Token do assembly, comparando com o assebly do programa
        /// </summary>
        /// <param name="assemblyReflection"></param>
        /// <returns></returns>
        private static bool ValidaPublicKey(Assembly assemblyReflection)
        {
            var pkExterna = assemblyReflection.GetName().GetPublicKeyToken();

            if (pkExterna != null)
            {
                var pkInterna = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();

                return pkInterna.SequenceEqual(pkExterna);
            }
            return false;
        }

        private static Image RetornaLogo(Assembly assembly)
        {
            try
            {
                var nomesResources = assembly.GetManifestResourceNames();
                var rm = new ResourceManager(nomesResources[0].Replace(".resources", string.Empty), assembly);
                return ((Image)(rm.GetObject("logo")));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        #endregion

    }
}
