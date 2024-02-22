namespace LRParser.CFG;


public enum SymbolType {
    Terminal,
    NonTerminal,
}

public enum SpecialType {
    epsilon,
    dollar
}

public class Symbol {
    public SymbolType Type { get; }
    internal readonly Enum Description;
    public object Attribut1;
    
    public Symbol(Enum description, SymbolType type) {
        Description = description;
        Type = type;
    }
    
    public Symbol(Enum description, string lexValue, SymbolType type) {
        Description = description;
        Type = type;
        Attribut1 = lexValue;
    }

    public static Symbol Epsilon => new (SpecialType.epsilon, SymbolType.Terminal);
    public static Symbol Dollar => new (SpecialType.dollar, SymbolType.Terminal);
    public bool IsEpsilon => Type == SymbolType.Terminal && Description.Equals(SpecialType.epsilon);
    public bool IsDollar => Type == SymbolType.Terminal && Description.Equals(SpecialType.dollar);
    
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
        return $"{Description}";
    }
}
