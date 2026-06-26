using System;
using System.Collections.Generic;
using LogHelper;
using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

namespace ExampleLang
{
    public enum Terminal
    {
        Type,
        Variable,
        Equals,
        Num,
        SemiColon,
    }

    public enum NonTerminal
    {
        LangObject,
        Declaration,
        Assigment,
    }

    public struct IntValue : ILanguageObject
    {
        public int Value { get; set; }

        public IntValue(int value)
        {
            Value = value;
        }
    }

    public class ExampleLang : Language<Terminal, NonTerminal>
    {
        public ExampleLang()
        {
        }

        private Dictionary<string, string> TypeTable = new();

        protected override void ResetState()
        {
            TypeTable.Clear();
        }

        protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions()
        {
            return new[]
            {
                new TokenDefinition<Terminal>(Terminal.SemiColon, ";"),
                new TokenDefinition<Terminal>(Terminal.Equals, "="),
                new TokenDefinition<Terminal>(Terminal.Num, "\\d+"),
                new TokenDefinition<Terminal>(Terminal.Type, "Int|Float"),
                new TokenDefinition<Terminal>(Terminal.Variable, "[A-Z][a-z]*")
            };
        }

        protected override void SetUpGrammar()
        {
            AddRule(Rule02, NonTerminal.LangObject, NonTerminal.Declaration);
            //AddRule(Rule02, NonTerminal.LangObject, NonTerminal.LangObject, NonTerminal.LangObject);
            AddRule(Rule03, NonTerminal.LangObject, NonTerminal.Declaration, NonTerminal.Assigment);

            //terminating
            AddRule(Rule02, NonTerminal.LangObject, InternalSymbol.Epsilon);
            AddRule(Rule04, NonTerminal.Declaration, Terminal.Type, Terminal.Variable, Terminal.SemiColon);
            AddRule(Rule05, NonTerminal.Assigment, Terminal.Variable, Terminal.Equals, Terminal.Num,
                Terminal.SemiColon);
        }

        private ILanguageObject Rule01(Symbol[] rhs)
        {
            return rhs[0].Attribute;
        }

        private ILanguageObject Rule02(Symbol[] rhs)
        {
            // rhs is empty for the epsilon production (LangObject -> epsilon).
            return rhs.Length > 0 ? rhs[0].Attribute : null!;
        }

        private ILanguageObject Rule03(Symbol[] rhs)
        {
            return rhs[1].Attribute;
        }

        private ILanguageObject Rule04(Symbol[] rhs)
        {
            var typeDeclaration = (LexValue)rhs[0].Attribute;
            var variable = (LexValue)rhs[1].Attribute;

            TypeTable.Add(variable.Value, typeDeclaration.Value);

            //Logger.Log(typeDeclaration.Value + " " + variable.Value);

            return variable;
        }

        private ILanguageObject Rule05(Symbol[] rhs)
        {
            var variable = (LexValue)rhs[0].Attribute;
            var num = (LexValue)rhs[2].Attribute;

            if (!TypeTable.TryGetValue(variable.Value, out var type))
            {
                throw new Exception($"Variable: {variable} not declared ");
            }

            if (type.Equals("Int"))
            {
                return new IntValue(int.Parse(num.Value));
            }

            throw new Exception($"Unsupported type '{type}' for variable '{variable}'");
        }
    }
}