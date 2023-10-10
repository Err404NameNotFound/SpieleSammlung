using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel.Count;

namespace SpieleSammlungTests.Model.Kniffel.Count
{
    [TestClass]
    public class DiceCounterTest
    {
        [TestMethod]
        public void TestGetCount()
        {
            Assert.AreEqual(5, new DiceCounter(4, 5).Count);
        }

        [TestMethod]
        public void TestGetValue()
        {
            Assert.AreEqual(3, new DiceCounter(3, 5).Value);
        }

        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("{ Value=4, Count=5 }", new DiceCounter(4, 5).ToString());
        }

        [TestMethod]
        public void TestToString2()
        {
            Assert.AreEqual("{ Value=2, Count=1 }", new DiceCounter(2, 1).ToString());
        }

        [TestMethod]
        public void TestCopyConstructor()
        {
            DiceCounter original = new DiceCounter(1, 5);
            DiceCounter copy = new DiceCounter(original);
            Assert.AreEqual(original.Count, copy.Count);
            Assert.AreEqual(original.Value, copy.Value);
        }

        [TestMethod]
        public void TestIntCount()
        {
            DiceCounter counter = new DiceCounter(2, 3);
            counter.IncCount();
            Assert.AreEqual(4, counter.Count);
        }

        [TestMethod]
        public void TestDecCount()
        {
            DiceCounter counter = new DiceCounter(5, 1);
            counter.DecCount();
            Assert.AreEqual(0, counter.Count);
        }
    }
}