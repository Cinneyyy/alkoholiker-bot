namespace src.Rules.Opt;

public static class OptMgr
{
    private static string path;


    public static void SetPath(string path)
    {
        Directory.CreateDirectory(path);
        OptMgr.path = path; 
    }

    public static bool IsOptedOut(u64 userId)
        => File.Exists($"{path}/{userId}");

    public static void OptOut(u64 userId)
        => File.Create($"{path}/{userId}");

    public static void OptIn(u64 userId)
        => File.Delete($"{path}/{userId}");
}