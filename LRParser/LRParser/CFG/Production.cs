using System;
using System.Linq;

namespace LRParser.CFG
{
    public class Production
    {
        internal readonly Symbol[] Conclusion;
        internal readonly Symbol Premise;
        public delegate void SemanticActionDelegate(ref Symbol symbol, Symbol[] parameters);
        public SemanticActionDelegate SemanticAction;

        public Production(Symbol premise, params Symbol[] conclusion)
        {
            Premise = premise;
            Conclusion = conclusion;
        }

        public void SetSemanticAction(SemanticActionDelegate action)
        {
            SemanticAction = action;
        }

        public override int GetHashCode()
        {
            var hash = Premise.GetHashCode();
            foreach (var conc in Conclusion)
            {
                hash = HashCode.Combine(hash, conc.GetHashCode());
            }

            return hash;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Production other)
            {
                return false;
            }

            if (!Premise.Equals(other.Premise) || Conclusion.Length != other.Conclusion.Length)
            {
                return false;
            }

            for (var i = 0; i < Conclusion.Length; i++)
            {
                if (!Conclusion[i].Equals(other.Conclusion[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Premise} -> {Conclusion.Aggregate("(", (c, n) => $"{c} {n},")})";
        }
    }
}
