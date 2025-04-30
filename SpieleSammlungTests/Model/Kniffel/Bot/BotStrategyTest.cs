#region

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlungTests.Utils;

#endregion

namespace SpieleSammlungTests.Model.Kniffel.Bot;

[TestClass]
public class BotStrategyTest
{
    private static readonly List<Player> Players = [new(), new()];
    private static readonly IntArrayComparer Comparer = new();
    private readonly KniffelPlayer _player = new(new Player());
    private readonly BotStrategy _strategy = new();

    [TestMethod]
    public void TestTriesToImproveToBigStreetShuffle1()
    {
        FlatDice dice = CreateDice(3, 4, 5, 6, 1);
        int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(dice, _player);
        Assert.AreEqual(1, shuffle.Length);
        Assert.AreEqual(1, dice[shuffle[0]]);
    }

    [TestMethod]
    public void TestTriesToImproveToBigStreetShuffle6()
    {
        FlatDice dice = CreateDice(3, 4, 5, 6, 6);
        int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(dice, _player);
        Assert.AreEqual(1, shuffle.Length);
        Assert.AreEqual(6, dice[shuffle[0]]);
    }

    [TestMethod]
    public void TestDoesNotReShuffleKniffel()
    {
        int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(CreateDice(1, 1, 1, 1, 1), _player);
        Assert.AreEqual(0, shuffle.Length);
    }

    [TestMethod]
    public void TestDoesNotReShuffleAll6Kniffel()
    {
        int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(CreateDice(6, 6, 6, 6, 6), _player);
        Assert.AreEqual(0, shuffle.Length);
    }

    [TestMethod]
    public void TestDoesNotReShuffleBigStreetWith1()
    {
        int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(CreateDice(2, 4, 5, 1, 3), _player);
        Assert.AreEqual(0, shuffle.Length);
    }

    [TestMethod]
    public void TestDoesNotReShuffleBigStreetWith6()
    {
        int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(CreateDice(6, 2, 4, 5, 3), _player);
        Assert.AreEqual(0, shuffle.Length);
    }

    [TestMethod]
    public void TestPicksKniffelIfPossible()
    {
        KniffelGame game = CreateGame(1, 1, 1, 1, 1);
        KniffelPlayer previous = game.CurrentPlayer;
        _strategy.ChooseBestField(game);
        Assert.AreEqual(KniffelGame.VALUE_KNIFFEL, previous[KniffelPointsTable.INDEX_KNIFFEL].Value);
    }

    [TestMethod]
    public void TestWritesChanceAsOnlyOption()
    {
        RandomStub rng = new RandomStub();
        rng.SetOutputConstant(1);
        KniffelGame game = new KniffelGame(Players, rng);
        for (int i = 0; i < 8; ++i) _strategy.ChooseBestField(game);
        rng.SetOutputConstant(3);
        for (int i = 0; i < 8; ++i) _strategy.ChooseBestField(game);
        Assert.AreEqual(KniffelGame.VALUE_KNIFFEL + 3 * 5 + 2 * 15,
            game.CurrentPlayer[KniffelPointsTable.INDEX_SUM].Value);
    }

    [TestMethod]
    public void TestKillsFieldsIfItHasTo()
    {
        RandomStub rng = new RandomStub();
        rng.SetOutputConstant(1);
        KniffelGame game = new KniffelGame(Players, rng);
        for (int i = 0; i < 8; ++i) _strategy.ChooseBestField(game);
        rng.SetOutputConstant(3);
        for (int i = 0; i < 8; ++i) _strategy.ChooseBestField(game);
        rng.SetOutputConstant(1);
        for (int i = 0; i < 2; ++i) _strategy.ChooseBestField(game);
        Assert.AreEqual(KniffelGame.VALUE_KNIFFEL + 3 * 5 + 2 * 15,
            game.CurrentPlayer[KniffelPointsTable.INDEX_SUM].Value);
    }

    [TestMethod]
    public void TestBotFinderIndex0()
    {
        BotStrategy strategy = new BotStrategy(0);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([0, 2], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    [TestMethod]
    public void TestBotFinderIndex1()
    {
        BotStrategy strategy = new BotStrategy(1);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([0, 2], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    [TestMethod]
    public void TestBotFinderIndex2()
    {
        BotStrategy strategy = new BotStrategy(2);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([0, 1, 2, 4], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    [TestMethod]
    public void TestBotFinderIndex3()
    {
        BotStrategy strategy = new BotStrategy(3);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([0, 1, 2], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    [TestMethod]
    public void TestBotFinderIndex4()
    {
        BotStrategy strategy = new BotStrategy(4);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([0, 1, 2], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    [TestMethod]
    public void TestBotFinderIndex5()
    {
        BotStrategy strategy = new BotStrategy(5);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    private void AssertAreEqual(int[] expected, int[] actual)
    {
        Assert.IsTrue(Comparer.Equals(expected, actual), "Expected: {0}, Actual: {1}", ArrayString(expected),
            ArrayString(actual));
    }

    private static string ArrayString(IEnumerable<int> array) => string.Join(", ", array);

    [TestMethod]
    public void TestBotFinderIndex6()
    {
        BotStrategy strategy = new BotStrategy(6);
        KniffelGame game = CreateGame(2, 3, 3, 6, 5);
        AssertAreEqual([1, 2, 3, 4], strategy.GenerateIndexToShuffleForNextBestMove(game));
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestExceptionThrownWhenNoMoveLeft()
    {
        KniffelGame game = CreateGame();
        game.Shuffle();
        game.Shuffle();
        _strategy.GenerateIndexToShuffleForNextBestMove(game);
    }

    [TestMethod]
    public void TestToString()
    {
        Assert.AreEqual(
            "Not reached: 0, 13, 9, 12, 10, 11, 8, 1, 2, 3, 4, 5" +
            "\nreached: 0, 1, 2, 13, 9, 12, 10, 11, 8, 3, 4, 5" +
            "\nIndex: 4" +
            "\nMin values: {Chance 15, Pair 3: 14, Pair 4: 13}",
            _strategy.ToString());
    }

    [TestMethod]
    public void TestChoosesSmallStreet()
    {
        KniffelGame game = CreateGame(3, 4, 5, 6, 6);
        _strategy.ChooseBestField(game);
        _strategy.ChooseBestField(game);
        Assert.AreEqual(KniffelGame.VALUE_SMALL_STREET,
            game.CurrentPlayer[KniffelPointsTable.INDEX_SMALL_STREET].Value);
    }

    private static FlatDice CreateDice(params int[] values) => new(new RandomStub(values));

    private static KniffelGame CreateGame(params int[] values) =>
        new(Players, new RandomStub(new[] { 1, 1, 1, 1, 1 }.Concat(values)));
}