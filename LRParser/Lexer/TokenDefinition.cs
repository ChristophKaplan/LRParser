using System.Text.RegularExpressions;
using LRParser.CFG;

namespace LRParser.Lexer;

public class TokenDefinition<T> where T : Enum {
    public TokenDefinition(T symbol, string regex) {
        Symbol = symbol;
        Regex = new Regex(regex);
    }

    private T Symbol { get; }
    public Regex Regex { get; }

    public Symbol<T> CreateTerminal(string value) {
        return new Symbol<T>(Symbol, value, SymbolType.Terminal);
    }
}