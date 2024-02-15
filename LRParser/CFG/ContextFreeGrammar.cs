namespace LRParser.CFG;

public class ContextFreeGrammar {
    public readonly List<NonTerminal> NonTerminals;
    public readonly List<ProductionRule> ProductionRules;
    public readonly NonTerminal StartSymbol;
    public readonly List<Terminal> Terminals;

    public ContextFreeGrammar(List<NonTerminal> n, List<Terminal> sigma, List<ProductionRule> p, NonTerminal s) {
        NonTerminals = n;
        Terminals = sigma;
        ProductionRules = p;
        StartSymbol = s;

        if (HasLeftRecursion()) {
            //ResolveLeftRecursion();
        }

        Console.WriteLine($"HasLeftRecursion() = {HasLeftRecursion()}");
    }

    internal List<ProductionRule> GetAllProdForNonTerminal(NonTerminal nonTerminal) {
        return ProductionRules.Where(rule => rule.Premise.Equals(nonTerminal)).ToList();
    }

    private bool HasLeftRecursion() {
        return ProductionRules.Any(rule => rule.CheckLeftRecursion());
    }

    private void ResolveLeftRecursion() {
        List<ProductionRule> toRemove = new();
        List<ProductionRule> toAdd = new();

        foreach (var rule in ProductionRules) {
            if (rule.CheckLeftRecursion()) {
                var alternatives = GetAllProdForNonTerminal(rule.Premise);
                var changed = ChangeLeftRecursionRule(rule, alternatives);
                toAdd.AddRange(changed);
                toRemove.AddRange(alternatives);
            }
        }

        ProductionRules.AddRange(toAdd);
        foreach (var removeMe in toRemove) {
            ProductionRules.Remove(removeMe);
        }

        foreach (var r in ProductionRules) {
            if (!NonTerminals.Contains(r.Premise)) {
                NonTerminals.Add(r.Premise);
            }
        }

        Console.WriteLine(toAdd.Aggregate("\tchanged rules:", (c, n) => $"{c} {n},"));
    }

    private List<ProductionRule> ChangeLeftRecursionRule(ProductionRule leftRecursiveRule, List<ProductionRule> alternatives) {
        Console.WriteLine($"Try Resolve left recursion at: {leftRecursiveRule}");
        List<ProductionRule> changed = new();
        var newNonTerminal = new NonTerminal($"{leftRecursiveRule.Premise.Description}'");

        foreach (var alt in alternatives) {
            if (alt.Equals(leftRecursiveRule)) {
                continue;
            }

            var temp = alt.Conclusion.ToList();
            temp.Add(newNonTerminal);
            changed.Add(new ProductionRule(alt.Premise, temp.ToArray()));
        }

        var newTo = leftRecursiveRule.Conclusion.ToList();
        newTo.RemoveAt(0);
        newTo.Add(newNonTerminal);

        changed.Add(new ProductionRule(newNonTerminal, new Terminal()));
        changed.Add(new ProductionRule(newNonTerminal, newTo.ToArray()));

        return changed;
    }

    public override string ToString() {
        var n = NonTerminals.Aggregate("", (current, next) => $"{current} {next},");
        var sigma = Terminals.Aggregate("", (current, next) => $"{current} {next},");
        var p = ProductionRules.Aggregate("", (current, next) => $"{current} {next},");
        var s = StartSymbol;

        return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
    }
}