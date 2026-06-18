using NetCord.Gateway;

namespace src.ActivityStatus;

public sealed class Activities
{
    public string[] custom { get; init; } = [];
    public string[] competing { get; init; } = [];
    public string[] listening { get; init; } = [];
    public string[] playing { get; init; } = [];
    public string[] watching { get; init; } = [];


    public UserActivityProperties[] properties { get; private set; } = [];


    public void GenerateProperties()
        => properties =
        [
            ..custom.Select(a => new UserActivityProperties(a, UserActivityType.Custom) { State = a }),
            ..competing.Select(a => new UserActivityProperties(a, UserActivityType.Competing)),
            ..listening.Select(a => new UserActivityProperties(a, UserActivityType.Listening)),
            ..playing.Select(a => new UserActivityProperties(a, UserActivityType.Playing)),
            ..watching.Select(a => new UserActivityProperties(a, UserActivityType.Competing)),
        ];
}
