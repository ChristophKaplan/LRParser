namespace PropositionalLogic;

public static class SentenceExtensions {

    public static void ReplaceAtom(this Sentence sentence, AtomicSentence replaceMe, string replaceWith) {
        foreach (var child in sentence.Children) {
            child.ReplaceAtom(replaceMe, replaceWith);
        }

        if (sentence.Equals(replaceMe) && sentence is AtomicSentence atomicSentence) {
            atomicSentence.Symbol = replaceWith;
        }
    }
    
    public static List<AtomicSentence> GetAtoms(this Sentence sentence) {
        if(sentence is AtomicSentence atomicSentence) {
            return new List<AtomicSentence> {atomicSentence};
        }
        
        var atoms = new List<AtomicSentence>();
        foreach (var child in sentence.Children) {
            atoms.AddRange(child.GetAtoms());
        }

        return atoms;
    }

    public static Sentence GetCopy(this Sentence sentence) {
        switch (sentence) {
            case AtomicSentence atomicSentence:
                return new AtomicSentence(atomicSentence.Symbol);
            case ComplexSentence complexSentence: {
                var result = new ComplexSentence(complexSentence.Operator);
                foreach (var child in complexSentence.Children) {
                    result.AddChild(child.GetCopy());
                }

                return result;
            }
            default:
                throw new Exception("Sentence type not found!");
        }
    }
}