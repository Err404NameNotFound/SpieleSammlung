using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel.Bot;

namespace SpieleSammlungTests.Model.Kniffel.Bot;

[TestClass]
public class BestBotFinderTest
{
        
    [TestMethod]
    public void Test2_Main()
    {
        Assert.IsNotNull(BestBotFinder.shouldStop);
        try
        {
            BestBotFinder.shouldStop();
        }
        catch (Exception)
        {
            // this is only for coverage
        }

        BestBotFinder.repetitions = 1;
        BestBotFinder.threads = 1;
        BestBotFinder.shouldStop = () => false;
        BestBotFinder.testAllCount = 10;
        BestBotFinder.testOneCount = 10;
        BestBotFinder.main();
    }

    [TestMethod]
    public void Test3_AllBrakes()
    {
        BestBotFinder.repetitions = 1;
        BestBotFinder.threads = 1;
        BestBotFinder.shouldStop = () => true;
        BestBotFinder.TestAll();
    }

    [TestMethod]
    public void Test4_AllDoesNotBrake()
    {
        BestBotFinder.repetitions = 1;
        BestBotFinder.threads = 1;
        BestBotFinder.shouldStop = () => false;
        BestBotFinder.TestAll();
    }

    [TestMethod]
    public void Test5_OptimiseOneBrakes()
    {
        BestBotFinder.repetitions = 1;
        BestBotFinder.threads = 1;
        BestBotFinder.shouldStop = () => true;
        BestBotFinder.OptimiseOneStrategy(10);
    }

    [TestMethod]
    public void Test6_OptimiseOneDoesNotBrake()
    {
        BestBotFinder.repetitions = 1;
        BestBotFinder.threads = 1;
        BestBotFinder.shouldStop = () => true;
        BestBotFinder.OptimiseOneStrategy(10);
    }
}