using LRParser.Language;

namespace PropositionalLogic;

public enum LogicSymbols {
    AND,
    OR,
    NOT
}

public class Function : ILanguageObject {
    public readonly string Func;
    public readonly ILanguageObject[] Parameters;

    public Function(string func, params ILanguageObject[] parameters) {
        Func = func;
        Parameters = parameters;
    }
}

public abstract class Sentence : ILanguageObject {
    private Sentence _parent;
    public readonly List<Sentence> Children = new();

    public void AddChild(Sentence sentence) {
        Children.Add(sentence);
        sentence._parent = this;
    }
    
    public void InsertChild(int index, Sentence sentence) {
        Children.Insert(index,sentence);
        sentence._parent = this;
    }
    
    public void Reparent(Sentence parentOfThis) {
        
        if (parentOfThis._parent == null) {
            return;
        }
        
        Sentence parent = parentOfThis._parent;
        Sentence found = null;
        foreach (var childInParent in parent.Children) {
            if (childInParent.Equals(parentOfThis)) {
                found = childInParent;
            }
        }

        var index = parent.Children.IndexOf(found);
        parent.Children.RemoveAt(index);
        parent.InsertChild(index, this);
    }

    public bool IsAtomComplexRelation(Sentence sentence, out AtomicSentence atomicSentence, out ComplexSentence complex) {
        atomicSentence = null;
        complex = null;
            
        if (sentence is AtomicSentence) return false;

        var lhs = sentence.Children[0];
        var rhs = sentence.Children[1];

        if((lhs is AtomicSentence && rhs is AtomicSentence) || (lhs is ComplexSentence && rhs is ComplexSentence)) return false;

        if (lhs is AtomicSentence lhs1 && rhs is ComplexSentence rhs1) {
            atomicSentence = lhs1;
            complex = rhs1;
        }
        else if (rhs is AtomicSentence rhs2 && lhs is ComplexSentence lhs2) {
            atomicSentence = rhs2;
            complex = lhs2;
        }

        return true;
    }
    
    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override string ToString() {
        if (this is AtomicSentence atomicSentence) {
            return atomicSentence.Symbol;
        }

        if (this is ComplexSentence complexSentence) {
            if (complexSentence.Operator.Equals("NOT")) {
                return $"{complexSentence.Operator} {complexSentence.Children[0]}";
            }

            return $"({complexSentence.Children[0]} {complexSentence.Operator} {complexSentence.Children[1]})";
        }

        return "Sentence";
    }
}

public class AtomicSentence : Sentence {
    public string Symbol;
    public bool IsTruthValue { get => Tautology || Falsum; }
    public bool Tautology { get => Symbol.Equals("True"); }
    public bool Falsum { get => Symbol.Equals("False"); }
    
    public AtomicSentence(string symbol) {
        Symbol = symbol;
    }
    public AtomicSentence(LexValue symbol) {
        Symbol = symbol.Value;
    }
}

public class ComplexSentence : Sentence {
    public readonly LogicSymbols Operator;

    public ComplexSentence(Sentence p, LogicSymbols @operator, Sentence q) {
        Operator = @operator;
        AddChild(p);
        AddChild(q);
    }

    public ComplexSentence(LogicSymbols @operator, Sentence p) {
        Operator = @operator;
        AddChild(p);
    }
    
    public ComplexSentence(LogicSymbols @operator) {
        Operator = @operator;
    }
}