using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class AboutView : Page
{
    public AboutView()
    {
        InitializeComponent();
        VersionText.Text = "版本 " + App.CurrentVersion;
    }

    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/Temmie1487") { UseShellExecute = true });
    }

    private void Bilibili_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://space.bilibili.com/1687580392") { UseShellExecute = true });
    }

    private void ProjectPage_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/Temmie1487/WinTools") { UseShellExecute = true });
    }

    private void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (App.HasUpdate())
        {
            var result = MessageBoxWindow.Show(
                $"发现新版本: {App.LatestVersion}\n当前版本: {App.CurrentVersion}\n\n是否前往下载？",
                "发现新版本",
                CustomMessageBoxType.Question);

            if (result == CustomMessageBoxResult.OK && !string.IsNullOrEmpty(App.UpdateUrl))
            {
                Process.Start(new ProcessStartInfo(App.UpdateUrl) { UseShellExecute = true });
            }
        }
        else if (!string.IsNullOrEmpty(App.LatestVersion))
        {
            MessageBoxWindow.Show($"当前已是最新版本 (v{App.CurrentVersion})", "检查更新", CustomMessageBoxType.Information);
        }
        else
        {
            MessageBoxWindow.Show("检查更新失败，请稍后重试", "检查更新", CustomMessageBoxType.Warning);
        }
    }
}
