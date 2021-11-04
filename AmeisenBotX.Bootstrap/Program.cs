using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Bootstrap
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            const string trinityBuildPath = "C:\\Build\\bin\\RelWithDebInfo";
            
            LaunchAuthServer(trinityBuildPath);
            LaunchWorldServer(trinityBuildPath);
            //
        }

        private static void LaunchAuthServer(string trinityBuildPath)
        {
            foreach(var process in Process.GetProcesses())
            {
                if (process.ProcessName != "authserver") continue;
                Console.WriteLine("Already running! Process: {0} ID: {1}", process.ProcessName, process.Id);
                return;
                // process.Kill();
            }

            var authServerPath = trinityBuildPath + "\\authserver.exe";

            if (File.Exists(authServerPath))
            {
                Console.WriteLine("Launching auth server!");

                var authLauncher = new Launcher(authServerPath, string.Empty, trinityBuildPath);
                authLauncher.Launch();

                if (authLauncher.ErrorOutput.Any())
                    throw new Exception("Errors occurred during launch of authserver.exe!");

                Console.WriteLine("Auth server ready!");
            }
            else
            {
                throw new FileNotFoundException($"Failed to find authserver.exe! at path: {authServerPath}");
            }
        }

        private static void LaunchWorldServer(string trinityBuildPath)
        {
            var worldServerPath = trinityBuildPath + "\\worldserver.exe";

            foreach(var process in Process.GetProcesses())
            {
                if (process.ProcessName != "worldserver") continue;
                Console.WriteLine("Already running! Process: {0} ID: {1}", process.ProcessName, process.Id);
                return;
                // process.Kill();
            }

            if (File.Exists(worldServerPath))
            {
                Console.WriteLine("Launching world server!");

                var worldLauncher = new Launcher(worldServerPath, string.Empty, trinityBuildPath);
                worldLauncher.Launch();

                if (worldLauncher.ErrorOutput.Any())
                    throw new Exception("Errors occurred during launch of authserver.exe!");

                Console.WriteLine("World server ready!");
            }
            else
            {
                throw new FileNotFoundException($"Failed to find worldserver.exe! at path: {worldServerPath}");
            }
        }
    }
}
