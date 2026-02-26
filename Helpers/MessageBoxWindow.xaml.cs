using System.Windows;
using Wpf.Ui.Controls;

namespace WinTools.Helpers;

public enum CustomMessageBoxResult
{
    None,
    OK,
    Cancel,
    Yes,
    No
}

public enum CustomMessageBoxType
{
    Information,
    Warning,
    Error,
    Question
}

public partial class MessageBoxWindow : FluentWindow
{
    public CustomMessageBoxResult Result { get; private set; } = CustomMessageBoxResult.None;

    public MessageBoxWindow()
    {
        InitializeComponent();
    }

    public static CustomMessageBoxResult Show(string message, string title = "提示", CustomMessageBoxType type = CustomMessageBoxType.Information, bool showCancel = false)
    {
        var msgBox = new MessageBoxWindow();
        msgBox.Title = title;
        msgBox.MessageText.Text = message;
        
        var accentColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 212));
        
        switch (type)
        {
            case CustomMessageBoxType.Information:
                msgBox.IconDisplay.Symbol = SymbolRegular.Info24;
                msgBox.IconDisplay.Foreground = accentColor;
                break;
            case CustomMessageBoxType.Warning:
                msgBox.IconDisplay.Symbol = SymbolRegular.Warning24;
                msgBox.IconDisplay.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0));
                break;
            case CustomMessageBoxType.Error:
                msgBox.IconDisplay.Symbol = SymbolRegular.ErrorCircle24;
                msgBox.IconDisplay.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));
                break;
            case CustomMessageBoxType.Question:
                msgBox.IconDisplay.Symbol = SymbolRegular.QuestionCircle24;
                msgBox.IconDisplay.Foreground = accentColor;
                break;
        }

        if (showCancel)
        {
            msgBox.OKButton.Visibility = Visibility.Collapsed;
            msgBox.CancelButton.Visibility = Visibility.Visible;
            msgBox.YesButton.Visibility = Visibility.Collapsed;
            msgBox.NoButton.Visibility = Visibility.Collapsed;
        }
        else if (type == CustomMessageBoxType.Question)
        {
            msgBox.OKButton.Visibility = Visibility.Collapsed;
            msgBox.CancelButton.Visibility = Visibility.Collapsed;
            msgBox.YesButton.Visibility = Visibility.Visible;
            msgBox.NoButton.Visibility = Visibility.Visible;
        }
        else
        {
            msgBox.OKButton.Visibility = Visibility.Visible;
            msgBox.CancelButton.Visibility = Visibility.Collapsed;
            msgBox.YesButton.Visibility = Visibility.Collapsed;
            msgBox.NoButton.Visibility = Visibility.Collapsed;
        }

        try
        {
            msgBox.Owner = System.Windows.Application.Current.MainWindow;
        }
        catch { }

        msgBox.ShowDialog();
        return msgBox.Result;
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        Result = CustomMessageBoxResult.OK;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = CustomMessageBoxResult.Cancel;
        Close();
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Result = CustomMessageBoxResult.Yes;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Result = CustomMessageBoxResult.No;
        Close();
    }
}
