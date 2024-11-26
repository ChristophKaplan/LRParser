using LRParser.Language;

namespace LRParser.CFG;

public enum SymbolType {
    Terminal,
    NonTerminal
}

public enum SpecialTerminal {
    Epsilon,
    Dollar,
}

public enum SpecialNonTerminal {
    Start
}

public class Symbol {
    private readonly Enum _enum;
    public ILanguageObject SyntheticAttribute;
    public object InheritedAttribute;
    
    public Symbol(Enum @enum, SymbolType type) {
        _enum = @enum;
        Type = type;
    }

    public Symbol(Enum @enum, string lexValue, SymbolType type) {
        _enum = @enum;
        Type = type;
        SyntheticAttribute = new LexValue(lexValue);
    }
    
    public Symbol(Symbol symbol) {
        _enum = symbol._enum;
        Type = symbol.Type;
        SyntheticAttribute = symbol.SyntheticAttribute;
        InheritedAttribute = symbol.InheritedAttribute;
    }

    public SymbolType Type { get; }
    
    public static Symbol Epsilon => new(SpecialTerminal.Epsilon, SymbolType.Terminal);
    public static Symbol Dollar => new(SpecialTerminal.Dollar, SymbolType.Terminal);
    public bool IsEpsilon => Type == SymbolType.Terminal && _enum.Equals(SpecialTerminal.Epsilon);
    public bool IsDollar => Type == SymbolType.Terminal && _enum.Equals(SpecialTerminal.Dollar);
    public bool IsStartSymbol => Type == SymbolType.NonTerminal && _enum.Equals(SpecialNonTerminal.Start);
    
    public override int GetHashCode() {
        return _enum.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Symbol other) {
            return _enum.Equals(other._enum);
        }

        return false;
    }

    public override string ToString() {
        return $"{_enum}";
    }
}