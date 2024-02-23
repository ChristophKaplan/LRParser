using LRParser.Language;
using LRParser.Lexer;

namespace OtherLang;

public enum Terminal {
    Type,
    Variable,
}

public enum NonTerminal {
    StartSymbol, Declaration
}

public class OtherLang: Language<Terminal, NonTerminal>
{
    public OtherLang(): base(
        new TokenDefinition<Terminal>(Terminal.Type, "Int|Float"),
        new TokenDefinition<Terminal>(Terminal.Variable, "[A-Z][a-z]*")) {
    }
    protected override void SetUpGrammar()
    {
        AddByEnumType(typeof(Terminal));
        AddByEnumType(typeof(NonTerminal));

        AddProductionRule(NonTerminal.StartSymbol, NonTerminal.Declaration);
        AddProductionRule(NonTerminal.Declaration, Terminal.Type, Terminal.Variable);
        
        AddStartSymbol(NonTerminal.StartSymbol);

        AddSemanticAction(0, input => input[0]);
        AddSemanticAction(1, input =>
        {
            return input[0] + " " + input[1];
        });
    }
}