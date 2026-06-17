using System;
using NetCord;
using NetCord.Rest;

Console.Write("Token: ");
string token = Console.ReadLine();

Console.Write("Guild: ");
bool isValidGuild = ulong.TryParse(Console.ReadLine(), out ulong guild);

RestClient restClient = new(new BotToken(token));
Application application = await restClient.GetCurrentApplicationAsync();

await restClient.BulkOverwriteGlobalApplicationCommandsAsync(application.Id, []);
Console.WriteLine("Overwrote global application commands.");

if(isValidGuild && guild != 0ul)
{
    await restClient.BulkOverwriteGuildApplicationCommandsAsync(application.Id, guild, []);
    Console.WriteLine("Overwrote guild application commands.");
}
