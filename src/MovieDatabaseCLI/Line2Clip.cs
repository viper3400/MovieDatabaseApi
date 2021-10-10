using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace MovieDatabaseCLI
{
    public class Line2Clip
    {
        private readonly string[] input;

        public Line2Clip(string inputFile)
        {
            input = File.ReadAllLines(inputFile);
        }

        public void Start()
        {
            var linecount = input.Length;
            foreach(var line in input)
            {
                Console.Clear();
                ClipboardService.SetText(line);
                Console.WriteLine(linecount);
                Console.WriteLine(line);
                Console.ReadLine();
                linecount--;
            }
        }
    }
}
