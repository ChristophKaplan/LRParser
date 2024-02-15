namespace LRParser.CFG;

public class ProductionRule {
    internal readonly Symbol[] Conclusion;
    internal readonly NonTerminal Premise;
    
    public Func<object[], object> SemanticAction;
    
    public ProductionRule(NonTerminal premise, params Symbol[] conclusion) {
        Premise = premise;
        Conclusion = conclusion;
    }

    public void SetSemanticAction(Func<object[], object> semanticAction){ 
        SemanticAction = semanticAction;
    }
    
    public bool CheckLeftRecursion() {
        for (var i = 0; i < Conclusion.Length; i++) {
            if (Conclusion[i] is NonTerminal nonTerminal) {
                return Premise.Equals(nonTerminal);
            }
        }

        return false;
    }

    public int GetMostRightPosOf(NonTerminal nonTerminal) {
        for (var i = Conclusion.Length - 1; i >= 0; i--) {
            if (Conclusion[i].Equals(nonTerminal)) {
                return i;
            }
        }

        return -1;
    }

    public bool ContainsNonTerminalConclusion() {
        return Conclusion.OfType<NonTerminal>().Any();
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