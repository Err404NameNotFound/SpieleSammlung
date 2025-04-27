using SpieleSammlung.Model.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpieleSammlung.Model.Schafkopf;

public class SchafkopfPlayer : MultiplayerPlayer
{
    #region Properties and Members

    private readonly List<Card> _playedCards;
    public List<Card> PlayableCards { get; }
    public List<SchafkopfMatchPossibility> Possibilities { get; private set; }
    public int Points { get; set; }
    public bool Aufgestellt { get; set; }
    public bool Kontra { get; set; }
    public int Number { get; set; }
    public int TeamIndex { get; set; }
    public bool? ContinueMatch;
    public MultiplayerPlayerState State { get; set; }

    #endregion

    public SchafkopfPlayer(string id, string name) : base(id, name)
    {
        PlayableCards = [];
        _playedCards = [];
        Points = 0;
        NewMatch(-1, false);
        State = MultiplayerPlayerState.Active;
        Possibilities = [];
    }

    #region CalculatingPossibilites

    public void UpdatePossibilities(SchafkopfMatch match) => UpdatePossibilities(match.MinimumGame);

    public void UpdatePossibilities(SchafkopfMode minimumGame)
    {
        Possibilities = [new SchafkopfMatchPossibility(SchafkopfMode.Weiter)];
        if (minimumGame == SchafkopfMode.SoloTout) return;
        List<string> solo = SoloPossibilities();
        if (minimumGame != SchafkopfMode.WenzTout)
        {
            // add WenzTout
            if (minimumGame != SchafkopfMode.Solo)
            {
                List<string> temp;
                if (minimumGame != SchafkopfMode.Wenz)
                {
                    if (minimumGame != SchafkopfMode.Sauspiel)
                    {
                        temp = [];
                        for (int i = 0; i < Card.COLOR_NAMES.Count; ++i)
                        {
                            if (CanPlaySauspielWithColor(Card.COLOR_NAMES[i]))
                                temp.Add(Card.COLOR_NAMES[i]);

                            if (i == 1)
                                i = 2; // skip Card.Heart
                        }

                        if (temp.Count > 0)
                            Possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.Sauspiel, temp));
                    }

                    Possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.Wenz));
                }

                temp = solo.ToList();

                Possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.Solo, temp));
            }

            if (HasEichelUnter())
                Possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.WenzTout));
        }

        if (HasEichelOber())
            Possibilities.Add(new SchafkopfMatchPossibility(SchafkopfMode.SoloTout, solo));
    }

    private bool HasEichelUnter() => PlayableCards.Any(card => card.IsUnter() && card.Color == Card.EICHEL);

    private bool HasEichelOber() => PlayableCards.Any(card => card.IsOber() && card.Color == Card.EICHEL);

    private List<string> SoloPossibilities()
    {
        List<string> temp = [];
        for (int i = 0; i < 4; ++i)
        {
            if (CanPlaySoloWithColor(Card.GetColor(i)))
                temp.Add(Card.GetColor(i));
        }

        if (temp.Count == 0)
            temp = [Card.EICHEL, Card.GRAS, Card.HERZ, Card.SCHELLE];

        return temp;
    }

    public int PossibilityIndexOf(SchafkopfMode mode)
    {
        int w = 0;
        while (w < Possibilities.Count)
        {
            if (Possibilities[w].Mode == mode)
                return w;
            ++w;
        }

        return -1;
    }

    public int PossibilityIndexOf(int index, string color)
    {
        int w = 0;
        while (w < Possibilities[index].Colors.Count)
        {
            if (Possibilities[index].Colors[w].Equals(color))
                return w;
            ++w;
        }

        return -1;
    }

    public void RemovePossibility(SchafkopfMode mode)
    {
        int tmp = PossibilityIndexOf(mode);
        if (tmp != -1)
            Possibilities.RemoveAt(tmp);
    }

    private bool CanPlaySoloWithColor(string color)
    {
        int w = 0;
        while (w < PlayableCards.Count)
        {
            if (PlayableCards[w].Color.Equals(color) &&
                !(PlayableCards[w].IsOber() || PlayableCards[w].IsUnter()))
                return true;

            ++w;
        }

        return false;
    }

    private bool CanPlaySauspielWithColor(string color)
    {
        int w = 0;
        bool hasColor = false;
        while (w < 8)
        {
            if (PlayableCards[w].Color.Equals(color))
            {
                if (PlayableCards[w].IsSau())
                    return false;
                if (!(PlayableCards[w].IsOber() || PlayableCards[w].IsUnter()))
                    hasColor = true;
            }

            ++w;
        }

        return hasColor;
    }

    #endregion

    #region New Match

    public void NewMatch(int n, bool sameRound)
    {
        Aufgestellt = Aufgestellt && sameRound;
        Kontra = false;
        Number = n;
        PlayableCards.Clear();
        _playedCards.Clear();
        TeamIndex = -1;
    }

    public void SortCards(SchafkopfMatchConfig match) => Card.SortCards(match, PlayableCards);

    #endregion

    #region Playing Cards

    public List<Card> GetPlayableCards() => PlayableCards.ToList();

    public bool PlayCard(int index, SchafkopfMatch match)
    {
        if (Number != match.CurrentRound.CurrentPlayer)
            return false;
        Card card = PlayableCards[index];
        match.CurrentRound.CurrentCards.Add(card);
        int value = card.GetValueOfThisCard(match);
        if (match.CurrentCardCount == 1)
        {
            match.CurrentRound.SemiTrumpf = card.Color;
            match.CurrentRound.NewHighestCard(Number, card.GetValueOfThisCard(match));
            if (card.Color.Equals(match.SauspielFarbe) && !card.IsSau() && HasGesuchte(match) && KannWeglaufen(match))
                match.SetIsWegGelaufen();
        }
        else if (value > match.CurrentRound.HighestValue)
            match.CurrentRound.NewHighestCard(Number, value);

        RemovePlayableCard(index);
        return true;
    }

    private int FirstIndexOfColor(string color, SchafkopfMatchConfig match)
    {
        int w = 0;
        while (w < PlayableCards.Count)
        {
            if (PlayableCards[w].Color.Equals(color) && !PlayableCards[w].IsTrumpf(match))
                return w;
            ++w;
        }

        return -1;
    }

    private bool HasColor(string color, SchafkopfMatchConfig match) => FirstIndexOfColor(color, match) != -1;

    private bool HasTrumpf(SchafkopfMatchConfig match)
    {
        int w = 0;
        while (w < PlayableCards.Count)
        {
            if (PlayableCards[w].IsTrumpf(match))
                return true;

            ++w;
        }

        return false;
    }

    public bool HasGesuchte(SchafkopfMatchConfig match)
    {
        if (match.Mode != SchafkopfMode.Sauspiel)
            return false;

        int index = FirstIndexOfColor(match.SauspielFarbe, match);
        return index != -1 && PlayableCards[index].Number.Equals("Sau");
    }

    private bool KannWeglaufen(SchafkopfMatchConfig match)
    {
        int index = FirstIndexOfColor(match.SauspielFarbe, match);
        int end = index + 4;
        if (index == -1 || PlayableCards.Count < end)
            return false;

        bool has4 = true;
        int w = index + 1;
        while (w < end && has4)
        {
            has4 = PlayableCards[w].Color.Equals(match.SauspielFarbe);
            ++w;
        }

        return has4;
    }

    public IReadOnlyList<bool> CheckPlayableCards(SchafkopfMatchConfig match, Card firstCard, bool isWegGelaufen)
    {
        if (PlayableCards.Count == 1)
            return new List<bool> { true };

        bool hasGesuchte = HasGesuchte(match);
        bool hasTrumpf = HasTrumpf(match);
        bool kannWeglaufen = hasGesuchte && KannWeglaufen(match);
        List<bool> playable = [];
        if (firstCard == null)
        {
            playable.AddRange(PlayableCards.Select(card =>
                !hasGesuchte
                || card.IsTrumpf(match)
                || card.IsNotGesuchte(match)
                || kannWeglaufen));
        }
        else
        {
            bool firstCardTrumpf = firstCard.IsTrumpf(match);
            bool hasFirstCardColor = HasColor(firstCard.Color, match);
            foreach (var card in PlayableCards)
            {
                bool canPlayCard;
                bool notGesuchteOrWeggelaufen = match.Mode != SchafkopfMode.Sauspiel || card.IsNotGesuchte(match) ||
                                                isWegGelaufen;
                if (firstCardTrumpf)
                {
                    canPlayCard = card.IsTrumpf(match)
                                  || !hasTrumpf
                                  && notGesuchteOrWeggelaufen;
                }
                else if (card.Color.Equals(firstCard.Color) && !card.IsTrumpf(match))
                {
                    canPlayCard = match.Mode != SchafkopfMode.Sauspiel
                                  || !card.Color.Equals(match.SauspielFarbe)
                                  || card.IsSau()
                                  || isWegGelaufen;
                }
                else
                    canPlayCard = !hasFirstCardColor && notGesuchteOrWeggelaufen;

                playable.Add(canPlayCard);
            }
        }

        return playable;
    }

    public IReadOnlyList<bool> CheckPlayableCards(SchafkopfMatch match)
    {
        return match.CurrentCardCount == 0
            ? CheckPlayableCards(match, null, match.IsWegGelaufen)
            : CheckPlayableCards(match, match.CurrentRound.CurrentCards[0], match.IsWegGelaufen);
    }

    private void RemovePlayableCard(int index)
    {
        _playedCards.Add(PlayableCards[index]);
        PlayableCards.RemoveAt(index);
    }

    #endregion

    #region Rejoining

    public string InfoForRejoin(string separator)
    {
        StringBuilder bob = new StringBuilder();
        bob.Append(PlayableCards.Count).Append(separator);
        foreach (var card in PlayableCards)
        {
            bob.Append(card.Index).Append(separator);
        }

        bob.Append(_playedCards.Count).Append(separator);
        foreach (var card in _playedCards)
        {
            bob.Append(card.Index).Append(separator);
        }

        bob.Append(Possibilities.Count).Append(separator);
        foreach (var possibility in Possibilities)
        {
            bob.Append(possibility.Mode).Append(separator).Append(possibility.Colors.Count)
                .Append(separator);
            foreach (var color in possibility.Colors)
            {
                bob.Append(color).Append(separator);
            }
        }

        bob.Append(Points).Append(separator);
        bob.Append(Aufgestellt).Append(separator);
        bob.Append(Kontra).Append(separator);
        bob.Append(Number).Append(separator);
        bob.Append(TeamIndex).Append(separator);
        bob.Append(ContinueMatch.HasValue ? ContinueMatch.Value.ToString() : "null").Append(separator);
        bob.Append(State);
        return bob.ToString();
    }

    public void RestoreFromInfo(string info, char separator)
    {
        string[] msgParts = info.Split(separator);
        int index;
        int length = int.Parse(msgParts[0]) + 1;
        for (index = 1; index < length; ++index)
        {
            PlayableCards.Add(Card.GetCard(int.Parse(msgParts[index])));
        }

        length = int.Parse(msgParts[index]) + index + 1;
        for (++index; index < length; ++index)
        {
            _playedCards.Add(Card.GetCard(int.Parse(msgParts[index])));
        }

        Possibilities = [];
        length = int.Parse(msgParts[index]);
        for (int i = 0; i < length; ++i)
        {
            SchafkopfMode mode = SchafkopfMatchConfig.StringToSchafkopfMode(msgParts[++index]);
            int colorCount = int.Parse(msgParts[++index]);
            List<string> colors = [];
            for (int color = 0; color < colorCount; ++color) colors.Add(msgParts[++index]);
            Possibilities.Add(new SchafkopfMatchPossibility(mode, colors));
        }

        Points = int.Parse(msgParts[++index]);
        Aufgestellt = bool.Parse(msgParts[++index]);
        Kontra = bool.Parse(msgParts[++index]);
        Number = int.Parse(msgParts[++index]);
        TeamIndex = int.Parse(msgParts[++index]);
        ++index;
        if (msgParts[index].Equals("null"))
        {
            ContinueMatch = null;
        }
        else
        {
            ContinueMatch = bool.Parse(msgParts[index]);
        }

        State = Convert(msgParts[++index]);
    }

    #endregion

    public PointsStorage PlayerPoints => new(Name, Points);

    public static MultiplayerPlayerState Convert(string state)
    {
        return state switch
        {
            nameof(MultiplayerPlayerState.Active) => MultiplayerPlayerState.Active,
            nameof(MultiplayerPlayerState.LeftMatch) => MultiplayerPlayerState.LeftMatch,
            nameof(MultiplayerPlayerState.Inactive) => MultiplayerPlayerState.Inactive,
            _ => throw new ArgumentException("Der String konnte nicht umgewandelt werden")
        };
    }
}