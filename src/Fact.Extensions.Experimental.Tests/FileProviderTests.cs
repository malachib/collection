using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Fact.Extensions.Collection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Fact.Extensions.Experimental.Tests
{
    [TestClass]
    public class FileProviderTests
    {
        void ByteArrayFileProviderTest(IAccessor<string, byte[]> accessor)
        {
            var p = new ByteArrayFileProvider(accessor);

            IFileInfo fi = p.GetFileInfo("file");

            var r = new StreamReader(fi.CreateReadStream());

            Assert.AreEqual("test", r.ReadLine());
        }

        [TestMethod]
        public void Dictionary_ByteArrayFileProviderTest()
        {
            var d = new SparseDictionary<string, byte[]>();

            // 'primes the pump' so that our struct-SparseDictionary allocates a lazy loaded value
            // which itself is a reference that does not change, thereby making this struct semi-ref'able
            d.Add("file", Encoding.UTF8.GetBytes("test"));

            ByteArrayFileProviderTest(d);
        }
        

        [TestMethod]
        public void Cached_ByteArrayFileProviderTest()
        {
            var sc = new ServiceCollection();
            sc.AddMemoryCache();
            var sp = sc.BuildServiceProvider();
            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            var cache = new Caching.MemoryCacheIndexer(memoryCache);
            var i = new NamedIndexerWrapper<byte[]>(
                k => (byte[]) cache[k], 
                (k, v) => cache[k] = v);

            i["file"] = Encoding.UTF8.GetBytes("test");

            ByteArrayFileProviderTest(i);
        }


        [TestMethod]
        public void Watching_ByteArrayFileProviderTest()
        {

        }
    }
}