namespace src.CallHistory;

public static class CallStatistics
{
    public static void OnVoiceCallEnd(IEnumerable<(u64 id, u32 seconds)> participantData)
    {
        Directory.CreateDirectory(App.GetPath("call_stats/"));

        foreach((u64 id, u32 seconds) in participantData)
        {
            string path = App.GetPath($"call_stats/{id}");
            u32 stat = seconds;

            if(File.Exists(path))
                stat += u32.Parse(File.ReadAllText(path).Trim());

            File.WriteAllText(path, stat.ToString());
        }
    }

    public static u32 GetUserCallSeconds(u64 userId)
    {
        string path = App.GetPath($"call_stats/{userId}");
        return File.Exists(path)
            ? u32.Parse(File.ReadAllText(path).Trim())
            : 0u;
    }

    public static (u64 id, u32 seconds)[] GetAllCallSeconds()
        => Directory.GetFiles(App.GetPath($"call_stats/"))
            .Select(f => (
                id: u64.Parse(Path.GetFileName(f)),
                seconds: u32.Parse(File.ReadAllText(f).Trim())))
            .ToArray();

    public static void RemoveUser(u64 userId)
        => File.Delete(App.GetPath($"call_stats/{userId}"));
}
