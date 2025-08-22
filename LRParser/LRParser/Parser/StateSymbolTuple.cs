using LRParser.CFG;

namespace LRParser.Parser
{
    public readonly struct StateSymbolTuple
    {
        public readonly int State;
        public readonly Symbol Symbol;

        public StateSymbolTuple(int state, Symbol symbol)
        {
            State = state;
            Symbol = symbol;
        }
        
        public override string ToString() 
        {
            return $"({State}, {Symbol})";
        }
    }
}
