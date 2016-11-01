// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;

namespace EDC.SoftwarePlatform.Activities.Task
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class UserTaskTargetTest
    {
        [Test]
        public void IsCompleteIsSet()
        {
            var t = Entity.Create<BaseUserTask>();
            t.TaskStatus_Enum =  TaskStatusEnum_Enumeration.TaskStatusNotStarted;
            t.UserTaskIsComplete = false;
            t.Save();

            Assert.That(t.UserTaskIsComplete, Is.False);
            Assert.That(t.PercentageCompleted, Is.Null);

            t = t.AsWritable<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            t.Save();

            Assert.That(t.UserTaskIsComplete, Is.True);
            Assert.That(t.PercentageCompleted, Is.EqualTo(new decimal?(100)));
            Assert.That(t.UserTaskCompletedOn, Is.Not.Null);

            var completedOn = t.UserTaskCompletedOn.Value;

            Assert.That(completedOn.Kind, Is.EqualTo(DateTimeKind.Utc), "The date needs to be UTC or it will end up incorrect in the DB");

            var timeDiffMs = (DateTime.UtcNow - completedOn).TotalMilliseconds;
            Assert.That(timeDiffMs, Is.LessThan(1000), "It has just been completed");
        }


        [Test]
        public void IsCompleteIsUnset()
        {
            var t = Entity.Create<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            t.Save();

            t = t.AsWritable<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusInProgress;
            t.Save();

            Assert.That(t.UserTaskIsComplete, Is.False);
            Assert.That(t.UserTaskCompletedOn, Is.Null);
        }

        [Test]
        public void PercentageCompleteIsSet()
        {
            var t = Entity.Create<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusNotStarted;
            t.Save();

            Assert.That(t.PercentageCompleted, Is.Null);

            t = t.AsWritable<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            t.Save();

            Assert.That(t.PercentageCompleted, Is.EqualTo(new decimal?(100)));
        }

        [Test]
        public void IsCompleteCantReset()
        {
            var t = Entity.Create<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            t.Save();

            Assert.That(t.UserTaskIsComplete, Is.True);

            t = t.AsWritable<BaseUserTask>();
            t.UserTaskIsComplete = false;
            t.Save();

            Assert.That(t.UserTaskIsComplete, Is.True);
        }


        [Test]
        public void CompletedPercentageCantChange()
        {
            var t = Entity.Create<BaseUserTask>();
            t.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            t.Save();

            Assert.That(t.PercentageCompleted, Is.EqualTo(new decimal?(100)));

            t = t.AsWritable<BaseUserTask>();
            t.PercentageCompleted = 60;
            t.Save();

            Assert.That(t.PercentageCompleted, Is.EqualTo(new decimal?(100)));
        }
    }
}
