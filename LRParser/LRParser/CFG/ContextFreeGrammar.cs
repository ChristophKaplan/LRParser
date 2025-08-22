using System;
using System.Collections.Generic;
using System.Linq;

namespace LRParser.CFG
{
    public class ContextFreeGrammar<T, N> where T : Enum where N : Enum
    {
        private readonly List<Symbol> _nonTerminals = new();
        public readonly List<Production> Productions = new();
        private readonly List<Symbol> _terminals = new();
        public readonly Symbol StartSymbol = Symbol.Start;

        private void AddTerminal(T terminal)
        {
            _terminals.Add(new Symbol(terminal, SymbolType.Terminal));
        }

        private void AddNonTerminal(N nonTerminal)
        {
            _nonTerminals.Add(new Symbol(nonTerminal, SymbolType.NonTerminal));
        }

        private static readonly Dictionary<Type, Array> _enumValuesCache = new();
        protected void AddByEnumType<E>() where E : Enum
        {
            if (!_enumValuesCache.TryGetValue(typeof(E), out var values))
            {
                values = Enum.GetValues(typeof(E));
                _enumValuesCache[typeof(E)] = values;
            }
            
            foreach (int i in Enum.GetValues(typeof(E)))
            {
                if (typeof(T).IsAssignableFrom(typeof(E)))
                {
                    AddTerminal((T)Enum.ToObject(typeof(E), i));
                }
                else if (typeof(N).IsAssignableFrom(typeof(E)))
                {
                    AddNonTerminal((N)Enum.ToObject(typeof(E), i));
                }
            }
        }

        protected void AddProductionRule(Production.SemanticActionDelegate semanticAction, Enum premise,
            params Enum[] conclusions)
        {
            var rule = new Production(semanticAction, EnumToSym(premise),
                conclusions.Select(conclusion => EnumToSym(conclusion)).ToArray());
            Productions.Add(rule);
        }

        protected void InsertStartProductionRule()
        {
            var firstAfterStart = Productions.First().Premise;
            var startRule = new Production(StartRule, EnumToSym(InternalSymbol.Start), firstAfterStart);
            Productions.Insert(0, startRule);
            return;

            void StartRule(ref Symbol lhs, Symbol[] rhs)
            {
                lhs.SyntheticAttribute = rhs[0].SyntheticAttribute;
            }
        }

        private Symbol EnumToSym(Enum symbol)
        {
            var type = symbol.GetType();
            if (type == typeof(T))
            {
                return new Symbol((T)symbol, SymbolType.Terminal);
            }
            else if (type == typeof(N))
            {
                return new Symbol((N)symbol, SymbolType.NonTerminal);
            }
            else if (type == typeof(InternalSymbol))
            {
                var internalSymbol = (InternalSymbol)symbol;
                return internalSymbol == InternalSymbol.Start
                    ? new Symbol(internalSymbol, SymbolType.NonTerminal)
                    : new Symbol(internalSymbol, SymbolType.Terminal);
            }

            throw new Exception($"enum type {type} not found");
        }

        public List<Production> GetAllProdForNonTerminal(Symbol nonTerminal)
        {
            return Productions.Where(rule => rule.Premise.Equals(nonTerminal)).ToList();
        }

        public override string ToString()
        {
            var n = _nonTerminals.Aggregate("", (current, next) => $"{current} {next},");
            var sigma = _terminals.Aggregate("", (current, next) => $"{current} {next},");
            var p = Productions.Aggregate("", (current, next) => $"{current} {next},");
            var s = StartSymbol;

            return $"N:{n}\nSigma:{sigma}\nP:{p}\nS:{s}\n";
        }
    }
}