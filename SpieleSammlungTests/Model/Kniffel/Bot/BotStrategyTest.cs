using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Kniffel.Bot
{
    [TestClass]
    public class BotStrategyTest
    {
        private readonly BotStrategy _strategy = new();
        private readonly KniffelPlayer _player = new(new Player());
        private static readonly List<Player> Players = new() { new Player(), new Player() };

        [TestMethod]
        public void TestTriesToImproveToBigStreetShuffle1()
        {
            DiceManager dice = CreateDice(3, 4, 5, 6, 1);
            int[] shuffle = _strategy.GenerateIndexToShuffleForNextBestMove(dice, _player);
            Assert.AreEqual(1, shuffle.Length);
            Assert.AreEqual(1, dice[shuffle[0]]);
        }

        [TestMethod]
        public void TestTriesToImproveToBigStreetShuffle6()
        {
            DiceManager dice = CreateDice(3, 4, 5, 6, 6);
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
        [ExpectedException(typeof(NotSupportedException))]
        public void TestExceptionThrownWhenNoMoveLeft()
        {
            KniffelGame game = CreateGame();
            game.Shuffle();
            game.Shuffle();
            _strategy.GenerateIndexToShuffleForNextBestMove(game);
        }

        private static DiceManager CreateDice(params int[] values) => new(new RandomStub(values));

        private static KniffelGame CreateGame(params int[] values) =>
            new(Players, new RandomStub(new[] { 1, 1, 1, 1, 1 }.Concat(values)));
    }
}