using System;
using System.Windows.Media.Imaging;
using SpieleSammlung.Model.Battleships;

namespace SpieleSammlung.View.UserControls.BattleShips
{
    /// <summary>
    /// Interaktionslogik für BoatField.xaml
    /// </summary>
    public partial class BoatField
    {
        private bool _isHit;
        private Boat _boat;
        private int _boatPart;

        public bool IsHit
        {
            get => _isHit;
            set
            {
                _isHit = value;
                BuildImagePath();
            }
        }

        public Boat PlacedBoat
        {
            get => _boat;
            set
            {
                _boat = value;
                BuildImagePath();
            }
        }

        public int BoatPart
        {
            get => _boatPart;
            private set
            {
                _boatPart = value;
                BuildImagePath();
            }
        }

        public BoatField()
        {
            InitializeComponent();
            _isHit = false;
            _boat = null;
            _boatPart = -1;
        }

        public BoatField(Boat b, int bp)
        {
            InitializeComponent();
            _isHit = false;
            _boat = b;
            _boatPart = bp;
            BuildImagePath();
        }

        public bool IsBoat()
        {
            return _boat != null;
        }

        private void BuildImagePath()
        {
            if (IsBoat())
            {
                BtnImage.Source =
                    new BitmapImage(new Uri(
                        @"Boat-" + _boat.Width + "-" + (_isHit ? "h" : "v") + "-" + _boatPart + "-" + ".png",
                        UriKind.Relative));
            }
            else
            {
                BtnImage.Source =
                    new BitmapImage(new Uri(@"empty-" + (_isHit ? "isHit" : "notHit") + ".png", UriKind.Relative));
            }
        }
    }
}