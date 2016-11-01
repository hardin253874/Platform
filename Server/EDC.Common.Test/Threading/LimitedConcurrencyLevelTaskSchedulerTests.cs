// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Common.Threading;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDC.Test.Threading
{

	[TestFixture]
    public class LimitedConcurrencyLevelTaskSchedulerTests
	{

        [Test]
        [Description("Crude test to make sure it is running the tasks")]
        public void TasksRun()
        {
            var sch = new LimitedConcurrencyLevelTaskScheduler(3) { };

            TaskFactory tf = new TaskFactory(sch);
            var count = 0;
            var tasks = new List<Task>();
            tasks.Add(tf.StartNew(() =>
            {
                count++;
            }));

            tasks.Add(tf.StartNew(() =>
            {
                count++;
            }));

            Task.WaitAll(tasks.ToArray());

            Assert.That(count, Is.EqualTo(2));
        }

	}
}