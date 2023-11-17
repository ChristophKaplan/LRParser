using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using Microsoft.VisualBasic;

namespace CNF;

public abstract class Symbol {
    protected internal readonly string _value;

    public Symbol(string value) {
        _value = value;
    }

    public override string ToString() {
        return _value;
    }

    public override int GetHashCode() {
        return _value.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is Symbol other) return _value.Equals(other._value);
        return false;
    }
}

public class Terminal : Symbol {
    public Terminal(string value) : base(value) {
    }

    public Terminal() : base("epsilon") {
    }
}

public class NonTerminal : Symbol {
    public NonTerminal(string value) : base(value) {
    }
}

public class ProductionRule {
    internal NonTerminal from;
    internal Symbol[] to;

    public ProductionRule(NonTerminal from, params Symbol[] to) {
        this.from = from;
        this.to = to;
    }

    public bool CheckLeftRecursion() {
        return from.Equals(to[0]);
    }

    public override string ToString() {
        return $"{from} -> {to.Aggregate("(", (c, n) => $"{c} {n},")})";
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is ProductionRule other) {
            return this.ToString().Equals(other.ToString());
        }

        return false;
    }
}

public class Node {
    public readonly Symbol Symbol;
    public readonly Node Parent;
    public readonly List<Node> Children;

    public Node(Symbol symbol, Node parent) {
        this.Symbol = symbol;
        this.Parent = parent;
        this.Children = new();
    }

    public void AddChild(Node child) {
        this.Children.Add(child);
    }

    public override string ToString() {
        return $"{Symbol} - {Children.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

    public Node GetRoot() {
        if (Parent == null) return this;
        return Parent.GetRoot();
    }
}

public class ContextFreeGrammar {
    private List<NonTerminal> _nonTerminals;
    private List<Terminal> _terminals;
    private List<ProductionRule> _productionRules;
    private NonTerminal _startSymbol;

    public ContextFreeGrammar(List<NonTerminal> N, List<Terminal> Sigma, List<ProductionRule> P, NonTerminal S) {
        _nonTerminals = N;
        _terminals = Sigma;
        _productionRules = P;
        _startSymbol = S;

        if (HasLeftRecursion()) ResolveLeftRecursion();
        Console.WriteLine($"HasLeftRecursion() = {HasLeftRecursion()}");
    }

    private bool HasLeftRecursion() {
        foreach (var rule in _productionRules) {
            if (rule.CheckLeftRecursion()) {
                return true;
            }
        }

        return false;
    }

    private void ResolveLeftRecursion() {
        List<ProductionRule> toRemove = new();
        List<ProductionRule> toAdd = new();

        foreach (var rule in _productionRules) {
            if (rule.CheckLeftRecursion()) {
                List<ProductionRule> alternatives = GetAllProdForNonTerminal(rule.from);
                var changed = ChangeLeftRecursionRule(rule, alternatives);
                toAdd.AddRange(changed);
                toRemove.AddRange(alternatives);
            }
        }

        _productionRules.AddRange(toAdd);
        foreach (var removeMe in toRemove) {
            _productionRules.Remove(removeMe);
        }

        Console.WriteLine(toAdd.Aggregate("\tchanged rules:", (c, n) => $"{c} {n},"));
    }

    private List<ProductionRule> ChangeLeftRecursionRule(ProductionRule leftRecursiveRule, List<ProductionRule> alternatives) {
        Console.WriteLine($"Try Resolve left recursion at: {leftRecursiveRule}");
        List<ProductionRule> changed = new();
        var newNonTerminal = new NonTerminal($"{leftRecursiveRule.from._value}'");

        foreach (var alt in alternatives) {
            if (alt == leftRecursiveRule) {
                continue;
            }

            var temp = alt.to.ToList();
            temp.Add(newNonTerminal);
            changed.Add(new ProductionRule(alt.from, temp.ToArray()));
        }

        var newTo = leftRecursiveRule.to.ToList();
        newTo.RemoveAt(0);
        newTo.Add(newNonTerminal);

        changed.Add(new ProductionRule(newNonTerminal, new Terminal()));
        changed.Add(new ProductionRule(newNonTerminal, newTo.ToArray()));

        return changed;
    }

