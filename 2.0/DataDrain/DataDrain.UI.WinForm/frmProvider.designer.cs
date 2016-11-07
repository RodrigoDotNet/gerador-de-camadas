using System.Windows.Forms;

namespace DataDrain.UI.WinForm
{
    partial class frmProvider
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProvider));
            this.rptProviders = new Microsoft.VisualBasic.PowerPacks.DataRepeater();
            this.lblBDMinimo = new System.Windows.Forms.Label();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblVersao = new System.Windows.Forms.Label();
            this.bntSelecionar = new System.Windows.Forms.Button();
            this.labelControl1 = new System.Windows.Forms.Label();
            this.lblTabela = new System.Windows.Forms.Label();
            this.lblView = new System.Windows.Forms.Label();
            this.lblProcedure = new System.Windows.Forms.Label();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.rptProviders.ItemTemplate.SuspendLayout();
            this.rptProviders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // rptProviders
            // 
            this.rptProviders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // rptProviders.ItemTemplate
            // 
            this.rptProviders.ItemTemplate.BackColor = System.Drawing.Color.White;
            this.rptProviders.ItemTemplate.Controls.Add(this.lblProcedure);
            this.rptProviders.ItemTemplate.Controls.Add(this.lblView);
            this.rptProviders.ItemTemplate.Controls.Add(this.lblTabela);
            this.rptProviders.ItemTemplate.Controls.Add(this.lblBDMinimo);
            this.rptProviders.ItemTemplate.Controls.Add(this.pbLogo);
            this.rptProviders.ItemTemplate.Controls.Add(this.lblVersao);
            this.rptProviders.ItemTemplate.Controls.Add(this.shapeContainer2);
            this.rptProviders.ItemTemplate.Size = new System.Drawing.Size(212, 321);
            this.rptProviders.ItemTemplate.DoubleClick += new System.EventHandler(this.rptProviders_ItemTemplate_DoubleClick);
            this.rptProviders.LayoutStyle = Microsoft.VisualBasic.PowerPacks.DataRepeaterLayoutStyles.Horizontal;
            this.rptProviders.Location = new System.Drawing.Point(13, 13);
            this.rptProviders.Name = "rptProviders";
            this.rptProviders.Size = new System.Drawing.Size(636, 329);
            this.rptProviders.TabIndex = 0;
            this.rptProviders.Text = "dataRepeater1";
            // 
            // lblBDMinimo
            // 
            this.lblBDMinimo.AutoSize = true;
            this.lblBDMinimo.Location = new System.Drawing.Point(14, 209);
            this.lblBDMinimo.Name = "lblBDMinimo";
            this.lblBDMinimo.Size = new System.Drawing.Size(71, 13);
            this.lblBDMinimo.TabIndex = 3;
            this.lblBDMinimo.Text = "BancoMinimo";
            // 
            // pbLogo
            // 
            this.pbLogo.Location = new System.Drawing.Point(15, 3);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(180, 180);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 1;
            this.pbLogo.TabStop = false;
            // 
            // lblVersao
            // 
            this.lblVersao.AutoSize = true;
            this.lblVersao.Location = new System.Drawing.Point(14, 187);
            this.lblVersao.Name = "lblVersao";
            this.lblVersao.Size = new System.Drawing.Size(40, 13);
            this.lblVersao.TabIndex = 2;
            this.lblVersao.Text = "Versão";
            // 
            // bntSelecionar
            // 
            this.bntSelecionar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bntSelecionar.Location = new System.Drawing.Point(574, 361);
            this.bntSelecionar.Name = "bntSelecionar";
            this.bntSelecionar.Size = new System.Drawing.Size(75, 23);
            this.bntSelecionar.TabIndex = 1;
            this.bntSelecionar.Text = "Selecionar";
            this.bntSelecionar.Click += new System.EventHandler(this.bntSelecionar_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelControl1.AutoSize = true;
            this.labelControl1.Enabled = false;
            this.labelControl1.Location = new System.Drawing.Point(13, 371);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(79, 13);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Versão: 1.0.0.0";
            // 
            // lblTabela
            // 
            this.lblTabela.AutoSize = true;
            this.lblTabela.Location = new System.Drawing.Point(14, 231);
            this.lblTabela.Name = "lblTabela";
            this.lblTabela.Size = new System.Drawing.Size(40, 13);
            this.lblTabela.TabIndex = 4;
            this.lblTabela.Text = "Tabela";
            // 
            // lblView
            // 
            this.lblView.AutoSize = true;
            this.lblView.Location = new System.Drawing.Point(14, 250);
            this.lblView.Name = "lblView";
            this.lblView.Size = new System.Drawing.Size(71, 13);
            this.lblView.TabIndex = 5;
            this.lblView.Text = "BancoMinimo";
            // 
            // lblProcedure
            // 
            this.lblProcedure.AutoSize = true;
            this.lblProcedure.Location = new System.Drawing.Point(14, 269);
            this.lblProcedure.Name = "lblProcedure";
            this.lblProcedure.Size = new System.Drawing.Size(71, 13);
            this.lblProcedure.TabIndex = 6;
            this.lblProcedure.Text = "BancoMinimo";
            // 
            // lineShape1
            // 
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = -6;
            this.lineShape1.X2 = 213;
            this.lineShape1.Y1 = 204;
            this.lineShape1.Y2 = 204;
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape2,
            this.lineShape1});
            this.shapeContainer2.Size = new System.Drawing.Size(211, 306);
            this.shapeContainer2.TabIndex = 7;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape2
            // 
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = -6;
            this.lineShape2.X2 = 213;
            this.lineShape2.Y1 = 226;
            this.lineShape2.Y2 = 226;
            // 
            // frmProvider
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 396);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.bntSelecionar);
            this.Controls.Add(this.rptProviders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmProvider";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Selecione o Banco";
            this.Load += new System.EventHandler(this.frmProvider_Load);
            this.rptProviders.ItemTemplate.ResumeLayout(false);
            this.rptProviders.ItemTemplate.PerformLayout();
            this.rptProviders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.VisualBasic.PowerPacks.DataRepeater rptProviders;
        private Label lblBDMinimo;
        private System.Windows.Forms.PictureBox pbLogo;
        private Label lblVersao;
        private Button bntSelecionar;
        private Label labelControl1;
        private Label lblProcedure;
        private Label lblView;
        private Label lblTabela;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
    }
}