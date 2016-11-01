// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Database;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EDC.ReadiNow.Test.Messaging
{
    /// <summary>
	/// </summary>
	[TestFixture]    
    public class DeferredChannelMessageContextTests
    {
        private ISuppression _suppression;

        [Test]
        public void Test_AddOrUpdateMessage_TryGetMessage()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {
                TestMessage msg = new TestMessage();
                msg.Data.Add(1);
                context.AddOrUpdateMessage("TestChannel", msg, null);

                TestMessage addedMsg;
                Assert.That(context.TryGetMessage("TestChannel", out addedMsg), Is.True, "TryGetMessage failed");

                Assert.AreSame(msg, addedMsg);
            }
        }

        [Test]
        public void Test_AddOrUpdateMessage_Null_Channel()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {
                TestMessage msg = new TestMessage();
                msg.Data.Add(1);
                Assert.Throws<ArgumentNullException>(()=> context.AddOrUpdateMessage(null, msg, null));                
            }
        }

        [Test]
        public void Test_AddOrUpdateMessage_Null_Message()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {                
                context.AddOrUpdateMessage<TestMessage>("TestChannel", null, null);

                TestMessage msg;
                Assert.That(context.TryGetMessage("TestChannel", out msg), Is.False, "TryGetMessage failed");                
            }
        }

        [Test]
        public void Test_TryGetMessage_Null_Channel()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {
                TestMessage msg;
                Assert.Throws<ArgumentNullException>(() => context.TryGetMessage(null, out msg));
            }
        }

        [Test]
        public void Test_AddOrUpdateMessage_Merge_TryGetMessage_Message()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {
                TestMessage msg = new TestMessage();
                msg.Data.Add(1);
                context.AddOrUpdateMessage("TestChannel", msg, null);

                TestMessage msg2 = new TestMessage();
                msg2.Data.Add(2);

                context.AddOrUpdateMessage("TestChannel", msg2, (e, n) => e.Data.UnionWith(n.Data));

                TestMessage addedMsg;
                Assert.That(context.TryGetMessage("TestChannel", out addedMsg), Is.True, "TryGetMessage failed");

                Assert.AreEqual(2, addedMsg.Data.Count);
                Assert.Contains(1, addedMsg.Data);
                Assert.Contains(2, addedMsg.Data);
            }
        }

        [Test]
        public void Test_AddOrUpdateMessage_Merge_No_Merge_Function()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {
                TestMessage msg = new TestMessage();
                msg.Data.Add(1);
                context.AddOrUpdateMessage("TestChannel", msg, null);

                TestMessage msg2 = new TestMessage();
                msg2.Data.Add(2);

                Assert.Throws<InvalidOperationException>(() => context.AddOrUpdateMessage("TestChannel", msg2, null));
            }
        }

        [Test]
        public void Test_AddOrUpdateMessage_MultipleLevels()
        {
            TestMessage msg;
            TestMessage msgOuter = new TestMessage();
            TestMessage msgInner = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                outerContext.AddOrUpdateMessage("OuterChannel", msgOuter, null);

                Assert.That(outerContext.TryGetMessage("OuterChannel", out msg), Is.True, "Outer context missing msg");

                using (DeferredChannelMessageContext innerContext = new DeferredChannelMessageContext())
                {
                    innerContext.AddOrUpdateMessage("InnerChannel", msgInner, null);

                    Assert.That(innerContext.TryGetMessage("InnerChannel", out msg), Is.True, "Inner context missing msg");
                    Assert.That(innerContext.TryGetMessage("OuterChannel", out msg), Is.False, "Inner context contains msg");

                    Assert.That(outerContext.TryGetMessage("OuterChannel", out msg), Is.True, "Outer context missing msg");
                    Assert.That(outerContext.TryGetMessage("InnerChannel", out msg), Is.False, "Outer context contains msg");
                }

                Assert.That(outerContext.TryGetMessage("OuterChannel", out msg), Is.True, "Outer context missing msg");
                Assert.That(outerContext.TryGetMessage("InnerChannel", out msg), Is.False, "Outer context contains msg");
            }
        }

        [Test]
        public void Test_AttachedContext()
        {
            TestMessage msg;
            TestMessage msgAttached = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                using (DeferredChannelMessageContext attachedContext = new DeferredChannelMessageContext(ContextType.Attached))
                {
                    attachedContext.AddOrUpdateMessage("Attached", msgAttached, null);

                    Assert.That(attachedContext.TryGetMessage("Attached", out msg), Is.True, "Attached context missing msg");

                    Assert.That(outerContext.TryGetMessage("Attached", out msg), Is.True, "Outer context missing msg");

                    Assert.AreSame(msg, msgAttached);
                }
            }
        }

        [Test]
        public void Test_AttachedContext_Does_Not_Flush_On_Dispose()
        {
            TestMessage msg;
            TestMessage msgAttached = new TestMessage();

            DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext();
            DeferredChannelMessageContext attachedContext = new DeferredChannelMessageContext(ContextType.Attached);
                        
            attachedContext.AddOrUpdateMessage("Attached", msgAttached, null);

            Assert.That(attachedContext.TryGetMessage("Attached", out msg), Is.True, "Attached context missing msg");
            Assert.That(outerContext.TryGetMessage("Attached", out msg), Is.True, "Outer context missing msg");
            Assert.AreSame(msg, msgAttached);

            // Disposing inner context should not flush the messages    
            attachedContext.Dispose();
            
            Assert.That(attachedContext.TryGetMessage("Attached", out msg), Is.True, "Attached context missing msg");
            Assert.That(outerContext.TryGetMessage("Attached", out msg), Is.True, "Outer context missing msg");
            Assert.AreSame(msg, msgAttached);

            // Disposing outer context should flush the messages    
            outerContext.Dispose();

            Assert.That(attachedContext.TryGetMessage("Attached", out msg), Is.False, "Attached context contains msg");
            Assert.That(outerContext.TryGetMessage("Attached", out msg), Is.False, "Outer context contains msg");
        }

        [Test]
        public void Test_Ctor()
        {
            using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
            {
                Assert.That(context, Has.Property("ContextType").EqualTo(ContextType.New));
            }
        }

        [Test]
        public void Test_DetachedContext()
        {
            TestMessage msg;
            TestMessage msgDetached = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                using (DeferredChannelMessageContext detachedContext = new DeferredChannelMessageContext(ContextType.Detached))
                {
                    detachedContext.AddOrUpdateMessage("Detached", msgDetached, null);

                    Assert.That(detachedContext.TryGetMessage("Detached", out msg), Is.True, "Detached context missing msg");

                    Assert.That(outerContext.TryGetMessage("Detached", out msg), Is.False, "Outer context contains msg");
                }
            }
        }

        [Test]
        public void Test_Dispose()
        {
            TestMessage msg;
            TestMessage msg1 = new TestMessage();
            TestMessage msg2 = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                outerContext.AddOrUpdateMessage("Channel1", msg1, null);
                outerContext.AddOrUpdateMessage("Channel2", msg2, null);

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg1);

                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg2);

                outerContext.Dispose();

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.False, "Outer context contains msg");
                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.False, "Outer context contains msg");
            }
        }

        [Test]
        public void Test_Dispose_DifferentThread()
        {
            TestMessage msg;
            TestMessage msg1 = new TestMessage();
            TestMessage msg2 = new TestMessage();            

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                outerContext.AddOrUpdateMessage("Channel1", msg1, null);
                outerContext.AddOrUpdateMessage("Channel2", msg2, null);

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg1);

                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg2);

                // Call the dispose on a different thread. 
                // This tests the Begin Request/End Request creation and disposal
                Thread tSetup = new Thread(new ThreadStart(() => { outerContext.Dispose(); }));
                tSetup.Start();
                tSetup.Join();                

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.False, "Outer context contains msg");
                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.False, "Outer context contains msg");
            }
        }

        [Test]
        public void Test_FlushMessages()
        {
            TestMessage msg;
            TestMessage msg1 = new TestMessage();
            TestMessage msg2 = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                outerContext.AddOrUpdateMessage("Channel1", msg1, null);
                outerContext.AddOrUpdateMessage("Channel2", msg2, null);

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg1);

                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg2);

                outerContext.FlushMessages();

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.False, "Outer context contains msg");
                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.False, "Outer context contains msg");
            }
        }

        [Test]
        public void Test_GetContext()
        {
            Assert.That(DeferredChannelMessageContext.IsSet(), Is.False, "Set initially");

            using (DeferredChannelMessageContext newContext = new DeferredChannelMessageContext())
            {
                using (DeferredChannelMessageContext attachedContext = DeferredChannelMessageContext.GetContext())
                {
                    Assert.That(newContext.ContextType, Is.EqualTo(ContextType.New), "Incorrect context type");
                    Assert.That(attachedContext.ContextType, Is.EqualTo(ContextType.Attached), "Incorrect context type");
                }

                Assert.That(DeferredChannelMessageContext.IsSet(), Is.True, "Attached Dispose() removed context");
            }

            Assert.That(DeferredChannelMessageContext.IsSet(), Is.False, "New Dispose() did not remove context");
        }

        [Test]
        public void Test_GetContext_NoContext()
        {
            DeferredChannelMessageContext context;

            context = DeferredChannelMessageContext.GetContext();
            Assert.That(context, Has.Property("ContextType").EqualTo(ContextType.Detached));
        }

        [Test]
        public void Test_IsSet_OneLevel()
        {
            Assert.That(DeferredChannelMessageContext.IsSet(), Is.False, "Set beforehand");
            using (new DeferredChannelMessageContext())
            {
                Assert.That(DeferredChannelMessageContext.IsSet(), Is.True, "Not set");
            }
            Assert.That(DeferredChannelMessageContext.IsSet(), Is.False, "Set afterwards");
        }

        [Test]
        public void Test_IsSet_TwoLevels()
        {
            Assert.That(DeferredChannelMessageContext.IsSet(), Is.False, "Set beforehand");
            using (new DeferredChannelMessageContext())
            {
                Assert.That(DeferredChannelMessageContext.IsSet(), Is.True, "Not pre-second level");
                using (new DeferredChannelMessageContext())
                {
                    Assert.That(DeferredChannelMessageContext.IsSet(), Is.True, "Not second level");
                }
                Assert.That(DeferredChannelMessageContext.IsSet(), Is.True, "Not post-second level");
            }
            Assert.That(DeferredChannelMessageContext.IsSet(), Is.False, "Set afterwards");
        }

        [Test]
        public void Test_Committing_Transaction_FlushesMessages()
        {
            TestMessage msg;
            TestMessage msg1 = new TestMessage();
            TestMessage msg2 = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                outerContext.AddOrUpdateMessage("Channel1", msg1, null);
                outerContext.AddOrUpdateMessage("Channel2", msg2, null);

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg1);

                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg2);

                using(DatabaseContext context = DatabaseContext.GetContext(true))
                {
                    context.CommitTransaction();
                }                

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.False, "Outer context contains msg");
                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.False, "Outer context contains msg");
            }
        }

        [Test]
        public void Test_Non_Transactional_Database_Context_Does_Not_FlushesMessages()
        {
            TestMessage msg;
            TestMessage msg1 = new TestMessage();
            TestMessage msg2 = new TestMessage();

            using (DeferredChannelMessageContext outerContext = new DeferredChannelMessageContext())
            {
                outerContext.AddOrUpdateMessage("Channel1", msg1, null);
                outerContext.AddOrUpdateMessage("Channel2", msg2, null);

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg1);

                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.True, "Outer context missing msg");

                Assert.AreSame(msg, msg2);

                using (DatabaseContext context = DatabaseContext.GetContext())
                {                    
                }

                Assert.That(outerContext.TryGetMessage("Channel1", out msg), Is.True, "Outer context missing msg");
                Assert.That(outerContext.TryGetMessage("Channel2", out msg), Is.True, "Outer context missing msg");
            }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            _suppression = Entity.DistributedMemoryManager.Suppress();
        }

        [TestFixtureTearDown]
        public void TestFixtureTeardown()
        {
            if (_suppression != null)
            {
                _suppression.Dispose();
            }
        }

        private class TestMessage
        {
            public TestMessage()
            {
                Data = new SortedSet<int>();
            }

            public SortedSet<int> Data { get; private set; }
        }
    }
}