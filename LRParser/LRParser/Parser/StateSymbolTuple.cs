using System;
using LRParser.CFG;

namespace LRParser.LRParser.Parser
{
    public readonly struct StateSymbolTuple : IEquatable<StateSymbolTuple>
    {
        private readonly int _hashCode;
        public readonly int StateId;
        public readonly Symbol Symbol;

        public StateSymbolTuple(int stateId, Symbol symbol)
        {
            StateId = stateId;
            Symbol = symbol;
            _hashCode = HashCode.Combine(stateId.GetHashCode(), symbol.GetHashCode());
        }
        
        public override string ToString() 
        {
            return $"({StateId}, {Symbol})";
        }
        
        public override int GetHashCode()
        {
            return _hashCode;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is StateSymbolTuple other && Equals(other);
        }

        public bool Equals(StateSymbolTuple other)
        {
            return GetHashCode() == other.GetHashCode();
        }
    }
}
