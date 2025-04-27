using System.Collections.Generic;
using System.Text;

namespace SpieleSammlung.Model.Schafkopf;

public class SchafkopfRound
{
    #region Member and Properties

    public readonly List<Card> CurrentCards;
    public string SemiTrumpf { get; set; }
    public int NextStartPlayer { get; private set; }
    public int HighestValue { get; private set; }
    private int _currentPlayer;
    public int StartPlayer { get; }

    public int CurrentPlayer => _currentPlayer % 4;

    #endregion

    #region Constructor

    private SchafkopfRound(int startPlayer)
    {
        StartPlayer = NextStartPlayer = startPlayer;
        CurrentCards = [];
        SemiTrumpf = "";
        _currentPlayer = NextStartPlayer;
        HighestValue = 0;
    }

    public SchafkopfRound() : this(0)
    {
    }

    public SchafkopfRound(SchafkopfRound previousRound) : this(previousRound.NextStartPlayer)
    {
    }

    public SchafkopfRound(IReadOnlyList<string> msgParts, ref int index)
    {
        int count = int.Parse(msgParts[++index]);
        CurrentCards = new List<Card>(count);
        for (int i = 0; i < count; ++i)
        {
            CurrentCards.Add(Card.GetCard(int.Parse(msgParts[++index])));
        }

        SemiTrumpf = msgParts[++index];
        NextStartPlayer = int.Parse(msgParts[++index]);
        HighestValue = int.Parse(msgParts[++index]);
        _currentPlayer = int.Parse(msgParts[++index]);
        StartPlayer = int.Parse(msgParts[++index]);
    }

    #endregion

    #region Methods

    public void NewHighestCard(int index, int value)
    {
        NextStartPlayer = index;
        HighestValue = value;
    }

    public void SetNextPlayer() => ++_currentPlayer;

    public void ResetNextPlayer() => _currentPlayer = StartPlayer;

    public string InfoForRejoin(string separator)
    {
        StringBuilder bob = new StringBuilder();
        bob.Append(CurrentCards.Count).Append(separator);
        foreach (var card in CurrentCards)
        {
            bob.Append(card.Index).Append(separator);
        }

        bob.Append(SemiTrumpf).Append(separator);
        bob.Append(NextStartPlayer).Append(separator);
        bob.Append(HighestValue).Append(separator);
        bob.Append(_currentPlayer).Append(separator);
        bob.Append(StartPlayer);
        return bob.ToString();
    }

    #endregion
}