namespace LRParser.CFG;

public class ProductionRule{
    internal readonly Symbol Premise;
    internal readonly Symbol[] Conclusion;
    public Func<object[], object> SemanticAction;
    
    public ProductionRule(Symbol premise, params Symbol[] conclusion) {
        Premise = premise;
        Conclusion = conclusion;
    }
    
    public void SetSemanticAction(Func<object[], object> semanticAction){ 
        SemanticAction = semanticAction;
    }
    
    public bool CheckLeftRecursion() {
        foreach (var sym in Conclusion) {
            if (sym.Type.Equals(SymbolType.NonTerminal)) {
                return Premise.Equals(sym);
            }
        }
        return false;
    }

    public int GetMostRightPosOf(Symbol symbol) {
        for (var i = Conclusion.Length - 1; i >= 0; i--) {
            if (Conclusion[i].Equals(symbol)) {
                return i;
            }
        }

        return -1;
    }

    public bool ContainsNonTerminalConclusion() {
        return Conclusion.Any(symbol => symbol.Type.Equals(SymbolType.NonTerminal));
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is ProductionRule other) {
            return ToString().Equals(other.ToString());
        }

        return false;
    }

    public override string ToString() {
        return $"{Premise} -> {Conclusion.Aggregate("(", (c, n) => $"{c} {n},")})";
    }
}