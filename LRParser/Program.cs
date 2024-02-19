using PropositionalLogic;

var logic = new PropositionalLogic.PropositionalLogic();
IPropositionalLanguage langObj = logic.TryParse("Forget(P OR Q,P)");
logic.GenerateInterpretations();

if(langObj is Sentence sentence) logic.EvaluateTruthTable(sentence);
if(langObj is Function function) logic.ExecuteFunction(function);