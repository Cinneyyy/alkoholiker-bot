using System.Text.RegularExpressions;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using SysRegex = System.Text.RegularExpressions.Regex;

namespace src.Rules;

public readonly partial struct Reply()
{
    public string text { get; init; } = null;
    public string[] reactions { get; init; } = null;
    public Poll? poll { get; init; } = null;
    public bool refMessage { get; init; } = false;
    public Dictionary<string, string[]> snippets { get; init; } = [];
    public f32 weight { get; init; } = 1f;
    public string[] attachments
    {
        get;
        init => field = value.SelectMany(v => v.EndsWith('/')
            ? Directory.GetFiles(App.GetPath($"res/{v}"), "*.*", SearchOption.AllDirectories)
            : [App.GetPath($"res/{v}")]
        ).ToArray();
    } = [];


    public async Task Apply(Message message, Rule rule)
    {
        if(text is not null || attachments is not null and not [] || poll is not null)
        {
            MessageProperties response = new()
            {
                Flags = MessageFlags.Get(ephemeral: false)
            };

            if(text is string content)
            {
                Dictionary<string, string[]> snippets = this.snippets;
                content = SnippetRegex().Replace(content, m => snippets[m.Groups[1].Value].SelectRandom());
                content = MentionRegex().Replace(content, m => $"<@{m.Groups[1].Value}>");
                content = ChannelRegex().Replace(content, m => $"<#{m.Groups[1].Value}>");

                if(rule.predicate.regex is Regex regex && regex.Match(message.Content, out Match match))
                    content = RegexRegex().Replace(content, m => match.Groups[i32.Parse(m.Groups[1].Value)].Value);

                response.Content = content;
            }

            if(refMessage)
                response = response.WithMessageReference(MessageReferenceProperties.Reply(message.Id));

            if(attachments is not null and not [])
            {
                string att = attachments.SelectRandom();
                response = response.WithAttachments([new(Path.GetFileName(att), File.OpenRead(att))]);
            }

            if(poll is Poll p)
            {
                response = response.WithPoll(new MessagePollProperties(
                    new()
                    {
                        Text = p.question.text,
                        Emoji = p.question.emoji is null ? null : Emoji.Parse(p.question.emoji).ToEmojiProperties()
                    },
                    p.answers.Select(a => new MessagePollAnswerProperties(new()
                    {
                        Text = a.text,
                        Emoji = a.emoji is null ? null : Emoji.Parse(a.emoji).ToEmojiProperties()
                    })))
                        .WithDurationInHours((i32)p.hours)
                        .WithAllowMultiselect(p.multiselect)
                );
            }

            await message.Channel.SendMessageAsync(response);
        }

        if(reactions is not null and not [])
        {
            foreach(string reaction in reactions)
                await message.AddReactionAsync(Emoji.Parse(reaction).ToReactionProperties());
        }
    }

    [GeneratedRegex(@"\$([A-Z-a-z0-9_]+)\$")]
    private static partial SysRegex SnippetRegex();

    [GeneratedRegex(@"@(\d+)@")]
    private static partial SysRegex MentionRegex();

    [GeneratedRegex(@"#(\d+)#")]
    private static partial SysRegex ChannelRegex();

    [GeneratedRegex(@"%(\d+)%")]
    private static partial SysRegex RegexRegex();
}
