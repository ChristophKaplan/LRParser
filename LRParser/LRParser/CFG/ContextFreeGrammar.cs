namespace LRParser.CFG;

public class ContextFreeGrammar<T, N>  where T : Enum where N : Enum {
    private readonly List<Symbol> NonTerminals = new();
    public readonly List<Production> Productions = new();
    private readonly List<Symbol> Terminals = new();
    public Symbol StartSymbol;
    
    public void AddStartSymbol(N startSymbol) {
        StartSymbol = new Symbol(startSymbol, SymbolType.NonTerminal);
    }

    public void AddTerminal(T terminal) {
        Terminals.Add(new Symbol(terminal, SymbolType.Terminal));
    }

    public void AddNonTerminal(N nonTerminal) {
        NonTerminals.Add(new Symbol(nonTerminal, SymbolType.NonTerminal));
    }

    public void AddByEnumType(Type enumType) {
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

    public void AddProductionRule(Enum premise, params Enum[] conclusions) {
        var a = EnumToSym(premise);
        if (a == null) {
            Console.WriteLine("Premise is null");
        }

        var rule = new Production(a, conclusions.Select(conclusion => EnumToSym(conclusion)).ToArray());

        if (rule.Premise == null) {
            Console.WriteLine("Premise is null");
        }

        Productions.Add(rule);
    }

    public void AddSemanticAction(int ruleId, Action<Symbol,Symbol[]> semanticAction) {
        Productions[ruleId].SetSemanticAction(semanticAction);
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
        var n = NonTerminals.Aggregate("", (current, next) => $"{current} {next},");
        var sigma = Terminals.Aggregate("", (current, next) => $"{current} {next},");
        var p = Productions.Aggregate("", (current, next) => $"{current} {next},");
        var s = StartSymbol;

        return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
    }
}