using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NET_API.Dtos.Notify;
using NET_API.Services;
namespace NET_API.Controllers.Notification
{
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly NotificationService _notificationService;

        public NotificationController(ILogger<NotificationController> logger, NotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        // 測試用來進行推播的方法
        [HttpPost]
        public async Task<IActionResult> Notify([FromBody] NotifyDto notifyDto)
        {
            await _notificationService.SendDatabaseChangeNotification(notifyDto.Message, notifyDto.Data);
            return Ok();
        }
    }
}