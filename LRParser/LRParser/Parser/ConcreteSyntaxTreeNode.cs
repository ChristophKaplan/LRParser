using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser
{
    public class ConcreteSyntaxTree
    {
        private Dictionary<int, ConcreteSyntaxTreeNode> _nodes = new();
        
        public int AddNode(Symbol symbol, Production.SemanticActionDelegate semanticAction)
        {
            var index = _nodes.Count;
            _nodes.Add(index, new ConcreteSyntaxTreeNode (index, symbol, semanticAction));
            return index;
        }
        
        public ConcreteSyntaxTreeNode GetNode(int index)
        {
            return _nodes.TryGetValue(index, out var node) ? node : throw new KeyNotFoundException($"Node with index {index} not found.");
        }

        public void EvaluateTree(int child)
        {
            if (_nodes[child]._children.Count == 0)
            {
                return;
            }

            foreach (var childId in _nodes[child]._children)
            {
                EvaluateTree(childId);
            }

            Semantic(child);
        }

        private void Semantic(int child)
        {
            var node = _nodes[child];
            //if (node._semanticAction == null) return;
            
            var parameters = node._children.Select(childId => _nodes[childId].Symbol).ToArray();
            node._semanticAction.Invoke(ref node.Symbol, parameters);
            _nodes[child] = node; // Update the node after semantic action
        }

        public void AddChildToParent(int child, int parent)
        {
            if (!_nodes.ContainsKey(child) || !_nodes.ContainsKey(parent))
            {
                throw new KeyNotFoundException($"Child {child} or parent {parent} not found in the tree.");
            }
            
            ConcreteSyntaxTreeNode p = _nodes[parent];
            p.AddChild(child);
            _nodes[parent] = p;
        }
    }

    public struct ConcreteSyntaxTreeNode
    {
        private int Id;
        public readonly Production.SemanticActionDelegate _semanticAction;
        public Symbol Symbol;
        public readonly List<int> _children;

        public ConcreteSyntaxTreeNode(int id, Symbol symbol, Production.SemanticActionDelegate semanticAction)
        {
            Id = id;
            Symbol = symbol;
            _semanticAction = semanticAction;
            _children = new List<int>();
        }

        public void AddChild(int child)
        {
            _children.Insert(0, child);
        }

        public override string ToString()
        {
            return $"\t{Symbol} {_children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
        }
    }
}