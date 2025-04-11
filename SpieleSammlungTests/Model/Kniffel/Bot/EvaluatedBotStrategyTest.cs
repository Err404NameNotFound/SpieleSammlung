using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.Model.Kniffel.Bot;
using SpieleSammlungTests.Utils;

namespace SpieleSammlungTests.Model.Kniffel.Bot;

[TestClass]
public class EvaluatedBotStrategyTest
{
    private readonly EvaluatedBotStrategy _strategy = new();
    private static readonly RandomStub Rng;

    static EvaluatedBotStrategyTest()
    {
        Rng = new RandomStub();
        EvaluatedBotStrategy.rng = Rng;
    }

    [TestMethod]
    public void TestFitnessCalculated()
    {
        Assert.AreNotEqual(0, _strategy.RecalculateFitness(repetitions: 1, threads: 1));
    }

    [TestMethod]
    public void TestFitnessCalculatedCorrectly()
    {
        RandomStub rng = new RandomStub();
        rng.SetOutputConstant(6);
        Assert.AreEqual(30 * 4 + KniffelGame.VALUE_KNIFFEL, _strategy.RecalculateFitness(repetitions: 2, threads: 1, random: rng));
    }

    [TestMethod]
    public void TestFitnessCalculatedCorrectlyParallel()
    {
        RandomStub rng = new RandomStub();
        rng.SetOutputConstant(6);
        Assert.AreEqual(30 * 4 + KniffelGame.VALUE_KNIFFEL, _strategy.RecalculateFitness(repetitions: 10, threads: 4, random: rng));
    }

    [TestMethod]
    public void TestMutantChangesNot()
    {
        Rng.SetNextDoubleOutputConstant(1.0);
        EvaluatedBotStrategy mutant = _strategy.CreateMutantWithoutEvaluation();
        Rng.ClearOutputConstant();
        Assert.AreEqual(_strategy.ToString(), mutant.ToString(false));
    }

    [TestMethod]
    public void TestMutantChanges()
    {
        Rng.SetNextDoubleOutputConstant(0.0);
        EvaluatedBotStrategy mutant = _strategy.CreateMutantWithoutEvaluation();
        Rng.ClearOutputConstant();
        Assert.AreNotEqual(_strategy.ToString(), mutant.ToString(false));
    }

    [TestMethod]
    public void TestMutantChangesAndFitnessIsEvaluated()
    {
        RandomStub rng = new RandomStub();
        rng.SetOutputConstant(6);
        Rng.SetNextDoubleOutputConstant(0.0);
        EvaluatedBotStrategy mutant = _strategy.MutateAndEvaluate(repetitions: 1, threads: 1, random: rng);
        Rng.ClearOutputConstant();
        Assert.AreNotEqual("", mutant.ToString());
    }

    [TestMethod]
    public void TestCalculatingFitnessDoesNotChangeConfiguration()
    {
        Rng.SetNextDoubleOutputConstant(1.0);
        EvaluatedBotStrategy mutant = _strategy.CreateMutantWithoutEvaluation();
        Rng.ClearOutputConstant();
        Assert.IsTrue(mutant.ToString().Contains(_strategy.ToString()));
    }

    [TestMethod]
    public void TestChangeElseCases()
    {
        Rng.SetNext(0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0,
            1.0);
        EvaluatedBotStrategy mutant = _strategy.CreateMutantWithoutEvaluation();
        Assert.AreNotEqual(_strategy.ToString(), mutant.ToString(false));
    }

    [TestMethod]
    public void TestConstructorWithSpecificBestOption()
    {
        Assert.AreEqual("Not reached: 0, 13, 9, 12, 10, 11, 8, 1, 2, 3, 4, 5\n" +
                        "reached: 0, 1, 2, 13, 9, 12, 10, 11, 8, 3, 4, 5\nIndex: 5" +
                        "\nMin values: {Chance 15, Pair 3: 14, Pair 4: 13}",
            new EvaluatedBotStrategy(5).ToString());
    }
}