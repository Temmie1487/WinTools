using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using WinTools.Models;

namespace WinTools.Views;

public partial class AddTileDialog : FluentWindow
{
    public TileItem? Result { get; private set; }
    private string _selectedPage = "";

    public AddTileDialog()
    {
        InitializeComponent();
    }

    private void NavigateButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        NavigatePopup.IsOpen = !NavigatePopup.IsOpen;
    }

    private void NavigateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavigateList.SelectedItem is ListBoxItem item)
        {
            _selectedPage = item.Tag?.ToString() ?? "";
            NavigateButtonText.Text = item.Content?.ToString() ?? "无";
            NavigatePopup.IsOpen = false;
        }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            System.Windows.MessageBox.Show("请输入标题", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        Result = new TileItem
        {
            Title = TitleBox.Text,
            Value = "",
            Icon = "Info24",
            IconColor = "#0078D4",
            NavigatePage = _selectedPage
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
