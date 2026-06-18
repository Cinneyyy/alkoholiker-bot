using System.Text;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src;

public static class App
{
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
        if (ctx.User.Id != Secrets.owner)
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
        sb.Insert(0, $"{span.Seconds:00}s");

        if(span.TotalMinutes >= 1d)
            sb.Insert(0, $"{span.Minutes:00}m ");

        if(span.TotalHours >= 1d)
            sb.Insert(0, $"{span.Hours:00}h ");

        if(span.TotalDays >= 1d)
            sb.Insert(0, $"{span.Days}d ");

        return sb.ToString();
    }
}
