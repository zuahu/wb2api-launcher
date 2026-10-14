namespace wb2api.launcher.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.FormPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();

            //
            // WindowBar
            //
            this.WindowBar.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.WindowBar.MaximizeBox = true;
            this.WindowBar.MinimizeBox = true;
            this.WindowBar.Size = new System.Drawing.Size(960, 53);

            //
            // FormPanel
            //
            this.FormPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormPanel.Location = new System.Drawing.Point(0, 53);
            this.FormPanel.Name = "FormPanel";
            this.FormPanel.TabIndex = 1;

            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 640);
            this.Controls.Add(this.FormPanel);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.IsRememberResize = false;
            this.MinimumSize = new System.Drawing.Size(960, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WorkBuddy2API 启动器";
            this.Controls.SetChildIndex(this.WindowBar, 0);
            this.Controls.SetChildIndex(this.FormPanel, 0);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel FormPanel;
    }
}
