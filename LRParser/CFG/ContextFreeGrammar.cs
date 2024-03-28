namespace LRParser.CFG;

public class ContextFreeGrammar<T, N>  where T : Enum where N : Enum {
    private readonly List<Symbol> _nonTerminals = new();
    public readonly List<Production> Productions = new();
    private readonly List<Symbol> _terminals = new();
    public readonly Symbol StartSymbol = new (SpecialNonTerminal.Start, SymbolType.NonTerminal);

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
        var rule = new Production(EnumToSym(premise), conclusions.Select(conclusion => EnumToSym(conclusion)).ToArray());
        Productions.Add(rule);
        return rule;
    }

    private Symbol EnumToSym(Enum symbol) {
        if (symbol.GetType() == typeof(T)) {
            return new Symbol((T)symbol, SymbolType.Terminal);
        }

        if (symbol.GetType() == typeof(N)) {
            return new Symbol((N)symbol, SymbolType.NonTerminal);
        }
        
        if (symbol.GetType() == typeof(SpecialTerminal)) {
            return new Symbol((SpecialTerminal)symbol, SymbolType.Terminal);
        }
        
        if (symbol.GetType() == typeof(SpecialNonTerminal)) {
            return new Symbol((SpecialNonTerminal)symbol, SymbolType.NonTerminal);
        }

        throw new Exception($"enum type {symbol.GetType()} not found");
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