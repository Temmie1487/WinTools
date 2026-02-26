using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class ProcessView : Page
{
    private readonly DispatcherTimer _timer;
    private List<ProcessInfo> _allProcesses = new();

    public ProcessView()
    {
        InitializeComponent();
        DataGridHelper.EnableSorting(ProcessGrid);
        LoadProcesses();
        
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += (s, e) => LoadProcesses();
        _timer.Start();
    }

    private void LoadProcesses()
    {
        try
        {
            var searchText = SearchBox?.Text?.ToLower() ?? "";
            var processes = Process.GetProcesses()
                .Where(p => string.IsNullOrEmpty(searchText) || p.ProcessName.ToLower().Contains(searchText))
                .Select(p => new ProcessInfo
                {
                    ProcessId = p.Id,
                    ProcessName = p.ProcessName,
                    MemoryMB = p.WorkingSet64 / (1024 * 1024),
                    Status = p.Responding ? "运行中" : "未响应",
                    FilePath = GetProcessPath(p)
                })
                .OrderByDescending(p => p.MemoryMB)
                .ToList();

            _allProcesses = processes;
            ProcessGrid.ItemsSource = processes;
        }
        catch { }
    }

    private string GetProcessPath(Process p)
    {
        try { return p.MainModule?.FileName ?? ""; }
        catch { return "拒绝访问"; }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadProcesses();
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadProcesses();
    }

    private void KillBtn_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessGrid.SelectedItem is ProcessInfo pi)
        {
            var result = MessageBoxWindow.Show(
                $"确定要结束进程 \"{pi.ProcessName}\" (PID: {pi.ProcessId}) 吗？\n\n注意：结束某些系统进程可能会导致系统不稳定或崩溃。",
                "确认结束进程",
                CustomMessageBoxType.Warning,
                false);
            
            if (result == CustomMessageBoxResult.Yes)
            {
                try
                {
                    var process = Process.GetProcessById(pi.ProcessId);
                    process.Kill();
                    LoadProcesses();
                    MessageBoxWindow.Show("进程已结束", "成功", CustomMessageBoxType.Information);
                }
                catch
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "taskkill",
                            Arguments = $"/PID {pi.ProcessId} /F",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            Verb = "runas"
                        };
                        var p = Process.Start(psi);
                        p?.WaitForExit();
                        if (p?.ExitCode == 0)
                        {
                            LoadProcesses();
                            MessageBoxWindow.Show("进程已结束（已使用管理员权限）", "成功", CustomMessageBoxType.Information);
                        }
                        else
                        {
                            MessageBoxWindow.Show("无法结束进程，可能需要管理员权限", "错误", CustomMessageBoxType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxWindow.Show($"无法结束进程: {ex.Message}", "错误", CustomMessageBoxType.Error);
                    }
                }
            }
        }
    }

    private void OpenLocationBtn_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessGrid.SelectedItem is ProcessInfo pi)
        {
            if (!string.IsNullOrEmpty(pi.FilePath) && pi.FilePath != "拒绝访问")
            {
                try
                {
                    var directory = System.IO.Path.GetDirectoryName(pi.FilePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Process.Start("explorer.exe", $"/select,\"{pi.FilePath}\"");
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法打开文件位置: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
            else
            {
                MessageBoxWindow.Show("无法访问进程文件路径", "提示", CustomMessageBoxType.Warning);
            }
        }
    }
}

public class ProcessInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = "";
    public long MemoryMB { get; set; }
    public string Status { get; set; } = "";
    public double CpuPercent { get; set; }
    public string FilePath { get; set; } = "";
}
