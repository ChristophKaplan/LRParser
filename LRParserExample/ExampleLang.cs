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

    public class IntValue : ILanguageObject
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
            AddProductionRule(Rule01, SpecialNonTerminal.Start, NonTerminal.LangObject);
            AddProductionRule(Rule02, NonTerminal.LangObject, NonTerminal.Declaration);
            AddProductionRule(Rule03, NonTerminal.LangObject, NonTerminal.Declaration, NonTerminal.Assigment);
            AddProductionRule(Rule04, NonTerminal.Declaration, Terminal.Type, Terminal.Variable, Terminal.SemiColon);
            AddProductionRule(Rule05, NonTerminal.Assigment, Terminal.Variable, Terminal.Equals, Terminal.Num, Terminal.SemiColon);
        }

        private void Rule01(ref Symbol lhs, Symbol[] rhs)
        {
            lhs.SyntheticAttribute = rhs[0].SyntheticAttribute;
        }

        private void Rule02(ref Symbol lhs, Symbol[] rhs)
        {
            lhs.SyntheticAttribute = rhs[0].SyntheticAttribute;
        }

        private void Rule03(ref Symbol lhs, Symbol[] rhs)
        {
            lhs.SyntheticAttribute = rhs[1].SyntheticAttribute;
        }

        private void Rule04(ref Symbol lhs, Symbol[] rhs)
        {
            var typeDeclaration = (LexValue)rhs[0].SyntheticAttribute;
            var variable = (LexValue)rhs[1].SyntheticAttribute;

            rhs[1].InheritedAttribute = typeDeclaration;
            TypeTable.Add(variable.Value, typeDeclaration.Value);

            //Logger.Log(typeDeclaration.Value + " " + variable.Value);
            lhs.InheritedAttribute = typeDeclaration;
            lhs.SyntheticAttribute = variable;
        }

        private void Rule05(ref Symbol lhs, Symbol[] rhs)
        {
            var variable = (LexValue)rhs[0].SyntheticAttribute;
            var num = (LexValue)rhs[2].SyntheticAttribute;

            if (!TypeTable.TryGetValue(variable.Value, out var type))
            {
                throw new Exception($"Variable: {variable} not declared ");
            }

            rhs[0].InheritedAttribute = type;

            lhs.InheritedAttribute = type;
            lhs.SyntheticAttribute = num;

            if (type.Equals("Int"))
            {
                lhs.InheritedAttribute = typeof(int);
                lhs.SyntheticAttribute = new IntValue(int.Parse(num.Value));
            }
        }

        public ILanguageObject TryParse(string input) => base.TryParse(input);
    }
}