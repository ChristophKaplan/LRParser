using System;
using System.Linq;
using LRParser.Language;

namespace LRParser.CFG
{
    public struct Production
    {
        public delegate ILanguageObject SemanticActionDelegate(Symbol[] parameters);
        public SemanticActionDelegate SemanticAction;
        
        public readonly Symbol[] Conclusion;
        public readonly Symbol Premise;

        // Number of conclusion symbols the parser actually pops on a reduce
        // (epsilon is a placeholder for the empty RHS and is never on the stack).
        // Precomputed since it is read on every reduce.
        public readonly int NonEpsilonLength;

        public Production(SemanticActionDelegate action, Symbol premise, params Symbol[] conclusion)
        {
            Premise = premise;
            Conclusion = conclusion;
            SemanticAction = action;
            NonEpsilonLength = conclusion.Count(s => !s.IsEpsilon);
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
