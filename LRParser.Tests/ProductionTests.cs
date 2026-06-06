using LRParser.CFG;
using LRParser.Language;

namespace LRParser.Tests;

public class ProductionTests
{
    private enum Sym { S, A, B }

    private static readonly Production.SemanticActionDelegate Noop = _ => null!;

    private static Symbol N(Sym s) => new(s, SymbolType.NonTerminal);
    private static Symbol T(Sym s) => new(s, SymbolType.Terminal);

    [Fact]
    public void Equals_SamePremiseAndConclusion_AreEqual()
    {
        var p1 = new Production(Noop, N(Sym.S), N(Sym.A), T(Sym.B));
        var p2 = new Production(Noop, N(Sym.S), N(Sym.A), T(Sym.B));

        Assert.True(p1.Equals(p2));
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentConclusion_AreNotEqual()
    {
        var p1 = new Production(Noop, N(Sym.S), N(Sym.A));
        var p2 = new Production(Noop, N(Sym.S), N(Sym.B));

        Assert.False(p1.Equals(p2));
    }

    [Fact]
    public void Equals_DifferentConclusionLength_AreNotEqual()
    {
        var p1 = new Production(Noop, N(Sym.S), N(Sym.A));
        var p2 = new Production(Noop, N(Sym.S), N(Sym.A), T(Sym.B));

        Assert.False(p1.Equals(p2));
    }

    [Fact]
    public void Equals_IgnoresSemanticAction()
    {
        Production.SemanticActionDelegate other = rhs => rhs[0].Attribute;
        var p1 = new Production(Noop, N(Sym.S), N(Sym.A));
        var p2 = new Production(other, N(Sym.S), N(Sym.A));

        Assert.True(p1.Equals(p2));
    }
}
