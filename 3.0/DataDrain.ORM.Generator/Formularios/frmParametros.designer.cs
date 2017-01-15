using System.Windows.Forms;

namespace DataDrain.ORM.Generator.Formularios
{
    partial class frmParametros
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmParametros));
            this.dgParametros = new System.Windows.Forms.DataGridView();
            this.bntGerar = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgParametros)).BeginInit();
            this.SuspendLayout();
            // 
            // dgParametros
            // 
            this.dgParametros.AllowUserToAddRows = false;
            this.dgParametros.AllowUserToDeleteRows = false;
            this.dgParametros.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgParametros.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgParametros.Location = new System.Drawing.Point(13, 13);
            this.dgParametros.Name = "dgParametros";
            this.dgParametros.Size = new System.Drawing.Size(423, 150);
            this.dgParametros.TabIndex = 0;
            // 
            // bntGerar
            // 
            this.bntGerar.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.bntGerar.Location = new System.Drawing.Point(361, 176);
            this.bntGerar.Name = "bntGerar";
            this.bntGerar.Size = new System.Drawing.Size(75, 23);
            this.bntGerar.TabIndex = 1;
            this.bntGerar.Text = "Executar";
            this.bntGerar.Click += new System.EventHandler(this.bntGerar_Click);
            // 
            // frmParametros
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 211);
            this.Controls.Add(this.bntGerar);
            this.Controls.Add(this.dgParametros);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmParametros";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parâmetros - ";
            this.Load += new System.EventHandler(this.frmParametros_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgParametros)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgParametros;
        private Button bntGerar;
    }
}