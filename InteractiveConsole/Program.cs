using System;
using System.Diagnostics;
using System.Threading;

namespace InteractiveConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var console = new InteractiveConsole("prompt");
            console.Process();
        }
    }
}
