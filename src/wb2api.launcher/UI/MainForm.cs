using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using my.antd.ui;
using my.common;
using wb2api.launcher.Services;

namespace wb2api.launcher.UI
{
    public partial class MainForm : AbstractForm
    {
        public const string MinimizeArgument = "--minimized";

        public BodyForm BodyForm { get; private set; }
        public LauncherConfig Config { get; private set; }
        public GatewayRunner Runner { get; private set; }

        /// <summary>由 Program.cs 在启动前写入：命令行带 --minimized 时强制最小化到托盘。</summary>
        public static bool StartMinimized { get; set; }

        private NotifyIcon _tray;
        private ToolStripMenuItem _startItem;
        private ToolStripMenuItem _stopItem;
        private bool _reallyExit;
        private bool _trayTipShown;
        private bool _cleanedUp;
        private Rectangle _normalBounds = Rectangle.Empty;

        private const string AppTitle = "WorkBuddy2API 启动器";

        public MainForm()
        {
            InitializeComponent();
            // 还原时按这个尺寸摆放，句柄重建后控件的 Bounds 未必可信，所以先记下设计期尺寸
            _normalBounds = new Rectangle(0, 0, this.Size.Width, this.Size.Height);
            this.Config = LauncherConfig.Load();
            this.Runner = new GatewayRunner();
            this.Runner.LogHandler = new Action<string>(OnGatewayLog);
            this.Runner.StateHandler = new Action<GatewayState, string>(OnGatewayState);
            InitTray();
            this.Shown += OnShownOnce;
        }

        private void OnShownOnce(object sender, EventArgs e)
        {
            BodyForm = new BodyForm(this);
            BodyForm.Dock = DockStyle.Fill;
            BodyForm.FormBorderStyle = FormBorderStyle.None;
            BodyForm.TopLevel = false;
            this.FormPanel.Controls.Add(BodyForm);
            BodyForm.Show();
            BodyForm.ApplyConfig(this.Config);
            BodyForm.WriteConsole("启动器已就绪。配置文件：" + ConfigFileHint(), "");

            if (!Config.HasValidExePath)
            {
                BringWindowToFront();
                Toast.Warn("请先选择 wb2api.exe 的路径后再启动服务。");
                Balloon("尚未配置启动文件", "请点击「浏览」选择 wb2api.exe，保存后点「启动服务」。", ToolTipIcon.Warning);
                BodyForm.FocusPathInput();
                return;
            }

            string error = StartGateway();
            if (StringUtil.IsNotEmpty(error))
            {
                BringWindowToFront();
                Toast.Error(error);
                return;
            }

            if (ShouldMinimizeOnStart())
            {
                BodyForm.WriteConsole("按设置直接收进托盘运行，双击托盘图标可回到本界面。", "");
                MinimizeToTray();
            }
        }

        /// <summary>命令行指定 --minimized，或配置里勾选了「启动后最小化」。</summary>
        public bool ShouldMinimizeOnStart()
        {
            return StartMinimized || (Config != null && Config.AutoMinimize);
        }

        public static bool ShouldAllowClose(bool reallyExit)
        {
            return reallyExit;
        }

        public string StartGateway()
        {
            return Runner.Start(Config);
        }

        public void StopGateway()
        {
            Runner.Stop();
        }

        public void SaveConfig()
        {
            Config.Save();
            BodyForm.WriteConsole("配置已保存到 cfg.ini 的 [Launcher] 段。", "");
        }

        public void OpenPanel()
        {
            OpenUrl(Config.PanelUrl);
        }

        public static void OpenUrl(string url)
        {
            if (StringUtil.IsEmpty(url)) return;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = url;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Toast.Error("打开浏览器失败：" + ex.Message);
            }
        }

        public void CheckHealth()
        {
            string text = GatewayRunner.FetchHealth(Config);
            BodyForm.WriteConsole("健康检查 /healthz → " + text, "");
            if (text.StartsWith("错误")) Toast.Warn("网关未响应：" + Shorten(text, 40));
            else Toast.Success("网关响应正常");
        }

        public void MinimizeToTray()
        {
            if (_tray == null) return;
            if (this.WindowState == FormWindowState.Normal && this.Width > 0 && this.Height > 0)
            {
                _normalBounds = this.DesktopBounds;
            }
            // 收进托盘一律走 Win32：托管的 Hide()/ShowInTaskbar 会重建窗口句柄，
            // 并把还原位置写成最小化的 -32000,-32000，之后 Show() 出来的窗口停在屏幕外面看不见。
            if (this.IsHandleCreated) ShowWindow(this.Handle, SW_HIDE);
            if (_tray != null) _tray.Visible = true;
        }

