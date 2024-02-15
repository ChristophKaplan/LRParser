

using LRParser.CFG;

namespace LRParser.Parser;

public class TreeNode<T> {
    private readonly List<TreeNode<T>> _children = new ();
    private ProductionRule _productionRule;
    
    public TreeNode(T data) {
        Data = data;
    }
    
    public TreeNode(T data, ProductionRule productionRule) {
        Data = data;
        _productionRule = productionRule;
    }

    public T Data { get; }
    private TreeNode<T> Parent { get; }

    public void AddChild(TreeNode<T> child) {
        _children.Add(child);
    }
    
    public override string ToString() {
        return $"{Data} - {_children.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

    public TreeNode<T> GetRoot() {
        return Parent == null ? this : Parent.GetRoot();
    }

    public void PreOrderReverse(Action<TreeNode<T>> action) {
        action(this);
        for (var i = _children.Count-1; i >= 0; i--) {
            var child = _children[i];
            child.PreOrderReverse(action);
        }
    }
    
    public void Eval() {
        if(_children.Count == 0) {
            return;
        }
        
        foreach (var child in _children) {
            //veerbten
            child.Eval();
        }
        
        //synthetischen

        var args = new List<object>();
        foreach (var child in _children) {
            if (child.Data is Symbol s) {
                args.Add(s._attribut1);
            }
        }

        args.Reverse();
        (Data as Symbol)._attribut1 = _productionRule.SemanticAction.Invoke(args.ToArray());
    }
}