using NetCord;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

[SlashCommand("casino", "casino", Contexts = [InteractionContextType.Guild])]
public sealed partial class CasinoCommands : ApplicationCommandModule<ApplicationCommandContext>
{
}
