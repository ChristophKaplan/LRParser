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
    public PropositionalLogic() : base(new TokenDefinition<Terminal>(Terminal.Function, "Mod|Forget|Int|Simplify|SwitchMany"),
        new TokenDefinition<Terminal>(Terminal.Open, "\\("),
        new TokenDefinition<Terminal>(Terminal.Comma, ","),
        new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
        new TokenDefinition<Terminal>(Terminal.Connective, "AND|OR"),
        new TokenDefinition<Terminal>(Terminal.Negation, "NOT|!"),
        new TokenDefinition<Terminal>(Terminal.AtomicSentence, "[A-Z][a-z]*")) {
    }

    protected override void SetUpGrammar() {
        AddByEnumType(typeof(Terminal));
        AddByEnumType(typeof(NonTerminal));
        AddStartSymbol(NonTerminal.StartSymbol);

        var rule01 = AddProductionRule(NonTerminal.StartSymbol, NonTerminal.SecondStart);
        var rule02 = AddProductionRule(NonTerminal.SecondStart, NonTerminal.Sentence);
        var rule03 = AddProductionRule(NonTerminal.SecondStart, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        var rule04 = AddProductionRule(NonTerminal.Sentence, Terminal.AtomicSentence);
        var rule05 = AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        var rule06 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.AtomicSentence, Terminal.Connective, NonTerminal.Sentence);
        var rule07 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close, Terminal.Connective, NonTerminal.Sentence);
        var rule08 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Negation, NonTerminal.Sentence);

        var rule09 = AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.SecondStart, NonTerminal.Ext);
        var rule10 = AddProductionRule(NonTerminal.Ext, Terminal.Comma, NonTerminal.SecondStart, NonTerminal.Ext);
        var rule11 = AddProductionRule(NonTerminal.Ext, Terminal.Close);
        
        rule01.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule02.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule03.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });

        rule04.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute);
        });

        rule05.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });

        rule06.SetSemanticAction((lhs, rhs) => {
            switch (((LexValue)rhs[1].SyntheticAttribute).Value) {
                case "OR": {
                    var p = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute);
                    lhs.SyntheticAttribute = new ComplexSentence(p, "OR", (Sentence)rhs[2].SyntheticAttribute);
                    return;
                }
                case "AND": {
                    var p = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute);
                    lhs.SyntheticAttribute = new ComplexSentence(p, "AND", (Sentence)rhs[2].SyntheticAttribute);
                    return;
                }
                default:
                    throw new Exception($"Error: {rhs[1].SyntheticAttribute} operator not found!");
            }
        });

        rule07.SetSemanticAction((lhs, rhs) => {
            switch (((LexValue)rhs[3].SyntheticAttribute).Value) {
                case "OR":
                    lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, "OR", (Sentence)rhs[4].SyntheticAttribute);
                    return;
                case "AND":
                    lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, "AND", (Sentence)rhs[4].SyntheticAttribute);
                    return;
                default:
                    throw new Exception($"Error: {rhs[3].SyntheticAttribute} operator not found!");
            }
        });

        rule08.SetSemanticAction((lhs, rhs) => lhs.SyntheticAttribute = new ComplexSentence("NOT", (Sentence)rhs[1].SyntheticAttribute));

        rule09.SetSemanticAction((lhs, rhs) => {
            var func = (LexValue)rhs[0].SyntheticAttribute;
            var ext = (ArrayValue)rhs[3].SyntheticAttribute;
            ext.Insert(rhs[2].SyntheticAttribute,0);

            for (var i = 0; i < ext.Value.Length; i++) {
                if (ext.Value[i] is Function f) {
                    ext.Value[i] = ExecuteFunction(f);
                }
            }

            lhs.SyntheticAttribute = new Function(func.Value, ext.Value);
        });
        
        rule10.SetSemanticAction((lhs, rhs) => {
            var second = (AtomicSentence)rhs[1].SyntheticAttribute;
            var ext = (ArrayValue)rhs[2].SyntheticAttribute;
            
            ext.Add(second);
            lhs.SyntheticAttribute = ext;
        });
        
        rule11.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new ArrayValue(Array.Empty<ILanguageObject>());
        });
    }

    private ILanguageObject ExecuteFunction(Function function) {
        switch (function.Func) {
            case "Int": {
                return this.Int((Sentence)function.Parameters[0]);
            }
            case "Mod": {
                return this.Mod((Sentence)function.Parameters[0]);
            }
            case "Simplify": {
                var result = this.Simplify((Sentence)function.Parameters[0]);
                Console.WriteLine($"Simplify: {function.Parameters[0]} equals: {result}");
                return result;
            }
            case "Forget": {
                var result = this.Forget((Sentence)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                Console.WriteLine($"Forget {function.Parameters[0]} in {function.Parameters[0]} equals: {result}");
                return result;
            }
            case "SwitchMany": {
                var result = this.SwitchMany((InterpretationSet)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                return result;
            }
        }

        Console.WriteLine($"No return value for: {function.Func}");
        return null;
    }

    public List<Interpretation> GenerateInterpretations(Sentence sentence) {
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

    protected override ILanguageObject TryParse(string input) {
        var langObj = base.TryParse(input);
        if (langObj is Function function) {
            return ExecuteFunction(function);
        }

        return langObj;
    }

    public void Interpret(string[] input) {
        foreach (var s in input) {
            Console.WriteLine(TryParse(s));
        }
    }
}