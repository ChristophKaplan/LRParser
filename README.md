# LRParser

A C# library for building LR(1) / LALR(1) parsers from a grammar declared in plain C#. Define terminals and non-terminals as `enum`s, attach regex token rules and semantic actions, and parse strings into a typed result.

## Features

- LR(1) and LALR(1) table construction (toggle via constructor flag)
- Regex-based lexer with line/column tracking
- Grammars written in C# using `enum`s for terminals/non-terminals
- ε (epsilon) productions via `InternalSymbol.Epsilon`
- Per-rule semantic actions producing typed `ILanguageObject` results
- Optional verbose/table dumping for debugging grammars

## Layout

```
LRParser/
├── LRParser/          # The library (netstandard2.1 + net8.0)
│   ├── CFG/           # ContextFreeGrammar, Production, Symbol
│   ├── Lexer/         # Regex lexer + TokenDefinition
│   ├── Parser/        # LR table construction, parser, syntax tree
│   └── Language/      # Language<T,N> base class + ILanguageObject
├── LRParserExample/   # Example console app (ExampleLang, DebugLang)
└── LRParser.Tests/    # xUnit test suite
```

## Requirements

- .NET 8.0 SDK (the library also targets `netstandard2.1`)
- An external `Logger` project referenced at `..\..\Logger\Logger\Logger.csproj`. Clone or adjust the reference before building.

## Build, run, test

```powershell
dotnet build LRParser\LRParser.sln
dotnet run  --project LRParserExample\LRParserExample.csproj
dotnet test LRParser.Tests\LRParser.Tests.csproj
```

## Defining a language

Derive from `Language<TTerminal, TNonTerminal>` and implement `SetUpTokenDefinitions` and `SetUpGrammar`. Token rules are tried in order; the first match wins. Whitespace between tokens is skipped, and unrecognized characters raise a lexer error.

```csharp
public enum Terminal { Type, Variable, Equals, Num, SemiColon }
public enum NonTerminal { LangObject, Declaration, Assignment }

public class ExampleLang : Language<Terminal, NonTerminal>
{
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
        AddRule(PassSecond,  NonTerminal.LangObject,  NonTerminal.Declaration, NonTerminal.Assignment);
        AddRule(PassThrough, NonTerminal.LangObject,  InternalSymbol.Epsilon);
        AddRule(Declare,     NonTerminal.Declaration, Terminal.Type, Terminal.Variable, Terminal.SemiColon);
        AddRule(Assign,      NonTerminal.Assignment,  Terminal.Variable, Terminal.Equals, Terminal.Num, Terminal.SemiColon);
    }

    private ILanguageObject PassThrough(Symbol[] rhs) => rhs[0].Attribute;
    private ILanguageObject PassSecond(Symbol[] rhs)  => rhs[1].Attribute;
    // ... Declare / Assign build the typed result ...
}
```

```csharp
var lang = new ExampleLang();
Console.WriteLine(lang.Parse("Int A; A = 50;"));

// Non-throwing variant:
if (lang.TryParse("Int A; A = 50;", out var result))
    Console.WriteLine(result);
```

`Parse` throws `LexerException` or `ParseException` (which exposes the source position and the expected symbols) on malformed input. `TryParse` returns `false` instead. A single language instance is safe to reuse across multiple `Parse`/`TryParse` calls; override `ResetState()` to clear any per-parse state (e.g. a symbol table).

## How it works

1. `Lexer<T>` tokenizes the input (first matching `TokenDefinition` wins).
2. `ContextFreeGrammar<T,N>` collects productions and wraps the first rule in an implicit start production.
3. `StateManager<T,N>` builds the LR(1) item sets (merged into LALR sets when `isLaLr = true`); `Table<T,N>` produces the `ACTION`/`GOTO` tables.
4. `Parser<T,N>.Parse` runs the shift/reduce loop, building a `ConcreteSyntaxTree`.
5. The tree is evaluated bottom-up, invoking each rule's semantic action to produce the typed result.

Flip `showOutput` / `debug` / `isLaLr` in the `Language<T,N>` constructor to trace parsing or dump tables.

## License

Licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).

Copyright © 2026 Christoph Kaplan.
