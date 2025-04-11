using SpieleSammlung.Model;
using SpieleSammlung.Model.Battleships;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SpieleSammlung.View.UserControls.BattleShips;

namespace SpieleSammlung.View.Sites;

/// <summary>
/// Interaktionslogik für Battleships.xaml
/// </summary>
public partial class Battleships
{
    private readonly List<BattleshipsPlayer> _players;
    private readonly int _activePlayer;
    private BattleshipsMode _mode;

    public Battleships(Player player1, Player player2, bool placeAutoP1, bool placeAutoP2)
    {
        InitializeComponent();
        _activePlayer = 0;
        _mode = BattleshipsMode.PlaceShips;
        _players = [new BattleshipsPlayer(player1), new BattleshipsPlayer(player2)];
        for (int i = 0; i < 10; ++i)
        {
            for (int i1 = 0; i1 < 10; ++i1)
            {
                BoatField fieldP1 = new BoatField();
                fieldP1.ToggleBtn.Click += FieldP1Klicked;
                FieldGrid.Children.Add(fieldP1);
                Grid.SetColumn(fieldP1, i);
                Grid.SetRow(fieldP1, i1);
                _players[0].Field[i, i1] = fieldP1;

                BoatField field2 = new BoatField();
                field2.ToggleBtn.Click += FieldP2Klicked;
                FieldGrid.Children.Add(field2);
                Grid.SetColumn(field2, i);
                Grid.SetRow(field2, i1);
                _players[1].Field[i, i1] = field2;
            }
        }

        ShowActivePlayer();
    }

    private static Coordinate FindKlickedField(object sender, BattleshipsPlayer player)
    {
        bool notFound = true;
        int w = 0;
        int w1 = 0;
        while (notFound && w < 10)
        {
            w1 = 0;
            while (notFound && w1 < 10)
            {
                if (sender.Equals(player.Field[w, w1]))
                {
                    notFound = false;
                    --w1;
                    --w;
                }

                ++w1;
            }

            ++w;
        }

        return new Coordinate(w, w1);
    }

    private void FieldP1Klicked(object sender, RoutedEventArgs e)
    {
        Coordinate temp = FindKlickedField(sender, _players[0]);
    }

    private void FieldP2Klicked(object sender, RoutedEventArgs e)
    {
        Coordinate temp = FindKlickedField(sender, _players[1]);
    }

    private void ShowActivePlayer()
    {
        int otherPlayer = (_activePlayer + 1) % 2;
        for (int i = 0; i < 10; ++i)
        {
            for (int i1 = 0; i1 < 10; ++i1)
            {
                _players[_activePlayer].Field[i, i1].Visibility = Visibility.Visible;
                _players[otherPlayer].Field[i, i1].Visibility = Visibility.Hidden;
            }
        }
    }
}