namespace PropositionalLogic;

public static class SentenceExtensions {
    public static void Forget(this Sentence sentence, AtomicSentence forgetMe) {
        foreach (var child in sentence.Children) {
            child.Forget(forgetMe);
        }

        if (sentence.Equals(forgetMe)) {
            //var parent = sentence.Parent;
        }
    }
    
    public static void PostOrder(this Sentence sentence, Action action) {
        foreach (var child in sentence.Children) {
            child.PostOrder(action);
        }
        
        action.Invoke();
    }
}