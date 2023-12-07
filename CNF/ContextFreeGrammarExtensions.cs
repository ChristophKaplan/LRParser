namespace CNF; 

public static class ContextFreeGrammarExtensions {
    public static bool GetGraphTopDownV1(this ContextFreeGrammar cnf, List<Terminal> tokens) {
        var root = new Node<Symbol>(cnf.StartSymbol, null);
        return GetGraphTopDownV1(cnf, root, 0, tokens, 0);
    }

    private static bool GetGraphTopDownV1(this ContextFreeGrammar cnf, Node<Symbol> currentNode, int childIndex, List<Terminal> tokens, int tokenIndex) {
        //Page 45 TopDownAnalyse nachempfunden
        
        Console.WriteLine($"check for: ({currentNode.data} != {tokens[tokenIndex]})");

        if (currentNode.data is Terminal && !currentNode.data.Equals(tokens[tokenIndex])) {
            Console.WriteLine($"\tterminals not matching ({currentNode.data} != {tokens[tokenIndex]}), go back");
            return false;
        }

        if (currentNode.data is Terminal && currentNode.data.Equals(tokens[tokenIndex])) {
            Console.WriteLine($"\tfound:{currentNode.data} = {tokens[tokenIndex]}, go next...");
            childIndex++;
            tokenIndex++;

            if (childIndex >= currentNode.Parent.Children.Count) {
                Console.WriteLine($"\tIDK");
                return true;
            }

            if (tokenIndex >= tokens.Count) {
                Console.WriteLine($"\tsuccess");
                return true;
            }

            return GetGraphTopDownV1(cnf, currentNode.Parent.Children[childIndex], childIndex, tokens, tokenIndex);
        }

        if (currentNode.data is NonTerminal nonTerminal && !currentNode.data.Equals(tokens[tokenIndex])) {
            var possibleProd = cnf.GetAllProdForNonTerminal(nonTerminal);
            Console.WriteLine($"\tFound {possibleProd.Count} prod's for {currentNode.data}");

            for (var i = 0; i < possibleProd.Count; i++) {
                foreach (var sym in possibleProd[i].to) {
                    var child = new Node<Symbol>(sym, currentNode);
                    currentNode.AddChild(child);
                }

                Console.WriteLine($"\tExpand from {currentNode.data} -> {currentNode.Children.First()}");
                if (GetGraphTopDownV1(cnf, currentNode.Children.First(), childIndex, tokens, tokenIndex)) {
                    return true;
                }

                Console.WriteLine($"\t\twe are back at {currentNode.data}, rule:({i + 1}/{possibleProd.Count})");
                currentNode.Children.Clear();
            }

            Console.WriteLine($"\t\t\ttested all prod's for {currentNode.data}, no success here");
            return false;
        }

        Console.WriteLine($"END: what is here? {currentNode.data}");
        return false;
    }

    public static Word START(this ContextFreeGrammar cnf, Word v, int k) {
        return v.Length < k ? v : v.Slice(k);
    }

    public static List<Symbol> FIRST(this ContextFreeGrammar cnf, Symbol S) {
        var result = new List<Symbol>();

        if (S is not NonTerminal A) {
            result.Add(S);
            return result;
        }

        var p = cnf.GetAllProdForNonTerminal(A);
        int n = p.Count;
        var D = new List<Symbol>[n];


        for (int i = 0; i < n; i++) {
            D[i] = new List<Symbol>();

            var alpha_i = p[i].to[0];
            if (alpha_i.Equals(new Terminal())) {
                D[i].Add(new Terminal());
            }
            else {
                int m = p[i].to.Length;

                D[i].AddRange(FIRST(cnf, p[i].to[0]));
                D[i].Remove(new Terminal());
                int j = 0;
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
        List<Node<NonTerminal>> nodes = new ();
        foreach (var nonTerminal in cnf.NonTerminals) {
            nodes.Add(new Node<NonTerminal>(nonTerminal,null));
        }

        foreach (var rule in cnf.ProductionRules) {
            for (var i = 0; i < rule.to.Length; i++) {
                var outcome = rule.to[i];
                if (outcome is NonTerminal ntOut) {
                    if (i == rule.to.Length - 1) {
                        connect(new Node<NonTerminal>(rule.from), new Node<NonTerminal>(ntOut));
                    }
                    else {
                        //nicht letzte
                        var first = FIRST(cnf, ntOut);
                        
                    }
                }
            }
        }
        
    }
    
}