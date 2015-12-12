using Microsoft.VisualStudio.TestTools.UnitTesting;
using TirkxDownloader.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework.Tests
{
    [TestClass()]
    public class ExtensionTests
    {
        [TestMethod()]
        public void LikeTest()
        {
            string stored = "darkwing.3dfxwave.com";
            bool result = stored.Like("darkwing.3dfxwave.com");

            Assert.AreEqual(true, result);
        }

        [TestMethod()]
        public void CompareDomainNameTest()
        {
            string stored = "*.3dfxwave.com";
            bool result = stored.IsSameDomain("darkwing.3dfxwave.com");

            Assert.AreEqual(true, result);
        }
    }
}