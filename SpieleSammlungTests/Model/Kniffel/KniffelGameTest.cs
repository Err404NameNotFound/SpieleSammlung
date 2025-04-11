using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel;
using System.Collections.Generic;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Kniffel;

[TestClass]
public class KniffelGameTest
{
    private static readonly List<Player> Players;

    private static KniffelGame _continuousGame;

    private static readonly RandomStub Rng;

    private static readonly KniffelGame RiggedGame;

    static KniffelGameTest()
    {
        Players = [new Player("player 1", false), new Player("player 2", false)];
        _continuousGame = new KniffelGame(Players, 420);
        Rng = new RandomStub();
        RiggedGame = new KniffelGame(Players, Rng);
    }

    [TestMethod]
    public void Test01_RoundCapValue()
    {
        int count = 0;
        while (_continuousGame.IsGameNotOver())
        {
            for (int i = 0; i < _continuousGame.Players.Count; ++i)
            {
                PlayTurn(_continuousGame);
            }

            ++count;
        }

        Assert.AreEqual(count, _continuousGame.Round);
    }

    [TestMethod]
    public void Test02_RoundCap()
    {
        Assert.AreEqual(KniffelPointsTable.WRITEABLE_FIELDS_COUNT, _continuousGame.Round);
    }

    [TestMethod]
    public void Test03_InitialShuffleAmount()
    {
        KniffelGame game = new KniffelGame(Players);
        Assert.AreEqual(KniffelGame.INITIAL_SHUFFLE_COUNT - 1, game.RemainingShuffles);
    }

    [TestMethod]
    public void Test04_ShuffleAmountDecreases()
    {
        KniffelGame game = new KniffelGame(Players);
        int expected = game.RemainingShuffles - 1;
        game.Shuffle([0, 1, 2]);
        Assert.AreEqual(expected, game.RemainingShuffles);
    }

    [TestMethod]
    public void Test05_ShuffleAmountDecreasesToZero()
    {
        KniffelGame game = new KniffelGame(Players);
        for (int i = game.RemainingShuffles; i > 0; --i)
        {
            game.Shuffle([0, 4]);
        }

        Assert.AreEqual(0, game.RemainingShuffles);
    }

    [TestMethod]
    [ExpectedException(typeof(IllegalMoveException))]
    public void Test06_ShuffleAmountLimited()
    {
        KniffelGame game = new KniffelGame(Players);
        for (int i = game.RemainingShuffles; i > 0; --i)
        {
            game.Shuffle([2, 3]);
        }

        game.Shuffle();
    }

    [TestMethod]
    public void Test07_GameStartsWithInitialisedOptions()
    {
        _continuousGame = new KniffelGame(Players, 420);
        Assert.AreEqual(8, _continuousGame.KillableFieldsCount);
        Assert.AreEqual(5, _continuousGame.WriteableFieldsCount);
    }

    [TestMethod]
    public void Test08_PointsAreWritten()
    {
        _continuousGame.WriteField(0);
        Assert.AreEqual(3, _continuousGame.Players[0][0].Value);
        Assert.AreEqual(3, _continuousGame.Players[0][KniffelPointsTable.INDEX_SUM].Value);
    }

    [TestMethod]
    public void Test09_PlayerChanges()
    {
        KniffelPlayer previousPlayer = _continuousGame.CurrentPlayer;
        _continuousGame.Shuffle([0]);
        _continuousGame.Shuffle([0]);
        _continuousGame.WriteField(3);
        Assert.AreNotEqual(previousPlayer, _continuousGame.CurrentPlayer);
    }

    [TestMethod]
    public void Test10_FullHouseValueIsWritten()
    {
        PlayTurn(_continuousGame);
        Assert.AreEqual(KniffelGame.VALUE_FULL_HOUSE,
            _continuousGame.CurrentPlayer[KniffelPointsTable.INDEX_FULL_HOUSE].Value);
    }

