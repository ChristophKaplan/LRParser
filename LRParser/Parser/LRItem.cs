using LRParser.CFG;

namespace LRParser.Parser;

public class LRItem {
    private readonly int _dotPosition;

    public LRItem(Production production, int dotPosition, List<Symbol> lookAheadSymbols) {
        Production = production;
        _dotPosition = dotPosition;
        LookAheadSymbols = lookAheadSymbols;

        if (!IsComplete && CurrentSymbol.IsEpsilon) {
            _dotPosition++;
        }
    }

    public Production Production {
        get;
    }

    public List<Symbol> LookAheadSymbols {
        get;
    }

    public bool IsComplete => _dotPosition == Production.Conclusion.Length;
    public Symbol CurrentSymbol => Production.Conclusion[_dotPosition];
    public LRItem NextItem {
        get {
            var nPos = _dotPosition;
            if (!IsComplete) nPos++;
            return new LRItem(Production, nPos , LookAheadSymbols);
        }
    }

    public List<Symbol> GetSymbolsAfterDot() {
        List<Symbol> symbols = new();
        for (var i = _dotPosition + 1; i < Production.Conclusion.Length; i++) {
            symbols.Add(Production.Conclusion[i]);
        }

        return symbols;
    }

    public bool CoreEquals(LRItem other) {
        return Production.Equals(other.Production) && _dotPosition == other._dotPosition;
    }

    private bool LookAheadEquals(List<Symbol> other) {
        return LookAheadSymbols.Count == other.Count && LookAheadSymbols.All(symbol => other.Contains(symbol));
    }

    public override bool Equals(object? obj) {
        if (obj == null || obj.GetType() != GetType()) {
            return false;
        }

        var other = (LRItem)obj;
        return Production.Equals(other.Production) && _dotPosition == other._dotPosition && LookAheadEquals(other.LookAheadSymbols);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Production, _dotPosition, LookAheadSymbols);
    }

    public override string ToString() {
        var s = $"{Production.Premise} ->";
        for (var i = 0; i < Production.Conclusion.Length; i++) {
            if (i == _dotPosition) {
                s += " .";
            }

            s += $" {Production.Conclusion[i]}";
        }

        if (_dotPosition == Production.Conclusion.Length) {
            s += " .";
        }

        s += $" ({string.Join(" ", LookAheadSymbols)})";
        return $"[{s}]";
    }
}