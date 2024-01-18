namespace CNF;

public class LRItem {
    private ProductionRule rule;
    private int dotPosition;
    private List<Symbol> lookahead;
    
    public LRItem(ProductionRule rule, int dotPosition, List<Symbol> lookahead) {
        this.rule = rule;
        this.dotPosition = dotPosition;
        this.lookahead = lookahead;
    }

    public ProductionRule Rule => rule;
    public int DotPosition => dotPosition;
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

    public bool IsNextSymbolTerminal() {
        if (IsComplete()) return false;
        return rule.to[dotPosition] is Terminal;
    }

    public bool IsNextSymbolNonTerminal() {
        if (IsComplete()) return false;
        return rule.to[dotPosition] is NonTerminal;
    }

    public bool HasLookahead(List<Symbol> lookahead) {
        foreach (var symbol in lookahead) {
            if (!this.lookahead.Contains(symbol)) return false;
        }

        return true;
    }

    public bool HasLookahead(Symbol lookahead) {
        return this.lookahead.Contains(lookahead);
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
}