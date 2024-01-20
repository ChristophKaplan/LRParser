using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CNF; 

public static class ContextFreeGrammarExtensions {
    public static bool GetGraphTopDownV1(this ContextFreeGrammar cnf, List<Terminal> tokens) {
        var root = new TreeNode<Symbol>(cnf.StartSymbol, null);
        return GetGraphTopDownV1(cnf, root, 0, tokens, 0);
    }

    private static bool GetGraphTopDownV1(this ContextFreeGrammar cnf, TreeNode<Symbol> currentTreeNode, int childIndex, List<Terminal> tokens, int tokenIndex) {
        //Page 45 TopDownAnalyse nachempfunden
        
        Console.WriteLine($"check for: ({currentTreeNode.data} != {tokens[tokenIndex]})");

        if (currentTreeNode.data is Terminal && !currentTreeNode.data.Equals(tokens[tokenIndex])) {
            Console.WriteLine($"\tterminals not matching ({currentTreeNode.data} != {tokens[tokenIndex]}), go back");
            return false;
        }

        if (currentTreeNode.data is Terminal && currentTreeNode.data.Equals(tokens[tokenIndex])) {
            Console.WriteLine($"\tfound:{currentTreeNode.data} = {tokens[tokenIndex]}, go next...");
            childIndex++;
            tokenIndex++;

            if (childIndex >= currentTreeNode.Parent.Children.Count) {
                Console.WriteLine($"\tIDK");
                return true;
            }

            if (tokenIndex >= tokens.Count) {
                Console.WriteLine($"\tsuccess");
                return true;
            }

            return GetGraphTopDownV1(cnf, currentTreeNode.Parent.Children[childIndex], childIndex, tokens, tokenIndex);
        }

        if (currentTreeNode.data is NonTerminal nonTerminal && !currentTreeNode.data.Equals(tokens[tokenIndex])) {
            var possibleProd = cnf.GetAllProdForNonTerminal(nonTerminal);
            Console.WriteLine($"\tFound {possibleProd.Count} prod's for {currentTreeNode.data}");

            for (var i = 0; i < possibleProd.Count; i++) {
                foreach (var sym in possibleProd[i].to) {
                    var child = new TreeNode<Symbol>(sym, currentTreeNode);
                    currentTreeNode.AddChild(child);
                }

                Console.WriteLine($"\tExpand from {currentTreeNode.data} -> {currentTreeNode.Children.First()}");
                if (GetGraphTopDownV1(cnf, currentTreeNode.Children.First(), childIndex, tokens, tokenIndex)) {
                    return true;
                }

                Console.WriteLine($"\t\twe are back at {currentTreeNode.data}, rule:({i + 1}/{possibleProd.Count})");
                currentTreeNode.Children.Clear();
            }

            Console.WriteLine($"\t\t\ttested all prod's for {currentTreeNode.data}, no success here");
            return false;
        }

        Console.WriteLine($"END: what is here? {currentTreeNode.data}");
        return false;
    }

    public static Word START(this ContextFreeGrammar cnf, Word v, int k) {
        return v.Length < k ? v : v.Slice(k);
    }

