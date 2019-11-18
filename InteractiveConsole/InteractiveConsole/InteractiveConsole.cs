using System;

namespace InteractiveConsole
{
    public class InteractiveConsole
    {
        private string _promptName;
        private int _promptLength;

        private bool _exit = false;

        private string _input;
        private int _cursorPosition;
        private bool _print;
        private bool _validate;
        private bool _setCursor;

        public InteractiveConsole(string promptName)
        {
            _promptName = promptName;
            _promptLength = _promptName.Length + 3;
        }

        public void Process()
        {
            ConsoleKeyInfo key;

            _input = string.Empty;
            _cursorPosition = 0;

            WritePrompt();

            do
            {
                key = Console.ReadKey(true);
                _validate = false;
                _print = false;
                _setCursor = false;

                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                        AddChar(key.KeyChar);
                        break;
                    case ConsoleKey.Tab:
                        break;
                    case ConsoleKey.Enter:
                        Validate();
                        break;
                    case ConsoleKey.Delete:
                        DeleteNextChar();
                        break;
                    case ConsoleKey.Backspace:
                        DeletePreviousChar();
                        break;
                    case ConsoleKey.LeftArrow:
                        GoPreviousChar();
                        break;
                    case ConsoleKey.RightArrow:
                        GoNextChar();
                        break;
                    default:
                        AddChar(key.KeyChar);
                        break;
                }

                Console.CursorVisible = false;

                if (_print)
                {
                    WriteCurrentLine(_validate);
                }

                if (_setCursor)
                {
                    Console.SetCursorPosition(_cursorPosition + _promptLength, Console.CursorTop);
                }

                Console.CursorVisible = true;

            } while (!_exit);
        }

        private void AddChar(char c)
        {
            _input = _input.Insert(_cursorPosition, c.ToString());
            _cursorPosition++;
            _print = true;
            _setCursor = true;
        }

        private void DeleteNextChar()
        {
            if (_cursorPosition == _input.Length)
            {
                return;
            }

            _input = _input.Remove(_cursorPosition, 1);
            _print = true;
            _setCursor = true;
        }

        private void DeletePreviousChar()
        {
            if(_cursorPosition == 0)
            {
                return;
            }

            _input = _input.Remove(--_cursorPosition, 1);
            _print = true;
            _setCursor = true;
        }

        private void GoPreviousChar()
        {
            if (_cursorPosition == 0)
            {
                return;
            }

            --_cursorPosition;
            _setCursor = true;
        }

        private void GoNextChar()
        {
            if (_cursorPosition == _input.Length)
            {
                return;
            }

            ++_cursorPosition;
            _setCursor = true;
        }

        private void Validate()
        {
            _print = true;
            _validate = true;
        }

        private void WritePrompt()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("@{0}> ", _promptName);
            Console.ResetColor();
        }

        private void WriteCurrentLine(bool validate)
        {
            Console.Write("\r");
            WritePrompt();
            Console.Write(_input);
            Console.Write(new string(' ', Console.BufferWidth - _promptLength - _input.Length));

            if(validate)
            {
                Console.WriteLine();
                WritePrompt();
                _input = string.Empty;
                _cursorPosition = 0;
                _setCursor = true;
            }
        }
    }
}
