using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

/// <summary>
/// Credit to @MisterName
/// </summary>
namespace SteamGameLocation
{
    public class FileSystem
    {
        public FileSystem(string steamRegKeyLocation, string regValue)
        {
            //Register = new Register();
            RegKey = steamRegKeyLocation;
            RegValue = regValue;
        }

        //public IRegister Register
        //{
        //    get;
        //    private set;
        //}

        public string RegKey
        {
            get;
            private set;
        }

        public string RegValue
        {
            get;
            private set;
        }

        //const string RegKey = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        //const string RegValue = "SteamPath";
        //const string ROR2_STEAMNAME = "Risk of Rain 2";
        //const string ROR2_STEAMID = "632360";
        public string GetGameFolder(string gameName, string gameID)
        {
            object regValue = (string)Registry.GetValue(RegKey, RegValue, string.Empty);
            if (regValue != null && (string)regValue != string.Empty)
            {
                string path = (string)regValue + @"\steamapps";
                string[] filePaths = GetACF(path);

                var gamePath = CheckPath(gameName, gameID, path, filePaths);

                if (!string.IsNullOrEmpty(gamePath))
                {
                    return gamePath;
                }
                else
                {
                    return FindinLibraryFolder(gameName, gameID, path);
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"{gameName} folder not found");
            }
        }

        private static string[] GetACF(string path)
        {
            return Directory.GetFiles(path, "*.acf");
        }

        private static string CheckPath(string gameName, string gameID, string path, string[] filePaths)
        {
            //Console.WriteLine("Filepath0: " +filePaths[0]);
            if (filePaths.Any(filepath => filepath.EndsWith($"appmanifest_{gameID}.acf")))
            {
                string gamePath = path + $"\\common\\{gameName}";
                //Console.WriteLine("Checking Path: " + gamePath);
                if (Directory.Exists(gamePath))
                {
                    //Console.WriteLine("CheckPath Found: " + gamePath);
                    return gamePath;
                }
            }

            return string.Empty;
        }

        static string FindinLibraryFolder(string gameName, string gameID, string path)
        {
            string libpath = Path.Combine(path, "libraryfolders.vdf");
            if (File.Exists(libpath))
            {
                try
                {
                    List<string> folders = new List<string>();
                    using (StreamReader streamReader = new StreamReader(File.OpenRead(libpath)))
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
                                        //System.Console.WriteLine(word);
                                    }
                                }
                            }
                        }
                    }

                    foreach (string folder in folders)
                    {
                        string steamappsPath = folder + @"\steamapps";
                        //Console.WriteLine(steamappsPath);
                        string[] files = GetACF(steamappsPath);
                        var gamePath = CheckPath(gameName, gameID, steamappsPath, files);
                        //Console.WriteLine("GamePath: " + gamePath);
                        if (!string.IsNullOrEmpty(gamePath))
                        {
                            //Console.WriteLine("Found GamePath, returning:" + gamePath);
                            return gamePath;
                        }
                    }
                    //throw new DirectoryNotFoundException();
                }
                finally { }
            } else Console.WriteLine(path + "Does not exist");

            return string.Empty;
        }
    }
}
