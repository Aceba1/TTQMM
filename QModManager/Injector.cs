using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using QModManager.Utility;

namespace QModManager
{
    public enum Game
    {
        None = -1,
        Subnautica,
        TerraTech,
    }

    public class QModInjector
    {
        public string gameDirectory;
        public string managedDirectory;
        public string installerFilename = @"QModInstaller.dll";
        public string mainFilename = @"/Assembly-CSharp.dll";
        public string backupFilename = @"/Assembly-CSharp.qoriginal.dll";

        public Game Game = Game.None;

        public QModInjector(string dir, string managedDir, Game game)
        {
            this.Game = game;
            gameDirectory = dir;
			if (managedDir == null)
			{
				managedDirectory = Path.Combine(gameDirectory, @"TerraTechWin64_Data/Managed");
			}
			else
			{
				managedDirectory = managedDir;
			}
            mainFilename = managedDirectory + mainFilename;
            backupFilename = managedDirectory + backupFilename;
        }
#warning TODO: Implement installer rollback in Inno Setup
        public void Inject()
        {
            try
            {
                if (IsInjected())
                {
                    Console.WriteLine("Tried to install, but it was already injected");
                    Console.WriteLine("Skipping installation");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(ExitCodes.TaskCompleted); // Patch already in matching state
                }
#warning TODO: Rename variables
                AssemblyDefinition game = AssemblyDefinition.ReadAssembly(mainFilename);

                if (File.Exists(backupFilename))
                    File.Delete(backupFilename);

                game.Write(backupFilename);

                AssemblyDefinition installer = AssemblyDefinition.ReadAssembly(installerFilename);
                MethodDefinition patchMethod = installer.MainModule.GetType("QModInstaller.QModPatcher").Methods.First(x => x.Name == "Patch");

                TypeDefinition type = game.MainModule.GetType("TankCamera");
                MethodDefinition method = type.Methods.Single(x => x.Name == "Awake");

                method.Body.GetILProcessor().InsertBefore(method.Body.Instructions[0], Instruction.Create(OpCodes.Call, method.Module.Import(patchMethod)));

                game.Write(mainFilename);

                if (!Directory.Exists(gameDirectory + @"/QMods"))
                    Directory.CreateDirectory(gameDirectory + @"/QMods");

                Console.WriteLine();
                Console.WriteLine("QModManager installed successfully");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(ExitCodes.TaskCompleted);
            }
            catch (Exception e)
            {
                ExceptionUtils.ParseException(e);
            }
        }

        public void Remove()
        {
            try
            {
                if (File.Exists(backupFilename))
                {
                    File.Delete(mainFilename);

                    File.Move(backupFilename, mainFilename);

                    Console.WriteLine();
                    Console.WriteLine("QModManager uninstalled successfully");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(ExitCodes.TaskCompleted);
                }

                Console.WriteLine();
                Console.WriteLine("Cannot uninstall, file 'Assembly-CSharp-qoriginal.dll' doesn't exist");
                Console.WriteLine("To uninstall, you will need to verify game contents in steam");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(ExitCodes.RequiredFileMissing);
            }
            catch (Exception e)
            {
                ExceptionUtils.ParseException(e);
            }
        }

        public bool IsInjected()
        {
            try
            {
                string typeName;
                string methodName;

                if (Game == Game.Subnautica)
                {
                    typeName = "GameInput";
                    methodName = "Awake";
                }
                else if (Game == Game.TerraTech)
                {
                    typeName = "TankCamera";
                    methodName = "Awake";
                }
                else
                {
                    typeName = null;
                    methodName = null;
                }

                var game = AssemblyDefinition.ReadAssembly(mainFilename);

                AssemblyDefinition installer = AssemblyDefinition.ReadAssembly(installerFilename);
                MethodDefinition patchMethod = installer.MainModule.GetType("QModInstaller.QModPatcher").Methods.Single(x => x.Name == "Patch");

                TypeDefinition type = game.MainModule.GetType(typeName);
                MethodDefinition method = type.Methods.First(x => x.Name == methodName);

                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode.Equals(OpCodes.Call) && instruction.Operand.ToString().Equals("System.Void QModInstaller.QModPatcher::Patch()"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                ExceptionUtils.ParseException(e);
                return false;
            }
        }

#warning TODO: Improve DetectGame() function
        public Game DetectGame()
        {
            if (IsSubnautica() == true && IsTerraTech() == true)
                return Game.None;
            else if (IsSubnautica() == false && IsTerraTech() == false)
                return Game.None;
            else if (IsSubnautica() == true)
                return Game.Subnautica;
            else if (IsTerraTech() == true)
                return Game.TerraTech;
            else
                return Game.None;
        }

        public bool IsSubnautica() => IsGame("Subnautica");
        public bool IsTerraTech() => IsGame("TerraTech");
        public bool IsGame(string gamename)
        {
#warning TODO: MAC Support
            if (File.Exists(gameDirectory + gamename + ".exe"))
                return true;
            else
                return false;
        }
    }
}