using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.Model.Kniffel.Fields;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Kniffel
{
    [TestClass]
    public class ShufflingOptionTest
    {
        private static readonly DiceManager Dice1 = new();
        private static readonly List<WriteOption> WriteOptions1 = new();
        private static readonly List<int> KillOptions1 = new();
        private static readonly DiceManager Dice2 = new();
        private static readonly List<WriteOption> WriteOptions2 = new();
        private static readonly List<int> KillOptions2 = new();

        [TestMethod]
        public void Test00_MaxOptionWithNoFields()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            Assert.IsNull(option.MaxWithoutChance);
        }

        [TestMethod]
        public void Test01_MaxOptionWithOnlyKillableFields()
        {
            KillOptions1.Add(1);
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            Assert.IsNull(option.MaxWithoutChance);
        }

        [TestMethod]
        public void Test02_MaxOptionOtherIsNull()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            Assert.AreEqual(option, option.MaxOptionWithoutChance(null));
        }

        [TestMethod]
        public void Test03_MaxOptionNoOptionAvailable()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(other, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test04_MaxOptionOtherNoOptionAvailable()
        {
            WriteOptions1.Add(new WriteOption(5, 18));
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test05_MaxOptionOtherHasChanceAsAnOption()
        {
            WriteOptions2.Add(new WriteOption(5, 12));
            WriteOptions2.Add(new WriteOption(KniffelPointsTable.INDEX_CHANCE, 30));
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test06_MaxOptionHasChanceAsAnOption()
        {
            WriteOptions1.Add(new WriteOption(KniffelPointsTable.INDEX_CHANCE, 30));
            WriteOptions2.Add(new WriteOption(KniffelPointsTable.INDEX_PAIR_SIZE_4, 26));
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(other, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test07_MaxOptionHasOnlyChanceAsAnOption()
        {
            List<WriteOption> writes = new List<WriteOption>
            {
                new(KniffelPointsTable.INDEX_CHANCE, 15),
                new(KniffelPointsTable.INDEX_PAIR_SIZE_4, 26)
            };
            List<WriteOption> writes2 = new List<WriteOption>
            {
                new(KniffelPointsTable.INDEX_CHANCE, 18),
                new(KniffelPointsTable.INDEX_PAIR_SIZE_4, 26),
                new(3, 8)
            };
            ShufflingOption option = new ShufflingOption(Dice1, writes, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, writes2, KillOptions2);
            Assert.AreEqual(other, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test08_MaxOptionOtherHasOnlyChanceAsAnOption()
        {
            List<WriteOption> writes = new List<WriteOption>
            {
                new(KniffelPointsTable.INDEX_CHANCE, 18),
                new(KniffelPointsTable.INDEX_PAIR_SIZE_4, 26),
                new(2, 6)
            };
            List<WriteOption> writes2 = new List<WriteOption>
            {
                new(KniffelPointsTable.INDEX_CHANCE, 15),
                new(KniffelPointsTable.INDEX_PAIR_SIZE_4, 26)
            };
            ShufflingOption option = new ShufflingOption(Dice1, writes, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, writes2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test09_MaxOptionAverage()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(other, option.MaxOptionAverage(other));
        }

        [TestMethod]
        public void Test10_MaxOptionSum()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(other, option.MaxOptionSum(other));
        }

        [TestMethod]
        public void Test11_MaxOptionWriteCount()
        {
            WriteOptions1.Add(new WriteOption(3, 12));
            WriteOptions1.Add(new WriteOption(KniffelPointsTable.INDEX_KNIFFEL, 50));
            WriteOptions1.Add(new WriteOption(KniffelPointsTable.INDEX_BIG_STREET, 40));
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionWriteCount(other));
        }

        [TestMethod]
        public void Test12_MaxOptionAverageWrite()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionAverageWrite(other));
        }

        [TestMethod]
        public void Test13_MaxOptionMaxOrSum()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionMaxOrSum(other));
        }

        [TestMethod]
        public void Test14_MaxOptionMax()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionMax(other));
        }

        [TestMethod]
        public void Test15_MaxOptionMaxWithoutChance()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void Test16_MaxOptionMaxOtherNull()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            Assert.AreEqual(option, option.MaxOptionMax(null));
        }

        [TestMethod]
        public void Test17_MaxOptionMaxOrSumOtherNull()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            Assert.AreEqual(option, option.MaxOptionMaxOrSum(null));
        }

        [TestMethod]
        public void Test18_MaxOptionMaxFirstTwoEqual()
        {
            WriteOptions1.Clear();
            WriteOptions1.AddRange(new[] { new WriteOption(5, 18), new WriteOption(3, 16) });
            WriteOptions2.Clear();
            WriteOptions2.Add(new WriteOption(5, 18));
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionMax(other));
        }

        [TestMethod]
        public void Test19_MaxOptionWithoutChanceFirstTwoEqual()
        {
            ShufflingOption option = new ShufflingOption(Dice1, WriteOptions1, KillOptions1);
            ShufflingOption other = new ShufflingOption(Dice2, WriteOptions2, KillOptions2);
            Assert.AreEqual(option, option.MaxOptionWithoutChance(other));
        }

        [TestMethod]
        public void TestToString()
        {
            List<WriteOption> writes = new List<WriteOption>
            {
                new(KniffelPointsTable.INDEX_CHANCE, 30), new(0, 1), new(1, 6), new(3, 8), new(5, 24)
            };
            List<int> removable = new List<int> { 6, 8, 9, 11 };
            const string expected =
                "{ { 1, 2, 3, 4, 5 }: M={Chance -> 30}, MWOC={6er -> 24}, A=7,67, AW=13,80, S=69,00, CountWrite=5 }";
            RandomStub rng = new RandomStub(1, 2, 3, 4, 5);
            DiceManager dice = new DiceManager(rng);
            Assert.AreEqual(expected, new ShufflingOption(dice, writes, removable).ToString());
        }

        [TestMethod]
        public void TestToStringLongForm()
        {
            List<WriteOption> writes = new List<WriteOption>
            {
                new(KniffelPointsTable.INDEX_CHANCE, 30),
                new(0, 1),
                new(1, 6),
                new(3, 8),
                new(5, 24)
            };
            List<int> removable = new List<int> { 6, 8, 9, 11 };
            const string expected =
                "{ { 5, 5, 5, 5, 6 }: M={Chance -> 30}, MWOC={6er -> 24}, A=7,67, AW=13,80, S=69,00, CountWrite=5 }\n" +
                "writable fields: {\nChance -> 30, 6er -> 24, 4er -> 8, 2er -> 6, 1er -> 1\n}\nkillable Fields: " +
                "{\nSumme oberer Teil -> 0, Dreierpasch -> 0, Viererpasch -> 0, kleine Straße -> 0\n}";
            RandomStub rng = new RandomStub(5, 5, 5, 5, 6);
            DiceManager dice = new DiceManager(rng);
            Assert.AreEqual(expected, new ShufflingOption(dice, writes, removable).ToString(true));
        }
    }
}