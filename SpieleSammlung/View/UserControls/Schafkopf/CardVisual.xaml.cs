#region

using System;
using System.Windows.Media.Imaging;
using SpieleSammlung.Model.Schafkopf;

#endregion

namespace SpieleSammlung.View.UserControls.Schafkopf;

/// <summary>
/// Interaktionslogik für CardVisual.xaml
/// </summary>
public partial class CardVisual
{
    private Card _card;

    public CardVisual()
    {
        InitializeComponent();
    }

    public Card Card
    {
        get => _card;
        set
        {
            _card = value;
            CardImage.Source = new BitmapImage(new Uri(@"../../Images/Schafkopf/" + value + ".jpg",
                UriKind.Relative));
        }
    }
}