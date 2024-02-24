using LRParser.CFG;

namespace LRParser.Parser;

public class States <T, N> where T : Enum where N : Enum{
    private readonly ContextFreeGrammar<T, N> _cfg;
    public List<State> StateList { get; }
    
    public States(LRItem startItem, ContextFreeGrammar<T, N> cfg, bool lalr = true) {
        _cfg = cfg;
        StateList = GenerateStates(startItem);

        if (!lalr) {
            Console.WriteLine($"Cannonical LR, States: {StateList.Count}");
            return;
        }
        
        var stateCountBefore = StateList.Count;
        MergeStates(StateList);
        Console.WriteLine($"LALR, States reduced from: {stateCountBefore} to: {StateList.Count}");
    }
    
    private List<State> GenerateStates(LRItem startItem) {
        var firstState = new State(Closure(new List<LRItem> { startItem }), 0);
        var states = new List<State> { firstState };
        var count = 0;
        GenerateStates(firstState, states, ref count);
        if (!ValidateStates(states)) {
            //throw new Exception("Conflicts, Grammar is not LR(1) parsable.");
        }

        return states;
    }

    private void GenerateStates(State currentState, List<State> states, ref int stateId) {
        var possibleTransitionGroups = currentState.GetIncompleteItems().GroupBy(i => i.CurrentSymbol);

        foreach (var possibleTransitions in possibleTransitionGroups) {
            if (possibleTransitions.Key.Equals(Symbol.Epsilon)) {
                continue;
            }

            var nextItems = (from item in possibleTransitions where !item.IsComplete select item.NextItem).ToList();
            var nextClosureSet = Closure(nextItems);

            var nextState = new State(nextClosureSet, ++stateId);

            if (!TryGetState(states, nextState, out var sameState)) {
                currentState.Transitions.Add(possibleTransitions.Key, nextState);
                states.Add(nextState);
                GenerateStates(nextState, states, ref stateId);
            }
            else {
                currentState.Transitions.Add(possibleTransitions.Key, sameState);
            }
        }
    }

    private bool ValidateStates(List<State> states) {
        var conflict = false;
        foreach (var state in states) {
            conflict = state.HasConflict();
        }

        return conflict;
    }

    private void MergeStates(List<State> states) {
        for (var i = 0; i < states.Count; i++) {
            for (var j = i + 1; j < states.Count; j++) {
                if (states[i].HasEqualCore(states[j])) {
                    //Console.WriteLine($"should merge {i} and {j}");
                    MergeStates(states[i], states[j], states);
                }
            }
        }
    }
    
    private void MergeStates(State state1,State mergeMe,List<State> states) {
        
        //merge lookaheads
        foreach (var item1 in state1.Items) {
            foreach (var mergeItem in mergeMe.Items) {
                if (item1.CoreEquals(mergeItem)) {
                    foreach (var s in mergeItem.LookAheadSymbols) {
                        if (!item1.LookAheadSymbols.Contains(s)) {
                            item1.LookAheadSymbols.Add(s);
                        }
                    }
                }
            }   
        }

        //reroute
        foreach (var state in states) {
            foreach (var (symbol, toState) in state.Transitions) {
                if(toState.Id == mergeMe.Id) {
                    state.Transitions[symbol] = state1;
                    //Console.WriteLine($"reroute {state.Id} to {state1.Id}");
                }
            }
        }
        
        //remove state2
        states.Remove(mergeMe);
    }
    
    private bool TryGetState(List<State> states, State state, out State foundState) {
        foreach (var curState in states.Where(curState => curState.HasEqualItems(state))) {
            foundState = curState;
            return true;
        }

        foundState = null;
        return false;
    }
    
    private List<LRItem> Closure(List<LRItem> lrItems) {
        var result = new List<LRItem>();
        var stack = new Stack<LRItem>(lrItems);

        while (stack.Count > 0) {
            
            var currentItem = stack.Pop();
            if (!FormUnionIfClosed(result, currentItem)) {
                result.Add(currentItem);
            }

            if (currentItem.IsComplete || currentItem.CurrentSymbol.Type == SymbolType.Terminal) {
                continue;
            }

            var afterDot = currentItem.GetSymbolsAfterDot();
            var oldLookahead = currentItem.LookAheadSymbols;
            
            foreach (var symbol in oldLookahead) {
                var curLookahead = _cfg.First(afterDot.Count == 0 ? symbol : afterDot[0]); //k = 1, only LL(1) or LR(1)
                var prods = _cfg.GetAllProdForNonTerminal(currentItem.CurrentSymbol);
                foreach (var prod in prods) {
                    var deeperItem = new LRItem(prod, 0, curLookahead);
                    if (!FormUnionIfClosed(stack, deeperItem)) {
                        stack.Push(deeperItem);
                    }
                }
            }
        }

        return result;
    }
    
    private static bool FormUnionIfClosed(IEnumerable<LRItem> closedSet, LRItem newItem) {
        var closedAlready = false;
        foreach (var closedItem in closedSet) {
            if (!closedItem.CoreEquals(newItem)) {
                continue;
            }

            closedAlready = true;
            foreach (var s in newItem.LookAheadSymbols) {
                if (closedItem.LookAheadSymbols.Contains(s)) {
                    continue;
                }

                closedItem.LookAheadSymbols.Add(s);
            }
        }

        return closedAlready;
    }
    
    public override string ToString() {
        var a = "ALL STATES:\n";
        foreach (var state in StateList) {
            a+=$"{state}\n";
        }

        return a;
    }
}