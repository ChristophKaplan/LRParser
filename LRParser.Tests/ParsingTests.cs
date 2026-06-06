using ExNs = ExampleLang;
using DbgNs = LRParserExample;

namespace LRParser.Tests;

// End-to-end parsing behaviour for the example grammars.
public class ParsingTests
{
    [Fact]
    public void ExampleLang_DeclarationOnly_ReturnsVariableName()
    {
        var result = new ExNs.ExampleLang().TryParse("Int A;");

        Assert.Equal("A", result.ToString());
    }

    [Fact]
    public void ExampleLang_DeclarationAndAssignment_ReturnsValue()
    {
        var result = new ExNs.ExampleLang().TryParse("Int A; A = 50;");

        var intValue = Assert.IsType<ExNs.IntValue>(result);
        Assert.Equal(50, intValue.Value);
    }

    [Fact]
    public void ExampleLang_SyntaxError_Throws()
    {
        // "Int ;" is missing the variable name between the type and the semicolon.
        Assert.Throws<Exception>(() => new ExNs.ExampleLang().TryParse("Int ;"));
    }

    [Fact]
    public void ExampleLang_AssignmentToUndeclaredVariable_Throws()
    {
        // A is declared, but B is assigned without a declaration.
        Assert.Throws<Exception>(() => new ExNs.ExampleLang().TryParse("Int A; B = 5;"));
    }

    [Fact]
    public void DebugLang_SingleIdentifier_Parses()
    {
        var result = new DbgNs.DebugLang().TryParse("A");

        Assert.Equal("A", result.ToString());
    }

    [Fact]
    public void DebugLang_UnrecognizedCharacter_Throws()
    {
        // The lexer rejects characters that match no token rule.
        Assert.Throws<Exception>(() => new DbgNs.DebugLang().TryParse("A % B"));
    }

    [Fact]
    public void SeparateInstances_ParseIndependently()
    {
        var first = new ExNs.ExampleLang().TryParse("Int A; A = 1;");
        var second = new ExNs.ExampleLang().TryParse("Int A; A = 2;");

        Assert.Equal(1, Assert.IsType<ExNs.IntValue>(first).Value);
        Assert.Equal(2, Assert.IsType<ExNs.IntValue>(second).Value);
    }

    [Fact]
    public void SameInstance_MultipleParses_AreIndependent()
    {
        var lang = new ExNs.ExampleLang();

        // Re-declaring the same variable across calls would previously throw
        // (accumulated TypeTable) and/or corrupt the reused syntax tree.
        var first = lang.TryParse("Int A; A = 1;");
        var second = lang.TryParse("Int A; A = 2;");

        Assert.Equal(1, Assert.IsType<ExNs.IntValue>(first).Value);
        Assert.Equal(2, Assert.IsType<ExNs.IntValue>(second).Value);
    }
}
