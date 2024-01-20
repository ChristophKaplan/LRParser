namespace CNF;

public enum Action {
    Shift,
    Reduce,
    Accept,
    Error,
}

public class Table {
    private List<State> states;
    private ContextFreeGrammar cfg;
    private Dictionary<(int, Symbol), (Action, int)> actionTable;
    private Dictionary<(int, Symbol), int> gotoTable;

    public Table(ContextFreeGrammar cfg, List<State> states) {
        this.cfg = cfg;
        this.states = states;
        this.actionTable = new Dictionary<(int, Symbol), (Action, int)>();
        this.gotoTable = new Dictionary<(int, Symbol), int>();
        GenerateTable();
    }

    public Dictionary<(int, Symbol), (Action, int)> GetActionTable() {
        return actionTable;
    }

    public Dictionary<(int, Symbol), int> GetGotoTable() {
        return gotoTable;
    }

    private void GenerateTable() {
        foreach (var state in states) {
            foreach (var item in state.Items) {
                if (item.IsComplete()) {
                    if (item.rule.from.Equals(cfg.StartSymbol)) {
                        actionTable[(state.Id, new Terminal("$"))] = (Action.Accept, -1);
                    } else {
                        foreach (var la in item.Lookahead) {
                            actionTable[(state.Id, la)] = (Action.Reduce, cfg.ProductionRules.IndexOf(item.rule));
                        }
                    }
                } else {
                    var symbol = item.GetSymbol();
                    if (symbol is Terminal) {
                        if(!state.transitions.TryGetValue(symbol, out var nextState)) {
                            Console.WriteLine($"Conflict at state {state} with symbol {symbol}.");
                        }
                        
                        actionTable[(state.Id, symbol)] = (Action.Shift, nextState.Id);
                    } else if (symbol is NonTerminal) {
                        var nextState = state.transitions[symbol];
                        gotoTable[(state.Id, symbol)] = nextState.Id;
                    }
                }
            }
        }
    }

    public override string ToString() {
        string s = "ACTION\n";
        foreach (var entry in actionTable) {
            s += $"({entry.Key.Item1}, {entry.Key.Item2}) -> {entry.Value}\n";
        }
        s += "\nGOTO\n";
        foreach (var entry in gotoTable) {
            s += $"({entry.Key}) -> {entry.Value}\n";
        }
        return s;
    }
}