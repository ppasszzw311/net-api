using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Dtos.TaiPower;
using NET_API.Services;

namespace NET_API.Controllers.TaiPower
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaiPowerDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ExcelService _excelService;
        private readonly WordExportService _wordExportService;
        private readonly PdfExportService _pdfExportService;
        private readonly TaiPowerChartService _chartService;

        public TaiPowerDataController(
            ApplicationDbContext context,
            ExcelService excelService,
            WordExportService wordExportService,
            PdfExportService pdfExportService,
            TaiPowerChartService chartService)
        {
            _context = context;
            _excelService = excelService;
            _wordExportService = wordExportService;
            _pdfExportService = pdfExportService;
            _chartService = chartService;
        }

        // 修正時間方法：將資料庫中多出8小時的時間減去8小時
        private DateTime CorrectTime(DateTime dbTime)
        {
            // 確保時間是 UTC 時間，然後減去8小時
            var utcTime = dbTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dbTime, DateTimeKind.Utc)
                : dbTime;
            return utcTime.AddHours(8);
        }

        // 取得全部
        [HttpGet]
        public async Task<ActionResult<PowerDataResponse>> GetTaiPowerData()
        {
            var data = await _context.TaiPowers.ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
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
                Time = CorrectTime(data.Time),
                EastConsumption = data.EastConsumption ?? 0,
                CentralConsumption = data.CentralConsumption ?? 0,
                NorthConsumption = data.NorthConsumption ?? 0,
                SouthConsumption = data.SouthConsumption ?? 0,
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

            // 將輸入的日期加上8小時來匹配資料庫中的時間，並確保是 UTC 時間
            var dbStartDate = DateTime.SpecifyKind(startDate.Date.AddHours(8), DateTimeKind.Utc);
            var dbEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);

            var data = await _context.TaiPowers.Where(d => d.Time >= dbStartDate && d.Time < dbEndDate).ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
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
                    var eastData = await _context.TaiPowers.Where(d => d.EastConsumption.HasValue && d.EastConsumption > 0).ToListAsync();
                    response.Data = eastData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        EastConsumption = d.EastConsumption ?? 0,
                    }).ToList();
                    response.Count = eastData.Count;
                    break;
                case "central":
                    var centralData = await _context.TaiPowers.Where(d => d.CentralConsumption.HasValue && d.CentralConsumption > 0).ToListAsync();
                    response.Data = centralData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        CentralConsumption = d.CentralConsumption ?? 0,
                    }).ToList();
                    response.Count = centralData.Count;
                    break;
                case "north":
                    var northData = await _context.TaiPowers.Where(d => d.NorthConsumption.HasValue && d.NorthConsumption > 0).ToListAsync();
                    response.Data = northData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        NorthConsumption = d.NorthConsumption ?? 0,
                    }).ToList();
                    response.Count = northData.Count;
                    break;
                case "south":
                    var southData = await _context.TaiPowers.Where(d => d.SouthConsumption.HasValue && d.SouthConsumption > 0).ToListAsync();
                    response.Data = southData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        SouthConsumption = d.SouthConsumption ?? 0,
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

        // 匯出全部資料為 Excel
        [HttpGet("export/excel/all")]
        public async Task<IActionResult> ExportAllTaiPowerDataToExcel()
        {
            var data = await _context.TaiPowers.ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound("沒有找到任何台電資料");
            }

            var excelBytes = _excelService.ExportTaiPowerDataToExcel(response, "TaiPowerData_All");
            var fileName = $"TaiPowerData_All_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // 匯出特定時間區段的資料為 Excel
        [HttpGet("export/excel/range/{start}/{end}")]
        public async Task<IActionResult> ExportTaiPowerDataRangeToExcel(string start, string end)
        {
            var startDate = DateTime.TryParse(start, out var startDateResult) ? startDateResult : DateTime.MinValue;
            var endDate = DateTime.TryParse(end, out var endDateResult) ? endDateResult : DateTime.MaxValue;

            if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue)
            {
                return BadRequest("日期格式錯誤，請使用 yyyy-MM-dd 格式");
            }

            // 將輸入的日期加上8小時來匹配資料庫中的時間，並確保是 UTC 時間
            var dbStartDate = DateTime.SpecifyKind(startDate.Date.AddHours(8), DateTimeKind.Utc);
            var dbEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);

            var data = await _context.TaiPowers.Where(d => d.Time >= dbStartDate && d.Time < dbEndDate).ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound($"在 {start} 到 {end} 期間沒有找到任何台電資料");
            }

            var excelBytes = _excelService.ExportTaiPowerDataToExcel(response, $"TaiPowerData_{start}_{end}");
            var fileName = $"TaiPowerData_{start}_{end}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // 匯出特定地區的資料為 Excel
        [HttpGet("export/excel/region/{region}")]
        public async Task<IActionResult> ExportTaiPowerDataByRegionToExcel(string region)
        {
            var response = new PowerDataResponse();

            switch (region.ToLower())
            {
                case "east":
                    var eastData = await _context.TaiPowers.Where(d => d.EastConsumption.HasValue && d.EastConsumption > 0).ToListAsync();
                    response.Data = eastData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        EastConsumption = d.EastConsumption ?? 0,
                    }).ToList();
                    response.Count = eastData.Count;
                    break;
                case "central":
                    var centralData = await _context.TaiPowers.Where(d => d.CentralConsumption.HasValue && d.CentralConsumption > 0).ToListAsync();
                    response.Data = centralData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        CentralConsumption = d.CentralConsumption ?? 0,
                    }).ToList();
                    response.Count = centralData.Count;
                    break;
                case "north":
                    var northData = await _context.TaiPowers.Where(d => d.NorthConsumption.HasValue && d.NorthConsumption > 0).ToListAsync();
                    response.Data = northData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        NorthConsumption = d.NorthConsumption ?? 0,
                    }).ToList();
                    response.Count = northData.Count;
                    break;
                case "south":
                    var southData = await _context.TaiPowers.Where(d => d.SouthConsumption.HasValue && d.SouthConsumption > 0).ToListAsync();
                    response.Data = southData.Select(d => new PowerData
                    {
                        Time = CorrectTime(d.Time),
                        SouthConsumption = d.SouthConsumption ?? 0,
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

            var excelBytes = _excelService.ExportTaiPowerDataByRegionToExcel(response, region, $"TaiPowerData_{region}");
            var fileName = $"TaiPowerData_{region}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // 匯出全部資料為 Word
        [HttpGet("export/word/all")]
        public async Task<IActionResult> ExportAllTaiPowerDataToWord()
        {
            var data = await _context.TaiPowers.ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound("沒有找到任何台電資料");
            }

            var wordBytes = _wordExportService.ExportTaiPowerDataToWord(response, "TaiPowerData_All");
            var fileName = $"TaiPowerData_All_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

            return File(wordBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        // 匯出特定時間區段的資料為 Word
        [HttpGet("export/word/range/{start}/{end}")]
        public async Task<IActionResult> ExportTaiPowerDataRangeToWord(string start, string end)
        {
            var startDate = DateTime.TryParse(start, out var startDateResult) ? startDateResult : DateTime.MinValue;
            var endDate = DateTime.TryParse(end, out var endDateResult) ? endDateResult : DateTime.MaxValue;

            if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue)
            {
                return BadRequest("日期格式錯誤，請使用 yyyy-MM-dd 格式");
            }

            // 將輸入的日期加上8小時來匹配資料庫中的時間，並確保是 UTC 時間
            var dbStartDate = DateTime.SpecifyKind(startDate.Date.AddHours(8), DateTimeKind.Utc);
            var dbEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);

            var data = await _context.TaiPowers.Where(d => d.Time >= dbStartDate && d.Time < dbEndDate).ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound($"在 {start} 到 {end} 期間沒有找到任何台電資料");
            }

            var wordBytes = _wordExportService.ExportTaiPowerDataToWord(response, $"TaiPowerData_{start}_{end}");
            var fileName = $"TaiPowerData_{start}_{end}.docx";

            return File(wordBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        // 匯出全部資料為 PDF
        [HttpGet("export/pdf/all")]
        public async Task<IActionResult> ExportAllTaiPowerDataToPdf()
        {
            var data = await _context.TaiPowers.ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound("沒有找到任何台電資料");
            }

            var pdfBytes = await _pdfExportService.ExportTaiPowerDataToPdfAsync(response, "TaiPowerData_All");
            var fileName = $"TaiPowerData_All_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // 匯出特定時間區段的資料為 PDF
        [HttpGet("export/pdf/range/{start}/{end}")]
        public async Task<IActionResult> ExportTaiPowerDataRangeToPdf(string start, string end)
        {
            var startDate = DateTime.TryParse(start, out var startDateResult) ? startDateResult : DateTime.MinValue;
            var endDate = DateTime.TryParse(end, out var endDateResult) ? endDateResult : DateTime.MaxValue;

            if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue)
            {
                return BadRequest("日期格式錯誤，請使用 yyyy-MM-dd 格式");
            }

            // 將輸入的日期加上8小時來匹配資料庫中的時間，並確保是 UTC 時間
            var dbStartDate = DateTime.SpecifyKind(startDate.Date.AddHours(8), DateTimeKind.Utc);
            var dbEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);

            var data = await _context.TaiPowers.Where(d => d.Time >= dbStartDate && d.Time < dbEndDate).ToListAsync();
            var response = new PowerDataResponse();
            response.Data = data.Select(d => new PowerData
            {
                Time = CorrectTime(d.Time),
                EastConsumption = d.EastConsumption ?? 0,
                CentralConsumption = d.CentralConsumption ?? 0,
                NorthConsumption = d.NorthConsumption ?? 0,
                SouthConsumption = d.SouthConsumption ?? 0,
            }).ToList();
            response.Count = data.Count;

            if (response.Count == 0)
            {
                return NotFound($"在 {start} 到 {end} 期間沒有找到任何台電資料");
            }

            var pdfBytes = await _pdfExportService.ExportTaiPowerDataToPdfAsync(response, $"TaiPowerData_{start}_{end}");
            var fileName = $"TaiPowerData_{start}_{end}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // 添加範例資料
        [HttpPost("seed")]
        public async Task<IActionResult> SeedSampleData()
        {
            try
            {
                // 檢查是否已有資料
                var existingData = await _context.TaiPowers.CountAsync();
                if (existingData > 0)
                {
                    return BadRequest("資料庫中已有資料，無法重複添加範例資料");
                }

                var sampleData = new List<NET_API.Models.TaiPower.TaiPower>();
                var baseTime = DateTime.UtcNow.AddDays(-7); // 從7天前開始

                // 生成7天的範例資料，每小時一筆
                for (int day = 0; day < 7; day++)
                {
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var time = baseTime.AddDays(day).AddHours(hour);
                        var random = new Random((int)time.Ticks); // 使用時間作為隨機種子，確保可重現

                        sampleData.Add(new NET_API.Models.TaiPower.TaiPower
                        {
                            Time = time,
                            EastConsumption = Math.Round(random.NextDouble() * 500 + 200, 2), // 200-700 MW
                            CentralConsumption = Math.Round(random.NextDouble() * 800 + 400, 2), // 400-1200 MW
                            NorthConsumption = Math.Round(random.NextDouble() * 1000 + 600, 2), // 600-1600 MW
                            SouthConsumption = Math.Round(random.NextDouble() * 600 + 300, 2), // 300-900 MW
                            EastGeneration = Math.Round(random.NextDouble() * 400 + 150, 2), // 150-550 MW
                            CentralGeneration = Math.Round(random.NextDouble() * 700 + 350, 2), // 350-1050 MW
                            NorthGeneration = Math.Round(random.NextDouble() * 900 + 500, 2), // 500-1400 MW
                            SouthGeneration = Math.Round(random.NextDouble() * 500 + 250, 2), // 250-750 MW
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.TaiPowers.AddRangeAsync(sampleData);
                await _context.SaveChangesAsync();

                return Ok($"成功添加 {sampleData.Count} 筆範例資料");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"添加範例資料時發生錯誤: {ex.Message}");
            }
        }

        // 清除所有資料
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAllData()
        {
            try
            {
                var count = await _context.TaiPowers.CountAsync();
                if (count == 0)
                {
                    return BadRequest("資料庫中沒有資料可清除");
                }

                _context.TaiPowers.RemoveRange(_context.TaiPowers);
                await _context.SaveChangesAsync();

                return Ok($"成功清除 {count} 筆資料");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"清除資料時發生錯誤: {ex.Message}");
            }
        }

        // 生成全部資料的圖表 (PNG格式)
        [HttpGet("chart/all")]
        public async Task<IActionResult> GenerateAllDataChart()
        {
            try
            {
                var data = await _context.TaiPowers.OrderBy(d => d.Time).ToListAsync();
                
                if (data.Count == 0)
                {
                    return NotFound("沒有找到任何台電資料");
                }

                var powerData = data.Select(d => new PowerData
                {
                    Time = CorrectTime(d.Time),
                    EastConsumption = d.EastConsumption ?? 0,
                    CentralConsumption = d.CentralConsumption ?? 0,
                    NorthConsumption = d.NorthConsumption ?? 0,
                    SouthConsumption = d.SouthConsumption ?? 0,
                }).ToList();

                var chartBytes = await _chartService.GenerateChartPngAsync(powerData);
                var fileName = $"TaiPowerChart_All_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                return File(chartBytes, "image/png", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"生成圖表時發生錯誤: {ex.Message}");
            }
        }

        // 生成特定時間區段的圖表 (PNG格式)
        [HttpGet("chart/range/{start}/{end}")]
        public async Task<IActionResult> GenerateRangeDataChart(string start, string end)
        {
            try
            {
                var startDate = DateTime.TryParse(start, out var startDateResult) ? startDateResult : DateTime.MinValue;
                var endDate = DateTime.TryParse(end, out var endDateResult) ? endDateResult : DateTime.MaxValue;

                if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue)
                {
                    return BadRequest("日期格式錯誤，請使用 yyyy-MM-dd 格式");
                }

                // 將輸入的日期加上8小時來匹配資料庫中的時間，並確保是 UTC 時間
                var dbStartDate = DateTime.SpecifyKind(startDate.Date.AddHours(8), DateTimeKind.Utc);
                var dbEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);

                var data = await _context.TaiPowers
                    .Where(d => d.Time >= dbStartDate && d.Time < dbEndDate)
                    .OrderBy(d => d.Time)
                    .ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound($"在 {start} 到 {end} 期間沒有找到任何台電資料");
                }

                var powerData = data.Select(d => new PowerData
                {
                    Time = CorrectTime(d.Time),
                    EastConsumption = d.EastConsumption ?? 0,
                    CentralConsumption = d.CentralConsumption ?? 0,
                    NorthConsumption = d.NorthConsumption ?? 0,
                    SouthConsumption = d.SouthConsumption ?? 0,
                }).ToList();

                var chartBytes = await _chartService.GenerateChartPngAsync(powerData);
                var fileName = $"TaiPowerChart_{start}_{end}.png";

                return File(chartBytes, "image/png", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"生成圖表時發生錯誤: {ex.Message}");
            }
        }

        // 生成最近N天的圖表 (PNG格式)
        [HttpGet("chart/recent/{days}")]
        public async Task<IActionResult> GenerateRecentDataChart(int days = 7)
        {
            try
            {
                if (days < 1 || days > 365)
                {
                    return BadRequest("天數必須在1到365之間");
                }

                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                var data = await _context.TaiPowers
                    .Where(d => d.Time >= cutoffDate)
                    .OrderBy(d => d.Time)
                    .ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound($"沒有找到最近 {days} 天的台電資料");
                }

                var powerData = data.Select(d => new PowerData
                {
                    Time = CorrectTime(d.Time),
                    EastConsumption = d.EastConsumption ?? 0,
                    CentralConsumption = d.CentralConsumption ?? 0,
                    NorthConsumption = d.NorthConsumption ?? 0,
                    SouthConsumption = d.SouthConsumption ?? 0,
                }).ToList();

                var chartBytes = await _chartService.GenerateChartPngAsync(powerData);
                var fileName = $"TaiPowerChart_Recent{days}Days_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                return File(chartBytes, "image/png", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"生成圖表時發生錯誤: {ex.Message}");
            }
        }

        // 生成Base64格式的圖表
        [HttpGet("chart/base64/all")]
        public async Task<IActionResult> GenerateAllDataChartBase64()
        {
            try
            {
                var data = await _context.TaiPowers.OrderBy(d => d.Time).ToListAsync();
                
                if (data.Count == 0)
                {
                    return NotFound("沒有找到任何台電資料");
                }

                var powerData = data.Select(d => new PowerData
                {
                    Time = CorrectTime(d.Time),
                    EastConsumption = d.EastConsumption ?? 0,
                    CentralConsumption = d.CentralConsumption ?? 0,
                    NorthConsumption = d.NorthConsumption ?? 0,
                    SouthConsumption = d.SouthConsumption ?? 0,
                }).ToList();

                var chartResponse = await _chartService.GenerateChartBase64Async(powerData);
                
                return Ok(new
                {
                    success = true,
                    data = chartResponse,
                    message = "圖表生成成功"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"生成圖表時發生錯誤: {ex.Message}");
            }
        }

        // 生成Base64格式的特定時間區段圖表
        [HttpGet("chart/base64/range/{start}/{end}")]
        public async Task<IActionResult> GenerateRangeDataChartBase64(string start, string end)
        {
            try
            {
                var startDate = DateTime.TryParse(start, out var startDateResult) ? startDateResult : DateTime.MinValue;
                var endDate = DateTime.TryParse(end, out var endDateResult) ? endDateResult : DateTime.MaxValue;

                if (startDate == DateTime.MinValue || endDate == DateTime.MaxValue)
                {
                    return BadRequest("日期格式錯誤，請使用 yyyy-MM-dd 格式");
                }

                // 將輸入的日期加上8小時來匹配資料庫中的時間，並確保是 UTC 時間
                var dbStartDate = DateTime.SpecifyKind(startDate.Date.AddHours(8), DateTimeKind.Utc);
                var dbEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);

                var data = await _context.TaiPowers
                    .Where(d => d.Time >= dbStartDate && d.Time < dbEndDate)
                    .OrderBy(d => d.Time)
                    .ToListAsync();

                if (data.Count == 0)
                {
                    return NotFound($"在 {start} 到 {end} 期間沒有找到任何台電資料");
                }

                var powerData = data.Select(d => new PowerData
                {
                    Time = CorrectTime(d.Time),
                    EastConsumption = d.EastConsumption ?? 0,
                    CentralConsumption = d.CentralConsumption ?? 0,
                    NorthConsumption = d.NorthConsumption ?? 0,
                    SouthConsumption = d.SouthConsumption ?? 0,
                }).ToList();

                var chartResponse = await _chartService.GenerateChartBase64Async(powerData);
                
                return Ok(new
                {
                    success = true,
                    data = chartResponse,
                    message = "圖表生成成功",
                    period = $"{start} - {end}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"生成圖表時發生錯誤: {ex.Message}");
            }
        }

        // 檢查圖表服務健康狀態
        [HttpGet("chart/health")]
        public async Task<IActionResult> CheckChartServiceHealth()
        {
            try
            {
                var isHealthy = await _chartService.CheckHealthAsync();
                
                return Ok(new
                {
                    success = true,
                    chartServiceHealthy = isHealthy,
                    message = isHealthy ? "圖表服務運行正常" : "圖表服務無法連接",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    chartServiceHealthy = false,
                    message = $"檢查圖表服務健康狀態時發生錯誤: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}