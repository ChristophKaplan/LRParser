using LRParser.Lexer;
using LRParser.Parser;
using ExNs = ExampleLang;
using DbgNs = LRParserExample;

namespace LRParser.Tests;

// End-to-end parsing behaviour for the example grammars.
public class ParsingTests
{
    [Fact]
    public void ExampleLang_DeclarationOnly_ReturnsVariableName()
    {
        var result = new ExNs.ExampleLang().Parse("Int A;");

        Assert.Equal("A", result.ToString());
    }

    [Fact]
    public void ExampleLang_DeclarationAndAssignment_ReturnsValue()
    {
        var result = new ExNs.ExampleLang().Parse("Int A; A = 50;");

        var intValue = Assert.IsType<ExNs.IntValue>(result);
        Assert.Equal(50, intValue.Value);
    }

    [Fact]
    public void ExampleLang_SyntaxError_Throws()
    {
        // "Int ;" is missing the variable name between the type and the semicolon.
        var ex = Assert.Throws<ParseException>(() => new ExNs.ExampleLang().Parse("Int ;"));
        Assert.NotEmpty(ex.Expected);
    }

    [Fact]
    public void ExampleLang_AssignmentToUndeclaredVariable_Throws()
    {
        // A is declared, but B is assigned without a declaration.
        Assert.Throws<Exception>(() => new ExNs.ExampleLang().Parse("Int A; B = 5;"));
    }

    [Fact]
    public void DebugLang_SingleIdentifier_Parses()
    {
        var result = new DbgNs.DebugLang().Parse("A");

        Assert.Equal("A", result.ToString());
    }

    [Fact]
    public void DebugLang_UnrecognizedCharacter_Throws()
    {
        // The lexer rejects characters that match no token rule.
        var ex = Assert.Throws<LexerException>(() => new DbgNs.DebugLang().Parse("A % B"));
        Assert.Equal('%', ex.Character);
    }

    [Fact]
    public void TryParse_ReturnsTrueWithResult_OnValidInput()
    {
        var ok = new ExNs.ExampleLang().TryParse("Int A;", out var result);

        Assert.True(ok);
        Assert.Equal("A", result.ToString());
    }

    [Fact]
    public void TryParse_ReturnsFalse_OnSyntaxError()
    {
        var ok = new ExNs.ExampleLang().TryParse("Int ;", out var result);

        Assert.False(ok);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_ReturnsFalse_OnLexerError()
    {
        var ok = new DbgNs.DebugLang().TryParse("A % B", out _);

        Assert.False(ok);
    }

    [Fact]
    public void SeparateInstances_ParseIndependently()
    {
        var first = new ExNs.ExampleLang().Parse("Int A; A = 1;");
        var second = new ExNs.ExampleLang().Parse("Int A; A = 2;");

        Assert.Equal(1, Assert.IsType<ExNs.IntValue>(first).Value);
        Assert.Equal(2, Assert.IsType<ExNs.IntValue>(second).Value);
    }

    [Fact]
    public void SameInstance_MultipleParses_AreIndependent()
    {
        var lang = new ExNs.ExampleLang();

        // Re-declaring the same variable across calls would previously throw
        // (accumulated TypeTable) and/or corrupt the reused syntax tree.
        var first = lang.Parse("Int A; A = 1;");
        var second = lang.Parse("Int A; A = 2;");

        Assert.Equal(1, Assert.IsType<ExNs.IntValue>(first).Value);
        Assert.Equal(2, Assert.IsType<ExNs.IntValue>(second).Value);
    }

    // Regression for the FIRST-set under-approximation (H2): for S -> OptA OptB c
    // with OptA and OptB both nullable, the input "c" requires the OptA -> epsilon
    // reduce to fire on lookahead c. That lookahead only exists if FIRST chains
    // through the nullable OptB to reach the terminal c. Before the fix this input
    // was wrongly rejected.
    [Theory]
    [InlineData("c")]
    [InlineData("a c")]
    [InlineData("b c")]
    [InlineData("a b c")]
    public void NullablePrefix_ParsesThroughNullableSymbols(string input)
    {
        var result = new NullablePrefixLang().Parse(input);

        Assert.Equal("parsed", result.ToString());
    }

    // The semantic action of an epsilon production must be evaluated even though
    // its tree node has no children. Empty input reduces S -> epsilon.
    [Fact]
    public void EpsilonProduction_SemanticActionIsEvaluated()
    {
        var result = new EpsilonValueLang().Parse(string.Empty);

        Assert.Equal("empty", result.ToString());
    }

    [Fact]
    public void EpsilonValueLang_NonEmptyInput_PassesThrough()
    {
        var result = new EpsilonValueLang().Parse("a");

        Assert.Equal("a", result.ToString());
    }
}
