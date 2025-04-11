using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung;
using SpieleSammlung.Model.Schafkopf;
using static SpieleSammlung.Model.Util.ArrayPrinter;

namespace SpieleSammlungTests.Model.Schafkopf
{
    [TestClass]
    public class SchafkopfPlayerTest
    {
        private static readonly SchafkopfPlayer Player = new("id", "name");

        private static readonly IReadOnlyList<string> SauspielColors = [Card.EICHEL, Card.GRAS, Card.SCHELLE];

        private static readonly IReadOnlyList<bool> AllEightTrue = new List<bool>
            { true, true, true, true, true, true, true, true };

        private static readonly IReadOnlyList<string> NoColors = new List<string> { "" };

        [TestMethod]
        public void TestConstructor()
        {
            const string id = "abd cfe";
            const string name = "thisIsAName";
            SchafkopfPlayer p = new SchafkopfPlayer(id, name);
            Assert.AreEqual(id, p.Id);
            Assert.AreEqual(name, p.Name);
        }

        [TestMethod]
        public void TestCardsSortedCorrectlyAufEichel()
        {
            SetCards(4, 16, 31, 1, 23, 7, 25, 26);
            Player.SortCards(new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.EICHEL));
            AssertCardsEqual([7, 23, 31, 16, 4, 1, 26, 25], Player.PlayableCards);
        }

