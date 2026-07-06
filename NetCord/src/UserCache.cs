using NetCord;

namespace src;

public static class UserCache
{
    public static readonly string cachePath = App.GetPath("cache");


    public static async Task<string> GetNameAsync(u64 userId, u64? guildId)
    {
        string path = GetNamePath(userId, guildId);
        if(File.Exists(path))
            return File.ReadAllText(path).Trim();

        return await UpdateNameAsync(userId, guildId);
    }

    public static async Task<string> UpdateNameAsync(u64 userId, u64? guildId)
    {
        (string name, string path) = await GetNameViaRestClientAsync(userId, guildId);

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, name);

        return name;
    }

    public static async Task UpdateAllNamesAsync()
    {
        Directory.CreateDirectory($"{cachePath}/global_names/");
        Directory.CreateDirectory($"{cachePath}/guild_names/");

        foreach(string file in Directory.GetFiles($"{cachePath}/global_names/"))
        {
            (string name, string path) = await GetNameViaRestClientAsync(file.GetFileName().ParseU64(), null);
            File.WriteAllText(path, name);
        }

        foreach(string dir in Directory.GetDirectories($"{cachePath}/guild_names/"))
        {
            foreach(string file in Directory.GetFiles(dir))
            {
                (string name, string path) = await GetNameViaRestClientAsync(file.GetFileName().ParseU64(), dir.GetFileName().ParseU64());
                File.WriteAllText(path, name);
            }
        }
    }

    public static string GetName(u64 userId, u64? guildId)
        => GetNameAsync(userId, guildId).GetAwaiter().GetResult();

    public static IEnumerable<(u64 userId, u64? guildId, string name)> GetNames()
    {
        if(!Directory.Exists(cachePath))
            return [];

        Directory.CreateDirectory($"{cachePath}/global_names/");
        Directory.CreateDirectory($"{cachePath}/guild_names/");

        IEnumerable<(u64, u64?, string)> globalNames = Directory.GetFiles($"{cachePath}/global_names/")
            .Select(f => (
                userId: f.GetFileName().ParseU64(),
                guildId: (u64?)null,
                name: File.ReadAllText(f).Trim()
            ));

        IEnumerable<(u64, u64?, string)> guildNames = Directory.GetDirectories($"{cachePath}/guild_names/")
            .Select(d => (
                guildId: d.GetFileName().ParseU64(),
                files: Directory.GetFiles(d)
            ))
            .SelectMany(d => d.files
                .Select(f => (
                    userId: f.GetFileName().ParseU64(),
                    guildId: (u64?)d.guildId,
                    name: File.ReadAllText(f)
                ))
            );

        return globalNames.Concat(guildNames);
    }


    private static string GetNamePath(u64 userId, u64? guildId)
        => guildId is u64 _guildId
            ? $"{cachePath}/guild_names/{_guildId}/{userId}"
            : $"{cachePath}/global_names/{userId}";

    private static async Task<(string name, string path)> GetNameViaRestClientAsync(u64 userId, u64? guildId)
    {
        if(guildId is u64 _guildId)
        {
            try
            {
                GuildUser guildUser = await App.restClient.GetGuildUserAsync(_guildId, userId);

                if(guildUser is not null)
                {
                    string guildName = guildUser.Nickname ?? guildUser.GlobalName;

                    if(!string.IsNullOrWhiteSpace(guildName))
                        return (guildName, $"{cachePath}/guild_names/{_guildId}/{userId}");
                }
            }
            catch
            {
            }
        }

        User user = await App.restClient.GetUserAsync(userId);
        return (user?.GlobalName ?? $"[{userId}]", $"{cachePath}/global_names/{userId}");
    }
}
