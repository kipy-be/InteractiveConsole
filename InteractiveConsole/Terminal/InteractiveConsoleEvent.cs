using System;

namespace Terminal
{
    internal class ConsoleCommandEmitedEventArgs : EventArgs
    {
        public Command Command { get; private set; }

        public ConsoleCommandEmitedEventArgs(Command command)
        {
            Command = command;
        }
    }
}
