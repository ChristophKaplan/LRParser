using PropositionalLogic;

var logic = new PropositionalLogic.PropositionalLogic();
Sentence sentence = logic.TryParse("Mod(P OR Q)");
logic.GenerateInterpretations();
logic.EvaluateTruthTable(sentence);