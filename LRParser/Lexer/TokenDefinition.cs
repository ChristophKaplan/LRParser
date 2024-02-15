using System.Text.RegularExpressions;
using LRParser.CFG;

namespace LRParser.Lexer;

public class TokenDefinition {
    public TokenDefinition(string symbol, string regex) {
        Symbol = symbol;
        Regex = new Regex(regex);
    }

    private string Symbol { get; }
    public Regex Regex { get; }

    public Terminal CreateTerminal(string value) {
        return new Terminal(Symbol, value);
    }
}