using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AmeisenBotX.Bootstrap
{
    public class Launcher
    {
        private static string _exePath;
        private readonly string _cmdArgs;
        private readonly string _workDirPath;

        public bool Finished { get; private set; }
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
            if (string.IsNullOrEmpty(_exePath))
                throw new InvalidOperationException("Path to exe null or empty!");

            var workDirPath = _workDirPath;
            if (string.IsNullOrEmpty(workDirPath))
            {
                workDirPath = new FileInfo(_exePath).DirectoryName;
                if (workDirPath == null)
                    throw new InvalidOperationException("Failed to obtain path to working directory!");
            }

            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = _exePath,
                        Arguments = _cmdArgs,
                        WorkingDirectory = workDirPath,
                        UseShellExecute = true,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                    }
                };

                if (process.StartInfo.UseShellExecute == false)
                {
                    process.OutputDataReceived += OutputHandler;
                    process.ErrorDataReceived += ErrorHandler;
                }

                if (process.Start())
                {
                    if (process.StartInfo.UseShellExecute == false)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    process.WaitForExit();

                    if (process.HasExited) Finished = true;
                }
                else if (process.Start() && process.HasExited && process.ExitCode != 0)
                {
                    throw new Exception($"Process exited after starting. ExitCode: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred during process launch!", ex);
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
                NormalOutput.Add(outLine.Data);
        }

        private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
                ErrorOutput.Add(outLine.Data);
        }
    }
}