using SpieleSammlung.Model.Schafkopf;
using System.Windows;
using SpieleSammlung.UserControls.Schafkopf;

namespace SpieleSammlung.Windows
{
    /// <summary>
    /// Interaktionslogik für LastStich.xaml
    /// </summary>
    public partial class LastStich : Window
    {
        private readonly StichView _view;

        public LastStich(SchafkopfMatch match, int offsetUi)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            int startPlayer = match.Rounds[match.Rounds.Count - 2].StartPlayer;
            if (offsetUi == -1)
            {
                for (int i = 0; i < 4; ++i)
                {
                    Stich.AddCard(match.LastCards[i], match.Players[(startPlayer + i) % 4].Number);
                }
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    Stich.AddCard(match.LastCards[i], (match.Players[(startPlayer + i) % 4].Number + 4 - offsetUi) % 4);
                }
            }
        }

        public LastStich(StichView view)
        {
            InitializeComponent();
            Stich.Visibility = Visibility.Collapsed;
            _view = view;
            GridStich.Children.Add(view);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GridStich.Children.Remove(_view);
        }
    }
}