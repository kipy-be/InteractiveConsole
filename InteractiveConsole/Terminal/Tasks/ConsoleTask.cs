using System.Collections.Generic;

namespace Terminal.Tasks
{
    internal abstract class ConsoleTask
    {
        public string Name { get; private set; }
        public string Command { get; private set; }
        public string[] Aliases { get; private set; } = new string[0];

        public string[] Description { get; private set; }
        public List<ConsoleTaskArgumentDescription> ArgumentsDescriptions { get; private set; } = new List<ConsoleTaskArgumentDescription>();

        public virtual bool HandleCancel { get; } = false;
        public virtual bool IsExit { get; } = false;

        public virtual void Parse(string[] args)
        {}

        public virtual void Exec()
        {}

        protected void SetName(string name)
        {
            Name = name;
        }

        protected void SetCommand(string command, params string[] aliases)
        {
            Command = command;
            Aliases = aliases;
        }

        protected void SetDescription(params string[] description)
        {
            Description = description;
        }

        protected void SetArgumentDescription(string argument, params string[] description)
        {
            ArgumentsDescriptions.Add(new ConsoleTaskArgumentDescription(argument, description));
        }
    }
}
