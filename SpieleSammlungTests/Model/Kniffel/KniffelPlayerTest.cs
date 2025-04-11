using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;
using SpieleSammlung.Model.Kniffel;

namespace SpieleSammlungTests.Model.Kniffel;

[TestClass]
public class KniffelPlayerTest
{
    [TestMethod]
    public void TestIndex()
    {
        KniffelPlayer player = new KniffelPlayer(new Player());
        Assert.AreEqual(player.Fields[5], player[5]);
    }

    [TestMethod]
    public void TestHasReachedBonus()
    {
        KniffelPlayer p = new KniffelPlayer(new Player())
            { [4] = { Value = 30 }, [5] = { Value = 36 } };
        p.Fields.UpdateSums();
        Assert.IsTrue(p.HasReachedBonus());
    }

    [TestMethod]
    public void TestHasNotReachedBonus()
    {
        KniffelPlayer p = new KniffelPlayer(new Player())
            { [4] = { Value = 25 }, [6] = { Value = 36 } };
        Assert.IsFalse(p.HasReachedBonus());
    }
}