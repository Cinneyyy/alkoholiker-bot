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
using src.Soundboard;

Secrets.Load(
#if DEBUG
    "secrets.debug.json"
#elif RELEASE
    "secrets.release.json"
#endif
);
Config.Load("config.json");
App.Load();
SoundboardDb.Load();

Console.WriteLine($"Data path: {App.dataPath}");

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

host.AddModules(typeof(Program).Assembly);

if(Secrets.guild != 0ul)
{
    IServiceProvider services = host.Services;
    ApplicationCommandService<ApplicationCommandContext> service = services.GetRequiredService<ApplicationCommandService<ApplicationCommandContext>>();
    RestClient client = services.GetRequiredService<RestClient>();
    ApplicationCommandProperties[] properties = await Task.WhenAll(service.GetCommands().Where(c => c.Name is not ("some" or "filter" or "if" or "needed")).Select(c => c.GetRawValueAsync().AsTask()));
    u64 guildId = Secrets.guild;
    await client.BulkOverwriteGuildApplicationCommandsAsync(((IEntityToken)client.Token!).Id, guildId, properties);
}

await host.RunAsync();