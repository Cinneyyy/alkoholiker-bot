using System.Reflection;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Interactions;

public sealed partial class DebugCommands
{
    [SubSlashCommand("config", "config")]
    public sealed class ConfigCommands : ApplicationCommandModule<ApplicationCommandContext>
    {
        private const BindingFlags BindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;


        [SubSlashCommand("get", "Get a config value.")]
        public async Task Get(Config.Field name, bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            try
            {
                PropertyInfo pInfo = typeof(Config).GetProperty(name.ToString(), BindingAttr);
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"`{nameof(Config)}.{pInfo.Name}` `({pInfo.PropertyType.Name})` has the value `{pInfo.GetValue(null)}`.",
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
        public async Task Set(Config.Field name, bool? @bool = null, string str = null, u8? u8 = null, i32? i32 = null, u32? u32 = null, i64? i64 = null, u64? u64 = null, f32? f32 = null, f64? f64 = null, string u64str = null, bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            try
            {
                object value = (object)@bool ?? (object)str ?? (object)u8 ?? (object)i32 ?? (object)u32 ?? (object)i64 ?? (object)u64 ?? (object)f32 ?? (object)f64 ?? (ulong.TryParse(u64str, out u64 parsedU64) ? (object)parsedU64 : null) ?? throw new($"No value specified.");

                PropertyInfo pInfo = typeof(Config).GetProperty(name.ToString(), BindingAttr);
                object oldValue = pInfo.GetValue(null);
                pInfo.SetValue(null, value);

                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"Changed the value of `{nameof(Config)}.{pInfo.Name}` `({pInfo.PropertyType.Name})` from `{oldValue}` to `{value}`.",
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
            if(!await App.CheckForOwner(Context))
                return;

            PropertyInfo[] properties = typeof(Config).GetProperties(BindingAttr);
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
            if(!await App.CheckForOwner(Context))
                return;

            Config.Load();

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Loaded config from `{Config.GetPath()}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("save", "Save the config to disk.")]
        public async Task Save(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            Config.Save();

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Saved config to `{Config.GetPath()}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
