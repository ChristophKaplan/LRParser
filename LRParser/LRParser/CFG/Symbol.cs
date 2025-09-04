using System;
using LRParser.Language;

namespace LRParser.CFG
{
    public enum SymbolType
    {
        Terminal,
        NonTerminal
    }

    public enum InternalSymbol
    {
        Epsilon,
        Dollar,
        Start //NonTerminal
    }

    public struct Symbol : IEquatable<Symbol>
    {
        private readonly int _hashcode;

        //these ore not performant
        private readonly Enum _enum;
        public ILanguageObject SyntheticAttribute;
        public object InheritedAttribute;
        public (int lineNumber, int linePosition) Position;
        
        public SymbolType Type { get; }

        public Symbol(Enum @enum, SymbolType type)
        {
            _enum = @enum;
            Type = type;
            SyntheticAttribute = default;
            InheritedAttribute = default;
            Position = (-1, -1);
            _hashcode = 0;
            _hashcode = CreateHashCode();
        }

        public static Symbol Epsilon => new(InternalSymbol.Epsilon, SymbolType.Terminal);
        public static Symbol Dollar => new(InternalSymbol.Dollar, SymbolType.Terminal);
        public static Symbol Start => new(InternalSymbol.Start, SymbolType.NonTerminal);
        public bool IsEpsilon => _enum.Equals(InternalSymbol.Epsilon);
        public bool IsDollar => _enum.Equals(InternalSymbol.Dollar);

        public void SetValue(string value)
        {
            SyntheticAttribute = new LexValue(value);
        }
        
        public void SetPosition((int lineNumber, int linePosition) position)
        {
            this.Position = position;
        }

        private int CreateHashCode()
        {
            const int internalSymbolMarker = 23;
            return _enum is InternalSymbol ? 
                HashCode.Combine(_enum, Type, internalSymbolMarker) : 
                HashCode.Combine(_enum, Type);
        }

        public override int GetHashCode() => _hashcode;

        public bool Equals(Symbol other)
        {
            return _hashcode == other._hashcode && _enum.Equals(other._enum);
        }

        public override bool Equals(object? obj)
        {
            return obj is Symbol other && Equals(other);
        }

        public override string ToString()
        {
            return $"{_enum}";
        }
    }
}