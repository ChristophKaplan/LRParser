using LRParser.CFG;

namespace LRParser.Parser;

public class Table <T, N> where T : Enum where N : Enum{
    private readonly ContextFreeGrammar<T, N> _cfg;
    public Table(States<T, N> states, ContextFreeGrammar<T, N> cfg) {
        _cfg = cfg;
        foreach (var state in states.StateList) {
            foreach (var item in state.Items) {
                CreateEntry(state, item);
            }
        }
        
        Console.WriteLine(this);
    }
    
    public Dictionary<(int, Symbol), (ParserAction, int)> ActionTable {
        get;
    } = new();

    public Dictionary<(int, Symbol), int> GotoTable {
        get;
    } = new();

    private void CreateEntry(State state, LRItem item) {
        if (item.IsComplete) {
            if (item.Production.Premise.Equals(_cfg.StartSymbol)) {
                ActionTable[(state.Id, Symbol.Dollar)] = (ParserAction.Accept, -1);
            }
            else {
                foreach (var symbol in item.LookAheadSymbols) {
                    if (ActionTable.ContainsKey((state.Id, symbol)) && ActionTable[(state.Id, symbol)].Item1 == ParserAction.Shift) {
                        Console.WriteLine($"shift reduce conflict, default to shift: {symbol}");
                        continue;
                    }

                    if (ActionTable.ContainsKey((state.Id, symbol)) && ActionTable[(state.Id, symbol)].Item1 == ParserAction.Reduce) {
                        throw new Exception($"reduce/reduce conflict : {symbol}");
                    }

                    ActionTable[(state.Id, symbol)] = (ParserAction.Reduce, _cfg.Productions.IndexOf(item.Production));
                }
            }
        }
        else {
            var symbol = item.CurrentSymbol;
            switch (symbol.Type) {
                case SymbolType.Terminal: {
                    if (ActionTable.ContainsKey((state.Id, symbol)) && ActionTable[(state.Id, symbol)].Item1 == ParserAction.Reduce) {
                        Console.WriteLine($"shift reduce conflict, default to shift : {symbol}");
                    }

                    if (!state.Transitions.TryGetValue(symbol, out var nextState)) {
                        throw new Exception($"cant find shift symbol {symbol} at state {state}!");
                    }

                    ActionTable[(state.Id, symbol)] = (ParserAction.Shift, nextState.Id);
                    break;
                }
                case SymbolType.NonTerminal: {
                    var nextState = state.Transitions[symbol];
                    GotoTable[(state.Id, symbol)] = nextState.Id;
                    break;
                }
            }
        }
    }

    public override string ToString() {
        var s = "ACTION\n";
        foreach (var entry in ActionTable) {
            s += $"({entry.Key.Item1}, {entry.Key.Item2}) -> {entry.Value}\n";
        }

        s += "\nGOTO\n";
        foreach (var entry in GotoTable) {
            s += $"({entry.Key}) -> {entry.Value}\n";
        }

        return s;
    }
}