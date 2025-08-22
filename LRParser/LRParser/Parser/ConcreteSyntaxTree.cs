using System.Collections.Generic;
using LRParser.CFG;

namespace LRParser.Parser { 
    public class ConcreteSyntaxTree : ArenaTree<Symbol>
    {
        private readonly List<Production.SemanticActionDelegate> _semanticActions = new();
        
        public int AddNode(Symbol symbol, Production.SemanticActionDelegate semanticAction)
        {
            _semanticActions.Add(semanticAction);
            return base.AddNode(symbol);
        }
        
        protected override void Semantic(int nodeId, int start, int count)
        {
            var parameters = new Symbol[count];
            for (var i = 0; i < count; i++)
            {
                parameters[i] = _data[_children[start + i]];
            }

            var symbol = _data[nodeId];
            _semanticActions[nodeId].Invoke(ref symbol, parameters);
            _data[nodeId] = symbol;
        }
    }
}