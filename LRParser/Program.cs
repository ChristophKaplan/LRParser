using PropositionalLogic;

var logic = new PropositionalLogic.PropositionalLogic();

var input = Console.ReadLine();

if (string.IsNullOrEmpty(input)) {
    input = "Int(Forget(P OR Q, P))";
}

var langObj = logic.TryParse(input);

if (langObj is Function function) {
    logic.ExecuteFunction(function);
}

var arg0 = "arg0";
var arg1 = "arg1";
var arg2 = "arg2";
var arg3 = "arg3";
    
Console.WriteLine(String.Format("|{0,5}|{1,5}|{2,5}|{3,5}|", arg0, arg1, arg2, arg3));