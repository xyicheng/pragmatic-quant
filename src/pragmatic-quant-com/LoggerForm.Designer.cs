namespace pragmatic_quant_com
{
    partial class LoggerForm
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
            this.logDisplayTrace = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // logDisplayTrace
            // 
            this.logDisplayTrace.BackColor = System.Drawing.SystemColors.HighlightText;
            this.logDisplayTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logDisplayTrace.ForeColor = System.Drawing.Color.DodgerBlue;
            this.logDisplayTrace.Location = new System.Drawing.Point(0, 0);
            this.logDisplayTrace.Multiline = true;
            this.logDisplayTrace.Name = "logDisplayTrace";
            this.logDisplayTrace.ReadOnly = true;
            this.logDisplayTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logDisplayTrace.Size = new System.Drawing.Size(461, 215);
            this.logDisplayTrace.TabIndex = 0;
            // 
            // LoggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 215);
            this.Controls.Add(this.logDisplayTrace);
            this.Name = "LoggerForm";
            this.Text = "pragmatic-quant log";
            this.Load += new System.EventHandler(this.LoggerForm_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox logDisplayTrace;
    }
}