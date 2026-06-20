namespace src;

public static class UserCache
{
    public static readonly string cachePath = App.GetPath("cache/global_names");


    public static async Task<string> GetNameAsync(u64 userId)
    {
        string path = $"{cachePath}/userId";
        if(File.Exists(path))
            return File.ReadAllText(path).Trim();

        return await UpdateNameAsync(userId);
    }

    public static async Task<string> UpdateNameAsync(u64 userId)
    {
        Directory.CreateDirectory(cachePath);

        string name = await GetNameViaRestClientAsync(userId);
        File.WriteAllText($"{cachePath}/{userId}", name);

        return name;
    }

    public static async Task UpdateAllNamesAsync()
    {
        foreach(string path in Directory.GetFiles(cachePath))
            File.WriteAllText(path, await GetNameViaRestClientAsync(u64.Parse(Path.GetFileName(path))));
    }

    public static string GetName(u64 userId)
        => GetNameAsync(userId).GetAwaiter().GetResult();

    public static IEnumerable<(u64 id, string name)> GetNames()
    {
        if(!Directory.Exists(cachePath))
            return [];

        return Directory.GetFiles(cachePath)
            .Select(p => (
                id: u64.Parse(Path.GetFileName(p)),
                name: File.ReadAllText(p).Trim()
            ));
    }


    private static async Task<string> GetNameViaRestClientAsync(u64 userId)
        => (await App.restClient.GetUserAsync(userId))?.GlobalName ?? "[unavailable]";
}
