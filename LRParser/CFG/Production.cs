namespace LRParser.CFG;

public class Production {
    internal readonly Symbol[] Conclusion;
    internal readonly Symbol Premise;
    public Func<object[], object> SemanticAction;

    public Production(Symbol premise, params Symbol[] conclusion) {
        Premise = premise;
        Conclusion = conclusion;
    }

    public void SetSemanticAction(Func<object[], object> semanticAction) {
        SemanticAction = semanticAction;
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Production other) {
            return ToString().Equals(other.ToString());
        }

        return false;
    }

    public override string ToString() {
        return $"{Premise} -> {Conclusion.Aggregate("(", (c, n) => $"{c} {n},")})";
    }
}