using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NetCord.Gateway;
using src.Rules.Opt;
using SysRegex = System.Text.RegularExpressions.Regex;

namespace src.Rules;

public readonly partial record struct Predicate()
{
    public static readonly Dictionary<string, DateTime> cooldowns = [];

    [JsonIgnore] public readonly string guid = Guid.NewGuid().ToString();


    public f32? chance { get; init; } = null;
    public Regex? regex { get; init; } = null;
    public u64? channel { get; init; } = null;
    public u64? author { get; init; } = null;
    public u32? cooldownSeconds { get; init; } = null;
    public bool? refMessage { get; init; } = null;
    public bool? hasAttachment { get; init; } = null;
    [JsonIgnore] public u32? cooldownMinutes { init => cooldownSeconds = 60 * value; }
    [JsonIgnore] public u32? cooldownHours { init => cooldownSeconds = 60*60 * value; }
    [JsonIgnore] public string cooldown { init => cooldownSeconds = TimeStrToSeconds(value); }
    [JsonIgnore] public u32 actualCooldownSeconds => cooldownSeconds ?? Config.defaultRuleCooldownSeconds;


    public bool CheckTruth(Rule rule, Message message)
    {
        bool failure(string ctx)
        {
            if(Config.logRuleFailure)
                Log.Out($"[{rule.name}] {ctx}");

            return false;
        }

        bool botIsPinged = message.Content.Contains(Secrets.botUserId.ToString());

        // check if opted out
        if(!botIsPinged && OptMgr.IsOptedOut(message.Author.Id))
            return failure("User is opted out.");

        // hasAttachment
        if(hasAttachment is bool reqAttachment && reqAttachment != (message.Attachments.Count > 0))
            return failure("Attachment requirement did not match.");

        // chance
        if(!botIsPinged && chance is f32 reqChance && reqChance < Random.Shared.NextSingle())
            return failure("Chance requirement failed.");

        // channel
        if(channel is u64 reqChannel && reqChannel != message.ChannelId) 
            return failure("Channel requirement failed.");

        // refMessage
        if(refMessage is bool reqRefMessage)
        {
            bool refBotMessage = message.ReferencedMessage?.Author?.Id == Secrets.botUserId;

            if(reqRefMessage != refBotMessage)
                return failure($"Message-reference requirement failed.");
        }

        // author
        if(author is u64 reqAuthor && reqAuthor != 0ul) 
        {
            u64 msgAuthor = ImpostorRegex().Match(message.Content) is Match match && match.Success && u64.TryParse(match.Groups[1].Value, out u64 impostor)
                ? impostor
                : message.Author.Id;

            if(reqAuthor != msgAuthor)
                return failure("Author requirement failed.");
        }

        // regex
        if(regex is Regex reqRegex && !reqRegex.IsMatch(message.Content))
            return failure("Regex requirement failed.");

        // cooldown
        if(actualCooldownSeconds > 0u)
        {
            DateTime now = DateTime.UtcNow;

            if(!botIsPinged && cooldowns.TryGetValue(guid, out DateTime lastUse))
            {
                u64 timeDiffSeconds = (u64)(now - lastUse).TotalSeconds;

                if(timeDiffSeconds < actualCooldownSeconds && !botIsPinged)
                    return failure($"Cooldown requirement failed ({timeDiffSeconds}s / {actualCooldownSeconds} elapsed).");
            }

            cooldowns[guid] = now;
        }

        return true;
    }


    public static u32 TimeStrToSeconds(string time)
        => (time[^1] switch
        {
            's' => 1u,
            'm' => 60u,
            'h' => 60u*60u,
            'd' => 24u*60u*60u,
            _ => throw new($"Invalid cooldown unit (\"{time}\"); must be s, m, h, or d.")
        }) * u32.Parse(time[..^1]);
    
    
    [GeneratedRegex(@"as\((\d+)\)")]
    private static partial SysRegex ImpostorRegex();
}
