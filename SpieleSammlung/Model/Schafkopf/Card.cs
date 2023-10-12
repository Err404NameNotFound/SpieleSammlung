using System;

namespace SpieleSammlung.Model.Schafkopf
{
    public class Card
    {
        public const string SAU = "Sau";
        public const string OBER = "Ober";
        public const string UNTER = "Unter";
        public const string EICHEL = "Eichel";
        public const string GRAS = "Gras";
        public const string HERZ = "Herz";
        public const string SCHELLE = "Schelle";
        private const string CALCULATION_NOT_POSSIBLE = "Calculation not possible for mode shuffle";
        public static Card[] allCards;
        public static string[] ColorNames { get; } = { EICHEL, GRAS, HERZ, SCHELLE };

        private static readonly string[] NumberNames =
            { "Sieben", "Acht", "Neun", "Koenig", "Zehn", SAU, UNTER, OBER };

        private static readonly int[] NumbersWenz = { 0, 1, 2, 7, 3, 4, 5, 6 };
        public readonly string color;
        public readonly string number;
        public readonly int points;
        public readonly int index;

        private Card(string c, string n)
        {
            color = c;
            number = n;
            int temp = NumberNameToInt(number);
            points = temp switch
            {
                < 3 => 0,
                3 => 4,
                4 => 10,
                5 => 11,
                6 => 2,
                _ => 3
            };
        }

        private Card(int c, int n) : this(IntToColor(c), IntToNumber(n))
        {
            index = c * 8 + n;
        }

        public static void InitializeAllCards()
        {
            allCards = new Card[32];
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    allCards[8 * i + j] = new Card(i, j);
                }
            }
        }

        public bool IsOber() => number.Equals(OBER);
        public bool IsUnter() => number.Equals(OBER);
        public bool IsSau() => number.Equals(SAU);
        
        public override string ToString() => color + " " + number;

        public static string IntToColor(int c) => ColorNames[c];

        public static string IntToNumber(int c) => NumberNames[c];

        public static int ColorNameToInt(string color)
        {
            int w = 0;
            bool notFound = true;
            while (w < 4 && notFound)
            {
                if (ColorNames[w].Contains(color)) notFound = false;
                else ++w;
            }

            return w;
        }

        public static int NumberNameToInt(string number, bool wenz = false)
        {
            int w = 0;
            bool notFound = true;
            if (wenz)
            {
                while (w < 8 && notFound)
                {
                    if (NumberNames[NumbersWenz[w]].Contains(number)) notFound = false;
                    else ++w;
                }
            }
            else
            {
                while (w < 8 && notFound)
                {
                    if (NumberNames[w].Contains(number)) notFound = false;
                    else ++w;
                }
            }

            return w;
        }

        public static bool IsTrumpf(Card card, SchafkopfMatch match)
        {
            switch (match.Mode)
            {
                case SchafkopfMode.Sauspiel:
                case SchafkopfMode.Solo:
                case SchafkopfMode.SoloTout:
                    return card.number.Equals(OBER) || card.number.Equals(UNTER) || card.color.Equals(match.Trumpf);
                case SchafkopfMode.Wenz:
                case SchafkopfMode.WenzTout:
                    return card.number.Equals(UNTER);
                case SchafkopfMode.Weiter:
                    break;
            }

            throw new Exception(CALCULATION_NOT_POSSIBLE);
        }

        public bool IsTrumpf(SchafkopfMatch match) => IsTrumpf(this, match);

        public bool IsHigherThan(Card other, SchafkopfMatch match)
        {
            return GetValueOfCard(this, match) > GetValueOfCard(other, match);
        }

        public static int GetValueOfCard(Card card, SchafkopfMatch match)
        {
            switch (match.Mode)
            {
                case SchafkopfMode.Sauspiel:
                case SchafkopfMode.Solo:
                case SchafkopfMode.SoloTout:
                    switch (card.number)
                    {
                        // 6x Semi, 6x Trumpf, 4x Ober, 4x Unter -> 20+1 Werte -> 
                        case OBER: return 20 - ColorNameToInt(card.color);
                        case UNTER: return 16 - ColorNameToInt(card.color);
                        default:
                        {
                            if (!(card.color.Equals(match.Trumpf) || card.color.Equals(match.CurrentRound.semiTrumpf)))
                            {
                                return 0;
                            }

                            return (card.color.Equals(match.Trumpf) ? 7 : 1) + NumberNameToInt(card.number);
                        }
                    }
                case SchafkopfMode.Wenz:
                case SchafkopfMode.WenzTout:
                    // 7x Semi, 4x Unter -> 11+1 Werte
                    if (card.number.Equals(UNTER)) return 12 - ColorNameToInt(card.color);
                    return card.color.Equals(match.CurrentRound.semiTrumpf)
                        ? NumberNameToInt(card.number, true) + 1
                        : 0;
                case SchafkopfMode.Weiter:
                    break;
            }

            throw new Exception(CALCULATION_NOT_POSSIBLE);
        }

        public int GetValueOfThisCard(SchafkopfMatch match)
        {
            return GetValueOfCard(this, match);
        }

        public static int GetSortValueOfCard(Card card, SchafkopfMatch match)
        {
            switch (match.Mode)
            {
                case SchafkopfMode.Wenz or SchafkopfMode.WenzTout:
                {
                    if (card.number.Equals(UNTER)) return 31 - ColorNameToInt(card.color);
                    return (3 - ColorNameToInt(card.color)) * 7 + NumberNameToInt(card.number, true);
                }
                case SchafkopfMode.Sauspiel or SchafkopfMode.Solo or SchafkopfMode.SoloTout:
                    switch (card.number)
                    {
                        case OBER: return 31 - ColorNameToInt(card.color);
                        case UNTER: return 27 - ColorNameToInt(card.color);
                        default:
                        {
                            if (card.color.Equals(match.Trumpf)) return 18 + NumberNameToInt(card.number);
                            //27 - 4 - 5 = 18 (weder Ober noch unter)
                            int factor;
                            if (match.Mode is SchafkopfMode.Solo or SchafkopfMode.SoloTout)
                            {
                                if (IntToColor(0).Equals(match.Trumpf))
                                {
                                    factor = 3 - ColorNameToInt(card.color);
                                }
                                else if (card.color.Equals(IntToColor(0))) factor = 2;
                                else if (card.color.Equals(IntToColor(3))) factor = 0;
                                else if (card.color.Equals(IntToColor(1))) factor = 1;
                                else factor = match.Trumpf.Equals(IntToColor(1)) ? 1 : 0;
                            }
                            else
                            {
                                if (card.color.Equals(match.SauspielFarbe)) factor = 2;
                                else if (card.color.Equals(IntToColor(0))) factor = 1;
                                else if (card.color.Equals(IntToColor(3))) factor = 0;
                                else factor = match.SauspielFarbe.Equals(IntToColor(0)) ? 1 : 0;
                            }

                            return factor * 6 + NumberNameToInt(card.number);
                        }
                    }
                default:
                    throw new Exception(CALCULATION_NOT_POSSIBLE);
            }
        }

        public int GetSortValueOfThisCard(SchafkopfMatch match)
        {
            return GetSortValueOfCard(this, match);
        }
    }
}