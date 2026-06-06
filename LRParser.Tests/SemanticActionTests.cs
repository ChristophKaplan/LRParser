using ExNs = ExampleLang;
using DbgNs = LRParserExample;

namespace LRParser.Tests;

// Finding #6 (fixed): Rule05 used to return null for any non-"Int" type, which
// propagated out of TryParse and caused an NRE for callers doing .ToString().
// It now throws a clear exception instead of returning null.
public class SemanticActionTests
{
    [Fact]
    public void ExampleLang_IntAssignment_ReturnsIntValue()
    {
        var lang = new ExNs.ExampleLang();

        var result = lang.TryParse("Int A; A = 50;");

        var intValue = Assert.IsType<ExNs.IntValue>(result);
        Assert.Equal(50, intValue.Value);
    }

    [Fact]
    public void ExampleLang_FloatAssignment_Throws()
    {
        var lang = new ExNs.ExampleLang();

        // The grammar accepts this input but Float assignment is unsupported;
        // it should fail loudly rather than hand back null.
        Assert.Throws<Exception>(() => lang.TryParse("Float A; A = 50;"));
    }

    [Fact]
    public void DebugLang_IdentifierList_ParsesToFirstIdentifier()
    {
        var lang = new DbgNs.DebugLang();

        var result = lang.TryParse("A B C");

        Assert.Equal("A", result.ToString());
    }
}
