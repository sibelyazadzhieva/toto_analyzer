using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TotoAnalyzer.Models;

namespace TotoAnalyzer.Services
{
    public class DataLoader
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://info.toto.bg/statistika/6x49";

        public DataLoader()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };

            _httpClient = new HttpClient(handler);

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "bg-BG,bg;q=0.9,en-US;q=0.8,en;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }

        public async Task<IEnumerable<Draw>> LoadAllDataAsync()
        {
            var draws = new List<Draw>();

            Console.WriteLine("Изтегляне на главната страница...");
            string htmlContent = "";
            try
            {
                htmlContent = await _httpClient.GetStringAsync(BaseUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при връзка със сайта: {ex.Message}");
                return draws;
            }

            var linkRegex = new Regex(@"href\s*=\s*['""]([^'""]*\.(?:txt|docx))['""]", RegexOptions.IgnoreCase);
            var matches = linkRegex.Matches(htmlContent);

            var fileUrls = matches.Select(m => m.Groups[1].Value)
                                  .Select(url => url.StartsWith("http") ? url : $"https://info.toto.bg{url}")
                                  .Distinct()
                                  .ToList();

            if (fileUrls.Count == 0)
            {
                var aggressiveRegex = new Regex(@"['""]([^'""]*\.(?:txt|docx))['""]", RegexOptions.IgnoreCase);
                fileUrls = aggressiveRegex.Matches(htmlContent)
                                  .Select(m => m.Groups[1].Value)
                                  .Select(url => url.StartsWith("http") ? url : $"https://info.toto.bg{url}")
                                  .Distinct()
                                  .ToList();
            }

            if (fileUrls.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nВНИМАНИЕ: Не са намерени файлове!");
                Console.WriteLine("Сървърът на Спорт Тото най-вероятно е активирал 'Защита от ботове' (напр. Cloudflare) поради твърде чести заявки.");
                Console.WriteLine("Това е нормално. Решение: Изчакайте 10-15 минути и опитайте отново.");
                Console.ResetColor();
                return draws;
            }

            Console.WriteLine($"Намерени са {fileUrls.Count} файла за обработка.");

            foreach (var url in fileUrls)
            {
                Console.WriteLine($"Обработка на: {url}");
                try
                {
                    if (url.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        var textData = await _httpClient.GetStringAsync(url);
                        draws.AddRange(ParseTextContent(textData, ExtractYearFromUrl(url)));
                    }
                    else if (url.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                    {
                        var docxBytes = await _httpClient.GetByteArrayAsync(url);
                        draws.AddRange(ParseDocxContent(docxBytes, ExtractYearFromUrl(url)));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Грешка при файла {url}: {ex.Message}");
                }
            }

            return draws;
        }

        private IEnumerable<Draw> ParseTextContent(string content, int year)
        {
            var draws = new List<Draw>();
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var draw = TryParseLine(line, year);
                if (draw != null) draws.Add(draw);
            }
            return draws;
        }

        private IEnumerable<Draw> ParseDocxContent(byte[] fileBytes, int year)
        {
            var draws = new List<Draw>();
            using (var stream = new MemoryStream(fileBytes))
            using (var wordDoc = WordprocessingDocument.Open(stream, false))
            {
                var body = wordDoc.MainDocumentPart?.Document.Body;
                if (body == null) return draws;

                foreach (var para in body.Elements<Paragraph>())
                {
                    var draw = TryParseLine(para.InnerText, year);
                    if (draw != null) draws.Add(draw);
                }
            }
            return draws;
        }

        private Draw TryParseLine(string line, int year)
        {
            var numbersRegex = new Regex(@"\b([1-4]?\d)\b");
            var matches = numbersRegex.Matches(line);

            if (matches.Count >= 7)
            {
                var numbers = matches.Select(m => int.Parse(m.Value)).ToList();
                var drawnNumbers = numbers.Skip(numbers.Count - 6).Take(6).Where(n => n >= 1 && n <= 49).ToList();

                if (drawnNumbers.Count == 6)
                {
                    return new Draw
                    {
                        Year = year,
                        DrawNumber = numbers.First(),
                        Numbers = drawnNumbers
                    };
                }
            }
            return null;
        }

        private int ExtractYearFromUrl(string url)
        {
            var match = Regex.Match(url, @"20\d{2}");
            return match.Success ? int.Parse(match.Value) : DateTime.Now.Year;
        }
    }
}