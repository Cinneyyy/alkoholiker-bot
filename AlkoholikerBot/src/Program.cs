using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace src;

public class Program
{

    private DiscordSocketClient client;
    private IServiceProvider services;


    public static DateTime startTime { get; private set; }
    public static string dataPath { get; private set; }
    public static string timeStr => DateTime.Now.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");


    private async Task RunBotAsync()
    {
        dataPath = File.ReadAllLines("datapath.txt").First();
        startTime = DateTime.UtcNow;

        EnvReader.Load($"{dataPath}/.env");
        RuleMgr.rules.Load(File.ReadAllText($"{dataPath}/rules.json"));

        client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All ^ GatewayIntents.GuildPresences ^ GatewayIntents.GuildScheduledEvents ^ GatewayIntents.GuildInvites,
            MessageCacheSize = 128
        });

        services = new ServiceCollection()
            .AddSingleton(client)
            .AddSingleton(service => new InteractionService(service.GetRequiredService<DiscordSocketClient>(), new() 
            { 
                DefaultRunMode = RunMode.Async
            }))
            .BuildServiceProvider();

        InteractionService interactions = services.GetRequiredService<InteractionService>();

        client.Log += async args => Console.WriteLine(args);

        await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);

#if RELEASE
        client.Ready += async () => await interactions.RegisterCommandsGloballyAsync();
#elif DEBUG
        client.Ready += async () => await interactions.RegisterCommandsToGuildAsync(u64.Parse(Environment.GetEnvironmentVariable("DEBUG_GUILD")));
#endif

        client.ModalSubmitted += ModalHandler.Handle;

        client.ButtonExecuted += ButtonExecutedHandler.Handle;

        client.InteractionCreated += async interaction =>
        {
            if(interaction.Type is InteractionType.ModalSubmit or InteractionType.MessageComponent)
                return;

            try
            {
                SocketInteractionContext ctx = new(client, interaction);
                IResult result = await interactions.ExecuteCommandAsync(ctx, services);

                if(!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        };

        client.MessageReceived += async args =>
        {
            if(args is not SocketUserMessage message || message.Author.IsBot)
                return;

            Console.WriteLine($"[{timeStr}] {FormatMessage(message)}");

            if(message.Content.StartsWith('/'))
                return;

            try
            {
                RuleMgr.HandleMessage(message);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        };

        client.MessageDeleted += async (message, channel)
            => Console.WriteLine($"[{timeStr}] Message deleted in {channel.Value?.Name ?? "[unknown channel]"} ({FormatMessage(message.Value)})");

        AppDomain.CurrentDomain.ProcessExit += async (_, _) => await client.StopAsync();

        string token = Environment.GetEnvironmentVariable("BOT_TOKEN");
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await client.SetCustomStatusAsync("Thinking about alcohol");

        await Task.Delay(-1);
    }


    public static string FormatMessage(IMessage msg)
    {
        if(msg is null)
            return "[null]";

        return $"{msg.Author.Username} ({msg.Author.Id}): {msg.Content} [{msg.Attachments.Count} attachment(s)]";
    }


    private static void Main()
        => new Program().RunBotAsync().GetAwaiter().GetResult();
}
