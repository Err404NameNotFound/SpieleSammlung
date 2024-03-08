using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SpieleSammlung.Model.Connect4;

namespace SpieleSammlung.UserControls.Connect4
{
    /// <summary>
    /// Interaktionslogik für Connect4Visual.xaml
    /// </summary>
    public partial class Connect4Visual : UserControl
    {
        private Connect4Tile _color;
        private bool _highlighted;

        public Connect4Tile Color
        {
            get => _color;
            set
            {
                _color = value;
                SetImage();
            }
        }

        public bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                SetImage();
            }
        }

        private string GetImageName()
        {
            return _color switch
            {
                Connect4Tile.Nobody => "EmptyCircle" + HighlightedToString() + ".png",
                Connect4Tile.Player => "YellowCircle" + HighlightedToString() + ".png",
                Connect4Tile.Machine => "RedCircle" + HighlightedToString() + ".png",
                _ => throw new Exception("Unreachable code.")
            };
        }

        private string HighlightedToString() => _highlighted ? "Highlighted" : "";

        private void SetImage()
        {
            BtnImage.Source = new BitmapImage(new Uri(@"../../Images/Connect4/" + GetImageName(), UriKind.Relative));
        }

        public Connect4Visual(Connect4Tile color = Connect4Tile.Nobody)
        {
            InitializeComponent();
            Color = color;
        }

        public Connect4Visual()
        {
            InitializeComponent();
            Color = 0;
        }
    }
}