#region

using System.Windows;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.View.Enums;

#endregion

namespace SpieleSammlung.View.Sites;

/// <summary>
/// Interaktionslogik für StartScreen.xaml
/// </summary>
public partial class StartScreen
{
    public delegate void OnChoseMode(GameMode chosenMode, int min, int max);

    public StartScreen() => InitializeComponent();

    public event OnChoseMode ChoseModeEvent;

    private void BtnZufallszahlen_Click(object sender, RoutedEventArgs e)
    {
        //NavigationService.Navigate(new PlayerCreator(GameMode.Zufallszahlen));
    }

    private void BtnLotto_Click(object sender, RoutedEventArgs e)
    {
        //NavigationService.Navigate(new PlayerCreator(GameMode.Lotto));
    }

    private void BtnMaexchen_Click(object sender, RoutedEventArgs e) => ChoseModeEvent(GameMode.Maexchen, 2, 100);

    private void Btn4gewinnt_Click(object sender, RoutedEventArgs e) => ChoseModeEvent(GameMode.VierGewinnt, 1, 2);

    private void BtnKniffel_Click(object sender, RoutedEventArgs e) =>
        ChoseModeEvent(GameMode.Kniffel, KniffelGame.MIN_PLAYER_COUNT, 10);

    private void BtnSchafkopf_Click(object sender, RoutedEventArgs e) => ChoseModeEvent(GameMode.Schafkopf, 4, 7);

    private void BtnMancala_Click(object sender, RoutedEventArgs e) => ChoseModeEvent(GameMode.Mancala, 2, 2);
}