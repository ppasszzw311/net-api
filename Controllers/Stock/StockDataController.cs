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
        public async Task<List<StockData>> GetStockDataByDate([FromQuery]string? startDate, [FromQuery] string? endDate)
        {
            var taipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
            DateTime taipeiTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taipeiTimeZone);
            DateTime DateStart = DateTime.Parse(startDate);
            DateTime DateEnd = DateTime.Parse(endDate);
            var query = _context.StockDatas.AsQueryable();

            // 如果都沒有值，給當天的
            if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                var nextDay = taipeiTime.Date.AddDays(1);
                query = query.Where(x => x.Timestamp >= taipeiTime.Date && x.Timestamp < nextDay);
            } else if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                query = query.Where( x => x.Timestamp >= DateStart.Date && x.Timestamp < DateEnd);
            } 
            else if (!string.IsNullOrEmpty(startDate))
            {
                var nextDay = DateStart.Date.AddDays(1);
                query = query.Where(x => x.Timestamp >= DateStart && x.Timestamp < DateEnd);
            }

            return await query.ToListAsync();
        }
    }
}