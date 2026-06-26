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

        // Boxed grammar-symbol identity; the IsEpsilon/IsDollar predicates are
        // resolved once in the constructor to keep the hot paths boxing-free.
        private readonly Enum _enum;
        public ILanguageObject Attribute;
        public (int lineNumber, int linePosition) Position;
        
        public SymbolType Type { get; }

        public Symbol(Enum @enum, SymbolType type)
        {
            _enum = @enum;
            Type = type;
            Attribute = default;
            Position = (-1, -1);

            // Resolve the internal-symbol predicates once. Doing it here (a single
            // unboxing) avoids boxing InternalSymbol on every IsEpsilon/IsDollar
            // call, which the closure/reduce hot paths hit constantly.
            var internalSymbol = @enum as InternalSymbol?;
            IsEpsilon = internalSymbol == InternalSymbol.Epsilon;
            IsDollar = internalSymbol == InternalSymbol.Dollar;

            _hashcode = CreateHashCode(@enum, type);
        }

        public static Symbol Epsilon => new(InternalSymbol.Epsilon, SymbolType.Terminal);
        public static Symbol Dollar => new(InternalSymbol.Dollar, SymbolType.Terminal);
        public static Symbol Start => new(InternalSymbol.Start, SymbolType.NonTerminal);
        public bool IsEpsilon { get; }
        public bool IsDollar { get; }

        public void SetValue(string value)
        {
            Attribute = new LexValue(value);
        }
        
        public void SetPosition((int lineNumber, int linePosition) position)
        {
            this.Position = position;
        }

        private static int CreateHashCode(Enum @enum, SymbolType type)
        {
            const int internalSymbolMarker = 23;
            return @enum is InternalSymbol ?
                HashCode.Combine(@enum, type, internalSymbolMarker) :
                HashCode.Combine(@enum, type);
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