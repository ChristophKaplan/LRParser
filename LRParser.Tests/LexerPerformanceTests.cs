using System.Diagnostics;
using System.Text;
using LRParser.Lexer;
using Xunit.Abstractions;

namespace LRParser.Tests;

// Performance characterisation of the lexer. The interesting case is input that
// does NOT contain some token (here: '='), which forces that rule's regex to
// scan ahead and fail at every position unless it is anchored to the scan
// position -- the O(n^2) trap this test exists to catch.
public class LexerPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public LexerPerformanceTests(ITestOutputHelper output) => _output = output;

    private enum Tok { Id, Eq }

    private const int SmallTokens = 2_500;
    private const int LargeTokens = 20_000;
    private const int SizeFactor = LargeTokens / SmallTokens;

    private static Lexer<Tok> MakeLexer() => new(
        new TokenDefinition<Tok>(Tok.Eq, "="),
        new TokenDefinition<Tok>(Tok.Id, "[A-Z][a-z]*"));

    private static string MakeInput(int tokenCount)
    {
        var sb = new StringBuilder(tokenCount * 3);
        for (var i = 0; i < tokenCount; i++)
        {
            sb.Append("Ab ");
        }

        return sb.ToString();
    }

    [Fact]
    public void Tokenize_ScalesLinearlyWithInputSize()
    {
        var lexer = MakeLexer();
        for (var i = 0; i < 20; i++)
        {
            lexer.Tokenize(MakeInput(2_000)); // warm up the JIT tiers and the compiled regexes
        }

        var small = NanosecondsPerChar(lexer, SmallTokens);
        var large = NanosecondsPerChar(lexer, LargeTokens);
        var growth = large / small;

        _output.WriteLine(
            $"{SmallTokens} tokens: {small:F1} ns/char | {LargeTokens} tokens: {large:F1} ns/char | growth={growth:F2}");

        // Linear tokenizing holds ns/char roughly flat as input grows (measured
        // ~1.6x over this range, from cache and allocation effects); dropping the
        // \G anchor makes it scale with input size instead (measured ~7.2x).
        // Comparing cost per char over one wide size range, rather than wall
        // clock across a single doubling, keeps the signal clear of noise.
        Assert.True(growth < 4.0,
            $"Tokenize appears worse than linear: cost per char grew {growth:F2}x over a {SizeFactor}x larger input.");
    }

    private static double NanosecondsPerChar(Lexer<Tok> lexer, int tokenCount)
    {
        var input = MakeInput(tokenCount);
        var best = BestOf(5, () => lexer.Tokenize(input));
        return best.TotalMilliseconds * 1_000_000.0 / input.Length;
    }

    // The minimum of repeated runs is the robust estimator: GC pauses and
    // scheduler noise only ever add time, and a single sample of a ~20ms
    // operation sits well inside that noise.
    private static TimeSpan BestOf(int repetitions, Action action)
    {
        var best = TimeSpan.MaxValue;
        for (var i = 0; i < repetitions; i++)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            if (sw.Elapsed < best)
            {
                best = sw.Elapsed;
            }
        }

        return best;
    }
}
