using System;
using System.Diagnostics;
using System.Threading;

namespace TestAutoCompletion
{
    class Program
    {
        static void Main(string[] args)
        {
            var console = new InteractiveConsole();
            console.Process();
        }
    }
}
