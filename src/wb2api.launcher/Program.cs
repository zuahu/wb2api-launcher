using System;
using System.Threading;
using System.Windows.Forms;
using my.common;
using my.antd.common;
using wb2api.launcher.Services;
using wb2api.launcher.UI;

namespace wb2api.launcher
{
    static class Program
    {
        public const string SelfTestArgument = "--selftest";
        public const string SingleMutexName = "WB2ApiLauncher.SingleInstance";

        private static Mutex _singleMutex;

        public static MainForm Form { get { return App.MainForm as MainForm; } }

        [STAThread]
        static void Main(string[] args)
        {
            if (ContainsArgument(args, SelfTestArgument))
            {
                Environment.ExitCode = SelfTest.Run();
                return;
            }

            // 已在托盘里就唤起那个实例，本进程立刻退出（放在框架初始化之前，避免留下挂住的空进程）
            if (!TryBecomeSingleInstance()) return;

            bool minimized = ContainsArgument(args, MainForm.MinimizeArgument);
            MainForm.StartMinimized = minimized;

            new NoneLoginApplication(BuildAppInfo()).Start(delegate { return new MainForm(); });
        }

        /// <summary>自己做的单实例：框架自带的唤起逻辑对「已隐藏到托盘」的窗口无效。</summary>
        internal static bool TryBecomeSingleInstance()
        {
            bool createdNew;
            try
            {
                _singleMutex = new Mutex(true, SingleMutexName, out createdNew);
            }
            catch (Exception)
            {
                return true;
            }
            if (createdNew) return true;
            InstanceActivator.NotifyOtherInstances();
            return false;
        }

        /// <summary>应用信息抽成方法，便于 --selftest 直接断言标题栏副标题为空。</summary>
        internal static AppInfo BuildAppInfo()
        {
            AppInfo info = new AppInfo();
            info.Name = "WorkBuddy2API 启动器";
            info.Version = "1.0.0.0";
            info.AppIconPath = "wb2api.launcher.app.ico";
            info.CompanyName = "";
            info.CompanyContact = "";
            // 单实例由 TryBecomeSingleInstance 负责（框架自带的唤起对隐藏到托盘的窗口无效）
            info.IsAllowMutiInstance = true;
            return info;
        }

        internal static bool ContainsArgument(string[] args, string wanted)
        {
            if (args == null) return false;
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], wanted, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
