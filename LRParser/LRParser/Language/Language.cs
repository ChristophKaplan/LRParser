using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

namespace LRParser.Language;

public interface ILanguageObject {
    
}

public class LexValue : ILanguageObject {
    public string Value { get; }

    public LexValue(string value) {
        Value = value;
    }
}

public abstract class Language<T,N>: ContextFreeGrammar<T,N> where T : Enum where N : Enum{
    private readonly Lexer<T> Lexer;
    private readonly Parser<T, N> Parser;
    protected abstract void SetUpGrammar();

    protected Language(params TokenDefinition<T>[] tokenDef) {
        Lexer = new Lexer<T>(tokenDef);
        SetUpGrammar();
        Parser = new Parser<T, N>(this);
    }

    protected virtual ILanguageObject TryParse(string input) {
        var tokens = Lexer.Tokenize(input);
        var tree = Parser.Parse(tokens);
        tree.EvaluateTree();
        return (ILanguageObject)tree.Symbol.SyntheticAttribute;
    }
}