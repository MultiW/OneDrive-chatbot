using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneDriveBot.Services;
using System.Text.RegularExpressions;

namespace OneDriveBotUnitTests
{
    [TestClass]
    public class TextUtilitiesTests
    {
        [TestMethod]
        public void TestExtractEmail()
        {
            Assert.AreEqual("abcabc@outlook.com", TextUtilities.ExtractEmailFromHyperlink(@"<a href=""mailto: abcabc@outlook.com"">abcabc@outlook.com</a>"));
            Assert.AreEqual("abcabc@outlook.com", TextUtilities.ExtractEmailFromHyperlink("abcabc@outlook.com"));
        }

        [TestMethod]
        public void TestEmailRegexPattern()
        {
            Assert.AreEqual(true, Regex.IsMatch("abcabc@outlook.com", TextUtilities.EmailRegexString));

            Match m = Regex.Match("abcabc@outlook.com", TextUtilities.EmailRegexString);
            Assert.AreEqual(true, m.Success);
        }
    }
}
