using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlungTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel.Fields;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace SpieleSammlungTests.Model.Kniffel
{
    [TestClass]
    public class DiceManagerTest
    {
        private static readonly HashSet<int[]> AllCombinations;
        private static readonly HashSet<int[]> KniffelCombinations;
        private const double DELTA = 0.00000000001;
        private readonly RandomStub _rng;
        private readonly DiceManager _dice;
        private static KniffelPlayer _player;

        public DiceManagerTest()
        {
            _rng = new RandomStub();
            _dice = new DiceManager(_rng);
        }

        static DiceManagerTest()
        {
            AllCombinations = new HashSet<int[]>(new Comparer());
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

            KniffelCombinations = new HashSet<int[]>(new Comparer());
            for (int i = 1; i < 7; ++i)
            {
                KniffelCombinations.Add(ArrayHelp.CreateIntArray(Dice.DICE_COUNT, i));
            }
        }

        [TestInitialize]
        public void Initialise()
        {
            _rng.ClearOutputConstant();
            _rng.ClearQueue();
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
            void Asserter(DiceManager d) => Assert.IsFalse(d.IsBigStreetPossible(), string.Format(message, d));
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
            void Asserter(DiceManager d) => Assert.IsFalse(d.IsSmallStreetPossible(), string.Format(message, d));
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
            void Asserter(DiceManager d) => Assert.IsFalse(d.IsFullHousePossible(), string.Format(message, d));
        }

        [TestMethod]
        public void TestShuffleAll()
        {
            RandomStub rng = new RandomStub(1, 2, 3, 4, 5);
            DiceManager dice = new DiceManager(rng);
            rng.SetNext(3, 5, 2, 4, 6);
            dice.Shuffle();
            Assert.AreEqual("{ 3, 5, 2, 4, 6 }", dice.ToString());
        }

        [TestMethod]
        public void TestShufflePartially()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(1, 2, 3, 4, 5);
            DiceManager dice = new DiceManager(rng);
            rng.SetNext(5, 2);
            dice.Shuffle(new[] { 1, 4 });
            Assert.AreEqual("{ 1, 5, 3, 4, 2 }", dice.ToString());
        }

        [TestMethod]
        public void TestGenerateDiceValue()
        {
            RandomStub rng = new RandomStub();
            DiceManager dice = new DiceManager(rng);
            rng.SetNext(3, 2, 4, 1);
            Assert.AreEqual(3, dice.GenerateDiceValue());
            Assert.AreEqual(2, dice.GenerateDiceValue());
            Assert.AreEqual(4, dice.GenerateDiceValue());
            Assert.AreEqual(1, dice.GenerateDiceValue());
        }

        [TestMethod]
        public void TestConstants()
        {
            Assert.AreEqual(3.5, DiceManager.AVERAGE_VALUE);
            Assert.AreEqual(5, Dice.DICE_COUNT);
            Assert.AreEqual(7, Dice.HIGHEST_VALUE);
            Assert.AreEqual(1, Dice.LOWEST_VALUE);
            Assert.AreEqual(6, Dice.VALUE_SPAN);
        }

        [TestMethod]
        public void TestSumOfAllDiceCorrect()
        {
            RandomStub stub = new RandomStub();
            DiceManager manager = new DiceManager(stub);
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
            DiceManager manager = new DiceManager(stub);
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
            DiceManager manager = new DiceManager(stub);
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
            DiceManager manager = new DiceManager(stub);
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
            int[] actual = DiceManager.AllDices;
            Assert.IsTrue(new Comparer().Equals(expected, actual), "expected: {0}, actual: {1}",
                string.Join(", ", expected), string.Join(", ", actual));
        }

        [TestMethod]
        public void TestPairOfSizePossible()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(3, 4, 2, 1, 5 /*1*/, 3, 4, 4, 3, 6 /*2*/, 2, 5, 6, 5, 5 /*3*/, 6, 1, 1, 1, 1 /*4*/, 6, 6, 6, 6,
                6 /*5*/);
            DiceManager dice = new DiceManager(rng);
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
        [ExpectedException(typeof(ArgumentException))]
        public void TestUnsetSameTwiceCausesException()
        {
            DiceManager dice = new DiceManager();
            dice.Remove(0);
            dice.Remove(0);
        }

        [TestMethod]
        public void TestUnsetDice()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(3, 4, 2, 5, 1);
            DiceManager dice = new DiceManager(rng);
            dice.Remove(0);
            dice.Remove(3);
            Assert.AreEqual("{ _, 4, 2, _, 1 }", dice.ToString());
            Assert.AreEqual(7, dice.SumOfAllDices);
        }

        [TestMethod]
        public void TestUnsetDiceGetIndices()
        {
            DiceManager dice = new DiceManager();
            dice.Remove(2);
            dice.Remove(4);
            Assert.AreEqual("2, 4", string.Join(", ", dice.GetUnsetDiceIndex()));
        }

        [TestMethod]
        public void TestGetDiceViaIndex()
        {
            _rng.SetNext(2, 5, 3, 1, 2);
            _dice.Shuffle();
            Assert.AreEqual(2, _dice[0]);
            Assert.AreEqual(5, _dice[1]);
            Assert.AreEqual(1, _dice[3]);
        }

        [TestMethod]
        public void TestEOfTop6WithoutRemove()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(5, 4, 6, 3, 2);
            DiceManager dice = new DiceManager(rng);
            Assert.AreEqual(0.0, dice.EOfTop6(1));
            Assert.AreEqual(5.0, dice.EOfTop6(5));
            Assert.AreEqual(3.0, dice.EOfTop6(3));
        }

        [TestMethod]
        public void TestEOfTop6WithRemove()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(5, 4, 6, 3, 2);
            DiceManager dice = new DiceManager(rng);
            dice.Remove(0);
            Assert.AreEqual(DiceManager.PROBABILITY, dice.EOfTop6(1));
            Assert.AreEqual(5.0 * DiceManager.PROBABILITY, dice.EOfTop6(5));
            Assert.AreEqual(7.0, dice.EOfTop6(6));
        }

        [TestMethod]
        public void TestEOfTop6WithRemove2()
        {
            RandomStub rng = new RandomStub();
            rng.SetNext(5, 4, 6, 3, 3);
            DiceManager dice = new DiceManager(rng);
            dice.Remove(1);
            dice.Remove(4);
            Assert.AreEqual(4.0 * DiceManager.PROBABILITY, dice.EOfTop6(2));
            Assert.AreEqual(2 * 4.0 * DiceManager.PROBABILITY, dice.EOfTop6(4));
            Assert.AreEqual(4.0, dice.EOfTop6(3));
        }

        [TestMethod]
        public void TestEOfTop6WithRemoveAll()
        {
            DiceManager dice = new DiceManager();
            RemoveAllDice(dice);
            Assert.AreEqual(5.0, dice.EOfTop6(6));
            Assert.AreEqual(2.5, dice.EOfTop6(3));
        }

        [TestMethod]
        public void TestEOfChanceAllSet()
        {
            _rng.SetNext(4, 5, 2, 4, 2, 5, 2, 4, 4, 4, 2, 6, 5, 3, 1, 5, 3, 6, 2, 1, 4, 1, 3, 5, 6, 2, 5, 3, 2, 4, 5);
            for (int i = 0; i < 5; ++i)
            {
                Assert.AreEqual(_dice.SumOfAllDices, _dice.EOfChance());
                _dice.Shuffle();
            }
        }

        [TestMethod]
        public void TestEOfChanceUnset3()
        {
            _rng.SetNext(4, 5, 2, 4, 2);
            _dice.Shuffle();
            _dice.Remove(1, 3, 4);
            Assert.AreEqual(16.5, _dice.EOfChance());
        }

        [TestMethod]
        public void TestEOfChanceUnsetAll()
        {
            DiceManager dice = GenerateUnsetDiceManager();
            Assert.AreEqual(17.5, dice.EOfChance());
        }

        [TestMethod]
        public void TestEOfPair3AllSetPossible()
        {
            int[][] values =
                { new[] { 1, 1, 3, 3, 3 }, new[] { 1, 1, 1, 1, 1 }, new[] { 6, 6, 6, 6, 1 }, new[] { 3, 4, 2, 4, 4 } };
            TestAllCombinations(values, dice => Assert.AreEqual(dice.SumOfAllDices, dice.EOfPair(3)));
        }

        [TestMethod]
        public void TestEOfPair4AllSetPossible()
        {
            int[][] values =
                { new[] { 3, 1, 3, 3, 3 }, new[] { 1, 1, 1, 1, 1 }, new[] { 6, 6, 6, 6, 1 }, new[] { 4, 4, 2, 4, 4 } };
            TestAllCombinations(values, dice => Assert.AreEqual(dice.SumOfAllDices, dice.EOfPair(4)));
        }

        [TestMethod]
        public void TestEOfPair3AllSetNotPossible()
        {
            int[][] values =
                { new[] { 1, 1, 3, 3, 2 }, new[] { 1, 3, 4, 5, 6 }, new[] { 6, 6, 4, 5, 1 }, new[] { 3, 4, 2, 3, 4 } };
            TestAllCombinations(values, dice => Assert.AreEqual(0, dice.EOfPair(3)));
        }

        [TestMethod]
        public void TestEOfPair4AllSetNotPossible()
        {
            int[][] values =
                { new[] { 3, 1, 5, 3, 3 }, new[] { 1, 6, 5, 1, 1 }, new[] { 6, 6, 3, 6, 1 }, new[] { 4, 3, 2, 4, 4 } };
            TestAllCombinations(values, dice => Assert.AreEqual(0, dice.EOfPair(4)));
        }

        [TestMethod]
        public void TestEOfPair3SomeSetNotPossible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(4);
            Assert.AreEqual(0, _dice.EOfPair(3), DELTA);
        }

        [TestMethod]
        public void TestEOfPair4SomeSetNotPossible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(4);
            Assert.AreEqual(0, _dice.EOfPair(4), DELTA);
            _rng.SetNext(1, 1, 2, 2, 2);
            _dice.Shuffle();
            _dice.Remove(4);
            Assert.AreEqual(0, _dice.EOfPair(4), DELTA);
        }

        [TestMethod]
        public void TestEOfPair3SomeSetPossible()
        {
            _rng.SetNext(1, 2, 2, 4, 5);
            _dice.Shuffle();
            _dice.Remove(3);
            Assert.AreEqual((1 + 2 + 2 + 2 + 5) / 6.0, _dice.EOfPair(3));
        }

        [TestMethod]
        public void TestEOfPair4SomeSetPossible()
        {
            _rng.SetNext(1, 5, 5, 4, 5);
            _dice.Shuffle();
            _dice.Remove(3);
            Assert.AreEqual((1 + 3 * 5 + 5) / 6.0, _dice.EOfPair(4));
        }

        [TestMethod]
        public void TestEOfPair3SomeSetPossibleFlexible()
        {
            _rng.SetNext(1, 2, 2, 4, 5);
            _dice.Shuffle();
            _dice.Remove(0, 3);
            Assert.AreEqual(180 / 36.0, _dice.EOfPair(3));
        }

        [TestMethod]
        public void TestEOfPair4SomeSetPossibleFlexible()
        {
            _rng.SetNext(6, 6, 6, 6, 6);
            _dice.Shuffle();
            _dice.Remove(1, 4);
            Assert.AreEqual(300 / 36.0, _dice.EOfPair(4), DELTA);
        }

        [TestMethod]
        public void TestEOfPair3NoneSet()
        {
            DiceManager dice = GenerateUnsetDiceManager();
            Assert.AreEqual(805.0 / 216, dice.EOfPair(3));
        }

        [TestMethod]
        public void TestEOfPair4NoneSet()
        {
            DiceManager dice = GenerateUnsetDiceManager();
            Assert.AreEqual(455.0 / 1296, dice.EOfPair(4), DELTA);
        }

        [TestMethod]
        public void TestPOfKniffelNoneSet()
        {
            DiceManager dice = GenerateUnsetDiceManager();
            Assert.AreEqual(Math.Pow(1.0 / 6, 4), dice.POfKniffel(), DELTA);
        }

        [TestMethod]
        public void TestPOfKniffelAllSetPossible()
        {
            foreach (int[] values in KniffelCombinations)
            {
                _rng.SetNext(values);
                _dice.Shuffle();
                Assert.AreEqual(1, _dice.POfKniffel());
            }
        }

        [TestMethod]
        public void TestPOfKniffelAllSetNotPossible()
        {
            int[][] values =
                { new[] { 1, 2, 3, 4, 5 }, new[] { 1, 1, 1, 1, 2 }, new[] { 6, 6, 6, 6, 1 }, new[] { 3, 4, 2, 3, 4 } };
            TestAllCombinations(values, dice => Assert.AreEqual(0, dice.POfKniffel()));
        }

        [TestMethod]
        public void TestPOfKniffelSomeSetPossible()
        {
            _rng.SetNext(1, 1, 1, 1, 1);
            _dice.Shuffle();
            _dice.Remove(4);
            Assert.AreEqual(1.0 / 6, _dice.POfKniffel(), DELTA);
            _rng.SetNext(2, 2, 2, 2, 2);
            _dice.Shuffle();
            _dice.Remove(2, 4);
            Assert.AreEqual(1.0 / 36, _dice.POfKniffel(), DELTA);
            _rng.SetNext(3, 3, 3, 3, 3);
            _dice.Shuffle();
            _dice.Remove(1, 2, 4);
            Assert.AreEqual(1.0 / 216.0, _dice.POfKniffel(), DELTA);
            _rng.SetNext(4, 4, 4, 4, 4);
            _dice.Shuffle();
            _dice.Remove(1, 2, 3, 4);
            Assert.AreEqual(1.0 / 1296.0, _dice.POfKniffel(), DELTA);
        }

        [TestMethod]
        public void TestPOfKniffelSomeSetNotPossible()
        {
            int[][] values =
                { new[] { 1, 2, 3, 4, 5 }, new[] { 1, 1, 1, 1, 2 }, new[] { 6, 6, 6, 6, 1 }, new[] { 3, 4, 2, 3, 4 } };
            foreach (int[] dValue in values)
            {
                _rng.SetNext(dValue);
                _dice.Shuffle();
                _dice.Remove(3);
                Assert.AreEqual(0, _dice.POfKniffel());
            }
        }

        [TestMethod]
        public void TestPOfBigStreetAllSetPossible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            Assert.AreEqual(1, _dice.POfBigStreet());
            _rng.SetNext(4, 6, 3, 2, 5);
            _dice.Shuffle();
            Assert.AreEqual(1, _dice.POfBigStreet());
        }

        [TestMethod]
        public void TestPOfBigStreetAllSetNotPossible()
        {
            _rng.SetNext(1, 2, 4, 5, 6);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfBigStreet());
            _rng.SetNext(4, 5, 3, 2, 5);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfBigStreet());
        }

        [TestMethod]
        public void TestPOfBigStreetNoneSet()
        {
            Assert.AreEqual(2 * Probabilities.Faculty(5) / 7776.0, GenerateUnsetDiceManager().POfBigStreet());
        }

        [TestMethod]
        public void TestPOfBigStreetSomeSetPossible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(3);
            Assert.AreEqual(1.0 / 6, _dice.POfBigStreet());
            _rng.SetNext(4, 6, 3, 2, 5);
            _dice.Shuffle();
            _dice.Remove(2, 4);
            Assert.AreEqual(1.0 / 18, _dice.POfBigStreet());
        }

        [TestMethod]
        public void TestPOfBigStreetSomeSetPossibleFlexible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(0);
            Assert.AreEqual(2.0 / 6, _dice.POfBigStreet());
        }

        [TestMethod]
        public void TestPOfBigStreetSomeSetNotPossible()
        {
            _rng.SetNext(3, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(1);
            Assert.AreEqual(0, _dice.POfBigStreet());
            _rng.SetNext(4, 6, 3, 1, 5);
            _dice.Shuffle();
            _dice.Remove(2, 4);
            Assert.AreEqual(0, _dice.POfBigStreet());
        }

        [TestMethod]
        public void TestPOfSmallStreetAllSetPossible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            Assert.AreEqual(1, _dice.POfSmallStreet());
            _rng.SetNext(4, 6, 3, 4, 5);
            _dice.Shuffle();
            Assert.AreEqual(1, _dice.POfSmallStreet());
        }

        [TestMethod]
        public void TestPOfSmallStreetAllSetNotPossible()
        {
            _rng.SetNext(1, 2, 4, 5, 6);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfSmallStreet());
            _rng.SetNext(4, 5, 3, 3, 5);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfSmallStreet());
        }

        [TestMethod]
        public void TestPOfSmallStreetNoneSet()
        {
            Assert.AreEqual(1200 / 7776.0, GenerateUnsetDiceManager().POfSmallStreet());
        }

        [TestMethod]
        public void TestPOfSmallStreetSomeSetPossible()
        {
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(0);
            Assert.AreEqual(1.0, _dice.POfSmallStreet());
            _rng.SetNext(1, 2, 3, 4, 5);
            _dice.Shuffle();
            _dice.Remove(1);
            Assert.AreEqual(2.0 / 6, _dice.POfSmallStreet());
            _rng.SetNext(4, 6, 3, 2, 5);
            _dice.Shuffle();
            _dice.Remove(1, 4);
            Assert.AreEqual((2 * 6 + 4 * 2) / 36.0, _dice.POfSmallStreet());
        }

        [TestMethod]
        public void TestPOfSmallStreetSomeSetNotPossibleClear()
        {
            _rng.SetNext(3, 3, 3, 4, 4);
            _dice.Shuffle();
            _dice.Remove(3);
            Assert.AreEqual(0, _dice.POfSmallStreet());
        }

        [TestMethod]
        public void TestPOfSmallStreetSomeSetNotPossible()
        {
            _rng.SetNext(3, 2, 3, 4, 6);
            _dice.Shuffle();
            _dice.Remove(3);
            Assert.AreEqual(0, _dice.POfSmallStreet());
            _rng.SetNext(4, 6, 3, 1, 6);
            _dice.Shuffle();
            _dice.Remove(0, 2);
            Assert.AreEqual(0, _dice.POfSmallStreet());
        }

        [TestMethod]
        public void TestPOfFullHouseAllSetNotPossible()
        {
            _rng.SetNext(1, 2, 4, 5, 3);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfFullHouse());
            _rng.SetNext(1, 1, 4, 4, 6);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfFullHouse());
            _rng.SetNext(1, 1, 1, 1, 4);
            _dice.Shuffle();
            Assert.AreEqual(0, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseAllSetPossible()
        {
            _rng.SetNext(1, 4, 4, 1, 4);
            _dice.Shuffle();
            Assert.AreEqual(1, _dice.POfFullHouse());
            _rng.SetNext(6, 3, 3, 6, 6);
            _dice.Shuffle();
            Assert.AreEqual(1, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseNoneSet()
        {
            Assert.AreEqual(300.0 / 7776.0, GenerateUnsetDiceManager().POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseSomeSetPossible()
        {
            _rng.SetNext(1, 2, 3, 1, 3);
            _dice.Shuffle();
            _dice.Remove(1, 3);
            int temp = CalculateCount(d => d.IsFullHousePossible() && d[0] == 1 && d[2] == 3 && d[4] == 3);
            Assert.AreEqual(temp / 36.0, _dice.POfFullHouse());
            _rng.SetNext(5, 5, 6, 6, 6);
            _dice.Shuffle();
            _dice.Remove(0, 1, 2);
            temp = CalculateCount(d => d.IsFullHousePossible() && d[3] == 6 && d[4] == 6);
            Assert.AreEqual(temp / 216.0, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseSomeSetPossibleEasy2()
        {
            _rng.SetNext(4, 4, 4, 5, 5);
            _dice.Shuffle();
            _dice.Remove(2);
            Assert.AreEqual(2.0 / 6, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseSomeSetPossibleEasy()
        {
            _rng.SetNext(2, 2, 2, 4, 4);
            _dice.Shuffle();
            _dice.Remove(3);
            Assert.AreEqual(1.0 / 6, _dice.POfFullHouse());
            _rng.SetNext(3, 2, 3, 2, 2);
            _dice.Shuffle();
            _dice.Remove(0, 2);
            Assert.AreEqual(1.0 / 6, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseSomeSetNotPossibleNotEnough()
        {
            _rng.SetNext(5, 5, 3, 1, 5);
            _dice.Shuffle();
            _dice.Remove(1);
            Assert.AreEqual(0, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseSomeSetNotPossibleNotEnough2()
        {
            _rng.SetNext(6, 1, 5, 2, 3);
            _dice.Shuffle();
            _dice.Remove(0, 1);
            Assert.AreEqual(0, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestPOfFullHouseSomeSetNotPossibleToMany()
        {
            _rng.SetNext(1, 1, 1, 1, 1);
            _dice.Shuffle();
            _dice.Remove(4);
            Assert.AreEqual(0, _dice.POfFullHouse());
        }

        [TestMethod]
        public void TestAlwaysAll32OptionCalculated()
        {
            DiceManager dice = new DiceManager();
            Assert.AreEqual(32, DiceManager.GenerateAllOptions(_player, dice).Count);
        }

        [TestMethod]
        public void TestExpectedValuesCalculatedCorrectAllSet()
        {
            _rng.SetNext(5, 5, 5, 5, 5);
            _dice.Shuffle();
            var options = DiceManager.CalculateExpectedValues(_player, _dice);
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
            _dice.Remove(1, 4);
            var options = DiceManager.CalculateExpectedValues(_player, _dice);
            Assert.AreEqual(1 + 2.0 / 6, options[0].ValueD, DELTA);
            Assert.AreEqual(4.0 / 6, options[1].ValueD, DELTA);
            Assert.AreEqual((10 + 14 + 16) / 36.0, options[6].ValueD, DELTA);
            Assert.AreEqual(0, options[7].ValueD);
            Assert.AreEqual(30 * 13 / 36.0, 30 * _dice.POfSmallStreet());
            Assert.AreEqual(30 * 13 / 36.0, options[9].ValueD);
            Assert.AreEqual(40 * 1.0 / 18, options[10].ValueD);
            Assert.AreEqual(0, options[11].ValueD);
            Assert.AreEqual(8 + 7, options[12].ValueD);
        }

        [TestMethod]
        public void TestContains32DifferentOptions()
        {
            DiceManager dice = new DiceManager();
            HashSet<string> set = new HashSet<string>();
            var options = DiceManager.GenerateAllOptions(_player, dice);
            foreach (var d in options)
            {
                set.Add(d.ToString());
            }

            Assert.AreEqual(32, set.Count);
        }

        [TestMethod]
        public void TestAllOptionsHaveSameRightLength()
        {
            var options = DiceManager.GenerateAllOptions(_player, _dice);
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
            var options = DiceManager.GenerateAllOptions(_player, _dice);
            foreach (var option in options)
            {
                Assert.AreEqual(KniffelPointsTable.WRITEABLE_FIELDS_COUNT - 2, option.Count);
            }
        }

        [TestMethod]
        public void TestOnlyOneOptionRemaining()
        {
            WriteEverythingExceptChance(_player);
            var options = DiceManager.GenerateAllOptions(_player, _dice);
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
            var options = DiceManager.GenerateAllOptions(_player, _dice);
            foreach (var option in options)
            {
                Assert.AreEqual(0, option.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestThrowsExceptionWhenBigStreetNotPossible()
        {
            _rng.SetNext(1,2,3,3,3);
            _dice.Shuffle();
            _dice.IndexToFlipWhenOptimisingToBigStreet();
        }


        //TODO: decide whether all inputs should be brute forced for combination of set/unset dice

        #region HelpMethods

        private static int CalculateExpectedValue(Func<DiceManager, bool> predicate)
        {
            RandomStub rng = new RandomStub();
            DiceManager dice = new DiceManager(rng);
            int sum = 0;
            foreach (int[] values in AllCombinations)
            {
                rng.SetNext(values);
                dice.Shuffle();
                if (predicate(dice))
                {
                    sum += dice.SumOfAllDices;
                }
            }

            return sum;
        }

        private static int CalculateCount(Func<DiceManager, bool> predicate)
        {
            RandomStub rng = new RandomStub();
            DiceManager dice = new DiceManager(rng);
            int sum = 0;
            foreach (int[] values in AllCombinations)
            {
                rng.SetNext(values);
                dice.Shuffle();
                if (predicate(dice))
                {
                    sum += 1;
                }
            }

            return sum;
        }

        private static void RemoveAllDice(DiceManager dice) => dice.Remove(0, 1, 2, 3, 4);

        private static DiceManager GenerateUnsetDiceManager()
        {
            DiceManager dice = new DiceManager();
            RemoveAllDice(dice);
            return dice;
        }

        private static void TestAllCombinations(IEnumerable<int[]> inputs, object expected, string messageTemplate,
            CalculateActual calculate)
        {
            RandomStub rng = new RandomStub();
            DiceManager dice = new DiceManager(rng);
            TestAllCombinations(rng, dice, inputs,
                a => Assert.AreEqual(expected, calculate(a), string.Format(messageTemplate, dice)));
        }

        private static void TestAllCombinations(IEnumerable<int[]> inputs, AssertCombinationOk combinationOk)
        {
            RandomStub rng = new RandomStub();
            DiceManager dice = new DiceManager(rng);
            TestAllCombinations(rng, dice, inputs, combinationOk);
        }

        private static void TestAllCombinations(RandomStub rng, DiceManager dice, IEnumerable<int[]> inputs,
            AssertCombinationOk combinationOk)
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

        #endregion
    }

    public delegate object CalculateActual(DiceManager dice);

    public delegate void AssertCombinationOk(DiceManager dice);

    internal class Comparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] a, int[] b)
        {
            if (a == null)
            {
                return b == null;
            }

            if (b == null || a.Length != b.Length)
            {
                return false;
            }

            int i = 0;
            while (i < a.Length && a[i] == b[i])
            {
                ++i;
            }

            return i == a.Length;
        }

        public int GetHashCode(int[] obj)
        {
            int sum = 0;
            const int factor = Dice.HIGHEST_VALUE - Dice.LOWEST_VALUE + 1;
            for (int i = obj.Length - 1; i >= 0; --i)
            {
                sum = (sum + obj[i] - Dice.LOWEST_VALUE) * factor;
            }

            return sum;
        }
    }
}