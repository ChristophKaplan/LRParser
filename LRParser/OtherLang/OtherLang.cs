using LRParser.Language;
using LRParser.Lexer;

namespace OtherLang;

public enum Terminal {
    Type,
    Variable,
    Equals,
    Num,
    SemiColon,
}

public enum NonTerminal {
    StartSymbol,SecondStart, Declaration, Assigment,
}

public class OtherLang: Language<Terminal, NonTerminal>
{
    public OtherLang(): base(
        new TokenDefinition<Terminal>(Terminal.SemiColon, ";"),
        new TokenDefinition<Terminal>(Terminal.Equals, "="),
        new TokenDefinition<Terminal>(Terminal.Num, "\\d+"),
        new TokenDefinition<Terminal>(Terminal.Type, "Int|Float"),
        new TokenDefinition<Terminal>(Terminal.Variable, "[A-Z][a-z]*")) {
    }

    private Dictionary<string, string> TypeTable = new ();
    protected override void SetUpGrammar()
    {
        AddByEnumType(typeof(Terminal));
        AddByEnumType(typeof(NonTerminal));

        AddProductionRule(NonTerminal.StartSymbol, NonTerminal.SecondStart);
        AddProductionRule(NonTerminal.SecondStart, NonTerminal.Declaration);
        AddProductionRule(NonTerminal.SecondStart, NonTerminal.Declaration, NonTerminal.Assigment);
        
        AddProductionRule(NonTerminal.Declaration, Terminal.Type, Terminal.Variable, Terminal.SemiColon);
        AddProductionRule(NonTerminal.Assigment, Terminal.Variable, Terminal.Equals, Terminal.Num, Terminal.SemiColon);
        
        AddStartSymbol(NonTerminal.StartSymbol);

        AddSemanticAction(0, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        AddSemanticAction(1, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        AddSemanticAction(2, (lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });
        
        AddSemanticAction(3, (lhs, rhs) =>
        {
            var typeDeclaration = (string)rhs[0].SyntheticAttribute;
            var variable = (string)rhs[1].SyntheticAttribute;
            
            rhs[1].InheritetAttribute = typeDeclaration;
            
            TypeTable.Add(variable, typeDeclaration);
            
            lhs.SyntheticAttribute = rhs[0].SyntheticAttribute + " " + rhs[1].SyntheticAttribute;
        });
        
        AddSemanticAction(4, (lhs, rhs) =>
        {
            var variable = (string)rhs[0].SyntheticAttribute;
            var num =  (string)rhs[2].SyntheticAttribute;

            if (!TypeTable.TryGetValue(variable, out var type)) {
                throw new Exception($"Variable: {variable} not declared ");
            }
            rhs[0].InheritetAttribute = type;
            
            
            lhs.InheritetAttribute = type;
            lhs.SyntheticAttribute = num;
            
            if (type.Equals("Int")) {
                lhs.InheritetAttribute = typeof(int);
                lhs.SyntheticAttribute = int.Parse(num);
            }
        });
    }
}