namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions {
    
    public static List<Symbol> First<T, N>(this ContextFreeGrammar<T, N> cfg, Symbol symbol, List<Symbol> alreadyChecked) where T : Enum where N : Enum {
        var result = new List<Symbol>();

        if (symbol.Type == SymbolType.Terminal) {
            result.Add(symbol);
            return result;
        }

        if (alreadyChecked.Contains(symbol)) {
            //Console.WriteLine("recursion: " + symbol);
            return result;
        }

        alreadyChecked.Add(symbol);

        var allProdForNonTerminal = cfg.GetAllProdForNonTerminal(symbol);
        var directorSet = new List<Symbol>[allProdForNonTerminal.Count];

        for (var i = 0; i < allProdForNonTerminal.Count; i++) {
            directorSet[i] = new List<Symbol>();

            if (allProdForNonTerminal[i].Conclusion[0].IsEpsilon) {
                directorSet[i].Add(Symbol.Epsilon);
            }
            else {
                var length = allProdForNonTerminal[i].Conclusion.Length;
                var j = 0;
                
                //add first set of forst symbol, remove eps
                var firstSet = First(cfg, allProdForNonTerminal[i].Conclusion[j], new List<Symbol>(alreadyChecked));
                AddRangeNoDuplicates(firstSet, directorSet[i]);
                directorSet[i].Remove(Symbol.Epsilon);

                //add first set of following symbols IFF eps is contained
                for (j = 1; j < length; j++)
                {
                    var nextFirstSet = First(cfg, allProdForNonTerminal[i].Conclusion[j], new List<Symbol>(alreadyChecked));
                    if (!nextFirstSet.Contains(Symbol.Epsilon)) continue;
                    AddRangeNoDuplicates(nextFirstSet, directorSet[i]);
                    directorSet[i].Remove(Symbol.Epsilon);
                }

                //if first set of last symbol containes epsilon, add only epsilon
                if (j == length-1 && First(cfg, allProdForNonTerminal[i].Conclusion[j], new List<Symbol>(alreadyChecked)).Contains(Symbol.Epsilon)) {
                    directorSet[i].Add(Symbol.Epsilon);
                }
            }

            AddRangeNoDuplicates(directorSet[i], result);
        }

        return result;
    }

    private static void AddRangeNoDuplicates(List<Symbol> from, List<Symbol> to)
    {
        foreach (var notContained in from.Where(fromSymbol => !to.Contains(fromSymbol)))
        {
            to.Add(notContained);
        }
    }
}