        [TestMethod]
        public void TestCardsSortedCorrectlyAufGras()
        {
            SetCards(16, 2, 11, 19, 23, 30, 28, 9);
            Player.SortCards(new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.GRAS));
            AssertCardsEqual([23, 30, 19, 16, 11, 9, 2, 28], Player.PlayableCards);
        }

        [TestMethod]
        public void TestCardsSortedCorrectlyAufSchelle()
        {
            SetCards(12, 11, 16, 3, 7, 29, 28, 9);
            Player.SortCards(new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.SCHELLE));
            AssertCardsEqual([7, 16, 29, 28, 3, 12, 11, 9], Player.PlayableCards);
        }

        [TestMethod]
        public void TestCardsSortedCorrectlyGrasSolo()
        {
            SetCards(6, 31, 7, 14, 15, 30, 23, 22);
            Player.SortCards(new SchafkopfMatchConfig(SchafkopfMode.Solo, Card.GRAS));
            AssertCardsEqual([7, 15, 23, 31, 6, 14, 22, 30], Player.PlayableCards);
        }

        [TestMethod]
        public void TestCardsSortedCorrectlyWenz()
        {
            SetCards(6, 21, 14, 13, 30, 22, 5, 29);
            Player.SortCards(new SchafkopfMatchConfig(SchafkopfMode.Solo, ""));
            AssertCardsEqual([6, 14, 22, 30, 5, 13, 21, 29], Player.PlayableCards);
        }

        [TestMethod]
        public void TestOpportunitiesCalculatedCorrectlyOnlyGrasSoloAndWenz()
        {
            SetCards(15, 23, 31, 13, 11, 10, 9, 8);
            Player.UpdatePossibilities(SchafkopfMode.Weiter);
            AssertPossibilityCountCorrect(3);
            AssertPossibilityIsWenz(1);
            AssertPossibilityCorrect(SchafkopfMode.Solo, [Card.GRAS], 2);
        }

        [TestMethod]
        public void TestOpportunitiesCalculatedCorrectlyOnlyHerzEichelSoloToutAndWenz()
        {
            SetCards(7, 15, 23, 31, 5, 4, 3, 17);
            Player.UpdatePossibilities(SchafkopfMode.Weiter);
            AssertPossibilityCountCorrect(4);
            AssertPossibilityIsWenz(1);
            string[] colors = [Card.EICHEL, Card.HERZ];
            AssertPossibilityCorrect(SchafkopfMode.Solo, colors, 2);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, colors, 3);
        }

        [TestMethod]
        public void TestOpportunitiesCalculatedCorrectlyOnlyHerzSchelleGrasSoloToutAndWenzTout()
        {
            SetCards(7, 15, 23, 31, 6, 13, 21, 29);
            Player.UpdatePossibilities(SchafkopfMode.Weiter);
            AssertPossibilityCountCorrect(5);
            AssertPossibilityIsWenz(1);
            string[] colors = [Card.GRAS, Card.HERZ, Card.SCHELLE];
            AssertPossibilityCorrect(SchafkopfMode.Solo, colors, 2);
            AssertPossibilityIsWenzTout(3);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, colors, 4);
        }

        [TestMethod]
        public void TestOpportunitiesCalculatedCorrectlyEverything()
        {
            SetCards(7, 15, 23, 6, 0, 8, 16, 24);
            Player.UpdatePossibilities(SchafkopfMode.Weiter);
            AssertPossibilityCountCorrect(6);
            AssertPossibilityCorrect(SchafkopfMode.Sauspiel, SauspielColors, 1);
            AssertPossibilityIsWenz(2);
            AssertPossibilityCorrect(SchafkopfMode.Solo, Card.COLOR_NAMES, 3);
            AssertPossibilityIsWenzTout(4);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, Card.COLOR_NAMES, 5);
        }

        [TestMethod]
        public void TestOpportunitiesBestCards()
        {
            SetCards(7, 15, 23, 31, 6, 14, 22, 30);
            Player.UpdatePossibilities(SchafkopfMode.Weiter);
            AssertPossibilityCountCorrect(5);
            AssertPossibilityIsWenz(1);
            AssertPossibilityCorrect(SchafkopfMode.Solo, Card.COLOR_NAMES, 2);
            AssertPossibilityIsWenzTout(3);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, Card.COLOR_NAMES, 4);
        }

        [TestMethod]
        public void TestCanOnlyPlayWeiter()
        {
            SetCards(7, 15, 23, 6, 0, 8, 16, 24);
            Player.UpdatePossibilities(SchafkopfMode.SoloTout);
            AssertPossibilityCountCorrect(1);
        }

        [TestMethod]
        public void TestCanOnlyPlaySoloTout()
        {
            SetCards(7, 15, 23, 6, 0, 8, 16, 24);
            Player.UpdatePossibilities(SchafkopfMode.WenzTout);
            AssertPossibilityCountCorrect(2);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, Card.COLOR_NAMES, 1);
        }

        [TestMethod]
        public void TestCanOnlyPlayTout()
        {
            SetCards(7, 15, 23, 6, 0, 8, 16, 24);
            Player.UpdatePossibilities(SchafkopfMode.Solo);
            AssertPossibilityCountCorrect(3);
            AssertPossibilityIsWenzTout(1);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, Card.COLOR_NAMES, 2);
        }

        [TestMethod]
        public void TestCanOnlyPlaySoloOrHigher()
        {
            SetCards(7, 15, 23, 6, 0, 8, 16, 24);
            Player.UpdatePossibilities(SchafkopfMode.Wenz);
            AssertPossibilityCountCorrect(4);
            AssertPossibilityCorrect(SchafkopfMode.Solo, Card.COLOR_NAMES, 1);
            AssertPossibilityIsWenzTout(2);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, Card.COLOR_NAMES, 3);
        }

        [TestMethod]
        public void TestCanOnlyPlayWenzOrHigher()
        {
            SetCards(7, 15, 23, 6, 0, 8, 16, 24);
            Player.UpdatePossibilities(SchafkopfMode.Sauspiel);
            AssertPossibilityCountCorrect(5);
            AssertPossibilityIsWenz(1);
            AssertPossibilityCorrect(SchafkopfMode.Solo, Card.COLOR_NAMES, 2);
            AssertPossibilityIsWenzTout(3);
            AssertPossibilityCorrect(SchafkopfMode.SoloTout, Card.COLOR_NAMES, 4);
        }

        [TestMethod]
        public void TestHasGesuchteNoSauspiel()
        {
            const string message = "The mode of this match is not Sauspiel, so the player can't have the \"Gesuchte\".";
            SetCards(5, 4, 13, 12, 21, 20, 29, 28);
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Solo, Card.HERZ);
            Assert.IsFalse(Player.HasGesuchte(match), message);
            match = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, "");
            Assert.IsFalse(Player.HasGesuchte(match), message);
        }

        [TestMethod]
        public void TestHasNotGesuchteSauspielButColor()
        {
            const string message = "The player has all \"Sau's\" but not the required one (Cards: {0}).";
            SetCards(0, 4, 13, 12, 21, 20, 29, 28);
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.EICHEL);
            Assert.IsFalse(Player.HasGesuchte(match), message, ArrayString(Player.PlayableCards));
        }

        [TestMethod]
        public void TestHasNotGesuchteSauspiel()
        {
            const string message = "The player has all \"Sau's\" but not the required one (Cards: {0}).";
            SetCards(7, 6, 13, 12, 21, 20, 29, 28);
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.EICHEL);
            Assert.IsFalse(Player.HasGesuchte(match), message, ArrayString(Player.PlayableCards));
        }

        [TestMethod]
        public void TestHasGesuchteSauspiel()
        {
            const string message = "The player has the \"Gesuchte\" (Cards: {0})";
            SetCards(7, 4, 13, 12, 21, 20, 29, 28);
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.GRAS);
            Assert.IsTrue(Player.HasGesuchte(match), message, ArrayString(Player.PlayableCards));
        }

        [TestMethod]
        public void TestCheckPlayableCardsHasRightLength()
        {
            SetCards(0, 1, 2, 3, 4, 5, 6, 7);
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.GRAS);
            IReadOnlyList<bool> result;
            while (Player.PlayableCards.Count > 0)
            {
                result = Player.CheckPlayableCards(match, null, false);
                Assert.AreEqual(Player.PlayableCards.Count, result.Count);
                Player.PlayableCards.RemoveAt(0);
            }

            result = Player.CheckPlayableCards(match, null, false);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TestCanRunAwayLastCardRightColor()
        {
            SetCards(5);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, Card.ALL_CARDS[0], false, true);
        }

        [TestMethod]
        public void TestCanRunAwayLastCardWrongColor()
        {
            SetCards(5);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, Card.ALL_CARDS[8], false, true);
        }

        [TestMethod]
        public void TestCanRunAwayLastCardFirstCardNotWegGelaufen()
        {
            SetCards(5);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, null, false, true);
        }

        [TestMethod]
        public void TestCanRunAwayLastCardFirstCardWegGelaufen()
        {
            SetCards(5);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, null, true, true);
        }

        [TestMethod]
        public void TestCanRunAwayPlaysFirstCard()
        {
            SetCards(7, 15, 6, 5, 4, 3, 2, 11);
            AssertCheckPlayableCardsCorrect(SchafkopfMode.Sauspiel, Card.EICHEL, null, false, AllEightTrue);
        }

        [TestMethod]
        public void TestCanRunAwayFirstCardNotEichel()
        {
            SetCards(7, 15, 6, 5, 4, 3, 2, 13);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, Card.ALL_CARDS[12], false,
                false, false, false, false, false, false, false, true);
        }

        [TestMethod]
        public void TestCanRunAwayFirstCardEichel()
        {
            SetCards(7, 15, 6, 5, 4, 3, 2, 11);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, Card.ALL_CARDS[0], false,
                false, false, false, true, false, false, false, false);
        }

        [TestMethod]
        public void TestCanPlayAnyCardAsFirstBecauseNoGesuchte()
        {
            SetCards(7, 15, 6, 5, 4, 3, 2, 11);
            AssertCheckPlayableCardsCorrect(SchafkopfMode.Sauspiel, Card.SCHELLE, null, false, AllEightTrue);
        }

        [TestMethod]
        public void TestHasRunAwayCanPlayOtherCardsThanSau()
        {
            SetCards(7, 15, 6, 5, 4, 11);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, Card.ALL_CARDS[0], true,
                false, false, false, true, true, false);
        }

        [TestMethod]
        public void TestHasRunAwayCanPlaySauAfterWeglaufen()
        {
            SetCards(5, 4, 11);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, Card.ALL_CARDS[7], true,
                true, true, true);
        }

        [TestMethod]
        public void TestCanPlayAnyCardAsFirstCardForWenz()
        {
            SetCards(4, 5, 1, 31, 23, 12, 19);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Sauspiel, Card.EICHEL, null, false,
                true, true, true, true, true, true, true);
        }

        [TestMethod]
        public void TestMustPlayUnterAtWenz()
        {
            SetCards(14, 5, 4, 3, 31);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.Wenz, "", Card.ALL_CARDS[6], false,
                true, false, false, false, false);
        }

        [TestMethod]
        public void TestSoloToutMustPlayTrumpf()
        {
            SetCards(31, 30, 29, 24, 5, 4, 21, 20);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.SoloTout, Card.SCHELLE, Card.ALL_CARDS[7], false,
                true, true, true, true, false, false, false, false);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.SoloTout, Card.SCHELLE, Card.ALL_CARDS[25], false,
                true, true, true, true, false, false, false, false);
        }

        [TestMethod]
        public void TestSoloToutMustPlayColor()
        {
            SetCards(31, 30, 5, 4, 29, 24, 21, 20);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.SoloTout, Card.EICHEL, Card.ALL_CARDS[21], false,
                false, false, false, false, false, false, true, true);
            AssertCheckPlayableCardsCorrectArgs(SchafkopfMode.SoloTout, Card.EICHEL, Card.ALL_CARDS[24], false,
                false, false, false, false, true, true, false, false);
        }

        private static void AssertPossibilityIsWenz(int index)
        {
            AssertPossibilityCorrect(SchafkopfMode.Wenz, NoColors, index);
        }

        private static void AssertPossibilityIsWenzTout(int index)
        {
            AssertPossibilityCorrect(SchafkopfMode.WenzTout, NoColors, index);
        }

        private static void AssertPossibilityCorrect(SchafkopfMode mode, IReadOnlyList<string> colors, int index)
        {
            SchafkopfMatchPossibility possibility = Player.Possibilities[index];
            Assert.AreEqual(mode, possibility.Mode);
            Assert.AreEqual(colors.Count, possibility.Colors.Count,
                "Incorrect number of colors.\nCards: {0}\nActual possibility: {1}",
                ArrayString(Player.PlayableCards), possibility);
            for (int i = 0; i < colors.Count; ++i)
            {
                Assert.AreEqual(colors[i], possibility.Colors[i],
                    "Incorrect color at index {0}.\nCards: {1}\nActual possibility: {2}", index,
                    ArrayString(Player.PlayableCards), possibility);
            }
        }

        private static void AssertPossibilityCountCorrect(int expected)
        {
            Assert.AreEqual(expected, Player.Possibilities.Count,
                "Incorrect number of possibilities.\nCards: {0}\nActual possibilities:\n{1}",
                ArrayString(Player.PlayableCards), ArrayString(Player.Possibilities, "\n"));
            AssertPossibilityCorrect(SchafkopfMode.Weiter, NoColors, 0);
        }

        private static void SetCards(params int[] cards)
        {
            Player.PlayableCards.Clear();
            foreach (int card in cards) Player.PlayableCards.Add(Card.ALL_CARDS[card]);
        }

        private static void AssertCardsEqual(IReadOnlyList<int> expected, IReadOnlyList<Card> actual)
        {
            bool equal = expected.Count == actual.Count && !expected.Where((t, i) => t != actual[i].Index).Any();
            Assert.IsTrue(equal, "\nExpected: {0}\nactual:   {1}\nExpected: {2}\nactual:   {3}",
                ArrayString(i => Card.ALL_CARDS[expected[i]].ToString(), expected.Count), ArrayString(actual),
                ArrayString(expected), ArrayString(i => actual[i].Index.ToString(), actual.Count));
        }

        private static void AssertCheckPlayableCardsCorrectArgs(SchafkopfMode mode, string color, Card firstCard,
            bool isWegGelaufen, params bool[] expected)
        {
            AssertCheckPlayableCardsCorrect(mode, color, firstCard, isWegGelaufen, expected);
        }

        private static void AssertCheckPlayableCardsCorrect(SchafkopfMode mode, string color, Card firstCard,
            bool isWegGelaufen, IReadOnlyList<bool> expected)
        {
            SchafkopfMatchConfig match = new SchafkopfMatchConfig(mode, color);
            IReadOnlyList<bool> playableCards = Player.CheckPlayableCards(match, firstCard, isWegGelaufen);
            Assert.AreEqual(expected.Count, playableCards.Count, "The player has {0} cards but only has {1} options.",
                expected.Count, playableCards.Count);
            for (int i = 0; i < playableCards.Count; ++i)
            {
                Assert.AreEqual(expected[i], playableCards[i],
                    "Card {0} should {1}be playable for a {2} {3} if the player has the cards: {4}.",
                    Player.PlayableCards[i], expected[i] ? "" : "not ", match,
                    firstCard == null ? "as the first card of the round" : $"with {firstCard} as the first card",
                    Player.PlayableCards);
            }
        }
    }
}