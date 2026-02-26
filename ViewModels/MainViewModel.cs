using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTools.Views;
using Serilog;

namespace WinTools.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private ObservableCollection<NavItem> _navigationItems = new();

    [ObservableProperty]
    private NavItem? _selectedNav;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentTime = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm");

    public MainViewModel()
    {
        InitializeNavigation();
        
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm");
        _timer.Start();

        if (NavigationItems.Count > 0)
        {
            SelectedNav = NavigationItems[0];
        }
    }

    private void InitializeNavigation()
    {
        NavigationItems = new ObservableCollection<NavItem>
        {
            new() { Title = "仪表盘", IconChar = "\uE80F", Description = "系统概览", View = new DashboardView() },
            new() { Title = "进程管理", IconChar = "\uE9D9", Description = "进程监控与管理", View = new ProcessView() },
            new() { Title = "服务管理", IconChar = "\uE996", Description = "Windows服务管理", View = new ServiceView() },
            new() { Title = "性能监视", IconChar = "\uE9D2", Description = "CPU/内存/磁盘监控", View = new PerformanceView() },
            new() { Title = "磁盘管理", IconChar = "\uEDA2", Description = "磁盘空间管理", View = new DiskView() },
            new() { Title = "网络管理", IconChar = "\uE968", Description = "网络连接状态", View = new NetworkView() },
            new() { Title = "应用管理", IconChar = "\uE8F1", Description = "已安装程序", View = new AppView() },
            new() { Title = "设备管理", IconChar = "\uE7F6", Description = "硬件设备列表", View = new DeviceView() },
            new() { Title = "启动项", IconChar = "\uE8A1", Description = "开机启动项", View = new StartupView() },
            new() { Title = "环境变量", IconChar = "\uE943", Description = "系统环境配置", View = new EnvironmentView() },
            new() { Title = "用户管理", IconChar = "\uE77B", Description = "用户账户管理", View = new UserView() },
            new() { Title = "电源管理", IconChar = "\uE945", Description = "电源计划设置", View = new PowerView() },
            new() { Title = "关于", IconChar = "\uE946", Description = "关于本软件", View = new AboutView() },
        };
    }

    partial void OnSelectedNavChanged(NavItem? value)
    {
        if (value != null)
        {
            CurrentView = value.View;
            Log.Debug("导航到: {Title}", value.Title);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        Log.Information("刷新数据");
    }
}

public partial class NavItem : ObservableObject
{
    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _iconChar = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private object? _view;
}
