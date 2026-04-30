using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using src.Rules;

namespace src;

public sealed class Interactions : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("uptime", "Retrieve the amount of time for which the current bot session has been running.")]
    public async Task Uptime(bool ephemeral = true)
    {
        TimeSpan timeSpan = (DateTime.UtcNow - Program.startTime);
        string timeSpanStr = timeSpan.Days > 0
            ? $"{timeSpan.Days}:{timeSpan:hh\\:mm\\:ss}"
            : $"{timeSpan:hh\\:mm\\:ss}";

        await RespondAsync(text: $"The bot has been running for {timeSpanStr}.", ephemeral: ephemeral, flags: MessageFlags.SuppressNotification);
    }

    [SlashCommand("ping", "Ping the bot and display the time it takes for the bot to send a message and receive the ACK.")]
    public async Task Ping(bool ephemeral = true)
    {
        DateTime start = DateTime.UtcNow;
        await RespondAsync(text: "Pong!", ephemeral: ephemeral, flags: MessageFlags.SuppressNotification);

        string newMessage = $"Pong! ({(DateTime.UtcNow - start).TotalMilliseconds:0}ms)";
        await ModifyOriginalResponseAsync(msg => msg.Content = newMessage);
    }

    [SlashCommand("rules", "List the currently active rules, as well as emoji, user, and channel aliases.")]
    public async Task Rules(bool ephemeral = true)
    {
        try
        {
            StringBuilder sb = new();

            sb.AppendLine($"emoji: [{string.Join(", ", RuleMgr.rules.emoji.Select(e => e.alias))}]");
            sb.AppendLine($"users: [{string.Join(", ", RuleMgr.rules.users.Select(e => e.alias))}]");
            sb.AppendLine($"channels: [{string.Join(", ", RuleMgr.rules.channels.Select(e => e.alias))}]");
            sb.AppendLine($"default cooldown: {RuleMgr.rules.defaultCooldownSeconds}s");

            sb.AppendLine("rules:");
            foreach(Rule rule in RuleMgr.rules.rules)
                sb.AppendLine(rule.ToString("- "));

            string path = Random.Shared.Next(i32.MaxValue).ToString();
            File.WriteAllText(path, sb.ToString());

            await RespondWithFileAsync(path, fileName: "rules.txt", ephemeral: ephemeral, flags: MessageFlags.SuppressNotification);

            File.Delete(path);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [SlashCommand("rulesjson", "Retrieve the raw rules.json file.")]
    public async Task RulesJson(bool ephemeral = true)
        => await RespondWithFileAsync("rules.json", fileName: "rules.json", ephemeral: ephemeral, flags: MessageFlags.SuppressNotification);
        
    [SlashCommand("janein", "Create a poll with yes/no response options.")]
    public async Task YesNo(string question, bool maybe = false, u32 durationHours = 1u)
    {
        List<PollMediaProperties> answers =
        [
            new() { Text = "Ja", Emoji = Emoji.Parse(":+1:") },
            new() { Text = "Nein", Emoji = Emoji.Parse(":-1:") }
        ];

        if(maybe)
        {
            string[] maybeEmojiList = [":call_me:", ":no_mouth:", ":skull:", ":eye:", ":older_man:", ":flag_nl:"];   
            string maybeEmoji = maybeEmojiList[Random.Shared.Next(maybeEmojiList.Length)];
            answers.Add(new() { Text = "Vielleicht", Emoji = Emoji.Parse(maybeEmoji) });
        }

        await RespondAsync(
            poll: new()
            {
                AllowMultiselect = false,
                Duration = u32.Clamp(durationHours, 1u, 768u),
                Question = new() { Text = $"{Emoji.Parse(":question:")} {question}" },
                Answers = answers,
                LayoutType = PollLayout.Default
            },
            ephemeral: false
        );

        // await FollowupAsync(
        //     components: new ComponentBuilder()
        //         .WithButton(label: "End poll", customId: "end_poll_btn", style: ButtonStyle.Primary)
        //         .Build(),
        //     ephemeral: true
        // );
    }

    [SlashCommand("optout", "Opt out of receiving bot responses.")]
    public async Task OptOut()
        => await RespondWithModalAsync(new ModalBuilder()
            .WithTitle("Opt in/out")
            .WithCustomId("optout_modal")
            .AddCheckBox("Receive bot responses", new CheckboxBuilder().WithCustomId("optin_cb").WithDefaultState(false))
            .Build());

    [SlashCommand("optin", "Opt into receiving bot responses.")]
    public async Task OptIn()
        => await RespondWithModalAsync(new ModalBuilder()
            .WithTitle("Opt in/out")
            .WithCustomId("optout_modal")
            .AddCheckBox("Receive bot responses", new CheckboxBuilder().WithCustomId("optin_cb").WithDefaultState(true))
            .Build());
}
