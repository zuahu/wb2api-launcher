using System;
using System.IO;
using System.Windows.Forms;
using my.antd.ui;
using my.common;
using wb2api.launcher.Services;

namespace wb2api.launcher.UI
{
    public partial class BodyForm : Form
    {
        private readonly MainForm _host;
        private bool _loading;
        private bool _savedAutoRun;
        private bool _savedAutoMin;

        public BodyForm(MainForm host)
        {
            InitializeComponent();
            _host = host;
            UpdateStatus();
        }

        public void ApplyConfig(LauncherConfig cfg)
        {
            if (cfg == null) return;
            _loading = true;
            try
            {
                ExePathInput.Text = cfg.ExePath ?? "";
                ArgsInput.Text = cfg.Arguments ?? "";
                UrlInput.Text = cfg.PanelUrl ?? "";
                AutoRunCheck.Checked = cfg.AutoRunOnStart;
                AutoMinCheck.Checked = cfg.AutoMinimize;
                // AntdUI 的 CheckboxChanged 可能在这一轮之后才补发，_loading 已经复位挡不住；
                // 记下「界面上刚显示的到底是什么值」，处理器里比对未变就当没发生。
                _savedAutoRun = cfg.AutoRunOnStart;
                _savedAutoMin = cfg.AutoMinimize;
            }
            finally
            {
                _loading = false;
            }
            UpdateStatus();
        }

        public LauncherConfig Collect()
        {
            LauncherConfig cfg = _host.Config;
            cfg.ExePath = Clean(ExePathInput.Text);
            cfg.Arguments = OrDefault(Clean(ArgsInput.Text), LauncherConfig.DefaultArguments);
            cfg.PanelUrl = OrDefault(Clean(UrlInput.Text), LauncherConfig.DefaultPanelUrl);
            cfg.AutoMinimize = AutoMinCheck.Checked;
            cfg.AutoRunOnStart = AutoRunCheck.Checked;
            return cfg;
        }

        public void FocusPathInput()
        {
            ExePathInput.Focus();
        }

        public void WriteConsole(string line, string prefix)
        {
            if (line == null) return;
            Console.Info(" " + line, prefix ?? "");
        }

        /// <summary>
        /// 网关进程的输出。Go 的默认 logger 走 stderr，所以 [err] 不等于出错，
        /// 只有正文里真出现错误关键字才标红。
        /// </summary>
        public void WriteGatewayLog(string line)
        {
            if (line == null) return;
            string body = line;
            string kind = "";
            if (line.StartsWith("[err]")) { body = Body(line, 5); kind = "err"; }
            else if (line.StartsWith("[warn]")) { body = Body(line, 6); kind = "warn"; }
            else if (line.StartsWith("[info]")) { body = Body(line, 6); kind = "info"; }
            else if (line.StartsWith("[out]")) { body = Body(line, 5); kind = "out"; }
            body = body.TrimStart();

            if (kind == "warn") Console.Warn(" " + body, "");
            else if (kind == "err" && LooksFatal(body)) Console.Error(" " + body, "");
            else Console.Info(" " + body, "");
        }

        private static bool LooksFatal(string text)
        {
            if (StringUtil.IsEmpty(text)) return false;
            string lower = text.ToLowerInvariant();
            string keywords = "error|panic|fatal|exception|failed|refused|超时|失败|错误";
            string[] parts = keywords.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                if (lower.Contains(parts[i])) return true;
            }
            return false;
        }

        public void ApplyGatewayState(GatewayState state, string message)
        {
            UpdateStatus();
            WriteConsole(message, "");
        }

        public void RefreshStates()
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            GatewayRunner runner = _host.Runner;
            GatewayState state = runner.State;
            string suffix = "";
            if (state == GatewayState.Running && runner.ProcessId > 0) suffix = "  PID " + runner.ProcessId;
            StatusLabel.Text = "状态：" + runner.StateText + suffix;
            StartBtn.Enabled = (state != GatewayState.Running && state != GatewayState.Starting && state != GatewayState.External);
            StopBtn.Enabled = (state == GatewayState.Running || state == GatewayState.External || state == GatewayState.Starting);
        }

        #region 按钮事件

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择 wb2api.exe";
            dlg.Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            string initial = InitialDirectory();
            if (StringUtil.IsNotEmpty(initial)) dlg.InitialDirectory = initial;
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            ExePathInput.Text = dlg.FileName;
            if (StringUtil.IsEmpty(Clean(ArgsInput.Text))) ArgsInput.Text = LauncherConfig.DefaultArguments;
            string directory = Path.GetDirectoryName(dlg.FileName) ?? "";
            string configFile = Path.Combine(directory, "config.json");
            if (File.Exists(configFile)) WriteConsole("检测到同目录下的 config.json，网关将按它监听端口。", "");
            else WriteConsole("注意：" + directory + " 下没有 config.json，请确认参数正确。", "");

            SaveBtn_Click(sender, e);
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            Collect();
            _host.SaveConfig();
            Toast.Success("配置已保存");
            UpdateStatus();
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            LauncherConfig cfg = Collect();
            if (!cfg.HasValidExePath)
            {
                Toast.Warn("请先选择有效的 wb2api.exe 路径。");
                FocusPathInput();
                return;
            }
            cfg.Save();
            string error = _host.StartGateway();
            if (StringUtil.IsNotEmpty(error)) Toast.Error(error);
            else Toast.Success("已静默启动网关，正在等待端口就绪…");
            UpdateStatus();
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            _host.StopGateway();
            UpdateStatus();
        }

        private void OpenPanelBtn_Click(object sender, EventArgs e)
        {
            Collect();
            _host.OpenPanel();
        }

        private void HealthBtn_Click(object sender, EventArgs e)
        {
            _host.CheckHealth();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            Console.Clear();
        }

        private void AutoRunCheck_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (_loading || AutoRunCheck.Checked == _savedAutoRun) return;
            bool wanted = AutoRunCheck.Checked;
            string error = wanted ? AutostartUtil.Enable() : AutostartUtil.Disable();
            if (StringUtil.IsNotEmpty(error))
            {
                Toast.Error(error);
                _loading = true;
                AutoRunCheck.Checked = _savedAutoRun;
                _loading = false;
                return;
            }
            _savedAutoRun = wanted;
            LauncherConfig cfg = Collect();
            cfg.Save();
            WriteConsole(wanted ? "开机自启已开启：" + AutostartUtil.CurrentCommand() : "开机自启已关闭。", "");
        }

        private void AutoMinCheck_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (_loading || AutoMinCheck.Checked == _savedAutoMin) return;
            _savedAutoMin = AutoMinCheck.Checked;
            LauncherConfig cfg = Collect();
            cfg.Save();
            WriteConsole("启动后最小化到托盘：" + (AutoMinCheck.Checked ? "开" : "关"), "");
        }

        #endregion

        private string InitialDirectory()
        {
            string path = Clean(ExePathInput.Text);
            if (StringUtil.IsNotEmpty(path))
            {
                string dir = Path.GetDirectoryName(path);
                if (StringUtil.IsNotEmpty(dir) && Directory.Exists(dir)) return dir;
            }
            string appPath = App.Path;
            return StringUtil.IsEmpty(appPath) ? Environment.CurrentDirectory : appPath;
        }

        private static string Body(string line, int skip)
        {
            if (line.Length <= skip) return "";
            return line.Substring(skip).TrimStart();
        }

        private static string Clean(string text)
        {
            return text == null ? "" : text.Trim();
        }

        private static string OrDefault(string text, string fallback)
        {
            return StringUtil.IsEmpty(text) ? fallback : text;
        }
    }
}
