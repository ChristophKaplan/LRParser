namespace LRParser.Parser;

public class TreeNode<T> {
    private T Data { get; }
    private TreeNode<T> Parent { get; }

    private readonly List<TreeNode<T>> _children;

    public TreeNode(T data, TreeNode<T> parent) {
        Data = data;
        Parent = parent;
        _children = new List<TreeNode<T>>();
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
}