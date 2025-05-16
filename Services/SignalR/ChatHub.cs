using Microsoft.AspNetCore.SignalR;

namespace NET_API.Services.SignalR
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // 當用戶連接時，可以將其加入特定的組
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // 當用戶斷開連接時的處理
            await base.OnDisconnectedAsync(exception);
        }

        // 用戶可以加入特定的通知組
        public async Task JoinNotificationGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // 用戶可以離開特定的通知組
        public async Task LeaveNotificationGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
