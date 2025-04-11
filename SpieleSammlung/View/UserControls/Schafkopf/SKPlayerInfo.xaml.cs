using System.Windows;
using System.Windows.Media;

namespace SpieleSammlung.View.UserControls.Schafkopf
{
    /// <summary>
    /// Interaktionslogik für SKPlayerInfo.xaml
    /// </summary>
    public partial class SkPlayerInfo
    {
        public static readonly string STATE_EMPTY = Properties.Resources.SKPlayerInfo_empty;
        public static readonly string STATE_AUFSTELLEN = Properties.Resources.SKPlayerInfo_Aufstellen;

        public bool IsStartPlayer
        {
            set => LblPLayerName.Background = value ? Brushes.PaleGreen : Brushes.White;
        }

        public string PlayerName
        {
            get => LblPLayerName.Content.ToString();
            set => LblPLayerName.Content = value;
        }

        public string State
        {
            get => LblPLayerState.Content.ToString();
            set
            {
                LblPLayerState.Content = value;
                LblPLayerState.Visibility = Visibility.Visible;
            }
        }

        public bool Aufgestellt
        {
            get => VisualAufgestellt.Visibility == Visibility.Visible;
            set
            {
                EmptyState();
                VisualAufgestellt.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool Kontra
        {
            get => VisualKontra.Visibility == Visibility.Visible;
            set => VisualKontra.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public SkPlayerInfo()
        {
            InitializeComponent();
        }

        public void NewMatch()
        {
            Kontra = false;
            if (!Aufgestellt) State = STATE_AUFSTELLEN;
        }

        public void NewMatch(string player)
        {
            Aufgestellt = false;
            NewMatch();
            PlayerName = player;
        }

        public void EmptyState()
        {
            LblPLayerState.Visibility = Visibility.Collapsed;
            LblPLayerState.Content = STATE_EMPTY;
        }

        public bool Focused
        {
            get => BorderFocused.BorderBrush == Brushes.Red;
            set => BorderFocused.BorderBrush = value ? Brushes.Red : Brushes.Transparent;
        }
    }
}