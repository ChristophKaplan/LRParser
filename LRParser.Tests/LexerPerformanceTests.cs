using System.Diagnostics;
using System.Text;
using LRParser.Lexer;
using Xunit.Abstractions;

namespace LRParser.Tests;

// Performance characterisation of the lexer. The interesting case is input that
// does NOT contain some token (here: '='), which forces that rule's regex to
// scan ahead and fail at every position.
public class LexerPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public LexerPerformanceTests(ITestOutputHelper output) => _output = output;

    private enum Tok { Id, Eq }

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
        lexer.Tokenize(MakeInput(2000)); // warm up JIT / regex

        const int n = 15000;
        var inputN = MakeInput(n);
        var input2N = MakeInput(n * 2);

        var timeN = Time(() => lexer.Tokenize(inputN));
        var time2N = Time(() => lexer.Tokenize(input2N));

        var ratio = time2N.TotalMilliseconds / timeN.TotalMilliseconds;
        _output.WriteLine(
            $"N={n}: {timeN.TotalMilliseconds:F1} ms | 2N={n * 2}: {time2N.TotalMilliseconds:F1} ms | ratio={ratio:F2}");

        // Linear scaling => ~2x for double the input; quadratic => ~4x.
        Assert.True(ratio < 3.0,
            $"Tokenize appears worse than linear (2N/N time ratio = {ratio:F2}); expected ~2x.");
    }

    private static TimeSpan Time(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return sw.Elapsed;
    }
}
