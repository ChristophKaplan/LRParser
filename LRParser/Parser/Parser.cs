using LRParser.CFG;

namespace LRParser.Parser;

public enum Action {
    Shift,
    Reduce,
    Accept
}

public class Parser {
    private readonly Table table;
    private readonly ContextFreeGrammar cfg;

    public Parser(ContextFreeGrammar cfg) {
        this.cfg = cfg;

        var states = GenerateStates(new LRItem(cfg.ProductionRules[0], 0, new List<Symbol>() { new Terminal("$") }));
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

            if (cur.IsComplete()) {
                continue;
            }

            var rest = cur.GetSymbolsAfterDot();
            var oldLookahead = cur.Lookahead;
            rest.AddRange(oldLookahead);

            var curLookahead = cfg.FIRST(rest[0]); //k = 1

            if (cur.GetSymbol() is NonTerminal nonTerminal) {
                var prods = cfg.GetAllProdForNonTerminal(nonTerminal);

                foreach (var prod in prods) {
                    var deeperItem = new LRItem(prod, 0, curLookahead);
                    var containedAlready = result.Where(r => r.Rule.Equals(deeperItem.rule) && r.dotPosition.Equals(deeperItem.dotPosition)).ToList();

                    if (containedAlready.Count == 0) {
                        stack.Push(deeperItem);
                    }
                    else {
                        if (containedAlready.Count > 1) {
                            throw new Exception("should not exist more than one time" + containedAlready.Aggregate("", (c, n) => $"{c} {n}, "));
                        }

                        foreach (var s in curLookahead) {
                            if (!containedAlready[0].Lookahead.Contains(s)) {
                                containedAlready[0].Lookahead.Add(s);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private List<State> GenerateStates(LRItem startItem) {
        var firstState = new State(Closure(new List<LRItem>() { startItem }), 0);
        var states = new List<State>() { firstState };
        var count = 0;
        GenerateStates(firstState, states, ref count);
        return states;
    }

    private void GenerateStates(State current, List<State> states, ref int count) {
        var possibleTransitionGroups = current.GetIncompleteItems().GroupBy(i => i.GetSymbol());

        foreach (var possibleTransitions in possibleTransitionGroups) {
            if (possibleTransitions.Key.Equals(new Terminal())) {
                continue;
            }

            var nextItems = new List<LRItem>();
            foreach (var item in possibleTransitions) {
                if (!item.IsComplete()) {
                    nextItems.Add(item.NextItem());
                }
            }

            var nextClosure = Closure(nextItems);
            count++;
            var next = new State(nextClosure, count);

            if (TryGetSameState(states, next, out var sameState)) {
                current.transitions.Add(possibleTransitions.Key, sameState);
                //Console.WriteLine(current.Id+ " add " +group.Key+ " transition to" + sameState );
            }
            else {
                current.transitions.Add(possibleTransitions.Key, next);
                states.Add(next);
                GenerateStates(next, states, ref count);
            }
        }
    }

    private static bool TryGetSameState(List<State> states, State state, out State sameState) {
        foreach (var s in states) {
            if (s.EqualItems(state)) {
                sameState = s;
                return true;
            }
        }

        sameState = null;
        return false;
    }

    private Table GenerateTable(List<State> states) {
        var table = new Table(cfg, states);

        foreach (var state in states) {
            foreach (var item in state.Items) {
                if (item.IsComplete()) {
                    if (item.rule.from.Equals(cfg.StartSymbol)) {
                        table.GetActionTable()[(state.Id, new Terminal("$"))] = (Action.Accept, -1);
                    }
                    else {
                        foreach (var la in item.Lookahead) {
                            table.GetActionTable()[(state.Id, la)] = (Action.Reduce, cfg.ProductionRules.IndexOf(item.rule));
                        }
                    }
                }
                else {
                    var symbol = item.GetSymbol();
                    switch (symbol) {
                        case Terminal: {
                            if (!state.transitions.TryGetValue(symbol, out var nextState)) {
                                Console.WriteLine($"Conflict at state {state} with symbol {symbol}.");
                            }

                            table.GetActionTable()[(state.Id, symbol)] = (Action.Shift, nextState.Id);
                            break;
                        }
                        case NonTerminal: {
                            var nextState = state.transitions[symbol];
                            table.GetGotoTable()[(state.Id, symbol)] = nextState.Id;
                            break;
                        }
                    }
                }
            }
        }

        return table;
    }

    public void Parse(List<Terminal> input) {
        input.Add(new Terminal("$"));

        var stackState = new Stack<int>();
        stackState.Push(0);

        while (true) {
            if (table.GetActionTable().TryGetValue((stackState.Peek(), input[0]), out var action)) {
                if (action.Item1 == Action.Accept) {
                    Console.WriteLine("ACCEPT");
                    break;
                }
                else if (action.Item1 == Action.Shift) {
                    Console.WriteLine("SHIFT:" + input[0]);
                    stackState.Push(action.Item2);
                    input.RemoveAt(0);
                }
                else if (action.Item1 == Action.Reduce) {
                    var rule = cfg.ProductionRules[action.Item2];
                    Console.WriteLine("REDUCE nr:" + action.Item2 + " = " + rule);

                    for (var i = 0; i < rule.to.Count(s => !s.IsEpsilon); i++) {
                        stackState.Pop();
                    }

                    if (table.GetGotoTable().TryGetValue((stackState.Peek(), rule.from), out var gotoId)) {
                        stackState.Push(gotoId);
                    }
                    else {
                        Console.WriteLine("Goto not found:" + (stackState.Peek(), rule.from));
                        break;
                    }
                }
            }
            else {
                Console.WriteLine("ERROR");
                break;
            }
        }
    }
}