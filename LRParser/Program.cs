using PropositionalLogic;

var logic = new PropositionalLogic.PropositionalLogic();

/*
var c = InputCreator.GeneratePropositionalSentences(3);
c.ForEach(x => {
    Console.WriteLine(x);
});
Console.WriteLine("\n");
logic.Interpret(c.ToArray());
*/

/*logic.Interpret(new []{
    "Mod(Forget(A AND B, B))",
    "Mod(A AND B)",
    "SwitchMany(Mod(A AND B), B)",
});*/

/*
logic.Interpret(new []{
    "Int(Forget((A OR B) AND C, A))",
    "Int(SkepForget((A OR B) AND C, A))",
    "Int(Forget((A AND B) OR C, A))",
    "Int(SkepForget((A AND B) OR C, A))"
});
*/


logic.Interpret(new []{
    "Int((A OR B) AND C, Simplify(Forget((A OR B) AND C, A)))",
    
    "Int(Simplify(Forget((A OR B) AND C, A)))",
});


/*
logic.Interpret(new []{
    "Simplify(Forget((A OR B) AND C, A))",
    "Simplify(Forget((A OR B) AND C, B))",
    "Simplify(Forget((A OR B) AND C, C))",
});

logic.Interpret(new []{
    "Simplify(SkepForget((A AND B) OR C, A))",
    "Simplify(SkepForget((A AND B) OR C, B))",
    "Simplify(SkepForget((A AND B) OR C, C))",
});
*/

/*
    "Int((P AND Q) OR Z)",
   "Int(Forget((P AND Q) OR Z, Z))",
   "Int(Simplify(Forget((P AND Q) OR Z, Z)))",
*/

/*    
var other = new OtherLang.OtherLang();
var languageObject = other.TryParse("Int A; A = 50;");
Console.WriteLine(languageObject);
*/