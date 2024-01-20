namespace CNF;

public class Parser {
    Table table;

    ContextFreeGrammar cfg;
    
    public Parser(ContextFreeGrammar cfg) {
        this.cfg = cfg;
        
        var states = cfg.GenerateStates(new LRItem(cfg.ProductionRules[0], 0, new List<Symbol>() { new Terminal("$") }));

        Console.WriteLine("ALL STATES:");
        foreach (var state in states) {
            Console.WriteLine(state);    
        }
        
        table = new Table(cfg, states);
        
        Console.WriteLine(table);
    }
    public void Parse(List<Terminal> input) {
        
        input.Add(new Terminal("$"));
        
        Stack<int> stackState = new Stack<int>();
        
        stackState.Push(0);
        
        while (true) {
            
            if (table.GetActionTable().TryGetValue((stackState.Peek(),input[0]), out var action)) {
            
                if (action.Item1 == Action.Accept) {
                    Console.WriteLine("ACCEPT");
                    break;
                } else if (action.Item1 == Action.Shift) {
                    Console.WriteLine("SHIFT:"+input[0]);
                    stackState.Push(action.Item2);
                    input.RemoveAt(0);
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