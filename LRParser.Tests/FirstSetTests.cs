using LRParser.CFG;

namespace LRParser.Tests;

// Direct unit tests for FIRST-set computation, covering the two bugs the FIRST
// rewrite fixed: under-approximation (chaining must continue through nullable
// non-terminals) and over-approximation (chaining must stop at the first
// non-nullable symbol).
public class FirstSetTests
{
    private static HashSet<Symbol> First(FirstSetGrammar g, params Symbol[] sequence)
        => new HashSet<Symbol>(g.First(sequence));

    private static HashSet<Symbol> Set(params Symbol[] symbols) => new HashSet<Symbol>(symbols);

    private static Symbol a => FirstSetGrammar.Term(FsTerminal.a);
    private static Symbol b => FirstSetGrammar.Term(FsTerminal.b);
    private static Symbol c => FirstSetGrammar.Term(FsTerminal.c);
    private static Symbol A => FirstSetGrammar.NonTerm(FsNonTerminal.A);
    private static Symbol B => FirstSetGrammar.NonTerm(FsNonTerminal.B);
    private static Symbol S => FirstSetGrammar.NonTerm(FsNonTerminal.S);
    private static Symbol T => FirstSetGrammar.NonTerm(FsNonTerminal.T);

    [Fact]
    public void First_OfTerminal_IsThatTerminal()
    {
        var g = new FirstSetGrammar();
        Assert.Equal(Set(a), First(g, a));
    }

    [Fact]
    public void First_OfNullableNonTerminal_IncludesEpsilon()
    {
        // A -> a | epsilon
        var g = new FirstSetGrammar();
        Assert.Equal(Set(a, Symbol.Epsilon), First(g, A));
    }

    [Fact]
    public void First_ChainsThroughNullablePrefix_ToReachLaterSymbol()
    {
        // A and B are both nullable, so FIRST(A B c) must reach c. (H2)
        var g = new FirstSetGrammar();
        Assert.Equal(Set(a, b, c), First(g, A, B, c));
    }

    [Fact]
    public void First_OfAllNullableSequence_IncludesEpsilon()
    {
        // A and B nullable and nothing follows => the sequence is nullable.
        var g = new FirstSetGrammar();
        Assert.Equal(Set(a, b, Symbol.Epsilon), First(g, A, B));
    }

    [Fact]
    public void First_StopsAtFirstNonNullableSymbol()
    {
        // T -> c B: c is a non-nullable terminal, so FIRST(T) must be {c} only,
        // never leaking B's FIRST. (H1)
        var g = new FirstSetGrammar();
        Assert.Equal(Set(c), First(g, T));
    }

    [Fact]
    public void First_OfStartProduction_ChainsCorrectly()
    {
        // S -> A B c with A, B nullable => {a, b, c}, no epsilon (c is non-nullable).
        var g = new FirstSetGrammar();
        Assert.Equal(Set(a, b, c), First(g, S));
    }
}
