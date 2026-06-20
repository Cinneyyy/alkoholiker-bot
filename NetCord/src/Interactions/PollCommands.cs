using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Interactions;

[SlashCommand("poll", "poll")]
public sealed class PollInteractions : ApplicationCommandModule<ApplicationCommandContext>
{
    private static readonly string[] maybeEmojis = ["🤙", "😶", "💀", "👁️", "🧓", "🇳🇱"];


    [SubSlashCommand("yes-no", "Create a yes/no choice poll.")]
    public async Task Binary(
        string question,
        [SlashCommandParameter(Description = "Poll duration in hours.", MinValue = 1.0, MaxValue = 768.0)] u32 duration,
        [SlashCommandParameter(Description = "Add third \"maybe\" option.")] bool maybe = false
    )
    {
        List<MessagePollAnswerProperties> answers =
        [
            new(new()
            {
                Text = "Ja",
                Emoji = EmojiProperties.Standard("👍")
            }),
            new(new()
            {
                Text = "Nein",
                Emoji = EmojiProperties.Standard("👎")
            })
        ];

        if(maybe)
        {
            answers.Add(new(new()
            {
                Text = "Vielleicht",
                Emoji = EmojiProperties.Standard(maybeEmojis.SelectRandom())
            }));
        }

        await RespondAsync(InteractionCallback.Message(new()
        {
            Poll = new(
                question: new()
                {
                    Text = $"❓ {question}"
                },
                answers: answers
            )
            {
                DurationInHours = (i32)duration,
                AllowMultiselect = false,
                LayoutType = MessagePollLayoutType.Default
            }
        }));
    }
}
