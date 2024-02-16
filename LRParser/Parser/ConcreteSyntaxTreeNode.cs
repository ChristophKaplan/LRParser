using LRParser.CFG;

namespace LRParser.Parser;

public class ConcreteSyntaxTreeNode {
    private ConcreteSyntaxTreeNode Parent { get; }
    private readonly List<ConcreteSyntaxTreeNode> _children = new ();
    private readonly ProductionRule _productionRule;
    public Symbol Symbol { get; }
    
    public ConcreteSyntaxTreeNode(Symbol symbol) {
        Symbol = symbol;
    }
    
    public ConcreteSyntaxTreeNode(ProductionRule productionRule) {
        Symbol = productionRule.Premise;
        _productionRule = productionRule;
    }

    public void AddChild(ConcreteSyntaxTreeNode child) {
        _children.Add(child);
    }
    
    public override string ToString() {
        return $"{Symbol} - {_children.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

    public ConcreteSyntaxTreeNode GetRoot() {
        return Parent == null ? this : Parent.GetRoot();
    }

    public void PreOrderReverse(Action<ConcreteSyntaxTreeNode> action) {
        action(this);
        for (var i = _children.Count-1; i >= 0; i--) {
            var child = _children[i];
            child.PreOrderReverse(action);
        }
    }
    
    public void Evaluate() {
        if(_children.Count == 0) {
            return;
        }
        
        foreach (var child in _children) {
            //inherrited
            child.Evaluate();
        }
        
        //synthetic

        var args = new List<object>();
        foreach (var child in _children) {
            if (child.Symbol is Symbol s) {
                args.Add(s._attribut1);
            }
        }

        args.Reverse();
        Symbol._attribut1 = _productionRule.SemanticAction.Invoke(args.ToArray());
    }
}