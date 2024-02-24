using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

namespace LRParser.Language;

public interface ILanguageObject { }

public abstract class Language<T,N>: ContextFreeGrammar<T,N> where T : Enum where N : Enum{
    private readonly Lexer<T> Lexer;
    private readonly Parser<T, N> Parser;
    protected abstract void SetUpGrammar();

    protected Language(params TokenDefinition<T>[] tokenDef) {
        Lexer = new Lexer<T>(tokenDef);
        SetUpGrammar();
        Parser = new Parser<T, N>(this);
    }
    
    public object TryParse(string input) {
        var tokens = Lexer.Tokenize(input);
        Console.WriteLine("Tokens: " + string.Join(", ", tokens));
        var tree = Parser.Parse(tokens);
        tree.EvaluateTree();
        return tree.Symbol.SyntheticAttribute;
    }
}