        /// <summary>
        /// 从托盘里把窗口叫回来。托管属性在这里不可靠：隐藏 + 最小化 + ShowInTaskbar 会重建句柄，
        /// WindowState/Bounds 的赋值会被丢掉（实测窗口停在 -32000,-32000 看不见），
        /// 所以显示、还原、定位、抢前台一律直接走 Win32。
        /// </summary>
        public void BringWindowToFront()
        {
            Rectangle bounds = ResolveVisibleBounds(_normalBounds, WorkArea());
            this.Show();
            if (!this.IsHandleCreated) return;
            // 显示、定位、抢前台一律走 Win32：托管属性在「隐藏过再显示」这条路上不可靠，
            // 实测窗口会停在 -32000,-32000 的屏外坐标，用户看到的就是「双击没反应」。
            ShowWindow(this.Handle, SW_RESTORE);
            SetWindowPos(this.Handle, IntPtr.Zero,
                bounds.Left, bounds.Top, bounds.Width, bounds.Height,
                SWP_SHOWWINDOW | SWP_NOZORDER);
            _normalBounds = bounds;
            // TOPMOST 再取消 TOPMOST 是「把窗口带到最前」的通用做法，单靠 Activate()
            // 从后台进程唤不起来（用户点了图标却没反应）。
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            SetForegroundWindow(this.Handle);
            this.Activate();
        }

        private const int SW_HIDE = 0;
        private const int SW_RESTORE = 9;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>只修正位置、保留原尺寸：最小化前记录的坐标可能停在 -32000，或落在已经拔掉的副屏上。</summary>
        public static Rectangle ResolveVisibleBounds(Rectangle saved, Rectangle workArea)
        {
            int width = Math.Min(saved.Width > 0 ? saved.Width : workArea.Width, workArea.Width);
            int height = Math.Min(saved.Height > 0 ? saved.Height : workArea.Height, workArea.Height);
            int x = saved.X;
            int y = saved.Y;
            if (x < workArea.Left || x + width > workArea.Right
                || y < workArea.Top || y + height > workArea.Bottom)
            {
                x = workArea.Left + (workArea.Width - width) / 2;
                y = workArea.Top + (workArea.Height - height) / 2;
            }
            return new Rectangle(x, y, width, height);
        }

        /// <summary>取主屏工作区：窗口此刻可能正停在 -32000 的屏外坐标，不能拿它自己来定位。</summary>
        public static Rectangle WorkArea()
        {
            try
            {
                Rectangle area = Screen.PrimaryScreen.WorkingArea;
                if (area.Width > 0 && area.Height > 0) return area;
            }
            catch (Exception) { }
            return new Rectangle(0, 0, 1280, 1024);
        }

        public void WriteConsole(string line)
        {
            if (BodyForm != null) BodyForm.WriteConsole(line, "");
        }

        public void RefreshStates()
        {
            if (BodyForm != null) BodyForm.RefreshStates();
        }

        private string ConfigFileHint()
        {
            try { return System.IO.Path.Combine(App.Path ?? "", "cfg.ini"); }
            catch (Exception) { return "cfg.ini"; }
        }

        #region 托盘

