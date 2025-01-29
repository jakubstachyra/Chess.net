using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Services
{
    public class Chess960Service
    {
        public static List<string> FenList = new List<string>();
        private static readonly Random _random = new Random();


        public static void LoadFens()
        {

            string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(directoryPath, "Chess960.txt");

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                FenList.AddRange(lines);
            }

        }
        public static string RandomStartFen()
        {
            if (FenList.Count == 0) return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
            int index = _random.Next(FenList.Count);
            return FenList[index];

        }
    }

}
