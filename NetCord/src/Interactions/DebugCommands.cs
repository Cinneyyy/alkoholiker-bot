using System.Diagnostics;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Interactions;

[SlashCommand("debug", "debug")]
public sealed partial class DebugCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("ping", "Ping the bot and display the time it took for the bot to send a message and receive the ACK.")]
    public async Task Ping(bool ephemeral = true)
    {
        DateTime start = DateTime.UtcNow;
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Pong!",
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        }));

        string newMessage = $"Pong! ({(DateTime.UtcNow - start).TotalMilliseconds:0}ms)";
        await Context.Interaction.ModifyResponseAsync(m => m.Content = newMessage);
    }

    [SubSlashCommand("uptime", "Check for how long the bot has been online.")]
    public async Task Uptime(bool ephemeral = true)
    {
        TimeSpan time = DateTime.UtcNow - App.startTime;
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"The bot has been running for {time.Days}d {time.Hours}h {time.Minutes}m {time.Seconds}s.",
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        }));
    }

    [SubSlashCommand("temp", "Display the raspberry-pi's temperature.")]
    public async Task Temp(bool ephemeral = true)
    {
        string message = null;

        try
        {
            using Process tempProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = "vcgencmd",
                Arguments = "measure_temp",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if(tempProcess.StandardError.ReadToEnd() is string stdErr && !string.IsNullOrWhiteSpace(stdErr))
                throw new(stdErr);

            message = tempProcess.StandardOutput.ReadToEnd();
        }
        catch(Exception e)
        {
            message = $"Failed to read system temperature ({e.Message}).";
        }
        finally
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = message,
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
