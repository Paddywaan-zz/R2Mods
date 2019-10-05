using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamGameLocation
{
    class Program
    {
        static FileSystemTest reggetter = new FileSystemTest(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath");
        static void Main(string[] args)
        {
            if (args[0].Equals("/?") || args[0].Equals("help"))
            {
                Console.WriteLine("Syntax: queryGameLocation [Game/Folder name] [GameID]");
            }
            else queryGameLocation(args);

            //Console.WriteLine("Testing Dishonored");
            //Console.WriteLine(reggetter.GetGameFolder("Dishonored", "205100"));
            //Console.WriteLine("Testing Risk of Rain 2");
            //Console.WriteLine(reggetter.GetGameFolder("Risk of Rain 2", "632360"));
            //Console.WriteLine("Testing Might and Magic Heroes VI");
            //Console.WriteLine(reggetter.GetGameFolder("Might and Magic Heroes VI", "48220"));
            //Console.WriteLine("Testing BioShock");
            //Console.WriteLine(reggetter.GetGameFolder("BioShock", "7670"));

        }
        static void queryGameLocation(string[] args)
        {
            Console.WriteLine(reggetter.GetGameFolder(args[0], args[1]));
        }
    }
}
