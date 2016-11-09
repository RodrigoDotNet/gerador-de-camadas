namespace DataDrain.UI.WinForm.SuportForms
{
    partial class frmMapeamentoUnitario
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
            this.gvColunasObjeto = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.btnAvancar2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gvColunasObjeto)).BeginInit();
            this.SuspendLayout();
            // 
            // gvColunasObjeto
            // 
            this.gvColunasObjeto.AllowUserToAddRows = false;
            this.gvColunasObjeto.AllowUserToDeleteRows = false;
            this.gvColunasObjeto.AllowUserToResizeRows = false;
            this.gvColunasObjeto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.gvColunasObjeto.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gvColunasObjeto.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvColunasObjeto.Location = new System.Drawing.Point(12, 17);
            this.gvColunasObjeto.Name = "gvColunasObjeto";
            this.gvColunasObjeto.Size = new System.Drawing.Size(851, 191);
            this.gvColunasObjeto.TabIndex = 0;
            this.gvColunasObjeto.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvColunasObjeto_CellValueChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.No;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.Location = new System.Drawing.Point(721, 224);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 34);
            this.button1.TabIndex = 2;
            this.button1.Text = "Cancelar Mapeamento";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnAvancar2
            // 
            this.btnAvancar2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAvancar2.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnAvancar2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAvancar2.Location = new System.Drawing.Point(581, 224);
            this.btnAvancar2.Name = "btnAvancar2";
            this.btnAvancar2.Size = new System.Drawing.Size(134, 34);
            this.btnAvancar2.TabIndex = 1;
            this.btnAvancar2.Text = "Mapear";
            this.btnAvancar2.Click += new System.EventHandler(this.btnAvancar2_Click);
            // 
            // frmMapeamentoUnitario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 267);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnAvancar2);
            this.Controls.Add(this.gvColunasObjeto);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMapeamentoUnitario";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mapeamento Avançado";
            this.Load += new System.EventHandler(this.frmMapeamentoUnitario_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gvColunasObjeto)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAvancar2;
        private System.Windows.Forms.DataGridView gvColunasObjeto;
        internal System.Windows.Forms.Button button1;
    }
}