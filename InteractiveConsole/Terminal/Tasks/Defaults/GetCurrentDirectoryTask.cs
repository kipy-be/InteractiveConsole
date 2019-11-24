using System;
using System.IO;

namespace Terminal.Tasks.Defaults
{
    internal class GetCurrentDirectoryTask : ConsoleTask
    {
        public GetCurrentDirectoryTask()
        {
            SetName("Get current dir");
            SetCommand("pwd");
            SetDescription("display the current directory");
        }

        public override void Exec()
        {
            Console.WriteLine("> {0}", Directory.GetCurrentDirectory());
        }
    }
}