    public override string ToString() {
        var n = _nonTerminals.Aggregate("", (current, next) => $"{current} {next},");
        var sigma = _terminals.Aggregate("", (current, next) => $"{current} {next},");
        var p = _productionRules.Aggregate("", (current, next) => $"{current} {next},");
        var s = _startSymbol;

        return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
    }

    private List<ProductionRule> GetAllProdForNonTerminal(NonTerminal nonTerminal) {
        return _productionRules.Where(rule => rule.from.Equals(nonTerminal)).ToList();
    }

    public bool GetGraph(List<Terminal> tokens) {
        var root = new Node(_startSymbol, null);
        return GetGraph(root, 0, tokens, 0);
    }

    private bool GetGraph(Node currentNode, int childIndex, List<Terminal> tokens, int tokenIndex) {
        Console.WriteLine($"check for: ({currentNode.Symbol} != {tokens[tokenIndex]})");

        if (currentNode.Symbol is Terminal && !currentNode.Symbol.Equals(tokens[tokenIndex])) {
            Console.WriteLine($"\tterminals not matching ({currentNode.Symbol} != {tokens[tokenIndex]}), go back");
            return false;
        }

        if (currentNode.Symbol is Terminal && currentNode.Symbol.Equals(tokens[tokenIndex])) {
            Console.WriteLine($"\tfound:{currentNode.Symbol} = {tokens[tokenIndex]}, go next...");
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

            return GetGraph(currentNode.Parent.Children[childIndex], childIndex, tokens, tokenIndex);
            ;
        }

        if (currentNode.Symbol is NonTerminal nonTerminal && !currentNode.Symbol.Equals(tokens[tokenIndex])) {
            var possibleProd = GetAllProdForNonTerminal(nonTerminal);
            Console.WriteLine($"\tFound {possibleProd.Count} prod's for {currentNode.Symbol}");

            for (var i = 0; i < possibleProd.Count; i++) {
                foreach (var sym in possibleProd[i].to) {
                    Node child = new Node(sym, currentNode);
                    currentNode.AddChild(child);
                }

                Console.WriteLine($"\tExpand from {currentNode.Symbol} -> {currentNode.Children.First()}");
                if (GetGraph(currentNode.Children.First(), childIndex, tokens, tokenIndex)) {
                    return true;
                }

                Console.WriteLine($"\t\twe are back at {currentNode.Symbol}, rule:({i + 1}/{possibleProd.Count})");
                currentNode.Children.Clear();
            }

            Console.WriteLine($"\t\t\ttested all prod's for {currentNode.Symbol}, no success here");
            return false;
        }

        Console.WriteLine($"END: what is here? {currentNode.Symbol}");
        return false;
    }


    public Symbol START(Symbol v, int k) {
        if (v._value.Length < k) return v;

        if (v is Terminal) return new Terminal(v._value.Substring(0, k));
        if (v is NonTerminal) return new NonTerminal(v._value.Substring(0, k));

        throw new Exception("error");
    }

    public List<Symbol> FIRST(Symbol S) {
        var result = new List<Symbol>();

        if (S is not NonTerminal A) {
            result.Add(S);
            return result;
        }

        var p = GetAllProdForNonTerminal(A);
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

                D[i].AddRange(FIRST(p[i].to[0]));
                D[i].Remove(new Terminal());
                int j = 0;
                while (FIRST(p[i].to[j]).Contains(new Terminal()) && j < m) {
                    j++;
                    D[i].AddRange(FIRST(p[i].to[j]));
                    D[i].Remove(new Terminal());
                }

                if (j == m && FIRST(p[i].to[m]).Contains(new Terminal())) {
                    D[i].Add(new Terminal());
                }
            }

            result.AddRange(D[i]);
        }

        return result;
    }
}