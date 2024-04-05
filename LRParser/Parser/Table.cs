using LRParser.CFG;

namespace LRParser.Parser;

public class Table <T, N> where T : Enum where N : Enum{
    public Dictionary<(int, Symbol), (ParserAction, int)> ActionTable { get; } = new();
    public Dictionary<(int, Symbol), int> GotoTable { get; } = new();
    
    private readonly ContextFreeGrammar<T, N> _cfg;
    private string _tableOutput = string.Empty;
    
    public Table(States<T, N> states, ContextFreeGrammar<T, N> cfg, bool showOutput = false) {
        _cfg = cfg;
        CreateTable(states);
        if(showOutput) Console.WriteLine($"Table:\n{_tableOutput}");
    }

    private void CreateTable(States<T, N> states) {
        foreach (var state in states.StateList) {
            foreach (var item in state.Items) {
                CreateEntry(state, item);
            }
        }
    }

    public List<Symbol> ExpectedSymbols(int state) {
        return ActionTable.Where(x => x.Key.Item1 == state).Select(x => x.Key.Item2).ToList();
    }
    
    private void CreateEntry(State state, LRItem item) {
        if (item.IsComplete) {
            if (item.Production.Premise.Equals(_cfg.StartSymbol)) {
                ActionTable[(state.Id, Symbol.Dollar)] = (ParserAction.Accept, -1);
            }
            else {
                foreach (var symbol in item.LookAheadSymbols) {
                    if (ActionTable.ContainsKey((state.Id, symbol)) && ActionTable[(state.Id, symbol)].Item1 == ParserAction.Shift) {
                        _tableOutput += $"Shift reduce conflict, default to shift: {symbol}\n";
                        continue;
                    }

                    if (ActionTable.ContainsKey((state.Id, symbol)) && ActionTable[(state.Id, symbol)].Item1 == ParserAction.Reduce) {
                        int r1 = ActionTable[(state.Id, symbol)].Item2;
                        int r2 = _cfg.Productions.IndexOf(item.Production);
                        
                        _tableOutput += $"Reduce/Reduce conflict: {symbol}\n {_cfg.Productions[r1]} vs. {_cfg.Productions[r2]} \n {state}";
                        throw new Exception(_tableOutput);
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
                        _tableOutput += $"Shift reduce conflict, default to shift : {symbol}\n";
                    }

                    if (!state.Transitions.TryGetValue(symbol, out var nextState)) {
                        _tableOutput += $"Cant find shift symbol {symbol} at state {state}!\n";
                        throw new Exception(_tableOutput);
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