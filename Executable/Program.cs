﻿using QModManager.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace QModManager
{
    // Class containing all stuff regarding the console app
    public static class ConsoleExecutable
    {
        // Raw argument received by the executable from the installer
        public static string RawArgument;
        // Dictionary of parsed arguments
        public static Dictionary<string, string> Arguments = new Dictionary<string, string>();

        // The game QModManager was installed for (argument received from installer)
        public static Game game = Game.None;
        // Entry point for the console app
        public static void Main(string[] args)
        {
#warning TODO: Add comments
            try
            {
                DisableExit();

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

                    if (!Arguments.TryGetValue("Game", out string temp))
                        game = Game.None;
                    else
                        try
                        {
                            game = (Game)Enum.Parse(typeof(Game), temp);
                        }
                        catch (ArgumentException)
                        {
                            game = Game.None;
                        }
                    if (!Arguments.TryGetValue("Type", out string Type))
                        goto AfterArgumentParsing;

                    if (Type == "Install")
                        forceInstall = true;
                    else if (Type == "Uninstall")
                        forceUninstall = true;
                    else
                        goto AfterArgumentParsing;
                }
                catch (Exception e)
                {
                    ExceptionUtils.ParseException(e);
                    Environment.Exit(ExitCodes.ArgumentParsingError);
                }

                AfterArgumentParsing:;

                string directory = Path.Combine(Environment.CurrentDirectory, @"../..");

                string ManagedDirectory = Environment.CurrentDirectory;

                if (!File.Exists(ManagedDirectory + "/Assembly-CSharp.dll"))
                {
                    Console.WriteLine("Could not find the assembly file.");
                    Console.WriteLine("Please make sure you have installed QModManager in the right folder.");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(ExitCodes.RequiredFileMissing);
                }
                
                if (Directory.GetFiles(directory, "Subnautica*.exe", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    game = Game.Subnautica;
                }
                else if (Directory.GetFiles(directory, "TerraTech*.exe", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    game = Game.TerraTech;
                } else
                {
                    Console.WriteLine("Could not find any game to patch!");
                    Console.WriteLine("An assembly file was found, but no executable was detected.");
                    Console.WriteLine("Please make sure you have installed QModManager in the right folder.");
                    Console.WriteLine("If the problem persists, open a bug report on NexusMods or an issue on GitHub");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(ExitCodes.RequiredFileMissing);
                }

#warning TODO: Improve injector code. It's 2018 out there...
                QModInjector injector = new QModInjector(directory, ManagedDirectory, game);

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
                        Console.WriteLine("Tried to install, but it was already injected");
                        Console.WriteLine("Skipping installation");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(0);
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
                        Console.WriteLine("Tried to uninstall, but it was not injected");
                        Console.WriteLine("Skipping uninstallation");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        Environment.Exit(0);
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
                        }
                        else if (key == ConsoleKey.N)
                        {
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Console.Write("Patch installed, remove? [Y/N] > ");
                        var key = Console.ReadKey().Key;
                        Console.WriteLine();
                        if (key == ConsoleKey.Y)
                        {
                            Console.Write("Removing QModManager...");
                            injector.Remove();
                        }
                        else if (key == ConsoleKey.N)
                        {
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionUtils.ParseException(e);
                Environment.Exit(ExitCodes.UnknownError);
            }
        }

        #region Disable exit

        public static void DisableExit()
        {
            DisableExitButton();
            Console.CancelKeyPress += CancelKeyPress;
            Console.TreatControlCAsInput = true;
        }

        public static void DisableExitButton() => EnableMenuItem(GetSystemMenu(GetConsoleWindow(), false), 0xF060, 0x1);
        private static void CancelKeyPress(object sender, ConsoleCancelEventArgs e) => e.Cancel = true;
        [DllImport("user32.dll")] public static extern int EnableMenuItem(IntPtr tMenu, int targetItem, int targetStatus);
        [DllImport("user32.dll")] public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("kernel32.dll", ExactSpelling = true)] public static extern IntPtr GetConsoleWindow();

        #endregion
    }
}