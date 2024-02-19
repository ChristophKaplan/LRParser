namespace PropositionalLogic;

public interface IPropositionalLanguage
{
}

public class Function:IPropositionalLanguage
{
    public string func;
    public Sentence sentence;
    public Sentence[] parameters;
    public Function(string func, Sentence sentence, params Sentence[] parameters)
    {
        this.func = func;
        this.sentence = sentence;
        this.parameters = parameters;
    }
}

public abstract class Sentence:IPropositionalLanguage
{
    public readonly List<Sentence> Children = new();
    protected void AddChild(Sentence sentence) => Children.Add(sentence);

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        return ToString().Equals(obj.ToString());
    }
    
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString() {
        if(this is AtomicSentence atomicSentence) {
            return atomicSentence.symbol;
        }
        else if(this is ComplexSentence complexSentence) {
            if (complexSentence.Operator.Equals("NOT")) {
                return $"{complexSentence.Operator} {complexSentence.Children[0]}";
            }
            return $"{complexSentence.Children[0]} {complexSentence.Operator} {complexSentence.Children[1]}";
        }
        else {
            return "Sentence";
        }
    }
}

public class AtomicSentence:Sentence
{
    public readonly string symbol;
    public AtomicSentence(string symbol)
    {
        this.symbol = symbol;
    }
}

public class ComplexSentence:Sentence
{
    public readonly string Operator;
    public ComplexSentence(Sentence p, string @operator, Sentence q)
    {
        Operator = @operator;
        AddChild(p);
        AddChild(q);
    }
    
    public ComplexSentence(string @operator, Sentence p)
    {
        Operator = @operator;
        AddChild(p);
    }
}