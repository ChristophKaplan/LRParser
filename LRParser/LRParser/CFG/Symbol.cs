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
        private readonly Enum _enum;

        public ILanguageObject SyntheticAttribute;
        public object InheritedAttribute;

        public SymbolType Type { get; }

        public Symbol(Enum @enum, SymbolType type)
        {
            _enum = @enum;
            _hashcode = _enum.GetHashCode();
            Type = type;
            SyntheticAttribute = default;
            InheritedAttribute = default;
        }

        public static Symbol Epsilon => new(InternalSymbol.Epsilon, SymbolType.Terminal);
        public static Symbol Dollar => new(InternalSymbol.Dollar, SymbolType.Terminal);
        public static Symbol Start => new(InternalSymbol.Start, SymbolType.NonTerminal);

        public bool IsEpsilon => _enum.Equals(InternalSymbol.Epsilon);
        public bool IsDollar => _enum.Equals(InternalSymbol.Dollar);
        public bool IsStartSymbol => _enum.Equals(InternalSymbol.Start);

        public void SetValue(string value)
        {
            SyntheticAttribute = new LexValue(value);
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
