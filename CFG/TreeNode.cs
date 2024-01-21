namespace CFG; 

public class TreeNode<T> {
    public readonly T data;
    public readonly TreeNode<T> Parent;
    public readonly List<TreeNode<T>> Children;

    public TreeNode(T data, TreeNode<T> parent) {
        data = data;
        Parent = parent;
        Children = new();
    }

    public void AddChild(TreeNode<T> child) {
        Children.Add(child);
    }

    public override string ToString() {
        return $"{data} - {Children.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

    public TreeNode<T> GetRoot() {
        return Parent == null ? this : Parent.GetRoot();
    }
}

public class Node<T> {
    public readonly T data;
    public readonly List<Node<T>> Edge;

    public Node(T data) {
        data = data;
        Edge = new();
    }

    public void AddEdge(Node<T> edge) {
        Edge.Add(edge);
    }

    public override string ToString() {
        return $"{data} - {Edge.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

}
