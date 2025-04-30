#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;
using SpieleSammlung.Model.Kniffel.Fields;

#endregion

namespace SpieleSammlungTests.Model.Kniffel;

[TestClass]
public class WriteOptionTest
{
    [TestMethod]
    public void TestIntOption()
    {
        WriteOption option = new WriteOption(0, 1);
        Assert.AreEqual(0, option.Index);
        Assert.AreEqual(1, option.Value);
        Assert.AreEqual(1.0, option.ValueD);
    }

    [TestMethod]
    public void TestDoubleOption()
    {
        WriteOption option = new WriteOption(7, 5.5);
        Assert.AreEqual(7, option.Index);
        Assert.AreEqual(5, option.Value);
        Assert.AreEqual(5.5, option.ValueD);
    }

    [TestMethod]
    public void TestOptionContainsFieldName()
    {
        string text = new WriteOption(6, 9.3).ToString();
        Assert.IsTrue(text.Contains(KniffelPointsTable.FIELD_NAMES[6]));
    }

    [TestMethod]
    public void TestOptionContainsPointsInt()
    {
        string text = new WriteOption(9, 4).ToString();
        Assert.IsTrue(text.Contains("4"));
    }

    [TestMethod]
    public void TestOptionContainsPointsDouble()
    {
        string text = new WriteOption(9, 3.14159).ToString();
        Assert.IsTrue(text.Contains("3,14"));
    }
}