        private void InitTray()
        {
            ContextMenuStrip menu = new ContextMenuStrip(this.components);
            ToolStripMenuItem openItem = new ToolStripMenuItem("显示主界面");
            openItem.Click += delegate(object s, EventArgs e) { BringWindowToFront(); };
            _startItem = new ToolStripMenuItem("启动服务");
            _startItem.Click += delegate(object s, EventArgs e)
            {
                string error = StartGateway();
                if (StringUtil.IsNotEmpty(error)) Toast.Error(error);
            };
            _stopItem = new ToolStripMenuItem("停止服务");
            _stopItem.Click += delegate(object s, EventArgs e) { StopGateway(); };
            ToolStripMenuItem panelItem = new ToolStripMenuItem("打开管理面板");
            panelItem.Click += delegate(object s, EventArgs e) { OpenPanel(); };
            ToolStripMenuItem healthItem = new ToolStripMenuItem("检查网关状态");
            healthItem.Click += delegate(object s, EventArgs e) { CheckHealth(); };
            ToolStripMenuItem exitItem = new ToolStripMenuItem("退出启动器");
            exitItem.Click += delegate(object s, EventArgs e) { ExitApp(); };

            menu.Items.Add(openItem);
            menu.Items.Add(_startItem);
            menu.Items.Add(_stopItem);
            menu.Items.Add(panelItem);
            menu.Items.Add(healthItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);
            menu.ShowImageMargin = false;

            _tray = new NotifyIcon(this.components);
            _tray.Icon = ResolveTrayIcon();
            _tray.Text = AppTitle;
            _tray.ContextMenuStrip = menu;
            _tray.Visible = true;
            _tray.DoubleClick += delegate(object s, EventArgs e) { BringWindowToFront(); };
            _tray.MouseClick += delegate(object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left) BringWindowToFront();
            };
            UpdateTrayText(GatewayState.Stopped);
        }

        private Icon ResolveTrayIcon()
        {
            if (this.Icon != null) return this.Icon;
            if (App.AppIcon != null) return App.AppIcon;
            try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch (Exception) { return SystemIcons.Application; }
        }

        private void UpdateTrayText(GatewayState state)
        {
            if (_tray == null) return;
            string text = AppTitle + " · " + Runner.StateText;
            if (text.Length > 60) text = text.Substring(0, 60);
            _tray.Text = text;
            if (_startItem != null) _startItem.Enabled = (state != GatewayState.Running && state != GatewayState.Starting);
            if (_stopItem != null) _stopItem.Enabled = (state == GatewayState.Running || state == GatewayState.External);
        }

        private void Balloon(string title, string text, ToolTipIcon icon)
        {
            if (_tray == null) return;
            try { _tray.ShowBalloonTip(4000, title, Shorten(text, 180), icon); }
            catch (Exception) { }
        }

        #endregion

        #region 网关回调

        private void OnGatewayLog(string line)
        {
            if (BodyForm == null) return;
            BodyForm.WriteGatewayLog(line);
        }

        private void OnGatewayState(GatewayState state, string message)
        {
            Action apply = delegate
            {
                BodyForm.ApplyGatewayState(state, message);
                UpdateTrayText(state);
                if (state == GatewayState.Running && !_trayTipShown)
                {
                    _trayTipShown = true;
                    Balloon(AppTitle, "网关服务已启动，面板地址 " + Config.PanelUrl, ToolTipIcon.Info);
                }
                else if (state == GatewayState.Failed)
                {
                    Balloon(AppTitle + " 启动失败", message, ToolTipIcon.Error);
                }
                else if (state == GatewayState.External)
                {
                    Balloon(AppTitle, message, ToolTipIcon.Info);
                }
            };
            if (this.IsHandleCreated) this.UpdateUI(new UIUpdateMethod(delegate(object[] args) { apply(); }), null, false);
            else apply();
        }

        #endregion

        #region 关闭与退出

        /// <summary>点关闭按钮时最小化到托盘，只有托盘菜单「退出」才真正结束进程。</summary>
        protected override bool IsCanClose()
        {
            if (ShouldAllowClose(_reallyExit)) return true;
            MinimizeToTray();
            Balloon(AppTitle, "已最小化到托盘，网关服务继续运行。双击托盘图标可回到主界面。", ToolTipIcon.Info);
            return false;
        }

        public void ExitApp()
        {
            BringWindowToFront();
            if (Runner.IsReady)
            {
                bool stopTogether = Dialog.Confirm("退出前是否一并停止网关服务？\n选择「否」，服务将继续在后台运行。", "退出确认", this);
                if (stopTogether) Runner.ShutdownIfOwned();
            }
            _reallyExit = true;
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Cleanup();
            base.OnFormClosed(e);
        }

        /// <summary>第二个实例双击图标时发来的唤起消息：和双击托盘图标走同一条路径。</summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == InstanceActivator.ShowMessageId)
            {
                BringWindowToFront();
                return;
            }
            base.WndProc(ref m);
        }

        private void Cleanup()
        {
            if (_cleanedUp) return;
            _cleanedUp = true;
            if (_tray != null)
            {
                try { _tray.Visible = false; } catch (Exception) { }
            }
        }

        #endregion

        private static string Shorten(string text, int max)
        {
            if (text == null) return "";
            if (text.Length <= max) return text;
            return text.Substring(0, max - 1) + "…";
        }
    }
}
