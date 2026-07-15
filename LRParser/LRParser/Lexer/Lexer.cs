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
                TokenDefinition<T>? tokenDefinition = null;

                // Maximal munch: pick the rule with the longest match at this
                // position. Ties keep the earliest-declared rule (strict '>'),
                // and zero-length matches are ignored so an empty-capable rule
                // (e.g. "\d*") cannot shadow a real token. The \G anchor in the
                // rule's regex guarantees any match starts exactly here.
                foreach (var rule in _tokenDefinitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);

                    if (match.Success && match.Length > matchLength)
                    {
                        tokenDefinition = rule;
                        matchLength = match.Length;
                    }
                }

                // No rule matched here (matchLength is non-zero exactly when a
                // rule was picked, since zero-length matches are ignored above).
                if (tokenDefinition is null)
                {
                    if (char.IsWhiteSpace(source[currentIndex]))
                    {
                        columnNumber++;
                        currentIndex++;
                        continue;
                    }

                    throw new LexerException(source[currentIndex], lineNumber, columnNumber);
                }

                var value = source.Substring(currentIndex, matchLength);
                result.Add(tokenDefinition.CreateTerminal(value, (lineNumber, columnNumber)));
                currentIndex += matchLength;

                // Advance line/column. A token may itself span newlines (e.g. a
                // multi-line string rule), so account for any '\n' inside it
                // rather than blindly adding the length to the column.
                var lastNewline = value.LastIndexOf('\n');
                if (lastNewline < 0)
                {
                    columnNumber += matchLength;
                }
                else
                {
                    foreach (var ch in value)
                    {
                        if (ch == '\n')
                        {
                            lineNumber++;
                        }
                    }

                    // Columns after the last newline, 1-based.
                    columnNumber = matchLength - lastNewline;
                }
            }

            return result;
        }
    }
}