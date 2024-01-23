using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Tasks.Defaults
{
    internal class HelpTask : ConsoleTask
    {
        private List<ConsoleTask> _tasks;
        private Dictionary<string, ConsoleTask> _tasksByCommand;
        
        private string _command;

        public HelpTask(List<ConsoleTask> tasks, Dictionary<string, ConsoleTask> tasksByCommand)
        {
            _tasks = tasks;
            _tasksByCommand = tasksByCommand;

            SetName("Show help");
            SetCommand("help");
            SetDescription("show help for the given command, list the available commands otherwise");
            AddArgumentDescription
            (
                "COMMAND",
                "specify the command you want help for",
                "if not specified or the command does not exist, the help will show all the available commands"
            );
        }

        private void ShowCommandsList()
        {
            Console.WriteLine("> Commands list :");

            foreach(var t in _tasks.OrderBy(t => t.Command))
            {
                Console.WriteLine("\t{0}", t.Command);
            }

            Console.WriteLine();
            Console.WriteLine("'help <command>' for further details");
        }

        public override void Parse(string[] args)
        {
			_command = null;
			
            if (args.Length > 0)
            {
                _command = args[0].ToLower();
            }
        }

        public override void Exec()
        {
            if (_command == null)
            {
                ShowCommandsList();
                return;
            }

            ConsoleTask task;
            if (!_tasksByCommand.TryGetValue(_command, out task))
            {
                ShowCommandsList();
                return;
            }

            Console.WriteLine("> Command :");
            Console.WriteLine("\t{0}", task.Command);
            Console.WriteLine();

            if (task.Aliases.Length > 0)
            {
                Console.WriteLine("> Aliases :");
                foreach (var alias in task.Aliases)
                {
                    Console.WriteLine("\t{0}", alias);
                }
                Console.WriteLine();
            }

            Console.WriteLine("> Description :");
            foreach (var line in task.Description)
            {
                Console.WriteLine("\t{0}", line);
            }
            Console.WriteLine();

            if (task.ArgumentsDescriptions.Count > 0)
            {
                Console.WriteLine("> Arguments :");
                foreach (var ad in task.ArgumentsDescriptions)
                {
                    Console.WriteLine("\t{0}", ad.Argument);
                    foreach (var line in ad.Description)
                    {
                        Console.WriteLine("\t\t{0}", line);
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
