using Microsoft.Extensions.Hosting;
using System.IO;

namespace NET_API.Services
{
    public class FontService
    {
        private readonly IHostEnvironment _environment;

        public FontService(IHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// 取得字體檔案路徑
        /// </summary>
        /// <returns>字體檔案路徑</returns>
        public string GetFontPath()
        {
            var fontPaths = new[]
            {
                Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "SourceHanSans-Regular.ttf"),
                Path.Combine(_environment.ContentRootPath, "wwwroot", "fonts", "NotoSansCJK-Regular.ttf")
            };

            foreach (var fontPath in fontPaths)
            {
                if (File.Exists(fontPath))
                {
                    Console.WriteLine($"找到字體檔案: {fontPath}");
                    return fontPath;
                }
            }

            Console.WriteLine("警告: 無法找到中文字體檔案");
            return string.Empty;
        }

        /// <summary>
        /// 檢查字體檔案是否存在
        /// </summary>
        /// <returns>是否存在可用的字體檔案</returns>
        public bool HasValidFont()
        {
            return !string.IsNullOrEmpty(GetFontPath());
        }
    }
} 