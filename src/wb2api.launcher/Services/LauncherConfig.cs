using System;
using System.IO;
using my.common;

namespace wb2api.launcher.Services
{
    /// <summary>启动器配置，落在框架的 cfg.ini（section = Launcher）。</summary>
    public class LauncherConfig
    {
        public const string Section = "Launcher";
        public const string DefaultArguments = "-config config.json";
        public const string DefaultPanelUrl = "http://127.0.0.1:7863/panel/";

        public string ExePath = "";
        public string Arguments = DefaultArguments;
        public string PanelUrl = DefaultPanelUrl;
        /// <summary>改注册表属于系统级动作，绝不能是「配置缺失时静默发生」的默认值。</summary>
        public bool AutoRunOnStart = false;
        /// <summary>只在开机自启（--minimized）之外的手动双击时也生效，所以默认关闭：双击图标必须看得见界面。</summary>
        public bool AutoMinimize = false;

        public static LauncherConfig Load()
        {
            LauncherConfig cfg = new LauncherConfig();
            Config ini = App.Config;
            if (ini == null) return cfg;
            cfg.ExePath = Trim(ini.GetValue("ExePath", Section));
            cfg.Arguments = OrDefault(Trim(ini.GetValue("Arguments", Section)), DefaultArguments);
            cfg.PanelUrl = OrDefault(Trim(ini.GetValue("PanelUrl", Section)), DefaultPanelUrl);
            cfg.AutoRunOnStart = ini.GetBool("AutoRunOnStart", Section, false);
            cfg.AutoMinimize = ini.GetBool("AutoMinimize", Section, false);
            return cfg;
        }

        public void Save()
        {
            Config ini = App.Config;
            if (ini == null) return;
            ini.SetValue("ExePath", ExePath ?? "", Section);
            ini.SetValue("Arguments", Arguments ?? "", Section);
            ini.SetValue("PanelUrl", PanelUrl ?? "", Section);
            ini.SetValue("AutoRunOnStart", AutoRunOnStart ? "Y" : "N", Section);
            ini.SetValue("AutoMinimize", AutoMinimize ? "Y" : "N", Section);
        }

        public bool HasValidExePath
        {
            get { return StringUtil.IsNotEmpty(ExePath) && File.Exists(ExePath); }
        }

        /// <summary>被启动程序的工作目录：它自己的目录（网关按相对路径找 config.json / auths / data）。</summary>
        public string WorkDirectory
        {
            get { return GetWorkDirectory(ExePath); }
        }

        public static string GetWorkDirectory(string exePath)
        {
            if (StringUtil.IsEmpty(exePath)) return Application_StartupPath();
            string dir = Path.GetDirectoryName(exePath);
            return StringUtil.IsNotEmpty(dir) ? dir : Application_StartupPath();
        }

        private static string Application_StartupPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>面板地址里的主机与端口，用于「服务是否已在监听」判活。</summary>
        public string HealthHost
        {
            get
            {
                Uri uri;
                if (Uri.TryCreate(OrDefault(PanelUrl, DefaultPanelUrl), UriKind.Absolute, out uri))
                {
                    if (uri.HostNameType == UriHostNameType.IPv6) return "127.0.0.1";
                    return uri.Host;
                }
                return "127.0.0.1";
            }
        }

        public int HealthPort
        {
            get
            {
                string url = OrDefault(PanelUrl, DefaultPanelUrl);
                Uri uri;
                // 只有地址里显式写了端口才采用；否则 Uri 会补 http 默认 80，探测就打错地方了。
                if (Uri.TryCreate(url, UriKind.Absolute, out uri)
                    && HasExplicitPort(url) && uri.Port > 0 && uri.Port <= 65535) return uri.Port;
                return 7863;
            }
        }

        private static bool HasExplicitPort(string url)
        {
            if (url == null) return false;
            int scheme = url.IndexOf("://");
            if (scheme < 0) return false;
            int start = scheme + 3;
            int end = url.Length;
            for (int i = start; i < url.Length; i++)
            {
                char c = url[i];
                if (c == '/' || c == '?' || c == '#')
                {
                    end = i;
                    break;
                }
            }
            string authority = url.Substring(start, end - start);
            int colon = authority.LastIndexOf(':');
            if (colon < 0 || colon == authority.Length - 1) return false;
            string digits = authority.Substring(colon + 1);
            for (int i = 0; i < digits.Length; i++)
            {
                if (digits[i] < '0' || digits[i] > '9') return false;
            }
            return digits.Length > 0;
        }

        private static string Trim(string value)
        {
            return value == null ? "" : value.Trim();
        }

        private static string OrDefault(string value, string def)
        {
            return StringUtil.IsEmpty(value) ? def : value;
        }
    }
}
