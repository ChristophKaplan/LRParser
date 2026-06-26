using System;

namespace LRParser.Lexer
{
    // Thrown when the lexer encounters a character that matches no token rule.
    // Carries the offending character and its position so callers can react
    // without parsing the exception message.
    public class LexerException : Exception
    {
        public char Character { get; }
        public int Line { get; }
        public int Column { get; }

        public LexerException(char character, int line, int column)
            : base($"Lexer error: unrecognized character '{character}' at line {line}, column {column}")
        {
            Character = character;
            Line = line;
            Column = column;
        }
    }
}
