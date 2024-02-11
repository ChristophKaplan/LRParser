namespace LRParser.CFG;

public static class ContextFreeGrammarExtensions {
    public static List<Symbol> FIRST(this ContextFreeGrammar cnf, Symbol S, List<NonTerminal> alreadyChecked = null) {
        var result = new List<Symbol>();
        
        if (S is not NonTerminal A) {
            result.Add(S);
            return result;
        }

        alreadyChecked ??= new List<NonTerminal>();
        if (alreadyChecked.Contains(S))
        {
            Console.WriteLine("recursive rule?!?" + A);
            //return result;
        }
        alreadyChecked.Add(A);
        
        var p = cnf.GetAllProdForNonTerminal(A);
        var n = p.Count;
        var D = new List<Symbol>[n];

        for (var i = 0; i < n; i++) {
            D[i] = new List<Symbol>();

            var alpha_i = p[i].to[0];
            var changed = false;
            
            if (alpha_i.Equals(new Terminal())) {
                D[i].Add(new Terminal());
            }
            else {
                var length = p[i].to.Length;
                
                //first symbol no eps
                AddRangeLikeSet(FIRST(cnf, p[i].to[0],alreadyChecked),D[i], ref changed);
                D[i].Remove(new Terminal());

                if(length == 1) continue; //first entry already checked
                
                //mid symbols,if has eps check next
                int j;
                for (j = 1; FIRST(cnf, p[i].to[j],alreadyChecked).Contains(new Terminal()) && (j < length); j++) {
                    AddRangeLikeSet(FIRST(cnf, p[i].to[j],alreadyChecked), D[i], ref changed);
                    D[i].Remove(new Terminal());
                }

                //last symbol if mid had no epsilon
                if (j == length && FIRST(cnf, p[i].to[length],alreadyChecked).Contains(new Terminal())) {
                    D[i].Add(new Terminal());
                }
            }
            
            AddRangeLikeSet(D[i],result, ref changed);
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