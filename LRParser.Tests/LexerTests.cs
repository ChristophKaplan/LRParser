using LRParser.Lexer;

namespace LRParser.Tests;

// Finding #5 (fixed): the lexer used to silently drop any input that matched no
// token rule. It now skips whitespace but throws on a genuinely unrecognized
// character.
public class LexerTests
{
    private enum Tok { Id, Eq }

    private enum Tok2 { Num, Id }

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
        var ex = Assert.Throws<LexerException>(() => MakeLexer().Tokenize("Abc # Def"));
        Assert.Equal('#', ex.Character);
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

    [Fact]
    public void Tokenize_LongestMatchWins_OverEarlierShorterRule()
    {
        // "Int" is a prefix of the identifier "Integer". The keyword rule is
        // listed first, but maximal munch must prefer the longer identifier;
        // first-match-wins would emit "Int" and then choke on "eger".
        var lexer = new Lexer<Tok>(
            new TokenDefinition<Tok>(Tok.Eq, "Int"),
            new TokenDefinition<Tok>(Tok.Id, "[A-Z][a-z]*"));

        var tokens = lexer.Tokenize("Integer");

        Assert.Single(tokens);
        Assert.Equal("Integer", tokens[0].Attribute.ToString());
        Assert.Equal(Tok.Id.ToString(), tokens[0].ToString());
    }

    [Fact]
    public void Tokenize_ZeroWidthCapableRule_DoesNotMaskLongerRule()
    {
        // "\d*" matches the empty string at a letter. A zero-length match must
        // not be selected (or it would shadow the real identifier rule and the
        // lexer would spuriously report an unrecognized character).
        var lexer = new Lexer<Tok2>(
            new TokenDefinition<Tok2>(Tok2.Num, "\\d*"),
            new TokenDefinition<Tok2>(Tok2.Id, "[A-Z][a-z]*"));

        var tokens = lexer.Tokenize("Abc");

        Assert.Single(tokens);
        Assert.Equal("Abc", tokens[0].Attribute.ToString());
        Assert.Equal(Tok2.Id.ToString(), tokens[0].ToString());
    }

    [Fact]
    public void Tokenize_TokenSpanningNewline_AdvancesLineForLaterTokens()
    {
        // The Num rule matches "1\n2", which contains a newline. A following
        // token must be reported on the next line, not with an inflated column.
        var lexer = new Lexer<Tok2>(
            new TokenDefinition<Tok2>(Tok2.Num, "[0-9]\n[0-9]"),
            new TokenDefinition<Tok2>(Tok2.Id, "[A-Z][a-z]*"));

        var tokens = lexer.Tokenize("1\n2 Ab");

        Assert.Equal(2, tokens.Count);
        Assert.Equal((1, 1), tokens[0].Position);
        // "Ab" is on line 2: '2'(col1) ' '(col2) 'A'(col3).
        Assert.Equal((2, 3), tokens[1].Position);
    }
}
