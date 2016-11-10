using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataDrain.BusinessLayer.History;
using DataDrain.Library.ExtensionMethods;
using DataDrain.Library.Helpers;
using DataDrain.Library.Registry;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;
using DataDrain.UI.WinForm.Control;
using DataDrain.UI.WinForm.SuportForms;

namespace DataDrain.UI.WinForm
{
    public partial class frmGerador : Form
    {
        #region Variaveis

        private List<DatabaseObjectInfo> _selectedObjects;

        private readonly ListViewColumnSorter _lvwColumnSorter;

        private List<KeyValuePair<string, Image>> _imgObjetosMapeados;

        private static bool _carregado;

        #endregion

        #region Propriedades

        public IInformationSchema Gerador { get; set; }

        public Image Logo { get; set; }

        private DatabaseUser User { get; set; }

        private List<StoredProcedureParameter> Parameters { get; set; }

        private string BancoSelecionado { get; set; }

        private BaseHistory Historico { get; set; }

        private List<DatabaseObjectMap> _dadosObjetos = new List<DatabaseObjectMap>();

        private TabPage PreviousTab;

        private TabPage CurrentTab;

        #endregion

        #region Load

        public frmGerador()
        {
            InitializeComponent();

            Historico = new XmlHistory();

            _lvwColumnSorter = new ListViewColumnSorter();
            lvObjetosBanco.ListViewItemSorter = _lvwColumnSorter;
        }

