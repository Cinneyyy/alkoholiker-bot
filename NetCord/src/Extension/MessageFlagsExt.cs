using NetCord;

namespace src.Extension;

public static class MessageFlagsExt
{
    extension(MessageFlags)
    {
        public static MessageFlags Get(bool ephemeral = true, bool silent = true)
            => (ephemeral ? MessageFlags.Ephemeral : 0) | (silent ? MessageFlags.SuppressNotifications : 0);
    }
}