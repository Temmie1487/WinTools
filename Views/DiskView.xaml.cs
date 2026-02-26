using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class DiskView : Page
{
    public DiskView()
    {
        InitializeComponent();
        DataGridHelper.EnableSorting(DiskGrid);
        LoadDisks();
    }

    private void LoadDisks()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => new DiskInfo
                {
                    DriveLetter = d.Name,
                    VolumeLabel = d.VolumeLabel,
                    FileSystem = d.DriveFormat,
                    TotalGB = Math.Round(d.TotalSize / (1024.0 * 1024 * 1024), 1),
                    FreeGB = Math.Round(d.AvailableFreeSpace / (1024.0 * 1024 * 1024), 1),
                    UsedGB = Math.Round((d.TotalSize - d.AvailableFreeSpace) / (1024.0 * 1024 * 1024), 1),
                    UsagePercent = Math.Round((d.TotalSize - d.AvailableFreeSpace) * 100.0 / d.TotalSize, 0) + "%"
                })
                .ToList();

            DiskGrid.ItemsSource = drives;
        }
        catch { }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadDisks();
    }

    private void OpenDefragBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("dfrgui.exe") { UseShellExecute = true });
        }
        catch
        {
            MessageBoxWindow.Show("无法打开磁盘碎片整理工具", "错误", CustomMessageBoxType.Error);
        }
    }

    private void OpenDiskMgrBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("diskmgmt.msc") { UseShellExecute = true });
        }
        catch
        {
            MessageBoxWindow.Show("无法打开磁盘管理", "错误", CustomMessageBoxType.Error);
        }
    }

    private void OpenCleanupBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("cleanmgr.exe") { UseShellExecute = true });
        }
        catch
        {
            MessageBoxWindow.Show("无法打开磁盘清理工具", "错误", CustomMessageBoxType.Error);
        }
    }

    private void OpenPropBtn_Click(object sender, RoutedEventArgs e)
    {
        if (DiskGrid.SelectedItem is DiskInfo disk)
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{disk.DriveLetter}") { UseShellExecute = true });
            }
            catch
            {
                MessageBoxWindow.Show("无法打开磁盘属性", "错误", CustomMessageBoxType.Error);
            }
        }
    }

    private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
    {
        if (DiskGrid.SelectedItem is DiskInfo disk)
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", disk.DriveLetter) { UseShellExecute = true });
            }
            catch
            {
                MessageBoxWindow.Show("无法打开文件夹", "错误", CustomMessageBoxType.Error);
            }
        }
    }

    private void DiskGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        OpenFolderBtn_Click(sender, e);
    }
}

public class DiskInfo
{
    public string DriveLetter { get; set; } = "";
    public string VolumeLabel { get; set; } = "";
    public string FileSystem { get; set; } = "";
    public double TotalGB { get; set; }
    public double FreeGB { get; set; }
    public double UsedGB { get; set; }
    public string UsagePercent { get; set; } = "";
}