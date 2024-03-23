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
        return ToTable(Interpretations, Sentences);
    }
}