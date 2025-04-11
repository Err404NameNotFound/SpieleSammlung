using System.Windows;

namespace SpieleSammlung.View.Windows;

/// <summary>
/// Interaktionslogik für WindowsMode.xaml
/// </summary>
public partial class WindowsMode
{
    public bool Status;

    public WindowsMode(string message)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        LblWarnung.Text = message;
        Status = false;
        MaxWidth = Width;
        MinWidth = Width;
        MaxHeight = Height;
        MinHeight = Height;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Status = false;
        Close();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        Status = true;
        Close();
    }
}