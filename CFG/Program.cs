using System.Diagnostics.SymbolStore;
using CNF;


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

cnf.GenerateStates(new LRItem(P[0], 0, new List<Symbol>() { new Terminal("$") })); 

