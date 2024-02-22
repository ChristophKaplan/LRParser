using System.Data;
using LRParser.CFG;

namespace LRParser.Parser;

public enum ParserAction
{
    Shift,
    Reduce,
    Accept
}

public class Parser
{
    private readonly ContextFreeGrammar _cfg;
    private readonly Table _table;

    public Parser(ContextFreeGrammar cfg)
    {
        this._cfg = cfg;

        var states = GenerateStates(new LRItem(cfg.ProductionRules[0], 0, new List<Symbol> { new Terminal("$") }));
        //MergeStates(states);
        _table = GenerateTable(states);

        Console.WriteLine("ALL STATES:");
        foreach (var state in states)
        {
            Console.WriteLine(state);
        }

        Console.WriteLine(_table);
    }

    private List<LRItem> Closure(List<LRItem> lrItems)
    {
        var result = new List<LRItem>();
        var stack = new Stack<LRItem>(lrItems);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (!MergeLookAheadIfClosed(result, cur))
            {
                result.Add(cur);
            }

            if (cur.IsComplete)
            {
                continue;
            }

            var afterDot = cur.GetSymbolsAfterDot();
            var oldLookahead = cur.LookAheadSymbols;

            foreach (var symbol in oldLookahead)
            {
                var curLookahead = _cfg.First(afterDot.Count == 0 ? symbol : afterDot[0]); //k = 1, only LL(1) or LR(1)

                if (cur.CurrentSymbol is NonTerminal nonTerminal)
                {
                    var prods = _cfg.GetAllProdForNonTerminal(nonTerminal);

                    foreach (var prod in prods)
                    {
                        var deeperItem = new LRItem(prod, 0, curLookahead);
                        if (!MergeLookAheadIfClosed(stack, deeperItem))
                        {
                            stack.Push(deeperItem);
                        }
                    }
                }
            }
        }

