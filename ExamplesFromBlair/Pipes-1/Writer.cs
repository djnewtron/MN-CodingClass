using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipes
{
    class Writer
    {
        // This section makes the Out (here called _out, representing Console.Out) available to the code in DoSomething().
        private TextWriter _out;
        public Writer(TextWriter @out)
        {
            _out = @out;
        }

        // This section does something with our Out, which our Reader should process somehow.
        // A real program would have access to the command-line arguments to control what this section does, but since
        // this is an incomplete simulation of two separate processes, it employs really simple logic.
        public void DoSomething()
        {
            // I write four lines of text.
            _out.WriteLine("This is line 1");
            //System.Threading.Thread.Sleep(1000);
            _out.WriteLine("This is line 2");
            //System.Threading.Thread.Sleep(1000);
            _out.WriteLine("This is line 3");
            //System.Threading.Thread.Sleep(1000);
            _out.WriteLine("This is line 4");
        }
    }
}
