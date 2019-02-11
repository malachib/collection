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

            // 'primes the pump' so that our struct-SparseDictionary allocates a lazy loaded value
            // which itself is a reference that does not change, thereby making this struct semi-ref'able
            d.Add("file", Encoding.UTF8.GetBytes("test"));

            var p = new ByteArrayFileProvider(d);

            IFileInfo fi = p.GetFileInfo("file");

            var r = new StreamReader(fi.CreateReadStream());

            Assert.AreEqual("test", r.ReadLine());
        }
        

        [TestMethod]
        public void Cached_ByteArrayFileProviderTest()
        {
        }
    }
}