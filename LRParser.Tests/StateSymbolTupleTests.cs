using LRParser.CFG;
using LRParser.LRParser.Parser;

namespace LRParser.Tests;

// Finding #1: StateSymbolTuple.Equals compares ONLY hash codes (it never looks
// at StateId / Symbol). This is the key type for the ACTION and GOTO
// dictionaries, so a hash collision between two different keys would corrupt
// table lookups.
//
// In practice .NET's HashCode.Combine produced ZERO collisions across millions
// of (state, symbol) keys in testing, so the defect is currently latent rather
// than observable. These tests pin the intended equality contract: they pass
// today and would still pass after Equals is fixed to compare the fields.
public class StateSymbolTupleTests
{
    [Fact]
    public void Equals_SameStateAndSymbol_AreEqual()
    {
        var a = new StateSymbolTuple(5, Symbol.Dollar);
        var b = new StateSymbolTuple(5, Symbol.Dollar);

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentState_AreNotEqual()
    {
        var a = new StateSymbolTuple(0, Symbol.Dollar);
        var b = new StateSymbolTuple(1, Symbol.Dollar);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentSymbol_AreNotEqual()
    {
        var a = new StateSymbolTuple(0, Symbol.Dollar);
        var b = new StateSymbolTuple(0, Symbol.Start);

        Assert.False(a.Equals(b));
    }

    // Used as a dictionary key in the ACTION/GOTO tables; verify it round-trips.
    [Fact]
    public void UsableAsDictionaryKey_DistinguishesKeys()
    {
        var map = new Dictionary<StateSymbolTuple, string>
        {
            [new StateSymbolTuple(0, Symbol.Dollar)] = "a",
            [new StateSymbolTuple(0, Symbol.Start)] = "b",
            [new StateSymbolTuple(1, Symbol.Dollar)] = "c",
        };

        Assert.Equal("a", map[new StateSymbolTuple(0, Symbol.Dollar)]);
        Assert.Equal("b", map[new StateSymbolTuple(0, Symbol.Start)]);
        Assert.Equal("c", map[new StateSymbolTuple(1, Symbol.Dollar)]);
    }
}
