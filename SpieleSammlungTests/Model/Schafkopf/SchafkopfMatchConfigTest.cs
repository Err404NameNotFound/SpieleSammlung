#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Schafkopf;
using SpieleSammlung.Properties;
using static SpieleSammlung.Model.Schafkopf.CardColor;

#endregion

namespace SpieleSammlungTests.Model.Schafkopf;

[TestClass]
public class SchafkopfMatchConfigTest
{
    [TestMethod]
    public void TestToStringWeiter()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Weiter, "");
        Assert.AreEqual(nameof(SchafkopfMode.Weiter), config.ToString());
    }

    [TestMethod]
    public void TestToStringWenz()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Wenz, "");
        Assert.AreEqual(nameof(SchafkopfMode.Wenz), config.ToString());
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
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Solo, Gras);
        Assert.AreEqual($"{Gras} {SchafkopfMode.Solo}", config.ToString());
    }

    [TestMethod]
    public void TestToStringEichelSoloTout()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.SoloTout, Eichel);
        Assert.AreEqual($"{Eichel} {SchafkopfMode.SoloTout}", config.ToString());
    }

    [TestMethod]
    public void TestToStringAufSchelle()
    {
        SchafkopfMatchConfig config = new SchafkopfMatchConfig(SchafkopfMode.Sauspiel, Schelle);
        Assert.AreEqual($"{Resources.SK_PrefixSauspielToString} {Schelle}", config.ToString());
    }
}