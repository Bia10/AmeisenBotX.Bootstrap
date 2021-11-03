using System;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Bootstrap
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            const string trinityBuildPath = "C:\\Build\\bin\\RelWithDebInfo\\";
            LaunchTrinity(trinityBuildPath);
        }

        private static void LaunchTrinity(string trinityBuildPath)
        {
            var authServerPath = trinityBuildPath + "authserver.exe";
            var worldServerPath = trinityBuildPath + "worldserver.exe";

            if (File.Exists(authServerPath))
            {
                Console.WriteLine("Launching auth server!");

                var launcher = new Launcher(authServerPath, string.Empty, trinityBuildPath);
                launcher.Launch();

                if (launcher.ErrorOutput.Any())
                    throw new Exception("Errors occurred during launch of authserver.exe!");

                Console.WriteLine("Auth server ready!");
            }
            else
            {
                throw new FileNotFoundException($"Failed to find authserver.exe! at path: {authServerPath}");
            }

            if (File.Exists(worldServerPath))
            {
                Console.WriteLine("Launching world server!");

                var launcher = new Launcher(worldServerPath, string.Empty, trinityBuildPath);
                launcher.Launch();

                if (launcher.ErrorOutput.Any())
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
