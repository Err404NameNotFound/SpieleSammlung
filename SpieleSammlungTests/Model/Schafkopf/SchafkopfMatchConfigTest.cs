using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung;
using SpieleSammlung.Model.Schafkopf;
using SpieleSammlung.Properties;

namespace SpieleSammlungTests.Model.Schafkopf;

[TestClass]
public class SchafkopfMatchConfigTest
{
    [TestMethod]
    public void TestToStringWeiter()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Weiter, "");
        Assert.AreEqual(SchafkopfMode.Weiter.ToString(), config.ToString());
    }

    [TestMethod]
    public void TestToStringWenz()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Wenz, "");
        Assert.AreEqual(SchafkopfMode.Wenz.ToString(), config.ToString());
    }

    [TestMethod]
    public void TestToStringWenzTout()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.WenzTout, "");
        Assert.AreEqual(SchafkopfMode.WenzTout.ToString(), config.ToString());
    }

    [TestMethod]
    public void TestToStringGrasSolo()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Solo, Card.GRAS);
        Assert.AreEqual($"{Card.GRAS} {SchafkopfMode.Solo}", config.ToString());
    }

    [TestMethod]
    public void TestToStringEichelSoloTout()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.SoloTout, Card.EICHEL);
        Assert.AreEqual($"{Card.EICHEL} {SchafkopfMode.SoloTout}", config.ToString());
    }

    [TestMethod]
    public void TestToStringAufSchelle()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Card.SCHELLE);
        Assert.AreEqual($"{Resources.SK_PrefixSauspielToString} {Card.SCHELLE}", config.ToString());
    }
}