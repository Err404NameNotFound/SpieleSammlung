#region

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel.Bot;

#endregion

namespace SpieleSammlungTests.Model.Kniffel.Bot;

[TestClass]
public class BestBotFinderTest
{
    [TestMethod]
    public void Test2_Main()
    {
        Assert.IsNotNull(BestBotFinder.ShouldStop);
        try
        {
            BestBotFinder.ShouldStop();
        }
        catch (Exception)
        {
            // this is only for coverage
        }

        BestBotFinder.Repetitions = 1;
        BestBotFinder.Threads = 1;
        BestBotFinder.ShouldStop = () => false;
        BestBotFinder.TestAllCount = 10;
        BestBotFinder.TestOneCount = 10;
        BestBotFinder.main();
    }

    [TestMethod]
    public void Test3_AllBrakes()
    {
        BestBotFinder.Repetitions = 1;
        BestBotFinder.Threads = 1;
        BestBotFinder.ShouldStop = () => true;
        BestBotFinder.TestAll();
    }

    [TestMethod]
    public void Test4_AllDoesNotBrake()
    {
        BestBotFinder.Repetitions = 1;
        BestBotFinder.Threads = 1;
        BestBotFinder.ShouldStop = () => false;
        BestBotFinder.TestAll();
    }

    [TestMethod]
    public void Test5_OptimiseOneBrakes()
    {
        BestBotFinder.Repetitions = 1;
        BestBotFinder.Threads = 1;
        BestBotFinder.ShouldStop = () => true;
        BestBotFinder.OptimiseOneStrategy(10);
    }

    [TestMethod]
    public void Test6_OptimiseOneDoesNotBrake()
    {
        BestBotFinder.Repetitions = 1;
        BestBotFinder.Threads = 1;
        BestBotFinder.ShouldStop = () => true;
        BestBotFinder.OptimiseOneStrategy(10);
    }
}