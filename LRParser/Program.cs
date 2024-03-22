var logic = new PropositionalLogic.PropositionalLogic();

logic.Interpret(new []{
    "Int(P AND Q)",
    "Switch(Int(P AND Q),Q)",
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