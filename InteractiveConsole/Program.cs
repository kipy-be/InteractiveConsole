using Terminal;

namespace InteractiveConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var console = new InteractiveConsole("prompt");
            console.Start();
        }
    }
}
