namespace Terminal.Tasks.Defaults
{
    internal class ExitTask : ConsoleTask
    {
        public override bool IsExit => true;

        public ExitTask()
        {
            SetName("Exit");
            SetCommand("exit", "quit");
            SetDescription("exit the application and stop all its running processes");
        }
    }
}
