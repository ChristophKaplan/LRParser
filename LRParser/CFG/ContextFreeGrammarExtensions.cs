namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions {
    public static List<Symbol> First(this ContextFreeGrammar cnf, Symbol Symbol, List<NonTerminal> alreadyChecked = null) {
        var result = new List<Symbol>();

        if (Symbol is not NonTerminal nonTerminal) {
            result.Add(Symbol);
            return result;
        }

        alreadyChecked ??= new List<NonTerminal>();
        if (alreadyChecked.Contains(Symbol)) {
            //Console.WriteLine("recursion: " + nonTerminal);
            return result;
        }

        alreadyChecked.Add(nonTerminal);

        var allProdForNonTerminal = cnf.GetAllProdForNonTerminal(nonTerminal);
        var directorSet = new List<Symbol>[allProdForNonTerminal.Count];

        for (var i = 0; i < allProdForNonTerminal.Count; i++) {
            directorSet[i] = new List<Symbol>();

            if (allProdForNonTerminal[i].Conclusion[0].Equals(new Terminal())) {
                directorSet[i].Add(new Terminal());
            }
            else {
                var length = allProdForNonTerminal[i].Conclusion.Length;

                //first symbol no eps
                var first = First(cnf, allProdForNonTerminal[i].Conclusion[0], alreadyChecked);
                AddRangeLikeSet(first, directorSet[i]);
                directorSet[i].Remove(new Terminal());

                if (length == 1) {
                    AddRangeLikeSet(directorSet[i], result);
                    continue; //first entry already checked
                }

                //mid symbols,if has eps check next
                int j;
                for (j = 1; First(cnf, allProdForNonTerminal[i].Conclusion[j], alreadyChecked).Contains(new Terminal()) && j < length; j++) {
                    AddRangeLikeSet(First(cnf, allProdForNonTerminal[i].Conclusion[j], alreadyChecked), directorSet[i]);
                    directorSet[i].Remove(new Terminal());
                }

                //last symbol if mid had no epsilon
                if (j == length && First(cnf, allProdForNonTerminal[i].Conclusion[length], alreadyChecked).Contains(new Terminal())) {
                    directorSet[i].Add(new Terminal());
                }
            }

            AddRangeLikeSet(directorSet[i], result);
        }

        return result;
    }

    public static List<Symbol> Follow(this ContextFreeGrammar cnf, NonTerminal S) {
        var followSets = cnf.NonTerminals.ToDictionary(nonTerminal => nonTerminal, nonTerminal => new List<Symbol> { new Terminal("$") });

        while (true) {
            var changed = false;

            foreach (var prod in cnf.ProductionRules) {
                if (!prod.Conclusion.Contains(S)) {
                    continue;
                }

                if (!prod.ContainsNonTerminalConclusion()) {
                    continue;
                }

                var posOfLastNonTerminal = prod.GetMostRightPosOf(S);


                var lastNonTerminal = (NonTerminal)prod.Conclusion[posOfLastNonTerminal];
                if (!followSets.TryGetValue(lastNonTerminal, out var curFollowSet)) {
                    throw new Exception($"cant find value: {lastNonTerminal} in followSets");
                }

                if (posOfLastNonTerminal == prod.Conclusion.Length - 1 || prod.Conclusion[posOfLastNonTerminal + 1].IsEpsilon) {
                    var f = Follow(cnf, prod.Premise);
                    changed = AddRangeLikeSet(f, curFollowSet);

                    if (!changed) {
                        break;
                    }

                    continue;
                }

                //we have beta
                var first_beta = First(cnf, prod.Conclusion[posOfLastNonTerminal + 1]);

                if (first_beta.Contains(new Terminal())) {
                    var f = Follow(cnf, prod.Premise);
                    changed = AddRangeLikeSet(f, curFollowSet);
                }

                var first_beta_withoutEps = new List<Symbol>(first_beta);
                foreach (var s in first_beta_withoutEps.Where(s => s.IsEpsilon)) {
                    first_beta_withoutEps.Remove(s);
                }

                changed = AddRangeLikeSet(first_beta_withoutEps, curFollowSet);
            }

            if (!changed) {
                break;
            }
        }

        return followSets[S];
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