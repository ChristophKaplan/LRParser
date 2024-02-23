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
        new TokenDefinition<Terminal>(Terminal.Function, "Mod|Forget|Int"),
        new TokenDefinition<Terminal>(Terminal.Open, "\\("),
        new TokenDefinition<Terminal>(Terminal.Comma, ","),
        new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
        new TokenDefinition<Terminal>(Terminal.Connective, "AND|OR"),
        new TokenDefinition<Terminal>(Terminal.Negation, "NOT|!"),
        new TokenDefinition<Terminal>(Terminal.AtomicSentence, "[A-Z][a-z]*")) {
    }

    public object ExecuteFunction(Function function) {
        if (function.Func.Equals("Int")) {
            Console.WriteLine($"Interpretations for {function.Sentence}");
            var interpretations = GenerateInterpretations(function.Sentence);
            
            foreach (var interpretation in interpretations) {
                Console.WriteLine($"{interpretation}");
            }
        }
        
        if (function.Func.Equals("Mod")) {
            Console.WriteLine($"Models for {function.Sentence}");
            var interpretations = GenerateInterpretations(function.Sentence);
            
            foreach (var interpretation in interpretations) {
                var t = interpretation.Evaluate(function.Sentence);
                if (t) {
                    Console.WriteLine($"{interpretation}");
                }
            }
        }

        if (function.Func.Equals("Forget")) {
            Console.WriteLine($"Forget {function.Parameters[0]} in {function.Sentence}");
            var result = Forget(function.Sentence,(AtomicSentence)function.Parameters[0]);
            return result;
        }

        return null;
    }

    private Sentence Forget(Sentence sentence, AtomicSentence forgetMe) {
        var lhs = sentence.GetCopy();
        var rhs = sentence.GetCopy();
        lhs.ReplaceAtom(forgetMe, "True");
        rhs.ReplaceAtom(forgetMe, "False");
        var n = new ComplexSentence(lhs, "OR", rhs);
        return n;
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

        AddProductionRule(NonTerminal.StartSymbol, NonTerminal.SecondStart);
        AddProductionRule(NonTerminal.SecondStart, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.Sentence, Terminal.AtomicSentence);
        AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.AtomicSentence, Terminal.Connective, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.Negation, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.Sentence, NonTerminal.Ext, Terminal.Close);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        AddProductionRule(NonTerminal.Ext, Terminal.Comma, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.SecondStart, Terminal.Close);

        AddStartSymbol(NonTerminal.StartSymbol);

        AddSemanticAction(0, input => input[0]);
        AddSemanticAction(1, input => input[0]);

        AddSemanticAction(2,
            input => {
                var p = new AtomicSentence((string)input[0]);
                return p;
            });

        AddSemanticAction(3, input => input[0]);
        AddSemanticAction(4,
            input => {
                if ((string)input[1] == "OR") {
                    var p = new AtomicSentence((string)input[0]);
                    return new ComplexSentence(p, "OR", (Sentence)input[2]);
                }

                if ((string)input[1] == "AND") {
                    var p = new AtomicSentence((string)input[0]);
                    return new ComplexSentence(p, "AND", (Sentence)input[2]);
                }

                throw new Exception("Error:");
            });

        AddSemanticAction(5, input => new ComplexSentence("NOT", (Sentence)input[1]));

        AddSemanticAction(6,
            input => {
                var func = (string)input[0];
                var sentence = (Sentence)input[2];
                var parameters = (Sentence)input[3];
                return new Function(func, sentence, parameters);
            });

        AddSemanticAction(7, input => input[0]);
        AddSemanticAction(8, input => input[1]);

        AddSemanticAction(9,
            input => {
                var func = (string)input[0];
                var f = (Function)input[2];
                var sentence = (Sentence)ExecuteFunction(f);
                return new Function(func, sentence);
            });
    }
}