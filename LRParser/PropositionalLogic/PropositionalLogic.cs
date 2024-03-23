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
    StartSymbol, LangObject, Sentence, ComplexSentence, Ext
}

public class PropositionalLogic : Language<Terminal, NonTerminal> {
    public PropositionalLogic() : base(
        new TokenDefinition<Terminal>(Terminal.Function, "Mod|Forget|SkepForget|MyForget|Int|Simplify|SwitchMany"),
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

        var rule01 = AddProductionRule(NonTerminal.StartSymbol, NonTerminal.LangObject);
        var rule02 = AddProductionRule(NonTerminal.LangObject, NonTerminal.Sentence);
        
        var rule03 = AddProductionRule(NonTerminal.Sentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        var rule04 = AddProductionRule(NonTerminal.Sentence, Terminal.AtomicSentence);
        var rule05 = AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        
        var rule06 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.AtomicSentence, Terminal.Connective, NonTerminal.Sentence);
        var rule07 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close, Terminal.Connective, NonTerminal.Sentence);
        
        var rule08 = AddProductionRule(NonTerminal.ComplexSentence, Terminal.Negation, NonTerminal.Sentence);

        var rule09 = AddProductionRule(NonTerminal.LangObject, Terminal.Function, Terminal.Open, NonTerminal.LangObject, NonTerminal.Ext);
        var rule10 = AddProductionRule(NonTerminal.Ext, Terminal.Comma, NonTerminal.LangObject, NonTerminal.Ext);
        var rule11 = AddProductionRule(NonTerminal.Ext, Terminal.Close);
        
        rule01.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule02.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule03.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });

        rule04.SetSemanticAction((lhs, rhs) => {
            lhs.SyntheticAttribute = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute);
        });

        rule05.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });

        rule06.SetSemanticAction((lhs, rhs) => {
            switch (((LexValue)rhs[1].SyntheticAttribute).AsLogicSymbol()) {
                case LogicSymbols.OR: {
                    var p = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute);
                    lhs.SyntheticAttribute = new ComplexSentence(p, LogicSymbols.OR, (Sentence)rhs[2].SyntheticAttribute);
                    return;
                }
                case LogicSymbols.AND: {
                    var p = new AtomicSentence((LexValue)rhs[0].SyntheticAttribute);
                    lhs.SyntheticAttribute = new ComplexSentence(p, LogicSymbols.AND, (Sentence)rhs[2].SyntheticAttribute);
                    return;
                }
                default:
                    throw new Exception($"Error: {rhs[1].SyntheticAttribute} operator not found!");
            }
        });

        rule07.SetSemanticAction((lhs, rhs) => {
            switch (((LexValue)rhs[3].SyntheticAttribute).AsLogicSymbol()) {
                case LogicSymbols.OR:
                    lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, LogicSymbols.OR, (Sentence)rhs[4].SyntheticAttribute);
                    return;
                case LogicSymbols.AND:
                    lhs.SyntheticAttribute = new ComplexSentence((Sentence)rhs[1].SyntheticAttribute, LogicSymbols.AND, (Sentence)rhs[4].SyntheticAttribute);
                    return;
                default:
                    throw new Exception($"Error: {rhs[3].SyntheticAttribute} operator not found!");
            }
        });

        rule08.SetSemanticAction((lhs, rhs) => lhs.SyntheticAttribute = new ComplexSentence(LogicSymbols.NOT, (Sentence)rhs[1].SyntheticAttribute));

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
            var second = (Sentence)rhs[1].SyntheticAttribute;
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

                Sentence[] a = new Sentence[function.Parameters.Length];
                for (var i = 0; i < function.Parameters.Length; i++) {
                    a[i] = (Sentence)function.Parameters[i];
                }

                return this.Int(a);
            }
            case "Mod": {
                return this.Mod((Sentence)function.Parameters[0]);
            }
            case "Simplify": {
                var result = this.Simplify((Sentence)function.Parameters[0]);
                //Console.WriteLine($"Simplify: {function.Parameters[0]} equals: {result}");
                return result;
            }
            case "Forget": {
                var result = this.Forget((Sentence)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                return result;
            }
            case "SkepForget": {
                var result = this.SkepForget((Sentence)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
                return result;
            }
            case "MyForget": {
                var result = this.MyForget((Sentence)function.Parameters[0], (AtomicSentence)function.Parameters[1]);
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

    public List<Interpretation> GenerateInterpretations(params Sentence[] sentences) {
        var interpretations = new List<Interpretation>();
        var cleanAtoms = GetAtoms(sentences);        
        var truthTable = GenerateTruthTable(cleanAtoms.Count);
        
        foreach (var truthValues in truthTable) {
            var interpretation = new Interpretation();
            var list = truthValues.ToArray();
            for (var i = 0; i < cleanAtoms.Count; i++) {
                interpretation.Add(cleanAtoms[i], list[i]);
            }

            if (!interpretations.Contains(interpretation)) {
                interpretations.Add(interpretation);
            }
        }

        return interpretations;

        List<AtomicSentence> GetAtoms(params Sentence[] sentences) {
            var reducedAtoms = new List<AtomicSentence>();
            var collectedAtoms = new List<AtomicSentence>();
            
            foreach (var s in sentences) {
                collectedAtoms.AddRange(s.GetAtoms());
            }
            
            foreach (var atom in collectedAtoms) {
                if (atom.Tautology || atom.Falsum) {
                    continue;
                }
                if(!reducedAtoms.Contains(atom)) reducedAtoms.Add(atom);
            }

            return reducedAtoms;
        }
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