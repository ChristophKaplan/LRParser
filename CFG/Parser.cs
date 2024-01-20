namespace CNF;

public class Parser {
    Table table;
    Stack<(int, Symbol)> stack;
    List<Terminal> input;
    ContextFreeGrammar cfg;
    
    public Parser(ContextFreeGrammar cfg) {
        this.cfg = cfg;
        var states = cfg.GenerateStates(new LRItem(cfg.ProductionRules[0], 0, new List<Symbol>() { new Terminal("$") }));

        Console.WriteLine("ALL STATES:");
        foreach (var state in states) {
            Console.WriteLine(state);    
        }
        
        this.table = new Table(cfg, states);
        this.stack = new Stack<(int, Symbol)>();
        this.input = new List<Terminal>();
        
        Console.WriteLine(table);
    }
    public void Parse(List<Terminal> input) {
        this.input = input;
        
        this.input.Add(new Terminal("$"));
        
        Stack<int> stackState = new Stack<int>();
        
        stackState.Push(0);
        
        while (true) {
            
            if (table.GetActionTable().TryGetValue((stackState.Peek(),this.input[0]), out var action)) {
            
                if (action.Item1 == Action.Accept) {
                    Console.WriteLine("ACCEPT");
                    break;
                } else if (action.Item1 == Action.Shift) {
                    Console.WriteLine("SHIFT:"+this.input[0]);
                    stackState.Push(action.Item2);
                    this.input.RemoveAt(0);
                } else if (action.Item1 == Action.Reduce) {

                    var rule = cfg.ProductionRules[action.Item2];
                    Console.WriteLine("REDUCE nr:"+action.Item2+" = "+rule);

                    for (int i = 0; i < rule.to.Count(s => !s.IsEpsilon); i++) {
                        stackState.Pop();
                    }
                    
                    if(table.GetGotoTable().TryGetValue((stackState.Peek(), rule.from), out var gotoId)) {
                        stackState.Push(gotoId);
                    } else {
                        Console.WriteLine("Goto not found:"+(stackState.Peek(), rule.from));
                        break;
                    }
                }
            } else {
                Console.WriteLine("ERROR");
                break;
            }
        }
    }
}