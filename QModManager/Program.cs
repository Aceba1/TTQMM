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

            string ManagedDirectory = TerraTechDirectory + @"\TerraTechWin64_Data\Managed";
            if (!File.Exists(ManagedDirectory + @"\Assembly -CSharp.dll"))
            {
                TerraTechDirectory = Environment.CurrentDirectory;
                retry:;
                ManagedDirectory = TerraTechDirectory + @"\TerraTechWin64_Data\Managed";
                if (!File.Exists(ManagedDirectory + @"\Assembly-CSharp.dll"))
                {
                    if (forceInstall || forceUninstall)
                    {
                        Console.WriteLine("Could not find Assembly file. Canceling.");
                        return;
                    }
                    Console.Write("Drag and drop, or type the path of the TerraTech executable here: ");
                    TerraTechDirectory = Path.Combine(Console.ReadLine().Replace("\"", string.Empty), @"..");
                    Console.WriteLine();
                    goto retry;
                }
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
                    Console.WriteLine("No patch detected, install? (Yes|No) ");
                    string consent = Console.ReadLine().Replace("'", string.Empty).ToLower();
                    if (consent == "yes" || consent == "y")
                    {
                        Console.Write("Installing... ");
                        if (injector.Inject())
                            Console.WriteLine("QMods was installed!");
                        else
                            Console.WriteLine("There was a problem installing QMods.\nPlease contact us on Discord (discord.gg/WsvbVrP)");
                    }
                }
                else
                {
                    Console.WriteLine("Patch already installed, remove? (Yes|No) ");
                    string consent = Console.ReadLine().Replace("'", string.Empty).ToLower();
                    if (consent == "yes" || consent == "y")
                    {
                        Console.Write("Removing... ");
                        if (injector.Remove())
                            Console.WriteLine("QMods was removed!");
                        else
                            Console.WriteLine("There was a problem removing QMods. You may have to reinstall / verify the game's files\nPlease contact us on Discord (discord.gg/WsvbVrP)");
                    }
                }

                Console.WriteLine("Press any key to exit ...");

                Console.ReadKey();
            }
        }
    }
}
