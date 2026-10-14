using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using my.common;

namespace wb2api.launcher.Services
{
    public enum GatewayState
    {
        Stopped,
        Starting,
        Running,
        External,
        Failed
    }

    /// <summary>
    /// 静默托管 wb2api.exe：无控制台窗口、输出回灌日志面板、TCP 端口探测判定就绪。
    /// </summary>
    public class GatewayRunner
    {
        public const int ReadyTimeoutSeconds = 25;
        public const int ConnectProbeTimeoutMs = 350;
        public const int ReadinessPollIntervalMs = 300;

        public Action<string> LogHandler;
        public Action<GatewayState, string> StateHandler;

        private Process _process;
        private int _pid;
        private string _processName = "";
        private LauncherConfig _launchedWith;
        private volatile GatewayState _state = GatewayState.Stopped;
        private volatile bool _stopping;
        private readonly object _sync = new object();

        public GatewayState State { get { return _state; } }
        public int ProcessId { get { return _pid; } }
        public bool IsOwned { get { return _process != null; } }
        public bool IsReady { get { return _state == GatewayState.Running || _state == GatewayState.External; } }

        public string StateText
        {
            get
            {
                if (_state == GatewayState.Starting) return "启动中";
                if (_state == GatewayState.Running) return "运行中（本启动器）";
                if (_state == GatewayState.External) return "运行中（外部实例）";
                if (_state == GatewayState.Failed) return "启动失败";
                return "已停止";
            }
        }

        /// <summary>发起启动。返回空串表示已受理，非空表示立即失败原因。</summary>
        public string Start(LauncherConfig cfg)
        {
            lock (_sync)
            {
                if (_state == GatewayState.Running || _state == GatewayState.Starting || _state == GatewayState.External)
                {
                    Notify("服务已在运行，无需重复启动。");
                    return "";
                }
                if (cfg == null || StringUtil.IsEmpty(cfg.ExePath)) return "请先选择 wb2api.exe 的路径。";
                if (!File.Exists(cfg.ExePath)) return "找不到文件：" + cfg.ExePath;

                _processName = Path.GetFileNameWithoutExtension(cfg.ExePath) ?? "";
                _launchedWith = cfg;

                string workDir = cfg.WorkDirectory;
                if (!Directory.Exists(workDir)) workDir = Path.GetDirectoryName(cfg.ExePath);

                if (IsPortOpen(cfg.HealthHost, cfg.HealthPort, ConnectProbeTimeoutMs))
                {
                    _state = GatewayState.External;
                    string message = "端口 " + cfg.HealthHost + ":" + cfg.HealthPort + " 已被占用，判定为已有网关在运行（不是本启动器拉起的）。";
                    Notify(message);
                    WriteLog("[warn]" + message);
                    return "";
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = cfg.ExePath;
                psi.Arguments = cfg.Arguments ?? "";
                psi.WorkingDirectory = workDir;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = new UTF8Encoding(false);
                psi.StandardErrorEncoding = new UTF8Encoding(false);

                Process process;
                try
                {
                    process = Process.Start(psi);
                }
                catch (Exception ex)
                {
                    _state = GatewayState.Failed;
                    Notify("进程启动失败：" + ex.Message);
                    return "进程启动失败：" + ex.Message;
                }
                if (process == null)
                {
                    _state = GatewayState.Failed;
                    return "进程启动失败：未取得进程句柄。";
                }

                _process = process;
                _pid = process.Id;
                _stopping = false;
                _state = GatewayState.Starting;
                process.OutputDataReceived += OnOutputData;
                process.ErrorDataReceived += OnErrorData;
                process.EnableRaisingEvents = true;
                process.Exited += OnProcessExited;
                try
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch (Exception ex)
                {
                    WriteLog("[warn]输出重定向失败：" + ex.Message);
                }

                Notify("已静默拉起网关进程（PID " + _pid + "），正在等待端口就绪…");
                WriteLog("[info]已启动进程 PID " + _pid + "：" + cfg.ExePath + " " + cfg.Arguments);
            }

            App.StartTask(new ThreadStart(delegate { WaitUntilReady(cfg); }), true, "wb2api-ready-wait");
            return "";
        }

        private void WaitUntilReady(LauncherConfig cfg)
        {
            int deadline = ReadyTimeoutSeconds * 1000;
            int waited = 0;
            while (waited < deadline)
            {
                Thread.Sleep(ReadinessPollIntervalMs);
                waited += ReadinessPollIntervalMs;

                Process process = _process;
                if (process != null)
                {
                    try
                    {
                        if (process.HasExited)
                        {
                            if (_stopping) return;
                            _state = GatewayState.Failed;
                            string message = "网关进程已退出，退出码 " + SafeExitCode(process) + "。请查看运行日志。";
                            Notify(message);
                            return;
                        }
                    }
                    catch (Exception) { }
                }

                if (IsPortOpen(cfg.HealthHost, cfg.HealthPort, ConnectProbeTimeoutMs))
                {
                    _state = GatewayState.Running;
                    Notify("网关已就绪，监听 " + cfg.HealthHost + ":" + cfg.HealthPort + "，面板地址 " + cfg.PanelUrl);
                    return;
                }
            }

            _state = GatewayState.Failed;
            Notify("启动超时：" + ReadyTimeoutSeconds + " 秒内端口 " + cfg.HealthPort + " 未就绪，进程仍在运行，可停止后重试。");
        }

        /// <summary>停止网关进程：本启动器拉起的按 PID 杀，外部实例按进程名杀。</summary>
        public string Stop()
        {
            Process process;
            int pid;
            string procName;
            LauncherConfig cfg;
            lock (_sync)
            {
                process = _process;
                pid = _pid;
                procName = _processName;
                cfg = _launchedWith;
                if (process == null)
                {
                    if (_state != GatewayState.External)
                    {
                        _state = GatewayState.Stopped;
                        Notify("当前没有运行中的网关进程。");
                        return "";
                    }
                    _stopping = true;
                }
                else _stopping = true;
            }

            if (process == null)
            {
                string error = StopExternal(procName, cfg);
                lock (_sync) { _stopping = false; _state = GatewayState.Stopped; }
                Notify(error == "" ? "已停止外部网关进程。" : error);
                return error;
            }

            WriteLog("[info]正在停止 PID " + pid + " …");
            KillTree(pid);
            try
            {
                if (!process.WaitForExit(4000))
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                WriteLog("[warn]停止进程时异常：" + ex.Message);
            }
            finally
            {
                lock (_sync)
                {
                    Detach(process);
                    _stopping = false;
                    _state = GatewayState.Stopped;
                }
                Notify("已停止网关进程。");
            }
            return "";
        }

        private string StopExternal(string procName, LauncherConfig cfg)
        {
            if (StringUtil.IsEmpty(procName)) return "无法确定进程名，请手动结束网关进程。";
            Process[] targets = Process.GetProcessesByName(procName);
            try
            {
                if (targets.Length == 0)
                {
                    WriteLog("[info]没有找到名为 " + procName + " 的进程。");
                    return "";
                }
                for (int i = 0; i < targets.Length; i++)
                {
                    try
                    {
                        WriteLog("[info]结束外部进程 " + procName + " PID " + targets[i].Id);
                        targets[i].Kill();
                    }
                    catch (Exception ex)
                    {
                        WriteLog("[warn]结束 PID " + targets[i].Id + " 失败：" + ex.Message);
                    }
                }
            }
            finally
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    try { targets[i].Dispose(); } catch (Exception) { }
                }
            }
            if (cfg != null)
            {
                for (int waited = 0; waited < 3000; waited += 200)
                {
                    if (!IsPortOpen(cfg.HealthHost, cfg.HealthPort, ConnectProbeTimeoutMs)) break;
                    Thread.Sleep(200);
                }
            }
            return "";
        }

