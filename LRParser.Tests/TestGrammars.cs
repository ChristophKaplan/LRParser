using LRParser.CFG;
using LRParser.Language;

namespace LRParser.Tests;

public enum RrTerminal { X, Y }

public enum RrNonTerminal { P, S, A, B }

// A grammar with a deliberate reduce/reduce conflict on a non-$ lookahead:
//   P -> S Y
//   S -> A | B
//   A -> X
//   B -> X
// In the state reached after shifting X, both [A -> X .] and [B -> X .] are
// complete with lookahead Y, which is a reduce/reduce conflict.
//
// It subclasses ContextFreeGrammar directly (not Language) so we can build the
// grammar without the Language pipeline eagerly constructing parser tables
// (which would throw on the conflict before we can inspect it).
public sealed class ReduceReduceGrammar : ContextFreeGrammar<RrTerminal, RrNonTerminal>
{
    public ReduceReduceGrammar()
    {
        AddTerminalsAndNonTerminals();

        AddRule(Noop, RrNonTerminal.P, RrNonTerminal.S, RrTerminal.Y);
        AddRule(Noop, RrNonTerminal.S, RrNonTerminal.A);
        AddRule(Noop, RrNonTerminal.S, RrNonTerminal.B);
        AddRule(Noop, RrNonTerminal.A, RrTerminal.X);
        AddRule(Noop, RrNonTerminal.B, RrTerminal.X);

        InsertStartProductionRule();
    }

    private static ILanguageObject Noop(Symbol[] rhs) => rhs.Length > 0 ? rhs[0].Attribute : null!;
}

public enum McTerminal { A, B, G, C, D, E, F, H }

public enum McNonTerminal { S, X, Y }

// A grammar engineered to produce THREE distinct LR(1) states that share the
// SAME core { X -> C ., Y -> C . } reached through different prefixes (a/b/g),
// each carrying pairwise-different lookaheads so none of them collapse during
// state generation. LALR must merge all three into a single state. This
// exercises the merge path where more than two states share a core, which is
// the scenario the (now fixed) index-skip bug could leave unmerged.
//
//   a C : X->C.{d}  Y->C.{e}
//   b C : X->C.{e}  Y->C.{d}
//   g C : X->C.{f}  Y->C.{h}
public sealed class ManySameCoreGrammar : ContextFreeGrammar<McTerminal, McNonTerminal>
{
    public ManySameCoreGrammar()
    {
        AddTerminalsAndNonTerminals();

        AddRule(Noop, McNonTerminal.S, McTerminal.A, McNonTerminal.X, McTerminal.D);
        AddRule(Noop, McNonTerminal.S, McTerminal.A, McNonTerminal.Y, McTerminal.E);
        AddRule(Noop, McNonTerminal.S, McTerminal.B, McNonTerminal.X, McTerminal.E);
        AddRule(Noop, McNonTerminal.S, McTerminal.B, McNonTerminal.Y, McTerminal.D);
        AddRule(Noop, McNonTerminal.S, McTerminal.G, McNonTerminal.X, McTerminal.F);
        AddRule(Noop, McNonTerminal.S, McTerminal.G, McNonTerminal.Y, McTerminal.H);
        AddRule(Noop, McNonTerminal.X, McTerminal.C);
        AddRule(Noop, McNonTerminal.Y, McTerminal.C);

        InsertStartProductionRule();
    }

    private static ILanguageObject Noop(Symbol[] rhs) => rhs.Length > 0 ? rhs[0].Attribute : null!;
}
