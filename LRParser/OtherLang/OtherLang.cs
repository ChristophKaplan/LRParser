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

public class IntValue : ILanguageObject {
    public int Value { get; set; }
    public IntValue(int value) {
        Value = value;
    }
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
        AddStartSymbol(NonTerminal.StartSymbol);
        
        var rule01 = AddProductionRule(NonTerminal.StartSymbol, NonTerminal.SecondStart);
        var rule02 = AddProductionRule(NonTerminal.SecondStart, NonTerminal.Declaration);
        var rule03 = AddProductionRule(NonTerminal.SecondStart, NonTerminal.Declaration, NonTerminal.Assigment);
        
        var rule04 = AddProductionRule(NonTerminal.Declaration, Terminal.Type, Terminal.Variable, Terminal.SemiColon);
        var rule05 = AddProductionRule(NonTerminal.Assigment, Terminal.Variable, Terminal.Equals, Terminal.Num, Terminal.SemiColon);
        
        
        rule01.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule02.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[0].SyntheticAttribute; });
        rule03.SetSemanticAction((lhs, rhs) => { lhs.SyntheticAttribute = rhs[1].SyntheticAttribute; });
        
        rule04.SetSemanticAction((lhs, rhs) =>
        {
            var typeDeclaration = (LexValue)rhs[0].SyntheticAttribute;
            var variable = (LexValue)rhs[1].SyntheticAttribute;
            
            rhs[1].InheritetAttribute = typeDeclaration;
            TypeTable.Add(variable.Value, typeDeclaration.Value);
            
            Console.WriteLine(typeDeclaration.Value + " " + variable.Value);
            lhs.InheritetAttribute = typeDeclaration;
            lhs.SyntheticAttribute = variable;
        });
        
        rule05.SetSemanticAction((lhs, rhs) =>
        {
            var variable = (LexValue)rhs[0].SyntheticAttribute;
            var num =  (LexValue)rhs[2].SyntheticAttribute;

            if (!TypeTable.TryGetValue(variable.Value, out var type)) {
                throw new Exception($"Variable: {variable} not declared ");
            }
            
            rhs[0].InheritetAttribute = type;
            
            lhs.InheritetAttribute = type;
            lhs.SyntheticAttribute = num;
            
            if (type.Equals("Int")) {
                lhs.InheritetAttribute = typeof(int);
                lhs.SyntheticAttribute = new IntValue(int.Parse(num.Value));
            }
        });
    }
}