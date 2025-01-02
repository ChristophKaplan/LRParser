using System;
using LogHelper;

namespace LRParserRun
{
    class Program
    {
        static void Main(string[] args)
        {
            var other = new ExampleLang.ExampleLang();
            var languageObject = other.TryParse("Int A; A = 50;");
            Logger.Log(languageObject.ToString());
        }
    }
}
