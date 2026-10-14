using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using my.common;
using wb2api.launcher.Services;
using wb2api.launcher.UI;

namespace wb2api.launcher
{
    /// <summary>
    /// 无消息循环自检：直接断言配置读写、端口探测、静默启动等关键能力。
    /// 不使用 Toast / Dialog（没有消息循环时行为不可预期），结果写入 selftest.log。
    /// </summary>
    internal static class SelfTest
    {
        private static int _pass;
        private static int _fail;
        private static readonly StringBuilder _buf = new StringBuilder();

        public static int Run()
        {
            _pass = 0;
            _fail = 0;
            _buf.Length = 0;
            try { Execute(); }
            catch (Exception ex) { Check("未捕获异常", false, ex.ToString()); }

            string summary = "===== 通过 " + _pass + " 项，失败 " + _fail + " 项 =====";
            _buf.AppendLine(summary);
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selftest.log");
                File.WriteAllText(logPath, _buf.ToString(), new UTF8Encoding(false));
            }
            catch (Exception) { }
            try { System.Console.Out.Write(_buf.ToString()); System.Console.Out.Flush(); }
            catch (Exception) { }
            return _fail;
        }

        private static void Execute()
        {
            IconResource();
            AppInfoSettings();
            ArgumentParsing();
            CloseGate();
            TrayRestoreBounds();
            ConfigDefaults();
            ConfigRoundTrip();
            HealthEndpointParsing();
            PortProbe();
            SilentProcessLaunch();
            RunnerGuards();
            ConstantsFromSplit();
            AutostartCommand();
            AutostartRegistry();
            HealthFetchFailure();
        }

