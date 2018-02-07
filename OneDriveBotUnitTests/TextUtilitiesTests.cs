using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneDriveBot.Services;

namespace OneDriveBotUnitTests
{
    [TestClass]
    public class TextUtilitiesTests
    {
        [TestMethod]
        public void TestExtractEmail()
        {
            Assert.AreEqual("multiw@outlook.com", TextUtilities.ExtractEmail(@"<a href=""mailto: multiw@outlook.com"">multiw@outlook.com</a>"));
            Assert.AreEqual("multiw@outlook.com", TextUtilities.ExtractEmail("multiw@outlook.com"));
        }
    }
}
