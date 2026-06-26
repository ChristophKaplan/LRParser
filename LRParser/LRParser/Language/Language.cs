using System;
using System.Collections.Generic;
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

    // A reference type (not a struct): it is handed around as ILanguageObject and
    // mutated via Add/Insert, which only works reliably with reference semantics.
    // Backed by a List so Add is amortized O(1) rather than reallocating each call.
    public class ArrayValue : ILanguageObject {
        private readonly List<ILanguageObject> _values;

        public ArrayValue(params ILanguageObject[] value) {
            _values = new List<ILanguageObject>(value ?? Array.Empty<ILanguageObject>());
        }

        public ILanguageObject[] Value => _values.ToArray();

        public void Add(ILanguageObject value) {
            _values.Add(value);
        }

        public void Insert(ILanguageObject value, int index) {
            _values.Insert(index, value);
        }

        public override string ToString() {
            return string.Join(" ", _values.Select(v => v.ToString()));
        }
    }

    public abstract class Language<T, N> : ContextFreeGrammar<T, N> where T : struct, Enum where N : struct, Enum {
        private readonly Lexer<T> Lexer;
        private readonly Parser<T, N> Parser;

        protected abstract TokenDefinition<T>[] SetUpTokenDefinitions();
        protected abstract void SetUpGrammar();

        // Override to clear any per-parse semantic state (e.g. symbol tables) so
        // the same language instance can be reused across multiple TryParse calls.
        protected virtual void ResetState() {
        }

        protected Language() {
            Lexer = new Lexer<T>(SetUpTokenDefinitions());
            AddTerminalsAndNonTerminals();
            SetUpGrammar();
            InsertStartProductionRule();

            var showOutput = false;
            var debug = false;
            var isLaLr = true;
            Parser = new Parser<T, N>(this, showOutput , debug, isLaLr);
        }

        // Parses the input and returns the typed result, throwing LexerException
        // or ParseException on malformed input (and propagating any exception a
        // semantic action raises).
        public virtual ILanguageObject Parse(string input) {
            ResetState();
            var tokens = Lexer.Tokenize(input);
            var rootNodeId = Parser.Parse(tokens, out var tree);
            tree.EvaluateTree(rootNodeId);
            var symbol = tree.GetSymbol(rootNodeId);
            return symbol.Attribute;
        }

        // Non-throwing variant: returns false (instead of throwing) when the
        // input cannot be lexed or parsed. Exceptions raised by semantic actions
        // are intentionally not swallowed.
        public bool TryParse(string input, out ILanguageObject result) {
            try {
                result = Parse(input);
                return true;
            }
            catch (LexerException) {
                result = null!;
                return false;
            }
            catch (ParseException) {
                result = null!;
                return false;
            }
        }
    }
}