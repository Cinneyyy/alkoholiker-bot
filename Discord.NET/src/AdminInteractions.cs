using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace src;

[RequireOwner]
public sealed class AdminInteractions : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("togglefailurelog", "Admin command; toggles logging of each rule that failed into the console.")]
    public async Task ToggleFailureLog(bool value)
    {
        RuleMgr.logFailure = value;
        await RespondAsync($"RuleMgr.logFailure is now set to {value}.", ephemeral: true, flags: MessageFlags.SuppressNotification);
    }

    [SlashCommand("reloadrules", "Admin command; unload and reload all rules.")]
    public async Task ReloadRules()
    {
        RuleMgr.rules.UnloadAll();
        RuleMgr.rules.Load(File.ReadAllText($"{Program.dataPath}/rules.json"));
         
        string[] files = Directory.GetFiles($"{Program.dataPath}/rules", "*.json");
        foreach(string path in files)
            RuleMgr.rules.Load(File.ReadAllText(path));
        
        await RespondAsync($"Loaded the following rule files: [{string.Join(", ", files.Select(Path.GetFileName).Append("rules.json"))}]", ephemeral: true, flags: MessageFlags.SuppressNotification);
    }

    [SlashCommand("clearspamcooldowns", "Clear the current cooldowns for the /spam command.")]
    public async Task ClearSpamCooldowns()
    {
        Interactions.spamCooldowns.Clear();
        await RespondAsync("Cleared spam cooldowns.", ephemeral: true, flags: MessageFlags.SuppressNotification);
    }
}