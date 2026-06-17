namespace src;

public static class Log
{
    public static void Out(string text)
    {
        text = $"[{App.GetTimeStr(DateTime.Now)}] {text}\n";
        Console.Write(text);
        File.AppendAllText(App.GetPath("log.txt"), text);
    }
}
