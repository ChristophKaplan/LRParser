namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions {
    
    public static List<Symbol> First<T, N>(this ContextFreeGrammar<T, N> cfg, Symbol symbol, List<Symbol> alreadyChecked = null)
        where T : Enum where N : Enum {
        var result = new List<Symbol>();

        if (symbol.Type == SymbolType.Terminal) {
            result.Add(symbol);
            return result;
        }

        alreadyChecked ??= new List<Symbol>();
        if (alreadyChecked.Contains(symbol)) {
            //Console.WriteLine("recursion: " + nonTerminal);
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

                //first symbol no eps
                var first = First(cfg, allProdForNonTerminal[i].Conclusion[0], alreadyChecked);
                AddRangeLikeSet(first, directorSet[i]);
                directorSet[i].Remove(Symbol.Epsilon);

                if (length == 1) {
                    AddRangeLikeSet(directorSet[i], result);
                    continue; //first entry already checked
                }

                //mid symbols,if has eps check next
                int j;
                for (j = 1; First(cfg, allProdForNonTerminal[i].Conclusion[j], alreadyChecked).Contains(Symbol.Epsilon) && j < length; j++) {
                    AddRangeLikeSet(First(cfg, allProdForNonTerminal[i].Conclusion[j], alreadyChecked), directorSet[i]);
                    directorSet[i].Remove(Symbol.Epsilon);
                }

                //last symbol if mid had no epsilon
                if (j == length && First(cfg, allProdForNonTerminal[i].Conclusion[length], alreadyChecked).Contains(Symbol.Epsilon)) {
                    directorSet[i].Add(Symbol.Epsilon);
                }
            }

            AddRangeLikeSet(directorSet[i], result);
        }

        return result;
    }

    private static bool AddRangeLikeSet(List<Symbol> from, List<Symbol> to) {
        var changed = false;
        foreach (var s in from) {
            if (to.Contains(s)) {
                continue;
            }

            to.Add(s);
            changed = true;
        }

        return changed;
    }
}