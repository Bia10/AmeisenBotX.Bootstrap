using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AmeisenBotX.Bootstrap
{
    public class Launcher
    {
        private static Process curProcess = new();
        private static TaskCompletionSource<bool> exitEventHandled = new();
        private static TaskCompletionSource<bool> unhandledExceptionEventHandled = new();
        private static TaskCompletionSource<bool> outputEventHandled = new();
        private static TaskCompletionSource<bool> errorEventHandled = new();

        private static string _exePath;
        private readonly string _cmdArgs;
        private readonly string _workDirPath;

        public static bool Finished { get; private set; }
        public readonly List<string> NormalOutput;
        public readonly List<string> ErrorOutput;

        public Launcher(string exePath, string cmdArgs)
        {
            _exePath = exePath;
            _cmdArgs = cmdArgs;
            NormalOutput = new List<string>();
            ErrorOutput = new List<string>();
        }

        public Launcher(string exePath, string cmdArgs, string workDirPath)
        {
            _exePath = exePath;
            _cmdArgs = cmdArgs;
            _workDirPath = workDirPath;
            NormalOutput = new List<string>();
            ErrorOutput = new List<string>();
        }

        public void Launch()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            if (string.IsNullOrEmpty(_exePath))
                throw new InvalidOperationException("Path to exe null or empty!");

            var workDirPath = _workDirPath;
            if (string.IsNullOrEmpty(workDirPath))
            {
                workDirPath = Path.GetDirectoryName(workDirPath);
                if (string.IsNullOrEmpty(workDirPath))
                    throw new InvalidOperationException("Failed to obtain path to working directory!");
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = _exePath,
                    Arguments = _cmdArgs,
                    WorkingDirectory = workDirPath,
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };

            curProcess = process;

            try
            {
                curProcess.EnableRaisingEvents = true;

                if (curProcess.StartInfo.UseShellExecute == false)
                {
                    curProcess.OutputDataReceived += OutputHandler;
                    curProcess.ErrorDataReceived += ErrorHandler;
                    curProcess.Exited += ExitedHandler;
                }

                if (curProcess.Start() && curProcess.HasExited)
                    throw new Exception($"Start() returned HasExited = true after starting. ExitCode: {curProcess.ExitCode}");

                if (curProcess.Start())
                {
                    if (curProcess.StartInfo.UseShellExecute == false)
                    {
                        curProcess.BeginOutputReadLine();
                        curProcess.BeginErrorReadLine();
                    }

                    Console.WriteLine(curProcess.Responding ? "Status = Running" : "Status = Not Responding");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred during process launch!", ex);
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;

            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);

            unhandledExceptionEventHandled.TrySetResult(true);
        }

        private enum ExitCode
        {
            normalTermination = 0,
            abnormalTermination,
        }

        private static void ExitedHandler(object sendingProcess, EventArgs e)
        {
            var proc = (Process)sendingProcess;
            Finished = true;

            Console.WriteLine($"Exit time: {proc.ExitTime}\n" + $"Exit code: {(ExitCode)proc.ExitCode}\n" +
                              $"Elapsed time: {Math.Round((proc.ExitTime - proc.StartTime).TotalMilliseconds)}ms");

            exitEventHandled.TrySetResult(true);
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            var line = outLine.Data;

            if (!string.IsNullOrEmpty(line))
                NormalOutput.Add(line);

            outputEventHandled.TrySetResult(true);
        }

        private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            var line = outLine.Data;

            if (!string.IsNullOrEmpty(line))
                ErrorOutput.Add(line);

            errorEventHandled.TrySetResult(true);
        }
    }
}