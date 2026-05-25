namespace src.Rules.Api;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ChannelAttribute : Attribute
{
    public readonly string name;
    public readonly u64 id;


    public ChannelAttribute(string name)
        => this.name = name;

    public ChannelAttribute(u64 id)
        => this.id = id;
}