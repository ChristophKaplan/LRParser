using LRParser.CFG;

namespace LRParser.Parser;

public enum ParserAction {
    Shift,
    Reduce,
    Accept,
    Error
}

public class Parser<T, N> where T : Enum where N : Enum {
    private readonly ContextFreeGrammar<T, N> _cfg;
    private readonly Table<T, N> _table;
    private string _parsingOutput = string.Empty;
    private readonly bool _showOutput;

    public Parser(ContextFreeGrammar<T, N> cfg, bool showOutput = false) {
        _cfg = cfg;
        _showOutput = showOutput;
        var startItem = new LRItem(cfg.Productions[0], 0, new List<Symbol> { Symbol.Dollar });
        var states = new States<T, N>(startItem, _cfg, showOutput);
        _table = new Table<T, N>(states, _cfg, showOutput);
        
        //DEBUG
        //Console.WriteLine(_table);
    }

    public ConcreteSyntaxTree Parse(List<Symbol> input) {
        input.Add(Symbol.Dollar);

        var stateStack = new Stack<int>();
        stateStack.Push(0);

        var treeStack = new Stack<ConcreteSyntaxTree>();

        while (true) {
            //DEBUG
            //Console.WriteLine(treeStack.Aggregate("\nDEBUG: ", (current, tree) => tree.Symbol + " " +current ));

            var action = GetAction(stateStack, input, out var pullEps);

            if (action.Item1 == ParserAction.Error)
            {
                Error(input, stateStack);
                break;
            }

            if (action.Item1 == ParserAction.Accept) {
                Accept();
                break;
            }
            else if (action.Item1 == ParserAction.Shift) {
                Shift(input, stateStack, treeStack, action.Item2, pullEps);
            }
            else if (action.Item1 == ParserAction.Reduce) {
                Reduce(stateStack, treeStack, action.Item2);
            }
            else {
                Error(input, stateStack);
                break;
            }
        }

        return treeStack.Pop();
    }

    private (ParserAction, int) GetAction(Stack<int> stateStack, List<Symbol> input, out bool pullEps)
    {
        pullEps = false;
        if (_table.ActionTable.TryGetValue((stateStack.Peek(), input[0]), out var action))
        {
            return action;
        }
        else if (_table.ActionTable.TryGetValue((stateStack.Peek(), Symbol.Epsilon), out var epsAction)) {
            pullEps = true;
            return epsAction;
        }
        return (ParserAction.Error, -1);
    }
    
    private void Accept() {
        _parsingOutput += "ACCEPT\n";
        if (_showOutput) Console.WriteLine(_parsingOutput);
    }

    private void Error(List<Symbol> input, Stack<int> stateStack) {
        var expected = _table.ExpectedSymbols(stateStack.Peek());
        _parsingOutput += $"ERROR: cant parse \"{input[0]}\". {stateStack.Peek()}\n Expected Symbols: {expected.Aggregate("", (current, symbol) => current + symbol + " ")}\n";
        
        //DEBUG
        foreach (var ex in expected) {
            _table.ActionTable.TryGetValue((stateStack.Peek(), ex), out var action);
            Console.WriteLine($"Expected: {ex}, Action: {action.Item1} {_cfg.Productions[action.Item2]} ");
        }
        throw new Exception($"Error:\n{_parsingOutput}");
    }

    private void Shift(List<Symbol> input, Stack<int> stateStack, Stack<ConcreteSyntaxTree> treeStack, int shiftState, bool pullEps) {
        if (pullEps)
        {
            //then what?
            //do nothing?
            return;
        }

        _parsingOutput += $"SHIFT: {input[0]}, next state:{shiftState}\n";
        stateStack.Push(shiftState);
        treeStack.Push(new ConcreteSyntaxTree(new Symbol(input[0])));
        input.RemoveAt(0);
    }

    private void Reduce(Stack<int> stateStack, Stack<ConcreteSyntaxTree> treeStack, int ruleId) {
        var rule = _cfg.Productions[ruleId];
        _parsingOutput += $"REDUCE ({ruleId}), Rule: {rule}\n";

        var reduced = new ConcreteSyntaxTree(new Symbol(rule.Premise), rule.SemanticAction); //new parent

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