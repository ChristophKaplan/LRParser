namespace CNF; 

public class Node<T> {
    public readonly T data;
    public readonly Node<T> Parent;
    public readonly List<Node<T>> Children;

    public Node(T data, Node<T> parent) {
        data = data;
        Parent = parent;
        Children = new();
    }

    public void AddChild(Node<T> child) {
        Children.Add(child);
    }

    public override string ToString() {
        return $"{data} - {Children.Aggregate("(", (current, next) => $"{current} {next},")})";
    }

    public Node<T> GetRoot() {
        return Parent == null ? this : Parent.GetRoot();
    }
}
