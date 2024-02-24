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
        Children.Insert(0, child);
    }

    public override string ToString() {
        return $"\t{Symbol} {Children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
    }

    public void EvaluateTree() {
        Console.WriteLine(ToString());
            
        if (Children.Count == 0) {
            return;
        }

        foreach (var child in Children) {
            child.EvaluateTree();
        }

        Semantic();
    }

    private void Semantic() {
        var parameters = Children.Select(child => child.Symbol).ToArray();
        _production.SemanticAction.Invoke(Symbol,parameters);
    }
}