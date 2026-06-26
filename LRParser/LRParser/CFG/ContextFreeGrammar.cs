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

        // Lazily built caches. They are populated on first use (i.e. during table
        // construction, after the grammar is fully assembled) and reused across
        // every closure/FIRST query instead of being recomputed each time.
        private static readonly List<Production> NoProductions = new();
        private Dictionary<Symbol, List<Production>>? _productionIndex;
        private Dictionary<Symbol, List<Symbol>>? _firstSets;

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
            foreach (T terminal in Enum.GetValues(typeof(T)))
            {
                AddTerminal(terminal);
            }
    
            foreach (N nonTerminal in Enum.GetValues(typeof(N)))
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

        // Wraps the grammar in an augmented start production Start -> <first rule's
        // premise>. By convention the FIRST rule added in SetUpGrammar defines the
        // grammar's start symbol, so rule order is significant.
        protected void InsertStartProductionRule()
        {
            if (Productions.Count == 0)
            {
                throw new InvalidOperationException(
                    "Grammar has no productions; add at least one rule before inserting the start production.");
            }

            var firstAfterStart = Productions.First().Premise;
            var startRule = new Production(StartRule, EnumToSym(InternalSymbol.Start), firstAfterStart);
            Productions.Insert(0, startRule);

            ValidateGrammar();
            return;

            ILanguageObject StartRule(Symbol[] rhs)
            {
                return rhs[0].Attribute;
            }
        }

        // Fails fast on grammars that would otherwise produce a broken parse
        // table: a non-terminal reachable from the start symbol that has no
        // productions (undefined) or cannot derive any terminal string
        // (unproductive). Unreachable non-terminals are harmless and ignored.
        private void ValidateGrammar()
        {
            EnsureProductionIndex();

            var reachable = new HashSet<Symbol>();
            var queue = new Queue<Symbol>();
            reachable.Add(StartSymbol);
            queue.Enqueue(StartSymbol);
            while (queue.Count > 0)
            {
                foreach (var production in GetProductionsForNonTerminal(queue.Dequeue()))
                {
                    foreach (var symbol in production.Conclusion)
                    {
                        if (symbol.Type == SymbolType.NonTerminal && reachable.Add(symbol))
                        {
                            queue.Enqueue(symbol);
                        }
                    }
                }
            }

            foreach (var nonTerminal in reachable)
            {
                if (GetProductionsForNonTerminal(nonTerminal).Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Non-terminal {nonTerminal} is reachable from the start symbol but has no productions.");
                }
            }

            // Productive = can derive a string of terminals. Fixpoint: a premise
            // is productive once some production has an all-productive conclusion
            // (terminals, epsilon, and already-productive non-terminals).
            var productive = new HashSet<Symbol>();
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var production in Productions)
                {
                    if (productive.Contains(production.Premise))
                    {
                        continue;
                    }

                    var allProductive = true;
                    foreach (var symbol in production.Conclusion)
                    {
                        if (symbol.Type != SymbolType.Terminal && !productive.Contains(symbol))
                        {
                            allProductive = false;
                            break;
                        }
                    }

                    if (allProductive && productive.Add(production.Premise))
                    {
                        changed = true;
                    }
                }
            }

            foreach (var nonTerminal in reachable)
            {
                if (!productive.Contains(nonTerminal))
                {
                    throw new InvalidOperationException(
                        $"Non-terminal {nonTerminal} is reachable but not productive (it cannot derive any terminal string).");
                }
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
            EnsureProductionIndex();
            return _productionIndex!.TryGetValue(nonTerminal, out var list) ? list : NoProductions;
        }

        private void EnsureProductionIndex()
        {
            if (_productionIndex != null)
            {
                return;
            }

            var index = new Dictionary<Symbol, List<Production>>();
            foreach (var production in Productions)
            {
                if (!index.TryGetValue(production.Premise, out var list))
                {
                    list = new List<Production>();
                    index[production.Premise] = list;
                }

                list.Add(production);
            }

            _productionIndex = index;
        }

        // FIRST of a symbol sequence (the terminals that can begin a string derived
        // from it; includes Epsilon iff the whole sequence is nullable). The per
        // non-terminal FIRST sets are precomputed once via a fixpoint and reused.
        public List<Symbol> First(IReadOnlyList<Symbol> sequence)
        {
            EnsureFirstSets();
            return FirstOfSequence(sequence, _firstSets!);
        }

        private void EnsureFirstSets()
        {
            if (_firstSets != null)
            {
                return;
            }

            EnsureProductionIndex();

            var first = new Dictionary<Symbol, List<Symbol>>();
            foreach (var premise in _productionIndex!.Keys)
            {
                first[premise] = new List<Symbol>();
            }

            // Iterate to a fixpoint: a production's premise gains FIRST of its
            // conclusion until no set changes. Standard FIRST construction.
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var production in Productions)
                {
                    var target = first[production.Premise];
                    foreach (var symbol in FirstOfSequence(production.Conclusion, first))
                    {
                        if (!target.Contains(symbol))
                        {
                            target.Add(symbol);
                            changed = true;
                        }
                    }
                }
            }

            _firstSets = first;
        }

        private static List<Symbol> FirstOfSequence(IReadOnlyList<Symbol> sequence,
            Dictionary<Symbol, List<Symbol>> firstSets)
        {
            var result = new List<Symbol>();
            var allNullable = true;

            foreach (var symbol in sequence)
            {
                if (symbol.IsEpsilon)
                {
                    // An explicit epsilon contributes nothing and is nullable, so
                    // the scan continues to the next symbol.
                    continue;
                }

                if (symbol.Type == SymbolType.Terminal)
                {
                    AddIfMissing(result, symbol);
                    allNullable = false;
                    break;
                }

                // Non-terminal: contribute its FIRST set; only look past it if it
                // is itself nullable (its FIRST contains epsilon).
                var symbolFirst = firstSets.TryGetValue(symbol, out var set) ? set : null;
                var nullable = false;
                if (symbolFirst != null)
                {
                    foreach (var s in symbolFirst)
                    {
                        if (s.IsEpsilon)
                        {
                            nullable = true;
                            continue;
                        }

                        AddIfMissing(result, s);
                    }
                }

                if (!nullable)
                {
                    allNullable = false;
                    break;
                }
            }

            if (allNullable)
            {
                AddIfMissing(result, Symbol.Epsilon);
            }

            return result;
        }

        private static void AddIfMissing(List<Symbol> set, Symbol symbol)
        {
            if (!set.Contains(symbol))
            {
                set.Add(symbol);
            }
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