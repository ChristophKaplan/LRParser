using System.Text;

namespace PropositionalLogic;

public class Interpretation
{
    private Dictionary<AtomicSentence, bool> _truthValues = new();
    
    public void Add(AtomicSentence atom, bool truthValue)
    {
        _truthValues.Add(atom, truthValue);
    }
    
    private bool Evaluate(AtomicSentence atomicSentence)
    {
        if(_truthValues.TryGetValue(atomicSentence, out var value)) {
            return value;
        }
        
        throw new Exception($"Error: {atomicSentence} not found in interpretation.");
    }

    private bool Evaluate(ComplexSentence complexSentence)
    {
        switch (complexSentence.Operator) {
            case "NOT":
                return !Evaluate(complexSentence.Children[0]);
            case "AND":
                return Evaluate(complexSentence.Children[0]) && Evaluate(complexSentence.Children[1]);
            case "OR":
                return Evaluate(complexSentence.Children[0]) || Evaluate(complexSentence.Children[1]);
        }

        throw new Exception($"Error: subtype of {this} not found.");
    }

    public bool Evaluate(Sentence sentence){
        switch (sentence) {
            case AtomicSentence atomicSentence:
                return Evaluate(atomicSentence);
            case ComplexSentence complexSentence:
                return Evaluate(complexSentence);
        }

        throw new Exception($"Error: subtype of {this} not found.");
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in _truthValues) {
            sb.Append($"{key}={value}, ");
        }
        return sb.ToString();
    }
}