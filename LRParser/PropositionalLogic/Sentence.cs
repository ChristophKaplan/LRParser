namespace LRParser.PropositionalLogic;

public abstract class Sentence
{
    private readonly List<Sentence> _children = new();
    protected void AddChild(Sentence sentence) => _children.Add(sentence);
    
    public bool Evaluate() {
        switch (this) {
            case AtomicSentence atomicSentence:
                return true;
            case ComplexSentence complexSentence:
                switch (complexSentence.Operator) {
                    case "NOT":
                        return !complexSentence._children[0].Evaluate();
                    case "AND":
                        return complexSentence._children[0].Evaluate() && complexSentence._children[1].Evaluate();
                    case "OR":
                        return complexSentence._children[0].Evaluate() || complexSentence._children[1].Evaluate();
                }

                break;
        }

        throw new Exception($"Error: subtype of {this} not found.");
    }

    public override string ToString() {
        if(this is AtomicSentence atomicSentence) {
            return atomicSentence.symbol;
        }
        else if(this is ComplexSentence complexSentence) {
            if (complexSentence.Operator.Equals("NOT")) {
                return $"{complexSentence.Operator} {complexSentence._children[0]}";
            }
            return $"{complexSentence._children[0]} {complexSentence.Operator} {complexSentence._children[1]}";
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