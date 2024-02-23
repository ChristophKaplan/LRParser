using LRParser.CFG;

namespace LRParser.Parser;

public class ConcreteSyntaxTree {
    private readonly Production _production;

    public ConcreteSyntaxTree(Symbol symbol) {
        Symbol = symbol;
        Children = new List<ConcreteSyntaxTree>();
    }

    public ConcreteSyntaxTree(Production production) {
        Symbol = production.Premise;
        _production = production;
        Children = new List<ConcreteSyntaxTree>();
    }

    private List<ConcreteSyntaxTree> Children {
        get;
    }

    public Symbol Symbol {
        get;
    }

    public void AddChild(ConcreteSyntaxTree child) {
        Children.Add(child);
    }

    public override string ToString() {
        return $"\t{Symbol} {Children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
    }

    public void EvaluateTree() {
        if (Children.Count == 0) {
            return;
        }

        foreach (var child in Children) {
            Inherit();
            child.EvaluateTree();
        }

        Synthesize();
    }

    private void Inherit() {
        Console.WriteLine("Inherit:" + _production + " s:" + Symbol.SyntheticAttribute + " v:" + Symbol.InheritetAttribute);
    }

    private void Synthesize() {
        var args = new List<object>();
        foreach (var child in Children) {
            if (child.Symbol.Type == SymbolType.Terminal) {
                args.Add(child.Symbol.SyntheticAttribute);
            }
        }

        args.Reverse();
        Symbol.SyntheticAttribute = _production.SemanticAction.Invoke(args.ToArray());
    }
}