namespace src;

public readonly struct GuildUserPair(u64 guild, u64 user)
{
    public readonly u64 guild = guild;
    public readonly u64 user = user;


    public GuildUserPair(u64? guild, u64 user) : this(guild ?? 0ul, user)
    {
    }


    public static implicit operator GuildUserPair((u64 guild, u64 user) tuple) => new(tuple.guild, tuple.user);
    public static implicit operator GuildUserPair((u64? guild, u64 user) tuple) => new(tuple.guild ?? 0ul, tuple.user);
}
