using ExampleLang;
using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace LRParserExample
{
    public enum Terminal
    {
        Identifier,
        Equals,
    }

    public enum NonTerminal
    {
        LangObject,
        Expression,
        List
    }
    
    public class DebugLang: Language<Terminal, NonTerminal>
    {
        protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions()
        {
            return new[]
            {
                new TokenDefinition<Terminal>(Terminal.Equals, "="),
                new TokenDefinition<Terminal>(Terminal.Identifier, "[A-Z][a-z]*")
            };
        }

        protected override void SetUpGrammar()
        {
            AddRule(Rule00, NonTerminal.List, NonTerminal.List, NonTerminal.LangObject);
            AddRule(Rule00, NonTerminal.List, NonTerminal.LangObject);
            
            AddRule(Rule00, NonTerminal.LangObject, Terminal.Identifier);
            AddRule(Rule00, NonTerminal.LangObject, InternalSymbol.Epsilon);
        }
        
        private ILanguageObject Rule00(Symbol[] rhs)
        {
           // rhs is empty for the epsilon production (LangObject -> epsilon).
           return rhs.Length > 0 ? rhs[0].Attribute : null!;
        }
    }
}