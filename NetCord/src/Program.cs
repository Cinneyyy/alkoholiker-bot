using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using src;
using src.Rules;
using src.Rules.Opt;
using src.Soundboard;

Secrets.Load(
#if DEBUG
    "secrets.debug.json"
#elif RELEASE
    "secrets.release.json"
#endif
);

App.Load();
Console.WriteLine($"Data path: {App.dataPath}");

Config.SetPath(App.GetPath("config.json"));
Config.Load();

RuleMgr.SetPath(App.GetPath("rules"));
RuleMgr.Load();

SoundboardDb.SetPath(App.GetPath("soundboard"));
SoundboardDb.Load();

OptMgr.SetPath(App.GetPath("opted_out"));

HostApplicationBuilder hostBuilder = Host.CreateApplicationBuilder(args);

hostBuilder.Services
    .AddDiscordGateway(options =>
    {
        options.Token = Secrets.token;
        options.Intents = GatewayIntents.All;
    })
    .AddApplicationCommands(options =>
    {
        if(Secrets.guild != 0ul)
            options.AutoRegisterCommands = false;
    })
    .AddGatewayHandlers(typeof(Program).Assembly)
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>();

IHost host = hostBuilder.Build();
RestClient client = host.Services.GetRequiredService<RestClient>();
App.SetClient(client);

host.AddModules(typeof(Program).Assembly);

if(Secrets.guild != 0ul)
{
    ApplicationCommandService<ApplicationCommandContext> service = host.Services.GetRequiredService<ApplicationCommandService<ApplicationCommandContext>>();
    ApplicationCommandProperties[] properties = await Task.WhenAll(service.GetCommands().Where(c => c.Name is not ("some" or "filter" or "if" or "needed")).Select(c => c.GetRawValueAsync().AsTask()));
    u64 guildId = Secrets.guild;
    await client.BulkOverwriteGuildApplicationCommandsAsync(((IEntityToken)client.Token!).Id, guildId, properties);
}

await host.RunAsync();
