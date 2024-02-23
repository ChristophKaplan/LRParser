namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions {
    public static List<Symbol> First<T, N>(this ContextFreeGrammar<T, N> cnf, Symbol Symbol, List<Symbol> alreadyChecked = null)
        where T : Enum where N : Enum {
        var result = new List<Symbol>();

        if (Symbol.Type == SymbolType.Terminal) {
            result.Add(Symbol);
            return result;
        }

        alreadyChecked ??= new List<Symbol>();
        if (alreadyChecked.Contains(Symbol)) {
            //Console.WriteLine("recursion: " + nonTerminal);
            return result;
        }

        alreadyChecked.Add(Symbol);


        var allProdForNonTerminal = cnf.GetAllProdForNonTerminal(Symbol);
        var directorSet = new List<Symbol>[allProdForNonTerminal.Count];

        for (var i = 0; i < allProdForNonTerminal.Count; i++) {
            directorSet[i] = new List<Symbol>();

            if (allProdForNonTerminal[i].Conclusion[0].IsEpsilon) {
                directorSet[i].Add(Symbol.Epsilon);
            }
            else {
                var length = allProdForNonTerminal[i].Conclusion.Length;

                //first symbol no eps
                var first = First(cnf, allProdForNonTerminal[i].Conclusion[0], alreadyChecked);
                AddRangeLikeSet(first, directorSet[i]);
                directorSet[i].Remove(Symbol.Epsilon);

                if (length == 1) {
                    AddRangeLikeSet(directorSet[i], result);
                    continue; //first entry already checked
                }

                //mid symbols,if has eps check next
                int j;
                for (j = 1; First(cnf, allProdForNonTerminal[i].Conclusion[j], alreadyChecked).Contains(Symbol.Epsilon) && j < length; j++) {
                    AddRangeLikeSet(First(cnf, allProdForNonTerminal[i].Conclusion[j], alreadyChecked), directorSet[i]);
                    directorSet[i].Remove(Symbol.Epsilon);
                }

                //last symbol if mid had no epsilon
                if (j == length && First(cnf, allProdForNonTerminal[i].Conclusion[length], alreadyChecked).Contains(Symbol.Epsilon)) {
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