    public static List<Symbol> FIRST(this ContextFreeGrammar cnf, Symbol S) {
        //Console.WriteLine( $"FIRST({S})");
        
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

    private static void AddRangeLikeSet(List<Symbol> from, List<Symbol> to, ref bool changed) {
        foreach (var s in from) {
            if (!to.Contains(s)) {
                to.Add(s);
                changed = true;
            }
        }
    }
    
    public static List<Symbol> FOLLOW(this ContextFreeGrammar cnf, NonTerminal S) {
        
        Dictionary<NonTerminal,List<Symbol>> followSets = new ();
        foreach (var nonTerminal in cnf.NonTerminals) {
            followSets.Add(nonTerminal,new List<Symbol>(){new Terminal("$")});
        }

        while (true) {
            bool changed = false;
            
            foreach (var prod in cnf.ProductionRules) {
                if(!prod.to.Contains(S)) continue;
                if(!prod.ContainsNonTerminalConclusion() ) continue;
                    
                var posOfLastNonTerminal = prod.GetMostRightPosOf(S);
                
                
                var lastNonTerminal = (NonTerminal)prod.to[posOfLastNonTerminal];
                if (!followSets.TryGetValue(lastNonTerminal, out var curFollowSet)) {
                    throw new Exception($"cant find value: {lastNonTerminal} in followSets");
                }
                
                if (posOfLastNonTerminal == prod.to.Length - 1 || prod.to[posOfLastNonTerminal+1].IsEpsilon) {
                    //end or epsilon
                    //A-> aB
                    var f = FOLLOW(cnf, prod.from);
                    AddRangeLikeSet(f,curFollowSet,ref changed);
                    
                    if (!changed) {
                        break;
                    }
                    continue;
                }
                
                //we have beta
                var first_beta = FIRST(cnf, prod.to[posOfLastNonTerminal + 1]);
                
                if (first_beta.Contains(new Terminal())) {
                    var f = FOLLOW(cnf, prod.from);
                    AddRangeLikeSet(f,curFollowSet,ref changed);
                }

                var first_beta_withoutEps = new List<Symbol>(first_beta);
                foreach (var s in first_beta_withoutEps.Where(s => s.IsEpsilon)) {
                    first_beta_withoutEps.Remove(s);
                }
                AddRangeLikeSet(first_beta_withoutEps,curFollowSet,ref changed);
            }

            if (!changed) {
                break;
            }
        }

        return followSets[S];
    }

    public static List<LRItem> Closure(this ContextFreeGrammar cfg, List<LRItem> lrItems, int k = 1) {

        var result = new List<LRItem>();
        var stack = new Stack<LRItem>(lrItems);
        
        while (stack.Count > 0) {
            var cur = stack.Pop();
            result.Add(cur);

            if (cur.IsComplete()) {
                continue;
            }
                
            var rest = cur.GetSymbolsAfterDot();
            var oldLookahead = cur.Lookahead;
            rest.AddRange(oldLookahead);
            
            var curLookahead = cfg.FIRST(rest[0]); //k = 1
            
            if (cur.GetSymbol() is NonTerminal nonTerminal) {
                var prods = cfg.GetAllProdForNonTerminal(nonTerminal);
            
                foreach (var prod in prods) {
                    
                    var deeperItem = new LRItem(prod, 0, curLookahead);
                    var containedAlready = result.Where(r => r.Rule.Equals(deeperItem.rule) && r.dotPosition.Equals(deeperItem.dotPosition)).ToList();
                    if(containedAlready.Count > 1) throw new Exception("this should not happen? " + containedAlready.Aggregate("", (c, n) => $"{c} {n}, "));
                    
                    if (containedAlready.Count == 0) {
                        stack.Push(deeperItem);
                    }
                    else {
                        bool dontNeed = false;
                        AddRangeLikeSet(curLookahead,containedAlready[0].Lookahead,ref dontNeed);
                    }
                }
            }
        }
        return result;
    }

    public static List<State> GenerateStates(this ContextFreeGrammar cfg, LRItem startItem) {
        var firstState = new State(cfg.Closure(new List<LRItem>(){startItem}), 0);
        var states = new List<State>(){firstState};
        int count = 0;
        EnfoldTransitions(cfg, firstState,states,ref count);
        return states;
    }
    
    private static void EnfoldTransitions(this ContextFreeGrammar cfg, State current, List<State> states, ref int count) {
        
        var groups = current.GetIncompleteItems().GroupBy(i => i.GetSymbol());

        foreach (var group in groups) {
            
            if (group.Key.Equals(new Terminal())) continue;
            
            var nextItems = new List<LRItem>();
            foreach (var item in group) {
                if(!item.IsComplete()) nextItems.Add(item.NextItem());
            }
            
            var nextClosure = cfg.Closure(nextItems);
            count++;
            var next = new State(nextClosure, count);
            
            if(states.TryGetSameState(next, out var sameState)) {
                if (!sameState.IsReflexive()) {
                    current.transitions.Add(group.Key, sameState);
                    Console.WriteLine(current.Id+ "add " +group.Key+ " transition to" + sameState );
                }
                else {
                    Console.WriteLine(current.Id+ "found reflexive " +group.Key+ " transition to" + sameState );
                }
            }
            else {
                current.transitions.Add(group.Key, next);
                states.Add(next);
                EnfoldTransitions(cfg, next,states,ref count);
            }
        }
    }

    private static bool TryGetSameState(this List<State> states, State state, out State sameState) {
        foreach (var s in states) {
            if (s.EqualItems(state)) {
                sameState = s;
                return true;
            }
        }

        sameState = null;
        return false;
    }
}