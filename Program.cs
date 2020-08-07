using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Savescum
{
    class Program
    {
        private const string ARGUMENT_SEPARATOR = "=";
        private const string OPERATION_SAVE = "save";
        private const string OPERATION_LOAD = "load";

        private const string PREFIX_BACKUP  = "SavescumBackup";
        private const string PREFIX_PROTECT = "SavescumOverwriteProtection";

        private const string FORMAT_SAVE    = "{0}\\{1}{2:D3}";
        private const string FORMAT_PROTECT = "{0}\\{1}{2:yyyy-MM-dd_hh-mm-ss}";

        private const string ARGUMENT_OPERATION = "operation";

        private const string ARGUMENT_OPERATION_CLEAN = "clean";
        private const string ARGUMENT_OPERATION_CLEAR = "clear";
        private const string ARGUMENT_OPERATION_SAVE = "save";
        private const string ARGUMENT_OPERATION_LOAD = "load";

        private const string ARGUMENT_PATH_GAME = "gamePath";
        private const string ARGUMENT_PATH_BACKUP = "backupPath";
        private const string ARGUMENT_PATH_PROTECT = "protectPath";
        private const string ARGUMENT_COUNT = "count";

        private const string ARGUMENT_PREFIX_BACKUP = "backupPrefix";
        private const string ARGUMENT_PREFIX_PROTECT = "protectPrefix";

        private static readonly int MAX_NAME_COUNT = Int32.MaxValue;
        private static Dictionary<string, string> s_argumentProperties;

        // These are required so no default set
        private static string s_pathGame;
        private static string s_pathBackup;
        private static string s_pathProtect;

        // Set argument defaults
        private static int s_argumentCount = 1;
        private static string s_prefixBackup = PREFIX_BACKUP;
        private static string s_prefixProtect = PREFIX_PROTECT;

        // Command-lines for testing
        //
        // operation=save 
        // operation=save gamePath=g:\source\git\Savescum backupPath=g:\source\git\SavescumBackup
        // gamePath=g:\source\git\Savescum backupPath=g:\source\git\SavescumBackup
        // operation=save gamePath=g:\source\git\Savescum backupPath=g:\source\git\SavescumBackup blah=blah=blah

        static void Main(string[] args)
        {
            PrintStartBanner();

            if (args.Length == 0)
            {
                PrintErrorNoArguments();
                PrintUsage();
                Environment.Exit(1);
            }

            try
            {
                s_argumentProperties = GetArgumentProperties(args);
            }

            catch (ArgumentException e)
            {
                PrintArgumentException(e);
                Environment.Exit(1);
            }

            s_prefixBackup = GetPropertyValue(ARGUMENT_PREFIX_BACKUP, PREFIX_BACKUP);
            s_prefixProtect = GetPropertyValue(ARGUMENT_PREFIX_PROTECT, PREFIX_PROTECT);

            s_pathGame = GetPropertyValue(ARGUMENT_PATH_GAME, null);
            s_pathBackup = GetPropertyValue(ARGUMENT_PATH_BACKUP, null);
            s_pathProtect = GetPropertyValue(ARGUMENT_PATH_PROTECT, s_pathBackup);

            if (!s_argumentProperties.TryGetValue(ARGUMENT_OPERATION, out string operation))
            {
                PrintArgumentRequired(ARGUMENT_OPERATION);
                PrintUsage();
                Environment.Exit(1);
            }

            switch (operation)
            {
                case OPERATION_SAVE:
                    DoSave();
                    break;

                case OPERATION_LOAD:
                    DoLoad();
                    break;

                default:
                    Console.WriteLine("Unknown operation: " + args[0]);
                    PrintUsage();
                    Environment.Exit(1);
                    break;
            }

            Environment.Exit(0);
        }

        private static string GetPropertyValue(string key, string defaultValue)
        {
            if (s_argumentProperties.TryGetValue(key, out string value))
            {
                return value;
            }

            // argument not found - null default indicates required value
            if (null == defaultValue)
            {
                PrintArgumentRequired(key);
                Environment.Exit(1);
            }

            return defaultValue;
        }

        private static Dictionary<string, string> GetArgumentProperties(string[] args)
        {
            Dictionary<string, string> argumentProperties = new Dictionary<string, string>(args.Length);

            foreach (string argument in args)
            {
                String[] argumentParts = argument.Split(ARGUMENT_SEPARATOR);
                if (argumentParts.Length != 2)
                {
                    throw new ArgumentException("Improperly formed argument: [" + argument + "]");
                }

                argumentProperties.Add(argumentParts[0], argumentParts[1]);
            }

            return argumentProperties;
        }

        private static void DoSave()
        {
            Console.WriteLine("Savescum SAVING ...");

            string savePath = GenerateSavePath(s_pathBackup, s_prefixBackup);

            if (savePath.Length == 0)
            {
                Console.WriteLine("Error: Couldn't generate save path");
                return;
            }

            PrintCopyInfo(s_pathGame, savePath);
            DirectoryCopy(s_pathGame, savePath, true);

            Console.WriteLine();
            Console.WriteLine("SAVE FINISHED to " + savePath);
            Console.WriteLine();
        }

        private static void DoLoad()
        {
            Console.WriteLine("Savescum LOADING...");

            // Find latest save - notify and bail out if it isn't found
            string lastSavePath = FindLastSavePath(s_pathBackup, s_prefixBackup);
            if (lastSavePath.Length == 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("  Error: No saves found at ");
                Console.Error.WriteLine("      path: " + s_pathBackup);
                Console.Error.WriteLine("    prefix: " + s_prefixBackup);
                Console.Error.WriteLine();
                Console.Error.WriteLine("  NO FILES CHANGED");
                Console.Error.WriteLine();

                return;
            }

            Console.WriteLine("  Found latest backup at");
            Console.WriteLine("      path: " + lastSavePath);

            // Backup existing save directory before writing over it
            string protectDirPath = GenerateProtectPath(s_pathProtect, s_prefixProtect);

            if (protectDirPath.Length == 0)
            {
                PrintProtectError(s_pathProtect, s_prefixProtect);
                return;
            }

            Console.WriteLine("  Backing up directory before writing over it");
            PrintCopyInfo(s_pathGame, protectDirPath);
            DirectoryCopy(s_pathGame, protectDirPath, true);

            // delete and write over
            Console.WriteLine("  Deleting directory: " + s_pathGame);
            DirectoryInfo deleteDir = new DirectoryInfo(s_pathGame);
            deleteDir.Delete(true);

            Console.WriteLine("  Restoring deleted directory from backup save");

            PrintCopyInfo(lastSavePath, s_pathGame);
            DirectoryCopy(lastSavePath, s_pathGame, true);

            Console.WriteLine();
            Console.WriteLine("LOAD FINISHED from " + lastSavePath);
            Console.WriteLine();
        }

        private static string FindLastSavePath(string pathSaves, string prefixSave)
        {
            // default to empty if unfound
            string lastFoundPath = "";

            for (int nameCount = 0; nameCount < MAX_NAME_COUNT; nameCount++)
            {
                string checkPath = String.Format(FORMAT_SAVE, pathSaves, prefixSave, nameCount);
                DirectoryInfo dir = new DirectoryInfo(checkPath);
                if (!dir.Exists)
                {
                    break;
                }

                lastFoundPath = checkPath;
            }

            return lastFoundPath;
        }

        private static string GenerateSavePath(string path, string prefix)
        {
            string savePath;

            for (int nameCount = 0; nameCount < MAX_NAME_COUNT; nameCount++)
            {
                savePath = String.Format(FORMAT_SAVE, path, prefix, nameCount);
                DirectoryInfo dir = new DirectoryInfo(savePath);

                if (!dir.Exists)
                {
                    return savePath;
                }
            }

            // Couldn't find a name that isn't used
            return "";
        }

        private static string GenerateProtectPath(string path, string prefix)
        {
            string protectDirPath = String.Format(FORMAT_PROTECT, path, prefix, DateTime.Now);
            DirectoryInfo dir = new DirectoryInfo(protectDirPath);

            if (!dir.Exists)
            {
                return protectDirPath;
            }

            return "";
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void PrintStartBanner()
        {
            Console.WriteLine();
            Console.WriteLine("Savescum backup utility");
            Console.WriteLine();
        }

        private static void PrintProtectError(string path, string prefix)
        {
            Console.Error.WriteLine("Error: Couldn't backup directory before writing over it");
            Console.Error.WriteLine("    Path  = " + path);
            Console.Error.WriteLine("   Prefix = " + prefix);
            Console.Error.WriteLine();
        }

        private static void PrintCopyInfo(string pathSource, string pathDest)
        {
            Console.WriteLine("    Copying directory");
            Console.WriteLine(String.Format("      [{0}] ->", pathSource));
            Console.WriteLine(String.Format("      [{0}] ...", pathDest));
            Console.WriteLine("    Copy finished");
        }

        private static void PrintArgumentRequired(string argument)
        {
            Console.Error.WriteLine("Error:");
            Console.Error.WriteLine("  Required argument not found: " + argument);
            Console.Error.WriteLine();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: Savescum operation=[save|load|clean|clear] [argument=value ...]");
            Console.WriteLine();
        }

        private static void PrintArgumentException(ArgumentException e)
        {
            Console.Error.WriteLine("Error processing arguments: ");
            Console.Error.WriteLine("  " + e.Message);
            Console.Error.WriteLine();
        }

        private static void PrintErrorNoArguments()
        {
            Console.Error.WriteLine("Error: No command-line arguments given");
            Console.Error.WriteLine();
        }
    }
}
