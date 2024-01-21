using LRParser.CFG;

namespace LRParser.Parser;

public class Table {
    private ContextFreeGrammar cfg;
    private Dictionary<(int, Symbol), (Action, int)> actionTable;
    private Dictionary<(int, Symbol), int> gotoTable;

    public Table(ContextFreeGrammar cfg, List<State> states) {
        this.cfg = cfg;
        actionTable = new Dictionary<(int, Symbol), (Action, int)>();
        gotoTable = new Dictionary<(int, Symbol), int>();
    }

    public Dictionary<(int, Symbol), (Action, int)> GetActionTable() {
        return actionTable;
    }

    public Dictionary<(int, Symbol), int> GetGotoTable() {
        return gotoTable;
    }

    public override string ToString() {
        var s = "ACTION\n";
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