namespace InteractiveConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var console = new InteractiveConsole("prompt");
            console.Process();
        }
    }
}
