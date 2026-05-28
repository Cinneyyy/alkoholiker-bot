using System.Reflection;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src.Rules;
using Conf = src.Config;

namespace src.Commands;

[SlashCommand("owner", "owner", DefaultGuildPermissions = Permissions.Administrator)]
public sealed class OwnerCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("rules", "rules")]
    public sealed class Rules : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("reload", "Reload rules.")]
        public async Task Reload(bool ephemeral = true)
        {
            if(!await CheckForOwner(Context))
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
            if(!await CheckForOwner(Context))
                return;

            IEnumerable<string> files = RuleMgr.GetRuleFiles();
            await RespondAsync(InteractionCallback.Message(new()
            {
                Attachments = files.Select(f => new AttachmentProperties(Path.GetFileName(f), File.OpenRead(f))),
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }

    [SubSlashCommand("config", "config")]
    public sealed class Config : ApplicationCommandModule<ApplicationCommandContext>
    {
        private const BindingFlags BindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;


        [SubSlashCommand("get", "Get a config value.")]
        public async Task Get(string name, bool ephemeral = true)
        {
            if(!await CheckForOwner(Context))
                return;

            try
            {
                PropertyInfo pInfo = typeof(Conf).GetProperty(name, BindingAttr);
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"`{nameof(Conf)}.{pInfo.Name}` `({pInfo.PropertyType.Name})` has the value `{pInfo.GetValue(null)}`.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));
            }
            catch(Exception e)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"Failed to fetch config value `{name}` ({e.Message}).",
                    Flags = MessageFlags.Get()
                }));
            }
        }

        [SubSlashCommand("set", "Set a config value.")]
        public async Task Set(string name, bool? @bool = null, string str = null, u8? u8 = null, i32? i32 = null, u32? u32 = null, i64? i64 = null, u64? u64 = null, f32? f32 = null, f64? f64 = null, string u64str = null, bool ephemeral = true)
        {
            if(!await CheckForOwner(Context))
                return;

            try
            {
                object value = (object)@bool ?? (object)str ?? (object)u8 ?? (object)i32 ?? (object)u32 ?? (object)i64 ?? (object)u64 ?? (object)f32 ?? (object)f64 ?? (ulong.TryParse(u64str, out u64 parsedU64) ? (object)parsedU64 : null) ?? throw new($"No value specified.");

                PropertyInfo pInfo = typeof(Conf).GetProperty(name, BindingAttr);
                object oldValue = pInfo.GetValue(null);
                pInfo.SetValue(null, value);

                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"Changed the value of `{nameof(Conf)}.{pInfo.Name}` `({pInfo.PropertyType.Name})` from `{oldValue}` to `{value}`.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));
            }
            catch(Exception e)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"Failed to fetch config value `{name}` ({e.Message}).",
                    Flags = MessageFlags.Get()
                }));
            }
        }

        [SubSlashCommand("list", "Get a list of all config names, types, and values.")]
        public async Task List(bool ephemeral = true)
        {
            if(!await CheckForOwner(Context))
                return;

            PropertyInfo[] properties = typeof(Conf).GetProperties(BindingAttr);
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = properties is []
                    ? $"No config properties available."
                    : $"```\n{string.Join("\n", properties.Select(p => $"- {p.Name} ({p.PropertyType.Name}): {p.GetValue(null)}"))}```",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("load", "Load the config from disk.")]
        public async Task Load(bool ephemeral = true)
        {
            if(!await CheckForOwner(Context))
                return;

            Conf.Load();

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Loaded config from `{Conf.GetPath()}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("save", "Save the config to disk.")]
        public async Task Save(bool ephemeral = true)
        {
            if(!await CheckForOwner(Context))
                return;

            Conf.Save();

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Saved config to `{Conf.GetPath()}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }


    private static async Task<bool> CheckForOwner(ApplicationCommandContext ctx)
    {
        if(ctx.User.Id != Secrets.owner)
        {
            await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(new()
            {
                Content = "Failed to execute command.",
                Flags = MessageFlags.Get()
            }));

            return false;
        }
        else
            return true;
    }
}
