using NET_API.Dtos.TaiPower;
using SkiaSharp;
using System.IO;

namespace NET_API.Services
{
  public class ChartService
  {
    /// <summary>
    /// 生成台電用電量折線圖
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <returns>圖表圖片</returns>
    public SKBitmap GenerateTaiPowerLineChart(PowerDataResponse data)
    {
      if (data.Data.Count == 0) return null;

      const int width = 800;
      const int height = 400;
      const int margin = 50;

      var bitmap = new SKBitmap(width, height);
      using (var canvas = new SKCanvas(bitmap))
      {
        // 清除背景
        canvas.Clear(SKColors.White);

        // 準備資料
        var eastValues = data.Data.Select(d => d.EastConsumption ?? 0).ToArray();
        var centralValues = data.Data.Select(d => d.CentralConsumption ?? 0).ToArray();
        var northValues = data.Data.Select(d => d.NorthConsumption ?? 0).ToArray();
        var southValues = data.Data.Select(d => d.SouthConsumption ?? 0).ToArray();

        // 計算範圍
        var allValues = eastValues.Concat(centralValues).Concat(northValues).Concat(southValues);
        var minValue = allValues.Min();
        var maxValue = allValues.Max();
        var valueRange = maxValue - minValue;

        // 繪製座標軸
        var axisPaint = new SKPaint
        {
          Color = SKColors.Black,
          StrokeWidth = 2,
          IsAntialias = true
        };
        canvas.DrawLine(margin, height - margin, width - margin, height - margin, axisPaint); // X軸
        canvas.DrawLine(margin, margin, margin, height - margin, axisPaint); // Y軸

        // 繪製標題
        var titlePaint = new SKPaint
        {
          Color = SKColors.Black,
          TextSize = 16,
          IsAntialias = true,
          FakeBoldText = true
        };
        canvas.DrawText("TaiPower Regional Power Consumption Trend", width / 2 - 200, 30, titlePaint);

        // 繪製Y軸標籤
        var labelPaint = new SKPaint
        {
          Color = SKColors.Black,
          TextSize = 8,
          IsAntialias = true
        };
        for (int i = 0; i <= 5; i++)
        {
          var value = minValue + (valueRange * i / 5);
          var y = height - margin - (height - 2 * margin) * i / 5;
          canvas.DrawText(value.ToString("F0"), 5, y + 5, labelPaint);
        }

        // 繪製折線
        var colors = new[] { SKColors.Red, SKColors.Blue, SKColors.Green, SKColors.Orange };
        var regions = new[] { eastValues, centralValues, northValues, southValues };
        var regionNames = new[] { "East", "Central", "North", "South" };

        for (int r = 0; r < regions.Length; r++)
        {
          var values = regions[r];
          var linePaint = new SKPaint
          {
            Color = colors[r],
            StrokeWidth = 2,
            IsAntialias = true
          };
          var pointPaint = new SKPaint
          {
            Color = colors[r],
            IsAntialias = true
          };

          for (int i = 1; i < values.Length; i++)
          {
            var x1 = margin + (width - 2 * margin) * (i - 1) / (values.Length - 1);
            var y1 = height - margin - (height - 2 * margin) * (values[i - 1] - minValue) / valueRange;
            var x2 = margin + (width - 2 * margin) * i / (values.Length - 1);
            var y2 = height - margin - (height - 2 * margin) * (values[i] - minValue) / valueRange;

            canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, linePaint);
            canvas.DrawCircle((float)x1, (float)y1, 3, pointPaint);
          }
        }

        // 繪製圖例
        var legendY = height - 30;
        for (int i = 0; i < regionNames.Length; i++)
        {
          var x = margin + i * 150;
          var legendPaint = new SKPaint
          {
            Color = colors[i],
            IsAntialias = true
          };
          canvas.DrawRect(x, legendY, 15, 15, legendPaint);
          canvas.DrawText(regionNames[i], x + 20, legendY + 12, labelPaint);
        }
      }

      return bitmap;
    }

    /// <summary>
    /// 將圖表轉換為 PNG 格式的 byte 陣列
    /// </summary>
    /// <param name="data">台電資料</param>
    /// <returns>PNG 格式的 byte 陣列</returns>
    public byte[] GenerateTaiPowerLineChartAsPng(PowerDataResponse data)
    {
      var chartImage = GenerateTaiPowerLineChart(data);
      if (chartImage == null) return null;

      using (var stream = new MemoryStream())
      {
        var image = SKImage.FromBitmap(chartImage);
        var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
        return encodedData.ToArray();
      }
    }
  }
}