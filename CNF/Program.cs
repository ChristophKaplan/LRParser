using CNF;


List<NonTerminal> N = new() {
    new NonTerminal("stmt"),
    new NonTerminal("cond"),
    new NonTerminal("assignment"),
    new NonTerminal("loop"),
    new NonTerminal("expr"),
    new NonTerminal("boolexpr"),
    new NonTerminal("numexpr"),
};

List<Terminal> Sigma = new() {
    new Terminal("if"),
    new Terminal("then"),
    new Terminal("else"),
    new Terminal("fi"),
    new Terminal("id"),
    new Terminal("cop"),
    new Terminal("const"),
    new Terminal("while"),
    new Terminal("do"),
    new Terminal("od"),
    new Terminal("("),
    new Terminal(")"),
    new Terminal(":="),
    new Terminal("*"),
    new Terminal("+"),
};

List<ProductionRule> P = new() {
    new ProductionRule(new NonTerminal("stmt"), new NonTerminal("assignment")),
    new ProductionRule(new NonTerminal("stmt"), new NonTerminal("cond")),
    new ProductionRule(new NonTerminal("stmt"), new NonTerminal("loop")),
    new ProductionRule(new NonTerminal("assignment"), new Terminal("id"), new Terminal(":="), new NonTerminal("expr")),
    new ProductionRule(new NonTerminal("cond"), new Terminal("if"), new NonTerminal("boolexpr"), new Terminal("then"), new NonTerminal("stmt"), new NonTerminal("condrest")),
    new ProductionRule(new NonTerminal("condrest"), new Terminal("fi")),
    new ProductionRule(new NonTerminal("condrest"), new Terminal("else"), new NonTerminal("stmt"), new Terminal("fi")),
    new ProductionRule(new NonTerminal("loop"), new Terminal("while"), new NonTerminal("boolexpr"), new Terminal("do"), new NonTerminal("stmt"), new Terminal("od")),
    new ProductionRule(new NonTerminal("expr"), new NonTerminal("boolexpr")),
    new ProductionRule(new NonTerminal("expr"), new NonTerminal("numexpr")),
    new ProductionRule(new NonTerminal("boolexpr"), new NonTerminal("numexpr"), new Terminal("cop"), new NonTerminal("numexpr")),
    new ProductionRule(new NonTerminal("numexpr"), new NonTerminal("numexpr"), new Terminal("+"), new NonTerminal("term")),
    new ProductionRule(new NonTerminal("numexpr"), new NonTerminal("term")),
    new ProductionRule(new NonTerminal("term"), new NonTerminal("term"), new Terminal("*"), new NonTerminal("factor")),
    new ProductionRule(new NonTerminal("term"), new NonTerminal("factor")),
    new ProductionRule(new NonTerminal("factor"), new Terminal("id")),
    new ProductionRule(new NonTerminal("factor"), new Terminal("const")),
    new ProductionRule(new NonTerminal("factor"), new Terminal("("), new NonTerminal("numexpr"), new Terminal(")"))
};

NonTerminal S = new NonTerminal("stmt");

ContextFreeGrammar cnf = new ContextFreeGrammar(N, Sigma, P, S);
Console.WriteLine(cnf);

var tokens = new List<Terminal>() {
    new("if"),
    new("id"),
    new("cop"),
    new("const"),
    new("then"),
    new("id"),
    new(":="),
    new("const"),
    new("fi"),
};

//Console.WriteLine(cnf.GetGraph(tokens));

//Console.WriteLine(cnf.START(new NonTerminal("wort"), 1));

Console.WriteLine(cnf.FIRST(new NonTerminal("stmt")).Aggregate("FIRST (stmt):", (c, n) => $"{c} {n},"));
Console.WriteLine(cnf.FOLLOW(new NonTerminal("stmt")).Aggregate("FOLLOW (stmt):", (c, n) => $"{c} {n},"));