using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;

namespace SpieleSammlungTests.Model
{
    [TestClass]
    public class IllegalMoveExceptionTest
    {
        [TestMethod]
        public void TestConstructorNoParam()
        {
            Exception e = new IllegalMoveException();
            Assert.AreEqual(e, e);
        }

        [TestMethod]
        public void TestConstructor1Param()
        {
            const string message = "this is a message";
            Exception e = new IllegalMoveException(message);
            Assert.AreEqual(message, e.Message);
        }

        [TestMethod]
        public void TestConstructor2Params()
        {
            const string message = "this is another message";
            Exception cause = new Exception();
            Exception e = new IllegalMoveException(message, cause);
            Assert.AreEqual(message, e.Message);
            Assert.AreEqual(cause, e.GetBaseException());
        }
    }
}