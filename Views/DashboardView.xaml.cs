using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using WinTools.Models;

namespace WinTools.Views;

public partial class DashboardView : Page
{
    private readonly DispatcherTimer _timer;
    private PerformanceCounter? _cpuCounter;
    private ObservableCollection<TileItem> _tiles = new();
    private bool _isEditMode = false;
    private bool _isFirstLoad = true;
    private bool _isDragging = false;
    private System.Windows.Point _dragStartPoint;
    private TileItem? _draggedTile;
    private Border? _draggedElement;
    private int _dragOverIndex = -1;
    private readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinTools", "tiles.json");

    public DashboardView()
    {
        InitializeComponent();
        TilesControl.ItemsSource = _tiles;
        
        Loaded += (s, e) =>
        {
            LoadSystemInfo();
            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                LoadTiles();
            }
        };
        
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(3);
        _timer.Tick += (s, e) => LoadSystemInfo();
        _timer.Start();
    }

    private void LoadTiles()
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                var tiles = System.Text.Json.JsonSerializer.Deserialize<List<TileItem>>(json);
                if (tiles != null && tiles.Count > 0)
                {
                    _tiles.Clear();
                    foreach (var tile in tiles)
                        _tiles.Add(tile);
                    return;
                }
            }
        }
        catch { }

        _tiles.Clear();
        _tiles.Add(new TileItem { Title = "运行进程", Value = "0", Icon = "Apps24", IconColor = "#FF9800", NavigatePage = "ProcessView" });
        _tiles.Add(new TileItem { Title = "系统服务", Value = "0", Icon = "SlideSettings24", IconColor = "#2196F3", NavigatePage = "ServiceView" });
        _tiles.Add(new TileItem { Title = "磁盘分区", Value = "0", Icon = "HardDrive24", IconColor = "#4CAF50", NavigatePage = "DiskView" });
        _tiles.Add(new TileItem { Title = "网络适配器", Value = "0", Icon = "Wifi24", IconColor = "#9C27B0", NavigatePage = "NetworkView" });
        _tiles.Add(new TileItem { Title = "已安装应用", Value = "0", Icon = "AppsPackage24", IconColor = "#00BCD4", NavigatePage = "AppView" });
    }

    private void SaveTiles()
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            var json = System.Text.Json.JsonSerializer.Serialize(_tiles.ToList());
            File.WriteAllText(_configPath, json);
        }
        catch { }
    }

    private void LoadSystemInfo()
    {
        try
        {
            ComputerName.Text = Environment.MachineName;
            OSVersion.Text = Environment.OSVersion.VersionString;
            UserName.Text = Environment.UserName;

            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            Uptime.Text = $"{uptime.Days}天 {uptime.Hours}小时";

            try
            {
                if (_cpuCounter == null)
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                
                var cpuValue = _cpuCounter.NextValue();
                CpuPercent.Text = $"{cpuValue:F1}%";
                CpuBar.Value = cpuValue;
            }
            catch { }

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

                MemPercent.Text = $"{availablePercent:F0}%";
                MemBar.Value = availablePercent;
                MemInfo.Text = $"{availableGB:F1} GB 可用 / {totalGB:F1} GB 总计";
            }
            catch { }

            try
            {
                var processCount = Process.GetProcesses().Length;
                foreach (var tile in _tiles)
                {
                    if (tile.Title == "运行进程")
                        tile.Value = processCount.ToString();
                }
            }
            catch { }

            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).ToList();
                foreach (var tile in _tiles)
                {
                    if (tile.Title == "磁盘分区")
                        tile.Value = drives.Count.ToString();
                }
            }
            catch { }

            try
            {
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up).ToList();
                foreach (var tile in _tiles)
                {
                    if (tile.Title == "网络适配器")
                        tile.Value = networkInterfaces.Count.ToString();
                }
            }
            catch { }
        }
        catch { }
    }

    private void AddTile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddTileDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _tiles.Add(dialog.Result);
            SaveTiles();
        }
    }

    private void EditMode_Click(object sender, RoutedEventArgs e)
    {
        _isEditMode = !_isEditMode;
        
        UpdateTileEditMode();
        
        EditModeBtn.Content = _isEditMode ? "✓ 完成" : "✎ 编辑";
    }

    private void UpdateTileEditMode()
    {
        for (int i = 0; i < TilesControl.Items.Count; i++)
        {
            var container = TilesControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
            if (container != null)
            {
                var border = FindVisualChild<Border>(container);
                if (border != null)
                {
                    var deleteBtn = FindChild<Border>(border, "DeleteBtn");
                    if (deleteBtn != null)
                    {
                        deleteBtn.Visibility = _isEditMode ? Visibility.Visible : Visibility.Collapsed;
                    }
                    border.Cursor = _isEditMode ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Hand;
                }
            }
        }
    }

    private void Tile_Click(object sender, MouseButtonEventArgs e)
    {
        if (_isEditMode) return;
        
        if (sender is FrameworkElement element && element.Tag is TileItem tile)
        {
            if (!string.IsNullOrEmpty(tile.NavigatePage))
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigateTo(tile.NavigatePage);
            }
        }
    }

    private void DeleteBtn_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (sender is FrameworkElement element && element.Tag is TileItem tile)
        {
            _tiles.Remove(tile);
            UpdateTileEditMode();
            SaveTiles();
        }
    }

    private void Tile_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isEditMode || e.ChangedButton != MouseButton.Left) return;
        
        if (sender is Border border && border.Tag is TileItem tile)
        {
            _dragStartPoint = e.GetPosition(TilesControl);
            _draggedTile = tile;
            _draggedElement = border;
            _isDragging = false;
        }
    }

    private void Tile_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isEditMode || _draggedTile == null || _draggedElement == null) return;
        
        var currentPos = e.GetPosition(TilesControl);
        var diff = _dragStartPoint - currentPos;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || 
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            _isDragging = true;
            _draggedElement.Opacity = 0.5;
            System.Windows.DragDrop.DoDragDrop(_draggedElement, _draggedTile, System.Windows.DragDropEffects.Move);
            
            if (_draggedElement != null)
                _draggedElement.Opacity = 1.0;
            
            _isDragging = false;
            _draggedTile = null;
            _draggedElement = null;
            _dragOverIndex = -1;
        }
    }

    private void Tile_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging) return;
        
        if (sender is FrameworkElement element && element.Tag is TileItem tile)
        {
            if (!string.IsNullOrEmpty(tile.NavigatePage))
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigateTo(tile.NavigatePage);
            }
        }
    }

    private void Tile_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        if (!_isEditMode) return;
        
        if (e.Data.GetDataPresent(typeof(TileItem)))
        {
            var draggedTile = e.Data.GetData(typeof(TileItem)) as TileItem;
            if (draggedTile != null && sender is Border targetBorder && targetBorder.Tag is TileItem targetTile)
            {
                int targetIndex = _tiles.IndexOf(targetTile);
                int draggedIndex = _tiles.IndexOf(draggedTile);
                
                if (targetIndex != draggedIndex && targetIndex >= 0)
                {
                    _dragOverIndex = targetIndex;
                    e.Effects = System.Windows.DragDropEffects.Move;
                }
                else
                {
                    e.Effects = System.Windows.DragDropEffects.None;
                }
            }
        }
        e.Handled = true;
    }

    private void Tile_DragLeave(object sender, System.Windows.DragEventArgs e)
    {
        _dragOverIndex = -1;
    }

    private void Tile_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (!_isEditMode) return;
        
        if (e.Data.GetDataPresent(typeof(TileItem)))
        {
            var draggedTile = e.Data.GetData(typeof(TileItem)) as TileItem;
            if (draggedTile != null && sender is Border targetBorder && targetBorder.Tag is TileItem targetTile)
            {
                int targetIndex = _tiles.IndexOf(targetTile);
                int draggedIndex = _tiles.IndexOf(draggedTile);
                
                if (targetIndex != draggedIndex && targetIndex >= 0)
                {
                    _tiles.Move(draggedIndex, targetIndex);
                    SaveTiles();
                }
            }
        }
        _dragOverIndex = -1;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }

    private static T? FindChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
    {
        if (parent == null) return null;
        
        int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild && typedChild.Name == childName)
                return typedChild;
            
            var foundChild = FindChild<T>(child, childName);
            if (foundChild != null)
                return foundChild;
        }
        return null;
    }
}
