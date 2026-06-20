using System.Text;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src;

public static class App
{
    public const string AnsiReset = "\x1b[0m";


    public static string dataPath { get; private set; }
    public static RestClient restClient { get; private set; }
    public static GatewayClient gatewayClient { get; private set; }
    public static DateTime startTime { get; } = DateTime.UtcNow;


    public static void Load()
        => dataPath = Secrets.dataPath.Replace("%", Path.TrimEndingDirectorySeparator(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));

    public static string GetPath(string relPath)
        => Path.Combine(dataPath, relPath);

    public static void SetClient(RestClient restClient, GatewayClient gatewayClient)
    {
        App.restClient = restClient;
        App.gatewayClient = gatewayClient;
    }

    public static async Task<bool> CheckForOwner(ApplicationCommandContext ctx)
    {
        if(ctx.User.Id != Secrets.owner)
        {
            await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(new()
            {
                Content = "Failed to execute command.",
                Flags = MessageFlags.Get()
            }));

            return false;
        }

        return true;
    }


    public static string GetTimeStr(DateTime time)
        => time.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
    public static string GetTimeStr(TimeSpan span)
    {
        StringBuilder sb = new();

        void prepend(i32 value, char unit, bool padWithZeros)
            => sb.Insert(0, padWithZeros ? $"{value:00}{unit} " : $"{value,2}{unit} ");

        bool hasMins = span.TotalMinutes > 1d;
        bool hasHours = span.TotalHours > 1d;
        bool hasDays = span.TotalDays > 1d;

        prepend(span.Seconds, 's', hasMins);
        sb.Remove(sb.Length-1, 1); // Remove trailing space

        if(hasMins)
            prepend(span.Minutes, 'm', hasHours);

        if(hasHours)
            prepend(span.Hours, 'h', hasDays);

        if(hasDays)
            prepend(span.Days, 'd', false);

        return sb.ToString();
    }

    public static string GetAnsiColor(u8 r, u8 g, u8 b)
        => $"\x1b[38;2;{r};{g};{b}m";
    public static string GetAnsiColor(u32 rgb)
        => GetAnsiColor((u8)(rgb >> 16), (u8)(rgb >> 8), (u8)rgb);
}
