using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpieleSammlung.Model;

namespace SpieleSammlungTests.Model
{
    [TestClass]
    public class ModelLogTest
    {
        [TestMethod]
        public void Test01_Write()
        {
            ModelLog.WriteToFile = true;
            ModelLog.WriteToConsole = true;
            const string message = "this is a message for testing";
            ModelLog.Write(message);
            Assert.AreEqual(message, Actual());
        }

        [TestMethod]
        public void Test02_WriteAll()
        {
            List<string> messages = ["firstMessage", "SecondMessage", "ThirdMessage"];
            ModelLog.Write(messages);
            Assert.AreEqual(string.Join(ModelLog.NEWLINE, messages) + ModelLog.NEWLINE, Actual());
        }

        [TestMethod]
        public void Test03_Append()
        {
            const string message1 = "The first Text is from the Write() method.";
            ModelLog.Write(message1);
            const string message2 = "This text on the other hand is from the Append() method.";
            ModelLog.Append(message2);
            Assert.AreEqual(message1 + message2, Actual());
        }

        [TestMethod]
        public void Test04_AppendLine()
        {
            const string message1 = "The first Text is from the Write() method.";
            ModelLog.Write(message1);
            const string message2 = "This text on the other hand is from the Append() method.";
            ModelLog.AppendLine(message2);
            Assert.AreEqual(message1 + message2 + ModelLog.NEWLINE, Actual());
        }

        [TestMethod]
        public void Test05_AppendMultiple()
        {
            const string message1 = "The first Text is from the Write() method.";
            ModelLog.Write(message1);
            List<string> messages = ["firstMessage", "SecondMessage", "ThirdMessage"];
            ModelLog.Append(messages);
            Assert.AreEqual(message1 + string.Join(ModelLog.NEWLINE, messages) + ModelLog.NEWLINE, Actual());
        }

        [TestMethod]
        public void Test05_AppendSeparatorLine0()
        {
            TestSeparatorAppending(0, ModelLog.SEPARATOR0);
        }

        [TestMethod]
        public void Test06_AppendSeparatorLine1()
        {
            TestSeparatorAppending(1, ModelLog.SEPARATOR1);
        }

        [TestMethod]
        public void Test07_AppendSeparatorLine2()
        {
            TestSeparatorAppending(2, ModelLog.SEPARATOR2);
        }

        [TestMethod]
        public void Test08_AppendSeparatorLineDefault1()
        {
            TestSeparatorAppending(3, ModelLog.SEPARATOR_DEFAULT);
        }

        [TestMethod]
        public void Test09_AppendSeparatorLineDefault2()
        {
            TestSeparatorAppending(-345798, ModelLog.SEPARATOR_DEFAULT);
        }

        [TestMethod]
        public void Test10_TestInitialWritesCorrect()
        {
            Assert.AreEqual(ModelLog.WriteToConsole || ModelLog.WriteToFile, ModelLog.Writes);
        }

        [TestMethod]
        public void Test11_TestWritesChangesToFalse()
        {
            ModelLog.WriteToConsole = false;
            ModelLog.WriteToFile = false;
            Assert.IsFalse(ModelLog.Writes);
        }

        [TestMethod]
        public void Test12_TestWritesChangesToTrue()
        {
            ModelLog.WriteToConsole = true;
            Assert.IsTrue(ModelLog.Writes);
        }

        [TestMethod]
        public void Test13_TestWritesChangesToTrue()
        {
            ModelLog.WriteToConsole = false;
            Assert.IsFalse(ModelLog.Writes);
            ModelLog.WriteToFile = true;
            Assert.IsTrue(ModelLog.Writes);
        }

        [TestMethod]
        public void Test14_AppendFormatted()
        {
            const string message1 = "The first Text is from the Write() method.";
            ModelLog.Write(message1);
            const string message2 = "This text is appended and {0} with additional parameters";
            ModelLog.Append(message2, "formatted");
            Assert.AreEqual(message1 + string.Format(message2, "formatted"), Actual());
        }

        [TestMethod]
        public void Test15_AppendFormattedLine()
        {
            const string message1 = "The first Text is from the Write() method.";
            ModelLog.Write(message1);
            const string message2 = "This text is appended and {0} with additional parameters.";
            const string message3 = "This text is war written to a new line.";
            ModelLog.AppendLine(message2, "formatted");
            ModelLog.Append(message3);
            Assert.AreEqual(message1 + string.Format(message2, "formatted") + ModelLog.NEWLINE + message3, Actual());
        }

        [TestMethod]
        public void Test16_AppendFormattedLine()
        {
            ModelLog.Write("This text will be replaced.");
            const string message = "This is the {0} text in the file";
            ModelLog.Write(message, "only");
            Assert.AreEqual(string.Format(message, "only"), Actual());
        }

        [TestMethod]
        public void Test17_DoesNotWriteToFileIfDisabled()
        {
            ModelLog.WriteToFile = false;
            string current = Actual();
            ModelLog.Write("This text should be ignored.");
            Assert.AreEqual(current, Actual());
        }

        [TestMethod]
        public void Test18_WriteToFileCanBeEnabled()
        {
            ModelLog.WriteToFile = true;
            const string message = "This text should be written.";
            ModelLog.Write(message);
            Assert.AreEqual(message, Actual());
        }


        [TestMethod]
        public void Test20_DeleteLogFile()
        {
            ModelLog.WriteToFile = false;
            ModelLog.WriteToConsole = false;
            ModelLog.DeleteLogFile();
            Assert.IsFalse(File.Exists(ModelLog.PATH));
        }

        private static void TestSeparatorAppending(int number, string separator)
        {
            const string message = "The first Text is from the Write() method.";
            ModelLog.Write(message);
            ModelLog.AppendSeparatorLine(number);
            Assert.AreEqual(message + separator + ModelLog.NEWLINE, Actual());
        }

        private static string Actual()
        {
            return File.ReadAllText(ModelLog.PATH);
        }
    }
}