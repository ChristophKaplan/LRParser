using System.Collections.Generic;
using LRParser.CFG;

namespace LRParser.Parser { 
    public class ConcreteSyntaxTree : ArenaTree<Symbol>
    {
        private readonly List<Production.SemanticActionDelegate?> _semanticActions = new();

        public int AddNode(Symbol symbol, Production.SemanticActionDelegate? semanticAction)
        {
            _semanticActions.Add(semanticAction);
            return base.AddNode(symbol);
        }
        
        protected override void Semantic(int nodeId, int start, int count)
        {
            // Leaf nodes (shifted terminals) have no semantic action; their
            // attribute was already set by the lexer, so leave it untouched.
            var action = _semanticActions[nodeId];
            if (action == null)
            {
                return;
            }

            var parameters = new Symbol[count];
            for (var i = 0; i < count; i++)
            {
                parameters[i] = _data[_children[start + i]];
            }

            var symbol = _data[nodeId];
            symbol.Attribute = action.Invoke(parameters);
            _data[nodeId] = symbol;
        }
    }
}