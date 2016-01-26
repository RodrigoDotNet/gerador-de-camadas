using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataDrain.ORM.Generator.Apoio;
using DataDrain.ORM.Interfaces;

namespace DataDrain.ORM.Generator.Formularios
{
    public partial class frmLog4Net : Form
    {
        #region Load

        public frmLog4Net()
        {
            InitializeComponent();
        }

        #endregion

        #region Propriedades

        public string StringConexao { get; set; }

        public IInformationSchema Gerador { get; set; }

        public string XmlRetorno { get; private set; }

        public bool Confirmou { get; private set; }

        #endregion

        #region Eventos

        #region Gerais

        private void bntAjuda_Click(object sender, EventArgs e)
        {
            Process.Start("http://logging.apache.org/log4net/release/config-examples.html");
        }

        private void txtQtdBackup_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) && e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }

        private void bntConfirmar_Click(object sender, EventArgs e)
        {
            if (cbLevel.SelectedIndex == -1)
            {
                errPadrao.SetErrorWithCount(cbLevel, "Selecione o level");
                return;
            }

            errPadrao.SetErrorWithCount(cbLevel, "");

            if (!errPadrao.HasErrors(tabCtrlLog4Net.SelectedTab))
            {
                var sbRetorno = new System.Text.StringBuilder();

                sbRetorno.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sbRetorno.AppendLine("<configuration>");
                sbRetorno.AppendLine("<configSections>");
                sbRetorno.AppendLine("<section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler,log4net\" />");
                sbRetorno.AppendLine("</configSections>");
                sbRetorno.AppendLine("[connectionStrings]");
                sbRetorno.AppendLine("<log4net>");
                sbRetorno.AppendLine("<root>");
                sbRetorno.AppendLine("<!--LogLevel: OFF, FATAL, ERROR, WARN, INFO, DEBUG, ALL -->");
                sbRetorno.AppendLine(string.Format("<level value=\"{0}\" />", cbLevel.SelectedItem.ToString().ToUpper()));

                if (rbArquivo.Checked)
                {
                    sbRetorno.AppendLine("<appender-ref ref=\"LogFileAppender\" />");
                    sbRetorno.AppendLine("</root>");
                    sbRetorno.AppendLine("<appender name=\"LogFileAppender\" type=\"log4net.Appender.RollingFileAppender\">");
                    sbRetorno.AppendLine(string.Format("      <file value=\"{0}.{1}\" />", txtNomeArquivo.Text, rbArquivoTXT.Checked ? "log" : "xml"));
                    sbRetorno.AppendLine(string.Format("      <appendToFile value=\"{0}\" />", chkAppendFile.Checked ? bool.TrueString : bool.FalseString));
                    sbRetorno.AppendLine(string.Format("      <rollingStyle value=\"{0}\" />", cbTipoTrocaArquivo.SelectedItem));
                    sbRetorno.AppendLine(string.Format("      <maxSizeRollBackups value=\"{0}\" />", txtQtdBackup.Text));
                    sbRetorno.AppendLine(string.Format("      <maximumFileSize value=\"{0}MB\" />", txtTamanhoBackup.Text));
                    sbRetorno.AppendLine("      <staticLogFileName value=\"true\" />");

                    if (rbArquivoTXT.Checked)
                    {
                        sbRetorno.AppendLine("<layout type=\"log4net.Layout.PatternLayout\">");
                        sbRetorno.AppendLine("      <conversionPattern value=\"%newline%date %newline[%thread] %newline%-5level %newline%logger %newline%M - %newline%newline%message%newline%newline%newline\" />");
                        sbRetorno.AppendLine("</layout>");
                    }
                    else if (rbArquivoXML.Checked)
                    {
                        sbRetorno.AppendLine("<layout type=\"log4net.Layout.XMLLayout\" />");
                    }
                    sbRetorno.AppendLine("</appender>");
                }
                else if (rbEmail.Checked)
                {
                    sbRetorno.AppendLine("<appender-ref ref=\"SmtpAppender\" />");
                    sbRetorno.AppendLine("</root>");
                    sbRetorno.AppendLine("<appender name=\"SmtpAppender\" type=\"log4net.Appender.SmtpAppender\">");
                    sbRetorno.AppendLine(string.Format("    <to value=\"{0}\" />", txtEmailPara.Text));
                    sbRetorno.AppendLine(string.Format("    <from value=\"{0}\" />", txtEmailDe.Text));
                    sbRetorno.AppendLine(string.Format("    <subject value=\"{0}\" />", txtAssunto.Text));
                    sbRetorno.AppendLine(string.Format("    <smtpHost value=\"{0}\" />", txtHost.Text));
                    sbRetorno.AppendLine("    <authentication value=\"Basic\" />");
                    sbRetorno.AppendLine(string.Format("    <port value=\"{0}\" />", txtPorta.Text));
                    sbRetorno.AppendLine(string.Format("    <username value=\"{0}\" />", txtUsuario.Text));
                    sbRetorno.AppendLine(string.Format("    <password value=\"{0}\" />", txtSenha.Text));
                    sbRetorno.AppendLine("<bufferSize value=\"1\" />");
                    sbRetorno.AppendLine(string.Format("    <EnableSsl value=\"{0}\"/>", (chkSSL.Checked ? bool.TrueString : bool.FalseString).ToLower()));
                    sbRetorno.AppendLine("<lossy value=\"false\" />");
                    sbRetorno.AppendLine("<layout type=\"log4net.Layout.PatternLayout\">");
                    sbRetorno.AppendLine("    <conversionPattern value=\"%newline%date %newline[%thread] %newline%-5level %newline%logger %newline%M - %newline%newline%message%newline%newline%newline\" />");
                    sbRetorno.AppendLine("</layout>");
                    sbRetorno.AppendLine("</appender>");
                }
                else if (rbBancoDados.Checked)
                {
                    var assembly = Gerador.GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(a => !a.Name.StartsWith("System") && !a.Name.StartsWith("DataDrain") && !a.Name.Contains("mscorlib"));
                    var appender = string.Format("AdoNetAppender_{0}", cbBanco.SelectedItem);

                    sbRetorno.AppendLine(string.Format("<appender-ref ref=\"{0}\" />", appender));
                    sbRetorno.AppendLine("</root>");
                    sbRetorno.AppendLine(string.Format("<appender name=\"{0}\" type=\"log4net.Appender.AdoNetAppender\">", appender));
                    sbRetorno.AppendLine("<bufferSize value=\"1\" />");
                    sbRetorno.AppendLine(string.Format("<connectionType value=\"{0}\" />", assembly != null ? assembly.FullName : Gerador.DotNetDataProviderFullName()));
                    sbRetorno.AppendLine("  <connectionString value=\"[connectionString]\" />");
                    sbRetorno.AppendLine("  <commandText value=\"INSERT INTO Log (Date,Thread,Level,Logger,Message,Method) VALUES (@log_date, @thread, @log_level, @logger, @message, @method)\" />");
                    sbRetorno.AppendLine("  <parameter>");
                    sbRetorno.AppendLine("    <parameterName value=\"@log_date\" />");
                    sbRetorno.AppendLine("    <dbType value=\"DateTime\" />");
                    sbRetorno.AppendLine("    <layout type=\"log4net.Layout.PatternLayout\" value=\"%date{yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}\" />");
                    sbRetorno.AppendLine("  </parameter>");
                    sbRetorno.AppendLine("  <parameter>");
                    sbRetorno.AppendLine("    <parameterName value=\"@thread\" />");
                    sbRetorno.AppendLine("    <dbType value=\"String\" />");
                    sbRetorno.AppendLine("    <size value=\"255\" />");
                    sbRetorno.AppendLine("    <layout type=\"log4net.Layout.PatternLayout\" value=\"%thread\" />");
                    sbRetorno.AppendLine("  </parameter>");
                    sbRetorno.AppendLine("  <parameter>");
                    sbRetorno.AppendLine("    <parameterName value=\"@log_level\" />");
                    sbRetorno.AppendLine("    <dbType value=\"String\" />");
                    sbRetorno.AppendLine("    <size value=\"50\" />");
                    sbRetorno.AppendLine("    <layout type=\"log4net.Layout.PatternLayout\" value=\"%level\" />");
                    sbRetorno.AppendLine("  </parameter>");
                    sbRetorno.AppendLine("  <parameter>");
                    sbRetorno.AppendLine("    <parameterName value=\"@logger\" />");
                    sbRetorno.AppendLine("    <dbType value=\"String\" />");
                    sbRetorno.AppendLine("    <size value=\"255\" />");
                    sbRetorno.AppendLine("    <layout type=\"log4net.Layout.PatternLayout\" value=\"%logger\" />");
                    sbRetorno.AppendLine("  </parameter>");
                    sbRetorno.AppendLine("  <parameter>");
                    sbRetorno.AppendLine("    <parameterName value=\"@message\" />");
                    sbRetorno.AppendLine("    <dbType value=\"String\" />");
                    sbRetorno.AppendLine("    <size value=\"4000\" />");
                    sbRetorno.AppendLine("    <layout type=\"log4net.Layout.PatternLayout\" value=\"%message\" />");
                    sbRetorno.AppendLine("  </parameter>");
                    sbRetorno.AppendLine("  <parameter>");
                    sbRetorno.AppendLine("    <parameterName value=\"@method\" />");
                    sbRetorno.AppendLine("    <dbType value=\"String\" />");
                    sbRetorno.AppendLine("    <size value=\"255\" />");
                    sbRetorno.AppendLine("    <layout type=\"log4net.Layout.PatternLayout\" value=\"%M\" />");
                    sbRetorno.AppendLine("  </parameter> ");
                    sbRetorno.AppendLine("</appender>");
                }

                sbRetorno.AppendLine("  </log4net>");
                sbRetorno.AppendLine("</configuration>");

                XmlRetorno = sbRetorno.ToString();
                Confirmou = true;
                Close();
            }
            else
            {
                MessageBox.Show("Preencha os campos obrigatórios", "ATENÇÃO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void rbArquivo_CheckedChanged(object sender, EventArgs e)
        {
            tabCtrlLog4Net.SelectedTab = tabArquivo;
        }

        private void rbEmail_CheckedChanged(object sender, EventArgs e)
        {
            tabCtrlLog4Net.SelectedTab = tabEmail;
        }

        private void rbBancoDados_CheckedChanged(object sender, EventArgs e)
        {
            tabCtrlLog4Net.SelectedTab = tabBanco;
        }

        #endregion

        #region Validação

        #region Tabulação Arquivo

        private void txtNomeArquivo_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtNomeArquivo, string.IsNullOrWhiteSpace(txtNomeArquivo.Text) ? "Informe o nome do arquivo" : "");

            txtNomeArquivo.Text = Arquivo.RetornaNomevalidoArquivo(txtNomeArquivo.Text);
        }

        private void txtQtdBackup_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtQtdBackup, txtQtdBackup.Text.ToInt32() <= 0 ? "Informe a quantidade de arquivos de backup" : "");
        }

        private void txtTamanhoBackup_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtTamanhoBackup, txtTamanhoBackup.Text.ToInt32() <= 0 ? "Informe o tamanho dos arquivos de Backup" : "");
        }

        private void cbLockFile_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(cbLockFile, cbLockFile.SelectedIndex == -1 ? "Selecione o tipo de Lock" : "");
        }

        private void cbTipoTrocaArquivo_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(cbTipoTrocaArquivo, cbTipoTrocaArquivo.SelectedIndex == -1 ? "Selecione o tipo de Rolling" : "");
        }

        #endregion

        #region Tabulação E-mail

        private void txtEmailDe_Validating(object sender, CancelEventArgs e)
        {
            ValidaPreencimentoEmail((TextBox)sender);
        }

        private void txtAssunto_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtAssunto, string.IsNullOrWhiteSpace(txtAssunto.Text) ? "Informe o assunto" : "");
        }

        private void txtHost_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtHost, string.IsNullOrWhiteSpace(txtHost.Text) ? "Informe o host" : "");
        }

        private void txtUsuario_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtUsuario, string.IsNullOrWhiteSpace(txtUsuario.Text) ? "Informe o usuário" : "");
        }

        private void txtSenha_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtSenha, string.IsNullOrWhiteSpace(txtSenha.Text) ? "Informe a senha" : "");
        }

        private void txtPorta_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(txtPorta, string.IsNullOrWhiteSpace(txtPorta.Text) ? "Informe a porta" : "");
        }

        #endregion

        #region Tabulação Banco

        private void cbBanco_Validating(object sender, CancelEventArgs e)
        {
            errPadrao.SetErrorWithCount(cbBanco, cbBanco.SelectedIndex == -1 ? "Selecione o Banco de Dados" : "");
        }

        #endregion

        #endregion

        #endregion

        #region Métodos

        public void ValidaPreencimentoEmail(TextBox txt)
        {
            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                errPadrao.SetErrorWithCount(txt, "Informe o e-mail");
            }
            else
            {
                const string matchEmailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                                                 + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                                                 + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                                                 + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";


                errPadrao.SetErrorWithCount(txt, !Regex.IsMatch(txt.Text, matchEmailPattern) ? "E-mail inválido" : "");
            }
        }

        #endregion

    }
}
