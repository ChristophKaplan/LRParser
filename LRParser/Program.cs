using LRParser.CFG;
using LRParser.Parser;

List<NonTerminal> N = new()
{
    new NonTerminal("S"), new NonTerminal("Sentence"), new NonTerminal("AtomicSentence"),
    new NonTerminal("ComplexSentence")
};

List<Terminal> Sigma = new() { new Terminal("P"), new Terminal("Q"), new Terminal("&&"),new Terminal() };

List<ProductionRule> P = new()
{
    new ProductionRule(new NonTerminal("S"), new NonTerminal("Sentence")),
    new ProductionRule(new NonTerminal("Sentence"), new NonTerminal("AtomicSentence")),
    new ProductionRule(new NonTerminal("Sentence"), new NonTerminal("ComplexSentence")),
    new ProductionRule(new NonTerminal("AtomicSentence"), new Terminal("P")),
    new ProductionRule(new NonTerminal("AtomicSentence"), new Terminal("Q")),
    new ProductionRule(new NonTerminal("ComplexSentence"),new NonTerminal("AtomicSentence"),new Terminal("&&"), new NonTerminal("Sentence")),
};

var S = new NonTerminal("S");

var cnf = new ContextFreeGrammar(N, Sigma, P, S);
Console.WriteLine(cnf);

var tokens = new List<Terminal>() { new("P"), new("&&"), new("Q")};

var parser = new Parser(cnf);
var tree = parser.Parse(tokens);

Console.WriteLine("tree: "+tree);