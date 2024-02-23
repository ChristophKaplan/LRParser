using LRParser.CFG;

namespace LRParser.Parser;

public class Table {
    public Dictionary<(int, Symbol), (ParserAction, int)> ActionTable {
        get;
    } = new();

    public Dictionary<(int, Symbol), int> GotoTable {
        get;
    } = new();


    public override string ToString() {
        var s = "ACTION\n";
        foreach (var entry in ActionTable) {
            s += $"({entry.Key.Item1}, {entry.Key.Item2}) -> {entry.Value}\n";
        }

        s += "\nGOTO\n";
        foreach (var entry in GotoTable) {
            s += $"({entry.Key}) -> {entry.Value}\n";
        }

        return s;
    }
}