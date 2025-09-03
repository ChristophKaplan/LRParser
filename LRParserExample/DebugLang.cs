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
            AddProductionRule(Rule00, NonTerminal.List, NonTerminal.List, NonTerminal.LangObject);
            AddProductionRule(Rule00, NonTerminal.List, NonTerminal.LangObject);
            
            AddProductionRule(Rule00, NonTerminal.LangObject, Terminal.Identifier);
            AddProductionRule(Rule00, NonTerminal.LangObject, InternalSymbol.Epsilon);
        }
        
        private void Rule00(ref Symbol lhs, Symbol[] rhs)
        {
            lhs.SyntheticAttribute = rhs[0].SyntheticAttribute;
        }

    }
}