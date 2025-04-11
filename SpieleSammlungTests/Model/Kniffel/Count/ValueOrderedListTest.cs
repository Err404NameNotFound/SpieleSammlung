using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel.Count;

namespace SpieleSammlungTests.Model.Kniffel.Count;

[TestClass]
public class ValueOrderedListTest
{
    [TestMethod]
    public void TestToStringEmpty()
    {
        ValueOrderedList counter = new ValueOrderedList();
        Assert.AreEqual("{  }", counter.ToString());
    }

    [TestMethod]
    public void TestToString()
    {
        ValueOrderedList counter = new ValueOrderedList();
        counter.IncCount(1);
        counter.IncCount(2);
        counter.IncCount(2);
        counter.IncCount(2);
        counter.IncCount(3);
        counter.IncCount(4);
        counter.IncCount(4);
        counter.DecCount(4);
        Assert.AreEqual(
            "{ { Value=1, Count=1 }, { Value=2, Count=3 }, { Value=3, Count=1 }, { Value=4, Count=1 } }",
            counter.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void TestExceptionWhenOutOfRangeToLow()
    {
        ValueOrderedList list = new ValueOrderedList();
        _ = list[-1];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void TestExceptionWhenOutOfRangeToHigh()
    {
        ValueOrderedList list = new ValueOrderedList();
        _ = list[list.Count];
    }
}