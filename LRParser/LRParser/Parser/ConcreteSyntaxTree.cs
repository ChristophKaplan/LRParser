using LRParser.CFG;

namespace LRParser.Parser;

public class ConcreteSyntaxTree {
    private readonly Action<Symbol,Symbol[]> _semanticAction;
    public readonly Symbol Symbol;
    private readonly List<ConcreteSyntaxTree> _children;
    
    public ConcreteSyntaxTree(Symbol symbol) {
        Symbol = symbol;
        _semanticAction = null;
        _children = new List<ConcreteSyntaxTree>();
    }

    public ConcreteSyntaxTree(Symbol symbol, Action<Symbol,Symbol[]> semanticAction) {
        Symbol = symbol;
        _semanticAction = semanticAction;
        _children = new List<ConcreteSyntaxTree>();
    }

    public void AddChild(ConcreteSyntaxTree child) {
        _children.Insert(0, child);
    }

    public override string ToString() {
        return $"\t{Symbol} {_children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
    }

    public void EvaluateTree() {
        if (_children.Count == 0) {
            return;
        }
        
        foreach (var child in _children) {
            child.EvaluateTree();
        }
        
        Semantic();
    }

    private void Semantic() {
        var parameters = _children.Select(child => child.Symbol).ToArray();
        _semanticAction.Invoke(Symbol, parameters);
    }
}