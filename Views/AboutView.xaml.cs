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
        MessageBoxWindow.Show("当前已是最新版本 (1.0.0)", "检查更新", CustomMessageBoxType.Information);
    }
}
