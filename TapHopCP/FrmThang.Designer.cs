namespace TapHopCP
{
    partial class FrmThang
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
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.seThang = new DevExpress.XtraEditors.SpinEdit();
            ((System.ComponentModel.ISupportInitialize)(this.seThang.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(103, 12);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 0;
            this.simpleButton1.Text = "Tập hợp";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // seThang
            // 
            this.seThang.EditValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.seThang.Location = new System.Drawing.Point(12, 12);
            this.seThang.Name = "seThang";
            this.seThang.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.seThang.Properties.MaxValue = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.seThang.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.seThang.Size = new System.Drawing.Size(62, 20);
            this.seThang.TabIndex = 1;
            // 
            // FrmThang
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(190, 49);
            this.Controls.Add(this.seThang);
            this.Controls.Add(this.simpleButton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmThang";
            this.Text = "Chọn tháng";
            ((System.ComponentModel.ISupportInitialize)(this.seThang.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.SpinEdit seThang;
    }
}