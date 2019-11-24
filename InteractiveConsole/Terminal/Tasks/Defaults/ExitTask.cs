namespace Terminal.Tasks.Defaults
{
    internal class ExitTask : ConsoleTask
    {
        public override string Name => "Exit";
        public override string Command => "exit";
        public override string[] Aliases => new string[] { "quit" };
        public override bool IsExit => true;

        public override void Exec()
        {}
    }
}
