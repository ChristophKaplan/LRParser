using LRParser.CFG;

namespace LRParser.Parser;

public class ConcreteSyntaxTreeNode {
    private ConcreteSyntaxTreeNode Parent { get; }
    public List<ConcreteSyntaxTreeNode> Children { get; }
    private readonly ProductionRule _productionRule;
    public Symbol Symbol { get; }
    
    public ConcreteSyntaxTreeNode(Symbol symbol) {
        Symbol = symbol;
        Children = new List<ConcreteSyntaxTreeNode>();
    }
    
    public ConcreteSyntaxTreeNode(ProductionRule productionRule) {
        Symbol = productionRule.Premise;
        _productionRule = productionRule;
        Children = new List<ConcreteSyntaxTreeNode>();
    }

    public void AddChild(ConcreteSyntaxTreeNode child) {
        Children.Add(child);
    }
    
    public override string ToString() {
        return $"\t{Symbol} {Children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
    }

    public ConcreteSyntaxTreeNode GetRoot() {
        return Parent == null ? this : Parent.GetRoot();
    }

    public void PreOrderReverse(Action<ConcreteSyntaxTreeNode> action) {
        action(this);
        for (var i = Children.Count-1; i >= 0; i--) {
            var child = Children[i];
            child.PreOrderReverse(action);
        }
    }
    
    public void Evaluate() {
        if(Children.Count == 0) {
            return;
        }
        
        foreach (var child in Children) {
            //inherrited
            child.Evaluate();
        }
        
        //synthetic

        var args = new List<object>();
        foreach (var child in Children) {
            if (child.Symbol is Symbol s) {
                args.Add(s.Attribut1);
            }
        }

        args.Reverse();
        Symbol.Attribut1 = _productionRule.SemanticAction.Invoke(args.ToArray());
    }
}