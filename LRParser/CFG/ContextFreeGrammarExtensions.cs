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

            if (S.Equals(alpha_i)) {
                Console.WriteLine("recursive ?!?" + p[i]);
                continue;
            }
            
            if (alpha_i.Equals(new Terminal())) {
                D[i].Add(new Terminal());
            }
            else {
                var length = p[i].to.Length;
                var changed1 = false;
                var f = FIRST(cnf, p[i].to[0]);
                AddRangeLikeSet(f,D[i], ref changed1);
                D[i].Remove(new Terminal());

                int j = 0;
                for (j = 0; FIRST(cnf, p[i].to[j]).Contains(new Terminal()) && (j < length); j++) {
                    var changed2 = false;
                    AddRangeLikeSet(FIRST(cnf, p[i].to[j]), D[i], ref changed2);
                    D[i].Remove(new Terminal());
                }

                if (j == length && FIRST(cnf, p[i].to[length]).Contains(new Terminal())) {
                    D[i].Add(new Terminal());
                }
            }
            
            var changed3 = false;
            AddRangeLikeSet(D[i],result, ref changed3);
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