using System.IO;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace NET_API.Services
{
    public class FontService
    {
        private readonly IHostEnvironment _environment;
        private SKTypeface? _chineseFont;
        private readonly object _fontLock = new object();

        public FontService(IHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// 取得繁體中文字體
        /// </summary>
        /// <returns>SkiaSharp 字體物件</returns>
        public SKTypeface GetChineseFont()
        {
            if (_chineseFont != null)
                return _chineseFont;

            lock (_fontLock)
            {
                if (_chineseFont != null)
                    return _chineseFont;

                _chineseFont = LoadChineseFont();
                return _chineseFont;
            }
        }

        /// <summary>
        /// 載入繁體中文字體
        /// </summary>
        /// <returns>字體物件</returns>
        private SKTypeface LoadChineseFont()
        {
            // 嘗試多個字體路徑
            var fontPaths = new[]
            {
                // 專案內的字體檔案
                Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "NotoSansCJK-Regular.ttc"),
                Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "NotoSansCJK-Regular.otf"),
                
                // 系統字體路徑 (macOS)
                "/System/Library/Fonts/PingFang.ttc",
                "/System/Library/Fonts/STHeiti Light.ttc",
                "/System/Library/Fonts/STHeiti Medium.ttc",
                
                // 系統字體路徑 (Linux)
                "/usr/share/fonts/truetype/noto/NotoSansCJK-Regular.ttc",
                "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.otf",
                
                // 通用字體路徑
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                "/System/Library/Fonts/Arial.ttf"
            };

            foreach (var fontPath in fontPaths)
            {
                try
                {
                    if (File.Exists(fontPath))
                    {
                        var font = SKTypeface.FromFile(fontPath);
                        if (font != null)
                        {
                            Console.WriteLine($"成功載入字體: {fontPath}");
                            return font;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"載入字體失敗 {fontPath}: {ex.Message}");
                }
            }

            // 如果所有字體都無法載入，使用預設字體
            Console.WriteLine("警告: 無法載入中文字體，使用預設字體");
            return SKTypeface.Default;
        }

        /// <summary>
        /// 檢查字體是否支援中文字符
        /// </summary>
        /// <param name="font">字體物件</param>
        /// <param name="text">要檢查的文字</param>
        /// <returns>是否支援</returns>
        public bool IsChineseFontSupported(SKTypeface font, string text)
        {
            if (font == null) return false;

            try
            {
                // 簡單檢查：如果字體不是預設字體，假設支援中文
                return font != SKTypeface.Default;
            }
            catch
            {
                // 如果檢查失敗，假設支援
                return font != SKTypeface.Default;
            }
        }

        /// <summary>
        /// 取得字體檔案路徑
        /// </summary>
        /// <returns>字體檔案路徑</returns>
        public string GetFontPath()
        {
            var fontPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "NotoSansCJK-Regular.ttc");
            return File.Exists(fontPath) ? fontPath : string.Empty;
        }
    }
} 