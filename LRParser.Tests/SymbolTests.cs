using LRParser.CFG;

namespace LRParser.Tests;

public class SymbolTests
{
    private enum Sym { A, B }

    [Fact]
    public void Equals_SameEnumAndType_AreEqualWithSameHash()
    {
        var a = new Symbol(Sym.A, SymbolType.Terminal);
        var b = new Symbol(Sym.A, SymbolType.Terminal);

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentEnumValue_AreNotEqual()
    {
        var a = new Symbol(Sym.A, SymbolType.Terminal);
        var b = new Symbol(Sym.B, SymbolType.Terminal);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_SameEnumDifferentType_AreNotEqual()
    {
        var terminal = new Symbol(Sym.A, SymbolType.Terminal);
        var nonTerminal = new Symbol(Sym.A, SymbolType.NonTerminal);

        Assert.False(terminal.Equals(nonTerminal));
    }

    [Fact]
    public void Type_ReflectsConstructorArgument()
    {
        Assert.Equal(SymbolType.Terminal, new Symbol(Sym.A, SymbolType.Terminal).Type);
        Assert.Equal(SymbolType.NonTerminal, new Symbol(Sym.A, SymbolType.NonTerminal).Type);
    }

    [Fact]
    public void InternalSymbols_HaveExpectedFlags()
    {
        Assert.True(Symbol.Epsilon.IsEpsilon);
        Assert.False(Symbol.Epsilon.IsDollar);

        Assert.True(Symbol.Dollar.IsDollar);
        Assert.False(Symbol.Dollar.IsEpsilon);

        Assert.False(Symbol.Start.IsEpsilon);
        Assert.False(Symbol.Start.IsDollar);
    }

    [Fact]
    public void InternalSymbols_AreDistinct()
    {
        Assert.False(Symbol.Epsilon.Equals(Symbol.Dollar));
        Assert.False(Symbol.Epsilon.Equals(Symbol.Start));
        Assert.False(Symbol.Dollar.Equals(Symbol.Start));
    }

    [Fact]
    public void SetValue_StoresLexValueAttribute()
    {
        var symbol = new Symbol(Sym.A, SymbolType.Terminal);
        symbol.SetValue("hello");

        Assert.Equal("hello", symbol.Attribute.ToString());
    }
}
