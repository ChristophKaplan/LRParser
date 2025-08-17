using System;
using System.Collections.Generic;
using System.Linq;
using LogHelper;
using LRParser.CFG;

namespace LRParser.Parser
{
    public class Parser<T, N> where T : Enum where N : Enum
    {
        private readonly ContextFreeGrammar<T, N> _cfg;
        private readonly Table<T, N> _table;
        private string _parsingOutput = string.Empty;
        private readonly bool _showOutput;

        private Stack<int> stateStack = new Stack<int>();
        private Stack<ConcreteSyntaxTreeNode> treeStack = new Stack<ConcreteSyntaxTreeNode>();

        public Parser(ContextFreeGrammar<T, N> cfg, bool showOutput = false, bool debug = false)
        {
            _cfg = cfg;
            _showOutput = showOutput;
            var startItem = new LRItem(cfg.Productions[0], 0, new List<Symbol> { Symbol.Dollar });
            var states = new States<T, N>(startItem, _cfg, showOutput, debug);
            _table = new Table<T, N>(states, _cfg, showOutput);
            if (debug)
            {
                Logger.Log(_table.ToString());
            }
        }

        private void ResetStack()
        {
            stateStack.Clear();
            stateStack.Push(0);
            treeStack.Clear();
        }

        public ConcreteSyntaxTreeNode Parse(List<Symbol> input)
        {
            input.Add(Symbol.Dollar);
            ResetStack();

            var shouldContinue = true;
            while (shouldContinue)
            {
                //DEBUG
                //Logging.Log(treeStack.Aggregate("\nDEBUG: ", (current, tree) => tree.Symbol + " " +current ));
                var action = GetAction(input, out var pullEps);
                shouldContinue = ProcessAction(input, action, pullEps);
            }

            return treeStack.Pop();
        }

        private bool ProcessAction(List<Symbol> input, ParserAction action, bool pullEps)
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
                    Shift(input, action.StateOrProdId, pullEps);
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

        private ParserAction GetAction(List<Symbol> input, out bool pullEps)
        {
            pullEps = false;
            if (_table.ActionTable.TryGetValue(new StateSymbolTuple(stateStack.Peek(), input[0]), out var action))
            {
                return action;
            }

            if (_table.ActionTable.TryGetValue(new StateSymbolTuple(stateStack.Peek(), Symbol.Epsilon),
                    out var epsAction))
            {
                pullEps = true;
                return epsAction;
            }

            return ParserAction.Default;
        }

        private void Accept()
        {
            if (_showOutput)
            {
                _parsingOutput += "ACCEPT\n";
                Logger.Log(_parsingOutput);
            }
        }

        private void Error(List<Symbol> input)
        {
            var expected = _table.ExpectedSymbols(stateStack.Peek());
            if (_showOutput)
            {
                _parsingOutput +=
                    $"ERROR: cant parse \"{input[0]}\". current: {stateStack.Peek()}\n Expected Symbols: {expected.Aggregate("", (current, symbol) => current + symbol + " ")}\n";
            }

            //DEBUG
            /*foreach (var ex in expected) {
                _table.ActionTable.TryGetValue((stateStack.Peek(), ex), out var action);
                Logging.Log($"Expected: {ex}, Action: {action.Item1} {_cfg.Productions[action.Item2]} ");
            }*/
            throw new Exception($"Error:\n{_parsingOutput}");
        }

        private void Shift(List<Symbol> input, int shiftState, bool pullEps)
        {
            if (pullEps)
            {
                //TODO: Implement what to do here?
                return;
            }

            if (_showOutput)
            {
                _parsingOutput += $"SHIFT: {input[0]}, current:{stateStack.Peek()}, next state:{shiftState}\n";
            }

            stateStack.Push(shiftState);
            treeStack.Push(new ConcreteSyntaxTreeNode(input[0].Clone()));
            input.RemoveAt(0);
        }

        private void Reduce(int ruleId)
        {
            var rule = _cfg.Productions[ruleId];
            if (_showOutput)
            {
                _parsingOutput += $"REDUCE ({ruleId}), Rule: {rule}\n";
            }

            var reduced = new ConcreteSyntaxTreeNode(rule.Premise.Clone(), rule.SemanticAction);

            for (var i = 0; i < rule.Conclusion.Count(s => !s.IsEpsilon); i++)
            {
                stateStack.Pop();
                reduced.AddChild(treeStack.Pop());
            }

            treeStack.Push(reduced);

            if (_table.GotoTable.TryGetValue(new StateSymbolTuple(stateStack.Peek(), rule.Premise), out var gotoId))
            {
                stateStack.Push(gotoId);
            }
            else
            {
                throw new Exception("Goto not found:" + (stateStack.Peek(), rule.Premise));
            }
        }
    }
}
