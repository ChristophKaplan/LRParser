using System.Text;
using LRParser.Language;

namespace PropositionalLogic;

public class InterpretationSet : ILanguageObject{
    public readonly List<Interpretation> _interpretations;
    public Sentence _sentence;

    public InterpretationSet(List<Interpretation> interpretations, Sentence sentence) {
        _interpretations = interpretations;
        _sentence = sentence;
    }
    
    public Interpretation Switch(Interpretation switchMe, AtomicSentence variable) { 
        /* where Switch(w, x) denotes the interpretation that maintains the same truth value as w for all variables except x,
        and assigns to x the opposite truth value given by w.*/
        
        bool IsSwitchable(Interpretation interpretation, Interpretation switchMe, AtomicSentence x) {
            if (!interpretation.EqualVariables(switchMe)) return false;

            foreach (var key in interpretation._truthValues.Keys) {
                if (interpretation._truthValues[key] != switchMe._truthValues[key]) {
                    return key.Equals(x);
                }
            }

            return false;
        }
        
        foreach (var interpretation in _interpretations) {
            if (IsSwitchable(interpretation, switchMe, variable)) {
                return interpretation;
            }
        }

        throw new Exception($"No switchable interpretation found for Switch({switchMe}, {variable})");
    }
    
    private string ToTable(List<Interpretation> interpretations, List<Sentence> sentences) {
        var tab = new StringBuilder();
        var keys = new StringBuilder();
        
        foreach (var (key, value) in interpretations[0]._truthValues) {
            keys.Append($"|{key}\t");
        }

        foreach (var sentence in sentences)
        {
            keys.Append($"||{sentence}\t");
        }
        
        tab.Append($"{keys}\n");
        
        foreach (var interpretation in interpretations) {
            var values = new StringBuilder();
            foreach (var (key, value) in interpretation._truthValues) {
                values.Append($"|{value}\t");
            }

            foreach (var sentence in sentences)
            { 
                values.Append($"||{interpretation.Evaluate(sentence)}\t");   
            }
            
            tab.Append($"{values}\n");
        }
        
        return tab.ToString();
    }

    public override string ToString() {
        return ToTable(_interpretations, new List<Sentence>() { _sentence });
    }
}