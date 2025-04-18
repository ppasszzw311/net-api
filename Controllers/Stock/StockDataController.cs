using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Models.Stock;

namespace NET_API.Controllers.Stock
{
    [ApiController]
    [Route("[controller]")]
    public class StockDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StockDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockData>>> GetStockData()
        {
            return await _context.StockDatas.ToListAsync();
        }

        // 依照代號取得股價列表
        [HttpGet("stmbol/{symbol}")]
        public async Task<ActionResult<IEnumerable<StockData>>> GetStockBySymbol(string symbol)
        {
            var stockList = await _context.StockDatas
                .Where(s => s.Symbol == symbol)
                .ToArrayAsync();

            if (stockList.Length == 0)
            {
                return NotFound();
            }

            return stockList;
        }

        // 依據日期取得結果 
        [HttpGet("date")]
        public async Task<ActionResult> GetStockDataByDate([FromQuery]string? startDate, [FromQuery] string? endDate)
        {
            var taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
            var query = _context.StockDatas.AsQueryable();

            if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                var todayTaipei = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taipeiZone).Date;
                var todayUtcStart = TimeZoneInfo.ConvertTimeToUtc(todayTaipei, taipeiZone);
                var todayUtcEnd = todayUtcStart.AddDays(1);

                query = query.Where(x => x.Timestamp >= todayUtcStart && x.Timestamp < todayUtcEnd);
            }
            else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                if (!DateTime.TryParse(startDate, out var parsedStart) || !DateTime.TryParse(endDate, out var parsedEnd))
                    return BadRequest("日期格式錯誤");

                var utcStart = TimeZoneInfo.ConvertTimeToUtc(parsedStart.Date, taipeiZone);
                var utcEnd = TimeZoneInfo.ConvertTimeToUtc(parsedEnd.Date.AddDays(1), taipeiZone);

                query = query.Where(x => x.Timestamp >= utcStart && x.Timestamp < utcEnd);
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                if (!DateTime.TryParse(startDate, out var parsedStart))
                    return BadRequest("startDate 格式錯誤");

                var utcStart = TimeZoneInfo.ConvertTimeToUtc(parsedStart.Date, taipeiZone);
                var utcEnd = utcStart.AddDays(1);

                query = query.Where(x => x.Timestamp >= utcStart && x.Timestamp < utcEnd);
            }

            return Ok(await query.ToListAsync());
        }
    }
}