namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions{
    
    public static List<Symbol<Enum>> First<T,N> (this ContextFreeGrammar<T,N> cnf, Symbol<Enum> Symbol, List<Symbol<Enum>> alreadyChecked = null) where T : Enum where N : Enum {
        var result = new List<Symbol<Enum>>();

        if (Symbol is not Symbol<N> nonTerminal) {
            result.Add(Symbol);
            return result;
        }

        alreadyChecked ??= new List<Symbol<Enum>>();
        if (alreadyChecked.Contains(Symbol)) {
            //Console.WriteLine("recursion: " + nonTerminal);
            return result;
        }
        alreadyChecked.Add(Symbol);

        
        var allProdForNonTerminal = cnf.GetAllProdForNonTerminal(nonTerminal);
        var directorSet = new List<Symbol<Enum>>[allProdForNonTerminal.Count];

        for (var i = 0; i < allProdForNonTerminal.Count; i++) {
            directorSet[i] = new List<Symbol<Enum>>();

            if (allProdForNonTerminal[i].Conclusion[0].IsEpsilon) {
                directorSet[i].Add(Symbol<Enum>.Epsilon);
            }
            else {
                var length = allProdForNonTerminal[i].Conclusion.Length;

                //first symbol no eps
                var first = First(cnf, allProdForNonTerminal[i].Conclusion[0], alreadyChecked);
                AddRangeLikeSet(first, directorSet[i]);
                directorSet[i].Remove(Symbol<Enum>.Epsilon);

                if (length == 1) {
                    AddRangeLikeSet(directorSet[i], result);
                    continue; //first entry already checked
                }

                //mid symbols,if has eps check next
                int j;
                for (j = 1; First(cnf, allProdForNonTerminal[i].Conclusion[j], alreadyChecked).Contains(Symbol<Enum>.Epsilon) && j < length; j++) {
                    AddRangeLikeSet(First(cnf, allProdForNonTerminal[i].Conclusion[j], alreadyChecked), directorSet[i]);
                    directorSet[i].Remove(Symbol<Enum>.Epsilon);
                }

                //last symbol if mid had no epsilon
                if (j == length && First(cnf, allProdForNonTerminal[i].Conclusion[length], alreadyChecked).Contains(Symbol<Enum>.Epsilon)) {
                    directorSet[i].Add(Symbol<Enum>.Epsilon);
                }
            }

            AddRangeLikeSet(directorSet[i], result);
        }

        return result;
    }

    private static bool AddRangeLikeSet(List<Symbol<Enum>> from, List<Symbol<Enum>> to){
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