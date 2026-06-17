using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src;

public sealed class ActivityStatusMgr : IReadyGatewayHandler
{
    private static readonly Thread statusUpdateThread = new(StatusUpdateThread)
    {
        IsBackground = true
    };


    public async ValueTask HandleAsync(ReadyEventArgs args)
    {
        Log.Out($"Starting status update thread.");
        statusUpdateThread.Start();
    }


    private static async void StatusUpdateThread()
    {
#pragma warning disable CA1861 // Avoid constant arrays as arguments
        UserActivityProperties[] activities =
        [
            ..new string[]
            {
                "Thinking about alcohol",
                "Vodka-O"
            }.Select(s => new UserActivityProperties(s, UserActivityType.Custom) { State = s }),
            new("Competing in a Drinking Contest", UserActivityType.Competing),
            new("Listening to Alcohol's Wisdom", UserActivityType.Listening),
            new("Playing Alcohol Speedrung Any%", UserActivityType.Playing),
            new("Watching Alcohol Ferment", UserActivityType.Watching),
        ];

        while(true)
        {
            await App.gatewayClient.UpdatePresenceAsync(new(UserStatusType.Online)
            {
                Afk = false,
                Since = new(new DateTime(2001, 9, 11)),
                Activities = [activities.SelectRandom()]
            });

            Thread.Sleep(Config.activityChangeIntervalMs);
        }
    }
}
