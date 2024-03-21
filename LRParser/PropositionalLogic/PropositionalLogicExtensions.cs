using System.Runtime.InteropServices;

namespace PropositionalLogic;

public static class PropositionalLogicExtensions
{
    public static void Simplify(this PropositionalLogic logic, Sentence sentence)
    {
        if(sentence is AtomicSentence atomicSentence) {
            
        }
        else if (sentence is ComplexSentence complexSentence)
        {
            while (SimplifyTruthValues(complexSentence))
            {
                
            }
            
            for (var i = 0; i < sentence.Children.Count; i++)
            {
                Simplify(logic, sentence.Children[i]);
            }
        }
    }

    private static bool SimplifyTruthValues(ComplexSentence complexSentence)
    {
        var lhs = complexSentence.Children[0];
        var rhs = complexSentence.Children[1];

        AtomicSentence atomicTruthValue = null;
        Sentence other = null;
        
        if (!(lhs is AtomicSentence { IsTruthValue: true } || rhs is AtomicSentence { IsTruthValue: true }))
        {
            return false;
        }
        
        if(lhs is AtomicSentence { IsTruthValue: true } atomicLhs)
        {
            atomicTruthValue = atomicLhs;
            other = rhs;
        }
        else if(rhs is AtomicSentence { IsTruthValue: true } atomicRhs) {
            atomicTruthValue = atomicRhs;
            other = lhs;
        }
        
        if(atomicTruthValue.Symbol.Equals("True")) {
            if (complexSentence.Operator.Equals("AND")) {
                complexSentence.ReplaceMeWith(other);
                return true;
            }
            if (complexSentence.Operator.Equals("OR")) {
                complexSentence.ReplaceMeWith(atomicTruthValue);
                return true;
            }
        }
        else if(atomicTruthValue.Symbol.Equals("False")) {
            if (complexSentence.Operator.Equals("AND")) {
                complexSentence.ReplaceMeWith(atomicTruthValue);
                return true;
            }
            if (complexSentence.Operator.Equals("OR")) {
                complexSentence.ReplaceMeWith(other);
                return true;
            }
        }

        return false;
    }
}