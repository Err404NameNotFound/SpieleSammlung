using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model.Kniffel;

namespace SpieleSammlungTests.Model.Kniffel
{
    [TestClass]
    public class ProbabilitiesTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestFacultyNegative()
        {
            Probabilities.Faculty(-1);
        }

        [TestMethod]
        public void TestFaculty0()
        {
            Assert.AreEqual(1, Probabilities.Faculty(0));
        }

        [TestMethod]
        public void TestFaculty1()
        {
            Assert.AreEqual(1, Probabilities.Faculty(1));
        }

        [TestMethod]
        public void TestFaculty5()
        {
            Assert.AreEqual(120, Probabilities.Faculty(5));
        }

        [TestMethod]
        public void TestFaculty10()
        {
            Assert.AreEqual(3628800, Probabilities.Faculty(10));
        }

        [TestMethod]
        public void TestFaculty20()
        {
            Assert.AreEqual(2432902008176640000, Probabilities.Faculty(20));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBinomialKGreaterN()
        {
            Probabilities.Binomial(1, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBinomialKNegative()
        {
            Probabilities.Binomial(1, -5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBinomialKNegativeAndGreaterN()
        {
            Probabilities.Binomial(-5, -1);
        }

        [TestMethod]
        public void TestBinomial1Choose1()
        {
            Assert.AreEqual(1, Probabilities.Binomial(1, 1));
        }

        [TestMethod]
        public void TestBinomial7Choose0()
        {
            Assert.AreEqual(1, Probabilities.Binomial(7, 0));
        }

        [TestMethod]
        public void TestBinomial10Choose1()
        {
            Assert.AreEqual(10, Probabilities.Binomial(10, 1));
        }

        [TestMethod]
        public void TestBinomial10Choose3()
        {
            Assert.AreEqual(120, Probabilities.Binomial(10, 3));
        }

        [TestMethod]
        public void TestBinomial20Choose7()
        {
            Assert.AreEqual(77520, Probabilities.Binomial(20, 7));
        }
    }
}