namespace Terminal.Tasks
{
    internal abstract class ConsoleTask
    {
        public abstract string Name { get; }
        public abstract string Command { get; }
        public virtual string[] Aliases { get; } = new string[0];

        public virtual bool HandleCancel { get; } = false;
        public virtual bool IsExit { get; } = false;

        public virtual void Parse(string[] args)
        {}

        public abstract void Exec();
    }
}
