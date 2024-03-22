namespace LRParser.CFG;

public class ContextFreeGrammar<T, N>  where T : Enum where N : Enum {
    private readonly List<Symbol> _nonTerminals = new();
    public readonly List<Production> Productions = new();
    private readonly List<Symbol> _terminals = new();
    public Symbol StartSymbol;

    protected void AddStartSymbol(N startSymbol) {
        StartSymbol = new Symbol(startSymbol, SymbolType.NonTerminal);
    }

    private void AddTerminal(T terminal) {
        _terminals.Add(new Symbol(terminal, SymbolType.Terminal));
    }

    private void AddNonTerminal(N nonTerminal) {
        _nonTerminals.Add(new Symbol(nonTerminal, SymbolType.NonTerminal));
    }

    protected void AddByEnumType(Type enumType) {
        foreach (int i in Enum.GetValues(enumType)) {
            if (Enum.IsDefined(enumType, i)) {
                if (typeof(T).IsAssignableFrom(enumType)) {
                    AddTerminal((T)Enum.ToObject(enumType, i));
                }
                else if (typeof(N).IsAssignableFrom(enumType)) {
                    AddNonTerminal((N)Enum.ToObject(enumType, i));
                }
            }
        }
    }

    protected Production AddProductionRule(Enum premise, params Enum[] conclusions) {
        var prem = EnumToSym(premise);
        if (prem == null) {
            Console.WriteLine("Premise is null");
        }

        var rule = new Production(prem, conclusions.Select(conclusion => EnumToSym(conclusion)).ToArray());

        if (rule.Premise == null) {
            Console.WriteLine("Premise is null");
        }

        Productions.Add(rule);
        return rule;
    }

    private Symbol EnumToSym(Enum p) {
        if (GetType(p) == SymbolType.Terminal) {
            return new Symbol((T)p, SymbolType.Terminal);
        }

        if (GetType(p) == SymbolType.NonTerminal) {
            return new Symbol((N)p, SymbolType.NonTerminal);
        }

        throw new Exception($"type {GetType(p)} not found");
    }

    private SymbolType GetType(Enum symbol) {
        if (symbol.GetType() == typeof(T)) {
            return SymbolType.Terminal;
        }

        if (symbol.GetType() == typeof(N)) {
            return SymbolType.NonTerminal;
        }

        throw new Exception("type not found");
    }

    public List<Production> GetAllProdForNonTerminal(Symbol nonTerminal) {
        return Productions.Where(rule => rule.Premise.Equals(nonTerminal)).ToList();
    }

    public override string ToString() {
        var n = _nonTerminals.Aggregate("", (current, next) => $"{current} {next},");
        var sigma = _terminals.Aggregate("", (current, next) => $"{current} {next},");
        var p = Productions.Aggregate("", (current, next) => $"{current} {next},");
        var s = StartSymbol;

        return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
    }
}