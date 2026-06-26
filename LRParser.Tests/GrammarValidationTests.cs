using DbgNs = LRParserExample;
using ExNs = ExampleLang;

namespace LRParser.Tests;

// The grammar is validated when the augmented start production is inserted:
// non-terminals reachable from the start symbol must be defined and productive.
public class GrammarValidationTests
{
    [Fact]
    public void ReachableNonTerminalWithoutProductions_ThrowsAtConstruction()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new UndefinedNonTerminalGrammar());
        Assert.Contains("no productions", ex.Message);
    }

    [Fact]
    public void UnproductiveNonTerminal_ThrowsAtConstruction()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new UnproductiveGrammar());
        Assert.Contains("not productive", ex.Message);
    }

    [Fact]
    public void ValidExampleGrammars_PassValidation()
    {
        // These build their tables (and therefore validate) without throwing.
        _ = new ExNs.ExampleLang();
        _ = new DbgNs.DebugLang();
    }
}
