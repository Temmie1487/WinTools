using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class AppView : Page
{
    private List<AppInfo> _allApps = new();

    public AppView()
    {
        InitializeComponent();
        LoadApps();
    }

    private void LoadApps()
    {
        try
        {
            var apps = new List<AppInfo>();
            
            var keys = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var keyPath in keys)
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        if (subKey != null)
                        {
                            var name = subKey.GetValue("DisplayName") as string;
                            if (!string.IsNullOrEmpty(name))
                            {
                                var installLocation = subKey.GetValue("InstallLocation") as string ?? "";
                                var (displaySize, bytesSize) = CalculateFolderSize(installLocation);
                                
                                apps.Add(new AppInfo
                                {
                                    Name = name,
                                    Version = subKey.GetValue("DisplayVersion") as string ?? "",
                                    Publisher = subKey.GetValue("Publisher") as string ?? "",
                                    InstallLocation = installLocation,
                                    Size = displaySize,
                                    SizeBytes = bytesSize
                                });
                            }
                        }
                    }
                }
            }

            using var userKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (userKey != null)
            {
                foreach (var subKeyName in userKey.GetSubKeyNames())
                {
                    using var subKey = userKey.OpenSubKey(subKeyName);
                    if (subKey != null)
                    {
                        var name = subKey.GetValue("DisplayName") as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            var installLocation = subKey.GetValue("InstallLocation") as string ?? "";
                            var (displaySize, bytesSize) = CalculateFolderSize(installLocation);
                            
                            apps.Add(new AppInfo
                            {
                                Name = name,
                                Version = subKey.GetValue("DisplayVersion") as string ?? "",
                                Publisher = subKey.GetValue("Publisher") as string ?? "",
                                InstallLocation = installLocation,
                                Size = displaySize,
                                SizeBytes = bytesSize
                            });
                        }
                    }
                }
            }

            var searchText = SearchBox?.Text?.ToLower() ?? "";
            _allApps = apps
                .Where(a => string.IsNullOrEmpty(searchText) || 
                           a.Name.ToLower().Contains(searchText) ||
                           a.Publisher.ToLower().Contains(searchText))
                .OrderBy(a => a.Name)
                .ToList();

            AppGrid.ItemsSource = _allApps;
        }
        catch { }
    }

    private (string DisplaySize, long Bytes) CalculateFolderSize(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return ("-", 0);
            
            var dirInfo = new DirectoryInfo(path);
            long size = 0;
            
            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                try { size += file.Length; }
                catch { }
            }
            
            if (size == 0) return ("-", 0);
            
            string display;
            if (size >= 1024 * 1024 * 1024)
                display = $"{size / (1024.0 * 1024 * 1024):F1} GB";
            else if (size >= 1024 * 1024)
                display = $"{size / (1024.0 * 1024):F1} MB";
            else if (size >= 1024)
                display = $"{size / 1024.0:F1} KB";
            else
                display = $"{size} B";
            
            return (display, size);
        }
        catch
        {
            return ("-", 0);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadApps();
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadApps();
    }

    private void UninstallBtn_Click(object sender, RoutedEventArgs e)
    {
        if (AppGrid.SelectedItem is AppInfo app)
        {
            var result = MessageBoxWindow.Show($"确定要卸载 \"{app.Name}\" 吗？\n\n注意：此操作仅打开系统卸载程序，不保证完全卸载。", 
                "卸载应用", CustomMessageBoxType.Warning, false);
            
            if (result == CustomMessageBoxResult.Yes)
            {
                try
                {
                    var keys = new[]
                    {
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
                    };
                    
                    string? uninstallString = null;
                    
                    foreach (var keyPath in keys)
                    {
                        using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                        if (key != null)
                        {
                            foreach (var subKeyName in key.GetSubKeyNames())
                            {
                                using var subKey = key.OpenSubKey(subKeyName);
                                if (subKey != null)
                                {
                                    var name = subKey.GetValue("DisplayName") as string;
                                    if (name == app.Name)
                                    {
                                        uninstallString = subKey.GetValue("UninstallString") as string;
                                        break;
                                    }
                                }
                            }
                        }
                        if (uninstallString != null) break;
                    }
                    
                    if (uninstallString != null)
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c {uninstallString}",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                    }
                    else
                    {
                        MessageBoxWindow.Show("无法找到卸载程序", "错误", CustomMessageBoxType.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法卸载: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
        }
    }

    private void OpenLocationBtn_Click(object sender, RoutedEventArgs e)
    {
        if (AppGrid.SelectedItem is AppInfo app)
        {
            if (!string.IsNullOrEmpty(app.InstallLocation) && Directory.Exists(app.InstallLocation))
            {
                Process.Start("explorer.exe", app.InstallLocation);
            }
            else
            {
                System.Windows.MessageBox.Show("安装路径不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void AppGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        OpenLocationBtn_Click(sender, e);
    }
}

public class AppInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string InstallLocation { get; set; } = "";
    public string Size { get; set; } = "-";
    public long SizeBytes { get; set; } = 0;
}