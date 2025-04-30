#region

using System;
using System.Collections.Generic;
using static SpieleSammlung.Model.Schafkopf.CardColor;
using static SpieleSammlung.Model.Schafkopf.CardNumber;

#endregion

namespace SpieleSammlung.Model.Schafkopf;

public class Card
{
    private const string CALCULATION_NOT_POSSIBLE = "Calculation not possible for mode shuffle";

    #region Static fields

    public static readonly IReadOnlyList<CardColor> COLOR_NAMES = new List<CardColor> { Eichel, Gras, Herz, Schelle };

    public static readonly IReadOnlyList<CardNumber> NUMBER_NAMES = new List<CardNumber>
        { Sieben, Acht, Neun, Koenig, Zehn, Sau, Unter, Ober };

    public static readonly IReadOnlyList<Card> ALL_CARDS = GenerateAllCards();

    private static readonly IReadOnlyList<int> NumbersWenz = new List<int> { 0, 1, 2, 7, 3, 4, 5, 6 };

    #endregion

    #region Fields

    public CardColor Color { get; }
    public CardNumber Number { get; }
    public int Points { get; }
    public int Index { get; }

    #endregion

    #region Constructors

    private Card(CardColor color, CardNumber number, int index)
    {
        Color = color;
        Number = number;
        Points = Number switch
        {
            Sieben => 0,
            Acht => 0,
            Neun => 0,
            Koenig => 4,
            Zehn => 10,
            Sau => 11,
            Unter => 2,
            Ober => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
        Index = index;
    }

    private static Card[] GenerateAllCards()
    {
        Card[] cards = new Card[COLOR_NAMES.Count * NUMBER_NAMES.Count];
        int i = 0;
        foreach (var color in COLOR_NAMES)
        {
            foreach (var number in NUMBER_NAMES)
            {
                cards[i] = new Card(color, number, i);
                ++i;
            }
        }

        return cards;
    }

    #endregion

    #region Static methods

    public static CardColor ParseColor(string color) => (CardColor)Enum.Parse(typeof(CardColor), color);
    public static CardNumber ParseNumber(string number) => (CardNumber)Enum.Parse(typeof(CardColor), number);

    public static CardColor? ParseNullableColor(string color) =>
        string.IsNullOrEmpty(color) || color == "null" ? null : ParseColor(color);

    public static CardNumber? ParseNullableNumber(string number) =>
        string.IsNullOrEmpty(number) || number == "null" ? null : ParseNumber(number);

    public static string StringifyColor(CardColor? color) => color == null ? "null" : color.ToString();
    public static string StringifyNumber(CardNumber? number) => number == null ? "null" : number.ToString();

    public static Card GetCard(string color, string number) =>
        GetCard((int)ParseColor(color), (int)ParseNumber(number));

    public static Card GetCard(CardColor color, CardNumber number) =>
        GetCard(ColorNameToInt(color), NumberNameToInt(number));

    public static Card GetCard(int colorIndex, int numberIndex) => ALL_CARDS[colorIndex * 8 + numberIndex];

    public static Card GetCard(int index) => ALL_CARDS[index];
    public static CardColor GetColor(int index) => COLOR_NAMES[index];

    private static int ColorNameToInt(CardColor color) => (int)color;

    private static int NumberNameToInt(CardNumber number, bool wenz = false)
    {
        if (!wenz)
            return (int)number;

        int w = 0;
        bool notFound = true;
        while (w < NUMBER_NAMES.Count && notFound)
        {
            if (NUMBER_NAMES[NumbersWenz[w]] == number)
                notFound = false;
            else
                ++w;
        }

        return w;
    }

    public static void SortCards(SchafkopfMatchConfig match, List<Card> cards)
    {
        var values = new Tuple<Card, int>[cards.Count];
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = new Tuple<Card, int>(cards[i], cards[i].GetSortValueOfThisCard(match));
        }

        Array.Sort(values, (tuple1, tuple2) => -tuple1.Item2.CompareTo(tuple2.Item2));
        for (int i = 0; i < cards.Count; ++i)
        {
            cards[i] = values[i].Item1;
        }
    }

    #endregion

