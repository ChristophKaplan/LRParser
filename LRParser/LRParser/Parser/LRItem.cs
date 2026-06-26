using System;
using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Parser {
    public class LRItem {
        private readonly int _dotPosition;
        private readonly int _coreHash;

        public LRItem(Production production, int dotPosition, List<Symbol> lookAheadSymbols) {
            Production = production;
            _dotPosition = dotPosition;
            // Copy so each item owns its lookahead set. Sharing the list (e.g. via
            // NextItem) let LALR lookahead merges mutate items in other states.
            LookAheadSymbols = new List<Symbol>(lookAheadSymbols);

            if (!IsComplete && CurrentSymbol.IsEpsilon) {
                _dotPosition++;
            }

            // The core (production + dot) is immutable, so cache its hash.
            _coreHash = HashCode.Combine(Production, _dotPosition);
        }

        public Production Production { get; }

        public List<Symbol> LookAheadSymbols { get; private set; }

        public bool IsComplete => _dotPosition == Production.Conclusion.Length;
        public Symbol CurrentSymbol => Production.Conclusion[_dotPosition];

        public LRItem NextItem {
            get {
                var nPos = _dotPosition;
                if (!IsComplete) {
                    nPos++;
                }

                return new LRItem(Production, nPos, LookAheadSymbols);
            }
        }

        public List<Symbol> GetSymbolsAfterDotSymbol() {
            List<Symbol> symbols = new();
            for (var i = _dotPosition + 1; i < Production.Conclusion.Length; i++) {
                symbols.Add(Production.Conclusion[i]);
            }

            return symbols;
        }

        public bool CoreEquals(LRItem other) {
            if(GetCoreHash() != other.GetCoreHash()) {
                return false;
            }
            
            return Production.Equals(other.Production) && _dotPosition == other._dotPosition;
        }
        
        public override bool Equals(object? obj) {
            if (obj == null || obj.GetType() != GetType()) {
                return false;
            }

            var other = (LRItem)obj;
            return Production.Equals(other.Production) && _dotPosition == other._dotPosition && LookAheadEquals(other.LookAheadSymbols);
        }
        
        private bool LookAheadEquals(List<Symbol> other) {
            return LookAheadSymbols.Count == other.Count && LookAheadSymbols.All(other.Contains);
        }
        
        public bool IsLookaheadContainedIn(LRItem otherItem)
        {
            foreach (var lookAheadSymbol in LookAheadSymbols)
            {
                if (!otherItem.LookAheadSymbols.Contains(lookAheadSymbol))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void AddLookahead(LRItem other)
        {
            foreach (var lookAheadSymbol in other.LookAheadSymbols)
            {
                if (!LookAheadSymbols.Contains(lookAheadSymbol))
                {
                    LookAheadSymbols.Add(lookAheadSymbol);
                }
            }
        }

        // Consistent with Equals: equal items share the same core, so hashing on
        // the core never distinguishes two items that Equals considers equal.
        // (Lookaheads are a mutable List and compared by content, so they must
        // not feed the hash.)
        public override int GetHashCode() {
            return _coreHash;
        }

        public int GetCoreHash()
        {
            return _coreHash;
        }

        public override string ToString() {
            var s = $"{Production.Premise} ->";
            for (var i = 0; i < Production.Conclusion.Length; i++) {
                if (i == _dotPosition) {
                    s += " .";
                }

                s += $" {Production.Conclusion[i]}";
            }

            if (_dotPosition == Production.Conclusion.Length) {
                s += " .";
            }

            s += $" ({string.Join(" ", LookAheadSymbols)})";
            return $"[{s}]";
        }
    }
}