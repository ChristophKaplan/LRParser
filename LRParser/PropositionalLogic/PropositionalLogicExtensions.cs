using LRParser.Language;

namespace PropositionalLogic;

public static class PropositionalLogicExtensions {
    public static LogicSymbols AsLogicSymbol(this LexValue lexValue) {
        Enum.TryParse<LogicSymbols>(lexValue.Value, out var symbol);
        return symbol;
    }

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
        foreach (var model in set.Interpretations) {
            list.Add(model.Switch(variable));
        }

        return new InterpretationSet(list, set.Sentences.ToArray());
    }

    public static InterpretationSet Int(this PropositionalLogic logic, params Sentence[] sentences) {
        var interpretations = logic.GenerateInterpretations(sentences);
        return new InterpretationSet(interpretations, sentences);
    }

    public static Sentence Forget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.FindReplaceAtom(forgetMe, "True");
        rhs.FindReplaceAtom(forgetMe, "False");
        var n = new ComplexSentence(lhs, LogicSymbols.OR, rhs);
        return n;
    }

    public static Sentence SkepForget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.FindReplaceAtom(forgetMe, "True");
        rhs.FindReplaceAtom(forgetMe, "False");
        var n = new ComplexSentence(lhs, LogicSymbols.AND, rhs);
        return n;
    }

    public static Sentence MyForget(this PropositionalLogic logic, Sentence sentence, AtomicSentence forgetMe) {
        AtomicSentence find = null;
        if (sentence is not ComplexSentence complexSentence) {
            return sentence;
        }

        var lhs = complexSentence.Children[0];
        var rhs = complexSentence.Children[1];
        
        if (lhs is ComplexSentence clhs) {
            logic.MyForget(clhs, forgetMe);
        }

        if (rhs is ComplexSentence crhs) {
            logic.MyForget(crhs, forgetMe);
        }
        
        if (lhs is AtomicSentence aLhs && aLhs.Equals(forgetMe)) {
            find = aLhs;
        }
        
        else if (rhs is AtomicSentence aRhs && aRhs.Equals(forgetMe)) {
            find = aRhs;
        }

        if (find != null) {
            if(complexSentence.Operator == LogicSymbols.AND) find.Symbol = "True";
            else if(complexSentence.Operator == LogicSymbols.OR) find.Symbol = "False";
        }
        
        return sentence;
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

        Absorption(ref copy);
        Absorption2(ref copy);
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

        (AtomicSentence truthValueSide, Sentence otherSide) _ = MapLhsRhs();

        switch (((ComplexSentence)sentence).Operator) {
            case LogicSymbols.AND when _.truthValueSide.Tautology:
                Replace(ref sentence, _.otherSide);
                break;
            case LogicSymbols.AND when _.truthValueSide.Falsum:
                Replace(ref sentence, _.truthValueSide);
                break;
            case LogicSymbols.OR when _.truthValueSide.Tautology:
                Replace(ref sentence, _.truthValueSide);
                break;
            case LogicSymbols.OR when _.truthValueSide.Falsum:
                Replace(ref sentence, _.otherSide);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        void StepDown(ref Sentence sentence) {
            for (var i = 0; i < sentence.Children.Count; i++) {
                var childSentence = sentence.Children[i];
                SimplifyTruthValues(ref childSentence);
            }
        }

        void Replace(ref Sentence sentence, Sentence replaceWith) {
            replaceWith.Reparent(sentence);
            sentence = replaceWith;
        }

        StepDown(ref sentence);
    }

    private static void Absorption(ref Sentence sentence) {
        if (!sentence.IsAtomComplexRelation(sentence, out var atomicSentence, out var complex)) {
            return;
        }

        if (complex.Children.Contains(atomicSentence) &&
            ((sentence is ComplexSentence { Operator: LogicSymbols.OR } && complex.Operator == LogicSymbols.AND) ||
             (sentence is ComplexSentence { Operator: LogicSymbols.AND } && complex.Operator == LogicSymbols.OR))) {
            atomicSentence.Reparent(sentence);
            sentence = atomicSentence;
        }
    }
    
    private static void Absorption2(ref Sentence sentence) {
        if (!sentence.IsAtomComplexRelation(sentence, out var atomicSentence, out var complex)) {
            return;
        }

        //(A AND (B AND A)) = (B AND A)
        //(A OR (B OR A)) = (B OR A)
        
        if (complex.Children.Contains(atomicSentence) &&
            ((sentence is ComplexSentence { Operator: LogicSymbols.AND } && complex.Operator == LogicSymbols.AND) ||
             (sentence is ComplexSentence { Operator: LogicSymbols.OR } && complex.Operator == LogicSymbols.OR))) {
            complex.Reparent(sentence);
            sentence = complex;
        }
    }
}