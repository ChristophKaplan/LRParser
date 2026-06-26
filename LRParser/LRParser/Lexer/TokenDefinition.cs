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
            // \G anchors the match to the exact scan position so the engine
            // cannot scan ahead looking for a later match (which made tokenizing
            // O(n^2)). The non-capturing group keeps alternations like "a|b"
            // bound to the anchor. Compiled for repeated use during lexing;
            // CultureInvariant keeps character-class matching locale independent.
            Regex = new Regex($@"\G(?:{regex})", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public Symbol CreateTerminal(string value, (int lineNumber, int linePosition) position)
        {
            var terminal = new Symbol(_symbol, SymbolType.Terminal);
            terminal.SetValue(value);
            terminal.SetPosition(position);
            return terminal;
        }
    }
}
