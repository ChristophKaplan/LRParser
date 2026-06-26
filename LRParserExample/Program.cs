using LogHelper;
using LRParserExample;

namespace LRParserRun
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var other = new ExampleLang.ExampleLang();
            var languageObject = other.Parse("Int A; A = 50;");
            Logger.Log(languageObject.ToString());
            */
            
            var other = new DebugLang();
            var languageObject = other.Parse("A B C");
            Logger.Log(languageObject.ToString());
        }
    }
}
