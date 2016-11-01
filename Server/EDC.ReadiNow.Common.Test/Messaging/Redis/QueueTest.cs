// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace EDC.ReadiNow.Test.Messaging.Redis
{
    [TestFixture]
    public class QueueTest
    {
        [Test]
        public void TestOrderAndLength()
        {
            var q = CreateQueue();

            Assert.That(q.Length, Is.EqualTo(0));
            Assert.That(q.Dequeue(), Is.EqualTo(null));
            q.Enqueue("first");
            Assert.That(q.Length, Is.EqualTo(1));
            q.Enqueue("second");
            Assert.That(q.Length, Is.EqualTo(2));
            Assert.That(q.Dequeue(), Is.EqualTo("first"));
            Assert.That(q.Length, Is.EqualTo(1));
            Assert.That(q.Dequeue(), Is.EqualTo("second"));
            Assert.That(q.Dequeue(), Is.EqualTo(null));
            Assert.That(q.Length, Is.EqualTo(0));

        }

        [Test]
        public void QueuesAreIndependent()
        {
            var qa = CreateQueue("qa" + DateTime.UtcNow.Ticks);
            var qb = CreateQueue("qb" + DateTime.UtcNow.Ticks);
            
            qa.Enqueue("first");
            Assert.That(qa.Length, Is.EqualTo(1));
            Assert.That(qb.Length, Is.EqualTo(0));
        }



        IQueue<string> CreateQueue(string queueName = null)
        {
            if (queueName == null)
            {
                queueName = "QueueTest " + DateTime.UtcNow.Ticks;
            }

            var mgr = new RedisManager();
            mgr.Connect();

            var q = mgr.GetQueue<string>(queueName);

            return q;
        }
    }
}
