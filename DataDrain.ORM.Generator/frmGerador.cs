using DataDrain.ORM.Generator.Apoio;
using DataDrain.ORM.Generator.Apoio.Base;
using DataDrain.ORM.Generator.Apoio.HistoricosConexao;
using DataDrain.ORM.Generator.Formularios;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace DataDrain.ORM.Generator
{
    public partial class frmGerador : Form
    {
        #region Variaveis

        private List<KeyValuePair<TipoObjetoBanco, List<DadosColunas>>> _objetosSelecionados;

        private readonly ListViewColumnSorter _lvwColumnSorter;

        private List<KeyValuePair<string, Image>> _imgObjetosMapeados;

        internal static string IdComputador;

        #endregion

        #region Propriedades

        public IInformationSchema Gerador { get; set; }

        public Image Logo { get; set; }

        private DadosUsuario DadosLogin { get; set; }

        private List<DadosStoredProceduresParameters> Parametros { get; set; }

        private string BancoSelecionado { get; set; }

        private string XmlLog4Net { get; set; }

        private HistoricoBase Historico { get; set; }

        private List<DadosObjeto> _dadosObjetos = new List<DadosObjeto>();

        private TabPage PreviousTab;

        private TabPage CurrentTab;

        #endregion

        #region Load

        public frmGerador()
        {
            InitializeComponent();

            Historico = new HistoricoSqlite();

            _lvwColumnSorter = new ListViewColumnSorter();
            lvObjetosBanco.ListViewItemSorter = _lvwColumnSorter;
        }

        private void frmGerador_Load(object sender, EventArgs e)
        {
            ProcessaIdComputador();

            txtPorta.Text = Gerador.InfoConexao.PortaPadrao.ToString();
            pbLogo.Image = Logo;

            lvObjetosBanco.CheckBoxes = true;

            CarregaConfiguracoes();
            ConfiguraAutoComplete();

            lblVersao.Text = Versao.RetornaVersao();

            chkTrustedConnection.Visible = Gerador.InfoConexao.TrustedConnection;

            toolTip1.SetToolTip(bntRefreshDatabase, "Carregar objetos do banco");
        }



        #endregion

        #region Eventos

        #region Nomais

        private void bntTestarConexao_Click(object sender, EventArgs e)
        {
            if (!errPadrao.HasErrors(tpConexao))
            {
                Cursor = Cursors.WaitCursor;
                var dl = new DadosUsuario
                    {
                        Servidor = txtServidor.Text.Trim(),
                        Usuario = txtUsuario.Text.Trim(),
                        Senha = txtSenha.Text.Trim(),
                        Porta = txtPorta.Text.ToInt32(),
                        TrustedConnection = chkTrustedConnection.Checked
                    };

                try
                {
                    MessageBox.Show(string.Format("Conexão realizada com sucesso.\nVersão: {0}", Gerador.TestarConexao(dl)), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Erro ao efetuar a conexão:\n{0}", ex.Message), ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            else
            {
                errPadrao.SetErrors();
            }
        }

        private void bntAvancar_Click(object sender, EventArgs e)
        {
            if (!errPadrao.HasErrors(tpConexao))
            {
                DadosLogin = new DadosUsuario
                {
                    Servidor = txtServidor.Text.Trim(),
                    Usuario = txtUsuario.Text.Trim(),
                    Senha = txtSenha.Text.Trim(),
                    Porta = txtPorta.Text.ToInt32(),
                    MaquinaID = IdComputador,
                    TrustedConnection = chkTrustedConnection.Checked,
                    NomeProvedor = Gerador.GetType().FullName
                };

                tbPrincipal.SelectTab(tpBancoDados);
            }
            else
            {
                errPadrao.SetErrors();
            }
        }


        private void chSelecionarTodos_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvObjetosBanco.Items)
            {
                item.Checked = item.Group != lvObjetosBanco.Groups["procedure"] && chSelecionarTodos.Checked;
            }
        }

        private void btnMapearSelecionados_Click(object sender, EventArgs e)
        {
            var nomesTiposObjetos = new List<TipoObjetoBanco>();

            for (var i = 0; i < lvObjetosBanco.Items.Count; i++)
            {
                if (lvObjetosBanco.Items[i].Checked && !_objetosSelecionados.Exists(a => a.Key.NomeObjeto == lvObjetosBanco.Items[i].Text))
                {
                    nomesTiposObjetos.Add(new TipoObjetoBanco(lvObjetosBanco.Items[i].Text, lvObjetosBanco.Items[i].SubItems[4].Text));
                }
            }

            //Apenas tipo tabela e view podem ter multiplas seleções
            if (nomesTiposObjetos.Count(o => o.TipoObjeto == TipoObjetoBanco.ETipoObjeto.Procedure) > 1)
            {
                foreach (var objetoBanco in nomesTiposObjetos)
                {
                    _objetosSelecionados.Add(new KeyValuePair<TipoObjetoBanco, List<DadosColunas>>(objetoBanco, CarregaCamposObjeto(objetoBanco.NomeObjeto, objetoBanco.TipoObjeto, true)));
                }
            }
            else
            {
                foreach (var objeto in nomesTiposObjetos)
                {
                    CarregaCamposObjeto(objeto.NomeObjeto, objeto.TipoObjeto);
                }
            }

            ExibeObjetosSelecionados();
            tbPrincipal.SelectTab(2);
            txtNameSpace.Focus();
        }

        private void btnMapear_Click(object sender, EventArgs e)
        {
            if (!errPadrao.HasErrors(this))
            {
                Cursor = Cursors.WaitCursor;

                try
                {
                    var fb = new FolderBrowserDialog { ShowNewFolderButton = true, RootFolder = Environment.SpecialFolder.Desktop, Description = "Selecione o local para salvar o projeto" };

                    if (fb.ShowDialog(this) == DialogResult.OK)
                    {
                        if (VerificaPermissaoPasta(fb.SelectedPath))
                        {

                            VerificaConfiguracaoLog4Net();

                            foreach (var colunas in from objeto in _objetosSelecionados from colunas in objeto.Value where colunas.RegExp == "sem" select colunas)
                            {
                                colunas.RegExp = "";
                            }

                            Gerador.RotinasApoio.CriarArquivosProjeto(new ParametrosCriarProjetos
                            {
                                CaminhoDestino = fb.SelectedPath,
                                DadosConexao = DadosLogin,
                                ObjetosMapeaveis = _objetosSelecionados,
                                NameSpace = txtNameSpace.Text.Trim(),
                                GerarAppConfig = chkGeraAppConfig.Checked,
                                AssinarProjeto = chkGeraSN.Checked,
                                XmlLog4Net = XmlLog4Net,
                                MapWcf = chkMapWcf.Checked,
                                MapLinq = chkMapLinq.Checked,
                                TiposObjetosAcaoBanco = Gerador.TiposObjetosAcaoBanco
                            });

                            CopiaDllLog(fb.SelectedPath);

                            MessageBox.Show("Mapeamento dos objetos realizado com sucesso", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start("explorer.exe", fb.SelectedPath);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("Não é possivel salvar os arquivos no caminho '{0}'\nverifique se o usuario possui permição nessa pasta e se ela não é somente leitura", fb.SelectedPath), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    fb.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro:\n" + ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void tbPrincipal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbPrincipal.SelectedIndex <= 2)
            {
                switch (tbPrincipal.SelectedIndex)
                {
                    case 0:
                    case 1:
                        if (errPadrao.HasErrors(tpConexao))
                        {
                            tbPrincipal.SelectTab(tpConexao);
                        }
                        else
                        {
                            DadosLogin = new DadosUsuario
                            {
                                Servidor = txtServidor.Text.Trim(),
                                Usuario = txtUsuario.Text.Trim(),
                                Senha = txtSenha.Text.Trim(),
                                Porta = txtPorta.Text.ToInt32(),
                                MaquinaID = IdComputador,
                                TrustedConnection = chkTrustedConnection.Checked,
                                NomeProvedor = Gerador.GetType().FullName
                            };

                            if (PreviousTab == tpConexao)
                            {
                                Historico.SalvaConexao(DadosLogin);
                            }

                            if (tbPrincipal.SelectedTab == tpBancoDados)
                            {
                                _objetosSelecionados = new List<KeyValuePair<TipoObjetoBanco, List<DadosColunas>>>();
                            }

                            CarregaBancosDeDados();
                        }
                        break;
                    case 2:
                        bool itemChecado = false;

                        for (int i = 0; i < lvObjetosBanco.Items.Count; i++)
                        {
                            if (lvObjetosBanco.Items[i].Checked)
                            {
                                itemChecado = true;
                                break;
                            }
                        }

                        if (itemChecado)
                        {
                            btnMapearSelecionados_Click(btnMapearSelecionados, EventArgs.Empty);
                        }
                        else
                        {
                            tbPrincipal.SelectedIndex = 1;
                        }


                        break;
                }
            }
        }

        private void txtPorta_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void lvObjetosBanco_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (lvObjetosBanco.Items[e.Index].Group == lvObjetosBanco.Groups["procedure"] && e.NewValue == CheckState.Checked)
            {
                var nomesTiposObjeto = new TipoObjetoBanco(lvObjetosBanco.Items[e.Index].Text, lvObjetosBanco.Items[e.Index].SubItems[4].Text, new List<DadosStoredProceduresParameters>());

                Parametros = Gerador.MapeamentoProcedure.ListaAllStoredProceduresParameters(BancoSelecionado, DadosLogin, lvObjetosBanco.Items[e.Index].Text);

                if (Parametros.Count > 0)
                {
                    var frm = new frmParametros { Parametros = Parametros };

                    if (frm.ShowDialog(this) == DialogResult.Yes)
                    {
                        nomesTiposObjeto.Parametros = frm.Parametros;
                    }
                    else
                    {
                        MessageBox.Show("Não é possivel carregar os dadso da StoredProcedure sem o preenchimento dos parametros", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        e.NewValue = CheckState.Unchecked;
                        return;
                    }
                    frm.Dispose();
                }
                _imgObjetosMapeados.Add(new KeyValuePair<string, Image>(lvObjetosBanco.Items[e.Index].Text, ilIcones.Images["cheked"]));
                _objetosSelecionados.Add(new KeyValuePair<TipoObjetoBanco, List<DadosColunas>>(nomesTiposObjeto, CarregaCamposObjeto(nomesTiposObjeto.NomeObjeto, nomesTiposObjeto.TipoObjeto, true)));
            }

            if (e.NewValue == CheckState.Unchecked)
            {
                _objetosSelecionados = _objetosSelecionados.Where(o => o.Key.NomeObjeto != lvObjetosBanco.Items[e.Index].Text).ToList();
                _imgObjetosMapeados = _imgObjetosMapeados.Where(o => o.Key != lvObjetosBanco.Items[e.Index].Text).ToList();
            }
        }

        private void lvObjetosBanco_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (var i = 0; i < lvObjetosBanco.SelectedItems.Count; i++)
            {
                if (lvObjetosBanco.SelectedItems[i].Group != lvObjetosBanco.Groups["procedure"])
                {
                    lvObjetosBanco.SelectedItems[i].Checked = true;
                }
                else
                {
                    lvObjetosBanco.SelectedItems[i].Selected = false;
                }
            }
        }

        private void lvObjetosBanco_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _lvwColumnSorter.SortColumn)
            {
                _lvwColumnSorter.Order = _lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _lvwColumnSorter.SortColumn = e.Column;
                _lvwColumnSorter.Order = SortOrder.Ascending;
            }

            AjustaCabecarioOrdemLIstView(e.Column, _lvwColumnSorter.Order);
            lvObjetosBanco.Sort();
        }

        private void lvObjetosBanco_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Header != lvObjetosBanco.Columns[4])
            {
                e.DrawDefault = true;
                return;
            }

            e.DrawBackground();
            var imageRect = new Rectangle(e.Bounds.X + ((lvObjetosBanco.Columns[e.ColumnIndex].Width / 2) - 8), e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
            var imgIcone = (_imgObjetosMapeados.FirstOrDefault(o => o.Key == e.Item.Text).Value) ?? ilIcones.Images["uncheked"];
            if (imgIcone == null) return;
            e.Graphics.DrawImage(imgIcone, imageRect);
        }

        private void lvObjetosBanco_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void lvObjetosBanco_MouseClick(object sender, MouseEventArgs e)
        {
            var retorno = lvObjetosBanco.HitTest(e.X, e.Y);

            if (retorno.SubItem != null)
            {
                if (retorno.SubItem.Text == "Tabela" || retorno.SubItem.Text == "View" || retorno.SubItem.Text == "Procedure")
                {
                    using (var frm = new frmMapeamentoUnitario())
                    {
                        frm.Gerador = Gerador;
                        frm.DadosLogin = DadosLogin;
                        frm.BancoSelecionado = BancoSelecionado;
                        frm.Parametros = Parametros;
                        frm.IlObjetos = ilObjetos;
                        frm.NomeObjeto = retorno.Item.Text;
                        frm.TipoObjeto = retorno.SubItem.Text.ConvertToEnum<TipoObjetoBanco.ETipoObjeto>();

                        if (_objetosSelecionados.Exists(o => o.Key.NomeObjeto == retorno.Item.Text))
                        {
                            frm.ObjetosSelecionado = _objetosSelecionados.FirstOrDefault(o => o.Key.NomeObjeto == retorno.Item.Text);
                        }

                        switch (frm.ShowDialog(this))
                        {
                            case DialogResult.Yes:
                                if (!_objetosSelecionados.Exists(o => o.Key.NomeObjeto == frm.ObjetosSelecionado.Key.NomeObjeto))
                                {
                                    _objetosSelecionados.Add(frm.ObjetosSelecionado);
                                    _imgObjetosMapeados.Add(new KeyValuePair<string, Image>(retorno.Item.Text, ilIcones.Images["cheked"]));
                                }
                                else
                                {
                                    _objetosSelecionados = _objetosSelecionados.Where(o => o.Key.NomeObjeto != retorno.Item.Text).ToList();
                                    _objetosSelecionados.Add(frm.ObjetosSelecionado);
                                }
                                break;
                            case DialogResult.No:
                                _objetosSelecionados = _objetosSelecionados.Where(o => o.Key.NomeObjeto != retorno.Item.Text).ToList();
                                _imgObjetosMapeados = _imgObjetosMapeados.Where(o => o.Key != retorno.Item.Text).ToList();
                                lvObjetosBanco.Items[retorno.Item.Index].Checked = false;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        private void bntRefreshDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (cbBancoDados.SelectedItem != null)
                {
                    BancoSelecionado = cbBancoDados.SelectedItem.ToString();
                    DadosLogin.DataBase = cbBancoDados.SelectedItem.ToString();

                    if (!bwDadosBanco.IsBusy)
                    {
                        bwDadosBanco.RunWorkerAsync();
                    }
                }
                else
                {
                    BancoSelecionado = "";
                    lvObjetosBanco.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os bancos", ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            var frm = new frmSobre();

            frm.ShowDialog(this);
        }

        private void chkTrustedConnection_CheckedChanged(object sender, EventArgs e)
        {
            txtUsuario.Enabled = !chkTrustedConnection.Checked;
            txtSenha.Enabled = !chkTrustedConnection.Checked;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CarregaListaObjetos(txtBuscar.Text);
        }

        private void tbPrincipal_Deselected(object sender, TabControlEventArgs e)
        {
            PreviousTab = e.TabPage;
        }

        private void tbPrincipal_Selected(object sender, TabControlEventArgs e)
        {
            CurrentTab = e.TabPage;
        }

        #endregion

        #region AutoComplete

        private void txtServidor_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtServidor.Text))
            {
                txtUsuario.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtUsuario.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtUsuario.AutoCompleteCustomSource = Historico.RetornaNomeLogins(txtServidor.Text, Gerador.GetType().FullName);
            }
        }

        private void txtUsuario_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                txtSenha.Text = Historico.RetornaSenhaLogins(txtServidor.Text, txtUsuario.Text, Gerador.GetType().FullName);
            }
        }

        private void chkOpcao_CheckedChanged(object sender, EventArgs e)
        {
            var chk = sender as CheckBox;

            if (chk != null)
            {
                RegistroWindows.GravaValor(chk.Name, chk.Checked.ToString());
            }
        }

        private void bwDadosBanco_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbAcao.Visible = true;
            pbAcao.Value = e.ProgressPercentage;
        }

        private void bwDadosBanco_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Ação cancelada pelo usuario.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                pbAcao.Visible = false;
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, e.Error.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                pbAcao.Visible = false;
                return;
            }

            var objetos = e.Result as List<DadosObjeto>;

            if (objetos == null)
            {
                MessageBox.Show("Não foi possivel recuperar a listagem de objetos.\nConsulte as permissões do usuario.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pbAcao.Visible = false;
                return;
            }

            _dadosObjetos = objetos;

            CarregaListaObjetos();

            pbAcao.Visible = false;
        }

        private void bwDadosBanco_DoWork(object sender, DoWorkEventArgs e)
        {
            CarregaObjetosBanco(e);
        }

        #endregion

        #region Validação

        private void txtServidor_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtServidor, string.IsNullOrWhiteSpace(txtServidor.Text) ? "Digite o servidor" : "");
        }

        private void txtUsuario_Validating(object sender, CancelEventArgs e)
        {
            if (!Gerador.InfoConexao.TrustedConnection)
            {
                errPadrao.SetErrorWithCount(txtUsuario, string.IsNullOrWhiteSpace(txtServidor.Text) ? "Digite o usuário" : "");
            }
        }

        private void txtPorta_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtPorta, string.IsNullOrWhiteSpace(txtPorta.Text) ? "Digite a porta" : "");
        }

        private void txtNameSpace_Validating(object sender, CancelEventArgs e)
        {
            txtNameSpace.Text = txtNameSpace.Text.Replace("..", "");
            var match = Regex.Match(txtNameSpace.Text, @"^(?![0-9.])[a-zA-Z0-9.]*$", RegexOptions.IgnoreCase);

            if (match.Success && !string.IsNullOrWhiteSpace(txtNameSpace.Text) && !txtNameSpace.Text.Contains("."))
            {
                errPadrao.SetErrorWithCount(txtNameSpace, "");
            }
            else
            {
                errPadrao.SetErrorWithCount(txtNameSpace, "Namespace inválido");
            }
        }

        #endregion

        #endregion

        #region Métodos

        private void CopiaDllLog(string diretorio)
        {
            if (chkLog4Net.Checked)
            {
                var asbl = GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(a => a.FullName.ToLower().Contains("log4net"));

                if (asbl != null)
                {
                    var arquivo = Assembly.ReflectionOnlyLoad(asbl.FullName).Location;

                    File.Copy(arquivo, string.Format("{0}\\{1}", diretorio, Path.GetFileName(arquivo)));
                }
            }
        }

        private void VerificaConfiguracaoLog4Net()
        {
            if (chkLog4Net.Checked)
            {
                chkGeraAppConfig.Checked = true;

                if (!string.IsNullOrEmpty(XmlLog4Net))
                {
                    if (MessageBox.Show("Já existe uma configuração ativa para o Log4Net.\nSe entrar no editor novamente a configuração antiga sera perdida.\nDeseja continuar?", "ATENÇÃO", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        return;
                    }
                }

                var frm = new frmLog4Net
                {
                    txtNomeArquivo = {Text = Arquivo.RetornaNomevalidoArquivo(txtNameSpace.Text)},
                    Gerador = Gerador
                };

                frm.ShowDialog(this);

                if (frm.Confirmou)
                {
                    XmlLog4Net = frm.XmlRetorno;
                }
                else
                {
                    chkLog4Net.Checked = false;
                    XmlLog4Net = string.Empty;
                }
            }
            else
            {
                XmlLog4Net = string.Empty;
            }
        }

        private void ConfiguraAutoComplete()
        {
            txtServidor.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtServidor.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtServidor.AutoCompleteCustomSource = Historico.RetornaNomeServidores(Gerador.GetType().FullName);
            txtServidor.Focus();
        }

        private void CarregaConfiguracoes()
        {
            chkGeraAppConfig.Checked = RegistroWindows.RecuperaValor("chkGeraAppConfig", "false") == "true";
            chkGeraSN.Checked = RegistroWindows.RecuperaValor("chkGeraSN", "false") == "true";
            chkLog4Net.Checked = RegistroWindows.RecuperaValor("chkLog4Net", "false") == "true";
            chkMapLinq.Checked = RegistroWindows.RecuperaValor("chkMapLinq", "false") == "true";
            chkMapWcf.Checked = RegistroWindows.RecuperaValor("chkMapWcf", "false") == "true";
        }

        private void CarregaBancosDeDados()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (DadosLogin == null)
                {
                    DadosLogin = new DadosUsuario
                    {
                        Servidor = txtServidor.Text.Trim(),
                        Usuario = txtUsuario.Text.Trim(),
                        Senha = txtSenha.Text.Trim(),
                        Porta = txtPorta.Text.ToInt32(),
                        TrustedConnection = chkTrustedConnection.Checked
                    };
                }

                cbBancoDados.DataSource = Gerador.ListAllDatabases(DadosLogin);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Houve um erro ao tentar recuperar a lista de bancos de dados.\nErro:" + ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CarregaObjetosBanco(DoWorkEventArgs doWorkEventArgs)
        {
            if (DadosLogin == null)
            {
                return;
            }

            var objetos = new List<DadosObjeto>();

            _imgObjetosMapeados = new List<KeyValuePair<string, Image>>();

            bwDadosBanco.ReportProgress(10);

            if (Gerador.CompativelMapeamentoTabela)
            {
                objetos.AddRange(Gerador.MapeamentoTabela.ListaAllTables(BancoSelecionado, DadosLogin));
                bwDadosBanco.ReportProgress(30);
            }

            if (Gerador.CompativelMapeamentoView)
            {
                objetos.AddRange(Gerador.MapeamentoView.ListaAllViews(BancoSelecionado, DadosLogin));
                bwDadosBanco.ReportProgress(60);
            }

            if (Gerador.CompativelMapeamentoProcedure)
            {
                objetos.AddRange(Gerador.MapeamentoProcedure.ListaAllStoredProcedures(BancoSelecionado, DadosLogin));
                bwDadosBanco.ReportProgress(90);
            }

            doWorkEventArgs.Result = objetos;
        }

        private static int RetornaImagem(string tipo)
        {
            switch (tipo.ToLower())
            {
                case "tabela":
                    return 0;
                case "view":
                    return 1;
                case "procedure":
                    return 2;
            }
            return 0;
        }

        private void AjustaCabecarioOrdemLIstView(int column, SortOrder order)
        {
            for (int i = 0; i < lvObjetosBanco.Columns.Count; i++)
            {
                if (lvObjetosBanco.Columns[i].Text.EndsWith("▲") || lvObjetosBanco.Columns[i].Text.EndsWith("▼"))
                {
                    lvObjetosBanco.Columns[i].Text = lvObjetosBanco.Columns[i].Text.Substring(0, lvObjetosBanco.Columns[i].Text.Length - 2);
                }
            }

            if (order == SortOrder.Ascending)
            {
                lvObjetosBanco.Columns[column].Text += " ▲";
            }
            else
            {
                lvObjetosBanco.Columns[column].Text += " ▼";
            }
        }

        /// <summary>
        /// Carrega os campos do objeto alvo
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto</param>
        /// <param name="tipo">tipo do objeto (tabela,view,procedure)</param>
        /// <param name="retornaDados">REtorna a lista de campos obtidas</param>
        private List<DadosColunas> CarregaCamposObjeto(string nomeObjeto, TipoObjetoBanco.ETipoObjeto tipo, bool retornaDados = false)
        {
            try
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
                        if (MessageBox.Show(string.Format("Algumas procedures podem desencadear uma sequencia de insert's, update's e delete's.\nExecute apenas procedures que você conheça o funcionamento e que retornem dados.\nDeseja executar a procedure '{0}' ?", nomeObjeto), "ATENÇÃO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            colunasObjeto = Gerador.MapeamentoProcedure.ListAllFieldsFromStoredProcedure(BancoSelecionado, nomeObjeto, Parametros, DadosLogin);
                        }
                        else
                        {
                            return retornaDados ? colunasObjeto : new List<DadosColunas>();
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException("tipo", "Tipo inválido");
                }

                if (retornaDados)
                {
                    return colunasObjeto;
                }
                //Carrega o objeto que deve ser exibido

                if (_objetosSelecionados == null)
                {
                    _objetosSelecionados = new List<KeyValuePair<TipoObjetoBanco, List<DadosColunas>>>();
                }

                _objetosSelecionados.Add(new KeyValuePair<TipoObjetoBanco, List<DadosColunas>>(new TipoObjetoBanco(nomeObjeto, tipo.ToString(), Parametros), colunasObjeto));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Erro:\n{0}", ex.Message), ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return null;
        }


        /// <summary>
        /// Carrega os itens que foram selecionados para mapeamento
        /// </summary>
        private void ExibeObjetosSelecionados()
        {
            lvObjetosSelecionados.Items.Clear();

            lvObjetosSelecionados.Columns.Add("Nome", lvObjetosSelecionados.Width - 30);

            if (_objetosSelecionados != null)
            {
                foreach (var objeto in _objetosSelecionados)
                {
                    lvObjetosSelecionados.Items.Add(new ListViewItem
                    {
                        Text = objeto.Key.NomeObjeto,
                        ImageIndex = RetornaImagem(objeto.Key.TipoObjeto.ToString()),
                        Group = lvObjetosSelecionados.Groups[objeto.Key.TipoObjeto.ToString()]
                    });
                }

                //Ajusta o cabeçario dos grupos
                var totalGrupo = _objetosSelecionados.GroupBy(t => new { TipoObjeto = t.Key.TipoObjeto.ToString(), Qtd = _objetosSelecionados.Count(o => o.Key.TipoObjeto == t.Key.TipoObjeto) }).ToList();

                foreach (var grupo in totalGrupo)
                {
                    var nomePadrao = lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto].Header.Contains(" ") ? lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto].Header.Split(' ')[0] : lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto].Header;

                    if (nomePadrao.EndsWith("s"))
                    {
                        nomePadrao = nomePadrao.Substring(0, nomePadrao.Length - 1);
                    }

                    lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto].Header = string.Format("{0}{1} ({2})", nomePadrao, grupo.Key.Qtd > 1 ? "s" : "", grupo.Key.Qtd);
                }
            }
        }

        private void ProcessaIdComputador()
        {
            try
            {
                const string computerName = "localhost";
                var scope = new ManagementScope(string.Format("\\\\{0}\\root\\CIMV2", computerName), null);
                scope.Connect();
                var query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
                var searcher = new ManagementObjectSearcher(scope, query);

                foreach (var wmiObject in searcher.Get().Cast<ManagementObject>())
                {
                    IdComputador = wmiObject["UUID"].ToString();
                }
            }
            catch (Exception)
            {

                IdComputador = Guid.NewGuid().ToString("D").ToUpper();
            }
        }


        private static bool VerificaPermissaoPasta(string caminho)
        {

            var di = new DirectoryInfo(caminho);
            var wi = WindowsIdentity.GetCurrent();

            if (wi != null)
            {
                var ntAccountName = wi.Name;
                var acl = di.GetAccessControl(AccessControlSections.Access);
                var rules = acl.GetAccessRules(true, true, typeof(NTAccount));

                foreach (var rule in rules.Cast<AuthorizationRule>().Where(rule => rule.IdentityReference.Value.Equals(ntAccountName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return (((FileSystemAccessRule)rule).FileSystemRights & FileSystemRights.WriteData) > 0;
                }
            }

            return !di.Attributes.HasFlag(FileAttributes.ReadOnly);
        }

        private void CarregaListaObjetos(string busca = null)
        {
            lvObjetosBanco.Clear();

            lvObjetosBanco.Columns.Add("Nome", 195);
            lvObjetosBanco.Columns.Add("Data de Criação", 107);
            lvObjetosBanco.Columns.Add("Data de Alteração", 120);
            lvObjetosBanco.Columns.Add("Qtd. Registros", 100, HorizontalAlignment.Center);
            lvObjetosBanco.Columns.Add("Avançado", 79, HorizontalAlignment.Center);

            var dadosFiltrados = string.IsNullOrWhiteSpace(busca) ? _dadosObjetos : _dadosObjetos.Where(d => d.Nome.ToLower().StartsWith(busca.ToLower().Trim())).ToList();

            foreach (var item in dadosFiltrados.OrderBy(o => o.Nome).Select(objeto => new ListViewItem
            {
                Text = objeto.Nome,
                ImageIndex = RetornaImagem(objeto.Tipo),
                Group = lvObjetosBanco.Groups[objeto.Tipo],
                SubItems = { objeto.DataCriacao.ToShortDateString(), objeto.DataAlteracao.ToShortDateString(), objeto.QtdRegistros.ToString(), objeto.Tipo.PrimeiraLetraMaiuscula() }
            }))
            {
                lvObjetosBanco.Items.Add(item);
            }

            var totalGrupo = dadosFiltrados.GroupBy(t => new { t.Tipo, Qtd = dadosFiltrados.Count(o => o.Tipo == t.Tipo) }).ToList();

            foreach (var grupo in totalGrupo)
            {
                var nomePadrao = lvObjetosBanco.Groups[grupo.Key.Tipo].Header.Split(' ')[0];

                if (nomePadrao.EndsWith("s"))
                {
                    nomePadrao = nomePadrao.Substring(0, nomePadrao.Length - 1);
                }

                lvObjetosBanco.Groups[grupo.Key.Tipo].Header = string.Format("{0}{1} ({2})", nomePadrao, grupo.Key.Qtd > 1 ? "s" : "", grupo.Key.Qtd);
            }
        }

        #endregion


    }
}