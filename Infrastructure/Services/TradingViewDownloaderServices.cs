using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Application.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Infrastructure.Services;

public class TradingViewDownloaderServices : ITradingViewDownloaderServices
{
    public  void DescargarImagenes(List<(int Id, string Url)> dataset)
    {
        string carpeta = "imagenes";
        Directory.CreateDirectory(carpeta);

        using var http = new HttpClient();

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");

        using var driver = new ChromeDriver(options);

        using var writer = new StreamWriter("mapping.csv");
        writer.WriteLine("numeric_id,chart_id,url,filename");

        foreach (var item in dataset)
        {
            try
            {
                Console.WriteLine($"Procesando ID {item.Id}");

                // 🔑 Extraer Chart ID
                var match = Regex.Match(item.Url, @"/x/([A-Za-z0-9]+)/");
                if (!match.Success)
                {
                    Console.WriteLine($"URL inválida: {item.Url}");
                    continue;
                }

                string chartId = match.Groups[1].Value;

                driver.Navigate().GoToUrl(item.Url);
                Thread.Sleep(3000); // esperar carga

                var imgs = driver.FindElements(By.TagName("img"));

                string imgUrl = null;

                foreach (var img in imgs)
                {
                    var src = img.GetAttribute("src");

                    if (!string.IsNullOrEmpty(src) &&
                        (src.Contains("s3") || src.Contains("tradingview")))
                    {
                        imgUrl = src;
                        break;
                    }
                }

                if (imgUrl == null)
                {
                    Console.WriteLine($"❌ No se encontró imagen en {item.Url}");
                    continue;
                }

                var bytes = http.GetByteArrayAsync(imgUrl).Result;

                string fileName = $"{item.Id}_{chartId}.png";
                string path = Path.Combine(carpeta, fileName);

                File.WriteAllBytes(path, bytes);

                writer.WriteLine($"{item.Id},{chartId},{item.Url},{fileName}");

                Console.WriteLine($"✔ Guardado: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ID {item.Id}: {ex.Message}");
            }
        }

        driver.Quit();
    }
}
