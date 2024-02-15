namespace LRParser.Parser;

public class TreeNode<T> {
    private readonly List<TreeNode<T>> _children;

    public TreeNode(T data, TreeNode<T> parent) {
        Data = data;
        Parent = parent;
        _children = new List<TreeNode<T>>();
    }

    public T Data {
        get;
    }

    private TreeNode<T> Parent {
        get;
    }

    public void AddChild(TreeNode<T> child) {
        _children.Add(child);
    }

    public override string ToString() {
        return $"{Data} - {_children.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

    public TreeNode<T> GetRoot() {
        return Parent == null ? this : Parent.GetRoot();
    }

    public void PreOrder(Action<TreeNode<T>> action) {
        action(this);
        foreach (var child in _children) {
            child.PreOrder(action);
        }
    }
}