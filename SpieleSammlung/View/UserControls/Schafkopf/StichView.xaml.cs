using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SpieleSammlung.Model.Schafkopf;

namespace SpieleSammlung.View.UserControls.Schafkopf;

/// <summary>
/// Interaktionslogik für StichView.xaml
/// </summary>
public partial class StichView : UserControl
{
    private readonly List<CardVisual> _cardVisuals;
    private int _cardCounter;

    public StichView()
    {
        InitializeComponent();
        _cardVisuals = [Card1, Card2, Card3, Card4];
        Reset();
    }

    public void AddCard(Card card, int index)
    {
        _cardVisuals[index].Card = card;
        _cardVisuals[index].Visibility = Visibility.Visible;
        Panel.SetZIndex(_cardVisuals[index], _cardCounter++);
    }

    public void Reset()
    {
        _cardCounter = 0;
        for (int i = 0; i < 4; ++i)
        {
            _cardVisuals[i].Visibility = Visibility.Hidden;
        }
    }
}