using System;
using System.Windows.Media.Imaging;

namespace SpieleSammlung.View.UserControls
{
    /// <summary>
    /// Interaktionslogik für CheckView.xaml
    /// </summary>
    public partial class CheckView
    {
        private bool? _checked;

        private readonly BitmapImage _imageUndecided = new(new Uri(@"..\..\Images\undecided.png", UriKind.Relative));

        private readonly BitmapImage _imageChecked = new(new Uri(@"..\..\Images\checked.png", UriKind.Relative));

        private readonly BitmapImage _imageUnchecked = new(new Uri(@"..\..\Images\crossed.png", UriKind.Relative));

        public bool? IsChecked
        {
            get => _checked;
            set
            {
                _checked = value;
                Visual.Source = value == null ? _imageUndecided : value.Value ? _imageChecked : _imageUnchecked;
            }
        }

        public CheckView() => InitializeComponent();
    }
}