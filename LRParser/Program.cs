var logic = new PropositionalLogic.PropositionalLogic();

logic.Interpret(new []{
    "Int(A OR B)",
    "SwitchMany(Int(A OR B),B)",
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