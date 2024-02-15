using LRParser;
using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

List<NonTerminal> nonTerminals = new() {
    new NonTerminal("S"), new NonTerminal("Sentence"), new NonTerminal("ComplexSentence")
};

List<Terminal> terminals = new() { new Terminal("AtomicSentence"), new Terminal("Connective"),new Terminal("Negation"), new Terminal() };

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
    
    throw new Exception("problem");
});

productionRules[4].SetSemanticAction(input=> {
    return !(bool)input[1];
});

var cfg = new ContextFreeGrammar(nonTerminals, terminals, productionRules, new NonTerminal("S"));
Console.WriteLine(cfg);

var lexer = new Lexer(new TokenDefinition("Connective", "AND|OR"), new TokenDefinition("Negation", "NOT|!"), new TokenDefinition("AtomicSentence", "[A-Z][a-z]*"));
var tokens = lexer.Tokenize("Premise OR Q");
tokens.ForEach(Console.WriteLine);

var parser = new Parser(cfg);
var tree = parser.Parse(tokens);
tree.Eval();

Sentence sentence = (Sentence) tree.Data._attribut1;

sentence.PreOrderReverse(sentence => {
    Console.WriteLine(sentence);
});