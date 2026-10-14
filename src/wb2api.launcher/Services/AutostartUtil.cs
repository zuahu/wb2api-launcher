using System;
using Microsoft.Win32;
using my.common;

namespace wb2api.launcher.Services
{
    /// <summary>开机自启：写当前用户的 Run 键，附带 --minimized 参数。</summary>
    public class AutostartUtil
    {
        public const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        public const string ValueName = "WB2ApiLauncher";
        public const string MinimizeArgument = "--minimized";

        public static string ExeFilePath
        {
            get { return System.Windows.Forms.Application.ExecutablePath; }
        }

        public static string BuildCommand(string exePath)
        {
            return "\"" + exePath + "\" " + MinimizeArgument;
        }

        public static string CurrentCommand()
        {
            return BuildCommand(ExeFilePath);
        }

        public static bool IsEnabled()
        {
            string stored = ReadValue();
            if (StringUtil.IsEmpty(stored)) return false;
            return stored.Replace("\"", "").StartsWith(ExeFilePath, StringComparison.OrdinalIgnoreCase);
        }

        public static string Enable()
        {
            return WriteValue(CurrentCommand());
        }

        public static string Disable()
        {
            return WriteValue(null);
        }

        /// <summary>恢复成给定值（自检里用完之后把原状写回去；null 表示删除）。</summary>
        public static string Restore(string previousValue)
        {
            return WriteValue(previousValue);
        }

        public static string ReadValue()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    if (key == null) return "";
                    object raw = key.GetValue(ValueName);
                    return raw == null ? "" : Convert.ToString(raw).Trim();
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string WriteValue(string value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
                {
                    if (key == null) return "无法打开注册表 Run 项。";
                    if (value == null) key.DeleteValue(ValueName, false);
                    else key.SetValue(ValueName, value);
                    return "";
                }
            }
            catch (Exception ex)
            {
                return "写注册表失败：" + ex.Message;
            }
        }
    }
}
