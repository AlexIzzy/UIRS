using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathLearnTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<string> s = new List<string>() { double.NaN.ToString() };
            double x = double.Parse(s[0])*2;
            Assert.AreEqual(x, double.NaN);
        }
    }
}
