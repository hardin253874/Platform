// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using NUnit.Framework;
using System;
using EDC.ReadiNow.Messaging.Redis;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
    [TestFixture]
    public class QueueActionerTest
    {

        [TestCase(1)]
        [TestCase(5)]
        public void GoldenPath(int concurrency)
        {
            string result = "";
            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { lock(this) result += s; };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {
                    Assert.That(actioner.State, Is.EqualTo(ActionerState.Stopped));

                    q.Enqueue("a");                 // wont be processed until started

                    Thread.Sleep(50);
                    Assert.That(result, Is.Empty);  

                    actioner.Start();

                    Thread.Sleep(50);
                    Assert.That(result, Is.EqualTo("a"));

                    q.Enqueue("b");

                    Thread.Sleep(50);
                    Assert.That(result, Is.EqualTo("ab"));

                    q.Enqueue("c");

                    Thread.Sleep(50);
                    Assert.That(result, Is.EqualTo("abc"));

                    var stopped = actioner.Stop(500);

                    Assert.That(stopped, Is.True);

                    Assert.That(actioner.State, Is.EqualTo(ActionerState.Stopped));

                    q.Enqueue("d");             // wont be processed

                    Thread.Sleep(50);
                    Assert.That(result, Is.EqualTo("abc"));

                }
            }
        }


        [TestCase(1)]
        [TestCase(5)]
        public void ProcessInitialEntries(int concurrency)
        {
            string result = "";

            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { lock(this) result += s; };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {

                    q.Enqueue("a");
                    q.Enqueue("b");
                    q.Enqueue("c");
                    q.Enqueue("d");
                    q.Enqueue("e");
                    q.Enqueue("f");

                    actioner.Start();

                    Thread.Sleep(100);

                    Assert.That(q.Length, Is.EqualTo(0));
                    Assert.That(result.Length, Is.EqualTo(6));
                    Assert.That(result, Is.StringContaining("a"));
                    Assert.That(result, Is.StringContaining("b"));
                    Assert.That(result, Is.StringContaining("c"));
                    Assert.That(result, Is.StringContaining("d"));
                    Assert.That(result, Is.StringContaining("e"));
                    Assert.That(result, Is.StringContaining("f"));
                }
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        public void CanRestart(int concurrency)
        {
            string result = "";

            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { lock(this) result += s; };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {


                    actioner.Start();
                    q.Enqueue("a");

                    Thread.Sleep(100);

                    var stopped = actioner.Stop(500);
                    Assert.That(stopped, Is.True);

                    q.Enqueue("b");

                    actioner.Start();

                    Thread.Sleep(100);
                    q.Enqueue("c");
                    Thread.Sleep(100);

                    Assert.That(result, Is.EqualTo("abc"));
                }
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        public void StoppingStateCorrect(int concurrency)
        {
            string result = "";
            using (var q = CreateQueue())
            {
                Action<string> action = (s) =>
                {
                    Thread.Sleep(200);
                    lock(this) result += s;
                };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {
                    actioner.Start();
                    q.Enqueue("a");
                    Thread.Sleep(100);

                    Assert.That(actioner.Stop(0), Is.False);

                    Assert.That(actioner.State, Is.EqualTo(ActionerState.Stopping));

                    Assert.That(actioner.Stop(200), Is.True);

                    Assert.That(actioner.State, Is.EqualTo(ActionerState.Stopped));

                    Assert.That(result, Is.EqualTo("a"));

                }
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        public void AbortsCleanly(int concurrency)
        {
            string result = "";

            using (var q = CreateQueue())
            {
                Action<string> action = (s) =>
                {
                    Thread.Sleep(100);
                    lock(this) result += s;
                };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {
                    actioner.Start();
                    q.Enqueue("a");
                    Thread.Sleep(50);

                    actioner.Stop();
                }
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        public void DoubleStartDoesNothing(int concurrency)
        {
            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {
                    actioner.Start();
                    actioner.Start();
                }
            }
        }


        [TestCase(1)]
        [TestCase(5)]
        public void DoubleStopDoesNothing(int concurrency)
        {
            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { Thread.Sleep(100); };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {
                    actioner.Start();
                    Thread.Sleep(50);
                    actioner.Stop(1);
                    actioner.Stop(1);
                }
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        public void EnqueuingStoppedDoesNothing(int concurrency)
        {
            string test = null;

            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { test = s; };

                using (var actioner = new QueueActioner<string>(q, action, concurrency))
                {
                    actioner.Queue.Enqueue("dummy");
                    Thread.Sleep(50);
                    Assert.That(test == null);
                }
            }
        }


        [Test]
        public void ExceptionInActionOK()
        {
            using (var q = CreateQueue())
            {
                Action<string> action = (s) => { throw new Exception("This needs to go somewhere"); };

                using (var actioner = new QueueActioner<string>(q, action, 1))
                {
                    actioner.Start();
                    q.Enqueue("a");
                    q.Enqueue("b");
                    Thread.Sleep(50);
                    Assert.That(q.Length, Is.EqualTo(0));
                }
            }
        }



        [Test(Description ="A faster concurrency test that does not use the redis queue so can process more events")]
        [Timeout(30000)]
        public void ConcurrencyTest_Listener()
        {
            using (var q = CreateQueue())
            {
                ConcurrencyQueueTester(q, 5, 10, 500, 10);
            }
        }

        [Test]
        [Timeout(30000)]
        public void ConcurrencyTest_Full()
        {
            var mgr = new RedisManager();
            mgr.Connect();
            var innerQ = mgr.GetQueue<string>("FullConcurrencyTest" + Guid.NewGuid().ToString(), false);

            using (var q = new ListeningQueue<string>(innerQ, mgr))
            {
                ConcurrencyQueueTester(q, 5, 10, 100, 40);
            }
        }

        public void ConcurrencyQueueTester(IListeningQueue<string> q, int machineCount, int threadCount, int messagesPerThread, int actionSleep)
        { 
            var rand = new Random(1234);

            int runCount = 0;
            int sentCount = 0;

            var sb = new StringBuilder(10000);

            var actioners = new List<QueueActioner<string>>(threadCount);

            try
            {
                for (int i = 0; i < machineCount; i++)
                {
                    var actioner = new QueueActioner<string>(q, (s) => 
                    {

                        lock(this)
                        {
                            runCount++;
                            if (runCount % 10 == 0)
                            {
                                sb.Append($"sentCount: {sentCount} \trunCount: {runCount}\n");
                            }
                        }
                        Thread.Sleep(actionSleep);

                    }, threadCount);
                    actioners.Add(actioner);
                    actioner.Start();
                    }


                for (int i=0; i< messagesPerThread * threadCount; i++)
                {
                    q.Enqueue("");
                    sentCount++;

                    var needToSleep = sentCount - runCount >  threadCount / 2;

                    if (needToSleep)
                        sb.Append("Getting too far ahead. Sleeping\n");

                    while (sentCount - runCount > threadCount / 2)
                        Thread.Sleep(5);

                    if (needToSleep)
                        sb.Append("Waking Up\n");

                }


                while (q.Length > 0)
                    Thread.Sleep(100);

                foreach (var actioner in actioners)
                    actioner.Stop(1000);

                sb.Append($"============================================\nsentCount: {sentCount} \trunCount: {runCount}\n");

                Assert.That(runCount, Is.EqualTo(sentCount));
            }
            finally
            {
                Console.Write(sb.ToString());

                foreach (var actioner in actioners)
                    actioner.Dispose();
            }
        }

        IListeningQueue<string> CreateQueue()
        {
            var mgr = new RedisManager();
            mgr.Connect();
            var q = new InMemoryTestQueue<string>();
            return new ListeningQueue<string>(q, mgr);
        }
    }

   
}
