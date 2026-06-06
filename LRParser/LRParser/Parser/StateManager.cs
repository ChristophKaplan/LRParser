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
            var firstState = new State(Closure(new List<LRItem> { startItem }), 0);
            States = new List<State> { firstState };
            var count = 0;
            GenerateStates(firstState, ref count);
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
                    States.Add(nextState);
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
                        MergeStates(a, b);
                        // MergeStates removes b from the list, shifting the
                        // remaining states down. Step back so the state that
                        // slid into slot j is examined and not skipped.
                        j--;
                    }
                }
            }
        }

        private void MergeStates(State myState, State mergeMe)
        {
            myState.MergeLookaheads(mergeMe);

            //reroute
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
                //Logging.Log($"reroute {state.Id} to {state1.Id}");
            }

            //remove state2
            States.Remove(mergeMe);
        }

        private bool TryGetEqualState(State state, out State foundState)
        {
            foreach (var curState in States.Where(curState => curState.HasEqualItems(state)))
            {
                foundState = curState;
                return true;
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

                var allAfterDotSymbol = currentItem.GetSymbolsAfterDotSymbol(); // das ist beta
                var oldLookahead = currentItem.LookAheadSymbols; // das ist s

                for (var i = 0; i < oldLookahead.Count; i++)
                {
                    var symbol = oldLookahead[i]; //jedes a in s
                    var input = new List<Symbol>(allAfterDotSymbol).Append(symbol).ToArray();
                    
                    var curLookahead = _cfg.First(input, new List<Symbol>()); //k = 1, only LL(1) or LR(1)   FIRST(beta a)
                    
                    // im not sure if eps is supposed to be in the lookaheads and the table 
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