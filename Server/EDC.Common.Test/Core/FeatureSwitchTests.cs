// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.FeatureSwitch;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Test.Core
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class FeatureSwitchTests
    {
        readonly string dummySwitch1 = "dummySwitch1" + DateTime.UtcNow.Ticks;

        [Test]
        public void FactoryConfigured()
        {
            Assert.That(Factory.FeatureSwitch, Is.Not.Null);
        }

        [Test]
        public void Get()
        {
            var fetched = Factory.FeatureSwitch.Get(dummySwitch1);
        }

        [TestCase(false,  false)]
        [TestCase(true, true)]
        public void Test_Set_Get(bool tenantValue, bool expectedValue)
        {

            Factory.FeatureSwitch.Set(dummySwitch1, tenantValue);
            var fetched = Factory.FeatureSwitch.Get(dummySwitch1);
            Assert.That(fetched, Is.EqualTo(expectedValue));
        }

        [TestCase("Me,You", ExpectedException = typeof(ArgumentException))]
        [TestCase("Me|You", ExpectedException = typeof(ArgumentException))]
        public void Bug_CommaInName_28077(string fsName)
        {
            Factory.FeatureSwitch.Set(fsName, true);
        }


        [TestCase("", "x1,x2", true, true)]
        [TestCase("", "x1=false,x2=true", false, true)]
        [TestCase("", "x1=,x2", true, true)]
        [TestCase("x1,x2", "x1=false,x2=true", false, true)]
        [TestCase("x1 , x2 = false", "x1=false,x2=true", false, true)]

        public void Test_SetList(string initial, string featureList, bool expectedF1, bool expectedF2)
        {
            var prefix = "fs" + DateTime.UtcNow.Ticks.ToString() + "Test";

            initial = initial.Replace("x", prefix);
            featureList = featureList.Replace("x", prefix);

            Factory.FeatureSwitch.Set(initial);

            Factory.FeatureSwitch.Set(featureList);

            var f1 = Factory.FeatureSwitch.Get(prefix + "1");
            var f2 = Factory.FeatureSwitch.Get(prefix + "2");
            Assert.That(f1, Is.EqualTo(expectedF1), "x1");
            Assert.That(f2, Is.EqualTo(expectedF2), "x2");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_SetList_BadValue()
        {
            Factory.FeatureSwitch.Set("f1=Flabber");
        }

        [Test]
        public void TestList()
        {
            var features = Factory.FeatureSwitch.List();


            Assert.That(features.Count(), Is.GreaterThanOrEqualTo(1));
            Assert.That(features.Where(f => f.Name == dummySwitch1).Count(), Is.EqualTo(1));
        }

        [Test]
        [RunWithTransaction]

        public void Test_FeatureListString()
        {
            string fs = "Test_FeatureListString" + DateTime.UtcNow.Millisecond;

            Assert.That(Factory.FeatureSwitch.GetFeatureListString(), Is.Not.StringContaining(fs));

            Factory.FeatureSwitch.Set(fs, true);

            Assert.That(Factory.FeatureSwitch.GetFeatureListString(), Is.StringContaining(fs));
        }

    }
}
