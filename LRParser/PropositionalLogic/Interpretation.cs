using System.Text;
using LRParser.Language;

namespace PropositionalLogic;

public class Interpretation : ILanguageObject{
    public readonly Dictionary<AtomicSentence, bool> _truthValues = new();
    
    public Interpretation() { }
    
    public Interpretation(Interpretation other) {
        foreach (var kv in other._truthValues) {
            _truthValues.Add(kv.Key, kv.Value);
        }
    }
    
    public Interpretation Switch(AtomicSentence variable) {
        var interpretation = new Interpretation(this);
        interpretation._truthValues[variable] = !interpretation._truthValues[variable];
        return interpretation;
    }
    
    public void Add(AtomicSentence atom, bool truthValue) {
        _truthValues.TryAdd(atom, truthValue);
    }

    private bool Evaluate(AtomicSentence atomicSentence) {
        if (atomicSentence.Tautology) return true;
        if (atomicSentence.Falsum) return false;
        
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

    public bool EqualVariables(Interpretation other) {
        if (_truthValues.Count != other._truthValues.Count) {
            return false;
        }
        foreach (var kv in _truthValues) {
            if (!other._truthValues.TryGetValue(kv.Key, out var value)) {
                return false;
            }
        }
        
        return true;
    }
    
    public override bool Equals(object? obj) {
        return GetHashCode().Equals(obj?.GetHashCode());
    }

    public override int GetHashCode() {
        var hash = 17;
        foreach (var kv in _truthValues) {
            var (key, value) = kv;
            hash = HashCode.Combine(hash ,key, value);
        }

        return hash;
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