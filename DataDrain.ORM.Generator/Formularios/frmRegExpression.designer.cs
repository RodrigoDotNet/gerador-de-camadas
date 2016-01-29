using System.Windows.Forms;

namespace DataDrain.ORM.Generator.Formularios
{
    partial class frmRegExpression
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
            this.bntGerar = new System.Windows.Forms.Button();
            this.txtRegexValidator = new System.Windows.Forms.TextBox();
            this.errProvRegExp = new System.Windows.Forms.ErrorProvider(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.txtTeste = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bntAjuda = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errProvRegExp)).BeginInit();
            this.SuspendLayout();
            // 
            // bntGerar
            // 
            this.bntGerar.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.bntGerar.Enabled = false;
            this.bntGerar.Location = new System.Drawing.Point(246, 128);
            this.bntGerar.Name = "bntGerar";
            this.bntGerar.Size = new System.Drawing.Size(75, 23);
            this.bntGerar.TabIndex = 2;
            this.bntGerar.Text = "Confirmar";
            this.bntGerar.Click += new System.EventHandler(this.bntGerar_Click);
            // 
            // txtRegexValidator
            // 
            this.txtRegexValidator.Location = new System.Drawing.Point(12, 25);
            this.txtRegexValidator.Name = "txtRegexValidator";
            this.txtRegexValidator.Size = new System.Drawing.Size(309, 20);
            this.txtRegexValidator.TabIndex = 3;
            this.txtRegexValidator.TextChanged += new System.EventHandler(this.txtRegexValidator_TextChanged);
            // 
            // errProvRegExp
            // 
            this.errProvRegExp.ContainerControl = this;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Regular Expression";
            // 
            // txtTeste
            // 
            this.txtTeste.Enabled = false;
            this.txtTeste.Location = new System.Drawing.Point(12, 64);
            this.txtTeste.Multiline = true;
            this.txtTeste.Name = "txtTeste";
            this.txtTeste.Size = new System.Drawing.Size(309, 48);
            this.txtTeste.TabIndex = 5;
            this.txtTeste.TextChanged += new System.EventHandler(this.txtTeste_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Teste";
            // 
            // bntAjuda
            // 
            this.bntAjuda.Image = global::DataDrain.ORM.Generator.Properties.Resources._1352224904_Help_book_3d;
            this.bntAjuda.Location = new System.Drawing.Point(12, 119);
            this.bntAjuda.Name = "bntAjuda";
            this.bntAjuda.Size = new System.Drawing.Size(32, 32);
            this.bntAjuda.TabIndex = 7;
            this.bntAjuda.Click += new System.EventHandler(this.bntAjuda_Click);
            // 
            // frmRegExpression
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 163);
            this.Controls.Add(this.bntAjuda);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtTeste);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtRegexValidator);
            this.Controls.Add(this.bntGerar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRegExpression";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Regular Expression";
            ((System.ComponentModel.ISupportInitialize)(this.errProvRegExp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button bntGerar;
        private System.Windows.Forms.ErrorProvider errProvRegExp;
        public TextBox txtRegexValidator;
        private Label label1;
        private Label label2;
        private TextBox txtTeste;
        private Button bntAjuda;
    }
}