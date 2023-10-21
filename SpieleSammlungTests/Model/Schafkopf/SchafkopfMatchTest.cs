using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung;
using SpieleSammlung.Model.Multiplayer;
using SpieleSammlung.Model.Schafkopf;
using SpieleSammlung.Model.Util;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Schafkopf
{
    [TestClass]
    public class SchafkopfMatchTest
    {
        private static readonly List<MultiplayerPlayer> Players = new()
        {
            new MultiplayerPlayer("1", "name1"),
            new MultiplayerPlayer("2", "name2"),
            new MultiplayerPlayer("3", "name3"),
            new MultiplayerPlayer("4", "name4"),
            new MultiplayerPlayer("5", "name5")
        };

        private static readonly IReadOnlyList<int> AllZero = ArrayHelp.CreateIntArray(Card.ALL_CARDS.Count, 0);

        [TestMethod]
        public void TestRoundGrasSoloTout()
        {
            SchafkopfMatch match = new SchafkopfMatch(Players, new Random(42), true);
            match.ShuffleCards();
            match.CurrentPlayers[0].Aufgestellt = true;
            match.ChoseGameMode(SchafkopfMode.SoloTout, Card.GRAS);
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            AssertGameStopsWhenAlwaysPlayingFirstCard(match);
            const string points = "\n\n+50 SoloTout\n*2 tout\n*2 aufgestellt\n_________________\n200";
            const string expectedPlayer1 = "Du hast das SoloTout mit 120 Augen gewonnen" + points;
            const string expectedPlayerLosers = "Du hast das SoloTout mit {0} und {1} mit 0 Augen verloren" + points;
            const string expectedOtherPlayers = "name1 hat das SoloTout mit 120 Augen gewonnen" + points;
            Assert.AreEqual(expectedPlayer1, match.FullSummary(0));
            Assert.AreEqual(string.Format(expectedPlayerLosers, Players[2].Name, Players[3].Name),
                match.FullSummary(1));
            Assert.AreEqual(string.Format(expectedPlayerLosers, Players[1].Name, Players[3].Name),
                match.FullSummary(2));
            Assert.AreEqual(string.Format(expectedPlayerLosers, Players[1].Name, Players[2].Name),
                match.FullSummary(3));
            Assert.AreEqual(expectedOtherPlayers, match.FullSummary(4));
        }
        [TestMethod]
        public void TestRoundGrasSoloToutLoose()
        {
            SchafkopfMatch match = new SchafkopfMatch(Players, new Random(98), true);
            match.ShuffleCards();
            match.CurrentPlayers[3].Aufgestellt = true;
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.SoloTout, Card.HERZ);
            List<int> clone = AllZero.ToList();
            clone[23] = 1;
            clone[27] = 1;
            AssertGameStopsWithTheseCards(match, clone);
            AssertMessagesCorrect(match,
                "\n\n+50 SoloTout\n*2 tout\n*2 aufgestellt\n_________________\n200",
                "Du hast das SoloTout mit name2 und name3 mit 0 Augen und 1 Stich gewonnen",
                "Du hast das SoloTout mit name1 und name3 mit 0 Augen und 1 Stich gewonnen",
                "Du hast das SoloTout mit name1 und name2 mit 0 Augen und 1 Stich gewonnen",
                "Du hast das SoloTout mit 120 Augen und 1 Gegenstich verloren",
                "name4 hat das SoloTout mit 120 Augen und 1 Gegenstich verloren");
        }

        [TestMethod]
        public void TestRoundAufSchelle()
        {
            SchafkopfMatch match = new SchafkopfMatch(Players, new Random(13), true);
            match.ShuffleCards();
            match.CurrentPlayers[3].Aufgestellt = true;
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Sauspiel, Card.SCHELLE);
            List<string> cards = new List<string>
            {
                "Eichel Neun", "Eichel Acht", "Eichel Zehn", "Eichel Sau", //player 0 (21)
                "Gras Ober", "Herz Sieben", "Herz Zehn", "Herz Unter", // player 3 (15)
                "Herz Neun", "Schelle Unter", "Eichel Unter", "Schelle Acht", // player 3 (4)
                "Eichel Ober", "Schelle Neun", "Herz Ober", "Herz Acht", // player 1 (6)
                "Herz Koenig", "Gras Neun", "Schelle Ober", "Gras Unter", // player 1 (9)
                "Gras Sau", "Gras Zehn", "Schelle Zehn", "Gras Sieben", // player 3 (31)
                "Schelle Sieben", "Herz Sau", "Schelle Sau", "Schelle Koenig", // player 3 (26)
                "Gras Koenig", "Eichel Sieben", "Eichel Koenig", "Gras Acht" // player 0 -> player 0 (8)
            };
            AssertGameStopsWithTheseCards(match, cards);
            const string points = "\n\n+20 Sauspiel\n+50 Laufende\n*2 aufgestellt\n_________________\n140";
            const string expectedPlayer0 = "Du hast das Sauspiel mit name3 mit 34 Augen verloren" + points;
            const string expectedPlayer1 = "Du hast das Sauspiel mit name4 mit 86 Augen gewonnen" + points;
            const string expectedPlayer2 = "Du hast das Sauspiel mit name1 mit 34 Augen verloren" + points;
            const string expectedPlayer3 = "Du hast das Sauspiel mit name2 mit 86 Augen gewonnen" + points;
            const string expectedOtherPlayers = "name4 hat das Sauspiel mit name2 mit 86 Augen gewonnen" + points;
            Assert.AreEqual(expectedPlayer0, match.FullSummary(0));
            Assert.AreEqual(expectedPlayer1, match.FullSummary(1));
            Assert.AreEqual(expectedPlayer2, match.FullSummary(2));
            Assert.AreEqual(expectedPlayer3, match.FullSummary(3));
            Assert.AreEqual(expectedOtherPlayers, match.FullSummary(4));
        }

        [TestMethod]
        public void TestRoundWenz()
        {
            SchafkopfMatch match = new SchafkopfMatch(Players, new Random(5), true);
            match.ShuffleCards();
            match.CurrentPlayers[2].Aufgestellt = true;
            match.CurrentPlayers[3].Aufgestellt = true;
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Weiter, "");
            match.ChoseGameMode(SchafkopfMode.Wenz, "");
            List<string> cards = new List<string>
            {
                "Gras Neun", "Gras Acht", "Gras Unter", "Gras Zehn", // winner: 2, points: 12 (start at 0)
                "Schelle Acht", "Schelle Sieben", "Schelle Zehn", "Eichel Sau", // winner: 0, points: 21
                "Herz Neun", "Herz Ober", "Herz Sieben", "Herz Sau", // winner: 3, points: 14
                "Eichel Unter", "Eichel Acht", "Schelle Unter", "Herz Acht", // winner: 3, points: 4
                "Herz Unter", "Gras Ober", "Gras Sieben", "Schelle Neun", // winner: 3, points: 
                "Gras Sau", "Gras Koenig", "Eichel Sieben", "Schelle Ober", // winner: 3, points: 
                "Herz Zehn", "Herz Koenig", "Eichel Neun", "Eichel Ober", // winner: 3, points: 
                "Schelle Sau", "Schelle Koenig", "Eichel Zehn", "Eichel Koenig", // winner: 3, points: 
            };
            AssertGameStopsWithTheseCards(match, cards);
            const string points = "\n\n+50 Wenz\n*2 aufgestellt\n*2 aufgestellt\n_________________\n200";
            const string expectedPlayer0 = "Du hast den Wenz mit name2 und name3 mit 33 Augen verloren" + points;
            const string expectedPlayer1 = "Du hast den Wenz mit name1 und name3 mit 33 Augen verloren" + points;
            const string expectedPlayer2 = "Du hast den Wenz mit name1 und name2 mit 33 Augen verloren" + points;
            const string expectedPlayer3 = "Du hast den Wenz mit 87 Augen gewonnen" + points;
            const string expectedOtherPlayers = "name4 hat den Wenz mit 87 Augen gewonnen" + points;
            Assert.AreEqual(expectedPlayer0, match.FullSummary(0));
            Assert.AreEqual(expectedPlayer1, match.FullSummary(1));
            Assert.AreEqual(expectedPlayer2, match.FullSummary(2));
            Assert.AreEqual(expectedPlayer3, match.FullSummary(3));
            Assert.AreEqual(expectedOtherPlayers, match.FullSummary(4));
        }

        [TestMethod]
        public void TestRoundWenzLoose()
        {
            SchafkopfMatch match = new SchafkopfMatch(Players, new Random(89), true);
            match.ShuffleCards();
            match.CurrentPlayers[0].Aufgestellt = true;
            match.ChoseGameMode(SchafkopfMode.Wenz, "", 0);
            match.ChoseGameMode(SchafkopfMode.Weiter, "", 1);
            match.ChoseGameMode(SchafkopfMode.Weiter, "", 2);
            match.ChoseGameMode(SchafkopfMode.Weiter, "", 3);
            match.CurrentPlayers[3].Kontra = true;
            List<string> cards = new List<string>
            {
                "Herz Neun", "Herz Sau", "Gras Sau", "Herz Ober", // winner: 1, points: 25 (start at 0)
                "Schelle Sau", "Schelle Ober", "Schelle Zehn", "Gras Koenig", // winner: 1, points: 28
                "Gras Neun", "Gras Zehn", "Gras Ober", "Schelle Unter", // winner: 0, points: 15
                "Eichel Sau", "Eichel Koenig", "Eichel Sieben", "Eichel Neun", // winner: 0, points: 15
                "Eichel Ober", "Eichel Zehn", "Eichel Acht", "Schelle Koenig", // winner: 1, points: 17
                "Herz Zehn", "Gras Sieben", "Herz Acht", "Eichel Unter", // winner: 0, points: 12
                "Gras Unter", "Herz Koenig", "Schelle Acht", "Gras Acht", // winner: 0, points: 6
                "Herz Unter", "Schelle Neun", "Schelle Sieben", "Herz Sieben", // winner: 0, points: 2
            };
            AssertGameStopsWithTheseCards(match, cards);
            AssertMessagesCorrect(match,
                "\n\n+50 Wenz\n+40 Laufende\n*2 aufgestellt\n*2 Kontra\n_________________\n360",
                "Du hast den Wenz mit 50 Augen verloren",
                "Du hast den Wenz mit name3 und name4 mit 70 Augen gewonnen",
                "Du hast den Wenz mit name2 und name4 mit 70 Augen gewonnen",
                "Du hast den Wenz mit name2 und name3 mit 70 Augen gewonnen",
                "name1 hat den Wenz mit 50 Augen verloren");
        }

        private static void AssertMessagesCorrect(SchafkopfMatch match, string points, string player0, string player1,
            string player2, string player3, string others)
        {
            Assert.AreEqual(player0 + points, match.FullSummary(0));
            Assert.AreEqual(player1 + points, match.FullSummary(1));
            Assert.AreEqual(player2 + points, match.FullSummary(2));
            Assert.AreEqual(player3 + points, match.FullSummary(3));
            Assert.AreEqual(others + points, match.FullSummary(4));
        }

        /*

            int[] cards = ConvertToInt(new List<string>
            {
                "", "", "", "", // winner: , points:  (start at 0)
                "", "", "", "", // winner: , points:
                "", "", "", "", // winner: , points:
                "", "", "", "", // winner: , points:
                "", "", "", "", // winner: , points:
                "", "", "", "", // winner: , points:
                "", "", "", "", // winner: , points:
                "", "", "", "", // winner: , points:
            }, match.CurrentPlayers);
         */

        [TestMethod]
        public void WriteFirstCardsOfAFewSeeds()
        {
            for (int i = 0; i < 1000; ++i)
            {
                ListCards(i);
                Console.WriteLine("-----------------");
            }
        }

        [TestMethod]
        public void Temp2() => ListCards(98, new SchafkopfMatchConfig(SchafkopfMode.SoloTout, Card.HERZ));

        private void AssertGameStopsWhenAlwaysPlayingFirstCard(SchafkopfMatch match)
        {
            AssertGameStopsWithTheseCards(match, AllZero);
        }

        private void AssertGameStopsWithTheseCards(SchafkopfMatch match, IReadOnlyList<int> cards)
        {
            AssertGameStopsWithTheseCards(match, i => cards[i]);
        }

        private void AssertGameStopsWithTheseCards(SchafkopfMatch match, IReadOnlyList<string> cards)
        {
            AssertGameStopsWithTheseCards(match, i => FindCard(match, cards, i));
        }

        private int FindCard(SchafkopfMatch match, IReadOnlyList<string> cards, int i)
        {
            List<Card> playerCards = match.CurrentPlayers[match.CurrentRound.CurrentPlayer].PlayableCards;
            int index = Index(cards[i], playerCards);
            if (index != -1) return index;
            throw new Exception($"The cards to pick do not match the seed at: index {i}, remaining cards: " +
                                $"{ArrayPrinter.ArrayString(playerCards)} and desired card {cards[i]}");
        }

        private void AssertGameStopsWithTheseCards(SchafkopfMatch match, Func<int, int> selector)
        {
            for (int i = 0; i < Card.ALL_CARDS.Count; ++i)
            {
                int currentPlayerIndex = match.CurrentRound.CurrentPlayer;
                int selected = selector(i);
                Card currentCard = match.CurrentPlayers[match.CurrentRound.CurrentPlayer].PlayableCards[selected];
                string first = match.CurrentCardCount == 0 ? "empty" : match.CurrentRound.currentCards[0].ToString();
                Assert.IsTrue(match.PlayCard(selected),
                    "Move {0}: player with index {1} should be able to play {2} with the first card being {3}",
                    i, currentPlayerIndex, currentCard, first);
            }

            Assert.IsTrue(match.IsGameOver);
        }

        private static int[] ConvertToInt(IReadOnlyList<string> cards, IReadOnlyList<SchafkopfPlayer> players)
        {
            int[] ret = new int[cards.Count];
            List<Card>[] cardsCloned = new List<Card>[players.Count];
            for (int i = 0; i < cardsCloned.Length; i++)
            {
                cardsCloned[i] = new List<Card>();
                cardsCloned[i].AddRange(players[i].PlayableCards);
            }

            for (int i = 0; i < cards.Count; ++i)
            {
                int playerIndex = 0;
                int index;
                while (-1 == (index = Index(cards[i], cardsCloned[playerIndex]))) ++playerIndex;
                ret[i] = index;
                cardsCloned[playerIndex].RemoveAt(index);
            }

            return ret;
        }

        private static int Index(string card, IReadOnlyList<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].ToString().Equals(card)) return i;
            }

            return -1;
        }

        private static void ListCards(int seed, bool listUnsorted = false)
        {
            ListCards(seed, new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.EICHEL), listUnsorted);
        }

        private static void ListCards(int seed, SchafkopfMatchConfig matchSort, bool listUnsorted = false)
        {
            Console.WriteLine($@"Cards for seed: {seed}");
            SchafkopfMatch match = new SchafkopfMatch(Players, new Random(seed), true);
            IEnumerable<string> result = match.ShuffleCards();
            SchafkopfPlayer[] players = new SchafkopfPlayer[SchafkopfMatch.PLAYER_PER_ROUND];
            for (int j = 0; j < players.Length; j++) players[j] = new SchafkopfPlayer(j.ToString(), "name" + j);
            int i = 0;
            foreach (var s in result)
            {
                try
                {
                    players[i % players.Length].PlayableCards.Add(Card.ALL_CARDS[int.Parse(s)]);
                    ++i;
                }
                catch (Exception)
                {
                    // should happen
                }
            }

            if (listUnsorted)
            {
                foreach (var player in players) PrintPlayerCardsWithId(player);
            }


            foreach (var player in players)
            {
                player.SortCards(matchSort);
                PrintPlayerCardsWithId(player);
            }
        }

        private static void PrintPlayerCardsWithId(SchafkopfPlayer player)
        {
            Console.WriteLine(player.Id + ": " + ArrayPrinter.ArrayString(player.PlayableCards));
        }
    }
}