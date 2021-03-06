﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration.Tests
{
    [TestClass]
    public class PolicyTests
    {
        [Alias("fact.policy.synthetic")]
        public class SyntheticPolicy : IPolicy
        {

        }

        [TestMethod]
        public void Policy1Test()
        {
            var pp = new PolicyProvider();

            var p = pp.GetPolicy<SyntheticPolicy>();
        }


        [TestMethod]
        public void NowPolicyTest()
        {
            var pp = new PolicyProvider();

            pp.AddPolicy<INowPolicy>(new SpecificNowPolicy(new DateTime(1974, 7, 16)));

            var p = pp.GetPolicy<INowPolicy>();

            Assert.IsTrue(DateTime.Now.Subtract(p.Now).TotalDays / 365 > 40);

            var pp2 = new PolicyProvider();

            var p2 = pp2.GetPolicy<INowPolicy>();

            Assert.IsTrue(DateTime.Now.Subtract(p2.Now).TotalSeconds <= 0);
        }

        class TestClass1
        {

        }


        class TestClass2
        {

        }

        [TestMethod]
        public void ClassPolicyTest()
        {
            var pp = new PolicyProvider();

            pp.Register<TestClass2>();

            var pp1 = PolicyProvider.Get<TestClass1>();
            var pp2 = PolicyProvider.Get<TestClass2>();

            Assert.AreEqual(pp1, PolicyProvider.System);
            Assert.AreEqual(pp2, pp);
        }
    }
}
