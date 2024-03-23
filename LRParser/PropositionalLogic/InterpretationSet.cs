using System.Text;
using LRParser.Language;
using ConsoleTables;

namespace PropositionalLogic;

public class InterpretationSet : ILanguageObject{
    public readonly List<Interpretation> Interpretations;
    public readonly List<Sentence> Sentences;
  
    public InterpretationSet(List<Interpretation> interpretations, params Sentence[] sentences) {
        Interpretations = interpretations;
        Sentences = sentences.ToList();
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

    private string ToTable2(List<Interpretation> interpretations, List<Sentence> sentences) {
        var columns = interpretations[0]._truthValues.Keys.Select(key => key.ToString()).ToList();
        columns.AddRange(sentences.Select(sentence => sentence.ToString()));

        var table = new ConsoleTable(columns.ToArray());
    
        foreach (var interpretation in interpretations) {
            var row = interpretation._truthValues.Values.Select(value => value.ToString()).ToList();
            row.AddRange(sentences.Select(sentence => interpretation.Evaluate(sentence).ToString()));
            table.AddRow(row.ToArray());
        }

        return table.ToMinimalString();
    }
    
    public override string ToString() {
        return ToTable2(Interpretations, Sentences);
    }
}