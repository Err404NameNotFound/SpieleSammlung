using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Schafkopf;
using static SpieleSammlung.Model.Schafkopf.Card;
using static SpieleSammlung.Model.Schafkopf.CardColor;
using static SpieleSammlung.Model.Schafkopf.CardNumber;
using static SpieleSammlung.Model.Util.ArrayPrinter;

namespace SpieleSammlungTests.Model.Schafkopf;

[TestClass]
public class CardTest
{
    private static void TestToString(CardColor color, CardNumber number)
    {
        Assert.AreEqual($"{color} {number}", GetCard(color, number).ToString());
    }

    private static readonly IReadOnlyCollection<CardColor> SauspielColors = new List<CardColor>
        { Eichel, Gras, Schelle };

    private static readonly IReadOnlyList<CardNumber> NumbersShort = new List<CardNumber>
        { Sieben, Acht, Neun, Koenig, Zehn, Sau };

    private static readonly IReadOnlyList<CardNumber> WenzNumbers = new List<CardNumber>
        { Sieben, Acht, Neun, Ober, Koenig, Zehn, Sau };

    private static readonly IReadOnlyList<int[]> ShuffledCards = new List<int[]>
    {
        new[]
        {
            16, 29, 22, 27, 6, 9, 31, 10, 5, 0, 25, 15, 8, 3, 23, 21, 26, 11, 24, 18, 30, 7, 4, 13, 14, 2, 1, 12,
            19, 20, 17, 28
        },
        new[]
        {
            29, 8, 15, 19, 20, 6, 31, 25, 12, 27, 10, 3, 22, 0, 9, 1, 5, 13, 18, 30, 21, 28, 4, 23, 24, 11, 14, 2,
            26, 7, 16, 17
        },
        new[]
        {
            19, 2, 12, 1, 31, 25, 14, 24, 18, 30, 5, 3, 13, 11, 17, 21, 20, 27, 7, 16, 0, 6, 8, 9, 10, 26, 28, 22,
            29, 23, 4, 15
        },
        new[]
        {
            23, 1, 25, 29, 13, 9, 24, 26, 2, 6, 16, 3, 19, 8, 15, 28, 5, 10, 11, 4, 30, 21, 14, 7, 31, 27, 17, 0,
            22, 12, 20, 18
        },
        new[]
        {
            4, 25, 22, 8, 18, 26, 31, 11, 23, 24, 28, 13, 30, 17, 6, 16, 9, 29, 2, 27, 0, 10, 5, 1, 12, 21, 15, 3,
            20, 19, 14, 7
        },
        new[]
        {
            12, 20, 2, 19, 3, 25, 16, 10, 14, 17, 6, 18, 8, 7, 28, 0, 11, 26, 24, 23, 31, 13, 30, 21, 1, 4, 5, 29,
            15, 22, 9, 27
        },
        new[]
        {
            0, 3, 6, 21, 22, 17, 14, 18, 28, 4, 25, 29, 10, 1, 13, 7, 26, 27, 15, 9, 12, 2, 31, 16, 5, 24, 23, 11,
            19, 20, 8, 30
        },
        new[]
        {
            13, 17, 4, 14, 10, 15, 28, 18, 8, 5, 26, 24, 30, 0, 12, 19, 11, 7, 2, 29, 1, 3, 31, 20, 6, 25, 22, 21,
            27, 23, 16, 9
        },
        new[]
        {
            12, 3, 20, 19, 30, 16, 27, 26, 17, 7, 6, 23, 8, 14, 18, 0, 2, 25, 5, 21, 10, 31, 13, 1, 15, 24, 9, 29,
            11, 4, 22, 28
        },
        new[]
        {
            5, 21, 25, 12, 29, 6, 16, 26, 28, 27, 7, 4, 22, 30, 17, 15, 1, 8, 3, 18, 31, 0, 9, 24, 19, 10, 13, 2,
            14, 11, 23, 20
        }
    };

    [TestMethod]
    public void TestToStringGrasSau() => TestToString(Gras, Sau);

