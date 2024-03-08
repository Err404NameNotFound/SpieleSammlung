using System.Windows;

namespace SpieleSammlung.Windows
{
    /// <summary>
    /// Interaktionslogik für WindowsMode.xaml
    /// </summary>
    public partial class WindowsMode : Window
    {
        public bool status;

        public WindowsMode(string message)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            LblWarnung.Text = message;
            status = false;
            MaxWidth = Width;
            MinWidth = Width;
            MaxHeight = Height;
            MinHeight = Height;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            status = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            status = true;
            Close();
        }
    }
}