using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace WinTools;

public partial class App : System.Windows.Application
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinTools", "Logs", "app.log");

    public static string CurrentVersion => "v1.0.0";
    public static string? LatestVersion { get; private set; }
    public static string? UpdateUrl { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var logDir = Path.GetDirectoryName(LogPath);
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir!);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(LogPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("WinTools 应用程序启动");

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        CheckForUpdateAsync();

        var window = new MainWindow();
        window.Show();
    }

    private async void CheckForUpdateAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WinTools");
            var response = await client.GetStringAsync("https://api.github.com/repos/Temmie1487/WinTools/releases/latest");
            var json = JsonDocument.Parse(response);
            
            var tagName = json.RootElement.GetProperty("tag_name").GetString();
            LatestVersion = tagName?.Replace("v", "").Replace("V", "").Trim();
            UpdateUrl = json.RootElement.GetProperty("html_url").GetString();

            Log.Information("检查更新: 当前版本 {Current}, 最新版本 {Latest}", CurrentVersion, LatestVersion);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "检查更新失败");
        }
    }

    public static bool HasUpdate()
    {
        if (string.IsNullOrEmpty(LatestVersion)) return false;
        return LatestVersion != CurrentVersion;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "UI线程未处理异常: {Message}", e.Exception.Message);
        System.Windows.MessageBox.Show($"发生错误: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "应用程序域未处理异常");
        }
    }
}
