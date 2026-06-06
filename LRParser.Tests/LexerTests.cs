using LRParser.Lexer;

namespace LRParser.Tests;

// Finding #5 (fixed): the lexer used to silently drop any input that matched no
// token rule. It now skips whitespace but throws on a genuinely unrecognized
// character.
public class LexerTests
{
    private enum Tok { Id, Eq }

    private static Lexer<Tok> MakeLexer() => new(
        new TokenDefinition<Tok>(Tok.Eq, "="),
        new TokenDefinition<Tok>(Tok.Id, "[A-Z][a-z]*"));

    [Fact]
    public void Tokenize_RecognizedInput_ProducesExpectedTokens()
    {
        var tokens = MakeLexer().Tokenize("Abc = Def");

        Assert.Equal(3, tokens.Count);
        Assert.Equal("Abc", tokens[0].Attribute.ToString());
        Assert.Equal("=", tokens[1].Attribute.ToString());
        Assert.Equal("Def", tokens[2].Attribute.ToString());
    }

    [Fact]
    public void Tokenize_WhitespaceBetweenTokens_IsSkipped()
    {
        // Spaces, tabs and newlines are not tokens but must not error.
        var tokens = MakeLexer().Tokenize("Abc\t=\n Def");

        Assert.Equal(3, tokens.Count);
    }

    [Fact]
    public void Tokenize_UnrecognizedCharacter_Throws()
    {
        // '#' matches no rule and is not whitespace.
        Assert.Throws<Exception>(() => MakeLexer().Tokenize("Abc # Def"));
    }

    [Fact]
    public void Tokenize_EmptyInput_ReturnsNoTokens()
    {
        Assert.Empty(MakeLexer().Tokenize(string.Empty));
    }

    [Fact]
    public void Tokenize_OnlyWhitespace_ReturnsNoTokens()
    {
        Assert.Empty(MakeLexer().Tokenize("   \t \n  "));
    }

    [Fact]
    public void Tokenize_LeadingAndTrailingWhitespace_IsIgnored()
    {
        var tokens = MakeLexer().Tokenize("   Abc   ");

        Assert.Single(tokens);
        Assert.Equal("Abc", tokens[0].Attribute.ToString());
    }

    [Fact]
    public void Tokenize_TracksLineAndColumnPositions()
    {
        // 'Ab' starts at line 1 col 1; after a newline 'Cd' starts at line 2 col 1.
        var tokens = MakeLexer().Tokenize("Ab\nCd");

        Assert.Equal(2, tokens.Count);
        Assert.Equal((1, 1), tokens[0].Position);
        Assert.Equal((2, 1), tokens[1].Position);
    }

    [Fact]
    public void Tokenize_ColumnAdvancesPastWhitespace()
    {
        // 'A'(col1) ' '(col2) '=' (col3)
        var tokens = MakeLexer().Tokenize("A =");

        Assert.Equal(2, tokens.Count);
        Assert.Equal((1, 1), tokens[0].Position);
        Assert.Equal((1, 3), tokens[1].Position);
    }

    [Fact]
    public void Tokenize_FirstMatchingRuleWins()
    {
        // Both rules can match "If" at position 0; the first one listed wins.
        var lexer = new Lexer<Tok>(
            new TokenDefinition<Tok>(Tok.Eq, "If"),
            new TokenDefinition<Tok>(Tok.Id, "[A-Z][a-z]*"));

        var tokens = lexer.Tokenize("If");

        Assert.Single(tokens);
        Assert.Equal("If", tokens[0].Attribute.ToString());
        // Symbol.ToString() renders the token enum, so we can confirm the first
        // rule (Tok.Eq) matched rather than the identifier rule.
        Assert.Equal(Tok.Eq.ToString(), tokens[0].ToString());
    }
}
