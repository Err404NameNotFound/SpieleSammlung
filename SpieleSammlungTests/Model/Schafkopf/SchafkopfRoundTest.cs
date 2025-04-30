#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Schafkopf;

#endregion

namespace SpieleSammlungTests.Model.Schafkopf;

[TestClass]
public class SchafkopfRoundTest
{
    [TestMethod]
    public void TestNewRound()
    {
        SchafkopfRound round = new SchafkopfRound();
        Assert.AreEqual(0, round.StartPlayer);
        Assert.AreEqual(0, round.NextStartPlayer);
        Assert.AreEqual(0, round.CurrentPlayer);
        Assert.AreEqual(0, round.HighestValue);
        Assert.AreEqual(null, round.SemiTrumpf);
    }

    [TestMethod]
    public void TestNextRound()
    {
        SchafkopfRound previous = new SchafkopfRound();
        previous.SetNextPlayer();
        previous.NewHighestCard(1, 10);
        previous.SetNextPlayer();
        SchafkopfRound round = new SchafkopfRound(previous);
        Assert.AreEqual(1, round.StartPlayer);
        Assert.AreEqual(1, round.NextStartPlayer);
        Assert.AreEqual(1, round.CurrentPlayer);
        Assert.AreEqual(0, round.HighestValue);
        Assert.AreEqual(null, round.SemiTrumpf);
    }

    [TestMethod]
    public void TestNextRoundHighestCardChanged()
    {
        SchafkopfRound previous = new SchafkopfRound();
        previous.SetNextPlayer();
        previous.NewHighestCard(1, 10);
        previous.SetNextPlayer();
        SchafkopfRound round = new SchafkopfRound(previous);
        round.SetNextPlayer();
        round.SetNextPlayer();
        round.NewHighestCard(2, 20);
        round.SetNextPlayer();
        Assert.AreEqual(1, round.StartPlayer);
        Assert.AreEqual(2, round.NextStartPlayer);
        Assert.AreEqual(0, round.CurrentPlayer);
        Assert.AreEqual(20, round.HighestValue);
        Assert.AreEqual(null, round.SemiTrumpf);
    }
}