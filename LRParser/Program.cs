var logic = new PropositionalLogic.PropositionalLogic();
var input = Console.ReadLine();

if (string.IsNullOrEmpty(input)) {
    input = "Int(Simplify(Forget((P AND Q) OR Z, Z)))";
}

var langObj = logic.TryParse(input);

if (langObj is PropositionalLogic.Function function) {
    logic.ExecuteFunction(function);
}

/*    
var other = new OtherLang.OtherLang();
var languageObject = other.TryParse("Int A; A = 50;");
Console.WriteLine(languageObject);
*/