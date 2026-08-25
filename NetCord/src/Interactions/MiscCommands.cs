using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src.Rules;

namespace src.Interactions;

public sealed partial class MiscCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly Dictionary<Language, string[]> crazyLinesByLanguage = new()
    {
        [Language.English] =
        [
            "$0?",
            "I was $l0 once.",
            "They put me in $l1.",
            "$2",
            "$2 with $l3.",
            "And $l3 make me $l0.",
            "$0?"
        ]
    };


    [SlashCommand("say", "Say something as the bot.")]
    public async Task Say(string message)
    {
        Log.Out($"{Context.User.Username} invoked /say with text \"{message.Replace("\n", "\\n")}\".");

        if(string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Message cannot be empty!",
                Flags = MessageFlags.Get(ephemeral: true)
            }));

            return;
        }

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Sending message.",
            Flags = MessageFlags.Get(ephemeral: true)
        }));

        await Context.Channel.SendMessageAsync(new()
        {
            Content = message,
            Flags = MessageFlags.Get(ephemeral: false, silent: false)
        });
    }

    [SlashCommand("crazy", "Create a crazy message.")]
    public async Task Crazy(string whatWereYouOnce, string whereDidTheyPutYou, string beMoreSpecific, string whatWasThere, Language language = Language.English)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: false)));

        string[] indexedData = [whatWereYouOnce, whereDidTheyPutYou, beMoreSpecific, whatWasThere];

        string replace(string ln)
        {
            for(i32 i = 0; i < indexedData.Length; i++)
                ln = ln.Replace($"${i}", indexedData[i]).Replace($"$l{i}", indexedData[i].ToLowerInvariant());

            return ln;
        }

        string message = string.Join("\n", crazyLinesByLanguage[language]);
        message = replace(message);
        GenerateRuleForMessage(whatWereYouOnce.ToLowerInvariant(), message);
        RuleMgr.Load();

        await FollowupAsync(new()
        {
            Content = message,
            Flags = MessageFlags.Get(ephemeral: false)
        });
    }


    private static void GenerateRuleForMessage(string trigger, string message)
    {
        string path = App.GetPath("rules/crazy_autogen/");
        Directory.CreateDirectory(path);

        path += $"{trigger}.json";

        // Just overwrite it idc
        // if(File.Exists(path))
        //     return;

        Rule rule = new()
        {
            @break = true,
            name = $"triggered_{trigger}_autogen",
            order = -50,
            predicate = new()
            {
                regex = new()
                {
                    pattern = $"^{trigger}$",
                    ignoreCase = true,
                }
            },
            replies =
            [
                new()
                {
                    text = message
                }
            ]
        };

        string json = Json.Serialize<Rule[]>([rule]);
        File.WriteAllText(path, json);
    }
}
