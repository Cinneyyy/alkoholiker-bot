using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using src.Rules;

namespace src;

public static partial class RuleMgr
{
    public static readonly RuleCollection rules = new();

    private static readonly Dictionary<i32, DateTime> cooldowns = [];


    public static u64 botUserId => u64.Parse(Environment.GetEnvironmentVariable("BOT_USER_ID"));
    public static bool logFailure { get; set; }


    public static IEnumerable<Rule> GetMatchingRules(SocketUserMessage msg)
        => rules.rules.Where(r => IsMatch(r.name, r.predicate, msg));

    public static bool IsMatch(string ruleName, Predicate p, SocketUserMessage msg)
    {
        bool error(string ctx)
        {
            if(logFailure)
                Console.WriteLine($"[{ruleName}] {ctx}");

            return false;
        }

        if(OptOutMgr.IsOptedOut(msg.Author.Id))
            return error("User is not opted in.");

        if(p.hasImage is not null && p.hasImage.Value != (msg.Attachments.Count > 0))
            return error("Attachment requirement did not match.");

        bool botIsPinged = msg.Content.Contains(botUserId.ToString());
        if(p.chance != 1f && p.chance < Random.Shared.NextDouble() && !botIsPinged)
            return error("Chance failed and bot was not pinged.");

        if(!string.IsNullOrEmpty(p.channel) && rules.GetChannel(p.channel)?.id != msg.Channel.Id)
            return error("Channel predicate failed.");

        if(p.refMessage is not null)
        {
            if(p.refMessage.Value && (msg.ReferencedMessage is null || msg.ReferencedMessage.Author.Id != botUserId))
                return error("Ref-message check failed (message did not reference a bot message).");
            else if(!p.refMessage.Value && (msg.ReferencedMessage is not null || msg.ReferencedMessage.Author.Id == botUserId))
                return error("Ref-message check failed (message referenced a bot message).");
        }

        if(!string.IsNullOrEmpty(p.user))
        {
            u64 authorId = msg.Author.Id;

            if(UserIdMaskRegex().Match(msg.Content) is Match userIdMatch && userIdMatch.Success)
                authorId = u64.Parse(userIdMatch.Groups[1].Value);
            else if(UserAliasMaskRegex().Match(msg.Content) is Match userAliasMatch && userAliasMatch.Success)
                authorId = rules.GetUser(userAliasMatch.Groups[1].Value).id;

            if(rules.GetUser(p.user).id != authorId)
                return error($"User ID ({authorId}) did not match rule requirement ({rules.GetUser(p.user).id}).");
        }

        if(!string.IsNullOrEmpty(p.regex) && !Regex.IsMatch(msg.Content, p.regex, p.regexIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
            return error("Regex did not find match.");

        if(p.GetActualCooldown(rules) > 0)
        {
            DateTime now = DateTime.UtcNow;

            if(cooldowns.TryGetValue(p.predicateId, out DateTime time))
            {
                if((now - time).TotalSeconds < p.GetActualCooldown(rules) && !botIsPinged)
                    return error($"Cooldown is not expired ({(now - time).TotalSeconds - p.GetActualCooldown(rules):0.0}s left).");
            }

            cooldowns[p.predicateId] = now;
        }

        return true;
    }

    public static async void HandleMessage(SocketUserMessage msg)
    {
        i32 numRulesApplied = 0;
        foreach(Rule rule in GetMatchingRules(msg))
        {
            try
            {
                Reply reply = rule.randomReply;
                MessageFlags flags = reply.silent ? MessageFlags.SuppressNotification : 0;

                if(reply.reactions is not (null or []))
                    await msg.AddReactionsAsync(reply.reactions.Select(rules.GetEmoji));

                MessageReference messageRef = !reply.refMessage ? null : new(messageId: msg.Id);

                if(reply.poll != default)
                {
                    await msg.Channel.SendMessageAsync(poll: new()
                    {
                        AllowMultiselect = reply.poll.multiselect,
                        Duration = reply.poll.hours,
                        Question = reply.poll.question.ToPollMediaProperties(rules),
                        Answers = [..reply.poll.answers.Select(a => a.ToPollMediaProperties(rules))],
                        LayoutType = PollLayout.Default
                    }, flags: flags, messageReference: messageRef);
                }
                else
                {
                    string text = null;
                    if(reply.emoji is not null)
                        text = rules.GetEmoji(reply.emoji).ToString();
                    else if(reply.text is not null)
                    {
                        Match match = null;

                        if(!string.IsNullOrEmpty(rule.predicate.regex))
                            match = Regex.Match(msg.Content, rule.predicate.regex, rule.predicate.regexIgnoreCase ? RegexOptions.IgnoreCase : 0);

                        text = reply.FormatText(rules, match);
                    }

                    if(reply.attachments is not [] && reply.randomAttachment is string att && File.Exists($"{Program.dataPath}/res/{att}"))
                    {
                        await msg.Channel.SendFileAsync(
                            filePath: att,
                            text: text,
                            messageReference: messageRef,
                            flags: flags
                        );
                    }
                    else if(text is not null)
                    {
                        await msg.Channel.SendMessageAsync(
                            text: text,
                            messageReference: messageRef,
                            flags: flags
                        );
                    }
                }

                numRulesApplied++;
                if(rule.@break)
                    break;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        if(numRulesApplied > 0)
            Console.WriteLine($"^ applied {numRulesApplied} rule(s)");
    }


    [GeneratedRegex("as\\(([0-9]+)\\)")]
    private static partial Regex UserIdMaskRegex();

    [GeneratedRegex("as\\(([a-z0-9\\-_]+)\\)")]
    private static partial Regex UserAliasMaskRegex();
}
