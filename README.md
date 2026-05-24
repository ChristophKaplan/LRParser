# LRParser

A C# library for building LR / LALR(1) parsers from a programmatically declared grammar. Define terminals and non-terminals as `enum`s, attach regex-based token rules and C# semantic actions, and parse strings into a typed syntax tree.

## Features

- LR(1) and LALR(1) table construction (toggle via constructor flag)
- Regex-based lexer driven by a `TokenDefinition<T>` list
- Grammars expressed in plain C# using `enum`s for terminals/non-terminals
- Support for ε (epsilon) productions via `InternalSymbol.Epsilon`
- Per-rule semantic actions that build typed `ILanguageObject` results
- Concrete syntax tree available after parsing
- Optional verbose output and table dumping for debugging grammars

## Project layout

```
LRParser/
├── LRParser/              # The parser library (netstandard2.1 + net8.0)
│   ├── CFG/               # ContextFreeGrammar, Production, Symbol
│   ├── Lexer/             # Regex-based Lexer + TokenDefinition
│   ├── Parser/            # LR table construction, parser, syntax tree
│   └── Language/          # Language<T,N> base class + ILanguageObject
└── LRParserExample/       # Example console app using the library
    ├── ExampleLang.cs     # Tiny typed declaration/assignment language
    ├── DebugLang.cs       # Minimal grammar used for debugging
    └── Program.cs
```

## Requirements

- .NET 8.0 SDK (the library also targets `netstandard2.1`)
- The solution references an external `Logger` project at `..\..\Logger\Logger\Logger.csproj` (sibling to this repo's parent directory). Clone or adjust the reference before building.

## Building

```powershell
dotnet build LRParser\LRParser.sln
```

Run the example:

```powershell
dotnet run --project LRParserExample\LRParserExample.csproj
```

## Defining a language

A language is a class that derives from `Language<TTerminal, TNonTerminal>` and implements two methods: `SetUpTokenDefinitions` and `SetUpGrammar`.

```csharp
public enum Terminal { Type, Variable, Equals, Num, SemiColon }
public enum NonTerminal { LangObject, Declaration, Assigment }

public class ExampleLang : Language<Terminal, NonTerminal>
{
    private readonly Dictionary<string, string> _typeTable = new();

    protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions() => new[]
    {
        new TokenDefinition<Terminal>(Terminal.SemiColon, ";"),
        new TokenDefinition<Terminal>(Terminal.Equals,    "="),
        new TokenDefinition<Terminal>(Terminal.Num,       "\\d+"),
        new TokenDefinition<Terminal>(Terminal.Type,      "Int|Float"),
        new TokenDefinition<Terminal>(Terminal.Variable,  "[A-Z][a-z]*"),
    };

    protected override void SetUpGrammar()
    {
        AddRule(PassThrough, NonTerminal.LangObject,  NonTerminal.Declaration);
        AddRule(PassSecond,  NonTerminal.LangObject,  NonTerminal.Declaration, NonTerminal.Assigment);
        AddRule(PassThrough, NonTerminal.LangObject,  InternalSymbol.Epsilon);

        AddRule(Declare,     NonTerminal.Declaration, Terminal.Type, Terminal.Variable, Terminal.SemiColon);
        AddRule(Assign,      NonTerminal.Assigment,   Terminal.Variable, Terminal.Equals, Terminal.Num, Terminal.SemiColon);
    }

    private ILanguageObject PassThrough(Symbol[] rhs) => rhs[0].Attribute;
    private ILanguageObject PassSecond(Symbol[] rhs)  => rhs[1].Attribute;

    private ILanguageObject Declare(Symbol[] rhs)
    {
        var type = (LexValue)rhs[0].Attribute;
        var name = (LexValue)rhs[1].Attribute;
        _typeTable.Add(name.Value, type.Value);
        return name;
    }

    private ILanguageObject Assign(Symbol[] rhs)
    {
        var name = (LexValue)rhs[0].Attribute;
        var num  = (LexValue)rhs[2].Attribute;
        if (!_typeTable.TryGetValue(name.Value, out var type))
            throw new Exception($"Variable: {name} not declared");
        return type == "Int" ? new IntValue(int.Parse(num.Value)) : null;
    }
}
```

Parse a string:

```csharp
var lang = new ExampleLang();
var result = lang.TryParse("Int A; A = 50;");
Console.WriteLine(result);
```

## How it works

1. `Lexer<T>` walks the input and, at each position, tries each `TokenDefinition` in order; the first regex that matches produces a `Symbol` carrying its raw value and source position.
2. `ContextFreeGrammar<T, N>` collects productions; an implicit start production wrapping the first user rule is inserted automatically.
3. `StateManager<T, N>` builds the LR(1) item sets (or merges them into LALR sets when `isLaLr = true`) and `Table<T, N>` produces the `ACTION` and `GOTO` tables.
4. `Parser<T, N>.Parse` runs the standard shift/reduce loop against the tables, building a `ConcreteSyntaxTree` as it goes.
5. After accepting, the tree is evaluated bottom-up; each reduction invokes its `SemanticActionDelegate`, returning a typed `ILanguageObject` for the root.

To inspect the generated tables or trace parsing, flip the flags in `Language<T,N>`'s constructor (`showOutput`, `debug`, `isLaLr`).

## License

No license file is currently included in the repository.
