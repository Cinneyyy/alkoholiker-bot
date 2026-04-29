using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace src;

public static class ModalHandler
{
    public static async Task Handle(SocketModal modal)
    {
        switch(modal.Data.CustomId)
        {
            case "optout_modal":
            {
                if(modal.Data.Components.FirstOrDefault(c => c.CustomId == "optin_cb")?.BoolValue is bool optIn)
                {
                    u64 user = modal.User.Id;
                    bool changed = optIn ? OptOutMgr.OptIn(user) : OptOutMgr.OptOut(user);

                    if(!changed)
                    {
                        Console.WriteLine($"User with ID {user} ({modal.User.Username}) tried to opt {(optIn ? "in" : "out")}, but nothing changed.");
                        await modal.RespondAsync($"You were already opted {(optIn ? "in" : "out")}.", ephemeral: true);
                    }
                    else
                    {
                        Console.WriteLine($"User with ID {user} ({modal.User.Username}) opted {(optIn ? "in" : "out")}.");
                        await modal.RespondAsync($"Successfully opted {(optIn ? "in" : "out")}.", ephemeral: true);
                    }
                }

                break;
            }

            default:
            {
                Console.WriteLine($"Unknown modal custom ID: {modal.Data.CustomId}");
                break;
            }
        }
    }
}