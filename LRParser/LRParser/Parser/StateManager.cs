using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogHelper;
using LRParser.CFG;
using LRParser.LRParser.Parser;

namespace LRParser.Parser
{
    public class StateManager<T, N> where T : struct, Enum where N : struct, Enum
    {
        private readonly ContextFreeGrammar<T, N> _cfg;
        public List<State> States { get; private set; }

        // Index of states by core hash, used only during generation to make
        // state de-duplication near O(1) instead of a linear scan of all states.
        private readonly Dictionary<int, List<State>> _statesByCoreHash = new();

        private string _statesOutput = string.Empty;

        public StateManager(LRItem startItem, ContextFreeGrammar<T, N> cfg, bool showOutput = false, bool debug = false, bool lalr = true)
        {
            _cfg = cfg;
            GenerateStates(startItem);

            if (!lalr)
            {
                _statesOutput += $"Cannonical LR, States: {States.Count}\n";
            }
            else
            {
                var stateCountBefore = States.Count;
                MergeStates();
                _statesOutput += $"LALR, States reduced from: {stateCountBefore} to: {States.Count}\n";
            }

            var hasConflict = ValidateStates();
            if (hasConflict)
            {
                _statesOutput += "Grammar contains one or more conflicts (see above).\n";
            }

            if (showOutput)
            {
                Logger.Log(_statesOutput);
            }

            if (debug)
            {
                Logger.Log(ToString());
            }
        }

        private void GenerateStates(LRItem startItem)
        {
            States = new List<State>();
            _statesByCoreHash.Clear();
            var firstState = new State(Closure(new List<LRItem> { startItem }), 0);
            AddState(firstState);
            var count = 0;
            GenerateStates(firstState, ref count);
        }

        private void AddState(State state)
        {
            States.Add(state);
            if (!_statesByCoreHash.TryGetValue(state.CoreHash, out var bucket))
            {
                bucket = new List<State>();
                _statesByCoreHash[state.CoreHash] = bucket;
            }

            bucket.Add(state);
        }

        private void GenerateStates(State currentState, ref int stateId)
        {
            var possibleTransitionGroups = currentState.GetIncompleteItems().GroupBy(i => i.CurrentSymbol);

            foreach (var possibleTransitions in possibleTransitionGroups)
            {
                if (possibleTransitions.Key.Equals(Symbol.Epsilon))
                {
                    continue;
                }

                var nextItems = (from item in possibleTransitions where !item.IsComplete select item.NextItem).ToList();
                var nextClosureSet = Closure(nextItems);

                var nextState = new State(nextClosureSet, ++stateId);

                if (!TryGetEqualState(nextState, out var equalState))
                {
                    currentState.Transitions.Add(possibleTransitions.Key, nextState);
                    AddState(nextState);
                    GenerateStates(nextState, ref stateId);
                }
                else
                {
                    currentState.Transitions.Add(possibleTransitions.Key, equalState);
                }
            }
        }

        private bool ValidateStates()
        {
            var conflict = false;
            foreach (var state in States)
            {
                conflict |= state.HasConflict(ref _statesOutput);
            }

            return conflict;
        }

        private void MergeStates()
        {
            for (var i = 0; i < States.Count; i++)
            {
                for (var j = i + 1; j < States.Count; j++)
                {
                    var a = States[i];
                    var b = States[j];

                    if (a.HasEqualCore(b))
                    {
                        MergeStatePair(a, b);
                        // MergeStatePair removes b from the list, shifting the
                        // remaining states down. Step back so the state that
                        // slid into slot j is examined and not skipped.
                        j--;
                    }
                }
            }
        }

        private void MergeStatePair(State myState, State mergeMe)
        {
            myState.MergeLookaheads(mergeMe);

            // Reroute every transition that pointed at the merged-away state so it
            // now targets the surviving state.
            var transitionsToModify = new List<(State state, Symbol symbol)>();

            foreach (var state in States)
            {
                foreach (var (symbol, toState) in state.Transitions)
                {
                    if (toState.Id == mergeMe.Id)
                    {
                        transitionsToModify.Add((state, symbol));
                    }
                }
            }

            foreach (var (state, symbol) in transitionsToModify)
            {
                state.Transitions[symbol] = myState;
            }

            States.Remove(mergeMe);
        }

        private bool TryGetEqualState(State state, out State foundState)
        {
            if (_statesByCoreHash.TryGetValue(state.CoreHash, out var bucket))
            {
                foreach (var curState in bucket)
                {
                    if (curState.HasEqualItems(state))
                    {
                        foundState = curState;
                        return true;
                    }
                }
            }

            foundState = default;
            return false;
        }

        private List<LRItem> Closure(List<LRItem> lrItems)
        {
            var result = new List<LRItem>();
            var stack = new Stack<LRItem>(lrItems);

            while (stack.Count > 0)
            {
                var currentItem = stack.Pop();

                if (!TryGetEqualCore(currentItem, result, out var resultItem))
                {
                    result.Add(currentItem);
                }
                else
                {
                    resultItem.AddLookahead(currentItem);
                }

                if (currentItem.IsComplete || 
                    currentItem.CurrentSymbol.Type == SymbolType.Terminal || 
                    currentItem.CurrentSymbol.IsEpsilon || 
                    currentItem.CurrentSymbol.IsDollar)
                {
                    continue;
                }

                var allAfterDotSymbol = currentItem.GetSymbolsAfterDotSymbol(); // beta
                var oldLookahead = currentItem.LookAheadSymbols;                // s

                for (var i = 0; i < oldLookahead.Count; i++)
                {
                    var symbol = oldLookahead[i]; // each a in s
                    var input = new List<Symbol>(allAfterDotSymbol).Append(symbol).ToArray();
                    
                    // FIRST(beta a) — the lookahead set for items derived from this
                    // non-terminal. k = 1 (LR(1)). Epsilon never belongs in a
                    // lookahead/ACTION entry, so strip it.
                    var curLookahead = _cfg.First(input);
                    curLookahead.RemoveAll(symbol => symbol.IsEpsilon);
                    
                    var productions = _cfg.GetProductionsForNonTerminal(currentItem.CurrentSymbol);

                    foreach (var prod in productions)
                    {
                        var deeperItem = new LRItem(prod, 0, curLookahead);
                        if (!TryGetEqualCore(deeperItem, stack, out var itemOnStack))
                        {
                            //not sure about this
                            bool canAddToStack = true;
                            if (TryGetEqualCore(deeperItem, result, out var closedItem))
                            {
                                if (deeperItem.IsLookaheadContainedIn(closedItem))
                                {
                                    canAddToStack = false;
                                }
                            }
                            
                            if (!canAddToStack) continue;
                            stack.Push(deeperItem);
                        }
                        else
                        {
                            itemOnStack.AddLookahead(deeperItem);
                        }
                    }
                }
            }

            return result;
        }

        private static bool TryGetEqualCore(LRItem newItem, IEnumerable<LRItem> closedSet, out LRItem closedItem)
        {
            foreach (var item in closedSet)
            {
                if (!item.CoreEquals(newItem)) continue;
                closedItem = item;
                return true;
            }

            closedItem = default;
            return false;
        }
        

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"States: {States.Count}");
            foreach (var state in States)
            {
                sb.Append($"{state}\n");
            }

            return sb.ToString();
        }
    }
}