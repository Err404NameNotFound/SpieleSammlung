#region

using System.Windows.Media;
using SpieleSammlung.Model.Schafkopf;

#endregion

namespace SpieleSammlung.View.UserControls.Schafkopf;

/// <summary>
/// Interaktionslogik für SelectableCard.xaml
/// </summary>
public partial class SelectableCard
{
    public const double HEIGHT_FACTOR = 150.0 / 87.0;
    public SelectableCard() => InitializeComponent();

    public bool IsChecked
    {
        get => ToggleBtn.IsChecked == true;
        set => ToggleBtn.IsChecked = value;
    }

    public Card Card
    {
        set => CardVisual.Card = value;
    }

    public bool IsClickable
    {
        get => ToggleBtn.IsEnabled;
        set
        {
            ToggleBtn.IsEnabled = value;
            CardBorder.BorderBrush = value ? Brushes.GreenYellow : Brushes.Transparent;
        }
    }
}