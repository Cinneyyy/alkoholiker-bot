using NetCord.Gateway;
using NetCord.Rest;

namespace src.Extension;

public static class MessageExt
{
    extension(Message message)
    {
        public string toLoggableStr => $"<{message.Author.Username}> \"{message.Content}\" ({message.Attachments.Count} attachments)";


        public async Task AddReactionAsync(string builtIn)
            => await message.AddReactionAsync(new ReactionEmojiProperties(builtIn));
        public async Task AddReactionAsync(string customName, u64 customId)
            => await message.AddReactionAsync(new(customName, customId));
        public async Task AddReactionAsync(Emoji emoji)
            => await message.AddReactionAsync(emoji.ToReactionProperties());

        public async Task AddReactionsAsync(IEnumerable<Emoji> emojis)
        {
            foreach(Emoji emoji in emojis)
                await message.AddReactionAsync(emoji);

        }
        public async Task AddReactionsAsync(IEnumerable<string> emojis)
            => await message.AddReactionsAsync(emojis.Select(Emoji.Parse));
    }
}
