using PropositionalLogic;
/*
var logic = new PropositionalLogic.PropositionalLogic();
var input = Console.ReadLine();

if (string.IsNullOrEmpty(input)) {
    input = "Int(Forget(P OR Q, P))";
}

var langObj = (ILanguageObject)logic.TryParse(input);

if (langObj is Function function) {
    logic.ExecuteFunction(function);
}*/

var other = new OtherLang.OtherLang();
var languageObject = other.TryParse("Int A");
Console.WriteLine(languageObject);
