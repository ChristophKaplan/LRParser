using LRParser.CFG;

namespace LRParser.Parser;

public class LRItem{
    public readonly int DotPosition;

    public LRItem(ProductionRule production, int dotPosition, List<Symbol<Enum>> lookAheadSymbols) {
        Production = production;
        DotPosition = dotPosition;
        LookAheadSymbols = lookAheadSymbols;

        if (CurrentSymbol != null && CurrentSymbol.IsEpsilon) {
            DotPosition++;
        }
    }

    public ProductionRule Production {
        get;
    }

    public List<Symbol<Enum>> LookAheadSymbols {
        get;
    }

    public bool IsComplete => DotPosition == Production.Conclusion.Length;
    public Symbol<Enum> CurrentSymbol => IsComplete ? null : Production.Conclusion[DotPosition];
    public LRItem NextItem => IsComplete ? null : new LRItem(Production, DotPosition + 1, LookAheadSymbols);

    public List<Symbol<Enum>> GetSymbolsAfterDot() {
        List<Symbol<Enum>> symbols = new();
        for (var i = DotPosition + 1; i < Production.Conclusion.Length; i++) {
            symbols.Add(Production.Conclusion[i]);
        }

        return symbols;
    }

    public bool CoreEquals(LRItem other) {
        return Production.Equals(other.Production) && DotPosition == other.DotPosition;
    }
    
    private bool LookAheadEquals(List<Symbol<Enum>> other) {
        return LookAheadSymbols.Count == other.Count && LookAheadSymbols.All(symbol => other.Contains(symbol));
    }
    
    public override bool Equals(object? obj) {
        if (obj == null || obj.GetType() != GetType()) {
            return false;
        }

        var other = (LRItem)obj;
        return Production.Equals(other.Production) && DotPosition == other.DotPosition && LookAheadEquals(other.LookAheadSymbols);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Production, DotPosition, LookAheadSymbols);
    }

    public override string ToString() {
        var s = $"{Production.Premise} ->";
        for (var i = 0; i < Production.Conclusion.Length; i++) {
            if (i == DotPosition) {
                s += " .";
            }

            s += $" {Production.Conclusion[i]}";
        }

        if (DotPosition == Production.Conclusion.Length) {
            s += " .";
        }

        s += $", {string.Join(" ", LookAheadSymbols)}";
        return $"[{s}]";
    }
}