using System.Text;
using LRParser.Language;

namespace PropositionalLogic;

public class InterpretationSet : ILanguageObject{
    public readonly List<Interpretation> Interpretations;
    public readonly Sentence Sentence;

    public InterpretationSet(List<Interpretation> interpretations, Sentence sentence) {
        Interpretations = interpretations;
        Sentence = sentence;
    }
    
    private string ToTable(List<Interpretation> interpretations, List<Sentence> sentences) {
        if(interpretations.Count == 0) return $"No interpretations for this set ?";
        
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
        return ToTable(Interpretations, new List<Sentence>() { Sentence });
    }
}