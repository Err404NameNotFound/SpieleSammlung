using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SpieleSammlung.Model.Schafkopf;

namespace SpieleSammlung.UserControls.Schafkopf
{
    /// <summary>
    /// Interaktionslogik für CardVisual.xaml
    /// </summary>
    public partial class CardVisual : UserControl
    {
        public CardVisual()
        {
            InitializeComponent();
        }

        private Card _card;

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
}