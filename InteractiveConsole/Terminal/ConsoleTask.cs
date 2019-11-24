namespace Terminal
{
    internal abstract class ConsoleTask
    {
        public bool HandleCancel { get; protected set; } = false;

        public virtual void Parse(string[] args)
        {}

        public abstract void Exec();
    }
}
