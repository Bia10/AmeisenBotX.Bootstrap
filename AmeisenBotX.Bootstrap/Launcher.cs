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
        private static string ExePath;
        private static string Args;
        private static string WorkDir;

        private static readonly TaskCompletionSource<bool> unhandledExceptionEventHandled = new();
        private static readonly TaskCompletionSource<bool> exitEventHandled = new();
        private static readonly TaskCompletionSource<bool> outputEventHandled = new();
        private static readonly TaskCompletionSource<bool> errorEventHandled = new();

        public static bool Finished { get; private set; }
        public readonly List<string> NormalOutput;
        public readonly List<string> ErrorOutput;

        public Launcher(string exePath, string cmdArgs)
        {
            ExePath = exePath;
            Args = cmdArgs;
            NormalOutput = new List<string>();
            ErrorOutput = new List<string>();
        }

        public Launcher(string exePath, string cmdArgs, string workDirPath)
        {
            ExePath = exePath;
            Args = cmdArgs;
            WorkDir = workDirPath;
            NormalOutput = new List<string>();
            ErrorOutput = new List<string>();
        }

        public void Launch()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            if (string.IsNullOrEmpty(ExePath))
                throw new ArgumentException("Path to exe null or empty!");

            var workDirPath = WorkDir;
            if (string.IsNullOrEmpty(workDirPath))
            {
                workDirPath = Path.GetDirectoryName(workDirPath);
                if (string.IsNullOrEmpty(workDirPath))
                    throw new Exception("Failed to obtain path to working directory!");
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = ExePath,
                    Arguments = Args,
                    WorkingDirectory = workDirPath,
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

                if (!curProcess.Start())
                    throw new Exception($"Failed to start process. ExitCode: {curProcess.ExitCode}");

                if (curProcess.StartInfo.UseShellExecute == false)
                {
                    curProcess.BeginOutputReadLine();
                    curProcess.BeginErrorReadLine();
                }

                Console.WriteLine(curProcess.Responding 
                    ? $"{curProcess.ProcessName}({curProcess.Id}) Status: Responding"
                    : $"{curProcess.ProcessName}({curProcess.Id}) Status: Not Responding");
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred during process launch!", ex);
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var exception = (Exception)args.ExceptionObject;

            Console.WriteLine($"UnhandledException caught: {exception.Message}");
            Console.WriteLine($"Runtime terminating: {args.IsTerminating}");

            unhandledExceptionEventHandled.TrySetResult(true);
        }

        private enum ExitCode
        {
            NormalTermination = 0,
            AbnormalTermination,
        }

        private static void ExitedHandler(object sendingProcess, EventArgs e)
        {
            Finished = true;
            var proc = (Process)sendingProcess;

            Console.WriteLine($"Start time: {proc.StartTime}\n" + $"Exit time: {proc.ExitTime}\n" 
                            + $"Run time: {proc.ExitTime.Millisecond - proc.StartTime.Millisecond}ms"
                            + $"Proc Id: {proc.Id}\n" + $"Exit code: {(proc.ExitCode == 0 ? ExitCode.NormalTermination : ExitCode.AbnormalTermination)}\n");

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