using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace QModInstaller
{
    public class QModInjector
    {
        private string terraTechDirectory;
        private string managedDirectory;
        private string installerFilename = @"QModInstaller.dll";
        private string mainFilename = @"\Assembly-CSharp.dll";
        private string backupFilename = @"\Assembly-CSharp.qoriginal.dll";


        public QModInjector(string dir, string managedDir = null)
        {
            terraTechDirectory = dir;
			if (managedDir == null)
			{
				managedDirectory = Path.Combine(terraTechDirectory, @"TerraTechWin64_Data\Managed");
			}
			else
			{
				managedDirectory = managedDir;
			}
            mainFilename = managedDirectory + mainFilename;
            backupFilename = managedDirectory + backupFilename;
        }


        public bool IsPatcherInjected()
        {
            return isInjected();
        }


        public bool Inject()
        {
            try
            {
                if (isInjected()) return false;

                // read dll
                var game = AssemblyDefinition.ReadAssembly(mainFilename);

                // delete old backups
                if (File.Exists(backupFilename))
                    File.Delete(backupFilename);

                // save a copy of the dll as a backup
                game.Write(backupFilename);

                // load patcher module
                var installer = AssemblyDefinition.ReadAssembly(installerFilename);
                var patchMethod = installer.MainModule.GetType("QModInstaller.QModPatcher").Methods.Single(x => x.Name == "Patch");

                // target the injection method
                var type = game.MainModule.GetType("TankCamera");
                var method = type.Methods.First(x => x.Name == "Start");

                // inject
                method.Body.GetILProcessor().InsertBefore(method.Body.Instructions[0], Instruction.Create(OpCodes.Call, method.Module.Import(patchMethod)));

                // save changes under original filename
                game.Write(mainFilename);

                if (!Directory.Exists(terraTechDirectory + @"\QMods"))
                    Directory.CreateDirectory(terraTechDirectory + @"\QMods");

                return true;
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
                return false;
            }
        }


        public bool Remove()
        {
            // if a backup exists
            if (File.Exists(backupFilename))
            {
                // remove the dirty dll
                File.Delete(mainFilename);

                // move the backup into its place
                File.Move(backupFilename, mainFilename);

                return true;
            }

            return false;
        }


        private bool isInjected()
        {
            try
            {
                var game = AssemblyDefinition.ReadAssembly(mainFilename);

                var rClass = "TankCamera";
                var rMethod = "Start";

                TypeDefinition type = null;
                MethodDefinition method = null;

                type = game.MainModule.GetType(rClass);
                method = type.Methods.First(x => x.Name == rMethod);

                var installer = AssemblyDefinition.ReadAssembly(installerFilename);
                var patchMethod = installer.MainModule.GetType("QModInstaller.QModPatcher").Methods.FirstOrDefault(x => x.Name == "Patch");

                bool patched = false;

                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode.Equals(OpCodes.Call) && instruction.Operand.ToString().Equals("System.Void QModInstaller.QModPatcher::Patch()"))
                    {
                        return true;
                    }
                }

                return patched;

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
                return false;
            }
        }
    }
}
