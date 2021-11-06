using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Bootstrap
{
    internal static class Program
    {
        private static void Main()
        {
            const string trinityBuildPath = "C:\\Build\\bin\\RelWithDebInfo";
            const string mmapsPath = "C:\\Games\\World of Warcraft 3.3.5a\\mmaps";

            LaunchServer("authserver", trinityBuildPath);
            LaunchServer("worldserver", trinityBuildPath);
            LaunchServer("AmeisenNavigationServer", mmapsPath);
        }

        private static void LaunchServer(string exeName, string pathToWorkDir)
        {
            ProcessExists(exeName, true);

            var serverFilePath = pathToWorkDir + $"\\{exeName}.exe";
            if (File.Exists(serverFilePath))
            {
                Console.WriteLine("Launching" + $" {exeName}" + " ...");

                var serverLauncher = new Launcher(serverFilePath, string.Empty, pathToWorkDir);
                serverLauncher.Launch();

                if (serverLauncher.ErrorOutput.Any())
                    throw new Exception("Errors occurred during launch of" + $" {exeName}.exe!");

                Console.WriteLine($"{exeName}" + " ready!");
            }
            else
            {
                throw new FileNotFoundException("Failed to find" + $" {exeName}.exe!" + $" at path: {serverFilePath}");
            }
        }

        private static void ProcessExists(string procName, bool killProc)
        {
            foreach(var process in Process.GetProcesses())
            { 
                if (process.ProcessName != procName) continue;

                Console.WriteLine("Already running {0}({1}) ...", process.ProcessName, process.Id);

                if (!killProc) continue;

                Console.WriteLine("Killing {0}({1}) ...", process.ProcessName, process.Id);
                process.Kill();
            }
        }
    }
}
