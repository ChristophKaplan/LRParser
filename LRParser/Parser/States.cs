using LRParser.CFG;

namespace LRParser.Parser;

public class State {
    public State(List<LRItem> items, int id) {
        Id = id;
        Items = items;
        Transitions = new Dictionary<Symbol, State>();
    }

    public int Id {
        get;
    }

    public List<LRItem> Items {
        get;
    }

    public Dictionary<Symbol, State> Transitions {
        get;
    }

    public List<LRItem> GetIncompleteItems() {
        return Items.Where(i => !i.IsComplete).ToList();
    }

    public bool HasEqualItems(State other) {
        return Items.Count == other.Items.Count && Items.All(item => other.Items.Contains(item));
    }

    public override string ToString() {
        return Items.Aggregate($"State({Id}):\n", (c, n) => $"{c} {n},\n") +
               Transitions.Aggregate("\ttransitions:\n", (c, n) => $"{c} {n.Key} -> {n.Value.Id},\n");
    }
}