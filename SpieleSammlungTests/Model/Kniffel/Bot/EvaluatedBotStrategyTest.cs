using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Kniffel.Bot
{
    [TestClass]
    public class EvaluatedBotStrategyTest
    {
        private readonly EvaluatedBotStrategy _strategy = new();
        
        [TestMethod]
        public void TestFitnessCalculatedCorrectly()
        {
            RandomStub rng = new RandomStub();
            rng.SetOutputConstant(6);
            Assert.AreEqual(30 * 4 + KniffelGame.VALUE_KNIFFEL, _strategy.RecalculateFitness(1, 10, rng));
        }
    }
}