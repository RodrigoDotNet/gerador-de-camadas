using System.Windows.Forms;

namespace DataDrain.UI.WinForm
{
    partial class frmGerador
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGerador));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Tabela", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("View", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Procedure", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Tabela", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("View", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("Procedure", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("Query", System.Windows.Forms.HorizontalAlignment.Left);
            this.tbPrincipal = new System.Windows.Forms.TabControl();
            this.tpConexao = new System.Windows.Forms.TabPage();
            this.chkTrustedConnection = new System.Windows.Forms.CheckBox();
            this.bntTestarConexao = new System.Windows.Forms.Button();
            this.bntAvancar = new System.Windows.Forms.Button();
            this.txtPorta = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.txtSenha = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServidor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tpBancoDados = new System.Windows.Forms.TabPage();
            this.bntMapConsulta = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.pbAcao = new System.Windows.Forms.ProgressBar();
            this.bntRefreshDatabase = new System.Windows.Forms.Button();
            this.cbBancoDados = new System.Windows.Forms.ComboBox();
            this.lvObjetosBanco = new System.Windows.Forms.ListView();
            this.ilIcones = new System.Windows.Forms.ImageList(this.components);
            this.btnMapearSelecionados = new System.Windows.Forms.Button();
            this.chSelecionarTodos = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tpMapeamento = new System.Windows.Forms.TabPage();
            this.simpleButton1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkMapWcf = new System.Windows.Forms.CheckBox();
            this.chkMapLinq = new System.Windows.Forms.CheckBox();
            this.chkGeraSN = new System.Windows.Forms.CheckBox();
            this.chkGeraAppConfig = new System.Windows.Forms.CheckBox();
            this.btnMapear = new System.Windows.Forms.Button();
            this.lvObjetosSelecionados = new System.Windows.Forms.ListView();
            this.txtNameSpace = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.errPadrao = new System.Windows.Forms.ErrorProvider(this.components);
            this.lblVersao = new System.Windows.Forms.Label();
            this.ilObjetos = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.bwDadosBanco = new System.ComponentModel.BackgroundWorker();
            this.tbPrincipal.SuspendLayout();
            this.tpConexao.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.tpBancoDados.SuspendLayout();
            this.tpMapeamento.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errPadrao)).BeginInit();
            this.SuspendLayout();
            // 
            // tbPrincipal
            // 
            this.tbPrincipal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPrincipal.Controls.Add(this.tpConexao);
            this.tbPrincipal.Controls.Add(this.tpBancoDados);
            this.tbPrincipal.Controls.Add(this.tpMapeamento);
            this.tbPrincipal.ImageList = this.imageList1;
            this.tbPrincipal.Location = new System.Drawing.Point(13, 13);
            this.tbPrincipal.Name = "tbPrincipal";
            this.tbPrincipal.SelectedIndex = 0;
            this.tbPrincipal.Size = new System.Drawing.Size(646, 286);
            this.tbPrincipal.TabIndex = 0;
            this.tbPrincipal.SelectedIndexChanged += new System.EventHandler(this.tbPrincipal_SelectedIndexChanged);
            this.tbPrincipal.Selected += new System.Windows.Forms.TabControlEventHandler(this.tbPrincipal_Selected);
            this.tbPrincipal.Deselected += new System.Windows.Forms.TabControlEventHandler(this.tbPrincipal_Deselected);
            // 
            // tpConexao
            // 
            this.tpConexao.Controls.Add(this.chkTrustedConnection);
            this.tpConexao.Controls.Add(this.bntTestarConexao);
            this.tpConexao.Controls.Add(this.bntAvancar);
            this.tpConexao.Controls.Add(this.txtPorta);
            this.tpConexao.Controls.Add(this.label6);
            this.tpConexao.Controls.Add(this.pbLogo);
            this.tpConexao.Controls.Add(this.txtSenha);
            this.tpConexao.Controls.Add(this.label3);
            this.tpConexao.Controls.Add(this.txtUsuario);
            this.tpConexao.Controls.Add(this.label2);
            this.tpConexao.Controls.Add(this.txtServidor);
            this.tpConexao.Controls.Add(this.label1);
            this.tpConexao.ImageIndex = 0;
            this.tpConexao.Location = new System.Drawing.Point(4, 23);
            this.tpConexao.Name = "tpConexao";
            this.tpConexao.Padding = new System.Windows.Forms.Padding(3);
            this.tpConexao.Size = new System.Drawing.Size(638, 259);
            this.tpConexao.TabIndex = 0;
            this.tpConexao.Text = "Conexão";
            this.tpConexao.UseVisualStyleBackColor = true;
            // 
            // chkTrustedConnection
            // 
            this.chkTrustedConnection.AutoSize = true;
            this.chkTrustedConnection.Location = new System.Drawing.Point(194, 119);
            this.chkTrustedConnection.Name = "chkTrustedConnection";
            this.chkTrustedConnection.Size = new System.Drawing.Size(119, 17);
            this.chkTrustedConnection.TabIndex = 4;
            this.chkTrustedConnection.Text = "Trusted Connection";
            this.chkTrustedConnection.UseVisualStyleBackColor = true;
            this.chkTrustedConnection.CheckedChanged += new System.EventHandler(this.chkTrustedConnection_CheckedChanged);
            // 
            // bntTestarConexao
            // 
            this.bntTestarConexao.Image = ((System.Drawing.Image)(resources.GetObject("bntTestarConexao.Image")));
            this.bntTestarConexao.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bntTestarConexao.Location = new System.Drawing.Point(10, 167);
            this.bntTestarConexao.Name = "bntTestarConexao";
            this.bntTestarConexao.Size = new System.Drawing.Size(122, 32);
            this.bntTestarConexao.TabIndex = 5;
            this.bntTestarConexao.Text = "Testar Conexão";
            this.bntTestarConexao.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bntTestarConexao.Click += new System.EventHandler(this.bntTestarConexao_Click);
            // 
            // bntAvancar
            // 
            this.bntAvancar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bntAvancar.Location = new System.Drawing.Point(10, 205);
            this.bntAvancar.Name = "bntAvancar";
            this.bntAvancar.Size = new System.Drawing.Size(122, 30);
            this.bntAvancar.TabIndex = 6;
            this.bntAvancar.Text = "Avançar";
            this.bntAvancar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bntAvancar.Click += new System.EventHandler(this.bntAvancar_Click);
            // 
            // txtPorta
            // 
            this.txtPorta.Location = new System.Drawing.Point(10, 117);
            this.txtPorta.MaxLength = 5;
            this.txtPorta.Name = "txtPorta";
            this.txtPorta.Size = new System.Drawing.Size(80, 20);
            this.txtPorta.TabIndex = 3;
            this.txtPorta.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPorta_KeyPress);
            this.txtPorta.Validating += new System.ComponentModel.CancelEventHandler(this.txtPorta_Validating);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Porta:";
            // 
            // pbLogo
            // 
            this.pbLogo.Location = new System.Drawing.Point(432, 35);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(200, 200);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 6;
            this.pbLogo.TabStop = false;
            // 
            // txtSenha
            // 
            this.txtSenha.Location = new System.Drawing.Point(194, 79);
            this.txtSenha.MaxLength = 255;
            this.txtSenha.Name = "txtSenha";
            this.txtSenha.PasswordChar = '*';
            this.txtSenha.Size = new System.Drawing.Size(160, 20);
            this.txtSenha.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(191, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Senha:";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(10, 79);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(160, 20);
            this.txtUsuario.TabIndex = 1;
            this.txtUsuario.Leave += new System.EventHandler(this.txtUsuario_Leave);
            this.txtUsuario.Validating += new System.ComponentModel.CancelEventHandler(this.txtUsuario_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Usuario:";
            // 
            // txtServidor
            // 
            this.txtServidor.Location = new System.Drawing.Point(10, 35);
            this.txtServidor.Name = "txtServidor";
            this.txtServidor.Size = new System.Drawing.Size(344, 20);
            this.txtServidor.TabIndex = 0;
            this.txtServidor.Leave += new System.EventHandler(this.txtServidor_Leave);
            this.txtServidor.Validating += new System.ComponentModel.CancelEventHandler(this.txtServidor_Validating);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Servidor:";
            // 
            // tpBancoDados
            // 
            this.tpBancoDados.Controls.Add(this.bntMapConsulta);
            this.tpBancoDados.Controls.Add(this.label8);
            this.tpBancoDados.Controls.Add(this.txtBuscar);
            this.tpBancoDados.Controls.Add(this.pbAcao);
            this.tpBancoDados.Controls.Add(this.bntRefreshDatabase);
            this.tpBancoDados.Controls.Add(this.cbBancoDados);
            this.tpBancoDados.Controls.Add(this.lvObjetosBanco);
            this.tpBancoDados.Controls.Add(this.btnMapearSelecionados);
            this.tpBancoDados.Controls.Add(this.chSelecionarTodos);
            this.tpBancoDados.Controls.Add(this.label4);
            this.tpBancoDados.ImageIndex = 1;
            this.tpBancoDados.Location = new System.Drawing.Point(4, 23);
            this.tpBancoDados.Name = "tpBancoDados";
            this.tpBancoDados.Padding = new System.Windows.Forms.Padding(3);
            this.tpBancoDados.Size = new System.Drawing.Size(638, 259);
            this.tpBancoDados.TabIndex = 1;
            this.tpBancoDados.Text = "Bancos de Dados";
            this.tpBancoDados.UseVisualStyleBackColor = true;
            // 
            // bntMapConsulta
            // 
            this.bntMapConsulta.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bntMapConsulta.Location = new System.Drawing.Point(396, 208);
            this.bntMapConsulta.Name = "bntMapConsulta";
            this.bntMapConsulta.Size = new System.Drawing.Size(120, 40);
            this.bntMapConsulta.TabIndex = 4;
            this.bntMapConsulta.Text = "Mapear \r\nSQL";
            this.bntMapConsulta.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bntMapConsulta.UseVisualStyleBackColor = true;
            this.bntMapConsulta.Click += new System.EventHandler(this.bntMapConsulta_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(369, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Pesquisar:";
            // 
            // txtBuscar
            // 
            this.txtBuscar.Location = new System.Drawing.Point(372, 34);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(260, 20);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // 
            // pbAcao
            // 
            this.pbAcao.Location = new System.Drawing.Point(10, 231);
            this.pbAcao.Name = "pbAcao";
            this.pbAcao.Size = new System.Drawing.Size(336, 17);
            this.pbAcao.TabIndex = 5;
            this.pbAcao.Visible = false;
            // 
            // bntRefreshDatabase
            // 
            this.bntRefreshDatabase.Location = new System.Drawing.Point(258, 34);
            this.bntRefreshDatabase.Name = "bntRefreshDatabase";
            this.bntRefreshDatabase.Size = new System.Drawing.Size(25, 23);
            this.bntRefreshDatabase.TabIndex = 1;
            this.bntRefreshDatabase.Click += new System.EventHandler(this.bntRefreshDatabase_Click);
            // 
            // cbBancoDados
            // 
            this.cbBancoDados.Location = new System.Drawing.Point(10, 35);
            this.cbBancoDados.Name = "cbBancoDados";
            this.cbBancoDados.Size = new System.Drawing.Size(249, 21);
            this.cbBancoDados.TabIndex = 0;
            // 
            // lvObjetosBanco
            // 
            this.lvObjetosBanco.FullRowSelect = true;
            listViewGroup1.Header = "Tabela";
            listViewGroup1.Name = "tabela";
            listViewGroup2.Header = "View";
            listViewGroup2.Name = "view";
            listViewGroup3.Header = "Procedure";
            listViewGroup3.Name = "procedure";
            this.lvObjetosBanco.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.lvObjetosBanco.Location = new System.Drawing.Point(10, 61);
            this.lvObjetosBanco.Name = "lvObjetosBanco";
            this.lvObjetosBanco.OwnerDraw = true;
            this.lvObjetosBanco.Size = new System.Drawing.Size(622, 141);
            this.lvObjetosBanco.SmallImageList = this.ilIcones;
            this.lvObjetosBanco.TabIndex = 2;
            this.lvObjetosBanco.UseCompatibleStateImageBehavior = false;
            this.lvObjetosBanco.View = System.Windows.Forms.View.Details;
            this.lvObjetosBanco.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvObjetosBanco_ColumnClick);
            this.lvObjetosBanco.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.lvObjetosBanco_DrawColumnHeader);
            this.lvObjetosBanco.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.lvObjetosBanco_DrawSubItem);
            this.lvObjetosBanco.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvObjetosBanco_ItemCheck);
            this.lvObjetosBanco.SelectedIndexChanged += new System.EventHandler(this.lvObjetosBanco_SelectedIndexChanged);
            this.lvObjetosBanco.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvObjetosBanco_MouseClick);
            // 
            // ilIcones
            // 
            this.ilIcones.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilIcones.ImageStream")));
            this.ilIcones.TransparentColor = System.Drawing.Color.Transparent;
            this.ilIcones.Images.SetKeyName(0, "tabela");
            this.ilIcones.Images.SetKeyName(1, "view");
            this.ilIcones.Images.SetKeyName(2, "procedure");
            this.ilIcones.Images.SetKeyName(3, "csharpfile");
            this.ilIcones.Images.SetKeyName(4, "uncheked");
            this.ilIcones.Images.SetKeyName(5, "ident.png");
            this.ilIcones.Images.SetKeyName(6, "pk.png");
            this.ilIcones.Images.SetKeyName(7, "cheked");
            this.ilIcones.Images.SetKeyName(8, "query");
            // 
            // btnMapearSelecionados
            // 
            this.btnMapearSelecionados.Image = ((System.Drawing.Image)(resources.GetObject("btnMapearSelecionados.Image")));
            this.btnMapearSelecionados.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMapearSelecionados.Location = new System.Drawing.Point(522, 208);
            this.btnMapearSelecionados.Name = "btnMapearSelecionados";
            this.btnMapearSelecionados.Size = new System.Drawing.Size(110, 40);
            this.btnMapearSelecionados.TabIndex = 5;
            this.btnMapearSelecionados.Text = "Mapear \r\nSelecionados";
            this.btnMapearSelecionados.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMapearSelecionados.Click += new System.EventHandler(this.btnMapearSelecionados_Click);
            // 
            // chSelecionarTodos
            // 
            this.chSelecionarTodos.Location = new System.Drawing.Point(10, 208);
            this.chSelecionarTodos.Name = "chSelecionarTodos";
            this.chSelecionarTodos.Size = new System.Drawing.Size(206, 19);
            this.chSelecionarTodos.TabIndex = 3;
            this.chSelecionarTodos.Text = "Selecionar Todos (menos procedures)";
            this.chSelecionarTodos.CheckedChanged += new System.EventHandler(this.chSelecionarTodos_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Bancos de Dados:";
            // 
            // tpMapeamento
            // 
            this.tpMapeamento.Controls.Add(this.simpleButton1);
            this.tpMapeamento.Controls.Add(this.groupBox2);
            this.tpMapeamento.Controls.Add(this.btnMapear);
            this.tpMapeamento.Controls.Add(this.lvObjetosSelecionados);
            this.tpMapeamento.Controls.Add(this.txtNameSpace);
            this.tpMapeamento.Controls.Add(this.label7);
            this.tpMapeamento.Controls.Add(this.label5);
            this.tpMapeamento.ImageIndex = 3;
            this.tpMapeamento.Location = new System.Drawing.Point(4, 23);
            this.tpMapeamento.Name = "tpMapeamento";
            this.tpMapeamento.Padding = new System.Windows.Forms.Padding(3);
            this.tpMapeamento.Size = new System.Drawing.Size(638, 259);
            this.tpMapeamento.TabIndex = 3;
            this.tpMapeamento.Text = "Mapeamento";
            this.tpMapeamento.UseVisualStyleBackColor = true;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.Image")));
            this.simpleButton1.Location = new System.Drawing.Point(595, 7);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(38, 38);
            this.simpleButton1.TabIndex = 4;
            this.simpleButton1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkMapWcf);
            this.groupBox2.Controls.Add(this.chkMapLinq);
            this.groupBox2.Controls.Add(this.chkGeraSN);
            this.groupBox2.Controls.Add(this.chkGeraAppConfig);
            this.groupBox2.Location = new System.Drawing.Point(197, 26);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(274, 161);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Opções";
            // 
            // chkMapWcf
            // 
            this.chkMapWcf.AutoSize = true;
            this.chkMapWcf.Location = new System.Drawing.Point(10, 88);
            this.chkMapWcf.Name = "chkMapWcf";
            this.chkMapWcf.Size = new System.Drawing.Size(115, 17);
            this.chkMapWcf.TabIndex = 5;
            this.chkMapWcf.Text = "Propriedades WCF";
            this.chkMapWcf.UseVisualStyleBackColor = true;
            this.chkMapWcf.CheckedChanged += new System.EventHandler(this.chkOpcao_CheckedChanged);
            // 
            // chkMapLinq
            // 
            this.chkMapLinq.AutoSize = true;
            this.chkMapLinq.Checked = true;
            this.chkMapLinq.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMapLinq.Location = new System.Drawing.Point(10, 66);
            this.chkMapLinq.Name = "chkMapLinq";
            this.chkMapLinq.Size = new System.Drawing.Size(153, 17);
            this.chkMapLinq.TabIndex = 4;
            this.chkMapLinq.Text = "Propriedades Mapeamento";
            this.chkMapLinq.UseVisualStyleBackColor = true;
            this.chkMapLinq.CheckedChanged += new System.EventHandler(this.chkOpcao_CheckedChanged);
            // 
            // chkGeraSN
            // 
            this.chkGeraSN.Checked = true;
            this.chkGeraSN.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGeraSN.Location = new System.Drawing.Point(10, 44);
            this.chkGeraSN.Name = "chkGeraSN";
            this.chkGeraSN.Size = new System.Drawing.Size(202, 19);
            this.chkGeraSN.TabIndex = 2;
            this.chkGeraSN.Text = "Gerar Strong Name e assinar projeto";
            this.chkGeraSN.CheckedChanged += new System.EventHandler(this.chkOpcao_CheckedChanged);
            // 
            // chkGeraAppConfig
            // 
            this.chkGeraAppConfig.Checked = true;
            this.chkGeraAppConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGeraAppConfig.Location = new System.Drawing.Point(10, 21);
            this.chkGeraAppConfig.Name = "chkGeraAppConfig";
            this.chkGeraAppConfig.Size = new System.Drawing.Size(108, 19);
            this.chkGeraAppConfig.TabIndex = 1;
            this.chkGeraAppConfig.Text = "Gerar App.config";
            this.chkGeraAppConfig.CheckedChanged += new System.EventHandler(this.chkOpcao_CheckedChanged);
            // 
            // btnMapear
            // 
            this.btnMapear.Image = ((System.Drawing.Image)(resources.GetObject("btnMapear.Image")));
            this.btnMapear.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMapear.Location = new System.Drawing.Point(477, 208);
            this.btnMapear.Name = "btnMapear";
            this.btnMapear.Size = new System.Drawing.Size(155, 34);
            this.btnMapear.TabIndex = 3;
            this.btnMapear.Text = "Mapear";
            this.btnMapear.Click += new System.EventHandler(this.btnMapear_Click);
            // 
            // lvObjetosSelecionados
            // 
            listViewGroup4.Header = "Tabela";
            listViewGroup4.Name = "Tabela";
            listViewGroup5.Header = "View";
            listViewGroup5.Name = "View";
            listViewGroup6.Header = "Procedure";
            listViewGroup6.Name = "Procedure";
            listViewGroup7.Header = "Query";
            listViewGroup7.Name = "Query";
            this.lvObjetosSelecionados.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup4,
            listViewGroup5,
            listViewGroup6,
            listViewGroup7});
            this.lvObjetosSelecionados.Location = new System.Drawing.Point(10, 26);
            this.lvObjetosSelecionados.Name = "lvObjetosSelecionados";
            this.lvObjetosSelecionados.Size = new System.Drawing.Size(181, 161);
            this.lvObjetosSelecionados.SmallImageList = this.ilIcones;
            this.lvObjetosSelecionados.TabIndex = 0;
            this.lvObjetosSelecionados.UseCompatibleStateImageBehavior = false;
            this.lvObjetosSelecionados.View = System.Windows.Forms.View.SmallIcon;
            // 
            // txtNameSpace
            // 
            this.txtNameSpace.Location = new System.Drawing.Point(10, 206);
            this.txtNameSpace.Name = "txtNameSpace";
            this.txtNameSpace.Size = new System.Drawing.Size(181, 20);
            this.txtNameSpace.TabIndex = 1;
            this.txtNameSpace.Validating += new System.ComponentModel.CancelEventHandler(this.txtNameSpace_Validating);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 190);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Namespace:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Objetos Selecionados";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "banco");
            this.imageList1.Images.SetKeyName(1, "tabela");
            this.imageList1.Images.SetKeyName(2, "objeto");
            this.imageList1.Images.SetKeyName(3, "gerar");
            this.imageList1.Images.SetKeyName(4, "opcoes");
            // 
            // errPadrao
            // 
            this.errPadrao.ContainerControl = this;
            // 
            // lblVersao
            // 
            this.lblVersao.AutoSize = true;
            this.lblVersao.Enabled = false;
            this.lblVersao.Location = new System.Drawing.Point(535, 301);
            this.lblVersao.Name = "lblVersao";
            this.lblVersao.Size = new System.Drawing.Size(79, 13);
            this.lblVersao.TabIndex = 1;
            this.lblVersao.Text = "Versão: 1.0.0.0";
            // 
            // ilObjetos
            // 
            this.ilObjetos.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilObjetos.ImageStream")));
            this.ilObjetos.TransparentColor = System.Drawing.Color.Transparent;
            this.ilObjetos.Images.SetKeyName(0, "pk");
            this.ilObjetos.Images.SetKeyName(1, "ident");
            this.ilObjetos.Images.SetKeyName(2, "vazio");
            // 
            // bwDadosBanco
            // 
            this.bwDadosBanco.WorkerReportsProgress = true;
            this.bwDadosBanco.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwDadosBanco_DoWork);
            this.bwDadosBanco.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwDadosBanco_ProgressChanged);
            this.bwDadosBanco.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwDadosBanco_RunWorkerCompleted);
            // 
            // frmGerador
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 318);
            this.Controls.Add(this.lblVersao);
            this.Controls.Add(this.tbPrincipal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmGerador";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Data Drain ORM";
            this.Load += new System.EventHandler(this.frmGerador_Load);
            this.tbPrincipal.ResumeLayout(false);
            this.tpConexao.ResumeLayout(false);
            this.tpConexao.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.tpBancoDados.ResumeLayout(false);
            this.tpBancoDados.PerformLayout();
            this.tpMapeamento.ResumeLayout(false);
            this.tpMapeamento.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errPadrao)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tbPrincipal;
        private System.Windows.Forms.TabPage tpConexao;
        private TextBox txtSenha;
        private Label label3;
        private System.Windows.Forms.TextBox txtUsuario;
        private Label label2;
        private System.Windows.Forms.TextBox txtServidor;
        private Label label1;
        private System.Windows.Forms.TabPage tpBancoDados;
        private System.Windows.Forms.TabPage tpMapeamento;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ErrorProvider errPadrao;
        private Label label4;
        private Button btnMapearSelecionados;
        private CheckBox chSelecionarTodos;
        private TextBox txtNameSpace;
        private Label label7;
        private Label label5;
        private System.Windows.Forms.ImageList ilIcones;
        private System.Windows.Forms.ListView lvObjetosSelecionados;
        private System.Windows.Forms.ListView lvObjetosBanco;
        private Button btnMapear;
        private TextBox txtPorta;
        private Label label6;
        private Label lblVersao;
        private System.Windows.Forms.ImageList ilObjetos;
        public System.Windows.Forms.PictureBox pbLogo;
        private Button bntAvancar;
        private Button bntTestarConexao;
        private ComboBox cbBancoDados;
        private Button bntRefreshDatabase;
        private CheckBox chkTrustedConnection;
        private ToolTip toolTip1;
        private GroupBox groupBox2;
        private CheckBox chkMapWcf;
        private CheckBox chkMapLinq;
        private CheckBox chkGeraSN;
        private CheckBox chkGeraAppConfig;
        private System.ComponentModel.BackgroundWorker bwDadosBanco;
        private ProgressBar pbAcao;
        private Label label8;
        private TextBox txtBuscar;
        private Button simpleButton1;
        private Button bntMapConsulta;
    }
}