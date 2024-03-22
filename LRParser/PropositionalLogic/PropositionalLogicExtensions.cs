namespace PropositionalLogic;

public static class PropositionalLogicExtensions {
    
    public static InterpretationSet Mod(this PropositionalLogic logic, Sentence sentence) {
        var interpretations = logic.GenerateInterpretations(sentence);
        var models = new List<Interpretation>();
        
        foreach (var interpretation in interpretations) {
            var mod = interpretation.Evaluate(sentence);
            if (mod) { models.Add(interpretation); }
        }
        
        return new InterpretationSet(models, sentence);
    }
    
    public static InterpretationSet SwitchMany(this PropositionalLogic logic, InterpretationSet set, AtomicSentence variable) { 
        var list = new List<Interpretation>();
        foreach (var model in set._interpretations) {
            var i = set.Switch(model, variable);
            list.Add(i);
        }
        
        return new InterpretationSet(list, set._sentence);
    }
    
    public static InterpretationSet Int(this PropositionalLogic logic, Sentence sentence) {
        var interpretations = logic.GenerateInterpretations(sentence);
        return new InterpretationSet(interpretations, sentence);
    }

    public static Sentence Forget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.FindReplaceAtom(forgetMe, "True");
        rhs.FindReplaceAtom(forgetMe, "False");
        var n = new ComplexSentence(lhs, "OR", rhs);
        return n;
    }

    public static Sentence Simplify(this PropositionalLogic logic, Sentence sentence) {
        var old = sentence;
        var copy = sentence.GetCopy(); 
        var changed = true;
        
        while (changed) {
            SimplifyTruthValues(ref copy);
            changed = !old.Equals(copy);
            old = copy.GetCopy();
        }       
        
        return copy;
    }

    private static void SimplifyTruthValues(ref Sentence sentence) {
        if (sentence is AtomicSentence) return;
        
        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];
        
        if (!(lhs is AtomicSentence { IsTruthValue: true } || rhs is AtomicSentence { IsTruthValue: true })) {
            StepDown(ref sentence);
            return;
        }

        (AtomicSentence, Sentence) MapLhsRhs() {
            (AtomicSentence atomicTruthValue, Sentence other) result = (null, null);
            if (lhs is AtomicSentence { IsTruthValue: true } atomicLhs) {
                result = (atomicLhs, rhs);
                
            }
            if (rhs is AtomicSentence { IsTruthValue: true } atomicRhs) {
                result = (atomicRhs, lhs);
            }
            return result;
        }
        
        (AtomicSentence truthValueSide, Sentence otherSide) _= MapLhsRhs();
        
        switch (((ComplexSentence)sentence).Operator) {
            case "AND" when _.truthValueSide.Symbol.Equals("True"):
                Replace(ref sentence, _.otherSide);
                break;
            case "AND" when _.truthValueSide.Symbol.Equals("False"):
                Replace(ref sentence, _.truthValueSide);
                break;
            case "OR" when _.truthValueSide.Symbol.Equals("True"):
                Replace(ref sentence, _.truthValueSide);
                break;
            case "OR" when _.truthValueSide.Symbol.Equals("False"):
                Replace(ref sentence, _.otherSide);
                break;
            default:
                break;
        }
        
        void StepDown(ref Sentence sentence) {
            for (var i = 0; i < sentence.Children.Count; i++) {
                var childSentence = sentence.Children[i];
                SimplifyTruthValues(ref childSentence);
            }
        }
        
        void Replace(ref Sentence sentence, Sentence replaceWith ) {
            replaceWith.Reparent(sentence);
            sentence = replaceWith;
        }
        
        StepDown(ref sentence);
    }
}