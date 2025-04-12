using System;
using NUnit.Framework;
using SpieleSammlung.Model.Util;

namespace SpieleSammlungTests.Model.Util;

[TestFixture]
[TestOf(typeof(MathHelp))]
public class MathHelpTest
{
    private const double EPSILON = 1e-5;
    [Test]
    public void TestLogBase2()
    {
        Assert.AreEqual(2.0, MathHelp.Log(4, 2));
    }

    [Test]
    public void TestLogBase10()
    {
        double result = MathHelp.Log(1000, 10);
        Assert.True(Math.Abs(3.0-result)<EPSILON, "Expected: 3, Actual:"+result);
    }
}