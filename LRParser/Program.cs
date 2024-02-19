using PropositionalLogic;

var logic = new PropositionalLogic.PropositionalLogic();
var langObj = logic.TryParse("Mod(Forget(P OR Q, P))");

if(langObj is Sentence sentence) logic.EvaluateTruthTable(sentence);
if(langObj is Function function) logic.ExecuteFunction(function);