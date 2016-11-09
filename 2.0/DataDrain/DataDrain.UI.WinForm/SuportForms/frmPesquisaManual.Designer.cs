namespace DataDrain.UI.WinForm.SuportForms
{
    partial class frmPesquisaManual
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPesquisaManual));
            this.label1 = new System.Windows.Forms.Label();
            this.txtConsulta = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlParametros = new System.Windows.Forms.Panel();
            this.dgParametros = new System.Windows.Forms.DataGridView();
            this.btnMapear = new System.Windows.Forms.Button();
            this.errPadrao = new System.Windows.Forms.ErrorProvider(this.components);
            this.pnlParametros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgParametros)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errPadrao)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Consulta:";
            // 
            // txtConsulta
            // 
            this.txtConsulta.AcceptsReturn = true;
            this.txtConsulta.AcceptsTab = true;
            this.txtConsulta.Location = new System.Drawing.Point(16, 29);
            this.txtConsulta.Multiline = true;
            this.txtConsulta.Name = "txtConsulta";
            this.txtConsulta.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConsulta.Size = new System.Drawing.Size(622, 171);
            this.txtConsulta.TabIndex = 1;
            this.txtConsulta.TextChanged += new System.EventHandler(this.txtConsulta_TextChanged);
            this.txtConsulta.Validating += new System.ComponentModel.CancelEventHandler(this.txtConsulta_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Parâmetros:";
            // 
            // pnlParametros
            // 
            this.pnlParametros.Controls.Add(this.dgParametros);
            this.pnlParametros.Controls.Add(this.label2);
            this.pnlParametros.Location = new System.Drawing.Point(4, 206);
            this.pnlParametros.Name = "pnlParametros";
            this.pnlParametros.Size = new System.Drawing.Size(517, 158);
            this.pnlParametros.TabIndex = 4;
            // 
            // dgParametros
            // 
            this.dgParametros.AllowUserToAddRows = false;
            this.dgParametros.AllowUserToDeleteRows = false;
            this.dgParametros.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgParametros.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgParametros.Location = new System.Drawing.Point(12, 25);
            this.dgParametros.Name = "dgParametros";
            this.dgParametros.Size = new System.Drawing.Size(502, 123);
            this.dgParametros.TabIndex = 4;
            this.dgParametros.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgParametros_CellValidating);
            this.dgParametros.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgParametros_CellValueChanged);
            // 
            // btnMapear
            // 
            this.btnMapear.Image = ((System.Drawing.Image)(resources.GetObject("btnMapear.Image")));
            this.btnMapear.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMapear.Location = new System.Drawing.Point(555, 215);
            this.btnMapear.Name = "btnMapear";
            this.btnMapear.Size = new System.Drawing.Size(83, 34);
            this.btnMapear.TabIndex = 5;
            this.btnMapear.Text = "Mapear";
            this.btnMapear.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMapear.Click += new System.EventHandler(this.btnMapear_Click);
            // 
            // errPadrao
            // 
            this.errPadrao.ContainerControl = this;
            // 
            // frmPesquisaManual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 366);
            this.Controls.Add(this.btnMapear);
            this.Controls.Add(this.pnlParametros);
            this.Controls.Add(this.txtConsulta);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPesquisaManual";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pesquisa Manual";
            this.Load += new System.EventHandler(this.frmPesquisaManual_Load);
            this.pnlParametros.ResumeLayout(false);
            this.pnlParametros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgParametros)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errPadrao)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConsulta;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pnlParametros;
        private System.Windows.Forms.Button btnMapear;
        private System.Windows.Forms.ErrorProvider errPadrao;
        private System.Windows.Forms.DataGridView dgParametros;
    }
}