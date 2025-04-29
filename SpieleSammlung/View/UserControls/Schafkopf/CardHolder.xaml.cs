using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using SpieleSammlung.Model.Schafkopf;

namespace SpieleSammlung.View.UserControls.Schafkopf;

/// <summary>
/// Interaktionslogik für CardHolder.xaml
/// </summary>
public partial class CardHolder
{
    private List<Card> _cards;
    private readonly List<SelectableCard> _cardVisuals;
    private bool? _aufgestellt;
    private const int INDEX_FIRST_CARD_SECOND_HALF = 4;

    public CardHolder()
    {
        InitializeComponent();
        _cardVisuals = [Card1, Card2, Card3, Card4, Card5, Card6, Card7, Card8];
        SelectedCard = -1;
        BtnAufstellen.Width = BtnShowRest.Width;
        ChangeViewMode(false);
    }

    [Browsable(true)]
    [Category("Action")]
    [Description("Invoked when user clicks a card")]
    public event EventHandler CardClicked;

    [Browsable(true)]
    [Category("Action")]
    [Description("Invoked when user clicks a card")]
    public event EventHandler ShowsAllCards;

    public int SelectedCard { get; private set; }

    public bool Aufgestellt => _aufgestellt ?? false;

    public List<Card> Cards
    {
        get => _cards;
        set
        {
            _cards = value;
            int i;
            for (i = 0; i < _cards.Count; ++i)
            {
                _cardVisuals[i].Card = _cards[i];
                _cardVisuals[i].Visibility = Visibility.Visible;
            }

            for (; i < _cardVisuals.Count; ++i)
                _cardVisuals[i].Visibility = Visibility.Collapsed;

            if (!_aufgestellt.HasValue)
                ChangeViewMode(false);
        }
    }

    private bool HasSelectedCard => SelectedCard > -1;

    public bool CanClickCards
    {
        get => Card1.IsClickable;
        set
        {
            foreach (var card in _cardVisuals)
                card.IsClickable = value;

            if (!value && HasSelectedCard)
                RemoveSelection();
        }
    }

    public bool Focused
    {
        get => Border.BorderBrush == Brushes.Red;
        set => Border.BorderBrush = value ? Brushes.Red : Brushes.Transparent;
    }

    public void RemoveCard(int index)
    {
        _cards.RemoveAt(index);
        for (int i = index; i < _cards.Count; ++i)
            _cardVisuals[i].Card = _cards[i];

        _cardVisuals[_cards.Count].Visibility = Visibility.Collapsed;
    }

    public void MarkSelectableCards(IReadOnlyList<bool> playableCards)
    {
        SelectedCard = -1;
        for (int i = 0; i < playableCards.Count; ++i)
        {
            _cardVisuals[i].IsClickable = playableCards[i];
        }
    }

    public void Reset() => _aufgestellt = null;

    private void RemoveSelection()
    {
        if (SelectedCard != -1)
        {
            _cardVisuals[SelectedCard].IsChecked = false;
            SelectedCard = -1;
        }
    }

    private void CardPressed(int index)
    {
        if (SelectedCard != -1)
            _cardVisuals[SelectedCard].IsChecked = false;

        if (_cardVisuals[index].IsChecked)
        {
            SelectedCard = index;
            CardClicked?.Invoke(this, new RoutedEventArgs());
        }
        else
            SelectedCard = -1;
    }

    private void Card1_Checked(object sender, RoutedEventArgs e) => CardPressed(0);

    private void Card2_Checked(object sender, RoutedEventArgs e) => CardPressed(1);

    private void Card3_Checked(object sender, RoutedEventArgs e) => CardPressed(2);

    private void Card4_Checked(object sender, RoutedEventArgs e) => CardPressed(3);

    private void Card5_Checked(object sender, RoutedEventArgs e) => CardPressed(4);

    private void Card6_Checked(object sender, RoutedEventArgs e) => CardPressed(5);

    private void Card7_Checked(object sender, RoutedEventArgs e) => CardPressed(6);

    private void Card8_Checked(object sender, RoutedEventArgs e) => CardPressed(7);

    private void BtnShowRest_Click(object sender, RoutedEventArgs e)
    {
        ShowAllCards(false);
        ShowsAllCards?.Invoke(this, e);
    }

    private void BtnAufstellen_Click(object sender, RoutedEventArgs e)
    {
        ShowAllCards(true);
        ShowsAllCards?.Invoke(this, e);
    }

    public void ShowAllCards(bool value)
    {
        _aufgestellt = value;
        ChangeViewMode(true);
    }

    private void ChangeViewMode(bool showCards)
    {
        Visibility visibilityBtn, visibilityCards;
        if (showCards)
        {
            visibilityBtn = Visibility.Collapsed;
            visibilityCards = Visibility.Visible;
        }
        else
        {
            visibilityBtn = Visibility.Visible;
            visibilityCards = Visibility.Hidden;
        }

        BtnAufstellen.Visibility = visibilityBtn;
        BtnShowRest.Visibility = visibilityBtn;
        for (int i = INDEX_FIRST_CARD_SECOND_HALF; i < _cardVisuals.Count; ++i)
            _cardVisuals[i].Visibility = visibilityCards;
    }

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_cards is { Count: > 0 })
        {
            //2 * Padding Border + 2 * Thickness Border + 2 * 1 * Padding Cards + 6 * 2 * Padding Cards -> 2*5+2*2+2*5+6*2*5 = 84 
            double height = SelectableCard.HEIGHT_FACTOR * (CardsGrid.ActualWidth - 84) / _cards.Count;
            foreach (var cardVisual in _cardVisuals)
                cardVisual.MaxHeight = height;
        }
    }
}