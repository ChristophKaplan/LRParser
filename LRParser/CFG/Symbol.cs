namespace LRParser.CFG;


public enum SymbolType {
    Terminal,
    NonTerminal,
}

public enum SpecialType {
    epsilon,
    dollar
}

public class Symbol<T> where T : Enum {
    public SymbolType Type { get; }
    internal readonly T Description;
    public object Attribut1;
    
    public Symbol(T description, SymbolType type) {
        Description = description;
        Type = type;
    }
    
    public Symbol(T description, string lexValue, SymbolType type) {
        Description = description;
        Type = type;
        Attribut1 = lexValue;
    }

    public static Symbol<Enum> Epsilon => new (SpecialType.epsilon, SymbolType.Terminal);
    public static Symbol<Enum> Dollar => new (SpecialType.dollar, SymbolType.Terminal);
    public bool IsEpsilon => Type == SymbolType.Terminal && Description.Equals(SpecialType.epsilon);
    public bool IsDollar => Type == SymbolType.Terminal && Description.Equals(SpecialType.dollar);
    
    public override int GetHashCode() {
        return Description.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Symbol<T> other) {
            return Description.Equals(other.Description);
        }

        return false;
    }

    public override string ToString() {
        return $"{Description}";
    }
}
