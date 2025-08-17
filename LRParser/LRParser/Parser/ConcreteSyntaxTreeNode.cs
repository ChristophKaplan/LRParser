using System;
using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser
{
    public class ConcreteSyntaxTreeNode
    {
        private readonly Production.SemanticActionDelegate _semanticAction;
        public Symbol Symbol;
        private readonly List<ConcreteSyntaxTreeNode> _children;

        public ConcreteSyntaxTreeNode(Symbol symbol)
        {
            Symbol = symbol;
            _semanticAction = null;
            _children = new List<ConcreteSyntaxTreeNode>();
        }

        public ConcreteSyntaxTreeNode(Symbol symbol, Production.SemanticActionDelegate semanticAction)
        {
            Symbol = symbol;
            _semanticAction = semanticAction;
            _children = new List<ConcreteSyntaxTreeNode>();
        }

        public void AddChild(ConcreteSyntaxTreeNode child)
        {
            _children.Insert(0, child);
        }

        public override string ToString()
        {
            return $"\t{Symbol} {_children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
        }

        public void EvaluateTree()
        {
            if (_children.Count == 0)
            {
                return;
            }

            foreach (var child in _children)
            {
                child.EvaluateTree();
            }

            Semantic();
        }

        private void Semantic()
        {
            var parameters = _children.Select(child => child.Symbol).ToArray();
            _semanticAction.Invoke(ref Symbol, parameters);
        }
    }
}
