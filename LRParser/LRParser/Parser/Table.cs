using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogHelper;
using LRParser.CFG;
using LRParser.Parser;

namespace LRParser.LRParser.Parser
{
    public class Table<T, N> where T : Enum where N : Enum
    {
        public Dictionary<StateSymbolTuple, ParserAction> ActionTable { get; } = new();
        public Dictionary<StateSymbolTuple, int> GotoTable { get; } = new();

        private readonly ContextFreeGrammar<T, N> _cfg;
        private StringBuilder _tableOutput = new();

        public Table(StateManager<T, N> stateManager, ContextFreeGrammar<T, N> cfg, bool showOutput = false)
        {
            _cfg = cfg;
            CreateTable(stateManager);
            if (showOutput)
            {
                Logger.Log($"Table:\n{_tableOutput.ToString()}");
            }
        }

        private void CreateTable(StateManager<T, N> stateManager)
        {
            foreach (var state in stateManager.States)
            {
                foreach (var item in state.Items)
                {
                    CreateEntry(state, item);
                }
            }
        }

        public List<Symbol> ExpectedSymbols(int state)
        {
            var result = new List<Symbol>();
            foreach (var entry in ActionTable)
            {
                if (entry.Key.StateId == state)
                {
                    result.Add(entry.Key.Symbol);
                }
            }
            return result;
        }

        private void CreateEntry(State state, LRItem item)
        {
            if (item.IsComplete)
            {
                CreateEntryComplete(state, item);
            }
            else
            {
                CreateEntryNonComplete(state, item);
            }
        }

        private void CreateEntryComplete(State state, LRItem item)
        {
            if (item.Production.Premise.Equals(_cfg.StartSymbol))
            {
                var symbolTuple = new StateSymbolTuple(state.Id, Symbol.Dollar);
                ActionTable[symbolTuple] = new ParserAction(ParserAction.Type.Accept, -1);
            }
            else
            {
                foreach (var symbol in item.LookAheadSymbols)
                {
                    var tuple = new StateSymbolTuple(state.Id, symbol);
                    var contained = ActionTable.TryGetValue(tuple, out var parserAction);

                    if (contained && parserAction.Action == ParserAction.Type.Shift)
                    {
                        _tableOutput.Append($"Shift reduce conflict, default to shift: {symbol}, State:{state.Id}\n");
                        continue;
                    }

                    if (contained && parserAction.Action == ParserAction.Type.Reduce)
                    {
                        var r1 = parserAction.StateOrProdId; //contained already
                        var r2 = _cfg.Productions.IndexOf(item.Production); //want to add but already contained

                        _tableOutput.Append($"Reduce/Reduce conflict: {symbol}\n {_cfg.Productions[r1]} vs. {_cfg.Productions[r2]} \n {state}");
                        throw new Exception(_tableOutput.ToString());
                    }

                    ActionTable[tuple] = new ParserAction(ParserAction.Type.Reduce,
                        _cfg.Productions.IndexOf(item.Production));
                }
            }
        }

        private void CreateEntryNonComplete(State state, LRItem item)
        {
            var symbol = item.CurrentSymbol;
            var symbolTuple = new StateSymbolTuple(state.Id, symbol);

            switch (symbol.Type)
            {
                case SymbolType.Terminal:
                {
                    if (ActionTable.TryGetValue(symbolTuple, out var parserAction) &&
                        parserAction.Action == ParserAction.Type.Reduce)
                    {
                        _tableOutput.Append( $"Shift reduce conflict, default to shift : {symbol} , State:{state.Id}\n");
                    }

                    if (!state.Transitions.TryGetValue(symbol, out var nextState))
                    {
                        _tableOutput.Append($"Cant find shift symbol {symbol} at state {state}!\n");
                        throw new Exception(_tableOutput.ToString());
                    }

                    ActionTable[symbolTuple] = new ParserAction(ParserAction.Type.Shift, nextState.Id);
                    break;
                }
                case SymbolType.NonTerminal:
                {
                    var nextState = state.Transitions[symbol];
                    GotoTable[symbolTuple] = nextState.Id;
                    break;
                }
            }
        }

        public override string ToString()
        {
            var s = "ACTION\n";
            foreach (var entry in ActionTable)
            {
                s += $"({entry.Key.StateId}, {entry.Key.Symbol}) -> {entry.Value}\n";
            }

            s += "\nGOTO\n";
            foreach (var entry in GotoTable)
            {
                s += $"({entry.Key}) -> {entry.Value}\n";
            }

            return s;
        }
    }
}