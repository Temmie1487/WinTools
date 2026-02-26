using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace WinTools.Views;

public partial class DeviceView : Page
{
    private List<DeviceInfo> _allDevices = new();

    public DeviceView()
    {
        InitializeComponent();
        LoadDevices();
    }

    private void LoadDevices()
    {
        try
        {
            var devices = new List<DeviceInfo>();
            
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Present = TRUE");
                foreach (ManagementObject device in searcher.Get())
                {
                    var name = device["Name"]?.ToString() ?? "";
                    var pnpClass = device["PNPClass"]?.ToString() ?? "";
                    
                    if (!string.IsNullOrEmpty(name))
                    {
                        devices.Add(new DeviceInfo
                        {
                            Name = name,
                            DeviceType = GetDeviceType(pnpClass, name),
                            Status = device["Status"]?.ToString() ?? "正常",
                            Manufacturer = device["Manufacturer"]?.ToString() ?? "",
                            DriverVersion = device["DriverVersion"]?.ToString() ?? "",
                            DriverDate = ""
                        });
                    }
                }
                searcher.Dispose();
            }
            catch { }
            
            if (devices.Count == 0)
            {
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                    foreach (ManagementObject cs in searcher.Get())
                    {
                        devices.Add(new DeviceInfo
                        {
                            Name = cs["Name"]?.ToString() ?? "",
                            DeviceType = "计算机",
                            Status = "正常",
                            Manufacturer = cs["SystemType"]?.ToString() ?? "",
                            DriverVersion = "",
                            DriverDate = ""
                        });
                    }
                    searcher.Dispose();
                }
                catch { }
                
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                    foreach (ManagementObject cpu in searcher.Get())
                    {
                        devices.Add(new DeviceInfo
                        {
                            Name = cpu["Name"]?.ToString() ?? "",
                            DeviceType = "处理器",
                            Status = "正常",
                            Manufacturer = cpu["Manufacturer"]?.ToString() ?? "",
                            DriverVersion = "",
                            DriverDate = ""
                        });
                    }
                    searcher.Dispose();
                }
                catch { }
                
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                    foreach (ManagementObject mem in searcher.Get())
                    {
                        var capacity = Convert.ToInt64(mem["Capacity"]) / (1024 * 1024 * 1024);
                        devices.Add(new DeviceInfo
                        {
                            Name = $"{capacity} GB",
                            DeviceType = "内存",
                            Status = "正常",
                            Manufacturer = mem["Manufacturer"]?.ToString() ?? "",
                            DriverVersion = "",
                            DriverDate = ""
                        });
                    }
                    searcher.Dispose();
                }
                catch { }
                
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        var size = Convert.ToInt64(disk["Size"]) / (1024 * 1024 * 1024);
                        devices.Add(new DeviceInfo
                        {
                            Name = disk["Model"]?.ToString() ?? "",
                            DeviceType = "磁盘",
                            Status = disk["Status"]?.ToString() ?? "正常",
                            Manufacturer = disk["Manufacturer"]?.ToString() ?? "",
                            DriverVersion = disk["DriverVersion"]?.ToString() ?? "",
                            DriverDate = $"{size} GB"
                        });
                    }
                    searcher.Dispose();
                }
                catch { }
                
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                    foreach (ManagementObject video in searcher.Get())
                    {
                        devices.Add(new DeviceInfo
                        {
                            Name = video["Name"]?.ToString() ?? "",
                            DeviceType = "显示",
                            Status = "正常",
                            Manufacturer = video["AdapterRAM"]?.ToString() ?? "",
                            DriverVersion = video["DriverVersion"]?.ToString() ?? "",
                            DriverDate = video["DriverDate"]?.ToString() ?? ""
                        });
                    }
                    searcher.Dispose();
                }
                catch { }
                
                try
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = TRUE");
                    foreach (ManagementObject net in searcher.Get())
                    {
                        devices.Add(new DeviceInfo
                        {
                            Name = net["Name"]?.ToString() ?? "",
                            DeviceType = "网络",
                            Status = net["NetConnectionStatus"]?.ToString() == "2" ? "已连接" : "未连接",
                            Manufacturer = net["Manufacturer"]?.ToString() ?? "",
                            DriverVersion = net["DriverVersion"]?.ToString() ?? "",
                            DriverDate = ""
                        });
                    }
                    searcher.Dispose();
                }
                catch { }
            }
            
            var searchText = SearchBox?.Text?.ToLower() ?? "";
            _allDevices = devices
                .Where(d => string.IsNullOrEmpty(searchText) || 
                           d.Name.ToLower().Contains(searchText) ||
                           d.DeviceType.ToLower().Contains(searchText))
                .OrderBy(d => d.DeviceType)
                .ThenBy(d => d.Name)
                .ToList();

            DeviceGrid.ItemsSource = _allDevices;
        }
        catch { }
    }

    private string GetDeviceType(string pnpClass, string name)
    {
        if (!string.IsNullOrEmpty(pnpClass))
        {
            var p = pnpClass.ToLower();
            if (p.Contains("processor")) return "处理器";
            if (p.Contains("memory")) return "内存";
            if (p.Contains("disk")) return "磁盘";
            if (p.Contains("network")) return "网络";
            if (p.Contains("display") || p.Contains("video")) return "显示";
            if (p.Contains("audio") || p.Contains("sound")) return "音频";
            if (p.Contains("usb")) return "USB";
            if (p.Contains("keyboard") || p.Contains("mouse") || p.Contains("pointing")) return "输入设备";
            if (p.Contains("monitor")) return "显示器";
            if (p.Contains("system")) return "系统设备";
            if (p.Contains("human")) return "人机接口";
        }
        
        name = name.ToLower();
        if (name.Contains("cpu") || name.Contains("processor"))
            return "处理器";
        if (name.Contains("memory") || name.Contains("ram"))
            return "内存";
        if (name.Contains("disk") || name.Contains("hard drive") || name.Contains("ssd") || name.Contains("nvme"))
            return "磁盘";
        if (name.Contains("network") || name.Contains("ethernet") || name.Contains("wifi") || name.Contains("adapter") || name.Contains("lan"))
            return "网络";
        if (name.Contains("display") || name.Contains("video") || name.Contains("graphics") || name.Contains("gpu") || name.Contains("nvidia") || name.Contains("amd") || name.Contains("intel"))
            return "显示";
            if (name.Contains("audio") || name.Contains("sound") || name.Contains("speaker") || name.Contains("realtek"))
            return "音频";
        if (name.Contains("usb"))
            return "USB";
        if (name.Contains("keyboard") || name.Contains("mouse") || name.Contains("pointing"))
            return "输入设备";
        if (name.Contains("monitor"))
            return "显示器";
        return "其他设备";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadDevices();
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadDevices();
    }
}

public class DeviceInfo
{
    public string Name { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public string Status { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string DriverVersion { get; set; } = "";
    public string DriverDate { get; set; } = "";
}