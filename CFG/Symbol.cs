using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

namespace CNF;

public abstract class Symbol {
    protected internal readonly string _value;
    public bool IsEpsilon => _value.Equals("epsilon");
    public Symbol(string value) {
        _value = value;
    }

    public override string ToString() {
        return _value;
    }

    public override int GetHashCode() {
        return _value.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Symbol other) return _value.Equals(other._value);
        return false;
    }
}

public class Terminal : Symbol {
    public Terminal(string value) : base(value) {
    }

    public Terminal() : base("epsilon") {
    }
    
}

public class NonTerminal : Symbol {
    public NonTerminal(string value) : base(value) {
    }
}

public class Word {
    private Terminal[] tokens;
    public Word(params Terminal[] terminals) {
        tokens = terminals;
    }

    public int Length => tokens.Length;

    public Word Slice(int n) {
        var t = new Terminal[n];
        for (int i = 0; i < n; i++) {
            t[i] = tokens[i];
        }

        return new Word(t);
    }
}