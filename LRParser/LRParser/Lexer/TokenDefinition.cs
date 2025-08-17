using System;
using System.Text.RegularExpressions;
using LRParser.CFG;

namespace LRParser.Lexer
{
    public class TokenDefinition<T> where T : Enum
    {
        private readonly T _symbol;
        public Regex Regex { get; }

        public TokenDefinition(T symbol, string regex)
        {
            _symbol = symbol;
            Regex = new Regex(regex);
        }

        public Symbol CreateTerminal(string value)
        {
            var terminal = new Symbol(_symbol, SymbolType.Terminal);
            terminal.SetValue(value);
            return terminal;
        }
    }
}
