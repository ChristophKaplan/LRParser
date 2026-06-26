using System.Reflection;
using LRParser.CFG;
using LRParser.Parser;          // StateManager, LRItem
using LRParser.LRParser.Parser; // Table, State, StateSymbolTuple
using DbgNs = LRParserExample;
using ExNs = ExampleLang;

namespace LRParser.Tests;

public class StateConstructionTests
{
    private static StateManager<T, N> BuildStates<T, N>(ContextFreeGrammar<T, N> cfg, bool lalr)
        where T : struct, Enum where N : struct, Enum
    {
        var startItem = new LRItem(cfg.Productions[0], 0, new List<Symbol> { Symbol.Dollar });
        return new StateManager<T, N>(startItem, cfg, false, false, lalr);
    }

    // Finding #2: the LALR merge loop removes states from the list while
    // iterating it by index, which can skip a state and leave two states that
    // share a core unmerged. After merging, no two states should share a core.
    [Fact]
    public void Lalr_DebugLang_HasNoDuplicateCoresAfterMerge()
    {
        var sm = BuildStates(new DbgNs.DebugLang(), lalr: true);
        AssertNoDuplicateCores(sm.States);
    }

    [Fact]
    public void Lalr_ExampleLang_HasNoDuplicateCoresAfterMerge()
    {
        var sm = BuildStates(new ExNs.ExampleLang(), lalr: true);
        AssertNoDuplicateCores(sm.States);
    }

    // Several states share one core here; the merge must collapse them all.
    [Fact]
    public void Lalr_ManySameCoreStates_AreFullyMerged()
    {
        var sm = BuildStates(new ManySameCoreGrammar(), lalr: true);
        AssertNoDuplicateCores(sm.States);
    }

    // Deterministic reproduction of #2: three states that share a core sit in
    // consecutive list slots. The old loop removed a merged state and advanced
    // the index, skipping the state that slid into the freed slot, leaving it
    // unmerged. All three must collapse into one with the union of lookaheads.
    [Fact]
    public void MergeStates_CollapsesThreeConsecutiveSameCoreStates()
    {
        var sm = BuildStates(new ManySameCoreGrammar(), lalr: true);

        var premise = new Symbol(McNonTerminal.X, SymbolType.NonTerminal);
        var body = new Symbol(McTerminal.C, SymbolType.Terminal);
        Production.SemanticActionDelegate noop = _ => null!;
        var production = new Production(noop, premise, body);

        State MakeState(int id, McTerminal lookahead)
        {
            // dot after the single body symbol => a completed item; the core is
            // (production, dotPosition) and is identical for all three states.
            var item = new LRItem(production, 1, new List<Symbol> { new Symbol(lookahead, SymbolType.Terminal) });
            return new State(new List<LRItem> { item }, id);
        }

        var states = new List<State>
        {
            MakeState(0, McTerminal.D),
            MakeState(1, McTerminal.E),
            MakeState(2, McTerminal.F),
        };

        var stateManagerType = typeof(StateManager<McTerminal, McNonTerminal>);
        stateManagerType.GetProperty("States")!.SetValue(sm, states);

        stateManagerType
            .GetMethod("MergeStates", BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null)!
            .Invoke(sm, null);

        Assert.Single(sm.States);
        Assert.Equal(3, sm.States[0].Items[0].LookAheadSymbols.Count);
    }

    // The canonical LR(1) path (lalr: false) skips state merging entirely and
    // was otherwise never exercised. It must still build a conflict-free table
    // and never have fewer states than the merged LALR machine.
    [Fact]
    public void CanonicalLr_DebugLang_BuildsAndHasAtLeastAsManyStatesAsLalr()
        => AssertCanonicalConsistentWithLalr(new DbgNs.DebugLang());

    [Fact]
    public void CanonicalLr_ExampleLang_BuildsAndHasAtLeastAsManyStatesAsLalr()
        => AssertCanonicalConsistentWithLalr(new ExNs.ExampleLang());

