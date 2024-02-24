using LRParser.CFG;

namespace LRParser.Parser;

public enum ParserAction {
    Shift,
    Reduce,
    Accept
}

public class Parser<T, N> where T : Enum where N : Enum {
    private readonly ContextFreeGrammar<T, N> _cfg;
    private readonly Table<T, N> _table;
    
    public Parser(ContextFreeGrammar<T, N> cfg) {
        _cfg = cfg;

        var startItem = new LRItem(cfg.Productions[0], 0, new List<Symbol> { Symbol.Dollar });
        var states = new States<T, N>(startItem,_cfg);
        Console.WriteLine(states);
        _table = new Table<T, N>(states,_cfg);
    }

    public ConcreteSyntaxTree Parse(List<Symbol> input) {
        input.Add(Symbol.Dollar);

        var stateStack = new Stack<int>();
        stateStack.Push(0);

        var treeStack = new Stack<ConcreteSyntaxTree>();

        while (true) {
            if (!_table.ActionTable.TryGetValue((stateStack.Peek(), input[0]), out var action)) {
                Error(input, stateStack);
            }

            if (action.Item1 == ParserAction.Accept) {
                Accept(); 
                break;
            }
            else if (action.Item1 == ParserAction.Shift) {
                Shift(input, stateStack, treeStack, action.Item2);
            }
            else if (action.Item1 == ParserAction.Reduce) {
                Reduce(stateStack, treeStack, action.Item2);
            }
            else {
                Error(input, stateStack);
            }
        }

        return treeStack.Pop();
    }

    private void Accept() {
        Console.WriteLine("ACCEPT");
    }

    private void Error(List<Symbol> input, Stack<int> stateStack) {
        throw new Exception($"ERROR: cant parse \"{input[0]}\". {stateStack.Peek()}");
    }

    private void Shift(List<Symbol> input, Stack<int> stateStack, Stack<ConcreteSyntaxTree> treeStack, int shiftState) {
        Console.WriteLine($"SHIFT: {input[0]}, next state:{shiftState}");
        stateStack.Push(shiftState);
        treeStack.Push(new ConcreteSyntaxTree(input[0]));
        input.RemoveAt(0);
    }

    private void Reduce(Stack<int> stateStack, Stack<ConcreteSyntaxTree> treeStack, int ruleId) {
        var rule = _cfg.Productions[ruleId];
        Console.WriteLine($"REDUCE ({ruleId}) Rule: {rule}");

        var reduced = new ConcreteSyntaxTree(rule);

        for (var i = 0; i < rule.Conclusion.Count(s => !s.IsEpsilon); i++) {
            stateStack.Pop();
            reduced.AddChild(treeStack.Pop());
        }

        treeStack.Push(reduced);

        if (_table.GotoTable.TryGetValue((stateStack.Peek(), rule.Premise), out var gotoId)) {
            stateStack.Push(gotoId);
        }
        else {
            throw new Exception("Goto not found:" + (stateStack.Peek(), rule.Premise));
        }
    }
}