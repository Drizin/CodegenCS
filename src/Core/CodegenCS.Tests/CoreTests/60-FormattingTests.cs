using NUnit.Framework;
using System;
using System.Linq;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace CodegenCS.Tests.CoreTests
{
    public class FormattingTests
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region IFormattable

        [Test]
        public void TestFormat1()
        {
            decimal amount = 100.00m;
            _w.Write($"Price: {amount:000}");
            Assert.AreEqual("Price: 100", _w.GetContents());
        }

        [Test]
        public void TestFormat2()
        {
            decimal amount = .34m;
            _w.Write($"Price: {amount:000.00}");
            Assert.AreEqual("Price: 000.34", _w.GetContents());
        }

        [Test]
        public void TestFormat3()
        {
            decimal[] prices = new decimal[] { .34m, .55m, 3.693m };
            Action<ICodegenTextWriter> writeFn = (w) => prices.ToList().ForEach(p => w.WriteLine($"Price: {p:000.0000}"));
            _w.Write(writeFn);
            string expected = @"
Price: 000.3400
Price: 000.5500
Price: 003.6930
".TrimStart(Environment.NewLine.ToCharArray());
            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestFormat4()
        {
            int val = 1;
            _w.Write($"11 elements: {val}{val}{val}{val}{val}{val}{val}{val}{val}{val}{val}");
            Assert.AreEqual("11 elements: 11111111111", _w.GetContents());
        }



        #endregion

    }

}
