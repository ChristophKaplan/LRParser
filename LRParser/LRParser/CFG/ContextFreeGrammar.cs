using System;
using System.Collections.Generic;
using System.Linq;
using LRParser.Language;

namespace LRParser.CFG
{
    public class ContextFreeGrammar<T, N> where T : struct, Enum where N : struct, Enum
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

        protected void AddTerminalsAndNonTerminals()
        {
            foreach (var terminal in Enum.GetValues<T>())
            {
                AddTerminal(terminal);
            }
            
            foreach (var nonTerminal in Enum.GetValues<N>())
            {
                AddNonTerminal(nonTerminal);
            }
        }

        protected void AddRule(Production.SemanticActionDelegate semanticAction, Enum premise, params Enum[] conclusions)
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

            ILanguageObject StartRule(Symbol[] rhs)
            {
                return rhs[0].Attribute;
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

        public List<Production> GetProductionsForNonTerminal(Symbol nonTerminal)
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