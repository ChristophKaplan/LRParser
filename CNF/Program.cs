using CNF;


List<NonTerminal> N = new()
{
    new NonTerminal("A"),    
};

List<Terminal> Sigma = new()
{
    new Terminal("a"),    
};

List<ProductionRule> P = new()
{
    new ProductionRule("A", "a", "b"),    
};

NonTerminal S = new NonTerminal("A");

ContextFreeGrammar cnf = new ContextFreeGrammar(N,Sigma,P,S);

Console.WriteLine(cnf);