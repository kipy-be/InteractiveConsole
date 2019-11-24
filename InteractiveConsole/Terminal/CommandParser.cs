using System.Collections.Generic;

namespace Terminal
{
    internal class CommandParser
    {
        private string _input;

        private string _action;
        private List<string> _args;

        private string _token;

        private bool _isNewToken;
        private bool _isActionSet;
        private bool _isAction;
        private bool _isArgument;

        private bool _isEnclosed;
        private bool _isBackSlashSet;

        private CommandParser(string input)
        {
            _input = input;
        }

        public static Command Parse(string input)
        {
            return new CommandParser(input).Parse();
        }

        private Command Parse()
        {
            _args = new List<string>();

            if (!string.IsNullOrWhiteSpace(_input))
            {
                _isActionSet = false;
                NewToken();

                foreach (char c in _input)
                {
                    switch (c)
                    {
                        case ' ':
                        case '\t':
                            HandleSpace(c);
                            break;
                        case '\\':
                            HandleBackSlash(c);
                            break;
                        case '"':
                            HandleDoubleQuote(c);
                            break;
                        default:
                            HandleChar(c);
                            break;
                    }
                }

                HandleToken();
            }

            return new Command { Action = _action, Arguments = _args.ToArray() };
        }

        private void HandleBackSlash(char c)
        {
            if (_isBackSlashSet)
            {
                HandleChar(c);
                _isBackSlashSet = false;
            }
            else
            {
                _isBackSlashSet = true;
            }
        }

        private void NewToken()
        {
            _token = string.Empty;

            _isNewToken = true;

            _isAction = false;
            _isArgument = false;

            _isEnclosed = false;
            _isBackSlashSet = false;
        }

        private void SetTokenType()
        {
            _isNewToken = false;

            if (_isActionSet)
            {
                _isArgument = true;
            }
            else
            {
                _isAction = true;
                _isActionSet = true;
            }   
        }

        private void HandleChar(char c)
        {
            if(_isBackSlashSet)
            {
                if(c != '"')
                    _token += "\\";
                _isBackSlashSet = false;
            }

            _token += c;

            if (_isNewToken)
            {
                SetTokenType();
            }
        }

        private void HandleDoubleQuote(char c)
        {
            if (!_isNewToken)
            {
                if (_isBackSlashSet)
                {
                    HandleChar(c);
                }
                else
                {
                    _isEnclosed = false;
                }
            }
            else
            {
                _isEnclosed = true;
                SetTokenType();
            }
        }


        private void HandleSpace(char c)
        {
            if (!_isEnclosed)
            {
                HandleToken();
            }
            else
            {
                HandleChar(c);
            }
        }

        private void HandleToken()
        {
            if (_isAction)
            {
                _action = _token;
            }
            else if (_isArgument)
            {
                _args.Add(_token);
            }

            NewToken();
        }
    }
}
