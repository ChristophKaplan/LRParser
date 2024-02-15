using System.Text.RegularExpressions;
using LRParser.CFG;

namespace LRParser;

public class TokenDefinition
{
    public string Name { get; }
    public Regex Regex { get; }

    public TokenDefinition(string name, Regex regex)
    {
        Name = name;
        Regex = regex;
    }
    
    public Terminal CreateTerminal(string value)
    {
        return new Terminal(Name);
    }
}

public class Lexer
{
    private readonly List<TokenDefinition> _tokenDefinitions = new();
    
    public Lexer()
    {
        Console.WriteLine("Lexer");
        
        var input = "Premise OR Q";
        
        _tokenDefinitions.Add(new TokenDefinition("LogicalOperator", new Regex("AND|OR")));
        _tokenDefinitions.Add(new TokenDefinition("AtomicSentence", new Regex("[A-Z][a-z]*")));
        
        var tokens = Tokenize(input);

        foreach (var token in tokens)
        {
            Console.WriteLine("token: "+token);
        }
    }
    
    public List<Terminal> Tokenize(string source)
    {
        var result = new List<Terminal>();
        var currentIndex = 0;
        TokenDefinition tokenDefinition = null;
        
        while (currentIndex < source.Length)
        {
            var matchLength = 0;

            foreach (var rule in _tokenDefinitions)
            {
                var match = rule.Regex.Match(source, currentIndex);
                
                if (match.Success && (match.Index - currentIndex) == 0)
                {
                    tokenDefinition = rule;
                    matchLength = match.Length;
                    break;
                }
            }

            if (matchLength == 0)
            {
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