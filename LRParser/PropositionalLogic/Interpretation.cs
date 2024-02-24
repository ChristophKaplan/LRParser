using System.Text;

namespace PropositionalLogic;

public class Interpretation {
    public readonly Dictionary<AtomicSentence, bool> _truthValues = new();
    
    public void Add(AtomicSentence atom, bool truthValue) {
        _truthValues.TryAdd(atom, truthValue);
    }

    private bool Evaluate(AtomicSentence atomicSentence) {
        if (_truthValues.TryGetValue(atomicSentence, out var value)) {
            return value;
        }

        throw new Exception($"Error: {atomicSentence} not found in interpretation.");
    }

    private bool Evaluate(ComplexSentence complexSentence) {
        return complexSentence.Operator switch {
            "NOT" => !Evaluate(complexSentence.Children[0]),
            "AND" => Evaluate(complexSentence.Children[0]) && Evaluate(complexSentence.Children[1]),
            "OR" => Evaluate(complexSentence.Children[0]) || Evaluate(complexSentence.Children[1]),
            _ => throw new Exception($"Error: subtype of {this} not found.")
        };
    }

    public bool Evaluate(Sentence sentence) {
        return sentence switch {
            AtomicSentence atomicSentence => Evaluate(atomicSentence),
            ComplexSentence complexSentence => Evaluate(complexSentence),
            _ => throw new Exception($"Error: subtype of {this} not found.")
        };
    }

    public override bool Equals(object? obj) {
        return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
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