using System.Text;
using LRParser.CFG;
using LRParser.Parser;          // StateManager, LRItem
using LRParser.LRParser.Parser; // Table
using DbgNs = LRParserExample;
using ExNs = ExampleLang;

namespace LRParser.Tests;

// Golden-snapshot regression tests for the generated LR tables. These pin down
// the exact ACTION/GOTO tables (and state count) so that performance refactors
// of the table generator can be proven to preserve behaviour.
public class TableSnapshotTests
{
    private static string Lines(params string[] lines) => string.Concat(lines.Select(l => l + "\n"));

    private static string Snapshot<T, N>(ContextFreeGrammar<T, N> cfg)
        where T : struct, Enum where N : struct, Enum
    {
        var startItem = new LRItem(cfg.Productions[0], 0, new List<Symbol> { Symbol.Dollar });
        var states = new StateManager<T, N>(startItem, cfg, false, false, true);
        var table = new Table<T, N>(states, cfg);

        var sb = new StringBuilder();
        sb.Append("STATES=").Append(states.States.Count).Append('\n');

        sb.Append("ACTION\n");
        foreach (var line in table.ActionTable
                     .Select(e => $"({e.Key.StateId},{e.Key.Symbol}) -> {e.Value}")
                     .OrderBy(s => s, StringComparer.Ordinal))
        {
            sb.Append(line).Append('\n');
        }

        sb.Append("GOTO\n");
        foreach (var line in table.GotoTable
                     .Select(e => $"({e.Key.StateId},{e.Key.Symbol}) -> {e.Value}")
                     .OrderBy(s => s, StringComparer.Ordinal))
        {
            sb.Append(line).Append('\n');
        }

        return sb.ToString();
    }

    [Fact]
    public void DebugLang_TableSnapshot_IsStable()
    {
        var expected = Lines(
            "STATES=5",
            "ACTION",
            "(0,Dollar) -> Reduce to production 4",
            "(0,Identifier) -> Shift, Next state: 3",
            "(1,Dollar) -> Accept",
            "(1,Identifier) -> Shift, Next state: 3",
            "(2,Dollar) -> Reduce to production 1",
            "(2,Identifier) -> Reduce to production 1",
            "(3,Dollar) -> Reduce to production 3",
            "(3,Identifier) -> Reduce to production 3",
            "(4,Dollar) -> Reduce to production 2",
            "(4,Identifier) -> Reduce to production 2",
            "GOTO",
            "(0,LangObject) -> 4",
            "(0,List) -> 1",
            "(1,LangObject) -> 2");

        Assert.Equal(expected, Snapshot(new DbgNs.DebugLang()));
    }

    [Fact]
    public void ExampleLang_TableSnapshot_IsStable()
    {
        var expected = Lines(
            "STATES=11",
            "ACTION",
            "(0,Dollar) -> Reduce to production 3",
            "(0,Type) -> Shift, Next state: 8",
            "(1,Dollar) -> Accept",
            "(10,Dollar) -> Reduce to production 4",
            "(10,Variable) -> Reduce to production 4",
            "(2,Dollar) -> Reduce to production 1",
            "(2,Variable) -> Shift, Next state: 4",
            "(3,Dollar) -> Reduce to production 2",
            "(4,Equals) -> Shift, Next state: 5",
            "(5,Num) -> Shift, Next state: 6",
            "(6,SemiColon) -> Shift, Next state: 7",
            "(7,Dollar) -> Reduce to production 5",
            "(8,Variable) -> Shift, Next state: 9",
            "(9,SemiColon) -> Shift, Next state: 10",
            "GOTO",
            "(0,Declaration) -> 2",
            "(0,LangObject) -> 1",
            "(2,Assigment) -> 3");

        Assert.Equal(expected, Snapshot(new ExNs.ExampleLang()));
    }
}
