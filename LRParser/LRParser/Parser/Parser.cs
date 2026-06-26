using System;
using System.Collections.Generic;
using System.Linq;
using LogHelper;
using LRParser.CFG;
using LRParser.LRParser.Parser;

namespace LRParser.Parser
{
    public class Parser<T, N> where T : struct, Enum where N : struct, Enum
    {
        private readonly ContextFreeGrammar<T, N> _cfg;
        private readonly Table<T, N> _table;
        private string _parsingOutput = string.Empty;
        private readonly bool _showOutput;
        private readonly Stack<int> _stateStack = new();
        private readonly Stack<int> _treeNodeStack = new();
        private ConcreteSyntaxTree _syntaxTree = new();
        // Cursor into the working input. Advancing an index instead of removing
        // the front element keeps parsing O(n) instead of O(n^2).
        private int _position;
        
        public Parser(ContextFreeGrammar<T, N> cfg, bool showOutput = false, bool debug = false, bool isLaLr = true)
        {
            _cfg = cfg;
            _showOutput = showOutput;
            var startItem = new LRItem(cfg.Productions[0], 0, new List<Symbol> { Symbol.Dollar });
            var states = new StateManager<T, N>(startItem, _cfg, showOutput, debug, isLaLr);
            _table = new Table<T, N>(states, _cfg, showOutput);
            if (debug)
            {
                Logger.Log(_table.ToString());
            }
        }

        private void ResetStack()
        {
            _stateStack.Clear();
            _stateStack.Push(0);
            _treeNodeStack.Clear();
            _syntaxTree = new ConcreteSyntaxTree();
            _position = 0;
        }

        public int Parse(List<Symbol> input,out ConcreteSyntaxTree tree)
        {
            // Work on a private copy so the caller's list is not mutated.
            var workingInput = new List<Symbol>(input) { Symbol.Dollar };
            ResetStack();

            var shouldContinue = true;
            while (shouldContinue)
            {
                var action = GetParserAction(workingInput);
                shouldContinue = ProcessAction(workingInput, action);
            }
            tree = this._syntaxTree;
            return _treeNodeStack.Pop();
        }

        private bool ProcessAction(List<Symbol> input, ParserAction action)
        {
            var parserAction = action.Action;

            switch (parserAction)
            {
                case ParserAction.Type.Error:
                    Error(input);
                    return false;
                case ParserAction.Type.Accept:
                    Accept();
                    return false;
                case ParserAction.Type.Shift:
                    Shift(input, action.StateOrProdId);
                    break;
                case ParserAction.Type.Reduce:
                    Reduce(action.StateOrProdId);
                    break;
                default:
                    Error(input);
                    return false;
            }

            return true;
        }

        private ParserAction GetParserAction(List<Symbol> input)
        {
            return _table.ActionTable.TryGetValue(new StateSymbolTuple(_stateStack.Peek(), input[_position]), out var action)
                ? action
                : ParserAction.Default;
        }

        private void Accept()
        {
            if (!_showOutput)
            {
                return;
            }

            _parsingOutput += "ACCEPT\n";
            Logger.Log(_parsingOutput);
        }

        private void Error(List<Symbol> input)
        {
            var current = input[_position];
            var expected = _table.ExpectedSymbols(_stateStack.Peek());
            var expectedText = expected.Count > 0
                ? string.Join(", ", expected.Select(s => s.ToString()))
                : "<none>";

            var message =
                $"Parse error at line {current.Position.lineNumber}, column {current.Position.linePosition}: " +
                $"unexpected {current} '{current.Attribute}'. Expected: {expectedText}.";

            if (_showOutput)
            {
                _parsingOutput += message + "\n";
                Logger.Log(_parsingOutput);
            }

            throw new ParseException(message, current.Position, expected);
        }

        private void Shift(List<Symbol> input, int shiftState)
        {
            var current = input[_position];
            if (_showOutput)
            {
                _parsingOutput += $"SHIFT: {current}, current:{_stateStack.Peek()}, next state:{shiftState}\n";
            }

            _stateStack.Push(shiftState);
            var nodeIndex = _syntaxTree.AddNode(current, null);
            _treeNodeStack.Push(nodeIndex);
            _position++;
        }

        private void Reduce(int ruleId)
        {
            var rule = _cfg.Productions[ruleId];
            if (_showOutput)
            {
                _parsingOutput += $"REDUCE ({ruleId}), Rule: {rule}\n";
            }
            
            var reducedIndex = _syntaxTree.AddNode(rule.Premise, rule.SemanticAction);

            var symbolsToPop = rule.NonEpsilonLength;
            for (var i = 0; i < symbolsToPop; i++)
            {
                _stateStack.Pop();
                _syntaxTree.AddChild(reducedIndex, _treeNodeStack.Pop());
            }

            _treeNodeStack.Push(reducedIndex);

            if (_table.GotoTable.TryGetValue(new StateSymbolTuple(_stateStack.Peek(), rule.Premise), out var gotoId))
            {
                _stateStack.Push(gotoId);
            }
            else
            {
                throw new Exception("Goto not found:" + (_stateStack.Peek(), rule.Premise));
            }
        }
    }
}
