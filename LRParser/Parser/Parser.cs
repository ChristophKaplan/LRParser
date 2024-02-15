using LRParser.CFG;

namespace LRParser.Parser;

public enum Action {
    Shift,
    Reduce,
    Accept
}

public class Parser {
    private readonly ContextFreeGrammar cfg;
    private readonly Table table;

    public Parser(ContextFreeGrammar cfg) {
        this.cfg = cfg;

        var states = GenerateStates(new LRItem(cfg.ProductionRules[0], 0, new List<Symbol> { new Terminal("$") }));
        table = GenerateTable(states);

        Console.WriteLine("ALL STATES:");
        foreach (var state in states) {
            Console.WriteLine(state);
        }

        Console.WriteLine(table);
    }

    private List<LRItem> Closure(List<LRItem> lrItems, int k = 1) {
        var result = new List<LRItem>();
        var stack = new Stack<LRItem>(lrItems);

        while (stack.Count > 0) {
            var cur = stack.Pop();
            result.Add(cur);

            if (cur.IsComplete) {
                continue;
            }

            var rest = cur.GetSymbolsAfterDot();
            var oldLookahead = cur.LookAheadSymbols;
            rest.AddRange(oldLookahead);

            var curLookahead = cfg.First(rest[0]); //k = 1

            if (cur.CurrentSymbol is NonTerminal nonTerminal) {
                var prods = cfg.GetAllProdForNonTerminal(nonTerminal);

                foreach (var prod in prods) {
                    var deeperItem = new LRItem(prod, 0, curLookahead);
                    var containedAlready = result
                        .Where(r => r.Production.Equals(deeperItem.Production) && r.DotPosition.Equals(deeperItem.DotPosition))
                        .ToList();

                    if (containedAlready.Count == 0) {
                        stack.Push(deeperItem);
                    }
                    else {
                        if (containedAlready.Count > 1) {
                            throw new Exception("should not exist more than one time" + containedAlready.Aggregate("", (c, n) => $"{c} {n}, "));
                        }

                        foreach (var s in curLookahead) {
                            if (!containedAlready[0].LookAheadSymbols.Contains(s)) {
                                containedAlready[0].LookAheadSymbols.Add(s);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private List<State> GenerateStates(LRItem startItem) {
        var firstState = new State(Closure(new List<LRItem> { startItem }), 0);
        var states = new List<State> { firstState };
        var count = 0;
        GenerateStates(firstState, states, ref count);
        return states;
    }

    private void GenerateStates(State current, List<State> states, ref int count) {
        var possibleTransitionGroups = current.GetIncompleteItems().GroupBy(i => i.CurrentSymbol);

        foreach (var possibleTransitions in possibleTransitionGroups) {
            if (possibleTransitions.Key.Equals(new Terminal())) {
                continue;
            }

            var nextItems = new List<LRItem>();
            foreach (var item in possibleTransitions) {
                if (!item.IsComplete) {
                    nextItems.Add(item.NextItem);
                }
            }

            var nextClosure = Closure(nextItems);
            count++;
            var next = new State(nextClosure, count);

            if (TryGetSameState(states, next, out var sameState)) {
                current.Transitions.Add(possibleTransitions.Key, sameState);
                //Console.WriteLine(current.Id+ " add " +group.Key+ " transition to" + sameState );
            }
            else {
                current.Transitions.Add(possibleTransitions.Key, next);
                states.Add(next);
                GenerateStates(next, states, ref count);
            }
        }
    }

    private static bool TryGetSameState(List<State> states, State state, out State sameState) {
        foreach (var s in states) {
            if (s.HasEqualItems(state)) {
                sameState = s;
                return true;
            }
        }

        sameState = null;
        return false;
    }

    private Table GenerateTable(List<State> states) {
        var table = new Table();

        foreach (var state in states) {
            foreach (var item in state.Items) {
                if (item.IsComplete) {
                    if (item.Production.Premise.Equals(cfg.StartSymbol)) {
                        table.ActionTable[(state.Id, new Terminal("$"))] = (Action.Accept, -1);
                    }
                    else {
                        foreach (var la in item.LookAheadSymbols) {
                            table.ActionTable[(state.Id, la)] = (Action.Reduce, cfg.ProductionRules.IndexOf(item.Production));
                        }
                    }
                }
                else {
                    var symbol = item.CurrentSymbol;
                    switch (symbol) {
                        case Terminal: {
                            if (!state.Transitions.TryGetValue(symbol, out var nextState)) {
                                Console.WriteLine($"Conflict at state {state} with symbol {symbol}.");
                            }

                            table.ActionTable[(state.Id, symbol)] = (Action.Shift, nextState.Id);
                            break;
                        }
                        case NonTerminal: {
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

    public TreeNode<Symbol> Parse(List<Terminal> input) {
        input.Add(new Terminal("$"));

        var stackState = new Stack<int>();
        stackState.Push(0);

        var tree = new Stack<TreeNode<Symbol>>();

        while (true) {
            if (table.ActionTable.TryGetValue((stackState.Peek(), input[0]), out var action)) {
                if (action.Item1 == Action.Accept) {
                    Console.WriteLine("ACCEPT");
                    break;
                }

                if (action.Item1 == Action.Shift) {
                    Console.WriteLine("SHIFT:" + input[0]);
                    stackState.Push(action.Item2);
                    tree.Push(new TreeNode<Symbol>(input[0], null));
                    input.RemoveAt(0);
                }
                else if (action.Item1 == Action.Reduce) {
                    var rule = cfg.ProductionRules[action.Item2];
                    Console.WriteLine("REDUCE nr:" + action.Item2 + " = " + rule);

                    var reduced = new TreeNode<Symbol>(rule.Premise, null);
                    for (var i = 0; i < rule.Conclusion.Count(s => !s.IsEpsilon); i++) {
                        stackState.Pop();
                        reduced.AddChild(tree.Pop());
                    }

                    tree.Push(reduced);

                    if (table.GotoTable.TryGetValue((stackState.Peek(), rule.Premise), out var gotoId)) {
                        stackState.Push(gotoId);
                    }
                    else {
                        Console.WriteLine("Goto not found:" + (stackState.Peek(), rule.Premise));
                        break;
                    }
                }
            }
            else {
                Console.WriteLine("ERROR");
                break;
            }
        }

        return tree.Pop();
    }
}