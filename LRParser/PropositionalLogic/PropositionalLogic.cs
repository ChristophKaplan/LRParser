using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

namespace PropositionalLogic;

public class PropositionalLogic {
    private Lexer _lexer;
    private Parser _parser;

    private readonly List<AtomicSentence> _universe = new ();
    private readonly List<Interpretation> _interpretations = new ();
    
    public PropositionalLogic() {
        _lexer = new Lexer(new ("Function", "Mod"),new ("(", "\\("),new (")", "\\)"),new ("Connective", "AND|OR"), new ("Negation", "NOT|!"), new ("AtomicSentence", "[A-Z][a-z]*"));
        var cfg = SetUpGrammar();
        _parser = new Parser(cfg);
    }

    public void EvaluateTruthTable(Sentence sentence) {
        Console.WriteLine($"Interpretations for {sentence}");
        foreach (var interpretation in _interpretations) {
            Console.WriteLine($"{interpretation}, {sentence} = {interpretation.Evaluate(sentence)}");
        }
    }
    
    private void AddToUniverse(AtomicSentence atomicSentence) {
        if(!_universe.Contains(atomicSentence)) _universe.Add(atomicSentence);
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

    private IEnumerable<IEnumerable<bool>> GenerateTruthTable(int n)
    {
        switch (n)
        {
            case 0:
                return Enumerable.Empty<IEnumerable<bool>>();
            case 1:
                return new List<List<bool>>
                {
                    new List<bool> { true },
                    new List<bool> { false }
                };
            default:
            {
                var subTables = GenerateTruthTable(n - 1);
                return subTables.SelectMany(row => new[]
                {
                    row.Append(true),
                    row.Append(false)
                });
            }
        }
    }
    
    private ContextFreeGrammar SetUpGrammar() {
        
        List<NonTerminal> nonTerminals = new() {new ("S'"), new ("S"), new ("Sentence"), new ("ComplexSentence")};
        List<Terminal> terminals = new() { new ("AtomicSentence"), new ("Connective"),new ("Negation"), new ("Function"), new ("("), new (")"), new () };

        List<ProductionRule> productionRules = new() {
            new ProductionRule(new NonTerminal("S'"), new NonTerminal("S")),
            new ProductionRule(new NonTerminal("S"), new NonTerminal("Sentence")),
            
            new ProductionRule(new NonTerminal("Sentence"), new Terminal("AtomicSentence")),
            new ProductionRule(new NonTerminal("Sentence"), new NonTerminal("ComplexSentence")),
            new ProductionRule(new NonTerminal("ComplexSentence"), new NonTerminal("AtomicSentence"), new Terminal("Connective"), new NonTerminal("Sentence")),
            new ProductionRule(new NonTerminal("ComplexSentence"), new Terminal("Negation"), new NonTerminal("Sentence")),

            new ProductionRule(new NonTerminal("S"), new Terminal("Function"), new Terminal("("), new NonTerminal("Sentence"), new Terminal(")"))
        };
        
        
        productionRules[0].SetSemanticAction(input => input[0]);
        productionRules[1].SetSemanticAction(input => input[0]);
        productionRules[2].SetSemanticAction(input =>
        {
            var p = new AtomicSentence((string)input[0]);
            AddToUniverse(p);
            return p;
        });
        productionRules[3].SetSemanticAction(input => input[0]);
        productionRules[4].SetSemanticAction(input => {
            if ((string) input[1] == "OR")
            {
                var p = new AtomicSentence((string)input[0]);
                AddToUniverse(p);
                return new ComplexSentence(p, "OR" , (Sentence)input[2]);
            }
    
            if ((string) input[1] == "AND") {
                var p = new AtomicSentence((string)input[0]);
                AddToUniverse(p);
                return new ComplexSentence(p, "AND" , (Sentence)input[2]);
            }
    
            throw new Exception("Error:");
        });
        
        productionRules[5].SetSemanticAction(input => new ComplexSentence("NOT" , (Sentence)input[1]));
        
        productionRules[6].SetSemanticAction(input =>
        {
            var func = (string)input[0];
            var sentence = (Sentence)input[2];
            Console.WriteLine($"Function: {func}, Sentence: {sentence}");
            return "mod";
        });
        
        var cfg = new ContextFreeGrammar(nonTerminals, terminals, productionRules, new NonTerminal("S"));
        return cfg;
    }

    public Sentence TryParse(string input) {
        var tokens = _lexer.Tokenize(input);
        var tree = _parser.Parse(tokens);
        tree.Evaluate();

        if (tree.Symbol._attribut1 is Sentence sentence) return sentence;
        
        Console.WriteLine(tree);
        throw new Exception("Error parsing:" + tree.Symbol._attribut1);
    }
}