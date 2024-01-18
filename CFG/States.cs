namespace CNF;

public class State {
    private List<LRItem> items;
    private Dictionary<Symbol,State> transitions;
    private int id;

    public State(List<LRItem> items, int id) {
        this.items = items;
        this.transitions = new Dictionary<Symbol, State>();
        this.id = id;
    }

    public List<LRItem> Items => items;
    public int Id => id;

    
    public void PossibleTransitions(ContextFreeGrammar cnf) {
        var groups = items.GroupBy(i => i.GetSymbol());
        
        int count = 0;
        foreach (var group in groups) {
            if (group.Key == null) continue;
            if (group.Key.Equals(new Terminal())) continue;
            
            var nextItems = new List<LRItem>();
            foreach (var item in group) {
                if(!item.IsComplete()) nextItems.Add(item.NextItem());
            }
            
            Console.WriteLine(group.Key +  nextItems.Aggregate( " -> ", (c, n) => $"{c} {n},"));

            var nextClosure = cnf.Closure(nextItems);
            var next = new State(nextClosure, ++count);
            
            Console.WriteLine(next);
            
            transitions.Add(group.Key, next);
        }
    }
    

    public override string ToString() {
        return items.Aggregate($"State({Id}):\n", (c, n) => $"{c} {n},\n");
    }
}