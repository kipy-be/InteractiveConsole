using System;

namespace TestAutoCompletion
{
    public class InteractiveConsole
    {
        private bool _exit = false;
        private string _input;
        private int _cursorPosition;
        private bool _print;
        private bool _setCursor;

        public void Process()
        {
            ConsoleKeyInfo key;

            _input = string.Empty;
            _cursorPosition = 0;

            do
            {
                key = Console.ReadKey(true);
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
                        ValidateLine();
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

                if (_print)
                {
                    WriteCurrentLine(_input);
                }

                if (_setCursor)
                {
                    Console.SetCursorPosition(_cursorPosition, Console.CursorTop);
                }

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

        private void WriteCurrentLine(string line, params object[] args)
        {
            if (args.Length > 0)
            {
                line = string.Format(line, args);
            }
            
            Console.Write("\r");
            Console.Write(line);
            Console.Write(new string(' ', Console.BufferWidth - line.Length));
        }

        private void ValidateLine()
        {
            WriteCurrentLine(_input);
            Console.WriteLine();
            _input = string.Empty;
            _cursorPosition = 0;
            _setCursor = true;
        }
    }
}
