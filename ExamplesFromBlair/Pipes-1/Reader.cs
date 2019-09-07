using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipes
{
    class Reader
    {
        // This section makes the In (here called _in, representing Console.In) available to the code in DoSomething().
        private TextReader _in;
        public Reader(TextReader @in)
        {
            _in = @in;
        }

        // This section does something with our Out, which our Writer so graciously provided.
        // A real program would have access to the command-line arguments to control what this section does, but since
        // this is an incomplete simulation of two separate processes, it employs really simple logic.
        public void DoSomething()
        {
            // I copy each line of text to the console, then exit when there is no more.
            //for (; ; )
            //{
            //    //var throwaway = _AppDomain == null
            //    //    ? "What!?"
            //    //    : _in == null
            //    //        ? "Not cool"
            //    //        : "Cool";
            //    var line = _in.ReadLine();
            //    if (line == null) { return; }
            //    Console.WriteLine(line);
            //}

            var lines = new List<string>();
            for (var line = _in.ReadLine(); line != null; line = _in.ReadLine())
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
