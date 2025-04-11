using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel.Fields;

namespace SpieleSammlungTests.Model.Kniffel.Fields;

[TestClass]
public class KniffelFieldSingleTest
{
    [TestMethod]
    public void TestValueSet()
    {
        KniffelFieldSingle field = new KniffelFieldSingle { Value = 1 };
        Assert.AreEqual(1, field.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestExceptionThrownWhenInvalid()
    {
        KniffelFieldSingle field = new KniffelFieldSingle { Value = 1 };
        field.Value = 5;
    }

    [TestMethod]
    public void TestValueNotSetWhenInvalid()
    {
        KniffelFieldSingle field = new KniffelFieldSingle { Value = 7 };
        try
        {
            field.Value = 5;
            Assert.Fail("This code should not be reachable since an exception should have been thrown");
        }
        catch
        {
            // this case is handled by another test
        }

        Assert.AreEqual(7, field.Value);
    }

    [TestMethod]
    public void TestIsEmptyFalse()
    {
        KniffelFieldSingle field = new KniffelFieldSingle();
        Assert.IsTrue(field.IsEmpty());
    }

    [TestMethod]
    public void TestIsEmptyTrue()
    {
        KniffelFieldSingle field = new KniffelFieldSingle { Value = 7 };
        Assert.IsFalse(field.IsEmpty());
    }
}