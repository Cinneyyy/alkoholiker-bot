using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.ActivityStatus;

public sealed class ActivityStatusMgr : IReadyGatewayHandler
{
    private static readonly Thread statusUpdateThread = new(StatusUpdateThread)
    {
        IsBackground = true
    };
    private static Activities activities = new();


    public async ValueTask HandleAsync(ReadyEventArgs args)
    {
        Log.Out($"Starting status update thread.");
        statusUpdateThread.Start();
    }


    public static void Load()
    {
        activities = Json.DeserializeFile<Activities>(App.GetPath($"statuses.json"));
        activities.GenerateProperties();
    }

    private static async void StatusUpdateThread()
    {
        Load();

        while(true)
        {
            await App.gatewayClient.UpdatePresenceAsync(new(UserStatusType.Online)
            {
                Afk = false,
                Since = new(new DateTime(2001, 9, 11)),
                Activities = [activities.properties.SelectRandom()]
            });

            Thread.Sleep(Config.activityChangeIntervalMs);
        }
    }
}
