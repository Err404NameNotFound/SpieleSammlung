using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung;
using SpieleSammlung.Model.Schafkopf;

namespace SpieleSammlungTests.Model.Schafkopf;

[TestClass]
public class SchafkopfMatchPossibilityTest
{
    [TestMethod]
    public void TestToStringSauspiel()
    {
        SchafkopfMatchPossibility possibility = new SchafkopfMatchPossibility(SchafkopfMode.Sauspiel);
        Assert.AreEqual(SchafkopfMode.Sauspiel.ToString(), possibility.ToString());
    }

    [TestMethod]
    public void TestToStringWenz()
    {
        SchafkopfMatchPossibility possibility = new SchafkopfMatchPossibility(SchafkopfMode.Wenz);
        Assert.AreEqual(SchafkopfMode.Wenz.ToString(), possibility.ToString());
    }

    [TestMethod]
    public void TestToStringConstructorWithList()
    {
        SchafkopfMode mode = SchafkopfMode.Sauspiel;
        List<string> colors = [Card.GRAS, Card.EICHEL, Card.SCHELLE];
        SchafkopfMatchPossibility possibility = new SchafkopfMatchPossibility(mode, colors);
        Assert.AreEqual(mode + ": " + string.Join(", ", colors), possibility.ToString());
    }

    [TestMethod]
    public void TestToStringConstructorWithListSoloTout()
    {
        SchafkopfMode mode = SchafkopfMode.SoloTout;
        List<string> colors = [Card.GRAS, Card.EICHEL, Card.SCHELLE, Card.HERZ];
        SchafkopfMatchPossibility possibility = new SchafkopfMatchPossibility(mode, colors);
        Assert.AreEqual(mode + ": " + string.Join(", ", colors), possibility.ToString());
    }
}