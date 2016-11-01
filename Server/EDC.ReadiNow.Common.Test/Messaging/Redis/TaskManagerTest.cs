// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Messaging.Redis;
using NUnit.Framework;
using System;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
    [TestFixture]
    public class TaskManagerTest
    {
        [Test]
        public void FullCycle()
        {
            var tm = Factory.WorkflowRunTaskManager;
            string id  = Guid.NewGuid() + "FullCycle";
            
            Assert.That(tm.HasStarted(id), Is.False);
            Assert.That(tm.HasCompleted(id), Is.False);

            tm.RegisterStart(id);

            Assert.That(tm.HasStarted(id), Is.True);
            Assert.That(tm.HasCompleted(id), Is.False);

            var info = Guid.NewGuid() + "Info";
            tm.RegisterComplete(id, info);

            Assert.That(tm.HasStarted(id), Is.True);
            Assert.That(tm.HasCompleted(id), Is.True);
            Assert.That(tm.GetResult(id), Is.EqualTo(info));

            tm.Clear(id);

            Assert.That(tm.HasStarted(id), Is.False);
            Assert.That(tm.HasCompleted(id), Is.False);
        }


        [Test]
        public void ClearNonExistant()
        {
            var tm = Factory.WorkflowRunTaskManager;
            string id = Guid.NewGuid() + "ClearNonExistant";
            tm.Clear(id);
        }

        [Test]
        public void TwoManagersInteracting()
        {
            var tm1 = Factory.WorkflowRunTaskManager;
            var tm2 = Factory.WorkflowRunTaskManager;
            string id  = Guid.NewGuid() + "TwoManagersInteracting";

            tm1.RegisterStart(id);
            Assert.That(tm2.HasStarted(id), Is.True);
        }

        [Test]
        public void GetContextBlock()
        {
            var tm1 = Factory.WorkflowRunTaskManager;
            var taskId = Guid.NewGuid().ToString();

            using (tm1.GetContextBlock(taskId))
            {
                Assert.That(tm1.HasStarted(taskId), Is.True);
            }

            Assert.That(tm1.HasCompleted(taskId), Is.True);
        }

        [Test]
        public void GetContextBlockWithExplicitComplete()
        {
            var tm1 = Factory.WorkflowRunTaskManager;
            var taskId = Guid.NewGuid().ToString();

            using (tm1.GetContextBlock(taskId))
            {
                tm1.RegisterComplete(taskId, "explicit");
            }

            Assert.That(tm1.GetResult(taskId), Is.EqualTo("explicit"));
        }

        [Test]
        public void GetContextBlockWithThrow()
        {
            var tm1 = Factory.WorkflowRunTaskManager;
            var taskId = Guid.NewGuid().ToString();

            try
            {
                using (tm1.GetContextBlock(taskId))
                {
                    throw new ArgumentException();
                }
            }
            catch(ArgumentException)
            {
                Assert.That(tm1.HasCompleted(taskId), Is.True);
            }
        }
    }
}
