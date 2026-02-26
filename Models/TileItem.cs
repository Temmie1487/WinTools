using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinTools.Models;

public class TileItem : INotifyPropertyChanged
{
    private string _title = "";
    private string _value = "";
    private string _icon = "Info24";
    private string _iconColor = "#0078D4";
    private int _columnSpan = 1;
    private int _rowSpan = 1;
    private string _navigatePage = "";

    public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
    public string Value { get => _value; set { _value = value; OnPropertyChanged(); } }
    public string Icon { get => _icon; set { _icon = value; OnPropertyChanged(); } }
    public string IconColor { get => _iconColor; set { _iconColor = value; OnPropertyChanged(); } }
    public int ColumnSpan { get => _columnSpan; set { _columnSpan = value; OnPropertyChanged(); } }
    public int RowSpan { get => _rowSpan; set { _rowSpan = value; OnPropertyChanged(); } }
    public string NavigatePage { get => _navigatePage; set { _navigatePage = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
