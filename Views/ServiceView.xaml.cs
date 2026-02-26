using System.Diagnostics;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class ServiceView : Page
{
    private List<ServiceInfo> _allServices = new();

    public ServiceView()
    {
        InitializeComponent();
        LoadServices();
    }

    private void LoadServices()
    {
        try
        {
            var searchText = SearchBox?.Text?.ToLower() ?? "";
            var services = ServiceController.GetServices()
                .Where(s => string.IsNullOrEmpty(searchText) || 
                           s.ServiceName.ToLower().Contains(searchText) || 
                           s.DisplayName.ToLower().Contains(searchText))
                .Select(s => new ServiceInfo
                {
                    ServiceName = s.ServiceName,
                    DisplayName = s.DisplayName,
                    Status = s.Status.ToString(),
                    StartType = s.StartType.ToString(),
                    Description = ""
                })
                .OrderBy(s => s.DisplayName)
                .ToList();

            _allServices = services;
            ServiceGrid.ItemsSource = services;
        }
        catch { }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadServices();
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadServices();
    }

    private void StartBtn_Click(object sender, RoutedEventArgs e)
    {
        if (ServiceGrid.SelectedItem is ServiceInfo si)
        {
            try
            {
                var sc = new ServiceController(si.ServiceName);
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    LoadServices();
                    MessageBoxWindow.Show($"服务 \"{si.DisplayName}\" 已启动", "成功", CustomMessageBoxType.Information);
                }
            }
            catch
            {
                TryAdminStartService(si.ServiceName, si.DisplayName);
            }
        }
    }

    private void StopBtn_Click(object sender, RoutedEventArgs e)
    {
        if (ServiceGrid.SelectedItem is ServiceInfo si)
        {
            try
            {
                var sc = new ServiceController(si.ServiceName);
                if (sc.CanStop && sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    LoadServices();
                    MessageBoxWindow.Show($"服务 \"{si.DisplayName}\" 已停止", "成功", CustomMessageBoxType.Information);
                }
            }
            catch
            {
                TryAdminStopService(si.ServiceName, si.DisplayName);
            }
        }
    }

    private void TryAdminStartService(string serviceName, string displayName)
    {
        var result = MessageBoxWindow.Show($"需要管理员权限才能启动服务 \"{displayName}\"，是否继续？", 
            "需要管理员权限", CustomMessageBoxType.Question, false);
        
        if (result == CustomMessageBoxResult.Yes)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = $"start \"{serviceName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };
                var p = Process.Start(psi);
                p?.WaitForExit();
                LoadServices();
                if (p?.ExitCode == 0)
                {
                    MessageBoxWindow.Show($"服务 \"{displayName}\" 已启动", "成功", CustomMessageBoxType.Information);
                }
                else
                {
                    MessageBoxWindow.Show("无法启动服务", "错误", CustomMessageBoxType.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBoxWindow.Show($"无法启动服务: {ex.Message}", "错误", CustomMessageBoxType.Error);
            }
        }
    }

    private void TryAdminStopService(string serviceName, string displayName)
    {
        var result = MessageBoxWindow.Show($"需要管理员权限才能停止服务 \"{displayName}\"，是否继续？", 
            "需要管理员权限", CustomMessageBoxType.Question, false);
        
        if (result == CustomMessageBoxResult.Yes)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = $"stop \"{serviceName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };
                var p = Process.Start(psi);
                p?.WaitForExit();
                LoadServices();
                if (p?.ExitCode == 0)
                {
                    MessageBoxWindow.Show($"服务 \"{displayName}\" 已停止", "成功", CustomMessageBoxType.Information);
                }
                else
                {
                    MessageBoxWindow.Show("无法停止服务", "错误", CustomMessageBoxType.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBoxWindow.Show($"无法停止服务: {ex.Message}", "错误", CustomMessageBoxType.Error);
            }
        }
    }
}

public class ServiceInfo
{
    public string ServiceName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "";
    public string StartType { get; set; } = "";
    public string Description { get; set; } = "";
}