        private static void IconResource()
        {
            Assembly assembly = typeof(Program).Assembly;
            string[] names = assembly.GetManifestResourceNames();
            bool found = false;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].EndsWith("app.ico", StringComparison.OrdinalIgnoreCase)) found = true;
            }
            Check("图标已作为嵌入资源", found, "资源清单=" + string.Join(",", names) + "；AppIconPath=" + Program.BuildAppInfo().AppIconPath);
            Stream stream = assembly.GetManifestResourceStream(Program.BuildAppInfo().AppIconPath);
            Check("按 AppIconPath 能取到图标流", stream != null, stream == null ? "取不到 wb2api.launcher.app.ico" : "字节数=" + stream.Length);
            if (stream != null) stream.Close();
        }

        private static void AppInfoSettings()
        {
            AppInfo info = Program.BuildAppInfo();
            Check("软件名非空", StringUtil.IsNotEmpty(info.Name), info.Name);
            Check("CompanyContact 留空（避免拼进标题副标题）", string.IsNullOrEmpty(info.CompanyContact), "[" + info.CompanyContact + "]");
            Check("CompanyName 留空", string.IsNullOrEmpty(info.CompanyName), "[" + info.CompanyName + "]");
            Check("AppIconPath 非空（留空会启动即崩）", StringUtil.IsNotEmpty(info.AppIconPath), info.AppIconPath);
            Check("单实例交给自己做（框架唤起对隐藏窗口无效）", info.IsAllowMutiInstance, "IsAllowMutiInstance=" + info.IsAllowMutiInstance);
            Check("单实例互斥名非空", StringUtil.IsNotEmpty(Program.SingleMutexName), Program.SingleMutexName);
            Check("唤起消息号注册成功", InstanceActivator.IsUsableMessageId(), "msg=" + InstanceActivator.ShowMessageId);
            Check("唤起按标题关键字找窗口", StringUtil.IsNotEmpty(InstanceActivator.TitleKeyword), InstanceActivator.TitleKeyword);
        }

        private static void ArgumentParsing()
        {
            Check("识别 --minimized", Program.ContainsArgument(new string[] { "x", MainForm.MinimizeArgument }, MainForm.MinimizeArgument), "");
            Check("参数大小写不敏感", Program.ContainsArgument(new string[] { "--Minimized" }, MainForm.MinimizeArgument), "");
            Check("null 参数不抛异常", !Program.ContainsArgument(null, MainForm.MinimizeArgument), "");
            Check("未传参时不最小化", !MainForm.StartMinimized, "StartMinimized=" + MainForm.StartMinimized);
            Check("自检参数不冲突", Program.ContainsArgument(new string[] { Program.SelfTestArgument }, Program.SelfTestArgument), "");
        }

        private static void CloseGate()
        {
            Check("关闭按钮默认不真正退出（收进托盘）", !MainForm.ShouldAllowClose(false), "");
            Check("托盘退出才允许关闭", MainForm.ShouldAllowClose(true), "");
        }

        private static void TrayRestoreBounds()
        {
            Rectangle work = new Rectangle(0, 0, 1920, 1040);
            Rectangle off = MainForm.ResolveVisibleBounds(new Rectangle(-32000, -32000, 960, 640), work);
            Check("屏外坐标会被拉回工作区", work.Contains(off.Location), off.ToString());
            Check("拉回后尺寸不变", off.Width == 960 && off.Height == 640, off.ToString());

            Rectangle normal = new Rectangle(180, 120, 960, 640);
            Check("正常位置原样保留", MainForm.ResolveVisibleBounds(normal, work) == normal,
                  MainForm.ResolveVisibleBounds(normal, work).ToString());

            Rectangle right = MainForm.ResolveVisibleBounds(new Rectangle(4000, 100, 960, 640), work);
            Check("副屏拔掉后的旧坐标也能拉回", work.Contains(right.Location), right.ToString());

            Rectangle never = MainForm.ResolveVisibleBounds(Rectangle.Empty, work);
            Check("从未最小化时用工作区尺寸", never.Width == work.Width && never.Height == work.Height, never.ToString());

            Rectangle small = MainForm.ResolveVisibleBounds(new Rectangle(-32000, -32000, 4000, 3000), work);
            Check("窗口大于屏幕时不越界", small.Width <= work.Width && small.Height <= work.Height, small.ToString());

            Check("主屏工作区可用", MainForm.WorkArea().Width > 0 && MainForm.WorkArea().Height > 0,
                  MainForm.WorkArea().ToString());
        }

        private static void ConfigDefaults()
        {
            App.Config = null;
            LauncherConfig cfg = LauncherConfig.Load();
            Check("App.Config 为 null 时不抛异常并回默认参数", cfg.Arguments == LauncherConfig.DefaultArguments, cfg.Arguments);
            Check("默认面板地址", cfg.PanelUrl == LauncherConfig.DefaultPanelUrl, cfg.PanelUrl);
            Check("默认不最小化（双击图标必须看得见界面）", !cfg.AutoMinimize, "" + cfg.AutoMinimize);
            Check("默认不开机自启（绝不静默写注册表）", !cfg.AutoRunOnStart, "" + cfg.AutoRunOnStart);
            Check("未配置路径时无效", !cfg.HasValidExePath, "[" + cfg.ExePath + "]");
            Check("工作目录回退到程序目录且存在", Directory.Exists(cfg.WorkDirectory), cfg.WorkDirectory);
            Check("配置段名固定为 Launcher", LauncherConfig.Section == "Launcher", LauncherConfig.Section);
        }

        private static void ConfigRoundTrip()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selftest_tmp");
            Directory.CreateDirectory(dir);
            string iniPath = Path.Combine(dir, "cfg.roundtrip.ini");
            if (File.Exists(iniPath)) File.Delete(iniPath);
            File.WriteAllText(iniPath, "", new UTF8Encoding(false));
            string exePath = Path.Combine(dir, "wb2api.exe");
            File.WriteAllText(exePath, "fake", new UTF8Encoding(false));

            App.Config = new Config(iniPath);
            LauncherConfig cfg = LauncherConfig.Load();
            cfg.ExePath = exePath;
            cfg.Arguments = "-config cfg.test.json";
            cfg.PanelUrl = "http://192.168.1.9:8000/panel/";
            cfg.AutoMinimize = true;
            cfg.AutoRunOnStart = true;
            cfg.Save();

            App.Config = new Config(iniPath);
            LauncherConfig reloaded = LauncherConfig.Load();
            Check("路径写入 cfg.ini 后能读回", reloaded.ExePath == exePath, reloaded.ExePath);
            Check("参数能读回", reloaded.Arguments == "-config cfg.test.json", reloaded.Arguments);
            Check("面板地址能读回", reloaded.PanelUrl == "http://192.168.1.9:8000/panel/", reloaded.PanelUrl);
            Check("最小化开关能读回（true）", reloaded.AutoMinimize, "" + reloaded.AutoMinimize);
            Check("自启开关能读回（true）", reloaded.AutoRunOnStart, "" + reloaded.AutoRunOnStart);
            Check("有效路径判定", reloaded.HasValidExePath, "");
            Check("工作目录取 exe 所在目录", reloaded.WorkDirectory == dir, reloaded.WorkDirectory);

            App.Config = null;
            try { Directory.Delete(dir, true); } catch (Exception) { }
        }

        private static void HealthEndpointParsing()
        {
            LauncherConfig cfg = new LauncherConfig();
            cfg.PanelUrl = "http://192.168.2.223:7863/panel/";
            Check("从面板地址解析主机", cfg.HealthHost == "192.168.2.223", cfg.HealthHost);
            Check("从面板地址解析端口", cfg.HealthPort == 7863, "" + cfg.HealthPort);

            cfg.PanelUrl = "不是地址";
            Check("非法地址回退本机", cfg.HealthHost == "127.0.0.1", cfg.HealthHost);
            Check("非法地址回退默认端口", cfg.HealthPort == 7863, "" + cfg.HealthPort);

            cfg.PanelUrl = "http://localhost/panel/";
            Check("无端口时用默认端口", cfg.HealthPort == 7863, "" + cfg.HealthPort);
            Check("无端口时主机为 localhost", cfg.HealthHost == "localhost", cfg.HealthHost);
        }

        private static void PortProbe()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.BeginAcceptTcpClient(new AsyncCallback(delegate(IAsyncResult ar)
            {
                try { listener.EndAcceptTcpClient(ar).Close(); } catch (Exception) { }
            }), null);

            bool open = GatewayRunner.IsPortOpen("127.0.0.1", port, 1200);
            listener.Stop();
            bool closed = GatewayRunner.IsPortOpen("127.0.0.1", port, 600);
            Check("端口占用时探测为已就绪", open, "port=" + port);
            Check("端口释放后探测为未就绪", !closed, "port=" + port);
            Check("非法端口不探测", !GatewayRunner.IsPortOpen("127.0.0.1", 0, 200), "");
        }

        private static void SilentProcessLaunch()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c echo SILENT_OK";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = new UTF8Encoding(false);
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Process process = Process.Start(psi);
            string captured = process.StandardOutput.ReadToEnd();
            bool exited = process.WaitForExit(8000);
            int code = exited ? process.ExitCode : -1;
            process.Close();

            Check("静默启动子进程（无控制台窗口）", exited && captured.Contains("SILENT_OK"),
                  "exited=" + exited + " code=" + code + " out=[" + captured.Trim() + "]");
        }

        private static void RunnerGuards()
        {
            GatewayRunner runner = new GatewayRunner();
            Check("初始状态为已停止", runner.State == GatewayState.Stopped, runner.StateText);
            Check("初始不可视为就绪", !runner.IsReady, "");
            Check("停止空进程返回无错误", runner.Stop() == "", "");

            LauncherConfig empty = new LauncherConfig();
            string error1 = new GatewayRunner().Start(empty);
            Check("未配置路径时拒绝启动", StringUtil.IsNotEmpty(error1), error1);

            LauncherConfig missing = new LauncherConfig();
            missing.ExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "not_exists_xyz.exe");
            string error2 = new GatewayRunner().Start(missing);
            Check("文件不存在时拒绝启动", error2.StartsWith("找不到文件"), error2);
        }

        private static void ConstantsFromSplit()
        {
            string[] parts = LauncherConfig.DefaultArguments.Split(' ');
            Check("默认参数按空格解析", parts.Length == 2 && parts[0] == "-config" && parts[1] == "config.json",
                  LauncherConfig.DefaultArguments + " → " + string.Join("|", parts));
            Check("默认端口常量来自解析而非数组", LauncherConfigSectionPort() == 7863, "");
        }

        private static int LauncherConfigSectionPort()
        {
            LauncherConfig cfg = new LauncherConfig();
            cfg.PanelUrl = LauncherConfig.DefaultPanelUrl;
            return cfg.HealthPort;
        }

        private static void AutostartCommand()
        {
            string command = AutostartUtil.BuildCommand("C:\\My Tools\\wb2api.launcher.exe");
            Check("自启命令带引号包裹路径", command.StartsWith("\"C:\\My Tools"), command);
            Check("自启命令附带最小化参数", command.EndsWith(MainForm.MinimizeArgument), command);
            Check("Run 键路径非空", StringUtil.IsNotEmpty(AutostartUtil.RunKeyPath), AutostartUtil.RunKeyPath);
            Check("Run 键值名非空", StringUtil.IsNotEmpty(AutostartUtil.ValueName), AutostartUtil.ValueName);
        }

        /// <summary>真实读写 HKCU Run 键，最后把原值恢复回去。</summary>
        private static void AutostartRegistry()
        {
            string before = AutostartUtil.ReadValue();
            string error = AutostartUtil.Enable();
            string written = AutostartUtil.ReadValue();
            Check("开机自启写入 Run 键无错误", error == "", error);
            Check("Run 键里能读回命令", written.Contains(MainForm.MinimizeArgument), written);
            Check("IsEnabled 能识别刚写入的值", AutostartUtil.IsEnabled(), "");

            string closeError = AutostartUtil.Disable();
            Check("关闭自启无错误", closeError == "", closeError);
            Check("关闭后 Run 键已清空", StringUtil.IsEmpty(AutostartUtil.ReadValue()), AutostartUtil.ReadValue());
            Check("关闭后 IsEnabled 为 false", !AutostartUtil.IsEnabled(), "");

            AutostartUtil.Restore(StringUtil.IsEmpty(before) ? null : before);
            Check("自启测试已恢复注册表原状", AutostartUtil.ReadValue() == before, "[" + AutostartUtil.ReadValue() + "]");
        }

        private static void HealthFetchFailure()
        {
            LauncherConfig cfg = new LauncherConfig();
            cfg.PanelUrl = "http://127.0.0.1:1/panel/";
            string text = GatewayRunner.FetchHealth(cfg);
            Check("端口无人监听时健康检查报错", text.StartsWith("错误"), text);
        }

        private static void Check(string name, bool ok, string detail)
        {
            if (ok) _pass++;
            else _fail++;
            string line = (ok ? "[OK]   " : "[FAIL] ") + name + (StringUtil.IsEmpty(detail) ? "" : "  " + detail);
            _buf.AppendLine(line);
        }
    }
}
