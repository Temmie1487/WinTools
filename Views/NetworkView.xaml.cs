using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class NetworkView : Page
{
    public NetworkView()
    {
        InitializeComponent();
        LoadNetworkInfo();
    }

    private void LoadNetworkInfo()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(n =>
                {
                    var ipProps = n.GetIPProperties();
                    var ipv4Address = ipProps.UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    
                    return new NetworkAdapterInfo
                    {
                        Name = n.Name,
                        Description = n.Description,
                        Status = n.OperationalStatus.ToString(),
                        MacAddress = n.GetPhysicalAddress().ToString(),
                        IpAddress = ipv4Address?.Address.ToString() ?? "N/A",
                        Subnet = ipv4Address?.IPv4Mask?.ToString() ?? "N/A",
                        Gateway = ipProps.GatewayAddresses.FirstOrDefault()?.Address.ToString() ?? "N/A",
                        Dns = string.Join(", ", ipProps.DnsAddresses.Where(d => d.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(d => d.ToString())),
                        InterfaceGuid = n.Id
                    };
                })
                .ToList();

            NetworkGrid.ItemsSource = interfaces;
        }
        catch { }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadNetworkInfo();
    }

    private void EnableBtn_Click(object sender, RoutedEventArgs e)
    {
        if (NetworkGrid.SelectedItem is NetworkAdapterInfo adapter)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface set interface \"{adapter.Name}\" enable",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                process.WaitForExit();
                MessageBoxWindow.Show($"网络适配器 \"{adapter.Name}\" 已启用", "成功", CustomMessageBoxType.Information);
                LoadNetworkInfo();
            }
            catch (Exception ex)
            {
                MessageBoxWindow.Show($"无法启用网络适配器: {ex.Message}", "错误", CustomMessageBoxType.Error);
            }
        }
        else
        {
            MessageBoxWindow.Show("请先选择一个网络适配器", "提示", CustomMessageBoxType.Warning);
        }
    }

    private void DisableBtn_Click(object sender, RoutedEventArgs e)
    {
        if (NetworkGrid.SelectedItem is NetworkAdapterInfo adapter)
        {
            var result = MessageBoxWindow.Show($"确定要禁用网络适配器 \"{adapter.Name}\" 吗？", 
                "确认禁用", CustomMessageBoxType.Question, false);
            
            if (result == CustomMessageBoxResult.Yes)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $"interface set interface \"{adapter.Name}\" disable",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            Verb = "runas"
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    MessageBoxWindow.Show($"网络适配器 \"{adapter.Name}\" 已禁用", "成功", CustomMessageBoxType.Information);
                    LoadNetworkInfo();
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法禁用网络适配器: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
        }
        else
        {
            MessageBoxWindow.Show("请先选择一个网络适配器", "提示", CustomMessageBoxType.Warning);
        }
    }

    private void IpConfigBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/release && ipconfig /renew",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            MessageBoxWindow.Show("IP 地址已释放并更新", "成功", CustomMessageBoxType.Information);
            LoadNetworkInfo();
        }
        catch (Exception ex)
        {
            MessageBoxWindow.Show($"无法释放/更新IP: {ex.Message}", "错误", CustomMessageBoxType.Error);
        }
    }

    private void FlushDnsBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            MessageBoxWindow.Show("DNS 缓存已刷新", "成功", CustomMessageBoxType.Information);
        }
        catch (Exception ex)
        {
            MessageBoxWindow.Show($"无法刷新DNS: {ex.Message}", "错误", CustomMessageBoxType.Error);
        }
    }
}

public class NetworkAdapterInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public string MacAddress { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Subnet { get; set; } = "";
    public string Gateway { get; set; } = "";
    public string Dns { get; set; } = "";
    public string InterfaceGuid { get; set; } = "";
}
