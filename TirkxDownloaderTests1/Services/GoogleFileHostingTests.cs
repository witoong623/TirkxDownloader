using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TirkxDownloader.Services.Tests
{
    [TestClass()]
    public class GoogleFileHostingTests
    {
        private GoogleFileHosting _googleDrive = new GoogleFileHosting();

        [TestMethod()]
        public void GetFileIdTest()
        {
            string id = _googleDrive.GetFileId(@"https://drive.google.com/uc?export=download&confirm=XUKV&id=0B2J3ASeP_4ucWHB0djFneFZCZzA");
            Assert.AreEqual("0B2J3ASeP_4ucWHB0djFneFZCZzA", id);
        }

        [TestMethod()]
        public void GetListIdTest()
        {
            string listId = _googleDrive.GetListId(@"https://drive.google.com/folderview?id=0B2J3ASeP_4ucUE4xOEJzenRaZW8&usp=drive_web#list");
            Assert.AreEqual("0B2J3ASeP_4ucUE4xOEJzenRaZW8", listId);
        }
    }
}