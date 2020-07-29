using System;
using System.IO;

namespace Savescum
{
    class Program
    {
        private const string OPERATION_SAVE = "save";
        private const string OPERATION_LOAD = "load";
        
        private const string PATH_SAVES     = @"G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\SavedArksLocal";
        private const string PATH_BACKUPS   = @"G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\BackupArksLocal";
        private const string PATH_PROTECT   = @"G:\games\steam\steamapps\common\ARK\ShooterGame\Saved\BackupArksLocal";
        
        private const string PREFIX_BACKUP  = "BackupArk";
        private const string PREFIX_PROTECT = "OverwrittenArk";

        private const string FORMAT_SAVE    = "{0}\\{1}{2:D3}";
        private const string FORMAT_PROTECT = "{0}\\{1}{2:yyyy-MM-dd_hh-mm-ss}";

        private static readonly int MAX_NAME_COUNT = Int32.MaxValue;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string operation = args[0].ToLower();
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
                    break;
            }
        }

        private static void DoLoad()
        {
            Console.WriteLine("LOADING...");

            // Find latest save - no use going past that if it isn't found
            string lastSavePath = FindLastSavePath(PATH_BACKUPS, PREFIX_BACKUP);
            if (lastSavePath.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Error: No saves found at ");
                Console.WriteLine("    path: " + PATH_BACKUPS);
                Console.WriteLine("  prefix: " + PREFIX_BACKUP);
                Console.WriteLine();
                Console.WriteLine("NO FILES CHANGED");
                Console.WriteLine();

                return;
            }

            Console.WriteLine("Found latest backup at");
            Console.WriteLine("    path: " + lastSavePath);

            // Backup existing save directory before writing over it
            string protectDirPath = GenerateProtectPath(PATH_PROTECT, PREFIX_PROTECT);

            if (protectDirPath.Length == 0)
            {
                PrintProtectError(PATH_PROTECT, PREFIX_PROTECT);
                return;
            }

            Console.WriteLine("Backing up directory before writing over it");
            PrintCopyInfo(PATH_SAVES, protectDirPath);
            DirectoryCopy(PATH_SAVES, protectDirPath, true);

            Console.WriteLine("Copying from backup");

            // delete and write over
            DirectoryInfo deleteDir = new DirectoryInfo(PATH_SAVES);
            deleteDir.Delete(true);
            PrintCopyInfo(lastSavePath, PATH_SAVES);
            DirectoryCopy(lastSavePath, PATH_SAVES, true);
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

        private static void PrintProtectError(string path, string prefix)
        {
            Console.WriteLine("Error: Couldn't backup directory before writing over it");
            Console.WriteLine("    Path  = " + path);
            Console.WriteLine("   Prefix = " + prefix);

        }

        private static void DoSave()
        {
            Console.WriteLine("SAVING...");
            string savePath = GenerateSavePath(PATH_BACKUPS, PREFIX_BACKUP);

            if (savePath.Length != 0)
            {
                PrintCopyInfo(PATH_SAVES, savePath);
                DirectoryCopy(PATH_SAVES, savePath, true);
                Console.WriteLine("SAVE FINISHED to " + savePath);
            }

            else
            {
                Console.WriteLine("Error: Couldn't generate save path");
            }
        }

        private static void PrintCopyInfo(string pathSource, string pathDest)
        {
            Console.WriteLine(              "    Copying directory");
            Console.WriteLine(String.Format("      [{0}] ->", pathSource));
            Console.WriteLine(String.Format("      [{0}] ...", pathDest));
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

        static void PrintUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("Usage: Savescum <operation>");
            Console.WriteLine("");
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
    }
}
