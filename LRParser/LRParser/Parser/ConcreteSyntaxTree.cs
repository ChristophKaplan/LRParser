using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser
{
    public class ConcreteSyntaxTree
    {
        private readonly Dictionary<int, ConcreteSyntaxTreeNode> _nodes = new();

        public int AddNode(Symbol symbol, Production.SemanticActionDelegate semanticAction)
        {
            var index = _nodes.Count;
            _nodes.Add(index, new ConcreteSyntaxTreeNode(symbol, semanticAction));
            return index;
        }

        public ConcreteSyntaxTreeNode GetNode(int index)
        {
            return _nodes.TryGetValue(index, out var node)
                ? node
                : throw new KeyNotFoundException($"Node with index {index} not found.");
        }

        public void EvaluateTree(int child)
        {
            if (_nodes[child].Children.Count == 0)
            {
                return;
            }

            foreach (var childId in _nodes[child].Children)
            {
                EvaluateTree(childId);
            }

            Semantic(child);
        }

        private void Semantic(int child)
        {
            var node = _nodes[child];
            var parameters = node.Children.Select(childId => _nodes[childId].Symbol).ToArray();
            node.SemanticAction.Invoke(ref node.Symbol, parameters);
            _nodes[child] = node; // Update the node after semantic action
        }

        public void AddChildToParent(int childId, int parentId)
        {
            if (!_nodes.ContainsKey(childId) ||
                !_nodes.TryGetValue(parentId, out var parentNode))
            {
                throw new KeyNotFoundException($"Child {childId} or parent {parentId} not found in the tree.");
            }
            
            parentNode.AddChild(childId);
            _nodes[parentId] = parentNode;
        }
    }
}