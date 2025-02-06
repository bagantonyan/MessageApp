using MessageApp.API.Models.Messages;
using MessageApp.DAL.Entities;
using Microsoft.AspNetCore.SignalR;

namespace MessageApp.API.Hubs
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(MessageResponseModel message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}