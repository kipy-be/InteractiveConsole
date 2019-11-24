namespace Terminal.Tasks
{
    internal class ConsoleTaskArgumentDescription
    {
        public string Argument { get; set; }
        public string[] Description { get; set; }

        public ConsoleTaskArgumentDescription(string argument, string[] description)
        {
            Argument = argument;
            Description = description;
        }
    }
}
