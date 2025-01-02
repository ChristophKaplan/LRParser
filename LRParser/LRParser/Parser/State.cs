using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser {
    public class State {
        public State(List<LRItem> items, int id) {
            Id = id;
            Items = items;
            Transitions = new Dictionary<Symbol, State>();
        }

        public int Id { get; }

        public List<LRItem> Items { get; }

        public Dictionary<Symbol, State> Transitions { get; }

        public List<LRItem> GetIncompleteItems() {
            return Items.Where(i => !i.IsComplete).ToList();
        }

        public bool HasEqualCore(State other) {
            return Items.Count == other.Items.Count && Items.All(item => other.Items.Any(item.CoreEquals));
        }

        public bool HasEqualItems(State other) {
            return Items.Count == other.Items.Count && Items.All(item => other.Items.Contains(item));
        }

        public bool HasConflict(ref string output) {
            return HasReduceReduceConflict(ref output) || HasShiftReduceConflict(ref output);
        }

        private bool HasReduceReduceConflict(ref string output) {
            foreach (var item in Items) {
                if (!item.IsComplete) {
                    continue;
                }

                foreach (var laSym in item.LookAheadSymbols) {
                    if (laSym.IsDollar) {
                        continue;
                    }

                    foreach (var item2 in Items) {
                        if (item2.IsComplete && !item.Equals(item2) && item2.LookAheadSymbols.Contains(laSym)) {
                            output += $"Reduce-Reduce conflict: {item} and {item2} contains reduce symbol:{laSym} on State:{Id}\n";
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool HasShiftReduceConflict(ref string output) {
            foreach (var shiftSymbol in Transitions.Keys) {
                foreach (var item in Items) {
                    if (item.IsComplete && item.LookAheadSymbols.Contains(shiftSymbol)) {
                        output += $"Shift-Reduce conflict: {item} is complete and contains ShiftSymbol:{shiftSymbol} on State:{Id}\n";
                        return true;
                    }
                }
            }

            return false;
        }

        public override string ToString() {
            return Items.Aggregate($"State({Id}):\n", (c, n) => $"{c} {n},\n") +
                   Transitions.Aggregate("\ttransitions:\n", (c, n) => $"{c} {n.Key} -> {n.Value.Id},\n");
        }
    }
}