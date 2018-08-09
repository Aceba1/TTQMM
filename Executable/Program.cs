using System;
using System.IO;

namespace QModManager
{
    public static class ConsoleExecutable
    {
        public static void Main(string[] args)
        {
            //Dictionary<string, string> parsedArgs = new Dictionary<string, string>();
            bool forceInstall = false;
            bool forceUninstall = false;
            string arg = "";

            if (args.Length > 0)
                arg = args[0];

            if (arg == "-i")
                forceInstall = true;
            else if (arg == "-u")
                forceUninstall = true;

            string Directory = Path.Combine(Environment.CurrentDirectory, @"../..");

            //if (parsedArgs.Keys.Contains("TerraTechDirectory"))
            //    TerraTechDirectory = parsedArgs["TerraTechDirectory"];
            //if (parsedArgs.Keys.Contains("Directory"))
            //    TerraTechDirectory = parsedArgs["Directory"];

            string ManagedDirectory = Environment.CurrentDirectory;

            if (!File.Exists(ManagedDirectory + @"/Assembly-CSharp.dll"))
            {
                Console.WriteLine("Could not find the assembly file.");
                Console.WriteLine("Please make sure you have installed QModManager in the right folder");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            QModInjector injector = new QModInjector(Directory, ManagedDirectory);

            bool isInjected = injector.IsInjected();

            if (forceInstall)
            {
                if (!isInjected)
                {
                    Console.WriteLine("Installing QModManager...");
                    injector.Inject();
                }
                else
                {
                    Console.WriteLine("Tried to force install, but it was already injected");
                    Console.WriteLine("Skipping installation");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }
            else if (forceUninstall)
            {
                if (isInjected)
                {
                    Console.WriteLine("Uninstalling QModManager...");
                    injector.Remove();
                }
                else
                {
                    Console.WriteLine("Tried to force uninstall, but it was not injected");
                    Console.WriteLine("Skipping uninstallation");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    return;
                }
            }
            else
            {
                if (!isInjected)
                {
                    Console.WriteLine("No patch detected, install? [Y/N] > ");
                    var key = Console.ReadKey().Key;
                    Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        Console.WriteLine("Installing QModManager...");
                        injector.Inject();
                        //if (injector.Inject())
                        //    Console.WriteLine("QMods was installed!");
                        //else
                        //    Console.WriteLine("There was a problem installing QMods.\nPlease contact us on Discord (discord.gg/WsvbVrP)");
                    }
                    else if (key == ConsoleKey.N)
                    {
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    Console.Write("Patch already installed, remove? [Y/N] > ");
                    var key = Console.ReadKey().Key;
                    Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        Console.Write("Removing QModManager...");
                        injector.Remove();
                        //if (injector.Remove())
                        //    Console.WriteLine("QMods was removed!");
                        //else
                        //    Console.WriteLine("There was a problem removing QMods. You may have to reinstall / verify the game's files\nPlease contact us on Discord (discord.gg/WsvbVrP)");
                    }
                    else if (key == ConsoleKey.N)
                    {
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }
            }
        }
    }
}
