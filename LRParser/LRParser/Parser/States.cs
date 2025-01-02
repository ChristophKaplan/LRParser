using System;
using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser {
    public class States<T, N> where T : Enum where N : Enum {
        private readonly ContextFreeGrammar<T, N> _cfg;
        public List<State> StateList { get; }

        private string _statesOutput = string.Empty;

        public States(LRItem startItem, ContextFreeGrammar<T, N> cfg, bool showOutput = false, bool debug = false, bool lalr = true) {
            _cfg = cfg;
            StateList = GenerateStates(startItem);

            if (!lalr) {
                _statesOutput += $"Cannonical LR, States: {StateList.Count}\n";
                return;
            }

            var stateCountBefore = StateList.Count;
            MergeStates(StateList);
            _statesOutput += $"LALR, States reduced from: {stateCountBefore} to: {StateList.Count}\n";

            var valid = ValidateStates(StateList);

            if (showOutput) {
                Logger.Logger.Log(_statesOutput);
            }

            if (debug) {
                Logger.Logger.Log(ToString());
            }
        }

        private List<State> GenerateStates(LRItem startItem) {
            var firstState = new State(Closure(new List<LRItem> { startItem }), 0);
            var states = new List<State> { firstState };
            var count = 0;
            GenerateStates(firstState, states, ref count);
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
                conflict = state.HasConflict(ref _statesOutput);
            }

            return conflict;
        }

        private void MergeStates(List<State> states) {
            for (var i = 0; i < states.Count; i++) {
                for (var j = i + 1; j < states.Count; j++) {
                    if (states[i].HasEqualCore(states[j])) {
                        MergeStates(states[i], states[j], states);
                    }
                }
            }
        }

        private void MergeStates(State state1, State mergeMe, List<State> states) {
            //merge lookaheads
            foreach (var item1 in state1.Items) {
                foreach (var mergeItem in mergeMe.Items) {
                    if (item1.CoreEquals(mergeItem)) {
                        for (var i = mergeItem.LookAheadSymbols.Count - 1; i >= 0; i--) {
                            var sym = mergeItem.LookAheadSymbols[i];
                            if (!item1.LookAheadSymbols.Contains(sym)) {
                                item1.LookAheadSymbols.Add(sym);
                            }
                        }
                    }
                }
            }

            //reroute
            var transitionsToModify = new List<(State state, Symbol symbol)>();

            foreach (var state in states) {
                foreach (var (symbol, toState) in state.Transitions) {
                    if (toState.Id == mergeMe.Id) {
                        transitionsToModify.Add((state, symbol));
                    }
                }
            }

            foreach (var (state, symbol) in transitionsToModify) {
                state.Transitions[symbol] = state1;
                //Logging.Log($"reroute {state.Id} to {state1.Id}");
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
                if (!IfContainedAddOnlyLookahead(currentItem, result)) {
                    result.Add(currentItem);
                }

                if (currentItem.IsComplete || currentItem.CurrentSymbol.Type == SymbolType.Terminal) {
                    continue;
                }

                var allAfterDotSymbol = currentItem.GetSymbolsAfterDotSymbol();
                var oldLookahead = currentItem.LookAheadSymbols;

                foreach (var symbol in oldLookahead) {
                    //var input = afterDot.Count == 0 ? new List<Symbol>() { symbol } : new List<Symbol>() { afterDot[0], symbol };
                    var input = allAfterDotSymbol.Count == 0 ? symbol : allAfterDotSymbol[0];
                    var curLookahead = _cfg.First(input, new List<Symbol>()); //k = 1, only LL(1) or LR(1)
                    var prods = _cfg.GetAllProdForNonTerminal(currentItem.CurrentSymbol);
                    foreach (var prod in prods) {
                        var deeperItem = new LRItem(prod, 0, curLookahead);
                        if (!IfContainedAddOnlyLookahead(deeperItem, stack)) {
                            stack.Push(deeperItem);
                        }
                    }
                }
            }

            return result;
        }

        private static bool IfContainedAddOnlyLookahead(LRItem newItem, IEnumerable<LRItem> closedSet) {
            var closedAlready = false;
            foreach (var closedItem in closedSet) {
                if (!closedItem.CoreEquals(newItem)) {
                    continue;
                }

                closedAlready = true;
                foreach (var lookAheadSymbol in newItem.LookAheadSymbols) {
                    if (!closedItem.LookAheadSymbols.Contains(lookAheadSymbol)) {
                        closedItem.LookAheadSymbols.Add(lookAheadSymbol);
                    }
                }
            }

            return closedAlready;
        }

        public override string ToString() {
            var a = "ALL STATES:\n";
            foreach (var state in StateList) {
                a += $"{state}\n";
            }

            return a;
        }
    }
}