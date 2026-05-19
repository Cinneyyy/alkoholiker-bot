using NetCord;

namespace src.Extension;

public static class EmojiPropertiesExt
{
    extension(EmojiProperties emojiProperties)
    {
        public string GetEmojiString()
            => emojiProperties.Id is null
                ? emojiProperties.Name
                : $"<:custom:{emojiProperties.Id.Value}>";
    }
}