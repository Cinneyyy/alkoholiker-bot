using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using src.Casino;

namespace src;

public sealed class ReactionCreatedHandler : IMessageReactionAddGatewayHandler
{
    private static readonly Dictionary<u64, DateTime> cooldowns = [];


    public ValueTask HandleAsync(MessageReactionAddEventArgs arg)
    {
        if(arg.User is null)
            return ValueTask.CompletedTask;

        u64 userId = arg.UserId;
        if(arg.User.IsBot || userId == Secrets.botUserId)
            return ValueTask.CompletedTask;

        if(arg.MessageAuthorId is not u64 authId || arg.GuildId is not u64 guildId)
            return ValueTask.CompletedTask;

        if(userId == authId)
            return ValueTask.CompletedTask;


        DateTime now = DateTime.UtcNow;
        f32 diffSeconds = f32.PositiveInfinity;
        if(cooldowns.TryGetValue(userId, out DateTime cooldownStart))
            diffSeconds = (f32)(now - cooldownStart).TotalSeconds;

        Emoji emoji = new(arg.Emoji.Name ?? "", arg.Emoji.Id ?? 0ul);
        if(diffSeconds < Config.reactionXpCooldownSeconds)
        {
            Log.Out($"{arg.User.Username} reacted with {emoji} to a message by {UserCache.GetName(authId, guildId)} [{authId}]; cooldown applied, {Config.reactionXpCooldownSeconds-diffSeconds:0.0s} left.");
            return ValueTask.CompletedTask;
        }

        cooldowns[userId] = now;

        Log.Out($"{arg.User.Username} reacted with {emoji} to a message by {UserCache.GetName(authId, guildId)} [{authId}].");
        LevelUpMgr.OnReaction((guildId, userId), (guildId, authId), emoji);

        return ValueTask.CompletedTask;
    }
}
