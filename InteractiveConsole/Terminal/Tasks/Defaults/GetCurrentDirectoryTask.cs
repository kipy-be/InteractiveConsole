using System;
using System.IO;

namespace Terminal.Tasks.Defaults
{
    internal class GetCurrentDirectoryTask : ConsoleTask
    {
        public override string Name => "Get current dir";
        public override string Command => "pwd";

        public override void Exec()
        {
            Console.WriteLine("> {0}", Directory.GetCurrentDirectory());
        }
    }
}
