namespace CustomerStatementReportTool.BatchExport
{
    partial class GenPdfSet
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbMix = new System.Windows.Forms.CheckBox();
            this.lb = new System.Windows.Forms.Label();
            this.cbnormal = new System.Windows.Forms.CheckBox();
            this.cbsplitdui = new System.Windows.Forms.RadioButton();
            this.cbcheck = new System.Windows.Forms.RadioButton();
            this.btnnext = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tmclose = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbMix);
            this.groupBox1.Controls.Add(this.lb);
            this.groupBox1.Controls.Add(this.cbnormal);
            this.groupBox1.Controls.Add(this.cbsplitdui);
            this.groupBox1.Controls.Add(this.cbcheck);
            this.groupBox1.Controls.Add(this.btnnext);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(325, 261);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "导出方式设置";
            // 
            // cbMix
            // 
            this.cbMix.AutoSize = true;
            this.cbMix.Location = new System.Drawing.Point(26, 48);
            this.cbMix.Name = "cbMix";
            this.cbMix.Size = new System.Drawing.Size(72, 16);
            this.cbMix.TabIndex = 6;
            this.cbMix.Text = "合拼导出";
            this.cbMix.UseVisualStyleBackColor = true;
            // 
            // lb
            // 
            this.lb.AutoSize = true;
            this.lb.Font = new System.Drawing.Font("新宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb.ForeColor = System.Drawing.Color.Maroon;
            this.lb.Location = new System.Drawing.Point(12, 168);
            this.lb.Name = "lb";
            this.lb.Size = new System.Drawing.Size(0, 12);
            this.lb.TabIndex = 5;
            // 
            // cbnormal
            // 
            this.cbnormal.AutoSize = true;
            this.cbnormal.Location = new System.Drawing.Point(26, 70);
            this.cbnormal.Name = "cbnormal";
            this.cbnormal.Size = new System.Drawing.Size(72, 16);
            this.cbnormal.TabIndex = 4;
            this.cbnormal.Text = "常规导出";
            this.cbnormal.UseVisualStyleBackColor = true;
            // 
            // cbsplitdui
            // 
            this.cbsplitdui.AutoSize = true;
            this.cbsplitdui.Location = new System.Drawing.Point(43, 114);
            this.cbsplitdui.Name = "cbsplitdui";
            this.cbsplitdui.Size = new System.Drawing.Size(155, 16);
            this.cbsplitdui.TabIndex = 2;
            this.cbsplitdui.TabStop = true;
            this.cbsplitdui.Text = "按\'客户\'拆分导出对账单";
            this.cbsplitdui.UseVisualStyleBackColor = true;
            // 
            // cbcheck
            // 
            this.cbcheck.AutoSize = true;
            this.cbcheck.Location = new System.Drawing.Point(43, 92);
            this.cbcheck.Name = "cbcheck";
            this.cbcheck.Size = new System.Drawing.Size(155, 16);
            this.cbcheck.TabIndex = 1;
            this.cbcheck.TabStop = true;
            this.cbcheck.Text = "调用二级客户对账单模板";
            this.cbcheck.UseVisualStyleBackColor = true;
            // 
            // btnnext
            // 
            this.btnnext.Location = new System.Drawing.Point(272, 232);
            this.btnnext.Name = "btnnext";
            this.btnnext.Size = new System.Drawing.Size(45, 23);
            this.btnnext.TabIndex = 0;
            this.btnnext.Text = "继续";
            this.btnnext.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tmclose});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(325, 25);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tmclose
            // 
            this.tmclose.Name = "tmclose";
            this.tmclose.Size = new System.Drawing.Size(44, 21);
            this.tmclose.Text = "关闭";
            // 
            // GenPdfSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 261);
            this.ControlBox = false;
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox1);
            this.Name = "GenPdfSet";
            this.Text = "打印导出设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnnext;
        private System.Windows.Forms.RadioButton cbsplitdui;
        private System.Windows.Forms.RadioButton cbcheck;
        private System.Windows.Forms.Label lb;
        private System.Windows.Forms.CheckBox cbnormal;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tmclose;
        private System.Windows.Forms.CheckBox cbMix;
    }
}