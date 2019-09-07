using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader
{
    class Program
    {
        static void Main(string[] args)
        {
            // I copy each line of text to the console, then exit when there is no more.

            var lines = new List<string>();
            for (var line = Console.ReadLine(); line != null; line = Console.ReadLine())
            {
                lines.Add(line);
            }
            lines.Reverse();
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }
    }
}
