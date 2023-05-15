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
            this.btnnext = new System.Windows.Forms.Button();
            this.cbcheck = new System.Windows.Forms.RadioButton();
            this.cbsplitdui = new System.Windows.Forms.RadioButton();
            this.cbMix = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbMix);
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
            // btnnext
            // 
            this.btnnext.Location = new System.Drawing.Point(238, 232);
            this.btnnext.Name = "btnnext";
            this.btnnext.Size = new System.Drawing.Size(75, 23);
            this.btnnext.TabIndex = 0;
            this.btnnext.Text = "继续";
            this.btnnext.UseVisualStyleBackColor = true;
            // 
            // cbcheck
            // 
            this.cbcheck.AutoSize = true;
            this.cbcheck.Location = new System.Drawing.Point(13, 42);
            this.cbcheck.Name = "cbcheck";
            this.cbcheck.Size = new System.Drawing.Size(155, 16);
            this.cbcheck.TabIndex = 1;
            this.cbcheck.TabStop = true;
            this.cbcheck.Text = "调用二级客户对账单模板";
            this.cbcheck.UseVisualStyleBackColor = true;
            // 
            // cbsplitdui
            // 
            this.cbsplitdui.AutoSize = true;
            this.cbsplitdui.Location = new System.Drawing.Point(13, 64);
            this.cbsplitdui.Name = "cbsplitdui";
            this.cbsplitdui.Size = new System.Drawing.Size(155, 16);
            this.cbsplitdui.TabIndex = 2;
            this.cbsplitdui.TabStop = true;
            this.cbsplitdui.Text = "按\'客户\'拆分导出对账单";
            this.cbsplitdui.UseVisualStyleBackColor = true;
            // 
            // cbMix
            // 
            this.cbMix.AutoSize = true;
            this.cbMix.Location = new System.Drawing.Point(13, 86);
            this.cbMix.Name = "cbMix";
            this.cbMix.Size = new System.Drawing.Size(71, 16);
            this.cbMix.TabIndex = 3;
            this.cbMix.TabStop = true;
            this.cbMix.Text = "合拼导出";
            this.cbMix.UseVisualStyleBackColor = true;
            // 
            // GenPdfSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 261);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Name = "GenPdfSet";
            this.Text = "打印导出设置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnnext;
        private System.Windows.Forms.RadioButton cbMix;
        private System.Windows.Forms.RadioButton cbsplitdui;
        private System.Windows.Forms.RadioButton cbcheck;
    }
}