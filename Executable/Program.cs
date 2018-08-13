using System;
using System.Collections.Generic;
using System.IO;

namespace QModManager
{
    public enum Game
    {
        Subnautica,
        TerraTech,
    }

    public static class ConsoleExecutable
    {
        public static string RawArgument;
        public static Dictionary<string, string> Arguments = new Dictionary<string, string>();

        public static Game Game;

        public static void Main(string[] args)
        {
            try
            {
                Dictionary<string, string> parsedArgs = new Dictionary<string, string>();
                bool forceInstall = false;
                bool forceUninstall = false;

                try
                {
                    if (args.Length < 1)
                    {
                        RawArgument = "";
                        goto AfterArgumentParsing;
                    }
                    else
                        RawArgument = args[0];

                    string[] arguments = RawArgument.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arguments.Length < 1)
                        goto AfterArgumentParsing;

                    foreach (string argument in arguments)
                    {
                        string[] parsed = argument.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parsed.Length < 2)
                            continue;
                        Arguments.Add(parsed[0], parsed[1]);
                    }

                    if (Arguments.Count == 0)
                        goto AfterArgumentParsing;

                    Arguments.TryGetValue("Game", out string temp);
                    Game = (Game)Enum.Parse(typeof(Game), temp);
                    Arguments.TryGetValue("Type", out string Type);

                    if (Type == "Install")
                        forceInstall = true;
                    if (Type == "Uninstall")
                        forceUninstall = true;
                }
                catch (Exception e)
                {
                    Utility.ExceptionUtils.ParseException(e);
                    Environment.Exit(2);
                }

                AfterArgumentParsing:;

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
                        Console.Write("No patch detected, install? [Y/N] > ");
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
            catch (Exception e)
            {
                Utility.ExceptionUtils.ParseException(e);
                Environment.Exit(1);
            }
        }
    }
}
