using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Commands;
using Terminal.Tasks;
using Terminal.Tasks.Defaults;

namespace Terminal
{
    internal class InteractiveConsole
    {
        private static int MAX_SUGGESTIONS = 16;

        private AutoResetEvent _onAction = new AutoResetEvent(false);
        private AutoResetEvent _waitNextAction = new AutoResetEvent(false);

        private string _promptName;
        private int _promptLength;

        private bool _exit = false;
        private bool _forceExit = false;

        private Dictionary<string, ConsoleTask> _tasks;
        private ConsoleTask _currentTask;

        private ConsoleKeyInfo _key;
        private string _input;
        private string _info;
        private int _cursorPosition;
        private bool _print;
        private bool _printInfo;
        private bool _validate;
        private bool _setCursor;

        private List<string> _history = new List<string>();
        private int _historyIndex = 0;

        private EnumerationOptions _enumerationOptions = new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive };
        private string _currentDirectory = Directory.GetCurrentDirectory();

        public event EventHandler OnExit;

        public InteractiveConsole(string promptName)
        {
            _promptName = promptName;
            _promptLength = _promptName.Length + 3;

            _tasks = new Dictionary<string, ConsoleTask>();
            AddDefaultsTasks();
        }

        private void AddDefaultsTasks()
        {
            AddTask<ExitTask>();
            AddTask<GetCurrentDirectoryTask>();
            AddTask<ChangeDirectoryTask>();
        }

        public void AddTask<T>()
            where T : ConsoleTask, new()
        {
            T task = new T();

            if (_tasks.ContainsKey(task.Command))
            {
                throw new TerminalTaskDuplicatedCommandException();
            }
            _tasks.Add(task.Command.ToLower(), task);

            foreach(var alias in task.Aliases)
            {
                if (_tasks.ContainsKey(alias))
                {
                    throw new TerminalTaskDuplicatedCommandException();
                }
                _tasks.Add(alias.ToLower(), task);
            }
        }

        private void StartKeyReading()
        {
            var t = Task.Run(() =>
            {
                while (!_exit && !_forceExit)
                {
                    _key = Console.ReadKey(true);
                    _onAction.Set();
                    _waitNextAction.WaitOne();
                }
            });
        }

        public void Start()
        {
            try
            {
                _exit = false;
                _forceExit = false;

                Console.CancelKeyPress += OnCancelKeyPress;
                var currentProcess = Process.GetCurrentProcess();
                currentProcess.Exited += OnExited;
                AppDomain.CurrentDomain.ProcessExit += OnExited;
                AppDomain.CurrentDomain.DomainUnload += OnExited;

                char lastKey = '\0';
                DateTime lastTimeKeyPress = DateTime.MinValue;
                bool doubleKeyPress;

                _input = string.Empty;
                _cursorPosition = 0;

                WritePrompt();

                StartKeyReading();

                while (true)
                {
                    _onAction.WaitOne();

                    if (_exit || _forceExit)
                    {
                        break;
                    }

                    doubleKeyPress = lastKey == _key.KeyChar && (DateTime.UtcNow - lastTimeKeyPress).TotalMilliseconds < 500;
                    lastTimeKeyPress = DateTime.UtcNow;

                    _validate = false;
                    _print = false;
                    _printInfo = false;
                    _setCursor = false;

                    switch (_key.Key)
                    {
                        case ConsoleKey.Spacebar:
                            AddChar(_key.KeyChar);
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
                        case ConsoleKey.UpArrow:
                            GoPreviousHistory();
                            break;
                        case ConsoleKey.DownArrow:
                            GoNextHistory();
                            break;
                        default:
                            AddChar(_key.KeyChar);
                            break;
                    }

                    lastKey = _key.KeyChar;

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

                    _waitNextAction.Set();

                    if (_exit || _forceExit)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("> Error : {0}", ex.Message);
            }
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

        private void GoPreviousHistory()
        {
            if(_historyIndex == 0)
            {
                return;
            }

            _input = _history[--_historyIndex];
            _cursorPosition = _input.Length;
            _print = true;
            _setCursor = true;
        }

        private void GoNextHistory()
        {
            if (_historyIndex + 1 >= _history.Count)
            {
                return;
            }

            _input = _history[++_historyIndex];
            _cursorPosition = _input.Length;
            _print = true;
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

        private bool IsAllowedPathChar(char c)
        {
            switch(c)
            {
                case '\\':
                case '/':
                case ':':
                case '*':
                case '?':
                case '"':
                case '<':
                case '>':
                case '|':
                    return false;
                default:
                    return true;
            }
        }

        private string GetPathEnd(string path)
        {
            var sb = new StringBuilder();

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '/' || path[i] == '\\')
                {
                    break;
                }

                sb.Insert(0, path[i]);
            }

            string res = sb.ToString();
            if (res.Length > 0 && !IsAllowedPathChar(res[res.Length - 1]))
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
            if (!string.IsNullOrEmpty(common) && common != pathEnd)
            {
                ReplaceToken(inputToken, pathEnd, common);
                return;
            }

            if (suggestions.Length > MAX_SUGGESTIONS)
            {
                _info = $"{string.Join("  ", suggestions, 0, MAX_SUGGESTIONS)}\n({(suggestions.Length - MAX_SUGGESTIONS)} more...)";
            }
            else
            {
                _info = $"{string.Join("  ", suggestions)}";
            }

            
            _print = true;
            _printInfo = true;
            _setCursor = true;
        }

        private void AddToHistory(string line)
        {
            _history.Add(line);
            _historyIndex = _history.Count;
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
                AddToHistory(_input);
                var command = CommandParser.Parse(_input);
                HandleCommand(command);

                if (!_exit)
                {
                    Console.WriteLine();
                    WritePrompt();
                    _input = string.Empty;
                    _cursorPosition = 0;
                    _setCursor = true;
                }
            }
        }

        private void HandleCommand(Command cmd)
        {
            ConsoleTask task = null;

            try
            {
                if (cmd.Action == null)
                {
                    return;
                }

                if (!_tasks.TryGetValue(cmd.Action.ToLower(), out task))
                {
                    Console.WriteLine("> Unknown command");
                    return;
                }

                _currentTask = task;

                task.Parse(cmd.Arguments);
                task.Exec();

                _exit = task.IsExit;

                _currentTask = null;
            }
            catch (TerminalArgumentException ex)
            {
                Console.WriteLine("> {0} error : {1}", task.Name, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("> {0} error : {1}", task.Name, ex.Message);
            }
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _forceExit = !(_currentTask?.HandleCancel == true);
            e.Cancel = true;

            if (_forceExit)
            {
                _onAction.Set();
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            RaiseOnExitEvent();
        }

        private void RaiseOnExitEvent()
        {
            OnExit?.Invoke(this, null);
        }
    }
}
