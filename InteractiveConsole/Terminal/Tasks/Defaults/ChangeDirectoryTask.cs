using System.IO;

namespace Terminal.Tasks.Defaults
{
    internal class ChangeDirectoryTask : ConsoleTask
    {
        private string _directory;

        public ChangeDirectoryTask()
        {
            SetName("Change current dir");
            SetCommand("cd");
            SetDescription("change the current directory");
        }

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
