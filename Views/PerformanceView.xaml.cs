using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WinTools.Views;

public partial class PerformanceView : Page
{
    private readonly DispatcherTimer _timer;
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _diskReadCounter;
    private PerformanceCounter? _diskWriteCounter;
    private PerformanceCounter? _netSentCounter;
    private PerformanceCounter? _netReceivedCounter;
    private string _gpuName = "";

    public PerformanceView()
    {
        InitializeComponent();
        InitCounters();
        StartMonitoring();
        
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => UpdatePerformance();
        _timer.Start();
    }

    private void InitCounters()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            _diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
            
            var category = new PerformanceCounterCategory("Network Interface");
            var instanceNames = category.GetInstanceNames();
            if (instanceNames.Length > 0)
            {
                var instance = instanceNames[0];
                _netSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
                _netReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
            }
            
            _cpuCounter.NextValue();
            _diskReadCounter.NextValue();
            _diskWriteCounter.NextValue();
            _netSentCounter?.NextValue();
            _netReceivedCounter?.NextValue();
        }
        catch { }
        
        LoadGpuInfo();
    }

    private void LoadGpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject gpu in searcher.Get())
            {
                _gpuName = gpu["Name"]?.ToString() ?? "未知显卡";
                break;
            }
        }
        catch
        {
            _gpuName = "未知显卡";
        }
    }

    private double GetGpuUsage()
    {
        try
        {
            var category = new PerformanceCounterCategory("GPU Engine");
            var instances = category.GetInstanceNames();
            
            if (instances.Length > 0)
            {
                double maxUsage = 0;
                foreach (var instance in instances)
                {
                    if (instance.Contains("engtype_3D"))
                    {
                        using var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance);
                        var usage = counter.NextValue();
                        if (usage > maxUsage) maxUsage = usage;
                    }
                }
                return maxUsage;
            }
        }
        catch { }
        
        try
        {
            var category = new PerformanceCounterCategory("GPU Engine");
            var instances = category.GetInstanceNames();
            if (instances.Length > 0)
            {
                using var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instances[0]);
                return counter.NextValue();
            }
        }
        catch { }
        
        return 0;
    }

    private void StartMonitoring()
    {
        UpdatePerformance();
    }

    private void UpdatePerformance()
    {
        try
        {
            if (_cpuCounter != null)
            {
                var cpu = _cpuCounter.NextValue();
                CpuPercent.Text = $"{cpu:F1}%";
                CpuBar.Value = cpu;
                CpuInfo.Text = $"CPU 核心数: {Environment.ProcessorCount}";
            }

            var gpuUsage = GetGpuUsage();
            GpuPercent.Text = $"{gpuUsage:F1}%";
            GpuBar.Value = gpuUsage;
            GpuInfo.Text = _gpuName;

            var memInfo = GetMemoryInfo();
            MemPercent.Text = memInfo.AvailablePercent;
            MemBar.Value = double.Parse(memInfo.AvailablePercent.TrimEnd('%'));
            MemInfo.Text = $"{memInfo.AvailableGB} GB 可用 / {memInfo.TotalGB} GB 总计";

            if (_diskReadCounter != null)
            {
                var read = _diskReadCounter.NextValue() / (1024 * 1024);
                var write = _diskWriteCounter?.NextValue() / (1024 * 1024) ?? 0;
                DiskRead.Text = $"{read:F2} MB/s";
                DiskWrite.Text = $"{write:F2} MB/s";
            }

            if (_netSentCounter != null && _netReceivedCounter != null)
            {
                var sent = _netSentCounter.NextValue() / 1024;
                var receive = _netReceivedCounter.NextValue() / 1024;
                NetSend.Text = $"{sent:F1} KB/s";
                NetReceive.Text = $"{receive:F1} KB/s";
            }
        }
        catch { }
    }

    private (string TotalGB, string UsedGB, string AvailableGB, string AvailablePercent) GetMemoryInfo()
    {
        try
        {
            var gcMemInfo = GC.GetGCMemoryInfo();
            long totalMemory = gcMemInfo.TotalAvailableMemoryBytes;
            long availableMemory = gcMemInfo.HighMemoryLoadThresholdBytes;
            long usedMemory = totalMemory - availableMemory;
            if (usedMemory < 0) usedMemory = 0;

            double totalGB = totalMemory / (1024.0 * 1024 * 1024);
            double usedGB = usedMemory / (1024.0 * 1024 * 1024);
            double availableGB = availableMemory / (1024.0 * 1024 * 1024);
            double availablePercent = (availableGB / totalGB) * 100;

            return ($"{totalGB:F1}", $"{usedGB:F1}", $"{availableGB:F1}", $"{availablePercent:F0}%");
        }
        catch
        {
            return ("0", "0", "0", "0%");
        }
    }
}