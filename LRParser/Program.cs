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
    "(A OR B) AND C",
    "A OR (B AND C)"
});



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