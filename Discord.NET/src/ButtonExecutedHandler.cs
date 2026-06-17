using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace src;

public static class ButtonExecutedHandler
{
    public static readonly List<(string username, string bet)> rockPaperScissorsBets = [];
    // public static readonly List<(string id, SocketUserMessage msg)> openPolls = [];


    public static async Task Handle(SocketMessageComponent com)
    {
        switch(com.Data.CustomId)
        {
            case "rock" or "paper" or "scissors":
            {
                string username = com.User.GlobalName;
                if(rockPaperScissorsBets.Exists(b => b.username == username))
                {
                    i32 index = rockPaperScissorsBets.FindIndex(r => r.username == username);
                    rockPaperScissorsBets[index] = rockPaperScissorsBets[index] with { bet = com.Data.CustomId };
                    break;
                }
                else
                    rockPaperScissorsBets.Add((username, com.Data.CustomId));

                await com.RespondAsync($"Sucessfully locked in {GetRockPapersScissorsChoiceString(com.Data.CustomId)}.", ephemeral: true);
                await com.FollowupAsync($"{username} locked something in.", flags: MessageFlags.SuppressNotification);
                
                break;
            }

            case "reveal":
            {
                if(rockPaperScissorsBets is [])
                {
                    await com.RespondAsync("Failed to reveal rock paper scissors results.", ephemeral: true, flags: MessageFlags.SuppressNotification);
                    break;
                }

                rockPaperScissorsBets.Sort((a, b) => a.CompareTo(b));

                await com.RespondAsync(embed: new EmbedBuilder()
                {
                    Title = "Ergebnisse",
                    Description = string
                        .Join("\n", rockPaperScissorsBets
                        .Select(r => $"{r.username}: {GetRockPapersScissorsChoiceString(r.bet)}"))
                }.Build());
            
                rockPaperScissorsBets.Clear();
            
                break;
            }
        //     case var _ when com.Data.CustomId.StartsWith("end_poll_btn"):
        //     {
        //         try
        //         {
        //             if(openPolls.Find(p => p.id == com.Data.CustomId.Split('#')[1]) is (string id, SocketUserMessage msg) && msg is not null)
        //             {
        //                 await msg.EndPollAsync();
        //                 await com.Message?.DeleteAsync();
        //                 openPolls.Remove((id, msg));
        //                 Console.WriteLine($"{com.User.Username}'s request to end their poll was completed successfully.");
        //             }
        //             else
        //                 throw new("Poll was not registered.");
        //         }
        //         catch(Exception e)
        //         {
        //             Console.WriteLine($"Failed to fulfill {com.User.Username}'s request to end their poll ({e.Message}).");
        //         }
        //         openPolls.RemoveAll(p => p.msg is null || !p.msg.Poll.HasValue || p.msg.Poll.Value.ExpiresAt.DateTime.Ticks <= DateTime.Now.Ticks);
        //         Console.WriteLine($"{openPolls} open polls are currently stored in memory.");

        //         break;   
        //     }

        //     default:
        //     {
        //         Console.WriteLine($"Unknown component custom ID: {com.Data.CustomId}");
        //         break;
        //     }
        }
    }


    private static string GetRockPapersScissorsChoiceString(string choice)
        => choice switch
        {
            "rock" => $"{Emoji.Parse(":rock:")} Stein",
            "paper" => $"{Emoji.Parse(":roll_of_paper:")} Papier",
            "scissors" => $"{Emoji.Parse(":scissors:")} Schere",
            _ => "[error]"
        };
}