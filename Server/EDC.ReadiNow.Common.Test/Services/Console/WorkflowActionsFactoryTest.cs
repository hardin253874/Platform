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
    public class WorkflowActionsFactoryTest
    {

        [Test]
        public void FetchedFromFactory()
        {
            var factory = EDC.ReadiNow.Core.Factory.WorkflowActionsFactory;
            var typeIds = new HashSet<long> { UserAccount.UserAccount_Type.Id };
            var fetched = factory.Fetch(typeIds);

            Assert.That(fetched, Is.Not.Null);
        }

        
        [Test]
        public void Test()
        {
            var myType = Entity.Create<EntityType>();
            myType.Save();

            var wf = Entity.Create<Workflow>();
            var inputArg = Entity.Create<ResourceArgument>();
            inputArg.ConformsToType = myType;
            wf.InputArgumentForAction = inputArg.As<ActivityArgument>();

            wf.Save();

            var factory = new WorkflowActionsFactory();

            var typeIds = new HashSet<long> { myType.Id };
            var fetched = factory.Fetch(typeIds);

            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched.Count(), Is.EqualTo(1));
            Assert.That(fetched.First().Id, Is.EqualTo(wf.Id) );
        }
    }
}
