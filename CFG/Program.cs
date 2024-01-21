using CFG;

/*
List<NonTerminal> N = new() {
    new NonTerminal("S'"),
    new NonTerminal("S"),
    new NonTerminal("E"),
};

List<Terminal> Sigma = new() {
    new Terminal("="),
    new Terminal("+"),
    new Terminal("id"),
};

List<ProductionRule> P = new() {
    new ProductionRule(new NonTerminal("S'"), new NonTerminal("S")),
    new ProductionRule(new NonTerminal("S"), new NonTerminal("E"), new Terminal("="), new NonTerminal("E")),
    new ProductionRule(new NonTerminal("S"), new Terminal("id")),
    new ProductionRule(new NonTerminal("E"), new NonTerminal("E"), new Terminal("+"), new Terminal("id")),
    new ProductionRule(new NonTerminal("E"), new Terminal("id")),
};

NonTerminal S = new NonTerminal("S'");

ContextFreeGrammar cnf = new ContextFreeGrammar(N, Sigma, P, S);
Console.WriteLine(cnf);

var tokens = new List<Terminal>() {

    new Terminal("id"),
    new Terminal("="),
    new Terminal("id"),
};

var parser = new Parser(cnf);
parser.Parse(tokens);

*/




List<NonTerminal> N = new() {
    new NonTerminal("A"),
    new NonTerminal("B")
};

List<Terminal> Sigma = new() {
    new Terminal("x"),
    new Terminal("y"),
    new Terminal("z"),
};

List<ProductionRule> P = new() {
    new ProductionRule(new NonTerminal("A"), new NonTerminal("B"),new Terminal("x")),
    new ProductionRule(new NonTerminal("B"),new Terminal("y"), new NonTerminal("B")),
    new ProductionRule(new NonTerminal("B"), new Terminal("z"))
};

NonTerminal S = new NonTerminal("A");

ContextFreeGrammar cnf = new ContextFreeGrammar(N, Sigma, P, S);
Console.WriteLine(cnf);

var tokens = new List<Terminal>() {
    new Terminal("y"),
    new Terminal("y"),
    new Terminal("z"),
    new Terminal("x"),
};

var parser = new LRParser(cnf);
parser.Parse(tokens);