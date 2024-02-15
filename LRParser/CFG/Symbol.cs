namespace LRParser.CFG;

public abstract class Symbol {
    protected internal readonly string Description;
    
    public object _attribut1;
    public object _attribut2;
    
    protected Symbol(string description) {
        Description = description;
    }

    public bool IsEpsilon => Description.Equals("epsilon");
    
    public override int GetHashCode() {
        return Description.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Symbol other) {
            return Description.Equals(other.Description);
        }

        return false;
    }

    public override string ToString() {
        return $"{Description}, a1= {_attribut1}";
    }
}

public class Terminal : Symbol {

    public Terminal(string description, string lexVal) : base(description) {
        _attribut1 = lexVal;
    }

    public Terminal(string description) : base(description) {
    }
    
    public Terminal() : base("epsilon") {
    }
}

public class NonTerminal : Symbol {
    public NonTerminal(string description) : base(description) {
    }
}