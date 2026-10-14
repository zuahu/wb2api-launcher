namespace wb2api.launcher.UI
{
    partial class BodyForm
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

            this.TopPanel = new AntdUI.Panel();
            this.ConsolePanel = new AntdUI.Panel();
            this.Console = new my.common.UI.CRichConsole(this.components);

            this.PathLabel = new AntdUI.Label();
            this.ExePathInput = new AntdUI.Input();
            this.BrowseBtn = new AntdUI.Button();
            this.SaveBtn = new AntdUI.Button();
            this.ArgsLabel = new AntdUI.Label();
            this.ArgsInput = new AntdUI.Input();
            this.UrlLabel = new AntdUI.Label();
            this.UrlInput = new AntdUI.Input();
            this.OpenPanelBtn = new AntdUI.Button();
            this.HealthBtn = new AntdUI.Button();
            this.AutoRunCheck = new AntdUI.Checkbox();
            this.AutoMinCheck = new AntdUI.Checkbox();
            this.StatusLabel = new AntdUI.Label();
            this.ClearBtn = new AntdUI.Button();
            this.StartBtn = new AntdUI.Button();
            this.StopBtn = new AntdUI.Button();

            this.TopPanel.SuspendLayout();
            this.ConsolePanel.SuspendLayout();
            this.SuspendLayout();

            //
            // TopPanel
            //
            this.TopPanel.Controls.Add(this.PathLabel);
            this.TopPanel.Controls.Add(this.ExePathInput);
            this.TopPanel.Controls.Add(this.BrowseBtn);
            this.TopPanel.Controls.Add(this.SaveBtn);
            this.TopPanel.Controls.Add(this.ArgsLabel);
            this.TopPanel.Controls.Add(this.ArgsInput);
            this.TopPanel.Controls.Add(this.UrlLabel);
            this.TopPanel.Controls.Add(this.UrlInput);
            this.TopPanel.Controls.Add(this.OpenPanelBtn);
            this.TopPanel.Controls.Add(this.HealthBtn);
            this.TopPanel.Controls.Add(this.AutoRunCheck);
            this.TopPanel.Controls.Add(this.AutoMinCheck);
            this.TopPanel.Controls.Add(this.StatusLabel);
            this.TopPanel.Controls.Add(this.ClearBtn);
            this.TopPanel.Controls.Add(this.StartBtn);
            this.TopPanel.Controls.Add(this.StopBtn);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.TopPanel.Radius = 0;
            this.TopPanel.Size = new System.Drawing.Size(960, 232);
            this.TopPanel.TabIndex = 0;

            //
            // PathLabel / ExePathInput
            //
            this.PathLabel.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.PathLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.PathLabel.Location = new System.Drawing.Point(24, 22);
            this.PathLabel.Name = "PathLabel";
            this.PathLabel.Size = new System.Drawing.Size(96, 22);
            this.PathLabel.TabIndex = 0;
            this.PathLabel.Text = "启动文件";

            this.ExePathInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.ExePathInput.Location = new System.Drawing.Point(130, 16);
            this.ExePathInput.Name = "ExePathInput";
            this.ExePathInput.PlaceholderText = "wb2api.exe 的完整路径，可点右侧「浏览…」选择";
            this.ExePathInput.Size = new System.Drawing.Size(600, 34);
            this.ExePathInput.TabIndex = 1;

            this.BrowseBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.BrowseBtn.Location = new System.Drawing.Point(742, 16);
            this.BrowseBtn.Name = "BrowseBtn";
            this.BrowseBtn.Size = new System.Drawing.Size(96, 34);
            this.BrowseBtn.TabIndex = 2;
            this.BrowseBtn.Text = "浏览…";
            this.BrowseBtn.Click += new System.EventHandler(this.BrowseBtn_Click);

            this.SaveBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.SaveBtn.Location = new System.Drawing.Point(850, 16);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(86, 34);
            this.SaveBtn.TabIndex = 3;
            this.SaveBtn.Text = "保存";
            this.SaveBtn.Type = AntdUI.TTypeMini.Primary;
            this.SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);

            //
            // ArgsLabel / ArgsInput
            //
            this.ArgsLabel.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.ArgsLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.ArgsLabel.Location = new System.Drawing.Point(24, 64);
            this.ArgsLabel.Name = "ArgsLabel";
            this.ArgsLabel.Size = new System.Drawing.Size(96, 22);
            this.ArgsLabel.TabIndex = 4;
            this.ArgsLabel.Text = "启动参数";

            this.ArgsInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.ArgsInput.Location = new System.Drawing.Point(130, 58);
            this.ArgsInput.Name = "ArgsInput";
            this.ArgsInput.PlaceholderText = "例如 -config config.json（工作目录自动取 exe 所在目录）";
            this.ArgsInput.Size = new System.Drawing.Size(806, 34);
            this.ArgsInput.TabIndex = 5;

            //
            // UrlLabel / UrlInput
            //
            this.UrlLabel.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.UrlLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.UrlLabel.Location = new System.Drawing.Point(24, 106);
            this.UrlLabel.Name = "UrlLabel";
            this.UrlLabel.Size = new System.Drawing.Size(96, 22);
            this.UrlLabel.TabIndex = 6;
            this.UrlLabel.Text = "面板地址";

            this.UrlInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.UrlInput.Location = new System.Drawing.Point(130, 100);
            this.UrlInput.Name = "UrlInput";
            this.UrlInput.PlaceholderText = "http://127.0.0.1:7863/panel/";
            this.UrlInput.Size = new System.Drawing.Size(600, 34);
            this.UrlInput.TabIndex = 7;

            this.OpenPanelBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.OpenPanelBtn.Location = new System.Drawing.Point(742, 100);
            this.OpenPanelBtn.Name = "OpenPanelBtn";
            this.OpenPanelBtn.Size = new System.Drawing.Size(96, 34);
            this.OpenPanelBtn.TabIndex = 8;
            this.OpenPanelBtn.Text = "打开面板";
            this.OpenPanelBtn.Click += new System.EventHandler(this.OpenPanelBtn_Click);

            this.HealthBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.HealthBtn.Location = new System.Drawing.Point(850, 100);
            this.HealthBtn.Name = "HealthBtn";
            this.HealthBtn.Size = new System.Drawing.Size(86, 34);
            this.HealthBtn.TabIndex = 9;
            this.HealthBtn.Text = "检查";
            this.HealthBtn.Click += new System.EventHandler(this.HealthBtn_Click);

            //
            // 选项行
            //
            this.AutoRunCheck.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.AutoRunCheck.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.AutoRunCheck.Location = new System.Drawing.Point(130, 144);
            this.AutoRunCheck.Name = "AutoRunCheck";
            this.AutoRunCheck.Size = new System.Drawing.Size(230, 24);
            this.AutoRunCheck.TabIndex = 10;
            this.AutoRunCheck.Text = "开机自动启动并最小化";
            this.AutoRunCheck.CheckedChanged += new AntdUI.BoolEventHandler(this.AutoRunCheck_CheckedChanged);

            this.AutoMinCheck.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.AutoMinCheck.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.AutoMinCheck.Location = new System.Drawing.Point(380, 144);
            this.AutoMinCheck.Name = "AutoMinCheck";
            this.AutoMinCheck.Size = new System.Drawing.Size(230, 24);
            this.AutoMinCheck.TabIndex = 11;
            this.AutoMinCheck.Text = "启动后最小化到托盘";
            this.AutoMinCheck.CheckedChanged += new AntdUI.BoolEventHandler(this.AutoMinCheck_CheckedChanged);

            //
            // 状态与操作行
            //
            this.StatusLabel.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.StatusLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.StatusLabel.Location = new System.Drawing.Point(24, 190);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(380, 22);
            this.StatusLabel.TabIndex = 12;
            this.StatusLabel.Text = "状态：已停止";

            this.ClearBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.ClearBtn.Location = new System.Drawing.Point(420, 184);
            this.ClearBtn.Name = "ClearBtn";
            this.ClearBtn.Size = new System.Drawing.Size(100, 34);
            this.ClearBtn.TabIndex = 13;
            this.ClearBtn.Text = "清空日志";
            this.ClearBtn.Click += new System.EventHandler(this.ClearBtn_Click);

            this.StartBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.StartBtn.Location = new System.Drawing.Point(640, 184);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(110, 34);
            this.StartBtn.TabIndex = 14;
            this.StartBtn.Text = "启动服务";
            this.StartBtn.Type = AntdUI.TTypeMini.Success;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);

            this.StopBtn.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.StopBtn.Location = new System.Drawing.Point(762, 184);
            this.StopBtn.Name = "StopBtn";
            this.StopBtn.Size = new System.Drawing.Size(110, 34);
            this.StopBtn.TabIndex = 15;
            this.StopBtn.Text = "停止服务";
            this.StopBtn.Type = AntdUI.TTypeMini.Error;
            this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);

            //
            // ConsolePanel / Console
            //
            this.ConsolePanel.Controls.Add(this.Console);
            this.ConsolePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsolePanel.Location = new System.Drawing.Point(0, 232);
            this.ConsolePanel.Name = "ConsolePanel";
            this.ConsolePanel.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            this.ConsolePanel.Radius = 0;
            this.ConsolePanel.Size = new System.Drawing.Size(960, 328);
            this.ConsolePanel.TabIndex = 1;

            this.Console.AsynDelay = 200;
            this.Console.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(65)))), ((int)(((byte)(66)))));
            this.Console.BatchSize = 200;
            this.Console.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Console.ContextMenuWidth = 200;
            this.Console.DebugTextBackColor = null;
            this.Console.DebugTextForeColor = null;
            this.Console.DetectUrls = false;
            this.Console.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Console.ErrorTextBackColor = null;
            this.Console.ErrorTextForeColor = "red";
            this.Console.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.Console.ForeColor = System.Drawing.Color.White;
            this.Console.InfoTextBackColor = null;
            this.Console.InfoTextForeColor = null;
            this.Console.IsAsyn = false;
            this.Console.IsAutoScroll = true;
            this.Console.IsDebugEnabled = false;
            this.Console.IsErrorEnabled = true;
            this.Console.IsHtmlLog = true;
            this.Console.IsInfoEnabled = true;
            this.Console.IsPrintTime = true;
            this.Console.IsViewSource = false;
            this.Console.IsWarnEnabled = true;
            this.Console.LineCount = 5000;
            this.Console.LineSpacingTwips = 300;
            this.Console.Location = new System.Drawing.Point(10, 6);
            this.Console.Name = "Console";
            this.Console.OutputTag = "pre";
            this.Console.PageBackColor = "#424142";
            this.Console.PageStyle = "margin:0px;padding:2px;font-size:12px;color:#fff;font-family:微软雅黑;";
            this.Console.ReadOnly = true;
            this.Console.Size = new System.Drawing.Size(940, 312);
            this.Console.TabIndex = 0;
            this.Console.Text = "";
            this.Console.TimePattern = "[HH:mm:ss]";
            this.Console.WarnTextBackColor = null;
            this.Console.WarnTextForeColor = "yellow";

            //
            // BodyForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 560);
            this.Controls.Add(this.ConsolePanel);
            this.Controls.Add(this.TopPanel);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BodyForm";
            this.Text = "BodyForm";

            this.TopPanel.ResumeLayout(false);
            this.ConsolePanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private AntdUI.Panel TopPanel;
        private AntdUI.Panel ConsolePanel;
        private my.common.UI.CRichConsole Console;

        private AntdUI.Label PathLabel;
        private AntdUI.Input ExePathInput;
        private AntdUI.Button BrowseBtn;
        private AntdUI.Button SaveBtn;
        private AntdUI.Label ArgsLabel;
        private AntdUI.Input ArgsInput;
        private AntdUI.Label UrlLabel;
        private AntdUI.Input UrlInput;
        private AntdUI.Button OpenPanelBtn;
        private AntdUI.Button HealthBtn;
        private AntdUI.Checkbox AutoRunCheck;
        private AntdUI.Checkbox AutoMinCheck;
        private AntdUI.Label StatusLabel;
        private AntdUI.Button ClearBtn;
        private AntdUI.Button StartBtn;
        private AntdUI.Button StopBtn;
    }
}
