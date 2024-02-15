namespace LRParser.CFG;

public abstract class Symbol {
    protected internal readonly string Value;

    protected Symbol(string value) {
        Value = value;
    }

    public bool IsEpsilon => Value.Equals("epsilon");

    public override int GetHashCode() {
        return Value.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Symbol other) {
            return Value.Equals(other.Value);
        }

        return false;
    }

    public override string ToString() {
        return Value;
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