using LRParser.CFG;
using LRParser.Parser;

List<NonTerminal> N = new() { new NonTerminal("A"), new NonTerminal("B") };

List<Terminal> Sigma = new() { new Terminal("x"), new Terminal("y"), new Terminal("z") };

List<ProductionRule> P = new() {
    new ProductionRule(new NonTerminal("A"), new NonTerminal("B"), new Terminal("x")),
    new ProductionRule(new NonTerminal("B"), new Terminal("y"), new NonTerminal("B")),
    new ProductionRule(new NonTerminal("B"), new Terminal("z"))
};

var S = new NonTerminal("A");

var cnf = new ContextFreeGrammar(N, Sigma, P, S);
Console.WriteLine(cnf);

var tokens = new List<Terminal>() { new("y"), new("y"), new("z"), new("x") };

var parser = new Parser(cnf);
parser.Parse(tokens);