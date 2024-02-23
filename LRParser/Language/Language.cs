using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

namespace LRParser.Language;

public abstract class Language<T,N>: ContextFreeGrammar<T,N> where T : Enum where N : Enum{
    protected readonly Lexer<T> Lexer;
    protected readonly Parser<T, N> Parser;
    protected abstract void SetUpGrammar();

    protected Language(params TokenDefinition<T>[] tokenDef) {
        Lexer = new Lexer<T>(tokenDef);
        SetUpGrammar();
        Parser = new Parser<T, N>(this);
    }
}