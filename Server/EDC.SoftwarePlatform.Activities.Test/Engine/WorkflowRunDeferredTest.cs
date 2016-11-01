// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Common.Workflow;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class WorkflowRunDeferredTest
    {

        [Test]
        [RunWithTransaction]
        public void TriggeringUserIsSet()
        {
            var userAccount = new UserAccount();
            userAccount.Name = "foo" + Guid.NewGuid().ToString();
            userAccount.Save();

            using (var setUser = new SetUser(userAccount))
            {
                var run = new WorkflowRunDeferred(new Workflow());

                Assert.That(run.TriggeringUserId, Is.EqualTo(userAccount.Id));
                Assert.That(run.TriggeringUser.Id, Is.EqualTo(userAccount.Id));
            }
        }
    }
}
