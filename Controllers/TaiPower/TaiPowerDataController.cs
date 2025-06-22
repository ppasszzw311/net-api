using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Dtos.TaiPower;

namespace NET_API.Controllers.TaiPower
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaiPowerDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaiPowerDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 取得全部
        [HttpGet]
        public async Task<ActionResult<PowerDataResponse>> GetTaiPowerData()
        {
            var data = await _context.TaiPowers.ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = d.Time,
                EastConsumption = d.EastConsumption,
                CentralConsumption = d.CentralConsumption,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound("沒有找到任何台電資料");
            }

            return response;
        }

        // 取得最近一筆
        [HttpGet("latest")]
        public async Task<ActionResult<PowerData>> GetLatestTaiPowerData()
        {
            var data = await _context.TaiPowers.OrderByDescending(d => d.Time).FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound("沒有找到任何台電資料");
            }

            return new PowerData
            {
                Time = data.Time,
                EastConsumption = data.EastConsumption,
                CentralConsumption = data.CentralConsumption,
            };
        }

        // 取得特定時間區段
        [HttpGet("range/{start}/{end}")]
        public async Task<ActionResult<PowerDataResponse>> GetTaiPowerDataRange(string start, string end)
        {
            var startDate = DateTime.TryParse(start, out var startDateResult) ? startDateResult : DateTime.MinValue;
            var endDate = DateTime.TryParse(end, out var endDateResult) ? endDateResult : DateTime.MaxValue;

            if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue)
            {
                return BadRequest("日期格式錯誤，請使用 yyyy-MM-dd 格式");
            }

            var data = await _context.TaiPowers.Where(d => d.Time >= startDate && d.Time <= endDate).ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = d.Time,
                EastConsumption = d.EastConsumption,
                CentralConsumption = d.CentralConsumption,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound($"在 {start} 到 {end} 期間沒有找到任何台電資料");
            }

            return response;
        }

        // 選擇特定地區的電力資料
        [HttpGet("region/{region}")]
        public async Task<ActionResult<PowerDataResponse>> GetTaiPowerDataByRegion(string region)
        {
            var response = new PowerDataResponse();

            switch (region.ToLower())
            {
                case "east":
                    var eastData = await _context.TaiPowers.Where(d => d.EastConsumption > 0).ToListAsync();
                    response.Data = eastData.Select(d => new PowerData
                    {
                        Time = d.Time,
                        EastConsumption = d.EastConsumption,
                    }).ToList();
                    response.Count = eastData.Count;
                    break;
                case "central":
                    var centralData = await _context.TaiPowers.Where(d => d.CentralConsumption > 0).ToListAsync();
                    response.Data = centralData.Select(d => new PowerData
                    {
                        Time = d.Time,
                        CentralConsumption = d.CentralConsumption,
                    }).ToList();
                    response.Count = centralData.Count;
                    break;
                case "north":
                    var northData = await _context.TaiPowers.Where(d => d.NorthConsumption > 0).ToListAsync();
                    response.Data = northData.Select(d => new PowerData
                    {
                        Time = d.Time,
                        NorthConsumption = d.NorthConsumption,
                    }).ToList();
                    response.Count = northData.Count;
                    break;
                case "south":
                    var southData = await _context.TaiPowers.Where(d => d.SouthConsumption > 0).ToListAsync();
                    response.Data = southData.Select(d => new PowerData
                    {
                        Time = d.Time,
                        SouthConsumption = d.SouthConsumption,
                    }).ToList();
                    response.Count = southData.Count;
                    break;
                default:
                    return BadRequest("無效的地區參數，請使用 east、central、north 或 south");
            }

            if (response.Count == 0)
            {
                return NotFound($"沒有找到 {region} 地區的台電資料");
            }

            return response;
        }
    }
}