using System;
using System.Collections.Generic;
using System.Linq;

namespace LRParser.CFG
{
    public static class ContextFreeGrammarExtensions
    {
        public static List<Symbol> First<T, N>(this ContextFreeGrammar<T, N> cfg, Symbol[] symbols,
            List<Symbol> alreadyChecked) where T : struct, Enum where N : struct, Enum
        {
            var result = new List<Symbol>();

            if (symbols.Length == 1 && symbols[0].IsEpsilon)
            {
                result.Add(Symbol.Epsilon);
                return result;
            }
            
            var symbol = symbols.First(s => !s.IsEpsilon);
            
            if (symbol.Type == SymbolType.Terminal)
            {
                //Trivial case
                result.Add(symbol);
                return result;
            }

            if (alreadyChecked.Contains(symbol))
            {
                //Logging.Log("recursion: " + symbol);
                return result;
            }

            alreadyChecked.Add(symbol);

            var productionsForNonTerminal = cfg.GetProductionsForNonTerminal(symbol);
            List<Symbol>[] directorSet = new List<Symbol>[productionsForNonTerminal.Count];

            for (var i = 0; i < productionsForNonTerminal.Count; i++)
            {
                directorSet[i] = FirstRules(cfg, productionsForNonTerminal[i].Conclusion, alreadyChecked);
                UnionMergeTo(directorSet[i], result);
            }

            return result;
        }

        private static List<Symbol> FirstRules<T, N>(this ContextFreeGrammar<T, N> cfg, Symbol[] symbols,
            List<Symbol> alreadyChecked) where T : struct, Enum where N : struct, Enum
        {
            var directorSet = new List<Symbol>();
            
            if (symbols.Length == 1 && symbols[0].IsEpsilon)
            {
                directorSet.Add(Symbol.Epsilon);
                return directorSet;
            }

            var length = symbols.Length;
            var j = 0;
            var allEpsContained = true;
            
            //add first-set of first symbol, remove eps
            var firstSet = First(cfg, new[]{symbols[j]}, new List<Symbol>(alreadyChecked));
            if (!firstSet.Contains(Symbol.Epsilon))
            {
                allEpsContained = false;
                
            }
            firstSet.Remove(Symbol.Epsilon);
            UnionMergeTo(firstSet, directorSet);
            
            //add first-set of following symbols IFF eps is contained
            for (j = 1; j < length; j++)
            {
                var nextFirstSet = First(cfg, new[]{symbols[j]}, new List<Symbol>(alreadyChecked));

                // prüfen, ob ε enthalten ist, bevor wir es entfernen
                bool epsilon = nextFirstSet.Contains(Symbol.Epsilon);

                // Terminals ohne ε hinzufügen
                nextFirstSet.Remove(Symbol.Epsilon);
                UnionMergeTo(nextFirstSet, directorSet);

                // nur abbrechen, wenn ε nicht enthalten ist
                if (!epsilon)
                {
                    allEpsContained = false;
                    break;
                }
            }

            //if first-set of last symbol containes epsilon, add only epsilon
            if (allEpsContained)
            {
                directorSet.Add(Symbol.Epsilon);
            }

            return directorSet;
        }

        private static void UnionMergeTo(List<Symbol> from, List<Symbol> to)
        {
            foreach (var notContained in from.Where(fromSymbol => !to.Contains(fromSymbol)))
            {
                to.Add(notContained);
            }
        }
    }
}