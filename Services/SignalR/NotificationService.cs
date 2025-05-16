using Microsoft.AspNetCore.SignalR;
using NET_API.Services.SignalR;

namespace NET_API.Services
{
  // 通知服務
  public class NotificationService
  {
    // 注入我們的聊天室chatHub
    private readonly IHubContext<ChatHub> _hubContext;

    public NotificationService(IHubContext<ChatHub> hubContext)
    {
      _hubContext = hubContext;
    }

    // 發送資料庫變更通知 
    public async Task SendDatabaseChangeNotification(string message, object data)
    {
      await _hubContext.Clients.All.SendAsync("ReceiveDatabaseChange", message, data);
    }

    // 發送特定用戶的通知
    public async Task SendUserNotification(string userId, string message, object data)
    {
      await _hubContext.Clients.User(userId).SendAsync("ReceiveUserNotification", message, data);
    }
  }
}