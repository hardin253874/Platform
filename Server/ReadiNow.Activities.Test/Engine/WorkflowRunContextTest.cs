// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Common.Workflow;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    public class WorkflowRunContextTest
    {
        [Test]
        public void Defaults()
        {
            Assert.That(WorkflowRunContext.Current, Is.Not.Null);
            Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(false));

            using (var ctx = new WorkflowRunContext())
            {
                Assert.That(WorkflowRunContext.Current, Is.Not.Null);
                Assert.That(WorkflowRunContext.Current, Is.EqualTo(ctx));
                Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(false));
            }

            Assert.That(WorkflowRunContext.Current, Is.Not.Null);
            Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(false));

        }

        [Test]
        public void SettingValue()
        {
            var tp1 = new TaskFactory();
            var tp2 = new TaskFactory();

            using (new WorkflowRunContext())
            {
                WorkflowRunContext.Current.RunTriggersInCurrentThread = true;
                WorkflowRunContext.Current.DisableTriggers = true;

                using (new WorkflowRunContext())
                {
                    Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(true));
                    Assert.That(WorkflowRunContext.Current.DisableTriggers, Is.EqualTo(true));

                    WorkflowRunContext.Current.RunTriggersInCurrentThread = false;
                    WorkflowRunContext.Current.DisableTriggers = false;

                    Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(false));
                    Assert.That(WorkflowRunContext.Current.DisableTriggers, Is.EqualTo(false));
                }

                Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(true));
                Assert.That(WorkflowRunContext.Current.DisableTriggers, Is.EqualTo(true));
            }

            Assert.That(WorkflowRunContext.Current.RunTriggersInCurrentThread, Is.EqualTo(false));
            Assert.That(WorkflowRunContext.Current.DisableTriggers, Is.EqualTo(false));
        }

        [Test]
        public void DeferedActionsRun()
        {
            bool a = false;

            using (new WorkflowRunContext())
            {
                WorkflowRunContext.Current.DeferAction(() => a = true);
                Assert.That(a, Is.False);
            }

            Assert.That(a, Is.True);
        }


        [Test]
        public void DeferedActionsPropegateUp()
        {
            bool a = false;


            using (new WorkflowRunContext())
            {
                using (new WorkflowRunContext())
                {
                    WorkflowRunContext.Current.DeferAction(() => a = true);
                }
                Assert.That(a, Is.False);
            }

            Assert.That(a, Is.True);
        }

        [Test]
        public void DeferedActionsRunInOrder()
        {
            string s = "a";
            using (new WorkflowRunContext())
            {
                WorkflowRunContext.Current.DeferAction(() => s += 'b');

                using (new WorkflowRunContext())
                {
                    WorkflowRunContext.Current.DeferAction(() => s += 'c');
                }

                WorkflowRunContext.Current.DeferAction(() => s += 'd');
            }

            Assert.That(s, Is.EqualTo("abcd"));
        }

        [Test]
        public void DeferredActionCanAddActions()
        {

            string s = "a";

            using (new WorkflowRunContext())
            {
                WorkflowRunContext.Current.DeferAction(() =>
                {
                    s += "b";
                    WorkflowRunContext.Current.DeferAction(() =>
                    {
                        s += "c";
                    });
                });
            }

            Assert.That(s, Is.EqualTo("abc"));
        }


        [Test]
        public void DeferredActionHandleDeferredSet()
        {

            string s = "a";

            using (new WorkflowRunContext())
            {
                using (new WorkflowRunContext(true))
                {
                    WorkflowRunContext.Current.DeferAction(() =>
                    {
                        s += "b";
                    });
                }

                s += "c";
            }

            Assert.That(s, Is.EqualTo("abc"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DeferredActionWithNoContext()
        {
            bool a = false;
            WorkflowRunContext.Current.DeferAction(() => a = true);
            Assert.That(a, Is.True);
        }
    }
}
