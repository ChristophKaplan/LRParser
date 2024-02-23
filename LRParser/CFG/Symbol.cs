namespace LRParser.CFG;

public enum SymbolType {
    Terminal,
    NonTerminal
}

public enum SpecialTerminal {
    epsilon,
    dollar
}

public class Symbol {
    private readonly Enum Description;
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

    public SymbolType Type {
        get;
    }

    public static Symbol Epsilon => new(SpecialTerminal.epsilon, SymbolType.Terminal);
    public static Symbol Dollar => new(SpecialTerminal.dollar, SymbolType.Terminal);
    public bool IsEpsilon => Type == SymbolType.Terminal && Description.Equals(SpecialTerminal.epsilon);
    public bool IsDollar => Type == SymbolType.Terminal && Description.Equals(SpecialTerminal.dollar);

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