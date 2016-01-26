using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataDrain.ORM.Generator.Formularios
{
    public partial class frmRegExpression : Form
    {
        #region Load

        public frmRegExpression()
        {
            InitializeComponent();
        }

        private void frmRegExpression_Load(object sender, EventArgs e)
        {

        }

        #endregion

        #region Eventos

        private void txtRegexValidator_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Regex.Match("", txtRegexValidator.Text);
                txtRegexValidator.BackColor = Color.LightGreen;
                errProvRegExp.SetError(txtRegexValidator, "");
                txtTeste.Enabled = true;
                bntGerar.Enabled = true;
                txtTeste_TextChanged(txtTeste, EventArgs.Empty);
            }
            catch (Exception)
            {
                txtRegexValidator.BackColor = Color.Red;
                errProvRegExp.SetError(txtRegexValidator, "Regular Expression inválida");
                txtTeste.Enabled = false;
                bntGerar.Enabled = false;
            }
        }

        private void bntGerar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtTeste_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtTeste.BackColor = Regex.Match(txtTeste.Text, txtRegexValidator.Text).Success ? Color.LightGreen : Color.Red;
            }
            catch (Exception)
            {
                txtTeste.BackColor = Color.Red;
            }
        }

        private void bntAjuda_Click(object sender, EventArgs e)
        {
            //http://msdn.microsoft.com/en-us/library/dd901590(v=vs.95).aspx
            var help = new frmRegExpHelp();
            help.ShowDialog();
            help.Dispose();
        }

        #endregion
    }
}
