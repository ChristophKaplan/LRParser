using System;
using System.Collections.Generic;
using System.Linq;
using LogHelper;
using LRParser.CFG;

namespace LRParser.Parser
{
    public class Table<T, N> where T : Enum where N : Enum
    {
        public Dictionary<StateSymbolTuple, ParserAction> ActionTable { get; } = new();
        public Dictionary<StateSymbolTuple, int> GotoTable { get; } = new();

        private readonly ContextFreeGrammar<T, N> _cfg;
        private string _tableOutput = string.Empty;

        public Table(States<T, N> states, ContextFreeGrammar<T, N> cfg, bool showOutput = false)
        {
            _cfg = cfg;
            CreateTable(states);
            if (showOutput)
            {
                Logger.Log($"Table:\n{_tableOutput}");
            }
        }

        private void CreateTable(States<T, N> states)
        {
            foreach (var state in states.StateList.Values)
            {
                foreach (var item in state.Items)
                {
                    CreateEntry(state, item);
                }
            }
        }

        public List<Symbol> ExpectedSymbols(int state)
        {
            return ActionTable
                .Where(x => x.Key.State == state)
                .Select(x => x.Key.Symbol)
                .ToList();
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
                    var contained = ActionTable.ContainsKey(tuple);

                    if (contained && ActionTable[tuple].Action == ParserAction.Type.Shift)
                    {
                        _tableOutput += $"Shift reduce conflict, default to shift: {symbol}, State:{state.Id}\n";
                        continue;
                    }

                    if (contained && ActionTable[tuple].Action == ParserAction.Type.Reduce)
                    {
                        var r1 = ActionTable[tuple].StateOrProdId;
                        var r2 = _cfg.Productions.IndexOf(item.Production);

                        _tableOutput +=
                            $"Reduce/Reduce conflict: {symbol}\n {_cfg.Productions[r1]} vs. {_cfg.Productions[r2]} \n {state}";
                        throw new Exception(_tableOutput);
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
                    if (ActionTable.ContainsKey(symbolTuple) &&
                        ActionTable[symbolTuple].Action == ParserAction.Type.Reduce)
                    {
                        _tableOutput += $"Shift reduce conflict, default to shift : {symbol} , State:{state.Id}\n";
                    }

                    if (!state.Transitions.TryGetValue(symbol, out var nextState))
                    {
                        _tableOutput += $"Cant find shift symbol {symbol} at state {state}!\n";
                        throw new Exception(_tableOutput);
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
                s += $"({entry.Key.State}, {entry.Key.Symbol}) -> {entry.Value}\n";
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