    private static void AssertCanonicalConsistentWithLalr<T, N>(ContextFreeGrammar<T, N> cfg)
        where T : struct, Enum where N : struct, Enum
    {
        var lalr = BuildStates(cfg, lalr: true);
        var canonical = BuildStates(cfg, lalr: false);

        Assert.True(canonical.States.Count >= lalr.States.Count,
            "Canonical LR(1) should never have fewer states than LALR.");

        // Both must yield a buildable (conflict-free) table.
        var lalrTable = new Table<T, N>(lalr, cfg);
        var canonicalTable = new Table<T, N>(canonical, cfg);
        Assert.NotEmpty(lalrTable.ActionTable);
        Assert.NotEmpty(canonicalTable.ActionTable);
    }

    private static void AssertNoDuplicateCores(IReadOnlyList<State> states)
    {
        for (var i = 0; i < states.Count; i++)
        {
            for (var j = i + 1; j < states.Count; j++)
            {
                Assert.False(
                    states[i].HasEqualCore(states[j]),
                    $"States {states[i].Id} and {states[j].Id} share a core but were not merged.");
            }
        }
    }

    // Finding #4: the parser has an unfinished epsilon-shift branch (pullEps).
    // It is only reachable if the ACTION table ever contains an entry keyed on
    // Symbol.Epsilon. This test verifies that, for an epsilon-using grammar, no
    // such entry is ever produced, i.e. the pullEps branch is currently dead.
    [Fact]
    public void Table_ForEpsilonGrammar_HasNoEpsilonKeyedAction()
    {
        var lang = new DbgNs.DebugLang();
        var sm = BuildStates(lang, lalr: true);
        var table = new Table<DbgNs.Terminal, DbgNs.NonTerminal>(sm, lang);

        Assert.DoesNotContain(table.ActionTable.Keys, key => key.Symbol.IsEpsilon);
    }

    // Finding #3: StateManager.ValidateStates overwrites its conflict flag each
    // iteration (so it reflects only the last state) and its return value is
    // discarded entirely. This checks whether it correctly reports that a
    // conflicting grammar has a conflict.
    [Fact]
    public void ValidateStates_ReportsExistingConflict()
    {
        var cfg = new ReduceReduceGrammar();
        var sm = BuildStates(cfg, lalr: true);

        // Ground truth: at least one state genuinely has a conflict.
        var anyConflict = sm.States.Any(s =>
        {
            var output = string.Empty;
            return s.HasConflict(ref output);
        });
        Assert.True(anyConflict, "Test grammar was expected to contain a reduce/reduce conflict.");

        var validate = typeof(StateManager<RrTerminal, RrNonTerminal>)
            .GetMethod("ValidateStates", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var reported = (bool)validate.Invoke(sm, null)!;

        Assert.True(reported, "ValidateStates did not report an existing conflict.");
    }

    // The reduce/reduce detector must not ignore $ as a lookahead: S -> A | B
    // with A -> epsilon and B -> epsilon conflict only on end-of-input.
    [Fact]
    public void HasConflict_DetectsReduceReduceOnDollarLookahead()
    {
        var sm = BuildStates(new DollarReduceReduceGrammar(), lalr: true);

        var anyConflict = sm.States.Any(s =>
        {
            var output = string.Empty;
            return s.HasConflict(ref output);
        });

        Assert.True(anyConflict, "Reduce/reduce conflict on $ was not detected.");
    }

    // A genuinely conflicting grammar must fail loudly when its table is built,
    // rather than silently producing a broken table.
    [Fact]
    public void Table_ForReduceReduceGrammar_ThrowsOnConflict()
    {
        var cfg = new ReduceReduceGrammar();
        var sm = BuildStates(cfg, lalr: true);

        Assert.Throws<Exception>(() => new Table<RrTerminal, RrNonTerminal>(sm, cfg));
    }
}
