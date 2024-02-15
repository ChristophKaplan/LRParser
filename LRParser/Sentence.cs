namespace LRParser;

public abstract class Sentence
{
    private readonly List<Sentence> _children = new();
    public void AddChild(Sentence sentence) => _children.Add(sentence);
    
    public void PreOrderReverse(Action<Sentence> action) {
        action(this);
        for (var i = _children.Count-1; i >= 0; i--) {
            var child = _children[i];
            child.PreOrderReverse(action);
        }
    }
}

public class AtomicSentence:Sentence
{
    private string symbol;
    public AtomicSentence(string symbol)
    {
        this.symbol = symbol;
    }
}

public class ComplexSentence:Sentence
{
    private string connective;
    public ComplexSentence(Sentence p, string connective, Sentence q)
    {
        this.connective = connective;
        AddChild(p);
        AddChild(q);
    }
}