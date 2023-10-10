using System.Windows.Controls;
using SpieleSammlung.Model.Schafkopf;

namespace SpieleSammlung.UserControls.Schafkopf
{
    /// <summary>
    /// Interaktionslogik für SelectableCard.xaml
    /// </summary>
    public partial class SelectableCard : UserControl
    {
        public SelectableCard() => InitializeComponent();

        public const double HEIGHT_FACTOR = 150.0 / 87.0;

        public bool IsChecked
        {
            get => ToggleBtn.IsChecked == true;
            set => ToggleBtn.IsChecked = value;
        }

        public Card Card
        {
            get => CardVisual.Card;
            set => CardVisual.Card = value;
        }

        public bool IsClickable
        {
            get => ToggleBtn.IsEnabled;
            set => ToggleBtn.IsEnabled = value;
        }
    }
}