        private void frmGerador_Load(object sender, EventArgs e)
        {

            txtPorta.Text = Gerador.InfoConnection.DefaultPort.ToString();
            pbLogo.Image = Logo;

            lvObjetosBanco.CheckBoxes = true;

            CarregaConfiguracoes();
            ConfiguraAutoComplete();

            lblVersao.Text = AssemblyHelper.RetornaVersao();

            chkTrustedConnection.Visible = Gerador.InfoConnection.IsTrustedConnection;

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
                var dl = new DatabaseUser
                {
                    ServerAddress = txtServidor.Text.Trim(),
                    UserName = txtUsuario.Text.Trim(),
                    Password = txtSenha.Text.Trim(),
                    Port = txtPorta.Text.ToInt32(),
                    IsTrustedConnection = chkTrustedConnection.Checked
                };

                var retornoTeste = Gerador.TestConnection(dl);

                if (retornoTeste.Key)
                {
                    MessageBox.Show(string.Format("Conexão realizada com sucesso.\nVersão: {0}", retornoTeste.Value), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Erro ao efetuar a conexão", "ATENÇÃO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Cursor = Cursors.Default;
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
                User = new DatabaseUser
                {
                    ServerAddress = txtServidor.Text.Trim(),
                    UserName = txtUsuario.Text.Trim(),
                    Password = txtSenha.Text.Trim(),
                    Port = txtPorta.Text.ToInt32(),
                    IsTrustedConnection = chkTrustedConnection.Checked,
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
            var nomesTiposObjetos = new List<DatabaseObjectInfo>();

            for (var i = 0; i < lvObjetosBanco.Items.Count; i++)
            {
                if (lvObjetosBanco.Items[i].Checked && !_selectedObjects.Exists(a => a.Name == lvObjetosBanco.Items[i].Text))
                {
                    nomesTiposObjetos.Add(new DatabaseObjectInfo(lvObjetosBanco.Items[i].Text,lvObjetosBanco.Items[i].SubItems[4].Text.ConvertToEnum<EDatabaseObjectType>()));
                }
            }

            if (nomesTiposObjetos.Count(o => o.DatabaseObjectType == EDatabaseObjectType.Procedure) > 1)
            {
                foreach (var objetoBanco in nomesTiposObjetos)
                {
                    objetoBanco.Columns = CarregaCamposObjeto(objetoBanco.Name, objetoBanco.DatabaseObjectType, true);
                    _selectedObjects.Add(objetoBanco);
                }
            }
            else
            {
                foreach (var objeto in nomesTiposObjetos)
                {
                    CarregaCamposObjeto(objeto.Name, objeto.DatabaseObjectType);
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

                            Gerador.Support.CreateProjectFiles(new Configuration
                            {
                                DestinationPath = fb.SelectedPath,
                                User = User,
                                ObjetosMapeaveis = _selectedObjects,
                                NameSpace = txtNameSpace.Text.Trim(),
                                GerarAppConfig = chkGeraAppConfig.Checked,
                                AssinarProjeto = chkGeraSN.Checked,
                                MapWcf = chkMapWcf.Checked,
                                MapLinq = chkMapLinq.Checked,
                                TiposObjetosAcaoBanco = Gerador.AdoNetConnectionObjects
                            }, Gerador.DictionaryOfTemplates);

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
                            User = new DatabaseUser
                            {
                                ServerAddress = txtServidor.Text.Trim(),
                                UserName = txtUsuario.Text.Trim(),
                                Password = txtSenha.Text.Trim(),
                                Port = txtPorta.Text.ToInt32(),
                                IsTrustedConnection = chkTrustedConnection.Checked,
                                NomeProvedor = Gerador.GetType().FullName
                            };

                            if (PreviousTab == tpConexao)
                            {
                                Historico.SalvaConexao(User);
                            }

                            if (tbPrincipal.SelectedTab == tpBancoDados)
                            {
                                _selectedObjects = new List<DatabaseObjectInfo>();
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
                        else if (_selectedObjects.Any(q => q.QuerySql == null))
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
                var nomesTiposObjeto = new DatabaseObjectInfo(lvObjetosBanco.Items[e.Index].Text, lvObjetosBanco.Items[e.Index].SubItems[4].Text.ConvertToEnum<EDatabaseObjectType>(), new List<StoredProcedureParameter>());

                Parameters = Gerador.StoredProcedureMapping.ListAllStoredProceduresParameters(BancoSelecionado, User, lvObjetosBanco.Items[e.Index].Text);

                if (Parameters.Count > 0)
                {
                    var frm = new frmParametros { Parametros = Parameters };

                    if (frm.ShowDialog(this) == DialogResult.Yes)
                    {
                        nomesTiposObjeto.AjustaParametros(frm.Parametros);
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

                nomesTiposObjeto.Columns = CarregaCamposObjeto(nomesTiposObjeto.Name, nomesTiposObjeto.DatabaseObjectType, true);
                _selectedObjects.Add(nomesTiposObjeto);
            }

            if (e.NewValue == CheckState.Unchecked)
            {
                _selectedObjects = _selectedObjects.Where(o => o.Name != lvObjetosBanco.Items[e.Index].Text).ToList();
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
                        frm.DadosLogin = User;
                        frm.BancoSelecionado = BancoSelecionado;
                        frm.Parametros = Parameters;
                        frm.IlObjetos = ilObjetos;
                        frm.NomeObjeto = retorno.Item.Text;
                        frm.TipoObjeto = retorno.SubItem.Text.ConvertToEnum<EDatabaseObjectType>();

                        if (_selectedObjects.Exists(o => o.Name == retorno.Item.Text))
                        {
                            frm.ObjetosSelecionado = _selectedObjects.FirstOrDefault(o => o.Name == retorno.Item.Text);
                        }

                        switch (frm.ShowDialog(this))
                        {
                            case DialogResult.Yes:
                                if (!_selectedObjects.Exists(o => o.Name == frm.ObjetosSelecionado.Name))
                                {
                                    _selectedObjects.Add(frm.ObjetosSelecionado);
                                    _imgObjetosMapeados.Add(new KeyValuePair<string, Image>(retorno.Item.Text, ilIcones.Images["cheked"]));
                                }
                                else
                                {
                                    _selectedObjects = _selectedObjects.Where(o => o.Name != retorno.Item.Text).ToList();
                                    _selectedObjects.Add(frm.ObjetosSelecionado);
                                }
                                break;
                            default:
                                _selectedObjects = _selectedObjects.Where(o => o.Name != retorno.Item.Text).ToList();
                                _imgObjetosMapeados = _imgObjetosMapeados.Where(o => o.Key != retorno.Item.Text).ToList();
                                lvObjetosBanco.Items[retorno.Item.Index].Checked = false;
                                break;
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
                    User.DatabaseName = cbBancoDados.SelectedItem.ToString();

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
            using (var frm = new frmSobre())
            {
                frm.ShowDialog(this);
            }
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

                var itensAutoComplete = new AutoCompleteStringCollection();
                itensAutoComplete.AddRange(Historico.RetornaNomeLogins(txtServidor.Text, Gerador.GetType().FullName).ToArray());

                txtUsuario.AutoCompleteCustomSource = itensAutoComplete;
            }
        }

        private void txtUsuario_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                txtSenha.Text = Historico.RetornaSenhaLogins(txtServidor.Text, txtUsuario.Text, Gerador.GetType().FullName);
            }
        }

        private void chkOpcao_CheckedChanged(object sender, EventArgs e)
        {
            var chk = sender as CheckBox;

            if (chk != null && _carregado)
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

            var objetos = e.Result as List<DatabaseObjectMap>;

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
            if (!Gerador.InfoConnection.IsTrustedConnection)
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

        private void ConfiguraAutoComplete()
        {
            txtServidor.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtServidor.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var itensAutoComplete = new AutoCompleteStringCollection();
            itensAutoComplete.AddRange(Historico.RetornaListaNomeServidores(Gerador.GetType().FullName).ToArray());

            txtServidor.AutoCompleteCustomSource = itensAutoComplete;
            txtServidor.Focus();
        }

        private void CarregaConfiguracoes()
        {
            chkGeraAppConfig.Checked = RegistroWindows.RecuperaValor("chkGeraAppConfig", "false") == "true";
            chkGeraSN.Checked = RegistroWindows.RecuperaValor("chkGeraSN", "false") == "true";
            chkMapLinq.Checked = RegistroWindows.RecuperaValor("chkMapLinq", "false") == "true";
            chkMapWcf.Checked = RegistroWindows.RecuperaValor("chkMapWcf", "false") == "true";

            _carregado = true;
        }

        private void CarregaBancosDeDados()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (User == null)
                {
                    User = new DatabaseUser
                    {
                        ServerAddress = txtServidor.Text.Trim(),
                        UserName = txtUsuario.Text.Trim(),
                        Password = txtSenha.Text.Trim(),
                        Port = txtPorta.Text.ToInt32(),
                        IsTrustedConnection = chkTrustedConnection.Checked
                    };
                }

                cbBancoDados.DataSource = Gerador.ListAllDatabases(User);
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
            if (User == null)
            {
                return;
            }

            var objetos = new List<DatabaseObjectMap>();

            _imgObjetosMapeados = new List<KeyValuePair<string, Image>>();

            bwDadosBanco.ReportProgress(10);

            if (Gerador.IsTableMapping)
            {
                objetos.AddRange(Gerador.TableMapping.ListAllTables(BancoSelecionado, User));
                bwDadosBanco.ReportProgress(30);
            }

            if (Gerador.IsViewMapping)
            {
                objetos.AddRange(Gerador.ViewMapping.ListAllViews(BancoSelecionado, User));
                bwDadosBanco.ReportProgress(60);
            }

            if (Gerador.IsStoredProcedureMapping)
            {
                objetos.AddRange(Gerador.StoredProcedureMapping.ListAllStoredProcedures(BancoSelecionado, User));
                bwDadosBanco.ReportProgress(90);
            }

            doWorkEventArgs.Result = objetos;
        }

        private static int RetornaImagem(EDatabaseObjectType tipo)
        {
            switch (tipo)
            {
                case EDatabaseObjectType.Tabela:
                    return 0;
                case EDatabaseObjectType.View:
                    return 1;
                case EDatabaseObjectType.Procedure:
                    return 2;
                case EDatabaseObjectType.Query:
                    return 8;
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
        private List<ColumnInfo> CarregaCamposObjeto(string nomeObjeto, EDatabaseObjectType tipo, bool retornaDados = false)
        {
            try
            {
                var colunasObjeto = new List<ColumnInfo>();

                switch (tipo)
                {
                    case EDatabaseObjectType.Tabela:
                        colunasObjeto = Gerador.TableMapping.ListAllFieldsFromTable(BancoSelecionado, nomeObjeto, User);
                        break;

                    case EDatabaseObjectType.View:
                        colunasObjeto = Gerador.ViewMapping.ListAllFieldsFromViews(BancoSelecionado, nomeObjeto, User);
                        break;

                    case EDatabaseObjectType.Procedure:
                        if (MessageBox.Show(string.Format("Algumas procedures podem desencadear uma sequencia de insert's, update's e delete's.\nExecute apenas procedures que você conheça o funcionamento e que retornem dados.\nDeseja executar a procedure '{0}' ?", nomeObjeto), "ATENÇÃO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            colunasObjeto = Gerador.StoredProcedureMapping.ListAllFieldsFromStoredProcedure(BancoSelecionado, nomeObjeto, Parameters, User);
                        }
                        else
                        {
                            return retornaDados ? colunasObjeto : new List<ColumnInfo>();
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

                if (_selectedObjects == null)
                {
                    return new List<ColumnInfo>();
                }

                return colunasObjeto;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Erro:\n{0}", ex.Message), ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        /// <summary>
        /// Carrega os itens que foram selecionados para mapeamento
        /// </summary>
        private void ExibeObjetosSelecionados()
        {
            lvObjetosSelecionados.Items.Clear();

            lvObjetosSelecionados.Columns.Add("Nome", lvObjetosSelecionados.Width - 30);

            if (_selectedObjects != null && _selectedObjects.Count(o => o != null) > 0)
            {
                foreach (var objeto in _selectedObjects)
                {
                    lvObjetosSelecionados.Items.Add(new ListViewItem
                    {
                        Text = objeto.Name,
                        ImageIndex = RetornaImagem(objeto.DatabaseObjectType),
                        Group = lvObjetosSelecionados.Groups[objeto.DatabaseObjectType.ToString()]
                    });
                }

                //Ajusta o cabeçario dos grupos
                var totalGrupo = _selectedObjects.GroupBy(t => new { TipoObjeto = t.DatabaseObjectType, Qtd = _selectedObjects.Count(o => o.DatabaseObjectType == t.DatabaseObjectType) }).ToList();

                foreach (var grupo in totalGrupo)
                {
                    var nomePadrao = lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto.ToString()].Header.Contains(" ") 
                        ? lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto.ToString()].Header.Split(' ')[0] 
                        : lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto.ToString()].Header;

                    if (nomePadrao.EndsWith("s"))
                    {
                        nomePadrao = nomePadrao.Substring(0, nomePadrao.Length - 1);
                    }

                    lvObjetosSelecionados.Groups[grupo.Key.TipoObjeto.ToString()].Header = string.Format("{0}{1} ({2})", nomePadrao, grupo.Key.Qtd > 1 ? "s" : "", grupo.Key.Qtd);
                }
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

            var dadosFiltrados = string.IsNullOrWhiteSpace(busca) ? _dadosObjetos : _dadosObjetos.Where(d => d.Name.ToLower().StartsWith(busca.ToLower().Trim())).ToList();

            foreach (var item in dadosFiltrados.OrderBy(o => o.Name).Select(objeto => new ListViewItem
            {
                Text = objeto.Name,
                ImageIndex = RetornaImagem(objeto.Type),
                Group = lvObjetosBanco.Groups[objeto.Type.ToString()],
                SubItems = { objeto.CreationDate.ToShortDateString(), objeto.ChangeDate.Value.ToShortDateString(), objeto.Records.ToString(), objeto.Type.ToString().PrimeiraLetraMaiuscula() }
            }))
            {
                lvObjetosBanco.Items.Add(item);
            }

            var totalGrupo = dadosFiltrados.GroupBy(t => new { t.Type, Qtd = dadosFiltrados.Count(o => o.Type == t.Type) }).ToList();

            foreach (var grupo in totalGrupo)
            {
                var nomePadrao = lvObjetosBanco.Groups[grupo.Key.Type.ToString()].Header.Split(' ')[0];

                if (nomePadrao.EndsWith("s"))
                {
                    nomePadrao = nomePadrao.Substring(0, nomePadrao.Length - 1);
                }

                lvObjetosBanco.Groups[grupo.Key.Type.ToString()].Header = string.Format("{0}{1} ({2})", nomePadrao, grupo.Key.Qtd > 1 ? "s" : "", grupo.Key.Qtd);
            }
        }

        #endregion

        private void bntMapConsulta_Click(object sender, EventArgs e)
        {
            using (var frm = new frmPesquisaManual { Gerador = Gerador, DadosLogin = User, BancoSelecionado = cbBancoDados.Text, IlObjetos = ilObjetos })
            {
                frm.ShowDialog();

                if (frm.ObjetoSelecionado != null)
                {
                    _selectedObjects.Add(frm.ObjetoSelecionado);
                }
                else
                {
                    return;
                }
            }

            ExibeObjetosSelecionados();
            tbPrincipal.SelectTab(2);
            txtNameSpace.Focus();
        }




    }
}