namespace DataDrain.ORM.Generator.Formularios
{
    partial class frmLog4Net
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLog4Net));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbBancoDados = new System.Windows.Forms.RadioButton();
            this.rbEmail = new System.Windows.Forms.RadioButton();
            this.rbArquivo = new System.Windows.Forms.RadioButton();
            this.tabCtrlLog4Net = new System.Windows.Forms.TabControl();
            this.tabArquivo = new System.Windows.Forms.TabPage();
            this.txtTamanhoBackup = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtQtdBackup = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbTipoTrocaArquivo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkAppendFile = new System.Windows.Forms.CheckBox();
            this.cbLockFile = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNomeArquivo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbArquivoXML = new System.Windows.Forms.RadioButton();
            this.rbArquivoTXT = new System.Windows.Forms.RadioButton();
            this.tabEmail = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtPorta = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkSSL = new System.Windows.Forms.CheckBox();
            this.txtSenha = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtAssunto = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtEmailPara = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtEmailDe = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabBanco = new System.Windows.Forms.TabPage();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.cbBanco = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.errPadrao = new System.Windows.Forms.ErrorProvider(this.components);
            this.bntConfirmar = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.cbLevel = new System.Windows.Forms.ComboBox();
            this.bntAjuda = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tabCtrlLog4Net.SuspendLayout();
            this.tabArquivo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabEmail.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabBanco.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errPadrao)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbBancoDados);
            this.groupBox1.Controls.Add(this.rbEmail);
            this.groupBox1.Controls.Add(this.rbArquivo);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(253, 49);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tipo de Armazenamento";
            // 
            // rbBancoDados
            // 
            this.rbBancoDados.AutoSize = true;
            this.rbBancoDados.Location = new System.Drawing.Point(130, 20);
            this.rbBancoDados.Name = "rbBancoDados";
            this.rbBancoDados.Size = new System.Drawing.Size(105, 17);
            this.rbBancoDados.TabIndex = 2;
            this.rbBancoDados.Text = "Banco de Dados";
            this.rbBancoDados.UseVisualStyleBackColor = true;
            this.rbBancoDados.CheckedChanged += new System.EventHandler(this.rbBancoDados_CheckedChanged);
            // 
            // rbEmail
            // 
            this.rbEmail.AutoSize = true;
            this.rbEmail.Location = new System.Drawing.Point(74, 20);
            this.rbEmail.Name = "rbEmail";
            this.rbEmail.Size = new System.Drawing.Size(50, 17);
            this.rbEmail.TabIndex = 1;
            this.rbEmail.Text = "Email";
            this.rbEmail.UseVisualStyleBackColor = true;
            this.rbEmail.CheckedChanged += new System.EventHandler(this.rbEmail_CheckedChanged);
            // 
            // rbArquivo
            // 
            this.rbArquivo.AutoSize = true;
            this.rbArquivo.Checked = true;
            this.rbArquivo.Location = new System.Drawing.Point(7, 20);
            this.rbArquivo.Name = "rbArquivo";
            this.rbArquivo.Size = new System.Drawing.Size(61, 17);
            this.rbArquivo.TabIndex = 0;
            this.rbArquivo.TabStop = true;
            this.rbArquivo.Text = "Arquivo";
            this.rbArquivo.UseVisualStyleBackColor = true;
            this.rbArquivo.CheckedChanged += new System.EventHandler(this.rbArquivo_CheckedChanged);
            // 
            // tabCtrlLog4Net
            // 
            this.tabCtrlLog4Net.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabCtrlLog4Net.Controls.Add(this.tabArquivo);
            this.tabCtrlLog4Net.Controls.Add(this.tabEmail);
            this.tabCtrlLog4Net.Controls.Add(this.tabBanco);
            this.tabCtrlLog4Net.ItemSize = new System.Drawing.Size(0, 21);
            this.tabCtrlLog4Net.Location = new System.Drawing.Point(13, 69);
            this.tabCtrlLog4Net.Name = "tabCtrlLog4Net";
            this.tabCtrlLog4Net.SelectedIndex = 0;
            this.tabCtrlLog4Net.Size = new System.Drawing.Size(518, 257);
            this.tabCtrlLog4Net.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabCtrlLog4Net.TabIndex = 2;
            // 
            // tabArquivo
            // 
            this.tabArquivo.Controls.Add(this.txtTamanhoBackup);
            this.tabArquivo.Controls.Add(this.label5);
            this.tabArquivo.Controls.Add(this.txtQtdBackup);
            this.tabArquivo.Controls.Add(this.label4);
            this.tabArquivo.Controls.Add(this.cbTipoTrocaArquivo);
            this.tabArquivo.Controls.Add(this.label3);
            this.tabArquivo.Controls.Add(this.chkAppendFile);
            this.tabArquivo.Controls.Add(this.cbLockFile);
            this.tabArquivo.Controls.Add(this.label2);
            this.tabArquivo.Controls.Add(this.txtNomeArquivo);
            this.tabArquivo.Controls.Add(this.label1);
            this.tabArquivo.Controls.Add(this.panel1);
            this.tabArquivo.Location = new System.Drawing.Point(4, 25);
            this.tabArquivo.Name = "tabArquivo";
            this.tabArquivo.Padding = new System.Windows.Forms.Padding(3);
            this.tabArquivo.Size = new System.Drawing.Size(510, 228);
            this.tabArquivo.TabIndex = 0;
            this.tabArquivo.Text = "tabPage1";
            this.tabArquivo.UseVisualStyleBackColor = true;
            // 
            // txtTamanhoBackup
            // 
            this.txtTamanhoBackup.Location = new System.Drawing.Point(289, 112);
            this.txtTamanhoBackup.MaxLength = 3;
            this.txtTamanhoBackup.Name = "txtTamanhoBackup";
            this.txtTamanhoBackup.Size = new System.Drawing.Size(69, 20);
            this.txtTamanhoBackup.TabIndex = 6;
            this.txtTamanhoBackup.Text = "5";
            this.txtTamanhoBackup.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQtdBackup_KeyPress);
            this.txtTamanhoBackup.Validating += new System.ComponentModel.CancelEventHandler(this.txtTamanhoBackup_Validating);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(286, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Maximum File Size (MB):";
            // 
            // txtQtdBackup
            // 
            this.txtQtdBackup.Location = new System.Drawing.Point(289, 67);
            this.txtQtdBackup.MaxLength = 3;
            this.txtQtdBackup.Name = "txtQtdBackup";
            this.txtQtdBackup.Size = new System.Drawing.Size(69, 20);
            this.txtQtdBackup.TabIndex = 5;
            this.txtQtdBackup.Text = "2";
            this.txtQtdBackup.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQtdBackup_KeyPress);
            this.txtQtdBackup.Validating += new System.ComponentModel.CancelEventHandler(this.txtQtdBackup_Validating);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(286, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Max Size Roll Backups :";
            // 
            // cbTipoTrocaArquivo
            // 
            this.cbTipoTrocaArquivo.FormattingEnabled = true;
            this.cbTipoTrocaArquivo.Items.AddRange(new object[] {
            "Once",
            "Size",
            "Date",
            "Composite"});
            this.cbTipoTrocaArquivo.Location = new System.Drawing.Point(11, 180);
            this.cbTipoTrocaArquivo.Name = "cbTipoTrocaArquivo";
            this.cbTipoTrocaArquivo.Size = new System.Drawing.Size(194, 21);
            this.cbTipoTrocaArquivo.TabIndex = 4;
            this.cbTipoTrocaArquivo.Text = "Size";
            this.cbTipoTrocaArquivo.Validating += new System.ComponentModel.CancelEventHandler(this.cbTipoTrocaArquivo_Validating);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 163);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Rolling Style :";
            // 
            // chkAppendFile
            // 
            this.chkAppendFile.AutoSize = true;
            this.chkAppendFile.Checked = true;
            this.chkAppendFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAppendFile.Location = new System.Drawing.Point(11, 139);
            this.chkAppendFile.Name = "chkAppendFile";
            this.chkAppendFile.Size = new System.Drawing.Size(101, 17);
            this.chkAppendFile.TabIndex = 3;
            this.chkAppendFile.Text = "Append To File ";
            this.chkAppendFile.UseVisualStyleBackColor = true;
            // 
            // cbLockFile
            // 
            this.cbLockFile.FormattingEnabled = true;
            this.cbLockFile.Items.AddRange(new object[] {
            "ExclusiveLock",
            "InterProcessLock",
            "MinimalLock"});
            this.cbLockFile.Location = new System.Drawing.Point(10, 111);
            this.cbLockFile.Name = "cbLockFile";
            this.cbLockFile.Size = new System.Drawing.Size(194, 21);
            this.cbLockFile.TabIndex = 2;
            this.cbLockFile.Text = "MinimalLock";
            this.cbLockFile.Validating += new System.ComponentModel.CancelEventHandler(this.cbLockFile_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Tipo de Lock:";
            // 
            // txtNomeArquivo
            // 
            this.txtNomeArquivo.Location = new System.Drawing.Point(7, 67);
            this.txtNomeArquivo.MaxLength = 100;
            this.txtNomeArquivo.Name = "txtNomeArquivo";
            this.txtNomeArquivo.Size = new System.Drawing.Size(242, 20);
            this.txtNomeArquivo.TabIndex = 1;
            this.txtNomeArquivo.Validating += new System.ComponentModel.CancelEventHandler(this.txtNomeArquivo_Validating);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nome do Arquivo:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbArquivoXML);
            this.panel1.Controls.Add(this.rbArquivoTXT);
            this.panel1.Location = new System.Drawing.Point(7, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(197, 31);
            this.panel1.TabIndex = 0;
            // 
            // rbArquivoXML
            // 
            this.rbArquivoXML.AutoSize = true;
            this.rbArquivoXML.Checked = true;
            this.rbArquivoXML.Location = new System.Drawing.Point(98, 4);
            this.rbArquivoXML.Name = "rbArquivoXML";
            this.rbArquivoXML.Size = new System.Drawing.Size(89, 17);
            this.rbArquivoXML.TabIndex = 1;
            this.rbArquivoXML.TabStop = true;
            this.rbArquivoXML.Text = "Arquivo .XML";
            this.rbArquivoXML.UseVisualStyleBackColor = true;
            // 
            // rbArquivoTXT
            // 
            this.rbArquivoTXT.AutoSize = true;
            this.rbArquivoTXT.Location = new System.Drawing.Point(4, 4);
            this.rbArquivoTXT.Name = "rbArquivoTXT";
            this.rbArquivoTXT.Size = new System.Drawing.Size(88, 17);
            this.rbArquivoTXT.TabIndex = 0;
            this.rbArquivoTXT.Text = "Arquivo .TXT";
            this.rbArquivoTXT.UseVisualStyleBackColor = true;
            // 
            // tabEmail
            // 
            this.tabEmail.Controls.Add(this.textBox1);
            this.tabEmail.Controls.Add(this.groupBox2);
            this.tabEmail.Controls.Add(this.txtAssunto);
            this.tabEmail.Controls.Add(this.label8);
            this.tabEmail.Controls.Add(this.txtEmailPara);
            this.tabEmail.Controls.Add(this.label7);
            this.tabEmail.Controls.Add(this.txtEmailDe);
            this.tabEmail.Controls.Add(this.label6);
            this.tabEmail.Location = new System.Drawing.Point(4, 25);
            this.tabEmail.Name = "tabEmail";
            this.tabEmail.Padding = new System.Windows.Forms.Padding(3);
            this.tabEmail.Size = new System.Drawing.Size(510, 228);
            this.tabEmail.TabIndex = 1;
            this.tabEmail.Text = "tabPage2";
            this.tabEmail.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.Red;
            this.textBox1.Location = new System.Drawing.Point(348, 96);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(156, 129);
            this.textBox1.TabIndex = 7;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtPorta);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.chkSSL);
            this.groupBox2.Controls.Add(this.txtSenha);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.txtUsuario);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.txtHost);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Location = new System.Drawing.Point(10, 90);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(332, 135);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SMTP Server";
            // 
            // txtPorta
            // 
            this.txtPorta.Location = new System.Drawing.Point(178, 109);
            this.txtPorta.MaxLength = 3;
            this.txtPorta.Name = "txtPorta";
            this.txtPorta.Size = new System.Drawing.Size(69, 20);
            this.txtPorta.TabIndex = 4;
            this.txtPorta.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQtdBackup_KeyPress);
            this.txtPorta.Validating += new System.ComponentModel.CancelEventHandler(this.txtPorta_Validating);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(178, 94);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "Porta:";
            // 
            // chkSSL
            // 
            this.chkSSL.AutoSize = true;
            this.chkSSL.Location = new System.Drawing.Point(8, 111);
            this.chkSSL.Name = "chkSSL";
            this.chkSSL.Size = new System.Drawing.Size(46, 17);
            this.chkSSL.TabIndex = 3;
            this.chkSSL.Text = "SSL";
            this.chkSSL.UseVisualStyleBackColor = true;
            // 
            // txtSenha
            // 
            this.txtSenha.Location = new System.Drawing.Point(178, 69);
            this.txtSenha.MaxLength = 100;
            this.txtSenha.Name = "txtSenha";
            this.txtSenha.Size = new System.Drawing.Size(140, 20);
            this.txtSenha.TabIndex = 2;
            this.txtSenha.Validating += new System.ComponentModel.CancelEventHandler(this.txtSenha_Validating);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(175, 54);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(41, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "Senha:";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(8, 69);
            this.txtUsuario.MaxLength = 100;
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(140, 20);
            this.txtUsuario.TabIndex = 1;
            this.txtUsuario.Validating += new System.ComponentModel.CancelEventHandler(this.txtUsuario_Validating);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(5, 54);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(46, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Usuário:";
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(6, 31);
            this.txtHost.MaxLength = 255;
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(312, 20);
            this.txtHost.TabIndex = 0;
            this.txtHost.Validating += new System.ComponentModel.CancelEventHandler(this.txtHost_Validating);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Host:";
            // 
            // txtAssunto
            // 
            this.txtAssunto.Location = new System.Drawing.Point(10, 64);
            this.txtAssunto.MaxLength = 255;
            this.txtAssunto.Name = "txtAssunto";
            this.txtAssunto.Size = new System.Drawing.Size(494, 20);
            this.txtAssunto.TabIndex = 2;
            this.txtAssunto.Validating += new System.ComponentModel.CancelEventHandler(this.txtAssunto_Validating);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Assunto:";
            // 
            // txtEmailPara
            // 
            this.txtEmailPara.Location = new System.Drawing.Point(265, 24);
            this.txtEmailPara.MaxLength = 255;
            this.txtEmailPara.Name = "txtEmailPara";
            this.txtEmailPara.Size = new System.Drawing.Size(239, 20);
            this.txtEmailPara.TabIndex = 1;
            this.txtEmailPara.Validating += new System.ComponentModel.CancelEventHandler(this.txtEmailDe_Validating);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(262, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Para:";
            // 
            // txtEmailDe
            // 
            this.txtEmailDe.Location = new System.Drawing.Point(10, 24);
            this.txtEmailDe.MaxLength = 255;
            this.txtEmailDe.Name = "txtEmailDe";
            this.txtEmailDe.Size = new System.Drawing.Size(239, 20);
            this.txtEmailDe.TabIndex = 0;
            this.txtEmailDe.Validating += new System.ComponentModel.CancelEventHandler(this.txtEmailDe_Validating);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "De:";
            // 
            // tabBanco
            // 
            this.tabBanco.Controls.Add(this.textBox8);
            this.tabBanco.Controls.Add(this.label14);
            this.tabBanco.Controls.Add(this.cbBanco);
            this.tabBanco.Controls.Add(this.label13);
            this.tabBanco.Location = new System.Drawing.Point(4, 25);
            this.tabBanco.Name = "tabBanco";
            this.tabBanco.Padding = new System.Windows.Forms.Padding(3);
            this.tabBanco.Size = new System.Drawing.Size(510, 228);
            this.tabBanco.TabIndex = 2;
            this.tabBanco.Text = "tabPage3";
            this.tabBanco.UseVisualStyleBackColor = true;
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(10, 69);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.ReadOnly = true;
            this.textBox8.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox8.Size = new System.Drawing.Size(494, 156);
            this.textBox8.TabIndex = 3;
            this.textBox8.Text = resources.GetString("textBox8.Text");
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 52);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(200, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "Rodar esse script no banco selecionado:";
            // 
            // cbBanco
            // 
            this.cbBanco.FormattingEnabled = true;
            this.cbBanco.Location = new System.Drawing.Point(10, 24);
            this.cbBanco.Name = "cbBanco";
            this.cbBanco.Size = new System.Drawing.Size(239, 21);
            this.cbBanco.TabIndex = 1;
            this.cbBanco.Validating += new System.ComponentModel.CancelEventHandler(this.cbBanco_Validating);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 7);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(90, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "Banco de Dados:";
            // 
            // errPadrao
            // 
            this.errPadrao.ContainerControl = this;
            // 
            // bntConfirmar
            // 
            this.bntConfirmar.Location = new System.Drawing.Point(434, 332);
            this.bntConfirmar.Name = "bntConfirmar";
            this.bntConfirmar.Size = new System.Drawing.Size(93, 37);
            this.bntConfirmar.TabIndex = 3;
            this.bntConfirmar.Text = "Confirmar";
            this.bntConfirmar.UseVisualStyleBackColor = true;
            this.bntConfirmar.Click += new System.EventHandler(this.bntConfirmar_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(288, 23);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(89, 13);
            this.label15.TabIndex = 3;
            this.label15.Text = "Nivel de Captura:";
            // 
            // cbLevel
            // 
            this.cbLevel.FormattingEnabled = true;
            this.cbLevel.Items.AddRange(new object[] {
            "Off",
            "Fatal",
            "Error",
            "Warn",
            "Info",
            "Debug",
            "All"});
            this.cbLevel.Location = new System.Drawing.Point(291, 40);
            this.cbLevel.Name = "cbLevel";
            this.cbLevel.Size = new System.Drawing.Size(147, 21);
            this.cbLevel.TabIndex = 1;
            this.cbLevel.Text = "All";
            // 
            // bntAjuda
            // 
            this.bntAjuda.Image = global::DataDrain.ORM.Generator.Properties.Resources._1352224904_Help_book_3d;
            this.bntAjuda.Location = new System.Drawing.Point(13, 337);
            this.bntAjuda.Name = "bntAjuda";
            this.bntAjuda.Size = new System.Drawing.Size(32, 32);
            this.bntAjuda.TabIndex = 4;
            this.bntAjuda.Click += new System.EventHandler(this.bntAjuda_Click);
            // 
            // frmLog4Net
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 381);
            this.Controls.Add(this.bntAjuda);
            this.Controls.Add(this.cbLevel);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.bntConfirmar);
            this.Controls.Add(this.tabCtrlLog4Net);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLog4Net";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configurações Log4Net";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabCtrlLog4Net.ResumeLayout(false);
            this.tabArquivo.ResumeLayout(false);
            this.tabArquivo.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabEmail.ResumeLayout(false);
            this.tabEmail.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabBanco.ResumeLayout(false);
            this.tabBanco.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errPadrao)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbBancoDados;
        private System.Windows.Forms.RadioButton rbEmail;
        private System.Windows.Forms.RadioButton rbArquivo;
        private System.Windows.Forms.TabControl tabCtrlLog4Net;
        private System.Windows.Forms.TabPage tabArquivo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbArquivoXML;
        private System.Windows.Forms.RadioButton rbArquivoTXT;
        private System.Windows.Forms.TabPage tabEmail;
        private System.Windows.Forms.TabPage tabBanco;
        private System.Windows.Forms.TextBox txtTamanhoBackup;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtQtdBackup;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbTipoTrocaArquivo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkAppendFile;
        private System.Windows.Forms.ComboBox cbLockFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtPorta;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkSSL;
        private System.Windows.Forms.TextBox txtSenha;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtAssunto;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtEmailPara;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtEmailDe;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox cbBanco;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ErrorProvider errPadrao;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button bntConfirmar;
        private System.Windows.Forms.ComboBox cbLevel;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button bntAjuda;
        public System.Windows.Forms.TextBox txtNomeArquivo;
    }
}