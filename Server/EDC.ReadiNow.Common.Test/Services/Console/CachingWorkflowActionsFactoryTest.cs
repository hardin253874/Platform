// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Services.Console;
using NUnit.Framework;
using System;
using EDC.ReadiNow.Model;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Services.Console.WorkflowActions;

namespace EDC.ReadiNow.Test.Services.Console.WorkflowActions
{
    [TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class CachingWorkflowActionsFactoryTest
    {

        class DummyFactory : IWorkflowActionsFactory
        {
            Action _act;

            public DummyFactory(Action act)
            {
                _act = act;
            }

            public IEnumerable<Workflow> Fetch(ISet<long> typeIds)
            {
                _act();
                return new List<Workflow>();
            }
        }


        [Test]
        public void Test()
        {
            int count = 0;
            var cache = new CachingWorkflowActionsFactory(new DummyFactory(() => count++));

            cache.Fetch(new HashSet<long>());

            Assert.That(count, Is.EqualTo(1));

            cache.Fetch(new HashSet<long>());

            Assert.That(count, Is.EqualTo(1), "Grabbed from cache");

            cache.Fetch(new HashSet<long>() { 999 });

            Assert.That(count, Is.EqualTo(2));

            cache.Fetch(new HashSet<long>() { 999 });

            Assert.That(count, Is.EqualTo(2), "Grabbed from cache");

            cache.Fetch(new HashSet<long>() { 999, 777 });

            Assert.That(count, Is.EqualTo(3), "Grabbed fresh");

            cache.Fetch(new HashSet<long>() { 999, 777 });

            Assert.That(count, Is.EqualTo(3), "Grabbed from cache");
        }
               
    }
}
