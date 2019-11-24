using System;

namespace Terminal
{
    internal class TerminalException : Exception
    {
        public TerminalException()
        { }

        public TerminalException(string message)
            : base(message)
        { }

        public TerminalException(string message, params object[] obj)
            : base(string.Format(message, obj))
        { }

        public TerminalException(string message, Exception inner)
            : base(message, inner)
        { }

        public TerminalException(Exception inner)
            : base(null, inner)
        { }
    }

    internal class TerminalArgumentException : TerminalException
    {
        public TerminalArgumentException()
        { }

        public TerminalArgumentException(string message)
            : base(message)
        { }

        public TerminalArgumentException(string message, params object[] obj)
            : base(string.Format(message, obj))
        { }

        public TerminalArgumentException(string message, Exception inner)
            : base(message, inner)
        { }

        public TerminalArgumentException(Exception inner)
            : base(null, inner)
        { }
    }

    internal class TerminalTaskDuplicatedCommandException : TerminalException
    {
        public TerminalTaskDuplicatedCommandException()
        { }

        public TerminalTaskDuplicatedCommandException(string message)
            : base(message)
        { }

        public TerminalTaskDuplicatedCommandException(string message, params object[] obj)
            : base(string.Format(message, obj))
        { }

        public TerminalTaskDuplicatedCommandException(string message, Exception inner)
            : base(message, inner)
        { }

        public TerminalTaskDuplicatedCommandException(Exception inner)
            : base(null, inner)
        { }
    }
}
