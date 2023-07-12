using System.ComponentModel;
using System.Net;

namespace AdCreativeCase;

public class Program
{
    public static bool _isCancel = false;
    public static int _downloadedImages = 0;
    private static Settings? _settings;



    static void Main(string[] args)
    {
        ReadSettings();

        string path = Path.Combine(Directory.GetCurrentDirectory(), _settings.SavePath);
        Uri uri = new Uri("https://picsum.photos/200/300");

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _isCancel = true;
        };
        Console.Write($"Downloading {_settings.Count} images  ({_settings.Parallelism} parallel downloads at most)");

        Parallel.For(0, _settings.Count, new ParallelOptions { MaxDegreeOfParallelism = _settings.Parallelism },
          async (i, state) =>
          {
              if (_isCancel)
                  state.Stop();

              WebClient wc = new();
              wc.DownloadFileCompleted += DownloadDataCompletedCallback;

              if (!Directory.Exists(path))
                  Directory.CreateDirectory(path);

              wc.DownloadFileTaskAsync(uri, Path.Combine(path, i.ToString() + ".png")).Wait();
          });

        if (_isCancel)
        {
            CleanUpDownloadedImages();
        }
    }

    private static void DownloadDataCompletedCallback(object? sender, AsyncCompletedEventArgs e)
    {
        Interlocked.Increment(ref _downloadedImages);
        Console.SetCursorPosition(0, 3);
        Console.Write($"{_downloadedImages}/{_settings.Count}");
    }

    private static void ReadSettings()
    {
        string settingPath = Path.Combine(Directory.GetCurrentDirectory(), "Input.json");
        var inputJson = File.ReadAllText(settingPath);
        _settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(inputJson);
    }

    private static void CleanUpDownloadedImages()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), _settings.SavePath);
        foreach (var imageName in Directory.GetFiles(path))
        {
            if (File.Exists(imageName))
                File.Delete(imageName);
        }
    }
}
