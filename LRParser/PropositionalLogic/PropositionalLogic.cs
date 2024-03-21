using System.Text;
using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace PropositionalLogic;

public enum Terminal {
    Function,
    Open,
    Comma,
    Close,
    Connective,
    Negation,
    AtomicSentence,
}

public enum NonTerminal {
    StartSymbol, SecondStart, Sentence, ComplexSentence, Ext
}

public class PropositionalLogic : Language<Terminal, NonTerminal> {
    public PropositionalLogic(): base(
        new TokenDefinition<Terminal>(Terminal.Function, "Mod|Forget|Int|Simplify"),
        new TokenDefinition<Terminal>(Terminal.Open, "\\("),
        new TokenDefinition<Terminal>(Terminal.Comma, ","),
        new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
        new TokenDefinition<Terminal>(Terminal.Connective, "AND|OR"),
        new TokenDefinition<Terminal>(Terminal.Negation, "NOT|!"),
        new TokenDefinition<Terminal>(Terminal.AtomicSentence, "[A-Z][a-z]*")) {
    }
    
    public object ExecuteFunction(Function function) {
        switch (function.Func) {
            case "Int": {
                var interpretations = GenerateInterpretations(function.Sentence);
                Console.WriteLine($"Interpretations for {function.Sentence}\n{ToTable(interpretations, new List<Sentence>(){function.Sentence})}");
                break;
            }
            case "Mod": {
                var interpretations = GenerateInterpretations(function.Sentence);
                var models = new List<Interpretation>();
                foreach (var interpretation in interpretations) {
                    var mod = interpretation.Evaluate(function.Sentence);
                    if (mod) { models.Add(interpretation); }
                }
                Console.WriteLine($"Models for {function.Sentence}\n{ToTable(models, new List<Sentence>(){function.Sentence})}");
                break;
            }
            case "Forget": {
                Console.WriteLine($"Forget {function.Parameters[0]} in {function.Sentence}");
                var result = this.Forget(function.Sentence,(AtomicSentence)function.Parameters[0]);
                return result;
            }
            case "Simplify": {
                var result = this.Simplify(function.Sentence);
                Console.WriteLine($"Simplify: {function.Sentence} to: {result}");
                return result;
            }
        }

        Console.WriteLine($"No return value for: {function.Func}");
        return null;
    }

    private List<Interpretation> GenerateInterpretations(Sentence sentence) {
        var interpretations = new List<Interpretation>();
        
        var atoms = sentence.GetAtoms();
        var truthTable = GenerateTruthTable(atoms.Count);

        foreach (var truthValues in truthTable) {
            var interpretation = new Interpretation();
            var list = truthValues.ToArray();
            for (var i = 0; i < atoms.Count; i++) {
                
                //validate, tautologies/contradictions
                if (atoms[i].Symbol.Equals("True")) {
                    list[i] = true;
                }

                if (atoms[i].Symbol.Equals("False")) {
                    list[i] = false;
                }

                interpretation.Add(atoms[i], list[i]);
            }

            if (!interpretations.Contains(interpretation)) {
                interpretations.Add(interpretation);
            }
        }

        return interpretations;
    }

    private IEnumerable<IEnumerable<bool>> GenerateTruthTable(int n) {
        switch (n) {
            case 0:
                return Enumerable.Empty<IEnumerable<bool>>();
            case 1:
                return new List<List<bool>> { new() { true }, new() { false } };
            default: {
                var subTables = GenerateTruthTable(n - 1);
                return subTables.SelectMany(row => new[] { row.Append(true), row.Append(false) });
            }
        }
    }

    protected override void SetUpGrammar() {
        AddByEnumType(typeof(Terminal));
        AddByEnumType(typeof(NonTerminal));
        AddStartSymbol(NonTerminal.StartSymbol);
        
        AddProductionRule(NonTerminal.StartSymbol, NonTerminal.SecondStart);
        AddProductionRule(NonTerminal.SecondStart, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.SecondStart,Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        AddProductionRule(NonTerminal.Sentence, Terminal.AtomicSentence);
        AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.AtomicSentence, Terminal.Connective, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close, Terminal.Connective, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.Negation, NonTerminal.Sentence);
        
        //functions
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.Sentence, NonTerminal.Ext, Terminal.Close);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        AddProductionRule(NonTerminal.Ext, Terminal.Comma, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.SecondStart, Terminal.Close);
        
        AddSemanticAction(0, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        AddSemanticAction(1, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        AddSemanticAction(2, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });

        AddSemanticAction(3, (lhs, rhs) => { lhs.SyntheticAttribute = new AtomicSentence((string)rhs[0].SyntheticAttribute); });

        AddSemanticAction(4, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        
        AddSemanticAction(5,
            (lhs, rhs) => {
                if ((string)rhs[1].SyntheticAttribute == "OR") {
                    var p = new AtomicSentence((string)rhs[0].SyntheticAttribute);
                    lhs.SyntheticAttribute = new ComplexSentence(p, "OR", (Sentence)rhs[2].SyntheticAttribute);
                    return;
                }

                if ((string)rhs[1].SyntheticAttribute == "AND") {
                    var p = new AtomicSentence((string)rhs[0].SyntheticAttribute);
                    lhs.SyntheticAttribute = new ComplexSentence(p, "AND", (Sentence)rhs[2].SyntheticAttribute);
                    return;
                }

                throw new Exception($"Error: {rhs[1].SyntheticAttribute} operator not found!");
            });

        AddSemanticAction(6,
            (lhs, rhs) => {
                if ((string)rhs[3].SyntheticAttribute == "OR") {
                    lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, "OR", (Sentence)rhs[4].SyntheticAttribute);
                    return;
                }

                if ((string)rhs[3].SyntheticAttribute == "AND") {
                    lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, "AND", (Sentence)rhs[4].SyntheticAttribute);
                    return;
                }

                throw new Exception($"Error: {rhs[3].SyntheticAttribute} operator not found!");
            });

        
        AddSemanticAction(7, (lhs, rhs) => lhs.SyntheticAttribute = new ComplexSentence("NOT", (Sentence)rhs[1].SyntheticAttribute));

        AddSemanticAction(8,
            (lhs, rhs) => {
                var func = (string)rhs[0].SyntheticAttribute;
                var sentence = (Sentence)rhs[2].SyntheticAttribute;
                var parameters = (Sentence)rhs[3].SyntheticAttribute;
                lhs.SyntheticAttribute = new Function(func, sentence, parameters);
            });

        AddSemanticAction(9,
            (lhs, rhs) => {
                var func = (string)rhs[0].SyntheticAttribute;
                var sentence = (Sentence)rhs[2].SyntheticAttribute;
                lhs.SyntheticAttribute = new Function(func, sentence);
            });
        
        AddSemanticAction(10, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });

        AddSemanticAction(11,
            (lhs, rhs) => {
                var func = (string)rhs[0].SyntheticAttribute;
                var f = (Function)rhs[2].SyntheticAttribute;
                var sentence = (Sentence)ExecuteFunction(f);
                lhs.SyntheticAttribute = new Function(func, sentence);
            });
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
}