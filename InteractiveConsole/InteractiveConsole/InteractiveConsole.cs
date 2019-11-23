using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace InteractiveConsole
{
    public class InputToken
    {
        public string Token { get; private set; }
        public int StartIndex { get; private set; }
        public bool IsValid => !string.IsNullOrEmpty(Token);
        public int Length => Token.Length;

        public InputToken(string token, int startIndex)
        {
            Token = token;
            StartIndex = startIndex;
        }
    }

    public class InteractiveConsole
    {
        private static int MAX_SUGGESTIONS = 16;

        private string _promptName;
        private int _promptLength;

        private bool _exit = false;

        private string _input;
        private string _info;
        private int _cursorPosition;
        private bool _print;
        private bool _printInfo;
        private bool _validate;
        private bool _setCursor;

        private EnumerationOptions _enumerationOptions = new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive };
        private string _currentDirectory = Directory.GetCurrentDirectory();

        public InteractiveConsole(string promptName)
        {
            _promptName = promptName;
            _promptLength = _promptName.Length + 3;
        }

        public void Process()
        {
            ConsoleKeyInfo key;
            char lastKey = '\0';
            DateTime lastTimeKeyPress = DateTime.MinValue;
            bool doubleKeyPress;

            _input = string.Empty;
            _cursorPosition = 0;

            WritePrompt();

            do
            {
                key = Console.ReadKey(true);
                doubleKeyPress = lastKey == key.KeyChar && (DateTime.UtcNow - lastTimeKeyPress).TotalMilliseconds < 500;
                lastTimeKeyPress = DateTime.UtcNow;

                _validate = false;
                _print = false;
                _printInfo = false;
                _setCursor = false;

                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                        AddChar(key.KeyChar);
                        break;
                    case ConsoleKey.Tab:
                        if (doubleKeyPress)
                        {
                            AutoComplete();
                        }
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

                lastKey = key.KeyChar;

                SetCursorVisible(false);

                if (_print)
                {
                    WriteCurrentLine(_validate);
                }

                if (_setCursor)
                {
                    Console.SetCursorPosition(_cursorPosition + _promptLength, Console.CursorTop);
                }

                SetCursorVisible(true);

            } while (!_exit);
        }

        private void SetCursorVisible(bool visible)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.CursorVisible = visible;
            }
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

        private InputToken GetCurrentToken()
        {
            if (_cursorPosition == 0)
                return null;

            var sb = new StringBuilder();

            int i = _cursorPosition - 1;
            for (; i >= 0; i--)
            {
                if(_input[i] == ' ')
                {
                    break;
                }

                sb.Insert(0, _input[i]);
            }

            return new InputToken(sb.ToString(), i + 1);
        }

        private string[] GetPathSuggestions(string dir, string token)
        {
            int i = 0;
            var suggestions = new List<string>();

            foreach(string file in Directory.EnumerateFileSystemEntries(dir, $"{token}*", _enumerationOptions))
            {
                suggestions.Add(Path.GetFileName(file));
                
                if (++i > MAX_SUGGESTIONS)
                {
                    break;
                }
            }

            return suggestions.ToArray();
        }

        private string GetCommonFromSuggestions(string[] suggestions)
        {
            char c;
            int i;
            int u = 0;
            string res = string.Empty;

            while (true)
            {
                if (u >= suggestions[0].Length)
                {
                    return res;
                }

                c = suggestions[0][u];

                for (i = 1; i < suggestions.Length; i++)
                {
                    if(u >= suggestions[i].Length || c != suggestions[i][u])
                    {
                        return res;
                    }
                }

                u++;
                res += c;
            }
        }

        private void ReplaceToken(InputToken token, string oldValue, string newValue)
        {
            int index = token.StartIndex;

            if (!string.IsNullOrEmpty(oldValue))
            {
                index += token.Token.LastIndexOf(oldValue);
            }
            else
            {
                index += token.Token.Length;
            }
            
            int count = token.StartIndex + token.Length - index;

            _input = _input.Remove(index, count);
            _cursorPosition -= count;

            _input = _input.Insert(_cursorPosition, newValue);
            _cursorPosition += newValue.Length;

            _print = true;
            _setCursor = true;
        }

        private string GetPathEnd(string path)
        {
            var sb = new StringBuilder();

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (_input[i] == '/' || _input[i] == '\\')
                {
                    break;
                }

                sb.Insert(0, _input[i]);
            }

            string res = sb.ToString();
            if (res.Length > 0 && !char.IsLetterOrDigit(res[res.Length - 1]))
            {
                return null;
            }

            return res;
        }

        private void AutoComplete()
        {
            var inputToken = GetCurrentToken();

            if(inputToken == null || !inputToken.IsValid)
            {
                return;
            }

            string dir = new FileInfo(inputToken.Token).DirectoryName;

            if (dir == null)
            {
                dir = new DirectoryInfo(inputToken.Token).FullName;
            }

            dir = Path.Combine(_currentDirectory, dir);
            string pathEnd = GetPathEnd(inputToken.Token);

            if (pathEnd == null)
            {
                return;
            }

            var suggestions = GetPathSuggestions(dir, pathEnd);
            if (suggestions.Length == 0)
            {
                return;
            }

            if (suggestions.Length == 1)
            {
                ReplaceToken(inputToken, pathEnd, suggestions[0]);
                return;
            }

            string common = GetCommonFromSuggestions(suggestions);
            if(common != pathEnd)
            {
                ReplaceToken(inputToken, pathEnd, common);
                return;
            }

            if(suggestions.Length > MAX_SUGGESTIONS)
            {
                _info = $"{string.Join("  ", suggestions, 0, MAX_SUGGESTIONS)}  (...)";
            }
            else
            {
                _info = $"{string.Join("  ", suggestions)}";
            }

            
            _print = true;
            _printInfo = true;
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

            if (_printInfo)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(_info);
                Console.ResetColor();
                Console.WriteLine();
                _printInfo = false;
                WriteCurrentLine(validate);
                return;
            }

            if (validate)
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
