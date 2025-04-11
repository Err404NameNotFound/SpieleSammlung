using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Multiplayer;
using SpieleSammlung.Model.Schafkopf;
using SpieleSammlung.Model.Util;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Schafkopf;

[TestClass]
public class SchafkopfMatchTest
{
    private static readonly List<MultiplayerPlayer> Players =
    [
        new("1", "name1"),
        new("2", "name2"),
        new("3", "name3"),
        new("4", "name4"),
        new("5", "name5")
    ];

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
        AssertMessagesCorrect(match, "\n\n+50 SoloTout\n*2 tout\n*2 aufgestellt\n_________________\n200",
            "Du hast das SoloTout mit 120 Augen gewonnen",
            "Du hast das SoloTout mit name3 und name4 mit 0 Augen verloren",
            "Du hast das SoloTout mit name2 und name4 mit 0 Augen verloren",
            "Du hast das SoloTout mit name2 und name3 mit 0 Augen verloren",
            "name1 hat das SoloTout mit 120 Augen gewonnen");
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
    public void TestRoundGrasSoloLooseOneStichNoPoints()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(98), true);
        match.ShuffleCards();
        match.CurrentPlayers[3].Aufgestellt = true;
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Solo, Card.HERZ);
        List<int> clone = AllZero.ToList();
        clone[23] = 1;
        clone[27] = 1;
        AssertGameStopsWithTheseCards(match, clone);
        AssertMessagesCorrect(match,
            "\n\n+50 Solo\n+10 Schneider\n*2 aufgestellt\n_________________\n120",
            "Du hast das Solo mit name2 und name3 mit 0 Augen verloren",
            "Du hast das Solo mit name1 und name3 mit 0 Augen verloren",
            "Du hast das Solo mit name1 und name2 mit 0 Augen verloren",
            "Du hast das Solo mit 120 Augen gewonnen",
            "name4 hat das Solo mit 120 Augen gewonnen");
    }

    [TestMethod]
    public void TestRoundGrasSoloLooseNoStich()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(98), true);
        match.ShuffleCards();
        match.CurrentPlayers[3].Aufgestellt = true;
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Solo, Card.HERZ);
        AssertGameStopsWithTheseCards(match, AllZero);
        AssertMessagesCorrect(match,
            "\n\n+50 Solo\n+20 Schneiderschwarz\n*2 aufgestellt\n_________________\n140",
            "Du hast das Solo mit name2 und name3 mit 0 Augen verloren",
            "Du hast das Solo mit name1 und name3 mit 0 Augen verloren",
            "Du hast das Solo mit name1 und name2 mit 0 Augen verloren",
            "Du hast das Solo mit 120 Augen gewonnen",
            "name4 hat das Solo mit 120 Augen gewonnen");
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
        List<string> cards =
        [
            "Eichel Neun", "Eichel Acht", "Eichel Zehn", "Eichel Sau", //player 0 (21)
            "Gras Ober", "Herz Sieben", "Herz Zehn", "Herz Unter", // player 3 (15)
            "Herz Neun", "Schelle Unter", "Eichel Unter", "Schelle Acht", // player 3 (4)
            "Eichel Ober", "Schelle Neun", "Herz Ober", "Herz Acht", // player 1 (6)
            "Herz Koenig", "Gras Neun", "Schelle Ober", "Gras Unter", // player 1 (9)
            "Gras Sau", "Gras Zehn", "Schelle Zehn", "Gras Sieben", // player 3 (31)
            "Schelle Sieben", "Herz Sau", "Schelle Sau", "Schelle Koenig", // player 3 (26)
            "Gras Koenig", "Eichel Sieben", "Eichel Koenig", "Gras Acht"
        ];
        AssertGameStopsWithTheseCards(match, cards);
        AssertMessagesCorrect(match, "\n\n+20 Sauspiel\n+50 Laufende\n*2 aufgestellt\n_________________\n140",
            "Du hast das Sauspiel mit name3 mit 34 Augen verloren",
            "Du hast das Sauspiel mit name4 mit 86 Augen gewonnen",
            "Du hast das Sauspiel mit name1 mit 34 Augen verloren",
            "Du hast das Sauspiel mit name2 mit 86 Augen gewonnen",
            "name4 hat das Sauspiel mit name2 mit 86 Augen gewonnen");
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
        List<string> cards =
        [
            "Gras Neun", "Gras Acht", "Gras Unter", "Gras Zehn", // winner: 2, points: 12 (start at 0)
            "Schelle Acht", "Schelle Sieben", "Schelle Zehn", "Eichel Sau", // winner: 0, points: 21
            "Herz Neun", "Herz Ober", "Herz Sieben", "Herz Sau", // winner: 3, points: 14
            "Eichel Unter", "Eichel Acht", "Schelle Unter", "Herz Acht", // winner: 3, points: 4
            "Herz Unter", "Gras Ober", "Gras Sieben", "Schelle Neun", // winner: 3, points: 
            "Gras Sau", "Gras Koenig", "Eichel Sieben", "Schelle Ober", // winner: 3, points: 
            "Herz Zehn", "Herz Koenig", "Eichel Neun", "Eichel Ober", // winner: 3, points: 
            "Schelle Sau", "Schelle Koenig", "Eichel Zehn", "Eichel Koenig" // winner: 3, points: 
        ];
        AssertGameStopsWithTheseCards(match, cards);
        AssertMessagesCorrect(match,
            "\n\n+50 Wenz\n*2 aufgestellt\n*2 aufgestellt\n_________________\n200",
            "Du hast den Wenz mit name2 und name3 mit 33 Augen verloren",
            "Du hast den Wenz mit name1 und name3 mit 33 Augen verloren",
            "Du hast den Wenz mit name1 und name2 mit 33 Augen verloren",
            "Du hast den Wenz mit 87 Augen gewonnen",
            "name4 hat den Wenz mit 87 Augen gewonnen");
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
        List<string> cards =
        [
            "Herz Neun", "Herz Sau", "Gras Sau", "Herz Ober", // winner: 1, points: 25 (start at 0)
            "Schelle Sau", "Schelle Ober", "Schelle Zehn", "Gras Koenig", // winner: 1, points: 28
            "Gras Neun", "Gras Zehn", "Gras Ober", "Schelle Unter", // winner: 0, points: 15
            "Eichel Sau", "Eichel Koenig", "Eichel Sieben", "Eichel Neun", // winner: 0, points: 15
            "Eichel Ober", "Eichel Zehn", "Eichel Acht", "Schelle Koenig", // winner: 1, points: 17
            "Herz Zehn", "Gras Sieben", "Herz Acht", "Eichel Unter", // winner: 0, points: 12
            "Gras Unter", "Herz Koenig", "Schelle Acht", "Gras Acht", // winner: 0, points: 6
            "Herz Unter", "Schelle Neun", "Schelle Sieben", "Herz Sieben" // winner: 0, points: 2
        ];
        AssertGameStopsWithTheseCards(match, cards);
        AssertMessagesCorrect(match,
            "\n\n+50 Wenz\n+40 Laufende\n*2 aufgestellt\n*2 Kontra\n_________________\n360",
            "Du hast den Wenz mit 50 Augen verloren",
            "Du hast den Wenz mit name3 und name4 mit 70 Augen gewonnen",
            "Du hast den Wenz mit name2 und name4 mit 70 Augen gewonnen",
            "Du hast den Wenz mit name2 und name3 mit 70 Augen gewonnen",
            "name1 hat den Wenz mit 50 Augen verloren");
    }

    [TestMethod]
    public void TestCardsReadFromMessages()
    {
        SchafkopfMatch host = new SchafkopfMatch(Players, new Random(31), true);
        SchafkopfMatch client = new SchafkopfMatch(Players.ToList(), new Random(31), false);
        IEnumerable<string> result = host.ShuffleCards(false, "-1");
        List<string> cards = result.Where(card => int.TryParse(card, out _)).ToList();
        client.ShuffleCards(cards, false);
        AssertCardsAreEqual(host, client);
    }

    [TestMethod]
    public void TestGameCanBeRestored()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(89), true);
        match.ShuffleCards();
        match.CurrentPlayers[0].Aufgestellt = true;
        match.ChoseGameMode(SchafkopfMode.Wenz, "", 0);
        match.ChoseGameMode(SchafkopfMode.Weiter, "", 1);
        match.ChoseGameMode(SchafkopfMode.Weiter, "", 2);
        match.ChoseGameMode(SchafkopfMode.Weiter, "", 3);
        match.CurrentPlayers[3].Kontra = true;
        List<string> cards =
        [
            "Herz Neun", "Herz Sau", "Gras Sau", "Herz Ober",
            "Schelle Sau", "Schelle Ober", "Schelle Zehn", "Gras Koenig",
            "Gras Neun", "Gras Zehn", "Gras Ober", "Schelle Unter",
            "Eichel Sau", "Eichel Koenig", "Eichel Sieben", "Eichel Neun",
            "Eichel Ober", "Eichel Zehn", "Eichel Acht", "Schelle Koenig",
            "Herz Zehn", "Gras Sieben", "Herz Acht", "Eichel Unter",
            "Gras Unter", "Herz Koenig"
        ];
        AssertAllCardsCanBePlayed(match, cards);
        SchafkopfMatch clientMatch = new SchafkopfMatch(Players.ToList(), false);
        const char separator = ';';
        const char fineSeparator = '|';
        string restoreString = "test" + separator + match.InfoForRejoin(separator, fineSeparator.ToString());
        clientMatch.RestoreFromInfo(restoreString.Split(separator), fineSeparator);
        List<string> cards2 =
        [
            "Schelle Acht", "Gras Acht",
            "Herz Unter", "Schelle Neun", "Schelle Sieben", "Herz Sieben"
        ];
        AssertAllCardsCanBePlayed(clientMatch, cards2);
        AssertMessagesCorrect(clientMatch,
            "\n\n+50 Wenz\n+40 Laufende\n*2 aufgestellt\n*2 Kontra\n_________________\n360",
            "Du hast den Wenz mit 50 Augen verloren",
            "Du hast den Wenz mit name3 und name4 mit 70 Augen gewonnen",
            "Du hast den Wenz mit name2 und name4 mit 70 Augen gewonnen",
            "Du hast den Wenz mit name2 und name3 mit 70 Augen gewonnen",
            "name1 hat den Wenz mit 50 Augen verloren");
    }

    [TestMethod]
    public void TestGameCanBeRestoredBeforeGameStarted()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(98), true);
        match.ShuffleCards();
        match.CurrentPlayers[3].Aufgestellt = true;
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        SchafkopfMatch clientMatch = new SchafkopfMatch(Players.ToList(), false);
        const char separator = ';';
        const char fineSeparator = '|';
        string restoreString = "test" + separator + match.InfoForRejoin(separator, fineSeparator.ToString());
        clientMatch.RestoreFromInfo(restoreString.Split(separator), fineSeparator);
        match = clientMatch;
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
    [ExpectedException(typeof(IllegalMoveException))]
    public void TestThrowsException()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, true);
        match.ShuffleCards();
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "", 3);
    }

    [TestMethod]
    public void TestAufEichelWeglaufen()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(5), true);
        match.ShuffleCards();
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Sauspiel, Card.EICHEL);
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        List<string> cards =
        [
            "Schelle Koenig", "Schelle Unter", "Schelle Acht",
            "Schelle Sieben", // winner: 1, points: 6 (start at 0)
            "Eichel Sieben", "Eichel Koenig", "Herz Zehn", "Eichel Acht", // winner: 3, points: 14
            "Gras Sau", "Gras Koenig", "Gras Sieben", "Herz Sieben", // winner: 2, points: 15
            "Eichel Ober", "Herz Unter", "Herz Neun", "Herz Ober", // winner: 2, points: 8
            "Herz Acht", "Herz Sau", "Herz Koenig", "Gras Acht", // winner: 3, points: 15
            "Schelle Sau", "Schelle Zehn", "Eichel Neun", "Schelle Neun", // winner: 3, points: 21
            "Gras Zehn", "Gras Neun", "Eichel Sau", "Schelle Ober", // winner: 2, points: 24
            "Gras Unter", "Eichel Unter", "Gras Ober", "Eichel Zehn" // winner: 0, points: 17
        ];
        AssertGameStopsWithTheseCards(match, cards);
        AssertMessagesCorrect(match, "\n\n+20 Sauspiel\n_________________\n20",
            "Du hast das Sauspiel mit name4 mit 67 Augen gewonnen",
            "Du hast das Sauspiel mit name3 mit 53 Augen verloren",
            "Du hast das Sauspiel mit name2 mit 53 Augen verloren",
            "Du hast das Sauspiel mit name1 mit 67 Augen gewonnen",
            "name3 hat das Sauspiel mit name2 mit 53 Augen verloren");
    }

    [TestMethod]
    public void TestAufEichelWeglaufenDoesNotWork()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(5), true);
        match.ShuffleCards();
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Sauspiel, Card.EICHEL);
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        List<string> cards =
        [
            "Eichel Acht", "Eichel Sau", "Eichel Koenig", "Herz Sau", // winner: 3, points: 26 (start at 0)
            "Gras Sau", "Gras Koenig", "Gras Sieben", "Herz Sieben", // winner: 2, points: 15
            "Herz Acht", "Herz Unter", "Herz Neun", "Herz Ober", // winner: 1, points: 5
            "Schelle Unter", "Schelle Ober", "Herz Zehn", "Gras Ober", // winner: 0, points: 18
            "Schelle Koenig", "Gras Acht", "Schelle Acht", "Schelle Sau", // winner: 3, points: 15
            "Schelle Sieben", "Schelle Zehn", "Eichel Sieben", "Schelle Neun", // winner: 0, points: 10
            "Gras Neun", "Eichel Neun", "Gras Unter", "Gras Zehn", // winner: 2, points: 12
            "Eichel Ober", "Eichel Unter", "Herz Koenig", "Eichel Zehn" // winner: 2, points: 19
        ];
        AssertGameStopsWithTheseCards(match, cards);
        AssertMessagesCorrect(match, "\n\n+20 Sauspiel\n_________________\n20",
            "Du hast das Sauspiel mit name4 mit 69 Augen gewonnen",
            "Du hast das Sauspiel mit name3 mit 51 Augen verloren",
            "Du hast das Sauspiel mit name2 mit 51 Augen verloren",
            "Du hast das Sauspiel mit name1 mit 69 Augen gewonnen",
            "name3 hat das Sauspiel mit name2 mit 51 Augen verloren");
    }

    [TestMethod]
    public void ReturnsFalseWhenNotCurrentPlayerPlaysCard()
    {
            
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(42), true);
        match.ShuffleCards();
        match.ChoseGameMode(SchafkopfMode.SoloTout, Card.GRAS);
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        int index = match.CurrentRound.CurrentPlayer;
        Assert.IsFalse(match.PlayCard(0, 1));
        Assert.AreEqual(index, match.CurrentRound.CurrentPlayer);
    }

    [TestMethod]
    [ExpectedException(typeof(IllegalMoveException))]
    public void ThrowsExceptionWhenShufflingAlthoughNotPossible1()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, true);
        match.ShuffleCards();
        match.ShuffleCards();
    }

    [TestMethod]
    [ExpectedException(typeof(IllegalMoveException))]
    public void ThrowsExceptionWhenShufflingAlthoughNotPossible2()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, true);
        match.ShuffleCards();
        match.ChoseGameMode(SchafkopfMode.Sauspiel, Card.GRAS);
        match.ShuffleCards();
    }

    [TestMethod]
    [ExpectedException(typeof(IllegalMoveException))]
    public void ThrowsExceptionWhenClientShuffles()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, false);
        match.ShuffleCards();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ExceptionThrownWhenSummaryOfNonexistentPlayer()
    {
        SchafkopfMatch match = new SchafkopfMatch(Players, new Random(42), true);
        match.ShuffleCards();
        match.CurrentPlayers[0].Aufgestellt = true;
        match.ChoseGameMode(SchafkopfMode.SoloTout, Card.GRAS);
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        match.ChoseGameMode(SchafkopfMode.Weiter, "");
        AssertGameStopsWhenAlwaysPlayingFirstCard(match);
        try
        {
            match.FullSummary(match.Players.Count);
            Assert.Fail("The last line should have thrown an exception");
        }
        catch (Exception)
        {
            match.FullSummary(-1);
        }
            
    }

    [TestMethod]
    public void WriteFirstCardsOfAFewSeeds()
    {
        // for (int i = 0; i < 1000; ++i)
        // {
        //     ListCards(i);
        //     Console.WriteLine("-----------------");
        // }
    }

    [TestMethod]
    public void Temp2() => ListCards(98, new SchafkopfMatchConfig(SchafkopfMode.SoloTout, Card.HERZ));
        
    /*
            "", "", "", "", // winner: , points:  (start at 0)
            "", "", "", "", // winner: , points:
            "", "", "", "", // winner: , points:
            "", "", "", "", // winner: , points:
            "", "", "", "", // winner: , points:
            "", "", "", "", // winner: , points:
            "", "", "", "", // winner: , points:
            "", "", "", "", // winner: , points:
     */
        
    private static void AssertCardsAreEqual(SchafkopfMatch host, SchafkopfMatch client)
    {
        Assert.AreEqual(host.CurrentPlayers.Count, client.CurrentPlayers.Count);
        for (int p = 0; p < host.CurrentPlayers.Count; ++p)
        {
            List<Card> hostCards = host.CurrentPlayers[p].PlayableCards;
            List<Card> clientCards = client.CurrentPlayers[p].PlayableCards;
            Assert.AreEqual(hostCards.Count, clientCards.Count);
            for (int c = 0; c < hostCards.Count; ++c)
            {
                Assert.AreEqual(hostCards[c], clientCards[c]);
            }
        }
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

    private static void AssertGameStopsWhenAlwaysPlayingFirstCard(SchafkopfMatch match)
        => AssertGameStopsWithTheseCards(match, AllZero);


    private static void AssertGameStopsWithTheseCards(SchafkopfMatch match, IReadOnlyList<int> cards)
        => AssertGameStopsWithTheseCards(match, i => cards[i]);


    private static void AssertGameStopsWithTheseCards(SchafkopfMatch match, IReadOnlyList<string> cards)
        => AssertGameStopsWithTheseCards(match, i => FindCard(match, cards, i));

    private static void AssertGameStopsWithTheseCards(SchafkopfMatch match, Func<int, int> selector)
    {
        AssertAllCardsCanBePlayed(match, selector, Card.ALL_CARDS.Count);
        Assert.IsTrue(match.IsGameOver);
    }

    private static int FindCard(SchafkopfMatch match, IReadOnlyList<string> cards, int i)
    {
        List<Card> playerCards = match.CurrentPlayers[match.CurrentRound.CurrentPlayer].PlayableCards;
        int index = Index(cards[i], playerCards);
        if (index != -1) return index;
        throw new Exception($"The cards to pick do not match the seed at: index {i}, remaining cards: " +
                            $"{ArrayPrinter.ArrayString(playerCards)} and desired card {cards[i]}");
    }

    private static void AssertAllCardsCanBePlayed(SchafkopfMatch match, IReadOnlyList<string> cards)
        => AssertAllCardsCanBePlayed(match, i => FindCard(match, cards, i), cards.Count);

    private static void AssertAllCardsCanBePlayed(SchafkopfMatch match, Func<int, int> selector, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            int currentPlayerIndex = match.CurrentRound.CurrentPlayer;
            int selected = selector(i);
            Card currentCard = match.CurrentPlayers[match.CurrentRound.CurrentPlayer].PlayableCards[selected];
            string first = match.CurrentCardCount == 0 ? "empty" : match.CurrentRound.CurrentCards[0].ToString();
            Assert.IsTrue(match.PlayCard(selected),
                "Move {0}: player with index {1} should be able to play {2} with the first card being {3}",
                i, currentPlayerIndex, currentCard, first);
        }
    }

    private static int[] ConvertToInt(IReadOnlyList<string> cards, IReadOnlyList<SchafkopfPlayer> players)
    {
        int[] ret = new int[cards.Count];
        List<Card>[] cardsCloned = new List<Card>[players.Count];
        for (int i = 0; i < cardsCloned.Length; i++)
        {
            cardsCloned[i] = [];
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