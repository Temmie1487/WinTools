using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class StartupView : Page
{
    private List<StartupInfo> _allStartupItems = new();

    public StartupView()
    {
        InitializeComponent();
        LoadStartupItems();
    }

    private void LoadStartupItems()
    {
        try
        {
            var items = new List<StartupInfo>();

            var runKeys = new[]
            {
                (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "用户启动项"),
                (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "系统启动项"),
                (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "系统启动项(32位)")
            };

            foreach (var (rootKey, keyPath, location) in runKeys)
            {
                using var key = rootKey.OpenSubKey(keyPath);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var command = key.GetValue(valueName)?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(command))
                        {
                            items.Add(new StartupInfo
                            {
                                Name = valueName,
                                Publisher = "",
                                Status = "已启用",
                                Location = location,
                                Command = command
                            });
                        }
                    }
                }
            }

            var searchText = SearchBox?.Text?.ToLower() ?? "";
            _allStartupItems = items
                .Where(s => string.IsNullOrEmpty(searchText) || 
                           s.Name.ToLower().Contains(searchText))
                .OrderBy(s => s.Location)
                .ThenBy(s => s.Name)
                .ToList();

            StartupGrid.ItemsSource = _allStartupItems;
        }
        catch { }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadStartupItems();
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadStartupItems();
    }

    private void DisableBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StartupGrid.SelectedItem is StartupInfo si)
        {
            try
            {
                var keyPath = si.Location.Contains("用户") 
                    ? @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
                    : @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    
                var rootKey = si.Location.Contains("用户") ? Registry.CurrentUser : Registry.LocalMachine;
                
                using var key = rootKey.OpenSubKey(keyPath, true);
                key?.DeleteValue(si.Name, false);
                LoadStartupItems();
                MessageBoxWindow.Show($"已禁用启动项: {si.Name}", "成功", CustomMessageBoxType.Information);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.Show($"无法禁用启动项: {ex.Message}", "错误", CustomMessageBoxType.Error);
            }
        }
    }

    private void EnableBtn_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxWindow.Show("请手动在注册表中添加启动项", "提示", CustomMessageBoxType.Warning);
    }

    private void OpenLocationBtn_Click(object sender, RoutedEventArgs e)
    {
        if (StartupGrid.SelectedItem is StartupInfo si)
        {
            try
            {
                var command = si.Command.Trim('"');
                var directory = Path.GetDirectoryName(command);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    Process.Start("explorer.exe", $"/select,\"{command}\"");
                }
            }
            catch { }
        }
    }
}

public class StartupInfo
{
    public string Name { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string Status { get; set; } = "";
    public string Location { get; set; } = "";
    public string Command { get; set; } = "";
}