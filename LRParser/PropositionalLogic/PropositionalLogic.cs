using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

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
    StartSymbol, SecondStart, Sentence,ComplexSentence, Ext
}

public class PropositionalLogic: ContextFreeGrammar<Terminal,NonTerminal> {
    private readonly Lexer<Terminal> _lexer;
    private readonly Parser<Terminal,NonTerminal> _parser;

    private readonly List<AtomicSentence> _universe = new();
    private readonly List<Interpretation> _interpretations = new();

    public PropositionalLogic() {
    
        _lexer = new Lexer<Terminal>(
            new(Terminal.Function, "Mod|Forget"),
            new(Terminal.Open, "\\("),
            new(Terminal.Comma, ","),
            new(Terminal.Close, "\\)"),
            new(Terminal.Connective, "AND|OR"),
            new(Terminal.Negation, "NOT|!"),
            new(Terminal.AtomicSentence, "[A-Z][a-z]*")
            );
        
        SetUpGrammar();
        _parser = new Parser<Terminal,NonTerminal>(this);
    }

    public void EvaluateTruthTable(Sentence sentence) {
        GenerateInterpretations();

        Console.WriteLine($"Interpretations for {sentence}");
        foreach (var interpretation in _interpretations) {
            Console.WriteLine($"{interpretation}, {sentence} = {interpretation.Evaluate(sentence)}");
        }
    }

    public object ExecuteFunction(Function function) {
        if (function.Func.Equals("Mod")) {
            
            GenerateInterpretations();
            
            Console.WriteLine($"Models for {function.Sentence}");
            
            foreach (var interpretation in _interpretations) {
                var t = interpretation.Evaluate(function.Sentence);
                if (t) Console.WriteLine($"{interpretation}");
            }
        }

        if (function.Func.Equals("Forget")) {
            Console.WriteLine($"Forget {function.Parameters[0]} in {function.Sentence}");
            Sentence result = function.Sentence;
            return result;
        }
        
        return null;
    }

    private void AddToUniverse(AtomicSentence atomicSentence) {
        if (!_universe.Contains(atomicSentence)) _universe.Add(atomicSentence);
    }

    public void GenerateInterpretations() {
        var truthTable = GenerateTruthTable(_universe.Count);

        foreach (var truthValues in truthTable) {
            var interpretation = new Interpretation();
            var list = truthValues.ToArray();
            for (int i = 0; i < _universe.Count; i++) {
                interpretation.Add(_universe[i], list[i]);
            }

            _interpretations.Add(interpretation);
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

    private void SetUpGrammar() {
        AddByEnumType(typeof(Terminal));
        AddByEnumType(typeof(NonTerminal));
        
        AddProductionRule(NonTerminal.StartSymbol, NonTerminal.SecondStart);
        AddProductionRule(NonTerminal.SecondStart, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.Sentence, Terminal.AtomicSentence);
        AddProductionRule(NonTerminal.Sentence, NonTerminal.ComplexSentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.AtomicSentence, Terminal.Connective, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.ComplexSentence, Terminal.Negation, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.Sentence, NonTerminal.Ext,Terminal.Close);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
        AddProductionRule(NonTerminal.Ext, Terminal.Comma, NonTerminal.Sentence);
        AddProductionRule(NonTerminal.SecondStart, Terminal.Function, Terminal.Open, NonTerminal.SecondStart, Terminal.Close);
        
        AddStartSymbol(NonTerminal.StartSymbol);
        
        AddSemanticAction(0, input => input[0]);
        AddSemanticAction(1, input => input[0]);
        
        AddSemanticAction(2, input => {
            var p = new AtomicSentence((string)input[0]);
            AddToUniverse(p);
            return p;
        });

        AddSemanticAction(3, input => input[0]);
        AddSemanticAction(4, input => {
            if ((string)input[1] == "OR") {
                var p = new AtomicSentence((string)input[0]);
                AddToUniverse(p);
                return new ComplexSentence(p, "OR", (Sentence)input[2]);
            }

            if ((string)input[1] == "AND") {
                var p = new AtomicSentence((string)input[0]);
                AddToUniverse(p);
                return new ComplexSentence(p, "AND", (Sentence)input[2]);
            }

            throw new Exception("Error:");
        });
        
        AddSemanticAction(5, input => new ComplexSentence("NOT", (Sentence)input[1]));

        AddSemanticAction(6, input => {
            var func = (string)input[0];
            var sentence = (Sentence)input[2];
            var parameters = (Sentence)input[3];
            return new Function(func, sentence, parameters);
        });
        
        AddSemanticAction(7, input => input[0]);
        AddSemanticAction(8, input => input[1]);
        
        AddSemanticAction(9, input => {
                var func = (string)input[0];
                var f = (Function)input[2];
                var sentence = (Sentence)ExecuteFunction(f);
                return new Function(func, sentence);
            });
    }

    public IPropositionalLanguage TryParse(string input) {
        List<Symbol> tokens = _lexer.Tokenize(input);
        var tree = _parser.Parse(tokens);
        tree.Evaluate();
        return (IPropositionalLanguage)tree.Symbol.Attribut1;
    }
}