using System;
using System.Collections.Generic;
using LRParser.CFG;

namespace LRParser.Parser
{
    // Thrown when the parser reaches a state with no valid action for the current
    // token. Carries the source position and the set of symbols that would have
    // been valid, so callers can build their own diagnostics.
    public class ParseException : Exception
    {
        public (int lineNumber, int linePosition) Position { get; }
        public IReadOnlyList<Symbol> Expected { get; }

        public ParseException(string message, (int lineNumber, int linePosition) position,
            IReadOnlyList<Symbol> expected)
            : base(message)
        {
            Position = position;
            Expected = expected;
        }
    }
}
