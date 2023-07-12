using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Runtime;
using System.Xml.Linq;

namespace AdCreativeCase;

public class Program
{
    public static bool isDelete = false;
    public static int DownloadedImages = 0;
    private static Settings? _settings;



    static void Main(string[] args)
    {
        string settingPath = Path.Combine(Directory.GetCurrentDirectory(), "Input.json");
        ReadSettings(settingPath);
        string path = Path.Combine(Directory.GetCurrentDirectory(), _settings.SavePath);
        Uri uri = new Uri("https://picsum.photos/200/300");

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            isDelete = true;
        };

        Parallel.For(0, _settings.Count, new ParallelOptions { MaxDegreeOfParallelism = _settings.Parallelism },
          async (i, state) =>
          {
              if (isDelete)
                  state.Stop();

              WebClient wc = new();
              wc.DownloadFileCompleted += DownloadDataCompletedCallback;
              if (!Directory.Exists(path))
                  Directory.CreateDirectory(path);
              wc.DownloadFileTaskAsync(uri, Path.Combine(path, i.ToString() + ".png")).Wait();
          });

        if (isDelete)
        {
            CleanUpDownloadedImages();
        }
    }
    private static void DownloadDataCompletedCallback(object? sender, AsyncCompletedEventArgs e)
    {
        Interlocked.Increment(ref DownloadedImages);
        Console.Write($"Downloading {DownloadedImages} images of {_settings.Count} ({_settings.Parallelism} parallel downloads at most)");
        Console.SetCursorPosition(0, 0);
    }

    static void ReadSettings(string inputFile)
    {
        var inputJson = File.ReadAllText(inputFile);
        _settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(inputJson);
    }


    private static void CleanUpDownloadedImages()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), _settings.SavePath);

        foreach (var imageName in Directory.GetFiles(path))
        {
            try
            {
                if (File.Exists(imageName))
                {
                    File.Delete(imageName);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

public class Settings
{
    public int Count { get; set; }
    public int Parallelism { get; set; }
    public string SavePath { get; set; } = "outputs";
}