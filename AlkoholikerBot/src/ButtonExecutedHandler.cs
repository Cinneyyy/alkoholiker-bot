using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace src;

public static class ButtonExecutedHandler
{
    // public static readonly List<(string id, SocketUserMessage msg)> openPolls = [];


    public static async Task Handle(SocketMessageComponent com)
    {
        // switch(com.Data.CustomId)
        // {
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
        // }
    }
}