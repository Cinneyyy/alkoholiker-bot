using System.Text;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src.Rules;

namespace src.Interactions;

public sealed partial class DebugCommands
{
    [SubSlashCommand("rules", "rules")]
    public sealed class Rules : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("reload", "Reload rules.")]
        public async Task Reload(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            IEnumerable<string> ruleFiles = RuleMgr.Load();
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Reloaded all rules from {RuleMgr.path} ({ruleFiles.Count()} files: [{string.Join(", ", ruleFiles.Select(Path.GetFileName))}]).",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("show", "Display the active bot rules.")]
        public async Task Show(bool ephemeral = true)
        {
            string rules = string.Join(",\n", RuleMgr
                .GetRuleFiles()
                .Select(File.ReadAllText)
                .Select(f => f.Trim())
                .Select(f => f.TrimStart('['))
                .Select(f => f.TrimEnd(']'))
                .Select(f => f.Trim('\n'))
            );

            await RespondAsync(InteractionCallback.Message(new()
            {
                Attachments =
                [
                    new("rules.json", new MemoryStream(Encoding.UTF8.GetBytes($"[\n{rules}\n]")))
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("clear-cooldowns", "Clear all rule cooldowns.")]
        public async Task ClearCooldowns(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            Predicate.cooldowns.Clear();
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Successfully cleared all rule cooldowns.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
