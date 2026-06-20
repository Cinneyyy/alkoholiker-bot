using NetCord;
using NetCord.Rest;

namespace src;

public static class UserCache
{
    public static async Task<string> GetName(u64 userId)
    {
        string path = App.GetPath($"user_cache/names/{userId}");
        if(File.Exists(path))
            return File.ReadAllText(path).Trim();

        return await UpdateName(userId);
    }

    public static async Task<string> UpdateName(u64 userId)
    {
        string path = App.GetPath("user_cache/names");
        Directory.CreateDirectory(path);

        string name = await GetNameViaRestClient(userId);
        File.WriteAllText($"{path}/{userId}", name);

        return name;
    }

    public static async void UpdateAllNames()
    {
        foreach(string path in Directory.GetFiles(App.GetPath("user_cache/names/")))
            File.WriteAllText(path, await GetNameViaRestClient(u64.Parse(Path.GetFileName(path))));
    }

    public static async Task<u32> GetRoleColor(u64 guildId, u64 userId)
    {
        string path = App.GetPath($"user_cache/colors/{guildId}/{userId}");
        if(File.Exists(path))
            return Convert.ToUInt32(File.ReadAllText(path).Trim(), 16);

        return await UpdateRoleColor(guildId, userId);
    }

    public static async Task<u32> UpdateRoleColor(u64 guildId, u64 userId)
    {
        string path = App.GetPath($"user_cache/colors/{guildId}");
        Directory.CreateDirectory(path);

        u32 color = await GetRoleColorViaRestClient(guildId, userId);
        File.WriteAllText($"{path}/{userId}", Convert.ToString(color, 16));

        return color;
    }

    public static async void UpdateAllRoleColors()
    {
        foreach(string guildPath in Directory.GetDirectories(App.GetPath("user_cache/colors/")))
            foreach(string userPath in Directory.GetFiles(guildPath))
                File.WriteAllText(userPath, Convert.ToString(await GetRoleColorViaRestClient(u64.Parse(Path.GetFileName(guildPath)), u64.Parse(Path.GetFileName(userPath))), 16));
    }


    private static async Task<string> GetNameViaRestClient(u64 userId)
        => (await App.restClient.GetUserAsync(userId)).GlobalName;

    private static async Task<u32> GetRoleColorViaRestClient(u64 guildId, u64 userId)
    {
        GuildUser user = await App.restClient.GetGuildUserAsync(guildId, userId);
        IEnumerable<Role> roles = await App.restClient.GetGuildRolesAsync(guildId);

        Role primaryRole = roles
            .Where(r => user.RoleIds.Contains(r.Id))
            .OrderByDescending(r => r.Position)
            .FirstOrDefault();

        Log.Out($"Role for {user.Username}: {primaryRole?.Name} // {primaryRole?.Colors?.PrimaryColor:x} // {primaryRole?.Colors?.SecondaryColor,6:x} // {primaryRole?.Colors?.TertiaryColor,6:x}");

        return primaryRole is null ? 0xffffff : (u32)(primaryRole.Colors.PrimaryColor.RawValue & 0xffffff);
    }
}
