using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Wpf.Ui.Controls;

namespace WinTools;

public partial class MainWindow : FluentWindow
{
    private bool _isNavigating = false;

    public MainWindow()
    {
        InitializeComponent();
        NavList.SelectedIndex = 0;
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isNavigating) return;
        
        if (NavList.SelectedItem is ListBoxItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    public void NavigateTo(string tag)
    {
        if (_isNavigating) return;
        
        _isNavigating = true;
        
        Page? page = tag switch
        {
            "Dashboard" => new Views.DashboardView(),
            "Process" => new Views.ProcessView(),
            "Service" => new Views.ServiceView(),
            "Performance" => new Views.PerformanceView(),
            "Disk" => new Views.DiskView(),
            "Network" => new Views.NetworkView(),
            "App" => new Views.AppView(),
            "Device" => new Views.DeviceView(),
            "Startup" => new Views.StartupView(),
            "Environment" => new Views.EnvironmentView(),
            "User" => new Views.UserView(),
            "Power" => new Views.PowerView(),
            "About" => new Views.AboutView(),
            _ => null
        };

        if (page != null)
        {
            ContentFrame.Navigate(page);
        }
        
        _isNavigating = false;
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is Page page)
        {
            page.Opacity = 0;
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            page.BeginAnimation(OpacityProperty, animation);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            Maximize_Click(sender, e);
        }
        else
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeBtn.Content = "□";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeBtn.Content = "❐";
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}