using NetCord.Services.ApplicationCommands;
using NetCord;
using NetCord.Rest;

namespace src.RockPaperScissors;

[SlashCommand("rock", "rock")]
public sealed class RpsCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("paper", "paper")]
    public sealed class Sub : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("scissors", "Play rock paper scissors.")]
        public async Task PlayRps()
            => await RespondAsync(InteractionCallback.Message(new()
            {
                Components =
                [
                    new ActionRowProperties(
                    [
                        new ButtonProperties($"button_rps_select:{(u8)RpsSelection.Rock}", "🪨", ButtonStyle.Primary),
                        new ButtonProperties($"button_rps_select:{(u8)RpsSelection.Paper}", "🧻", ButtonStyle.Primary),
                        new ButtonProperties($"button_rps_select:{(u8)RpsSelection.Scissors}", "✂️", ButtonStyle.Primary),
                        new ButtonProperties("button_rps_finish", "Done", ButtonStyle.Primary)
                    ])
                ]
            }));
    }
}
