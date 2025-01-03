using System;
using System.Linq;
using LRParser.CFG;
using LRParser.Lexer;
using LRParser.Parser;

namespace LRParser.Language {
    public interface ILanguageObject {
    }

    public struct LexValue : ILanguageObject {
        public readonly string Value;

        public LexValue(string value) {
            Value = value;
        }

        public override string ToString() {
            return Value;
        }

        public static implicit operator string(LexValue value) {
            return value.Value;
        }
    }

    public struct ArrayValue : ILanguageObject {
        public ILanguageObject[] Value { get; private set; }

        public ArrayValue(params ILanguageObject[] value) {
            Value = value;
        }

        public void Add(ILanguageObject value) {
            var temp = new ILanguageObject[Value.Length + 1];
            for (var i = 0; i < Value.Length; i++) {
                temp[i] = Value[i];
            }

            temp[^1] = value;
            Value = temp;
        }

        public void Insert(ILanguageObject value, int index) {
            var temp = new ILanguageObject[Value.Length + 1];
            for (var i = 0; i < index; i++) {
                temp[i] = Value[i];
            }

            temp[index] = value;

            for (var i = index + 1; i < temp.Length; i++) {
                temp[i] = Value[i - 1];
            }

            Value = temp;
        }
    }

    public abstract class Language<T, N> : ContextFreeGrammar<T, N> where T : Enum where N : Enum {
        private readonly Lexer<T> Lexer;
        private readonly Parser<T, N> Parser;

        protected abstract TokenDefinition<T>[] SetUpTokenDefinitions();
        protected abstract void SetUpGrammar();

        protected Language() {
            Lexer = new Lexer<T>(SetUpTokenDefinitions());
            AddByEnumType(typeof(T));
            AddByEnumType(typeof(N));
            SetUpGrammar();
            Parser = new Parser<T, N>(this, false);

            if (!HasStartSymbol()) {
                throw new Exception("No production with start symbol found!");
            }
        }

        private bool HasStartSymbol() {
            return Productions.Any(rule => rule.Premise.IsStartSymbol);
        }

        public virtual ILanguageObject TryParse(string input) {
            var tokens = Lexer.Tokenize(input);
            var tree = Parser.Parse(tokens);
            tree.EvaluateTree();
            return tree.Symbol.SyntheticAttribute;
        }
    }
}