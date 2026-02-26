using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace WinTools.Views;

public partial class PowerView : Page
{
    private DispatcherTimer? _timer;
    private bool _isInitializing = true;

    public PowerView()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            _isInitializing = false;
            LoadPowerInfo();
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += (sender, args) => LoadBatteryInfo();
            _timer.Start();
        };
    }

    private void LoadPowerInfo()
    {
        try
        {
            LoadPowerPlan();
            LoadBatteryInfo();
        }
        catch { }
    }

    private void LoadPowerPlan()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "/getactivescheme",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            _isInitializing = true;
            
            PowerPlanList.Items.Clear();
            PowerPlanList.Items.Add(new ListBoxItem { Content = "平衡 (Balanced)", Tag = "381b4222-f694-41f0-9685-ff5bb260df2e" });
            PowerPlanList.Items.Add(new ListBoxItem { Content = "高性能 (High Performance)", Tag = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" });
            PowerPlanList.Items.Add(new ListBoxItem { Content = "节能 (Power Saver)", Tag = "a1841308-3541-4fab-bc81-f71556f20b4a" });
            
            int selectedIndex = -1;
            if (output.Contains("381b4222-f694-41f0-9685-ff5bb260df2e") || output.Contains("平衡"))
            {
                selectedIndex = 0;
                CurrentPlanName.Text = "当前使用: 平衡 (Balanced)";
            }
            else if (output.Contains("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c") || output.Contains("高性能"))
            {
                selectedIndex = 1;
                CurrentPlanName.Text = "当前使用: 高性能 (High Performance)";
            }
            else if (output.Contains("a1841308-3541-4fab-bc81-f71556f20b4a") || output.Contains("节能"))
            {
                selectedIndex = 2;
                CurrentPlanName.Text = "当前使用: 节能 (Power Saver)";
            }
            else
            {
                var nameStart = output.IndexOf("(");
                var nameEnd = output.IndexOf(")");
                if (nameStart > 0 && nameEnd > nameStart)
                {
                    var customName = output.Substring(nameStart + 1, nameEnd - nameStart - 1);
                    CurrentPlanName.Text = "当前使用: " + customName;
                    
                    PowerPlanList.Items.Add(new ListBoxItem { Content = "当前: " + customName, Tag = "custom" });
                    selectedIndex = PowerPlanList.Items.Count - 1;
                }
                else
                {
                    CurrentPlanName.Text = "当前使用: 自定义电源计划";
                }
            }
            
            if (selectedIndex >= 0)
            {
                PowerPlanList.SelectedIndex = selectedIndex;
                PowerPlanButtonText.Text = (PowerPlanList.SelectedItem as ListBoxItem)?.Content?.ToString() ?? "选择电源计划";
            }
            _isInitializing = false;
        }
        catch { _isInitializing = false; }
    }

    private void LoadBatteryInfo()
    {
        try
        {
            var batteryStatus = System.Windows.Forms.SystemInformation.PowerStatus;
            
            if (batteryStatus.BatteryLifePercent >= 0 && batteryStatus.BatteryLifePercent <= 100)
            {
                BatteryPercent.Text = batteryStatus.BatteryLifePercent + "%";
                BatteryBar.Value = batteryStatus.BatteryLifePercent;
                
                if (batteryStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
                {
                    ChargingStatus.Text = "已接通电源";
                    BatteryStatus.Text = batteryStatus.BatteryLifePercent == 100 ? "已充满" : "充电中";
                }
                else
                {
                    ChargingStatus.Text = "使用电池";
                    BatteryStatus.Text = "电池供电";
                }
                
                if (batteryStatus.BatteryLifeRemaining > 0)
                {
                    var minutes = batteryStatus.BatteryLifeRemaining / 60;
                    if (minutes >= 60)
                    {
                        var hours = minutes / 60;
                        var mins = minutes % 60;
                        BatteryTime.Text = $"{hours}小时{mins}分钟";
                    }
                    else
                    {
                        BatteryTime.Text = $"{minutes} 分钟";
                    }
                }
                else
                {
                    BatteryTime.Text = "N/A";
                }
                
                try
                {
                    var voltage = batteryStatus.BatteryFullLifetime / 1000;
                    BatteryVoltage.Text = voltage > 0 ? $"{voltage} mV" : "N/A";
                }
                catch
                {
                    BatteryVoltage.Text = "N/A";
                }
                
                PowerUsage.Text = "N/A";
            }
            else
            {
                BatteryStatus.Text = "未检测到电池";
                BatteryPercent.Text = "N/A";
                BatteryBar.Value = 0;
                ChargingStatus.Text = "N/A";
                BatteryTime.Text = "N/A";
                BatteryVoltage.Text = "N/A";
                PowerUsage.Text = "N/A";
            }
        }
        catch
        {
            BatteryStatus.Text = "未知";
            BatteryPercent.Text = "N/A";
            BatteryBar.Value = 0;
            ChargingStatus.Text = "N/A";
            BatteryTime.Text = "N/A";
            BatteryVoltage.Text = "N/A";
            PowerUsage.Text = "N/A";
        }
    }

    private void PowerPlanButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        PowerPlanPopup.IsOpen = !PowerPlanPopup.IsOpen;
    }

    private void PowerPlanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        
        if (PowerPlanList.SelectedItem is ListBoxItem item && item.Tag is string guid && guid != "custom")
        {
            PowerPlanPopup.IsOpen = false;
            
            var result = System.Windows.MessageBox.Show($"确定要切换到 \"{item.Content}\" 电源计划吗？", 
                "切换电源计划", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powercfg",
                            Arguments = $"/setactive {guid}",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    
                    LoadPowerPlan();
                    System.Windows.MessageBox.Show("电源计划已切换", "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"无法切换电源计划: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    LoadPowerPlan();
                }
            }
            else
            {
                LoadPowerPlan();
            }
        }
        else if (PowerPlanList.SelectedItem is ListBoxItem selectedItem)
        {
            PowerPlanButtonText.Text = selectedItem.Content?.ToString() ?? "选择电源计划";
            PowerPlanPopup.IsOpen = false;
        }
    }

    private void RefreshPowerPlan_Click(object sender, RoutedEventArgs e)
    {
        LoadPowerPlan();
    }

    private void Balanced_Click(object sender, RoutedEventArgs e)
    {
        SetPowerPlan("381b4222-f694-41f0-9685-ff5bb260df2e", "平衡");
    }

    private void PowerSaver_Click(object sender, RoutedEventArgs e)
    {
        SetPowerPlan("a1841308-3541-4fab-bc81-f71556f20b4a", "节能");
    }

    private void HighPerf_Click(object sender, RoutedEventArgs e)
    {
        SetPowerPlan("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", "高性能");
    }

    private void SetPowerPlan(string guid, string name)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/setactive {guid}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            LoadPowerPlan();
            System.Windows.MessageBox.Show($"已切换到 {name} 电源计划", "成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"无法切换电源计划: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            LoadPowerPlan();
        }
    }
}
