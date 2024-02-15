using System.Text.RegularExpressions;
using LRParser.CFG;

namespace LRParser.Lexer;

public class Lexer {
    private readonly List<TokenDefinition> _tokenDefinitions;

    public Lexer(params TokenDefinition[] tokenDefinitions) {
        _tokenDefinitions = tokenDefinitions.ToList();
    }

    public List<Terminal> Tokenize(string source) {
        var result = new List<Terminal>();
        var currentIndex = 0;
        TokenDefinition tokenDefinition = null;

        while (currentIndex < source.Length) {
            
            var matchLength = 0;

            foreach (var rule in _tokenDefinitions) {
                var match = rule.Regex.Match(source, currentIndex);

                if (match.Success && match.Index - currentIndex == 0) {
                    tokenDefinition = rule;
                    matchLength = match.Length;
                    break;
                }
            }

            if (matchLength == 0) {
                currentIndex++;
                continue;
            }

            var value = source.Substring(currentIndex, matchLength);
            result.Add(tokenDefinition.CreateTerminal(value));

            currentIndex += matchLength;
        }

        return result;
    }
}