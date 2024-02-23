namespace PropositionalLogic;

public interface IPropositionalLanguage {
}

public class Function : IPropositionalLanguage {
    public readonly string Func;
    public readonly Sentence[] Parameters;
    public readonly Sentence Sentence;

    public Function(string func, Sentence sentence, params Sentence[] parameters) {
        Func = func;
        Sentence = sentence;
        Parameters = parameters;
    }
}

public abstract class Sentence : IPropositionalLanguage {
    public readonly List<Sentence> Children = new();

    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
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

            return $"{complexSentence.Children[0]} {complexSentence.Operator} {complexSentence.Children[1]}";
        }

        return "Sentence";
    }
}

public class AtomicSentence : Sentence {
    public string Symbol;

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