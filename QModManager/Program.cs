using QModInstaller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QModManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = new Dictionary<string, string>();
            bool forceInstall = false;
            bool forceUninstall = false;

            foreach (var arg in args)
            {
                if (arg.Contains("="))
                {
                    parsedArgs = args.Select(s => s.Split(new[] { '=' }, 1)).ToDictionary(s => s[0], s => s[1]);
                }
                else if (arg.StartsWith("-"))
                {
                    if (arg == "-i")
                        forceInstall = true;

                    if (arg == "-u")
                        forceUninstall = true;
                }
            }

            //string SubnauticaDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\TerraTech";
            string TerraTechDirectory = Path.Combine(Environment.CurrentDirectory, @"..\..");

            if (parsedArgs.Keys.Contains("TerraTechDirectory"))
                TerraTechDirectory = parsedArgs["TerraTechDirectory"];
            if (parsedArgs.Keys.Contains("Directory"))
                TerraTechDirectory = parsedArgs["Directory"];

            string ManagedDirectory = Environment.CurrentDirectory;
            if (!File.Exists(ManagedDirectory + @"\Assembly-CSharp.dll"))
            {
                Console.Write("Could not find Assembly file.");
                if (forceInstall || forceUninstall)
                {
                    Console.WriteLine("Canceling.");
                    return;
                }
                Console.WriteLine("\nPress any key to exit ...");
                Console.ReadKey();
                return;
            }
            QModInjector injector = new QModInjector(TerraTechDirectory, ManagedDirectory);

            bool isInjected = injector.IsPatcherInjected();
            if (forceInstall)
            {
                if (!isInjected)
                {
                    Console.WriteLine("Installing QMods...");
                    injector.Inject();
                }
                else
                {
                    Console.WriteLine("Tried to force install, but it was already injected. Skipping installation.");
                    return;
                }
            }
            else if (forceUninstall)
            {
                if (isInjected)
                {
                    Console.WriteLine("Uninstalling QMods...");
                    try
                    {
                        injector.Remove();
                    }
                    catch (NullReferenceException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        if (e.InnerException != null)
                        {
                            Console.WriteLine(e.InnerException.Message);
                            Console.WriteLine(e.InnerException.StackTrace);
                        }
                        Console.ReadKey();
                        Environment.Exit(0);
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Tried to force uninstall, but it was not injected. Skipping uninstallation.");
                    return;
                }
            }
            else
            {
                if (!isInjected)
                {
                    Console.Write("No patch detected, install? [Y/N] ");
                    var key = Console.ReadKey().Key;
                    Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        Console.WriteLine("Installing... ");
                        if (injector.Inject())
                            Console.WriteLine("QMods was installed!");
                        else
                            Console.WriteLine("There was a problem installing QMods.\nPlease contact us on Discord (discord.gg/WsvbVrP)");
                    }
                    else if (key == ConsoleKey.N)
                    {
                        Console.WriteLine("Installation aborted.");
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.Write("Patch already installed, remove? [Y/N] ");
                    var key = Console.ReadKey().Key;
                    Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        Console.Write("Removing... ");
                        if (injector.Remove())
                            Console.WriteLine("QMods was removed!");
                        else
                            Console.WriteLine("There was a problem removing QMods. You may have to reinstall / verify the game's files\nPlease contact us on Discord (discord.gg/WsvbVrP)");
                    }
                    else if (key == ConsoleKey.N)
                    {
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }

                }

                Console.WriteLine("Press any key to exit...");

                Console.ReadKey();
            }
        }
    }
}
