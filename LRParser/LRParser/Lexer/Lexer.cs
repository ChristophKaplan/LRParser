using System;
using System.Collections.Generic;
using System.Linq;
using LRParser.CFG;

namespace LRParser.Lexer
{
    public class Lexer<T> where T : Enum
    {
        private readonly List<TokenDefinition<T>> _tokenDefinitions;

        public Lexer(params TokenDefinition<T>[] tokenDefinitions)
        {
            _tokenDefinitions = tokenDefinitions.ToList();
        }

        public List<Symbol> Tokenize(string source)
        {
            var result = new List<Symbol>();
            var currentIndex = 0;
            var lineNumber = 1;
            var columnNumber = 1; // Spaltenzähler hinzugefügt
            TokenDefinition<T> tokenDefinition = null;

            while (currentIndex < source.Length)
            {
                if (source[currentIndex] == '\n')
                {
                    lineNumber++;
                    columnNumber = 1;
                    currentIndex++;
                    continue;
                }

                var matchLength = 0;

                foreach (var rule in _tokenDefinitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);

                    if (match.Success && match.Index - currentIndex == 0)
                    {
                        tokenDefinition = rule;
                        matchLength = match.Length;
                        break;
                    }
                }

                if (matchLength == 0)
                {
                    if (char.IsWhiteSpace(source[currentIndex]))
                    {
                        columnNumber++;
                        currentIndex++;
                        continue;
                    }

                    throw new Exception(
                        $"Lexer error: unrecognized character '{source[currentIndex]}' at line {lineNumber}, column {columnNumber}");
                }

                var value = source.Substring(currentIndex, matchLength);
                var positon = (lineNumber, columnNumber);
                result.Add(tokenDefinition.CreateTerminal(value, positon));
                currentIndex += matchLength;
                columnNumber += matchLength; // Erst NACH der Positionsbestimmung erhöhen
            }

            return result;
        }
    }
}