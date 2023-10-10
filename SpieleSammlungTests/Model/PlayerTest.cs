using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;

namespace SpieleSammlungTests.Model
{
    [TestClass]
    public class PlayerTest
    {
        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("thisIsANameForAPlayer", new Player("thisIsANameForAPlayer", false).ToString());
        }

        [TestMethod]
        public void TestDefaultPlayerIsBot()
        {
            Assert.IsTrue(new Player().IsBot);
        }
    }
}