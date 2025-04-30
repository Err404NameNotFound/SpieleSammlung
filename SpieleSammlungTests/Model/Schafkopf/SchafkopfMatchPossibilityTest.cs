#region

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Schafkopf;
using static SpieleSammlung.Model.Schafkopf.CardColor;

#endregion

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
        const SchafkopfMode mode = SchafkopfMode.Sauspiel;
        List<CardColor?> colors = [Gras, Eichel, Schelle];
        SchafkopfMatchPossibility possibility = new SchafkopfMatchPossibility(mode, colors);
        Assert.AreEqual(mode + ": " + string.Join(", ", colors), possibility.ToString());
    }

    [TestMethod]
    public void TestToStringConstructorWithListSoloTout()
    {
        const SchafkopfMode mode = SchafkopfMode.SoloTout;
        List<CardColor?> colors = [Gras, Eichel, Schelle, Herz];
        SchafkopfMatchPossibility possibility = new SchafkopfMatchPossibility(mode, colors);
        Assert.AreEqual(mode + ": " + string.Join(", ", colors), possibility.ToString());
    }
}