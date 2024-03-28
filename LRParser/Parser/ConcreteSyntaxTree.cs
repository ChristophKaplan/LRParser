using LRParser.CFG;

namespace LRParser.Parser;

public class ConcreteSyntaxTree {
    private readonly Action<Symbol,Symbol[]> _semanticAction;
    public Symbol Symbol { get; }
    private List<ConcreteSyntaxTree> Children { get; }
    
    public ConcreteSyntaxTree(Symbol symbol) {
        Symbol = symbol;
        _semanticAction = null;
        Children = new List<ConcreteSyntaxTree>();
    }

    public ConcreteSyntaxTree(Symbol symbol, Action<Symbol,Symbol[]> semanticAction) {
        Symbol = symbol;
        _semanticAction = semanticAction;
        Children = new List<ConcreteSyntaxTree>();
    }

    public void AddChild(ConcreteSyntaxTree child) {
        Children.Insert(0, child);
    }

    public override string ToString() {
        return $"\t{Symbol} {Children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
    }

    public void EvaluateTree() {
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
        _semanticAction.Invoke(Symbol, parameters);
    }
}