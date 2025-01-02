using System;
using System.Linq;

namespace LRParser.CFG {
    public class Production {
        internal readonly Symbol[] Conclusion;
        internal readonly Symbol Premise;

        public Action<Symbol, Symbol[]> SemanticAction;

        public Production(Symbol premise, params Symbol[] conclusion) {
            Premise = premise;
            Conclusion = conclusion;
        }

        public void SetSemanticAction(Action<Symbol, Symbol[]> semanticAction) {
            SemanticAction = semanticAction;
        }

        public override int GetHashCode() {
            var hash = Premise.GetHashCode();
            foreach (var conc in Conclusion) {
                hash = HashCode.Combine(hash, conc.GetHashCode());
            }

            return hash;
        }

        public override bool Equals(object? obj) {
            if (obj is not Production other) {
                return false;
            }

            if (!Premise.Equals(other.Premise) || Conclusion.Length != other.Conclusion.Length) {
                return false;
            }

            for (var i = 0; i < Conclusion.Length; i++) {
                if (!Conclusion[i].Equals(other.Conclusion[i])) {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() {
            return $"{Premise} -> {Conclusion.Aggregate("(", (c, n) => $"{c} {n},")})";
        }
    }
}