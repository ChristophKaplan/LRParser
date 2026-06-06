using LRParser.CFG;
using LRParser.Parser;
using DbgNs = LRParserExample;

namespace LRParser.Tests;

// Finding #7 (fixed): Parser.Parse used to mutate the caller's input list (it
// appended Symbol.Dollar and removed consumed tokens). It now works on a copy,
// so the caller's list is left untouched and can be reused.
public class ParserMutationTests
{
    private static Symbol Identifier(string value)
    {
        var symbol = new Symbol(DbgNs.Terminal.Identifier, SymbolType.Terminal);
        symbol.SetValue(value);
        return symbol;
    }

    [Fact]
    public void Parse_DoesNotMutateCallerInputList()
    {
        var lang = new DbgNs.DebugLang();
        var parser = new Parser<DbgNs.Terminal, DbgNs.NonTerminal>(lang);

        var input = new List<Symbol> { Identifier("A"), Identifier("B"), Identifier("C") };
        var countBefore = input.Count;

        parser.Parse(input, out _);

        Assert.Equal(countBefore, input.Count);
        Assert.DoesNotContain(input, s => s.IsDollar);
    }
}
