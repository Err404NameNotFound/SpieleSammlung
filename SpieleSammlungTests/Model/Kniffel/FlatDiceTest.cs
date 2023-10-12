using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlungTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlungTests.Model.Kniffel
{
    [TestClass]
    public class FlatDiceTest
    {
        private static readonly HashSet<int[]> AllCombinations;
        private static readonly HashSet<int[]> KniffelCombinations;
        private const double DELTA = 0.00000000001;
        private readonly RandomStub _rng;
        private readonly FlatDice _dice;
        private static KniffelPlayer _player;

        public FlatDiceTest()
        {
            _rng = new RandomStub();
            _dice = new FlatDice(_rng);
        }

        static FlatDiceTest()
        {
            AllCombinations = new HashSet<int[]>(new IntArrayComparer());
            int[] dice = new int[Dice.DICE_COUNT];
            for (int i = 0; i < dice.Length; ++i)
            {
                dice[i] = Dice.LOWEST_VALUE;
            }

            int end = (int)(Math.Pow(Dice.VALUE_SPAN, Dice.DICE_COUNT) - 1);
            AllCombinations.Add((int[])dice.Clone());
            const int highest = Dice.HIGHEST_VALUE - 1;
            int startDice = dice.Length - 1;
            for (int i = 0; i < end; ++i)
            {
                int index = startDice;
                while (dice[index] == highest)
                {
                    dice[index] = Dice.LOWEST_VALUE;
                    --index;
                }

                ++dice[index];
                AllCombinations.Add((int[])dice.Clone());
            }

            KniffelCombinations = new HashSet<int[]>(new IntArrayComparer());
            for (int i = 1; i < 7; ++i)
            {
                KniffelCombinations.Add(ArrayHelp.CreateIntArray(Dice.DICE_COUNT, i));
            }
        }

        [TestInitialize]
        public void Initialise()
        {
            _rng.ClearOutputConstant();
            _rng.ClearQueues();
            _player = new KniffelPlayer(new Player());
        }

        [TestMethod]
        public void TestKniffelPossible()
        {
            for (int i = 1; i < 7; ++i)
            {
                _rng.SetOutputConstant(i);
                _dice.Shuffle();
                Assert.IsTrue(_dice.IsKniffelPossible(),
                    "Kniffel should be possible with all values the same, dice: {0}", _dice);
            }
        }

        [TestMethod]
        public void TestKniffelNotPossible()
        {
            foreach (var values in AllCombinations.Where(values => !KniffelCombinations.Contains(values)))
            {
                _rng.SetNext(values);
                _dice.Shuffle();
                Assert.IsFalse(_dice.IsKniffelPossible(),
                    "Kniffel should not be possible without all values being the same, dice: {0}", _dice);
            }
        }

        [TestMethod]
        public void TestBigStreetPossible()
        {
            int[][] inputs =
            {
                new[] { 1, 2, 3, 4, 5 }, new[] { 2, 3, 4, 5, 6 },
                new[] { 5, 4, 3, 2, 1 }, new[] { 6, 5, 4, 3, 2 },
                new[] { 4, 5, 1, 3, 2 }, new[] { 6, 2, 5, 3, 4 },
                new[] { 5, 4, 3, 1, 2 }, new[] { 4, 6, 5, 2, 3 }
            };
            const string message = "Big Street should be possible with dice: {0}";
            TestAllCombinations(inputs, true, message, dice => dice.IsBigStreetPossible());
        }

        [TestMethod]
        public void TestBigStreetNotPossible()
        {
            // This method does not test every wrong combination like 1*1*4*5*6=120 although this combination is no big street
            const string message = "Big Street should not be possible with dice: {0}";
            foreach (int[] values in AllCombinations)
            {
                int product = values[0];
                for (int i = 1; i < values.Length; ++i)
                {
                    product *= values[i];
                }

                if (product != 120 && product != 720)
                {
                    _rng.SetNext(values);
                    _dice.Shuffle();
                    Asserter(_dice);
                }
            }

            int[][] inputs = { new[] { 1, 1, 4, 5, 6 }, new[] { 1, 2, 2, 5, 6 }, new[] { 2, 2, 2, 3, 5 } };
            TestAllCombinations(inputs, Asserter);
            return;
            void Asserter(FlatDice d) => Assert.IsFalse(d.IsBigStreetPossible(), string.Format(message, d));
        }

        [TestMethod]
        public void TestSmallStreetPossible()
        {
            int[][] inputs =
            {
                new[] { 1, 2, 3, 4, 2 }, new[] { 2, 3, 4, 5, 6 },
                new[] { 1, 4, 3, 2, 1 }, new[] { 6, 5, 4, 3, 2 },
                new[] { 4, 5, 1, 3, 2 }, new[] { 6, 6, 5, 3, 4 },
                new[] { 5, 4, 3, 1, 2 }, new[] { 4, 2, 5, 2, 3 }
            };
            const string message = "Small Street should be possible with dice: {0}";
            TestAllCombinations(inputs, true, message, dice => dice.IsSmallStreetPossible());
        }
        
        [TestMethod]
        public void TestSmallStreetNotPossible()
        {
            // This method does not test every wrong combination like 1*1*4*5*6=120 although this combination is no small street
            const string message = "Small Street should not be possible with dice: {0}";
            foreach (int[] values in AllCombinations)
            {
                int product = values[0];
                for (int i = 1; i < values.Length; ++i)
                {
                    product *= values[i];
                }

                if (product % 24 > 0)
                {
                    _rng.SetNext(values);
                    _dice.Shuffle();
                    Asserter(_dice);
                }
            }

            int[][] inputs = { new[] { 1, 1, 4, 5, 6 }, new[] { 1, 2, 2, 5, 6 }, new[] { 1, 2, 2, 3, 5 } };
            TestAllCombinations(inputs, Asserter);
            return;
            void Asserter(FlatDice d) => Assert.IsFalse(d.IsSmallStreetPossible(), string.Format(message, d));
        }

        [TestMethod]
        public void TestFullHousePossible()
        {
            int[][] inputs =
            {
                new[] { 1, 2, 1, 1, 2 }, new[] { 3, 3, 4, 4, 4 },
                new[] { 1, 4, 1, 4, 1 }, new[] { 6, 5, 5, 6, 5 },
                new[] { 2, 5, 5, 5, 2 }, new[] { 5, 4, 5, 5, 4 },
                new[] { 3, 3, 3, 2, 2 }, new[] { 6, 6, 6, 5, 5 }
            };
            const string message = "Full House should be possible with dice: {0}";
            TestAllCombinations(inputs, true, message, dice => dice.IsFullHousePossible());
        }

        [TestMethod]
        public void TestFullHouseNotPossible()
        {
            // This method does not test every wrong combination
            const string message = "FullHouse should not be possible with dice: {0}";
            foreach (int[] values in AllCombinations)
            {
                _rng.SetNext(values);
                _dice.Shuffle();
                if (!_dice.IsPairOfSizePossible(3) || _dice.IsPairOfSizePossible(4))
                {
                    Asserter(_dice);
                }
            }

            int[][] inputs = { new[] { 1, 1, 4, 5, 6 }, new[] { 1, 2, 2, 5, 6 }, new[] { 1, 2, 2, 3, 5 } };
            TestAllCombinations(inputs, Asserter);
            return;
            void Asserter(FlatDice d) => Assert.IsFalse(d.IsFullHousePossible(), string.Format(message, d));
        }

        [TestMethod]
        public void TestShuffleAll()
        {
            RandomStub rng = new RandomStub(1, 2, 3, 4, 5);
            FlatDice dice = new FlatDice(rng);
            rng.SetNext(3, 5, 2, 4, 6);
            dice.Shuffle();
            Assert.AreEqual("{ 3, 5, 2, 4, 6 }", dice.ToString());
        }

        [TestMethod]
        public void TestShufflePartially()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(1, 2, 3, 4, 5);
            FlatDice dice = new FlatDice(rng);
            rng.SetNext(5, 2);
            dice.Shuffle(new[] { 1, 4 });
            Assert.AreEqual("{ 1, 5, 3, 4, 2 }", dice.ToString());
        }

        [TestMethod]
        public void TestGenerateDiceValue()
        {
            RandomStub rng = new RandomStub();
            FlatDice dice = new FlatDice(rng);
            rng.SetNext(3, 2, 4, 1);
            Assert.AreEqual(3, dice.GenerateDiceValue());
            Assert.AreEqual(2, dice.GenerateDiceValue());
            Assert.AreEqual(4, dice.GenerateDiceValue());
            Assert.AreEqual(1, dice.GenerateDiceValue());
        }
        
        [TestMethod]
        public void TestSumOfAllDiceCorrect()
        {
            RandomStub stub = new RandomStub();
            FlatDice manager = new FlatDice(stub);
            foreach (int[] dice in AllCombinations)
            {
                stub.SetNext(dice);
                manager.Shuffle();
                int sum = 0;
                Array.ForEach(dice, i => sum += i);
                Assert.AreEqual(sum, manager.SumOfAllDices);
            }
        }

        [TestMethod]
        public void TestCountsSumUpToDiceCount()
        {
            RandomStub stub = new RandomStub();
            FlatDice manager = new FlatDice(stub);
            foreach (int[] dice in AllCombinations)
            {
                stub.SetNext(dice);
                manager.Shuffle();
                int sum = 0;
                for (int i = 0; i < Dice.VALUE_SPAN; ++i)
                {
                    sum += manager.GetCountOfValue(i);
                }

                Assert.AreEqual(Dice.DICE_COUNT, sum);
            }
        }

        [TestMethod]
        public void TestCountsCorrectAllSame()
        {
            RandomStub stub = new RandomStub();
            FlatDice manager = new FlatDice(stub);
            for (int i = 0; i < Dice.VALUE_SPAN; ++i)
            {
                stub.SetOutputConstant(i + Dice.LOWEST_VALUE);
                manager.Shuffle();
                Assert.AreEqual(Dice.DICE_COUNT, manager.GetCountOfValue(i));
            }
        }

        [TestMethod]
        public void TestCountsCorrectAllSingle()
        {
            RandomStub stub = new RandomStub();
            FlatDice manager = new FlatDice(stub);
            stub.SetNext(3, 4, 1, 5, 2);
            manager.Shuffle();
            Assert.AreEqual(1, manager.GetCountOfValue(0));
            Assert.AreEqual(1, manager.GetCountOfValue(1));
            Assert.AreEqual(1, manager.GetCountOfValue(2));
            Assert.AreEqual(1, manager.GetCountOfValue(3));
            Assert.AreEqual(1, manager.GetCountOfValue(4));
            Assert.AreEqual(0, manager.GetCountOfValue(5));
        }

        [TestMethod]
        public void TestAllDiceIndex()
        {
            int[] expected = { 0, 1, 2, 3, 4 };
            int[] actual = FlatDice.AllDices;
            Assert.IsTrue(new IntArrayComparer().Equals(expected, actual), "expected: {0}, actual: {1}",
                string.Join(", ", expected), string.Join(", ", actual));
        }

        [TestMethod]
        public void TestPairOfSizePossible()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(3, 4, 2, 1, 5 /*1*/, 3, 4, 4, 3, 6 /*2*/, 2, 5, 6, 5, 5 /*3*/, 6, 1, 1, 1, 1 /*4*/,
                6, 6, 6, 6, 6 /*5*/);
            FlatDice dice = new FlatDice(rng);
            for (int i = 0; i < 5; ++i)
            {
                for (int e = 0; e <= i; ++e)
                {
                    Assert.IsTrue(dice.IsPairOfSizePossible(e + 1),
                        "A pair of size {0} should be possible for dice: {1}", e + 1, dice);
                }

                for (int e = i + 1; e < 5; ++e)
                {
                    Assert.IsFalse(dice.IsPairOfSizePossible(e + 1),
                        "A pair of size {0} should not be possible for dice: {1}", e + 1, dice);
                }

                dice.Shuffle();
            }
        }
        
        [TestMethod]
        public void TestAlwaysAll32OptionCalculated()
        {
            FlatDice dice = new FlatDice();
            Assert.AreEqual(32, FlatDice.GenerateAllOptions(_player, dice).Count);
        }

        [TestMethod]
        public void TestExpectedValuesCalculatedCorrectAllSet()
        {
            _rng.SetNext(5, 5, 5, 5, 5);
            _dice.Shuffle();
            var options = FlatDice.CalculateExpectedValues(_player, _dice);
            Assert.AreEqual(0, options[0].Value);
            Assert.AreEqual(25, options[4].Value);
            Assert.AreEqual(25, options[6].Value);
            Assert.AreEqual(25, options[7].Value);
            Assert.AreEqual(0, options[9].Value);
            Assert.AreEqual(50, options[11].Value);
            Assert.AreEqual(25, options[12].Value);
        }

        [TestMethod]
        public void TestExpectedValuesCalculatedCorrectSomeNotSet()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            // _dice.Remove(1, 4);
            var options = FlatDice.GenerateAllOptions(_player, _dice)[22];
            // var options = FlatDice.CalculateExpectedValues(_player, _dice);
            Assert.AreEqual(1 + 2.0 / 6, options[0].ValueD, DELTA);
            Assert.AreEqual(4.0 / 6, options[1].ValueD, DELTA);
            Assert.AreEqual((10 + 14 + 16) / 36.0, options[6].ValueD, DELTA);
            Assert.AreEqual(30 * 13 / 36.0, options[7].ValueD);
            Assert.AreEqual(40 * 1.0 / 18, options[8].ValueD);
            Assert.AreEqual(8 + 7, options[9].ValueD);
        }

        [TestMethod]
        public void TestContains32DifferentOptions()
        {
            FlatDice dice = new FlatDice();
            HashSet<string> set = new HashSet<string>();
            var options = FlatDice.GenerateAllOptions(_player, dice);
            foreach (var d in options)
            {
                set.Add(d.ToString());
            }

            Assert.AreEqual(32, set.Count);
        }

        [TestMethod]
        public void TestAllOptionsHaveSameRightLength()
        {
            var options = FlatDice.GenerateAllOptions(_player, _dice);
            foreach (var option in options)
            {
                Assert.AreEqual(KniffelPointsTable.WRITEABLE_FIELDS_COUNT, option.Count);
            }
        }

        [TestMethod]
        public void TestNotFeasibleOptionsAreRemoved()
        {
            _player[KniffelPointsTable.INDEX_KNIFFEL].Value = 50;
            _player[KniffelPointsTable.INDEX_BIG_STREET].Value = 40;
            var options = FlatDice.GenerateAllOptions(_player, _dice);
            foreach (var option in options)
            {
                Assert.AreEqual(KniffelPointsTable.WRITEABLE_FIELDS_COUNT - 2, option.Count);
            }
        }

        [TestMethod]
        public void TestOnlyOneOptionRemaining()
        {
            WriteEverythingExceptChance(_player);
            var options = FlatDice.GenerateAllOptions(_player, _dice);
            foreach (var option in options)
            {
                Assert.AreEqual(1, option.Count);
            }
        }

        [TestMethod]
        public void TestOnlyNoOptionRemaining()
        {
            WriteEverythingExceptChance(_player);
            _player[KniffelPointsTable.INDEX_CHANCE].Value = 30;
            var options = FlatDice.GenerateAllOptions(_player, _dice);
            foreach (var option in options)
            {
                Assert.AreEqual(0, option.Count);
            }
        }
        
        #region HelpMethods
        
        private static void TestAllCombinations(IEnumerable<int[]> inputs, object expected, string messageTemplate,
            CalculateActual2 calculate)
        {
            RandomStub rng = new RandomStub();
            FlatDice dice = new FlatDice(rng);
            TestAllCombinations(rng, dice, inputs,
                a => Assert.AreEqual(expected, calculate(a), string.Format(messageTemplate, dice)));
        }

        private static void TestAllCombinations(IEnumerable<int[]> inputs, AssertCombinationOk2 combinationOk)
        {
            RandomStub rng = new RandomStub();
            FlatDice dice = new FlatDice(rng);
            TestAllCombinations(rng, dice, inputs, combinationOk);
        }

        private static void TestAllCombinations(RandomStub rng, FlatDice dice, IEnumerable<int[]> inputs,
            AssertCombinationOk2 combinationOk)
        {
            foreach (int[] values in inputs)
            {
                rng.SetNext(values);
                dice.Shuffle();
                combinationOk(dice);
            }
        }

        private static void WriteEverythingExceptChance(KniffelPlayer player)
        {
            for (int i = 0; i < 6; ++i)
            {
                player[i].Value = 3 * i;
            }

            player[KniffelPointsTable.INDEX_PAIR_SIZE_3].Value = 30;
            player[KniffelPointsTable.INDEX_PAIR_SIZE_4].Value = 30;
            player[KniffelPointsTable.INDEX_FULL_HOUSE].Value = 25;
            player[KniffelPointsTable.INDEX_SMALL_STREET].Value = 30;
            player[KniffelPointsTable.INDEX_BIG_STREET].Value = 40;
            player[KniffelPointsTable.INDEX_KNIFFEL].Value = 50;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestThrowsExceptionWhenBigStreetNotPossible()
        {
            _rng.SetNext(1, 2, 3, 3, 3);
            _dice.Shuffle();
            _dice.IndexToFlipWhenOptimisingToBigStreet();
        }

        #endregion
    }

    public delegate object CalculateActual2(FlatDice dice);

    public delegate void AssertCombinationOk2(FlatDice dice);
}