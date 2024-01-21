namespace CFG;

public class LRItem {
    public ProductionRule rule;
    public int dotPosition;
    private List<Symbol> lookahead;
    
    public LRItem(ProductionRule rule, int dotPosition, List<Symbol> lookahead) {
        this.rule = rule;
        this.dotPosition = dotPosition;
        this.lookahead = lookahead;

        if (GetSymbol() != null && GetSymbol().IsEpsilon) {
            this.dotPosition++;
            //this.rule = new ProductionRule(rule.from, new Symbol[] { });
        }
    }

    public ProductionRule Rule => rule;
    public List<Symbol> Lookahead => lookahead;

    public bool IsComplete() {
        return dotPosition == rule.to.Length;
    }

    public Symbol GetSymbol() {
        if (IsComplete()) return null;
        return rule.to[dotPosition];
    }
    
    public List<Symbol> GetSymbolsAfterDot() {
        List<Symbol> symbols = new();
        for (int i = dotPosition + 1; i < rule.to.Length; i++) {
            symbols.Add(rule.to[i]);
        }

        return symbols;
    }
    
    public LRItem NextItem() {
        if (IsComplete()) return null;
        return new LRItem(rule, dotPosition + 1, lookahead);
    }

    public override string ToString() {
        string s = $"{rule.from} ->";
        for (int i = 0; i < rule.to.Length; i++) {
            if (i == dotPosition) s += " .";
            s += $" {rule.to[i]}";
        }
        if (dotPosition == rule.to.Length) s += " .";
        s += $", {string.Join(" ", lookahead)}";
        return $"[{s}]";
    }

    public override bool Equals(object? obj) {
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;
        var other = (LRItem) obj;
        return rule.Equals(other.rule) && dotPosition == other.dotPosition && LookAheadEquals(other.lookahead);
    }

    public override int GetHashCode() {
        return HashCode.Combine(rule, dotPosition, lookahead);
    }
    
    private bool LookAheadEquals(List<Symbol> other) {
        if (lookahead.Count != other.Count) return false;
        foreach (var symbol in lookahead) {
            if (!other.Contains(symbol)) {
                return false;
            }
        }

        return true;
    }
}