        return result;
    }

    private bool MergeLookAheadIfClosed(IEnumerable<LRItem> closedSet, LRItem newItem)
    {
        var closedAlready = false;
        foreach (var closedItem in closedSet)
        {
            if (!closedItem.CoreEquals(newItem))
            {
                continue;
            }

            closedAlready = true;
            foreach (var s in newItem.LookAheadSymbols)
            {
                if (!closedItem.LookAheadSymbols.Contains(s))
                {
                    closedItem.LookAheadSymbols.Add(s);
                }
            }
        }

        return closedAlready;
    }

    private List<State> GenerateStates(LRItem startItem)
    {
        var firstState = new State(Closure(new List<LRItem> { startItem }), 0);
        var states = new List<State> { firstState };
        var count = 0;
        GenerateStates(firstState, states, ref count);
        if (!ValidateStates(states))
        {
            //throw new Exception("Conflicts, Grammar is not LR(1) parsable.");
        }

        return states;
    }

    private void GenerateStates(State current, List<State> states, ref int count)
    {
        var possibleTransitionGroups = current.GetIncompleteItems().GroupBy(i => i.CurrentSymbol);

        foreach (var possibleTransitions in possibleTransitionGroups)
        {
            if (possibleTransitions.Key.Equals(new Terminal()))
            {
                continue;
            }

            var nextItems = new List<LRItem>();
            foreach (var item in possibleTransitions)
            {
                if (!item.IsComplete)
                {
                    nextItems.Add(item.NextItem);
                }
            }

            var nextClosure = Closure(nextItems);
            count++;
            var next = new State(nextClosure, count);

            if (TryGetSameState(states, next, out var sameState))
            {
                current.Transitions.Add(possibleTransitions.Key, sameState);
            }
            else
            {
                current.Transitions.Add(possibleTransitions.Key, next);
                states.Add(next);
                GenerateStates(next, states, ref count);
            }
        }
    }

    private bool ValidateStates(List<State> states)
    {
        var conflict = false;
        foreach (var state in states)
        {
            conflict = state.HasConflict();
        }

        return conflict;
    }

    private void MergeStates(List<State> states)
    {
        for (var i = 0; i < states.Count; i++)
        {
            for (var j = i + 1; j < states.Count; j++)
            {
                if (states[i].HasEqualCore(states[j]))
                {
                    Console.WriteLine($"should merge {i} and {j}");
                }
            }
        }
    }

    private static bool TryGetSameState(List<State> states, State state, out State sameState)
    {
        foreach (var s in states)
        {
            if (s.HasEqualItems(state))
            {
                sameState = s;
                return true;
            }
        }

        sameState = null;
        return false;
    }

    private Table GenerateTable(List<State> states)
    {
        var table = new Table(_cfg);

        foreach (var state in states)
        {
            foreach (var item in state.Items)
            {
                if (item.IsComplete)
                {
                    if (item.Production.Premise.Equals(_cfg.StartSymbol))
                    {
                        table.ActionTable[(state.Id, new Terminal("$"))] = (ParserAction.Accept, -1);
                    }
                    else
                    {
                        foreach (var symbol in item.LookAheadSymbols)
                        {
                            if (table.ActionTable.ContainsKey((state.Id, symbol)) && table.ActionTable[(state.Id, symbol)].Item1 == ParserAction.Shift)
                            {
                                Console.WriteLine("shift reduce conflict, default to shift");
                                continue;
                            }

                            if (table.ActionTable.ContainsKey((state.Id, symbol)) && table.ActionTable[(state.Id, symbol)].Item1 == ParserAction.Reduce)
                            {
                                throw new Exception("reduce/reduce conflict");
                            }
                            
                            table.ActionTable[(state.Id, symbol)] = (ParserAction.Reduce, _cfg.ProductionRules.IndexOf(item.Production));
                        }
                    }
                }
                else
                {
                    var symbol = item.CurrentSymbol;
                    switch (symbol)
                    {
                        case Terminal:
                        {
                            if (table.ActionTable.ContainsKey((state.Id, symbol)) && table.ActionTable[(state.Id, symbol)].Item1 == ParserAction.Reduce)
                            {
                                Console.WriteLine("shift reduce conflict, default to shift");
                            }

                            if (!state.Transitions.TryGetValue(symbol, out var nextState))
                            {
                                throw new Exception($"cant find shift symbol {symbol} at state {state}!");
                            }
                            
                            table.ActionTable[(state.Id, symbol)] = (ParserAction.Shift, nextState.Id);
                            break;
                        }
                        case NonTerminal:
                        {
                            var nextState = state.Transitions[symbol];
                            table.GotoTable[(state.Id, symbol)] = nextState.Id;
                            break;
                        }
                    }
                }
            }
        }

        return table;
    }

    public ConcreteSyntaxTreeNode Parse(List<Terminal> input)
    {
        input.Add(new Terminal("$"));

        var stateStack = new Stack<int>();
        stateStack.Push(0);

        var treeStack = new Stack<ConcreteSyntaxTreeNode>();

        while (true)
        {
            if (!_table.ActionTable.TryGetValue((stateStack.Peek(), input[0]), out var action))
            {
                return treeStack.Pop();
            }

            if (action.Item1 == ParserAction.Accept)
            {
                Accept();
                break;
            }
            else if (action.Item1 == ParserAction.Shift)
            {
                Shift(input, stateStack, treeStack, action.Item2);
            }
            else if (action.Item1 == ParserAction.Reduce)
            {
                Reduce(stateStack, treeStack, action.Item2);
            }
            else
            {
                Error(input, stateStack);
            }
        }

        return treeStack.Pop();
    }
    
    private void Accept()
    {
        Console.WriteLine("ACCEPT");
    }
    
    private void Error(List<Terminal> input, Stack<int> stateStack)
    {
        throw new Exception($"ERROR: cant parse \"{input[0]}\". {stateStack.Peek()}");
    }
    
    private void Shift(List<Terminal> input, Stack<int> stateStack, Stack<ConcreteSyntaxTreeNode> treeStack, int shiftState)
    {
        Console.WriteLine($"SHIFT: {input[0]}, next state:{shiftState}");
        stateStack.Push(shiftState);
        treeStack.Push(new ConcreteSyntaxTreeNode(input[0]));
        input.RemoveAt(0);
    }

    private void Reduce(Stack<int> stateStack, Stack<ConcreteSyntaxTreeNode> treeStack, int ruleId)
    {
        var rule = _cfg.ProductionRules[ruleId];
        Console.WriteLine($"REDUCE ({ruleId}) Rule: {rule}");

        var reduced = new ConcreteSyntaxTreeNode(rule);

        for (var i = 0; i < rule.Conclusion.Count(s => !s.IsEpsilon); i++)
        {
            stateStack.Pop();
            reduced.AddChild(treeStack.Pop());
        }

        treeStack.Push(reduced);

        if (_table.GotoTable.TryGetValue((stateStack.Peek(), rule.Premise), out var gotoId))
        {
            stateStack.Push(gotoId);
        }
        else
        {
            throw new Exception("Goto not found:" + (stateStack.Peek(), rule.Premise));
        }
    }
}