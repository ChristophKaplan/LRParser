namespace LRParser.CFG;

public interface IContextFreeGrammar<T,N> where T: Enum where N: Enum {
    public void AddByEnumType(Type enumType);
    public void AddTerminal(T terminal);
    public void AddNonTerminal(N nonTerminal);
    public void AddStartSymbol(N startSymbol);
    public void AddProductionRule(Enum premise, params Enum[] conclusions);
    public void AddSemanticAction(int ruleId, Func<object[], object> semanticAction);
}

public class ContextFreeGrammar<T,N> : IContextFreeGrammar<T,N> where T: Enum where N: Enum{
    public readonly List<Symbol<T>> Terminals = new();
    public readonly List<Symbol<N>> NonTerminals = new();
    public readonly List<ProductionRule> ProductionRules = new();
    public Symbol<N> StartSymbol;

    
    public void AddStartSymbol(N startSymbol) {
        StartSymbol = new Symbol<N>(startSymbol, SymbolType.NonTerminal);
    }
    
    public void AddTerminal(T terminal) {
        Terminals.Add(new Symbol<T>(terminal, SymbolType.Terminal));
    }
    
    public void AddNonTerminal(N nonTerminal) {
        NonTerminals.Add(new Symbol<N>(nonTerminal, SymbolType.NonTerminal));
    }
    
    public void AddByEnumType(Type enumType) {
        foreach (int i in Enum.GetValues(enumType))
        {
            if (Enum.IsDefined(enumType, i))
            {
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
        if(a == null) {
            Console.WriteLine("Premise is null");
        }
        
        var rule = new ProductionRule(a, conclusions.Select(conclusion => EnumToSym(conclusion)).ToArray());
        
        if (rule.Premise == null) {
            Console.WriteLine("Premise is null");
        }
        
        ProductionRules.Add(rule);
    }

    private Symbol<Enum> EnumToSym(Enum p) {
        if (GetType(p) == SymbolType.Terminal) {
            var tSym = new Symbol<T>((T)p, SymbolType.Terminal);
            return (Symbol<Enum>)tSym;
        }
        if (GetType(p) == SymbolType.NonTerminal) {
            var nSym = new Symbol<N>((N)p, SymbolType.NonTerminal);
            return (Symbol<Enum>)nSym;
        }
        throw new Exception($"type {GetType(p)} not found");
    }

    private SymbolType GetType(Enum symbol) {
        if (symbol.GetType().Equals(typeof(T))) {
            return SymbolType.Terminal;
        }
        else if (symbol.GetType().Equals(typeof(N))) {
            return SymbolType.NonTerminal;
        }
        
        throw new Exception("type not found");
    }

    public void AddSemanticAction(int ruleId, Func<object[], object> semanticAction) {
        ProductionRules[ruleId].SetSemanticAction(semanticAction);
    }

    public List<ProductionRule> GetAllProdForNonTerminal(Symbol<N> nonTerminal) {
        return ProductionRules.Where(rule => rule.Premise.Equals(nonTerminal)).ToList();
    }

    public override string ToString() {
        var n = NonTerminals.Aggregate("", (current, next) => $"{current} {next},");
        var sigma = Terminals.Aggregate("", (current, next) => $"{current} {next},");
        var p = ProductionRules.Aggregate("", (current, next) => $"{current} {next},");
        var s = StartSymbol;

        return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
    }
}