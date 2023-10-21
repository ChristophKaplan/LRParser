namespace CNF;

public abstract class Symbol
{
    private readonly string _value;

    public Symbol(string value)
    {
        this._value = value;
    }

    public override string ToString()
    {
        return _value;
    }
}

public class Terminal : Symbol
{
    public Terminal(string value) : base(value)
    {
    }
}

public class NonTerminal : Symbol
{
    public NonTerminal(string value) : base(value)
    {
    }
}

public class ProductionRule
{
    private NonTerminal from;
    private Symbol[] to;

    public ProductionRule(NonTerminal from,params Symbol[] to)
    {
        this.from = from;
        this.to = to;
    }

    public ProductionRule(string from,params string[] to)
    {
        this.from = new NonTerminal(from);

        var a = new Symbol[to.Length];
        for (var i = 0; i < to.Length; i++)
        {
            a[i] = new Terminal(to[i]);
        }

        this.to = a;
    }

    
    public override string ToString()
    {
        var toAll = to.Aggregate("", (current, next) => $"{current} {next}|");
        return $"{from} -> {toAll}";
    }
}

public class ContextFreeGrammar
{
    private List<NonTerminal> _nonTerminals;
    private List<Terminal> _terminals;
    private List<ProductionRule> _productionRules;
    private NonTerminal _startSymbol;
    
    public ContextFreeGrammar(List<NonTerminal> N, List<Terminal> Sigma, List<ProductionRule> P, NonTerminal S)
    {
        _nonTerminals = N;
        _terminals = Sigma;
        _productionRules = P;
        _startSymbol = S;
    }

    public override string ToString()
    {
        var n = _nonTerminals.Aggregate("", (current, next) => $"{current} {next},");
        var sigma = _terminals.Aggregate("", (current, next) => $"{current} {next},");
        var p = _productionRules.Aggregate("", (current, next) => $"{current} {next},");
        var s = _startSymbol;

        return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
    }
}