    [TestMethod]
    public void Test11_GameAlwaysReachesGameOverState()
    {
        FinishGame(_continuousGame);
    }

    [TestMethod]
    public void Test13_KillableFieldsHasRightLength()
    {
        KniffelGame game = new KniffelGame(Players, 13);
        Assert.AreEqual(8, game.KillableFieldsCount);
    }

    [TestMethod]
    public void Test14_WriteableFieldsHasRightLength()
    {
        KniffelGame game = new KniffelGame(Players, 60);
        Assert.AreEqual(6, game.WriteableFieldsCount);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Test15_RequiresAtLeast2Players()
    {
        KniffelGame game = new KniffelGame(new List<Player> { new() });
        Assert.AreEqual(6, game.WriteableFieldsCount);
    }

    [TestMethod]
    public void Test16_ConstructorBotStrategy()
    {
        _ = new KniffelGame(new List<Player> { new(), new() }, new BotStrategy());
    }

    [TestMethod]
    public void TestRigged01_WriteKniffel()
    {
        Rng.SetNext(4, 5, 4, 3, 2);
        RiggedGame.Shuffle();
        Rng.SetOutputConstant(4);
        RiggedGame.Shuffle([1, 3, 4]);
        Rng.ClearOutputConstant();
        Rng.SetNext(2, 3, 4, 5, 6);
        Assert.AreEqual(5, RiggedGame.WriteableFieldsCount);
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.WriteField(3);
        Assert.AreEqual(KniffelGame.VALUE_KNIFFEL, previous[KniffelPointsTable.INDEX_KNIFFEL].Value);
    }

    [TestMethod]
    public void TestRigged02_WriteBigStreet()
    {
        Assert.AreEqual(8, RiggedGame.WriteableFieldsCount);
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.WriteField(6);
        Assert.AreEqual(KniffelGame.VALUE_BIG_STREET, previous[KniffelPointsTable.INDEX_BIG_STREET].Value);
    }

    [TestMethod]
    public void TestRigged03_WriteSmallStreet()
    {
        Rng.SetNext(4, 5, 5, 3, 2);
        RiggedGame.Shuffle();
        Assert.AreEqual(6, RiggedGame.WriteableFieldsCount);
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        Rng.SetNext(5, 5, 3, 3, 3);
        RiggedGame.WriteField(4);
        Assert.AreEqual(KniffelGame.VALUE_SMALL_STREET, previous[KniffelPointsTable.INDEX_SMALL_STREET].Value);
    }

    [TestMethod]
    public void TestRigged04_WriteOption()
    {
        Assert.AreEqual(2, RiggedGame.GetWriteableFieldsIndex(0).Index);
        Assert.AreEqual(9, RiggedGame.GetWriteableFieldsIndex(0).Value);
        Assert.AreEqual(4, RiggedGame.GetWriteableFieldsIndex(1).Index);
        Assert.AreEqual(10, RiggedGame.GetWriteableFieldsIndex(1).Value);
    }

    [TestMethod]
    public void TestRigged05_WritePair3()
    {
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.WriteField(2);
        Assert.AreEqual(19, previous[KniffelPointsTable.INDEX_PAIR_SIZE_3].Value);
    }

    [TestMethod]
    public void TestRigged06_WritePair4()
    {
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        Rng.SetOutputConstant(1);
        RiggedGame.Shuffle();
        RiggedGame.WriteField(1);
        Rng.ClearOutputConstant();
        Assert.AreEqual(5, previous[KniffelPointsTable.INDEX_PAIR_SIZE_3].Value);
    }

    [TestMethod]
    public void TestRigged07_KillOption()
    {
        Rng.SetNext(3, 2, 4, 5, 1);
        RiggedGame.Shuffle();
        Assert.AreEqual(5, RiggedGame.GetKillableFieldIndex(0));
        Assert.AreEqual(9, RiggedGame.GetKillableFieldIndex(1));
        Assert.AreEqual(13, RiggedGame.GetKillableFieldIndex(3));
    }

    [TestMethod]
    public void TestRigged08_KillKniffel()
    {
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.KillFieldOption(3);
        Assert.AreEqual(0, previous[KniffelPointsTable.INDEX_KNIFFEL].Value);
    }

    [TestMethod]
    public void TestRigged09_ShufflesLeft()
    {
        Assert.IsTrue(RiggedGame.AreShufflesLeft);
        Assert.IsFalse(RiggedGame.AreNoShufflesLeft);
    }

    [TestMethod]
    public void TestRigged10_NoShufflesLeft()
    {
        RiggedGame.Shuffle();
        RiggedGame.Shuffle();
        Assert.IsFalse(RiggedGame.AreShufflesLeft);
        Assert.IsTrue(RiggedGame.AreNoShufflesLeft);
    }

    [TestMethod]
    public void TestRigged11_GetDice()
    {
        Rng.SetNext(5, 5, 3, 2, 1);
        RiggedGame.KillFieldOption(0);
        Assert.AreEqual("{ 5, 5, 3, 2, 1 }", RiggedGame.Dice.ToString());
    }

    [TestMethod]
    public void TestRigged12_GetDiceValue()
    {
        Assert.AreEqual(1, RiggedGame.GetDiceValue(4));
        Assert.AreEqual(5, RiggedGame.GetDiceValue(0));
        Assert.AreEqual(3, RiggedGame.GetDiceValue(2));
    }

    [TestMethod]
    public void TestRigged13_GetDiceIsACopy()
    {
        Rng.SetNext(1, 1, 1, 1, 1);
        RiggedGame.Dice.Shuffle();
        Assert.AreEqual("{ 5, 5, 3, 2, 1 }", RiggedGame.Dice.ToString());
    }

    [TestMethod]
    public void TestRigged14_BotMoveExecutesAMoveForABot()
    {
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.DoBotMove(_ => { }, () => { });
        Assert.AreNotEqual(previous, RiggedGame.CurrentPlayer);
    }

    [TestMethod]
    public void TestRigged15_BotMoveInstantExecutesAMoveForABot()
    {
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.DoBotMoveInstant();
        Assert.AreNotEqual(previous, RiggedGame.CurrentPlayer);
    }

    [TestMethod]
    public void TestRigged16_GetRandomValueHasRightBounds()
    {
        Assert.IsTrue(RiggedGame.RandomDiceValue() >= Dice.LOWEST_VALUE);
        Assert.IsTrue(RiggedGame.RandomDiceValue() < Dice.HIGHEST_VALUE);
    }

    [TestMethod]
    public void TestRigged17_KillFieldGlobal()
    {
        RiggedGame.Shuffle();
        Rng.SetNext(1, 1, 1, 1, 1);
        RiggedGame.Shuffle();
        KniffelPlayer previous = RiggedGame.CurrentPlayer;
        RiggedGame.KillFieldGlobalIndex(KniffelPointsTable.INDEX_PAIR_SIZE_4);
        Assert.IsFalse(previous[KniffelPointsTable.INDEX_PAIR_SIZE_4].IsEmpty());
        Assert.AreEqual(0, previous[KniffelPointsTable.INDEX_PAIR_SIZE_4].Value);
    }

    private static void PlayTurn(KniffelGame game)
    {
        if (game.WriteableFieldsCount > 0)
        {
            game.WriteField(0);
        }
        else
        {
            game.KillFieldOption(0);
        }
    }

    private static void FinishGame(KniffelGame game)
    {
        for (int i = 0; game.IsGameNotOver(); ++i)
        {
            if (i >= KniffelGame.INITIAL_SHUFFLE_COUNT * KniffelPointsTable.WRITEABLE_FIELDS_COUNT)
            {
                Assert.Fail("The game should already be over, due to too many shuffles");
            }

            PlayTurn(game);
        }
    }
}