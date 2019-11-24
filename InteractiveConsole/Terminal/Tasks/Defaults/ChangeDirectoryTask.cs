using System.IO;

namespace Terminal.Tasks.Defaults
{
    internal class ChangeDirectoryTask : ConsoleTask
    {
        public override string Name => "Change current dir";
        public override string Command => "cd";

        private string _directory;

        public override void Parse(string[] args)
        {
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

            Directory.SetCurrentDirectory(di.FullName);
        }
    }
}