    #region Calculated Properties

    public bool IsOber() => Number == Ober;
    public bool IsUnter() => Number == Unter;
    public bool IsSau() => Number == Sau;

    public bool IsGesuchte(SchafkopfMatchConfig match) => Color.Equals(match.SauspielFarbe) && IsSau();
    public bool IsNotGesuchte(SchafkopfMatchConfig match) => !Color.Equals(match.SauspielFarbe) || Number != Sau;

    public override string ToString() => $"{Color} {Number}";

    public bool IsTrumpf(SchafkopfMatchConfig match)
    {
        switch (match.Mode)
        {
            case SchafkopfMode.Sauspiel or SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                return IsOber() || IsUnter() || Color.Equals(match.Trumpf);
            case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
                return IsUnter();
            case SchafkopfMode.Weiter:
            default: throw new Exception(CALCULATION_NOT_POSSIBLE);
        }
    }

    public int GetValueOfThisCard(SchafkopfMatch match) => GetValueOfThisCard(match, match.CurrentRound.SemiTrumpf);

    public int GetValueOfThisCard(SchafkopfMatchConfig match, CardColor? semiTrumpf)
    {
        switch (match.Mode)
        {
            case SchafkopfMode.Sauspiel or SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                switch (Number)
                {
                    // 6x Semi, 6x Trumpf, 4x Ober, 4x Unter → 20 + 1 Werte → 
                    case Ober: return 20 - ColorNameToInt(Color);
                    case Unter: return 16 - ColorNameToInt(Color);
                    case Sieben:
                    case Acht:
                    case Neun:
                    case Koenig:
                    case Zehn:
                    case Sau:
                    {
                        if (!(Color.Equals(match.Trumpf) || Color.Equals(semiTrumpf)))
                            return 0;

                        return (Color.Equals(match.Trumpf) ? 7 : 1) + NumberNameToInt(Number);
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
                // 7x Semi, 4x Unter -> 11+1 Werte
                if (IsUnter())
                    return 12 - ColorNameToInt(Color);

                return Color.Equals(semiTrumpf) ? NumberNameToInt(Number, true) + 1 : 0;
            case SchafkopfMode.Weiter:
            default:
                throw new Exception(CALCULATION_NOT_POSSIBLE);
        }
    }

    private int GetSortValueOfThisCard(SchafkopfMatchConfig match)
    {
        switch (match.Mode)
        {
            case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
            {
                if (IsUnter())
                    return 31 - ColorNameToInt(Color);

                return (3 - ColorNameToInt(Color)) * 7 + NumberNameToInt(Number, true);
            }
            case SchafkopfMode.Sauspiel or SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                switch (Number)
                {
                    case Ober: return 31 - ColorNameToInt(Color);
                    case Unter: return 27 - ColorNameToInt(Color);
                    default:
                    {
                        if (Color.Equals(match.Trumpf)) return 18 + NumberNameToInt(Number);
                        //27 - 4 - 5 = 18 (neither OBER nor UNTER)
                        int factor;
                        if (match.Mode is SchafkopfMode.Solo or SchafkopfMode.SoloTout)
                        {
                            if (GetColor(0).Equals(match.Trumpf))
                                factor = 3 - ColorNameToInt(Color);
                            else if (Color.Equals(GetColor(0)))
                                factor = 2;
                            else if (Color.Equals(GetColor(3)))
                                factor = 0;
                            else if (Color.Equals(GetColor(1)))
                                factor = 1;
                            else
                                factor = match.Trumpf.Equals(GetColor(1)) ? 1 : 0;
                        }
                        else
                        {
                            if (Color.Equals(match.SauspielFarbe))
                                factor = 2;
                            else if (Color.Equals(GetColor(0)))
                                factor = 1;
                            else if (Color.Equals(GetColor(3)))
                                factor = 0;
                            else
                                factor = match.SauspielFarbe.Equals(GetColor(0)) ? 1 : 0;
                        }

                        return factor * 6 + NumberNameToInt(Number);
                    }
                }
            default:
                throw new NotSupportedException(CALCULATION_NOT_POSSIBLE);
        }
    }

    #endregion
}