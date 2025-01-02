class ExampleClass
{
    static void Main(string[] args)
    {
        var other = new ExampleLang.ExampleLang();
        var languageObject = other.TryParse("Int A; A = 50;");
        Logger.Logger.Log(languageObject.ToString());
    }
}