        /// <summary>退出前清理：仅在用户选择「一并停止」时调用。</summary>
        public void ShutdownIfOwned()
        {
            if (_process != null || _state == GatewayState.External) Stop();
        }

        public static bool IsPortOpen(string host, int port, int timeoutMs)
        {
            if (StringUtil.IsEmpty(host) || port <= 0 || port > 65535) return false;
            TcpClient client = null;
            try
            {
                client = new TcpClient();
                IAsyncResult ar = client.BeginConnect(host, port, null, null);
                bool done = ar.AsyncWaitHandle.WaitOne(timeoutMs, true);
                if (!done) return false;
                client.EndConnect(ar);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (client != null) { try { client.Close(); } catch (Exception) { } }
            }
        }

        /// <summary>访问 /healthz，返回响应正文；失败时返回以“错误：”开头的文本。</summary>
        public static string FetchHealth(LauncherConfig cfg)
        {
            if (cfg == null) return "错误：未配置";
            string url = "http://" + cfg.HealthHost + ":" + cfg.HealthPort + "/healthz";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 3000;
                request.UserAgent = "wb2api-launcher";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    if (stream == null) return "错误：空响应";
                    using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false)))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                string body = ReadErrorBody(ex);
                if (StringUtil.IsNotEmpty(body)) return "响应（" + ex.Message + "）：" + body;
                return "错误：" + ex.Message;
            }
            catch (Exception ex)
            {
                return "错误：" + ex.Message;
            }
        }

        /// <summary>网关未就绪时 /healthz 会回 503 + JSON，正文里才有真正原因（比如没有可用账号）。</summary>
        private static string ReadErrorBody(WebException ex)
        {
            try
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                if (response == null) return "";
                using (Stream stream = response.GetResponseStream())
                {
                    if (stream == null) return "";
                    using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false)))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void OnOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) WriteLog("[out] " + e.Data);
        }

        private void OnErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) WriteLog("[err] " + e.Data);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            if (_stopping) return;
            Process process = sender as Process;
            string message = "网关进程意外退出，退出码 " + SafeExitCode(process) + "。";
            _state = GatewayState.Failed;
            lock (_sync) { Detach(process); }
            Notify(message);
            WriteLog("[warn]" + message);
        }

        private static int SafeExitCode(Process process)
        {
            try { return process == null ? -1 : process.ExitCode; }
            catch (Exception) { return -1; }
        }

        private void Detach(Process process)
        {
            if (process == null) return;
            try { process.OutputDataReceived -= OnOutputData; } catch (Exception) { }
            try { process.ErrorDataReceived -= OnErrorData; } catch (Exception) { }
            try { process.Exited -= OnProcessExited; } catch (Exception) { }
            try { process.Dispose(); } catch (Exception) { }
            if (ReferenceEquals(_process, process))
            {
                _process = null;
                _pid = 0;
            }
        }

        private static void KillTree(int pid)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "taskkill.exe";
                psi.Arguments = "/F /T /PID " + pid;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                Process killer = Process.Start(psi);
                if (killer != null)
                {
                    killer.WaitForExit(3000);
                    killer.Dispose();
                }
            }
            catch (Exception) { }
        }

        private void WriteLog(string line)
        {
            Action<string> handler = LogHandler;
            if (handler != null)
            {
                try { handler(line); }
                catch (Exception) { }
            }
        }

        private void Notify(string message)
        {
            Action<GatewayState, string> handler = StateHandler;
            if (handler != null)
            {
                try { handler(_state, message); }
                catch (Exception) { }
            }
        }
    }
}
