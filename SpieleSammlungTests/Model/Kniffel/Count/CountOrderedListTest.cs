using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel.Count;

namespace SpieleSammlungTests.Model.Kniffel.Count;

[TestClass]
public class CountOrderedListTest
{
    [TestMethod]
    public void TestToStringEmpty()
    {
        CountOrderedList counter = new CountOrderedList();
        Assert.AreEqual("{  }", counter.ToString());
    }

    [TestMethod]
    public void TestToString()
    {
        CountOrderedList counter = new CountOrderedList();
        counter.IncCount(1);
        counter.IncCount(2);
        counter.IncCount(2);
        counter.IncCount(2);
        counter.IncCount(3);
        counter.IncCount(4);
        counter.IncCount(4);
        counter.DecCount(4);
        Assert.AreEqual(
            "{ { Value=2, Count=3 }, { Value=4, Count=1 }, { Value=1, Count=1 }, { Value=3, Count=1 },  }",
            counter.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void TestExceptionWhenOutOfRangeToLow()
    {
        CountOrderedList list = new CountOrderedList();
        _ = list[-1];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void TestExceptionWhenOutOfRangeToHigh()
    {
        CountOrderedList list = new CountOrderedList();
        _ = list[list.Count];
    }
}