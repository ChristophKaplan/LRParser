namespace Helpers;

public class Logger {
    public static void Log(string message) {
#if UNITY_EDITOR || UNITY_STANDALONE
        UnityEngine.Debug.Log(message);
#else
        Console.WriteLine(message);
#endif
    }
}