    [TestMethod]
    public void TestToStringHerzKoenigSau() => TestToString(Herz, Koenig);

    [TestMethod]
    public void TestToStringHerzOber() => TestToString(Herz, Ober);

    [TestMethod]
    public void TestToStringGrasUnter() => TestToString(Gras, Unter);

    [TestMethod]
    public void TestToStringSchelleZehn() => TestToString(Schelle, Zehn);

    [TestMethod]
    public void TestToStringSchelleNeun() => TestToString(Schelle, Neun);

    [TestMethod]
    public void TestToStringHerzAcht() => TestToString(Herz, Acht);

    [TestMethod]
    public void TestToStringEichelSieben() => TestToString(Eichel, Sieben);

    [TestMethod]
    public void TestFirstCardIsEichelSieben()
    {
        Assert.AreEqual(Eichel + " " + Sieben, GetCard(0).ToString());
    }

    [TestMethod]
    public void TestFirstCardIsSchellenOber()
    {
        Assert.AreEqual(Schelle + " " + Ober, GetCard(31).ToString());
    }

    [TestMethod]
    public void TestGetColor0IsEichel() => Assert.AreEqual(Eichel, GetColor(0));

    [TestMethod]
    public void TestGetColor1IsGras() => Assert.AreEqual(Gras, GetColor(1));

    [TestMethod]
    public void TestGetColor2IsHerz() => Assert.AreEqual(Herz, GetColor(2));

    [TestMethod]
    public void TestGetColor3IsSchelle() => Assert.AreEqual(Schelle, GetColor(3));

    [TestMethod]
    public void TestPoints7Schelle() => Assert.AreEqual(0, GetCard(Schelle, Sieben).Points);

    [TestMethod]
    public void TestPoints8Herz() => Assert.AreEqual(0, GetCard(Herz, Acht).Points);

    [TestMethod]
    public void TestPoints9Gras() => Assert.AreEqual(0, GetCard(Gras, Neun).Points);

    [TestMethod]
    public void TestPoints10Eichel() => Assert.AreEqual(10, GetCard(Eichel, Zehn).Points);

    [TestMethod]
    public void TestPointsUnterEichel() => Assert.AreEqual(2, GetCard(Eichel, Unter).Points);

    [TestMethod]
    public void TestPointsOberEichel() => Assert.AreEqual(3, GetCard(Eichel, Ober).Points);

    [TestMethod]
    public void TestPointsKoenigEichel() => Assert.AreEqual(4, GetCard(Eichel, Koenig).Points);

    [TestMethod]
    public void TestPointsSauEichel() => Assert.AreEqual(11, GetCard(Eichel, Sau).Points);

    [TestMethod]
    public void TestEveryIndexCorrect()
    {
        for (int i = 0; i < ALL_CARDS.Count; ++i)
            Assert.AreEqual(i, GetCard(i).Index);
    }

    [TestMethod]
    public void TestUnterOrNeunIsOber()
    {
        foreach (var color in COLOR_NAMES)
        {
            Assert.IsFalse(GetCard(color, Unter).IsOber());
            Assert.IsFalse(GetCard(color, Neun).IsOber());
        }
    }

    [TestMethod]
    public void TestOberIsOber()
    {
        foreach (var color in COLOR_NAMES)
            Assert.IsTrue(GetCard(color, Ober).IsOber());
    }

    [TestMethod]
    public void TestOberOrSiebenIsUnter()
    {
        foreach (var color in COLOR_NAMES)
        {
            Assert.IsFalse(GetCard(color, Ober).IsUnter());
            Assert.IsFalse(GetCard(color, Sieben).IsUnter());
        }
    }

    [TestMethod]
    public void TestUnterIsUnter()
    {
        foreach (var color in COLOR_NAMES)
            Assert.IsTrue(GetCard(color, Unter).IsUnter());
    }

    [TestMethod]
    public void TestKoenigOrZehnIsSau()
    {
        foreach (var color in COLOR_NAMES)
        {
            Assert.IsFalse(GetCard(color, Koenig).IsSau());
            Assert.IsFalse(GetCard(color, Zehn).IsSau());
        }
    }

