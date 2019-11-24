namespace Terminal
{
    internal class InputToken
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
}
