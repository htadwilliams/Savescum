using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace Savescum
{
    class Program
    {
        private const string ARGUMENT_SEPARATOR = "=";
        private const string OPERATION_SAVE = "save" ;
        private const string OPERATION_LOAD = "load";
        private const string OPERATION_QUICKLOAD = "quickload";
        private const string OPERATION_CLEAN = "clean";
        private const string OPERATION_CLEAR = "clear";

        private const string PREFIX_BACKUP  = "SavescumBackup";
        private const string PREFIX_PROTECT = "SavescumOverwriteProtection";

        private const string FORMAT_SAVE    = "{0}\\{1}{2:D3}";
        private const string FORMAT_PROTECT = "{0}\\{1}{2:yyyy-MM-dd_hh-mm-ss}";

        private const string ARGUMENT_OPERATION = "operation";

        private const string ARGUMENT_PATH_GAME = "gamePath";
        private const string ARGUMENT_PATH_BACKUP = "backupPath";
        private const string ARGUMENT_PATH_PROTECT = "protectPath";
        private const string ARGUMENT_COUNT = "count";

        private const string ARGUMENT_PREFIX_BACKUP = "backupPrefix";
        private const string ARGUMENT_PREFIX_PROTECT = "protectPrefix";

        private static readonly int MAX_NAME_COUNT = 999;

        private static ArgumentProperties s_argumentProperties;

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
                s_argumentProperties = new ArgumentProperties(args, ARGUMENT_SEPARATOR);
            }
            catch (ArgumentException e)
            {
                HandleArgumentException(e);
            }

            string operation = "";
            try
            {
                // required arguments give no defaults
                operation = s_argumentProperties.GetString(ARGUMENT_OPERATION, null);
                s_pathGame = s_argumentProperties.GetString(ARGUMENT_PATH_GAME, null);
                s_pathBackup = s_argumentProperties.GetString(ARGUMENT_PATH_BACKUP, null);

                // optional arguments *should* never throw when default is supplied 
                s_prefixBackup = s_argumentProperties.GetString(ARGUMENT_PREFIX_BACKUP, PREFIX_BACKUP);
            }
            catch (ArgumentException e)
            {
                HandleArgumentException(e);
            }

            switch (operation)
            {
                case OPERATION_SAVE:
                    DoSave();
                    break;

                case OPERATION_LOAD:
                    DoLoad();
                    break;

                case OPERATION_CLEAN:
                case OPERATION_CLEAR:
                case OPERATION_QUICKLOAD:
                    throw new NotImplementedException();

                default:
                    Console.WriteLine("Unknown operation: " + args[0]);
                    PrintUsage();
                    Environment.Exit(1);
                    break;
            }

            Environment.Exit(0);
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
            // read optional parameters used only by load operation
            s_prefixProtect = s_argumentProperties.GetString(ARGUMENT_PREFIX_PROTECT, PREFIX_PROTECT);
            s_pathProtect = s_argumentProperties.GetString(ARGUMENT_PATH_PROTECT, s_pathBackup);

            Console.WriteLine("Savescum LOADING...");

            // Find latest save - notify and bail out if it isn't found
            string latestBackupPath = FindLatestBackupPath(s_pathBackup, s_prefixBackup);
            if (latestBackupPath.Length == 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("  Error: No backup saves found at ");
                Console.Error.WriteLine("      path: " + s_pathBackup);
                Console.Error.WriteLine("    prefix: " + s_prefixBackup);
                Console.Error.WriteLine();
                Console.Error.WriteLine("  NO FILES CHANGED");
                Console.Error.WriteLine();

                return;
            }

            Console.WriteLine("  Found latest backup at");
            Console.WriteLine("      path: " + latestBackupPath);

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

            PrintCopyInfo(latestBackupPath, s_pathGame);
            DirectoryCopy(latestBackupPath, s_pathGame, true);

            Console.WriteLine();
            Console.WriteLine("LOAD FINISHED from " + latestBackupPath);
            Console.WriteLine();
        }

        private static string FindLatestBackupPath(string pathBackup, string prefixBackup)
        {
            DirectoryInfo backupDirectoryInfo = new DirectoryInfo(pathBackup);

            List<DirectoryInfo> directoryInfos = new List<DirectoryInfo>(
                backupDirectoryInfo.GetDirectories(
                    prefixBackup + "*.*"));

            // nothing found
            if (directoryInfos.Count == 0)
            {
                return "";
            }

            // sort results to find latest by creation time
            IOrderedEnumerable<DirectoryInfo> orderedInfos = directoryInfos.OrderBy(
                directoryInfo =>
                    directoryInfo.CreationTime);

            // newest will be last in the list
            return orderedInfos.Last().FullName;
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

        private static void HandleArgumentException(ArgumentException e)
        {
            PrintArgumentException(e);
            PrintUsage();
            Environment.Exit(1);
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

        static void PrintUsage()
        {
            Console.WriteLine("Usage: Savescum operation=[save|load|clean|clear] [argument=value ...]");
            Console.WriteLine();
        }

        private static void PrintArgumentException(ArgumentException e)
        {
            Console.Error.WriteLine("Error: ");
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
