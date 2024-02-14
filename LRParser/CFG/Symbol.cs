namespace LRParser.CFG;

public abstract class Symbol {
    protected internal readonly string Value;
    public bool IsEpsilon => Value.Equals("epsilon");

    protected Symbol(string value) {
        Value = value;
    }

    public override int GetHashCode() => Value.GetHashCode();
    
    public override bool Equals(object? obj) {
        if (obj is Symbol other) {
            return Value.Equals(other.Value);
        }

        return false;
    }
    
    public override string ToString() => Value;
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