    [TestMethod]
    public void TestSauIsSau()
    {
        foreach (var color in COLOR_NAMES)
            Assert.IsTrue(GetCard(color, Sau).IsSau());
    }

    [TestMethod]
    public void TestIsTrumpfOberUnterSauspiel()
    {
        SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Gras);
        foreach (var color in COLOR_NAMES)
        {
            Assert.IsTrue(GetCard(color, Ober).IsTrumpf(match));
            Assert.IsTrue(GetCard(color, Unter).IsTrumpf(match));
        }
    }

    [TestMethod]
    public void TestIsTrumpfHerzSauspiel()
    {
        SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Schelle);
        foreach (var number in NUMBER_NAMES)
            Assert.IsTrue(GetCard(Herz, number).IsTrumpf(match));
    }

    [TestMethod]
    public void TestIsNotTrumpfSauspiel()
    {
        SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Eichel);
        foreach (var number in NUMBER_NAMES.Where(number => !number.Equals(Ober) && !number.Equals(Unter)))
        {
            foreach (var color in SauspielColors)
            {
                Card card = GetCard(color, number);
                Assert.IsFalse(card.IsTrumpf(match), $"{card} should not be Trumpf for a {match}");
            }
        }
    }

    [TestMethod]
    public void TestIsTrumpfOberUnterSoloAndSoloTout()
    {
        foreach (var trumpf in COLOR_NAMES)
        {
            SchafkopfMatchConfig match1 = new SchafkopfMatchConfig(SchafkopfMode.Solo, trumpf);
            SchafkopfMatchConfig match2 = new SchafkopfMatchConfig(SchafkopfMode.SoloTout, trumpf);
            foreach (var color in COLOR_NAMES)
            {
                Assert.IsTrue(GetCard(color, Ober).IsTrumpf(match1));
                Assert.IsTrue(GetCard(color, Unter).IsTrumpf(match1));
                Assert.IsTrue(GetCard(color, Ober).IsTrumpf(match2));
                Assert.IsTrue(GetCard(color, Unter).IsTrumpf(match2));
            }
        }
    }

    [TestMethod]
    public void TestIsTrumpUnterWenzAndWenzTout()
    {
        foreach (var trumpf in COLOR_NAMES)
        {
            SchafkopfMatchConfig match1 = new SchafkopfMatchConfig(SchafkopfMode.Wenz, trumpf);
            SchafkopfMatchConfig match2 = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, trumpf);
            foreach (var color in COLOR_NAMES)
            {
                Assert.IsTrue(GetCard(color, Unter).IsTrumpf(match1));
                Assert.IsTrue(GetCard(color, Unter).IsTrumpf(match2));
            }
        }
    }

    [TestMethod]
    public void TestIsNotTrumpWenzAndWenzTout()
    {
        foreach (var trumpf in COLOR_NAMES)
        {
            SchafkopfMatchConfig match1 = new SchafkopfMatchConfig(SchafkopfMode.Wenz, trumpf);
            SchafkopfMatchConfig match2 = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, trumpf);
            foreach (var color in COLOR_NAMES)
            {
                foreach (var number in NUMBER_NAMES.Where(number => !number.Equals(Unter)))
                {
                    Assert.IsFalse(GetCard(color, number).IsTrumpf(match1));
                    Assert.IsFalse(GetCard(color, number).IsTrumpf(match2));
                }
            }
        }
    }

    [TestMethod]
    public void TestExceptionThrownForWeiterAtIsTrumpf()
    {
        foreach (var color in COLOR_NAMES)
        {
            foreach (Card card in ALL_CARDS)
            {
                try
                {
                    SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Weiter, color);
                    card.IsTrumpf(match);
                    Assert.Fail("Should throw exception for mode {0}", match);
                }
                catch
                {
                    // this is supposed to happen
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardNoValue()
    {
        TestGetValueOfThisCardNoValue(SauspielColors, SchafkopfMode.Sauspiel);
        TestGetValueOfThisCardNoValue(COLOR_NAMES, SchafkopfMode.Solo);
        TestGetValueOfThisCardNoValue(COLOR_NAMES, SchafkopfMode.SoloTout);
    }

    private static void TestGetValueOfThisCardNoValue(IReadOnlyCollection<CardColor> gameColors, SchafkopfMode mode)
    {
        foreach (var gameColor in gameColors)
        {
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(mode, gameColor);
            foreach (var semiTrumpf in COLOR_NAMES)
            {
                foreach (var number in NUMBER_NAMES.Where(s => !s.Equals(Ober) && !s.Equals(Unter)))
                {
                    foreach (var color in gameColors.Where(c => !c.Equals(semiTrumpf) && !c.Equals(gameColor)))
                    {
                        Assert.AreEqual(0, GetCard(color, number).GetValueOfThisCard(match, semiTrumpf));
                    }
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardOberUnter()
    {
        TestGetValueOfThisCardOberUnter(SauspielColors, SchafkopfMode.Sauspiel);
        TestGetValueOfThisCardOberUnter(COLOR_NAMES, SchafkopfMode.Solo);
        TestGetValueOfThisCardOberUnter(COLOR_NAMES, SchafkopfMode.SoloTout);
    }

    private static void TestGetValueOfThisCardOberUnter(IEnumerable<CardColor> gameColors, SchafkopfMode mode)
    {
        foreach (var gameColor in gameColors)
        {
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(mode, gameColor);
            foreach (var semiTrumpf in COLOR_NAMES)
            {
                for (var i = 0; i < COLOR_NAMES.Count; i++)
                {
                    Assert.AreEqual(20 - i, GetCard(COLOR_NAMES[i], Ober).GetValueOfThisCard(match, semiTrumpf));
                    Assert.AreEqual(16 - i, GetCard(COLOR_NAMES[i], Unter).GetValueOfThisCard(match, semiTrumpf));
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardSemiTrumpf()
    {
        TestGetValueOfThisCardSemiTrumpf(SauspielColors, SchafkopfMode.Sauspiel);
        TestGetValueOfThisCardSemiTrumpf(COLOR_NAMES, SchafkopfMode.Solo);
        TestGetValueOfThisCardSemiTrumpf(COLOR_NAMES, SchafkopfMode.SoloTout);
    }

    private static void TestGetValueOfThisCardSemiTrumpf(IEnumerable<CardColor> gameColors, SchafkopfMode mode)
    {
        foreach (var gameColor in gameColors)
        {
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(mode, gameColor);
            foreach (var semiTrumpf in COLOR_NAMES.Where(s => !s.Equals(match.Trumpf)))
            {
                for (int i = 0; i < NumbersShort.Count; ++i)
                {
                    Card card = GetCard(semiTrumpf, NumbersShort[i]);
                    int actual = card.GetValueOfThisCard(match, semiTrumpf);
                    const string message = "For {0} in a {1} with first color {2}";
                    Assert.AreEqual(1 + i, actual, message, card, match, semiTrumpf);
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardTrumpf()
    {
        TestGetValueOfThisCardTrumpf(SauspielColors, SchafkopfMode.Sauspiel);
        TestGetValueOfThisCardTrumpf(COLOR_NAMES, SchafkopfMode.Solo);
        TestGetValueOfThisCardTrumpf(COLOR_NAMES, SchafkopfMode.SoloTout);
    }

    private static void TestGetValueOfThisCardTrumpf(IEnumerable<CardColor> gameColors, SchafkopfMode mode)
    {
        foreach (var gameColor in gameColors)
        {
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(mode, gameColor);
            Assert.IsTrue(match.Trumpf != null, "match.Trumpf != null");
            CardColor trumpf = (CardColor)match.Trumpf;
            foreach (var semiTrumpf in COLOR_NAMES.Where(s => !s.Equals(match.Trumpf)))
            {
                for (int i = 0; i < NumbersShort.Count; ++i)
                {
                    Card card = GetCard(trumpf, NumbersShort[i]);
                    int actual = card.GetValueOfThisCard(match, semiTrumpf);
                    const string message = "For {0} in a {1} with first color {2}";
                    Assert.AreEqual(7 + i, actual, message, card, match, semiTrumpf);
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardWenzNoValue()
    {
        foreach (var color in COLOR_NAMES)
        {
            const string message = "{0} should have other value for {1} with first color {2}";
            SchafkopfMatchConfig match1 = new SchafkopfMatchConfig(SchafkopfMode.Wenz, color);
            SchafkopfMatchConfig match2 = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, color);
            foreach (var semiTrumpf in COLOR_NAMES)
            {
                foreach (var number in WenzNumbers)
                {
                    foreach (var cardColor in COLOR_NAMES.Where(c => !c.Equals(semiTrumpf)))
                    {
                        Card card = GetCard(cardColor, number);
                        Assert.AreEqual(0, card.GetValueOfThisCard(match1, semiTrumpf), message, card, match1,
                            semiTrumpf);
                        Assert.AreEqual(0, card.GetValueOfThisCard(match2, semiTrumpf), message, card, match1,
                            semiTrumpf);
                    }
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardWenzSemiTrumpf()
    {
        foreach (var color in COLOR_NAMES)
        {
            const string message = "{0} should have other value for {1} with first color {2}";
            SchafkopfMatchConfig match1 = new SchafkopfMatchConfig(SchafkopfMode.Wenz, color);
            SchafkopfMatchConfig match2 = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, color);
            foreach (var semiTrumpf in COLOR_NAMES)
            {
                for (int i = 0; i < WenzNumbers.Count; ++i)
                {
                    Card card = GetCard(semiTrumpf, WenzNumbers[i]);
                    int actual1 = card.GetValueOfThisCard(match1, semiTrumpf);
                    int actual2 = card.GetValueOfThisCard(match2, semiTrumpf);
                    Assert.AreEqual(1 + i, actual1, message, card, match1, semiTrumpf);
                    Assert.AreEqual(1 + i, actual2, message, card, match1, semiTrumpf);
                }
            }
        }
    }

    [TestMethod]
    public void TestGetValueOfThisCardWenzUnter()
    {
        foreach (var gameColor in COLOR_NAMES)
        {
            const string message = "{0} should have other value for {1} with first color {2}";
            SchafkopfMatchConfig match1 = new SchafkopfMatchConfig(SchafkopfMode.Wenz, gameColor);
            SchafkopfMatchConfig match2 = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, gameColor);
            foreach (var semiTrumpf in COLOR_NAMES)
            {
                for (int i = 0; i < COLOR_NAMES.Count; i++)
                {
                    var color = COLOR_NAMES[i];
                    Card card = GetCard(color, Unter);
                    int actual1 = card.GetValueOfThisCard(match1, semiTrumpf);
                    int actual2 = card.GetValueOfThisCard(match2, semiTrumpf);
                    Assert.AreEqual(12 - i, actual1, message, card, match1, semiTrumpf);
                    Assert.AreEqual(12 - i, actual2, message, card, match1, semiTrumpf);
                }
            }
        }
    }

    [TestMethod]
    public void TestExceptionThrownForWeiterAtGetValueForCard()
    {
        foreach (var color in COLOR_NAMES)
        {
            foreach (var semiTrumpf in COLOR_NAMES)
            {
                foreach (Card card in ALL_CARDS)
                {
                    try
                    {
                        SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Weiter, color);
                        card.GetValueOfThisCard(match, semiTrumpf);
                        Assert.Fail("Should throw exception for mode {0}", match);
                    }
                    catch
                    {
                        // this is supposed to happen
                    }
                }
            }
        }
    }

    [TestMethod]
    public void TestSortEichelSauspielHerzSoloOrSoloTout()
    {
        int[] expected =
        [
            7, 15, 23, 31, 6, 14, 22, 30, 21, 20, 19, 18, 17, 16, 5, 4, 3, 2, 1, 0, 13, 12, 11, 10, 9, 8, 29, 28,
            27, 26, 25, 24
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Eichel));
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Solo, Herz));
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.SoloTout, Herz));
    }

    [TestMethod]
    public void TestSortGrasSauspiel()
    {
        int[] expected =
        [
            7, 15, 23, 31, 6, 14, 22, 30, 21, 20, 19, 18, 17, 16, 13, 12, 11, 10, 9, 8, 5, 4, 3, 2, 1, 0, 29, 28,
            27, 26, 25, 24
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Gras));
    }

    [TestMethod]
    public void TestSortSchelleSauspiel()
    {
        int[] expected =
        [
            7, 15, 23, 31, 6, 14, 22, 30, 21, 20, 19, 18, 17, 16, 29, 28, 27, 26, 25, 24, 5, 4, 3, 2, 1, 0, 13, 12,
            11, 10, 9, 8
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Schelle));
    }

    [TestMethod]
    public void TestSortSoloEichel()
    {
        int[] expected =
        [
            7, 15, 23, 31, 6, 14, 22, 30, 5, 4, 3, 2, 1, 0, 13, 12, 11, 10, 9, 8, 21, 20, 19, 18, 17, 16, 29, 28,
            27, 26, 25, 24
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Solo, Eichel));
    }

    [TestMethod]
    public void TestSortSoloGras()
    {
        int[] expected =
        [
            7, 15, 23, 31, 6, 14, 22, 30, 13, 12, 11, 10, 9, 8, 5, 4, 3, 2, 1, 0, 21, 20, 19, 18, 17, 16, 29, 28,
            27, 26, 25, 24
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Solo, Gras));
    }

    [TestMethod]
    public void TestSortSoloSchelle()
    {
        int[] expected =
        [
            7, 15, 23, 31, 6, 14, 22, 30, 29, 28, 27, 26, 25, 24, 5, 4, 3, 2, 1, 0, 13, 12, 11, 10, 9, 8, 21, 20,
            19, 18, 17, 16
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Solo, Schelle));
    }

    [TestMethod]
    public void TestSortWenzAndWenzTout()
    {
        int[] expected =
        [
            6, 14, 22, 30, 5, 4, 3, 7, 2, 1, 0, 13, 12, 11, 15, 10, 9, 8, 21, 20, 19, 23, 18, 17, 16, 29, 28, 27,
            31, 26, 25, 24
        ];
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.Wenz, (CardColor?)null));
        AssertSortsCardsCorrect(expected, new SchafkopfMatchConfig(SchafkopfMode.WenzTout, (CardColor?)null));
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestCannotSortCardsForWeiter()
    {
        SortCards(new SchafkopfMatchConfig(SchafkopfMode.Weiter, Herz), ALL_CARDS.ToList());
    }

    private static List<Card> CardIndexToList(IReadOnlyCollection<int> cards)
    {
        List<Card> ret = new List<Card>(cards.Count);
        for (int i = 0; i < cards.Count; ++i) ret.Add(GetCard(i));
        return ret;
    }

    private static void AssertSortsCardsCorrect(IReadOnlyList<int> expected, SchafkopfMatchConfig match)
    {
        foreach (var cards in ShuffledCards)
        {
            List<Card> actual = CardIndexToList(cards);
            SortCards(match, actual);
            AssertCardsEqual(expected, actual);
        }
    }

    private static void AssertCardsEqual(IReadOnlyList<int> expected, IReadOnlyList<Card> actual)
    {
        bool equal = expected.Count == actual.Count && !expected.Where((t, i) => t != actual[i].Index).Any();
        Assert.IsTrue(equal, "\nExpected: {0}\nactual:   {1}\nExpected: {2}\nactual:   {3}",
            ArrayString(i => GetCard(expected[i]).ToString(), expected.Count), ArrayString(actual),
            ArrayString(expected), ArrayString(i => actual[i].Index.ToString(), actual.Count));
    }
}