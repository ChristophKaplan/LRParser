using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser
{
    public struct ConcreteSyntaxTreeNode
    {
        public Symbol Symbol;
        public readonly List<int> Children;
        public readonly Production.SemanticActionDelegate SemanticAction;
        
        public ConcreteSyntaxTreeNode(Symbol symbol, Production.SemanticActionDelegate semanticAction)
        {
            Symbol = symbol;
            Children = new List<int>();
            
            SemanticAction = semanticAction;
        }

        public void AddChild(int child)
        {
            Children.Insert(0, child);
        }

        public override string ToString()
        {
            return $"\t{Symbol} {Children.Aggregate("\n\t", (current, next) => $"{current} {next}")}";
        }
    }
}