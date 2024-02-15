using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

List<NonTerminal> nonTerminals = new() {
    new NonTerminal("S"), new NonTerminal("Sentence"), new NonTerminal("ComplexSentence")
};

List<Terminal> terminals = new() { new Terminal("AtomicSentence"), new Terminal("Connective"), new Terminal() };

List<ProductionRule> productionRules = new() {
    new ProductionRule(new NonTerminal("S"), new NonTerminal("Sentence")),
    new ProductionRule(new NonTerminal("Sentence"), new Terminal("AtomicSentence")),
    new ProductionRule(new NonTerminal("Sentence"), new NonTerminal("ComplexSentence")),
    new ProductionRule(new NonTerminal("ComplexSentence"), new NonTerminal("AtomicSentence"), new Terminal("Connective"), new NonTerminal("Sentence"))
};

var StartSymbol = new NonTerminal("S");

var cfg = new ContextFreeGrammar(nonTerminals, terminals, productionRules, StartSymbol);
Console.WriteLine(cfg);

var lexer = new Lexer(new TokenDefinition("Connective", "AND|OR"),new TokenDefinition("AtomicSentence", "[A-Z][a-z]*"));
var tokens = lexer.Tokenize("Premise OR Q");

var parser = new Parser(cfg);
var tree = parser.Parse(tokens);

tree.PreOrder(node => {
    if (node.Data is Terminal) {
        Console.WriteLine(node);
    }
});
