using LRParser.CFG;

namespace LRParser.Parser;

public class State {
    private List<LRItem> items;
    public Dictionary<Symbol, State> transitions;
    private int id;

    public State(List<LRItem> items, int id) {
        this.items = items;
        transitions = new Dictionary<Symbol, State>();
        this.id = id;
    }

    public List<LRItem> Items => items;
    public int Id => id;

    public override string ToString() {
        return items.Aggregate($"State({Id}):\n", (c, n) => $"{c} {n},\n") +
               transitions.Aggregate("\ttransitions:\n", (c, n) => $"{c} {n.Key} -> {n.Value.Id},\n");
    }

    public List<LRItem> GetIncompleteItems() {
        return items.Where(i => !i.IsComplete()).ToList();
    }

    public bool EqualItems(State other) {
        if (items.Count != other.items.Count) {
            return false;
        }

        foreach (var item in items) {
            if (!other.items.Contains(item)) {
                return false;
            }
        }

        return true;
    }
}