using LRParser.Language;

namespace PropositionalLogic;

public class Function : ILanguageObject {
    public readonly string Func;
    public readonly Sentence[] Parameters;
    public readonly Sentence Sentence;

    public Function(string func, Sentence sentence, params Sentence[] parameters) {
        Func = func;
        Sentence = sentence;
        Parameters = parameters;
    }
}

public abstract class Sentence : ILanguageObject {
    public Sentence Parent;
    public readonly List<Sentence> Children = new();

    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
        sentence.Parent = this;
    }
    
    public void InsertChild(int index, Sentence sentence) {
        Children.Insert(index,sentence);
        sentence.Parent = this;
    }
    
    public void Reparent(Sentence parentOfThis) {
        
        if (parentOfThis.Parent == null) {
            return;
        }
        
        Sentence parent = parentOfThis.Parent;
        Sentence found = null;
        foreach (var childInParent in parent.Children) {
            if (childInParent.Equals(parentOfThis)) {
                found = childInParent;
            }
        }

        var index = parent.Children.IndexOf(found);
        parent.Children.RemoveAt(index);
        parent.InsertChild(index, this);
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override string ToString() {
        if (this is AtomicSentence atomicSentence) {
            return atomicSentence.Symbol;
        }

        if (this is ComplexSentence complexSentence) {
            if (complexSentence.Operator.Equals("NOT")) {
                return $"{complexSentence.Operator} {complexSentence.Children[0]}";
            }

            return $"({complexSentence.Children[0]} {complexSentence.Operator} {complexSentence.Children[1]})";
        }

        return "Sentence";
    }
}

public class AtomicSentence : Sentence {
    public string Symbol;
    public bool IsTruthValue { get => Symbol.Equals("True") || Symbol.Equals("False"); }
    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }
}

public class ComplexSentence : Sentence {
    public readonly string Operator;

    public ComplexSentence(Sentence p, string @operator, Sentence q) {
        Operator = @operator;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(string @operator, Sentence p) {
        Operator = @operator;
        AddChild(p);
    }
    
    public ComplexSentence(string @operator) {
        Operator = @operator;
    }
}