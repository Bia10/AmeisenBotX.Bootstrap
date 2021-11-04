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
            
            LaunchAuthServer(trinityBuildPath);
            LaunchWorldServer(trinityBuildPath);
            LaunchAmeisenBotNavServer(mmapsPath);
        }

        private static void LaunchAuthServer(string trinityBuildPath)
        {
            ProcessExists("authserver", true);

            var authServerPath = trinityBuildPath + "\\authserver.exe";
            if (File.Exists(authServerPath))
            {
                Console.WriteLine("Launching auth server ...");

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
            ProcessExists("worldserver", true);

            var worldServerPath = trinityBuildPath + "\\worldserver.exe";
            if (File.Exists(worldServerPath))
            {
                Console.WriteLine("Launching world server ...");

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

        private static void LaunchAmeisenBotNavServer(string mmapsPath)
        { 
            ProcessExists("AmeisenNavigationServer", true);

            var navServerPath = mmapsPath + "\\AmeisenNavigationServer.exe";
            if (File.Exists(navServerPath))
            {
                Console.WriteLine("Launching nav server ...");

                var navLauncher = new Launcher(navServerPath, string.Empty, mmapsPath);
                navLauncher.Launch();

                if (navLauncher.ErrorOutput.Any())
                    throw new Exception("Errors occurred during launch of AmeisenNavigationServer.exe!");

                Console.WriteLine("Nav server ready!");
            }
            else
            {
                throw new FileNotFoundException($"Failed to find AmeisenNavigationServer.exe! at path: {navServerPath}");
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
