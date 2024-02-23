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