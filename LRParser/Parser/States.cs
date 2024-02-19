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

    public bool HasEqualCore(State other) {
        return Items.Count == other.Items.Count && Items.All(item => other.Items.Any(otherItem => item.CoreEquals(otherItem)));
    }
    
    public bool HasEqualItems(State other) {
        return Items.Count == other.Items.Count && Items.All(item => other.Items.Contains(item));
    }

    public bool HasConflict() {
        return HasReduceReduceConflict() || HasShiftReduceConflict();
    }

    private bool HasReduceReduceConflict() {
        foreach (var item in Items) {
            if(!item.IsComplete) continue;
            
            foreach (var laSym in item.LookAheadSymbols) {
                if(laSym.Equals(new Terminal("$"))) continue;
                
                foreach (var item2 in Items) {
                    if (item2.IsComplete && !item.Equals(item2) && item2.LookAheadSymbols.Contains(laSym)) {
                        Console.WriteLine($"Reduce-Reduce conflict: {item} and {item2} on {laSym}");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool HasShiftReduceConflict() {
        foreach (var trSym in Transitions.Keys) {
            foreach (var item in Items) {
                if (item.LookAheadSymbols.Contains(trSym)) {
                    Console.WriteLine($"Shift-Reduce conflict: {item} and {Transitions[trSym]} on {trSym}");
                    return true;
                }
            }    
        }

        return false;
    }
    
    
    
    public override string ToString() {
        return Items.Aggregate($"State({Id}):\n", (c, n) => $"{c} {n},\n") +
               Transitions.Aggregate("\ttransitions:\n", (c, n) => $"{c} {n.Key} -> {n.Value.Id},\n");
    }
}