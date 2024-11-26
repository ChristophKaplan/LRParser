using Helpers;

class ExampleClass
{
    static void Run(string[] args)
    {
        var other = new ExampleLang.ExampleLang();
        var languageObject = other.TryParse("Int A; A = 50;");
        Logger.Log(languageObject.ToString());
    }
}

