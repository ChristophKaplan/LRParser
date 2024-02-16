using LRParser.PropositionalLogic;

PropositionalLogic logic = new PropositionalLogic();
Sentence sentence = logic.TryParse("P OR NOT Q");
Console.WriteLine(sentence +" = "+ sentence.Evaluate()); 
