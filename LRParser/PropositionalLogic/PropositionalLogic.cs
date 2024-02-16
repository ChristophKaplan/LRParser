using LRParser.CFG;

namespace LRParser.PropositionalLogic;

public class PropositionalLogic {
    private Lexer.Lexer _lexer;
    private Parser.Parser _parser;
    
    public PropositionalLogic() {
        _lexer = new Lexer.Lexer(new ("Connective", "AND|OR"), new ("Negation", "NOT|!"), new ("AtomicSentence", "[A-Z][a-z]*"));
        var cfg = SetUpGrammar();
        _parser = new Parser.Parser(cfg);
    }

    private ContextFreeGrammar SetUpGrammar() {
        
        List<NonTerminal> nonTerminals = new() { new ("S"), new ("Sentence"), new ("ComplexSentence")};
        List<Terminal> terminals = new() { new ("AtomicSentence"), new ("Connective"),new ("Negation"), new () };

        List<ProductionRule> productionRules = new() {
            new ProductionRule(new NonTerminal("S"), new NonTerminal("Sentence")),
            new ProductionRule(new NonTerminal("Sentence"), new Terminal("AtomicSentence")),
            new ProductionRule(new NonTerminal("Sentence"), new NonTerminal("ComplexSentence")),
            new ProductionRule(new NonTerminal("ComplexSentence"), new NonTerminal("AtomicSentence"), new Terminal("Connective"), new NonTerminal("Sentence")),
            new ProductionRule(new NonTerminal("ComplexSentence"), new Terminal("Negation"), new NonTerminal("Sentence"))
        };

        productionRules[0].SetSemanticAction(input => input[0]);
        productionRules[1].SetSemanticAction(input => new AtomicSentence((string)input[0]));
        productionRules[2].SetSemanticAction(input => input[0]);
        productionRules[3].SetSemanticAction(input => {
            if ((string) input[1] == "OR") {
                return new ComplexSentence(new AtomicSentence((string)input[0]), "OR" , (Sentence)input[2]);
            }
    
            if ((string) input[1] == "AND") {
                return new ComplexSentence(new AtomicSentence((string)input[0]), "AND" , (Sentence)input[2]);
            }
    
            throw new Exception("Error:");
        });
        
        productionRules[4].SetSemanticAction(input => new ComplexSentence("NOT" , (Sentence)input[1]));

        var cfg = new ContextFreeGrammar(nonTerminals, terminals, productionRules, new NonTerminal("S"));
        return cfg;
    }

    public Sentence TryParse(string input) {
        var tokens = _lexer.Tokenize(input);
        var tree = _parser.Parse(tokens);
        tree.Evaluate();

        Sentence sentence = (Sentence) tree.Symbol._attribut1;
        return sentence;
    }
}

