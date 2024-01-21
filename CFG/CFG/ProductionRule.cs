namespace LRParser.CFG;

public class ProductionRule {
    internal NonTerminal from;
    internal Symbol[] to;

    public ProductionRule(NonTerminal from, params Symbol[] to) {
        this.from = from;
        this.to = to;
    }

    public bool CheckLeftRecursion() {
        return from.Equals(to[0]);
    }

    public override string ToString() {
        return $"{from} -> {to.Aggregate("(", (c, n) => $"{c} {n},")})";
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

    public int GetMostRightPosOf(NonTerminal S) {
        for (var i = to.Length - 1; i >= 0; i--) {
            if (to[i].Equals(S)) {
                return i;
            }
        }

        return -1;
    }

    public bool ContainsNonTerminalConclusion() {
        return to.OfType<NonTerminal>().Any();
    }
}