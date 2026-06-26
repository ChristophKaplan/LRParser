using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

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

public enum DrTerminal { End }

public enum DrNonTerminal { S, A, B }

// A grammar whose only reduce/reduce conflict is on the $ (end-of-input)
// lookahead: S -> A | B, A -> epsilon, B -> epsilon. Empty input could reduce
// via either A or B. Used to verify the conflict detector no longer ignores $.
public sealed class DollarReduceReduceGrammar : ContextFreeGrammar<DrTerminal, DrNonTerminal>
{
    public DollarReduceReduceGrammar()
    {
        AddTerminalsAndNonTerminals();

        AddRule(Noop, DrNonTerminal.S, DrNonTerminal.A);
        AddRule(Noop, DrNonTerminal.S, DrNonTerminal.B);
        AddRule(Noop, DrNonTerminal.A, InternalSymbol.Epsilon);
        AddRule(Noop, DrNonTerminal.B, InternalSymbol.Epsilon);

        InsertStartProductionRule();
    }

    private static ILanguageObject Noop(Symbol[] rhs) => rhs.Length > 0 ? rhs[0].Attribute : null!;
}

public enum FsTerminal { a, b, c }

public enum FsNonTerminal { S, A, B, T }

// Grammar built to exercise FIRST-set computation directly:
//   S -> A B c        (FIRST should chain through nullable A and B to reach c)
//   A -> a | epsilon  (nullable)
//   B -> b | epsilon  (nullable)
//   T -> c B          (first symbol non-nullable: FIRST(T) must be {c} only)
// It subclasses ContextFreeGrammar directly so tests can query First(...) on a
// fully built grammar without constructing parser tables.
public sealed class FirstSetGrammar : ContextFreeGrammar<FsTerminal, FsNonTerminal>
{
    public FirstSetGrammar()
    {
        AddTerminalsAndNonTerminals();

        AddRule(Noop, FsNonTerminal.S, FsNonTerminal.A, FsNonTerminal.B, FsTerminal.c);
        AddRule(Noop, FsNonTerminal.A, FsTerminal.a);
        AddRule(Noop, FsNonTerminal.A, InternalSymbol.Epsilon);
        AddRule(Noop, FsNonTerminal.B, FsTerminal.b);
        AddRule(Noop, FsNonTerminal.B, InternalSymbol.Epsilon);
        AddRule(Noop, FsNonTerminal.T, FsTerminal.c, FsNonTerminal.B);

        InsertStartProductionRule();
    }

    public static Symbol Term(FsTerminal t) => new Symbol(t, SymbolType.Terminal);
    public static Symbol NonTerm(FsNonTerminal n) => new Symbol(n, SymbolType.NonTerminal);

    private static ILanguageObject Noop(Symbol[] rhs) => rhs.Length > 0 ? rhs[0].Attribute : null!;
}

public enum UndefTerminal { X }

public enum UndefNonTerminal { S, A, B }

// S -> A B, A -> X, but B is reachable and has no productions. Construction must
// fail fast rather than build a broken table.
public sealed class UndefinedNonTerminalGrammar : ContextFreeGrammar<UndefTerminal, UndefNonTerminal>
{
    public UndefinedNonTerminalGrammar()
    {
        AddTerminalsAndNonTerminals();

        AddRule(Noop, UndefNonTerminal.S, UndefNonTerminal.A, UndefNonTerminal.B);
        AddRule(Noop, UndefNonTerminal.A, UndefTerminal.X);

        InsertStartProductionRule();
    }

    private static ILanguageObject Noop(Symbol[] rhs) => rhs.Length > 0 ? rhs[0].Attribute : null!;
}

public enum UnprTerminal { X }

public enum UnprNonTerminal { S, A }

// S -> A, A -> A X. A can only derive via itself, so it never reaches a terminal
// string: A (and therefore S) is unproductive. Construction must fail fast.
public sealed class UnproductiveGrammar : ContextFreeGrammar<UnprTerminal, UnprNonTerminal>
{
    public UnproductiveGrammar()
    {
        AddTerminalsAndNonTerminals();

        AddRule(Noop, UnprNonTerminal.S, UnprNonTerminal.A);
        AddRule(Noop, UnprNonTerminal.A, UnprNonTerminal.A, UnprTerminal.X);

        InsertStartProductionRule();
    }

    private static ILanguageObject Noop(Symbol[] rhs) => rhs.Length > 0 ? rhs[0].Attribute : null!;
}

public enum EvTerminal { Ta }

public enum EvNonTerminal { S }

// Exercises evaluation of an epsilon production's semantic action: S -> a | epsilon.
// For empty input the S -> epsilon action must run and yield its value, even
// though that tree node has no children.
public sealed class EpsilonValueLang : Language<EvTerminal, EvNonTerminal>
{
    protected override TokenDefinition<EvTerminal>[] SetUpTokenDefinitions()
    {
        return new[] { new TokenDefinition<EvTerminal>(EvTerminal.Ta, "a") };
    }

    protected override void SetUpGrammar()
    {
        AddRule(PassThrough, EvNonTerminal.S, EvTerminal.Ta);
        AddRule(Empty, EvNonTerminal.S, InternalSymbol.Epsilon);
    }

    private static ILanguageObject PassThrough(Symbol[] rhs) => rhs[0].Attribute;
    private static ILanguageObject Empty(Symbol[] rhs) => new LexValue("empty");
}

public enum NpTerminal { Ta, Tb, Tc }

public enum NpNonTerminal { S, OptA, OptB }

// End-to-end language over the same nullable-prefix grammar so the FIRST fix can
// be verified through real parsing:
//   S -> OptA OptB c     OptA -> a | epsilon     OptB -> b | epsilon
// Input "c" parses only if A->epsilon reduces on lookahead c, which requires
// FIRST to chain through the nullable OptB to reach the terminal c.
public sealed class NullablePrefixLang : Language<NpTerminal, NpNonTerminal>
{
    protected override TokenDefinition<NpTerminal>[] SetUpTokenDefinitions()
    {
        return new[]
        {
            new TokenDefinition<NpTerminal>(NpTerminal.Ta, "a"),
            new TokenDefinition<NpTerminal>(NpTerminal.Tb, "b"),
            new TokenDefinition<NpTerminal>(NpTerminal.Tc, "c"),
        };
    }

    protected override void SetUpGrammar()
    {
        AddRule(SAction, NpNonTerminal.S, NpNonTerminal.OptA, NpNonTerminal.OptB, NpTerminal.Tc);
        AddRule(Noop, NpNonTerminal.OptA, NpTerminal.Ta);
        AddRule(Noop, NpNonTerminal.OptA, InternalSymbol.Epsilon);
        AddRule(Noop, NpNonTerminal.OptB, NpTerminal.Tb);
        AddRule(Noop, NpNonTerminal.OptB, InternalSymbol.Epsilon);
    }

    private static ILanguageObject SAction(Symbol[] rhs) => new LexValue("parsed");
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
