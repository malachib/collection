using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Fact.Extensions.Collection;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Fact.Extensions.Experimental.Tests
{
    [TestClass]
    public class FileProviderTests
    {
        [TestMethod]
        public void ByteArrayFileProviderTest()
        {
            var d = new SparseDictionary<string, byte[]>();
            var p = new ByteArrayFileProvider(d);

            d.Add("file", Encoding.UTF8.GetBytes("test"));

            IFileInfo fi = p.GetFileInfo("file");

            var r = new StreamReader(fi.CreateReadStream());

            Assert.AreEqual("test", r.ReadLine());
        }
        
    }
}