using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using my.common;

namespace wb2api.launcher.Services
{
    /// <summary>
    /// 单实例唤起：第二个实例不启动界面，而是给已在托盘里的实例发一条自定义消息，
    /// 由它自己把窗口恢复显示（走和双击托盘图标完全相同的代码路径）。
    /// </summary>
    public static class InstanceActivator
    {
        public const string ShowMessageName = "WB2ApiLauncher.ShowRequest";
        public const string TitleKeyword = "WorkBuddy2API";

        public static readonly int ShowMessageId = RegisterWindowMessage(ShowMessageName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>通知同名的其它实例显示主窗口。返回是否至少通知到一个窗口。</summary>
        public static bool NotifyOtherInstances()
        {
            HashSet<uint> pids = OtherProcessIds();
            if (pids.Count == 0) return false;

            List<IntPtr> targets = new List<IntPtr>();
            EnumWindowsProc callback = delegate(IntPtr hWnd, IntPtr lParam)
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                if (pid == 0 || !pids.Contains(pid)) return true;
                StringBuilder text = new StringBuilder(256);
                GetWindowText(hWnd, text, text.Capacity);
                if (text.ToString().IndexOf(TitleKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    targets.Add(hWnd);
                }
                return true;
            };
            EnumWindows(callback, IntPtr.Zero);

            for (int i = 0; i < targets.Count; i++)
            {
                PostMessage(targets[i], (uint)ShowMessageId, IntPtr.Zero, IntPtr.Zero);
            }
            return targets.Count > 0;
        }

        private static HashSet<uint> OtherProcessIds()
        {
            HashSet<uint> pids = new HashSet<uint>();
            int self = Process.GetCurrentProcess().Id;
            string name;
            try { name = Process.GetCurrentProcess().ProcessName; }
            catch (Exception) { return pids; }

            Process[] peers = Process.GetProcessesByName(name);
            try
            {
                for (int i = 0; i < peers.Length; i++)
                {
                    try
                    {
                        if (peers[i].Id == self) continue;
                        pids.Add((uint)peers[i].Id);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
            finally
            {
                for (int i = 0; i < peers.Length; i++)
                {
                    try { peers[i].Dispose(); } catch (Exception) { }
                }
            }
            return pids;
        }

        /// <summary>消息号是否注册成功（RegisterWindowMessage 返回 0 表示失败）。</summary>
        public static bool IsUsableMessageId()
        {
            return ShowMessageId > 0 && StringUtil.IsNotEmpty(ShowMessageName);
        }
    }
}
