namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions {
    public static List<Symbol> FIRST(this ContextFreeGrammar cnf, Symbol S) {
        var result = new List<Symbol>();

        if (S is not NonTerminal A) {
            result.Add(S);
            return result;
        }

        var p = cnf.GetAllProdForNonTerminal(A);
        var n = p.Count;
        var D = new List<Symbol>[n];

        for (var i = 0; i < n; i++) {
            D[i] = new List<Symbol>();

            var alpha_i = p[i].to[0];
            if (alpha_i.Equals(new Terminal())) {
                D[i].Add(new Terminal());
            }
            else {
                var m = p[i].to.Length;

                D[i].AddRange(FIRST(cnf, p[i].to[0]));
                D[i].Remove(new Terminal());
                var j = 0;
                while (FIRST(cnf, p[i].to[j]).Contains(new Terminal()) && j < m) {
                    j++;
                    D[i].AddRange(FIRST(cnf, p[i].to[j]));
                    D[i].Remove(new Terminal());
                }

                if (j == m && FIRST(cnf, p[i].to[m]).Contains(new Terminal())) {
                    D[i].Add(new Terminal());
                }
            }

            result.AddRange(D[i]);
        }

        return result;
    }

    public static List<Symbol> FOLLOW(this ContextFreeGrammar cnf, NonTerminal S) {
        Dictionary<NonTerminal, List<Symbol>> followSets = new();
        foreach (var nonTerminal in cnf.NonTerminals) {
            followSets.Add(nonTerminal, new List<Symbol>() { new Terminal("$") });
        }

        while (true) {
            var changed = false;

            foreach (var prod in cnf.ProductionRules) {
                if (!prod.to.Contains(S)) {
                    continue;
                }

                if (!prod.ContainsNonTerminalConclusion()) {
                    continue;
                }

                var posOfLastNonTerminal = prod.GetMostRightPosOf(S);


                var lastNonTerminal = (NonTerminal)prod.to[posOfLastNonTerminal];
                if (!followSets.TryGetValue(lastNonTerminal, out var curFollowSet)) {
                    throw new Exception($"cant find value: {lastNonTerminal} in followSets");
                }

                if (posOfLastNonTerminal == prod.to.Length - 1 || prod.to[posOfLastNonTerminal + 1].IsEpsilon) {
                    //end or epsilon
                    //A-> aB
                    var f = FOLLOW(cnf, prod.from);
                    AddRangeLikeSet(f, curFollowSet, ref changed);

                    if (!changed) {
                        break;
                    }

                    continue;
                }

                //we have beta
                var first_beta = FIRST(cnf, prod.to[posOfLastNonTerminal + 1]);

                if (first_beta.Contains(new Terminal())) {
                    var f = FOLLOW(cnf, prod.from);
                    AddRangeLikeSet(f, curFollowSet, ref changed);
                }

                var first_beta_withoutEps = new List<Symbol>(first_beta);
                foreach (var s in first_beta_withoutEps.Where(s => s.IsEpsilon)) {
                    first_beta_withoutEps.Remove(s);
                }

                AddRangeLikeSet(first_beta_withoutEps, curFollowSet, ref changed);
            }

            if (!changed) {
                break;
            }
        }

        return followSets[S];
    }

    private static void AddRangeLikeSet(List<Symbol> from, List<Symbol> to, ref bool changed) {
        foreach (var s in from) {
            if (!to.Contains(s)) {
                to.Add(s);
                changed = true;
            }
        }
    }
}