using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Rules.Opt;

[SlashCommand("opt", "opt")]
public sealed class OptCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("out", "Opt out of receiving bot responses.")]
    public async Task Out()
    {
        if(OptMgr.IsOptedOut(Context.User.Id))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "You are already opted out.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        OptMgr.OptOut(Context.User.Id);
        
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Successfully opted out.",
            Flags = MessageFlags.Get()
        }));
    }

    [SubSlashCommand("in", "Opt into receiving bot responses.")]
    public async Task In()
    {
        if(!OptMgr.IsOptedOut(Context.User.Id))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "You are already opted in.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        OptMgr.OptIn(Context.User.Id);
        
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Successfully opted in.",
            Flags = MessageFlags.Get()
        }));
    }
}