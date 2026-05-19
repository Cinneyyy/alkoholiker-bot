using NetCord;
using NetCord.Services.ApplicationCommands;

namespace src.TypeReaders;

public class EmojiTypeReader<TContext> : SlashCommandTypeReader<TContext> where TContext : IApplicationCommandContext 
{
    public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.String;


    public override ValueTask<SlashCommandTypeReaderResult> ReadAsync(string value, TContext context, SlashCommandParameter<TContext> parameter, ApplicationCommandServiceConfiguration<TContext> configuration, IServiceProvider serviceProvider)
        => new(SlashCommandTypeReaderResult.Success(
            u64.TryParse(value, out u64 customId)
                ? EmojiProperties.Custom(customId)
                : EmojiProperties.Standard(value)
        ));
}