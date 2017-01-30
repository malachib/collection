﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class SerializationContainerTests
    {
        public class TestRecord1
        {
            int field1 = 11;
            string field2 = "Field #2";
        }

        [TestMethod]
        public void Test1()
        {
            var _sc = new ServiceCollection();
            // VS bugged out, can't find it
            //var fieldReflectionSerializer = new FieldReflectionSerializer();
            var sp = _sc.BuildServiceProvider();
            var sc = new SerializationContainer(sp);
        }
    }
}