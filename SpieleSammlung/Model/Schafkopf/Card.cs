using System;
using System.Collections.Generic;

namespace SpieleSammlung.Model.Schafkopf
{
    public class Card
    {
        #region Constants

        public const string SAU = "Sau";
        public const string KOENIG = "Koenig";
        public const string OBER = "Ober";
        public const string UNTER = "Unter";
        public const string ZEHN = "Zehn";
        public const string NEUN = "Neun";
        public const string ACHT = "Acht";
        public const string SIEBEN = "Sieben";
        public const string EICHEL = "Eichel";
        public const string GRAS = "Gras";
        public const string HERZ = "Herz";
        public const string SCHELLE = "Schelle";
        private const string CALCULATION_NOT_POSSIBLE = "Calculation not possible for mode shuffle";

        #endregion

        #region Static fields

        public static readonly IReadOnlyList<string> COLOR_NAMES = new List<string> { EICHEL, GRAS, HERZ, SCHELLE };

        public static readonly IReadOnlyList<string> NUMBER_NAMES = new List<string>
            { SIEBEN, ACHT, NEUN, KOENIG, ZEHN, SAU, UNTER, OBER };
        public static readonly IReadOnlyList<Card> ALL_CARDS = GenerateAllCards();

        private static readonly IReadOnlyList<int> NumbersWenz = new List<int> { 0, 1, 2, 7, 3, 4, 5, 6 };

        #endregion

        #region Fields

        public string Color { get; }
        public string Number { get; }
        public int Points { get; }
        public int Index { get; }

        #endregion

        #region Constructors

        private Card(int c, int n)
        {
            Color = GetColor(c);
            Number = IntToNumber(n);
            int temp = NumberNameToInt(Number);
            Points = temp switch
            {
                < 3 => 0,
                3 => 4,
                4 => 10,
                5 => 11,
                6 => 2,
                _ => 3
            };
            Index = c * 8 + n;
        }

        private static Card[] GenerateAllCards()
        {
            Card[] cards = new Card[32];
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    cards[8 * i + j] = new Card(i, j);
                }
            }

            return cards;
        }

        #endregion

        #region Static methods

        public static Card GetCard(string color, string number)
            => ALL_CARDS[ColorNameToInt(color) * 8 + NumberNameToInt(number)];

        public static Card GetCard(int index) => ALL_CARDS[index];
        public static string GetColor(int index) => COLOR_NAMES[index];
        private static string IntToNumber(int c) => NUMBER_NAMES[c];

        private static int ColorNameToInt(string color)
        {
            int w = 0;
            bool notFound = true;
            while (w < 4 && notFound)
            {
                if (COLOR_NAMES[w].Contains(color)) notFound = false;
                else ++w;
            }

            return w;
        }

        private static int NumberNameToInt(string number, bool wenz = false)
        {
            int w = 0;
            bool notFound = true;
            if (wenz)
            {
                while (w < 8 && notFound)
                {
                    if (NUMBER_NAMES[NumbersWenz[w]].Contains(number)) notFound = false;
                    else ++w;
                }
            }
            else
            {
                while (w < 8 && notFound)
                {
                    if (NUMBER_NAMES[w].Contains(number)) notFound = false;
                    else ++w;
                }
            }

            return w;
        }
        
        public static void SortCardsShort(SchafkopfMatchConfig match, List<Card> cards)
        {
            var values = new Tuple<Card, int>[cards.Count];
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = new Tuple<Card, int>(cards[i], cards[i].GetSortValueOfThisCard(match));
            }
            Array.Sort(values, (tuple1, tuple2) => -tuple1.Item2.CompareTo(tuple2.Item2));
            for (int i = 0; i < cards.Count; ++i) cards[i] = values[i].Item1;
        }

        #endregion

        #region Calculated Properties

        public bool IsOber() => Number.Equals(OBER);
        public bool IsUnter() => Number.Equals(UNTER);
        public bool IsSau() => Number.Equals(SAU);

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

        public int GetValueOfThisCard(SchafkopfMatch match) => GetValueOfThisCard(match, match.CurrentRound.semiTrumpf);

        public int GetValueOfThisCard(SchafkopfMatchConfig match, string semiTrumpf)
        {
            switch (match.Mode)
            {
                case SchafkopfMode.Sauspiel or SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                    switch (Number)
                    {
                        // 6x Semi, 6x Trumpf, 4x Ober, 4x Unter -> 20+1 Werte -> 
                        case OBER: return 20 - ColorNameToInt(Color);
                        case UNTER: return 16 - ColorNameToInt(Color);
                        default:
                        {
                            if (!(Color.Equals(match.Trumpf) || Color.Equals(semiTrumpf)))
                            {
                                return 0;
                            }

                            return (Color.Equals(match.Trumpf) ? 7 : 1) + NumberNameToInt(Number);
                        }
                    }
                case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
                    // 7x Semi, 4x Unter -> 11+1 Werte
                    if (IsUnter()) return 12 - ColorNameToInt(Color);
                    return Color.Equals(semiTrumpf) ? NumberNameToInt(Number, true) + 1 : 0;
                case SchafkopfMode.Weiter:
                default: throw new Exception(CALCULATION_NOT_POSSIBLE);
            }
        }

        public int GetSortValueOfThisCard(SchafkopfMatchConfig match)
        {
            switch (match.Mode)
            {
                case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
                {
                    if (IsUnter()) return 31 - ColorNameToInt(Color);
                    return (3 - ColorNameToInt(Color)) * 7 + NumberNameToInt(Number, true);
                }
                case SchafkopfMode.Sauspiel or SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                    switch (Number)
                    {
                        case OBER: return 31 - ColorNameToInt(Color);
                        case UNTER: return 27 - ColorNameToInt(Color);
                        default:
                        {
                            if (Color.Equals(match.Trumpf)) return 18 + NumberNameToInt(Number);
                            //27 - 4 - 5 = 18 (weder Ober noch unter)
                            int factor;
                            if (match.Mode is SchafkopfMode.Solo or SchafkopfMode.SoloTout)
                            {
                                if (GetColor(0).Equals(match.Trumpf))
                                {
                                    factor = 3 - ColorNameToInt(Color);
                                }
                                else if (Color.Equals(GetColor(0))) factor = 2;
                                else if (Color.Equals(GetColor(3))) factor = 0;
                                else if (Color.Equals(GetColor(1))) factor = 1;
                                else factor = match.Trumpf.Equals(GetColor(1)) ? 1 : 0;
                            }
                            else
                            {
                                if (Color.Equals(match.SauspielFarbe)) factor = 2;
                                else if (Color.Equals(GetColor(0))) factor = 1;
                                else if (Color.Equals(GetColor(3))) factor = 0;
                                else factor = match.SauspielFarbe.Equals(GetColor(0)) ? 1 : 0;
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
}