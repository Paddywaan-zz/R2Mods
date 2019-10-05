using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SteamGameLocation
{
    public class FileSystemTest
    {
        public FileSystemTest(string steamRegKeyLocation, string regValue)
        {
            object folderObject = Registry.GetValue(steamRegKeyLocation, regValue, string.Empty);
            if (folderObject != null && (string)folderObject != string.Empty)
            {
                
                SteamFolder = ((string)folderObject).Replace('/', '\\');
                Console.WriteLine(SteamFolder);
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        public string SteamFolder { get; private set; }

        public string MainGameFolder
        {
            get { return SteamFolder + @"\steamapps"; }
        }

        private string[] GetACFFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.acf");
        }

        //const string RegKey = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        //const string RegValue = "SteamPath";
        //const string ROR2_STEAMNAME = "Risk of Rain 2";
        //const string ROR2_STEAMID = "632360";
        public string GetGameFolder(string gameName, string gameID)
        {
            var gamePath = CheckFolder(gameName, gameID, MainGameFolder);

            if (!string.IsNullOrEmpty(gamePath))
            {
                return gamePath;
            }

            gamePath = FindinLibraryFolder(gameName, gameID);
            if (!string.IsNullOrEmpty(gamePath))
            {
                return gamePath;
            }

            throw new DirectoryNotFoundException($"{gameName} folder not found");

        }

        private string CheckFolder(string gameName, string gameID, string folder)
        {
            string[] filePaths = GetACFFiles(folder);
            if (filePaths.Any(filepath => filepath.EndsWith($"appmanifest_{gameID}.acf")))
            {
                string gamePath = folder + $"\\common\\{gameName}";

                if (Directory.Exists(gamePath))
                {
                    return gamePath.Replace("\\\\", "\\");
                }
            }

            return string.Empty;
        }

        private string FindinLibraryFolder(string gameName, string gameID)
        {
            string[] folders = GetSecondaryFolders();
            foreach (string folder in folders)
            {
                string steamappsPath = folder + @"\steamapps";
                var gamePath = CheckFolder(gameName, gameID, steamappsPath);

                if (!string.IsNullOrEmpty(gamePath))
                {
                    return gamePath;
                }
            }

            return string.Empty;
        }

        private string[] GetSecondaryFolders()
        {
            var libraryfolders = Path.Combine(MainGameFolder, "libraryfolders.vdf");
            if (File.Exists(libraryfolders))
            {
                List<string> folders = new List<string>();
                using (StreamReader streamReader = new StreamReader(File.OpenRead(libraryfolders)))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (line.Contains(@"\\"))
                        {
                            var words = line.Split('"');
                            foreach (var word in words)
                            {
                                if (word.Contains(@"\\"))
                                {
                                    folders.Add(word);
                                }
                            }
                        }
                    }
                }

                return folders.ToArray();
            }
            throw new FileNotFoundException();
        }
    }
}