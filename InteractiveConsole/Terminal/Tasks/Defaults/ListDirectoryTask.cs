using System;
using System.IO;
using System.Linq;

namespace Terminal.Tasks.Defaults
{
    internal class ListDirectoryTask : ConsoleTask
    {
        private string _directory;

        public ListDirectoryTask()
        {
            SetName("List current dir");
            SetCommand("list", "ls");
            SetDescription("display the content of the current directory");
        }

        public override void Parse(string[] args)
        {
            _directory = Directory.GetCurrentDirectory();

            if (args.Length > 0)
            {
                _directory = args[0];
            }
        }

        public override void Exec()
        {
            if(_directory == null)
            {
                throw new TerminalArgumentException("Directory not specified");
            }

            var di = new DirectoryInfo(_directory);

            if (di == null || !di.Exists)
            {
                throw new TerminalArgumentException("Directory does not exists");
            }

            Console.WriteLine("> {0}", string.Join("  ", Directory.EnumerateFileSystemEntries(di.FullName).Select(fse => Path.GetFileName(fse))));
        }
    }
}
