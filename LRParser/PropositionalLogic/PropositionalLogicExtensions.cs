namespace PropositionalLogic;

public static class PropositionalLogicExtensions
{
    public static Sentence Forget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.FindReplaceAtom(forgetMe, "True");
        rhs.FindReplaceAtom(forgetMe, "False");
        var n = new ComplexSentence(lhs, "OR", rhs);
        return n;
    }
    
    public static void Simplify(this PropositionalLogic logic,Sentence sentence)
    {
        if(sentence is AtomicSentence atomicSentence) {
            
        }
        else if (sentence is ComplexSentence complexSentence) {
            
            SimplifyTruthValues(complexSentence);
            
            for (var i = 0; i < sentence.Children.Count; i++)
            {
                Simplify(logic, sentence.Children[i]);
            }
        }
    }

    private static bool SimplifyTruthValues(Sentence sentence)
    {
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

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
        
        if (((ComplexSentence)sentence).Operator.Equals("AND")) {
            if (atomicTruthValue.Symbol.Equals("True")) {
                other.Reparent(sentence);   
                sentence = other;
                return true;
            }
            else if (atomicTruthValue.Symbol.Equals("False")) {
                atomicTruthValue.Reparent(sentence);
                sentence = atomicTruthValue;
                return true;
            }
        }
        else if (((ComplexSentence)sentence).Operator.Equals("OR")) {
            if (atomicTruthValue.Symbol.Equals("True")) {
                atomicTruthValue.Reparent(sentence);
                sentence = atomicTruthValue;
                return true;
            }
            else if (atomicTruthValue.Symbol.Equals("False")) {
                other.Reparent(sentence);   
                sentence = other;
                return true;
            }
        }

        return false;
    }
}