// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Threading.Tasks;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Test.Messaging
{
    /// <summary>
	/// </summary>
	[TestFixture]
    [RunWithTransaction]
    public class DeferredChannelMessageContextEntryTests
    {
        private ISuppression _suppression;

        [Test]
        public void Test_AddOrUpdateMessage_Null_Channel()
        {
            DeferredChannelMessageContextEntry entry = new DeferredChannelMessageContextEntry();
            Assert.Throws<ArgumentNullException>(() => entry.AddOrUpdateMessage(null, new TestMessage1(), null));
        }

        [Test]
        public void Test_AddOrUpdateMessage_Null_Message()
        {
            DeferredChannelMessageContextEntry entry = new DeferredChannelMessageContextEntry();
            entry.AddOrUpdateMessage<TestMessage1>("Channel", null, null);

            TestMessage1 msg;
            Assert.That(entry.TryGetMessage("Channel", out msg), Is.False, "Entry contains msg");
        }

        [Test]
        public void Test_Ctor()
        {
            DeferredChannelMessageContextEntry entry = new DeferredChannelMessageContextEntry(Entity.DistributedMemoryManager);
            Assert.IsNotNull(entry);
        }

        [Test]
        public void Test_Ctor_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new DeferredChannelMessageContextEntry(null));
        }

        [Test]
        public void Test_FlushChannelMessages()
        {
            TestMessage1 msg1 = new TestMessage1();
            msg1.Data.Add(100);
            msg1.Data.Add(200);
            string channel1Name = "TestChannel1";

            TestMessage2 msg2 = new TestMessage2();
            msg2.Data.Add(1000);
            msg2.Data.Add(2000);
            string channel2Name = "TestChannel2";

            // Setup the mocks
            MockDistributedMemoryManager distributedMemoryManagerMock = new MockDistributedMemoryManager();            

            DeferredChannelMessageContextEntry entry = new DeferredChannelMessageContextEntry(distributedMemoryManagerMock);
            entry.AddOrUpdateMessage(channel1Name, msg1, null);
            entry.AddOrUpdateMessage(channel2Name, msg2, null);
            entry.FlushMessages();

            TestMessage1 msg1Out;
            TestMessage2 msg2Out;

            Assert.IsFalse(entry.TryGetMessage(channel1Name, out msg1Out), "Channel1 should have no messages");
            Assert.IsFalse(entry.TryGetMessage(channel2Name, out msg2Out), "Channel2 should have no messages");

            // Validation
            MockChannel<TestMessage1> channel1 = distributedMemoryManagerMock.CreatedChannels[channel1Name] as MockChannel<TestMessage1>;
            Assert.IsNotNull(channel1, "{0} was not found", channel1Name);
            Assert.AreEqual(1, channel1.PublishedMessages.Count);
            Assert.AreSame(msg1, channel1.PublishedMessages[0]);

            MockChannel<TestMessage2> channel2 = distributedMemoryManagerMock.CreatedChannels[channel2Name] as MockChannel<TestMessage2>;
            Assert.IsNotNull(channel2, "{0} was not found", channel2Name);
            Assert.AreEqual(1, channel2.PublishedMessages.Count);
            Assert.AreSame(msg2, channel2.PublishedMessages[0]);
        }

        [Test]
        public void Test_AddOrUpdateMessage_Merge_FlushChannelMessages()
        {
            TestMessage1 msg11 = new TestMessage1();
            msg11.Data.Add(100);
            msg11.Data.Add(200);
            TestMessage1 msg12 = new TestMessage1();
            msg12.Data.Add(300);
            msg12.Data.Add(400);
            string channel1Name = "TestChannel1";

            TestMessage2 msg21 = new TestMessage2();
            msg21.Data.Add(1000);
            msg21.Data.Add(2000);
            TestMessage2 msg22 = new TestMessage2();
            msg22.Data.Add(3000);
            msg22.Data.Add(4000);
            msg22.Data.Add(5000);
            string channel2Name = "TestChannel2";

            // Setup the mocks
            MockDistributedMemoryManager distributedMemoryManagerMock = new MockDistributedMemoryManager();

            DeferredChannelMessageContextEntry entry = new DeferredChannelMessageContextEntry(distributedMemoryManagerMock);            
            entry.AddOrUpdateMessage(channel1Name, msg11, (e, n) => e.Data.UnionWith(n.Data));
            entry.AddOrUpdateMessage(channel1Name, msg12, (e, n) => e.Data.UnionWith(n.Data)); // This will merge
            entry.AddOrUpdateMessage(channel2Name, msg21, (e, n) => e.Data.UnionWith(n.Data));
            entry.AddOrUpdateMessage(channel2Name, msg22, (e, n) => e.Data.UnionWith(n.Data)); // This will merge
            entry.FlushMessages();

            TestMessage1 msg1Out;
            TestMessage2 msg2Out;

            Assert.IsFalse(entry.TryGetMessage(channel1Name, out msg1Out), "Channel1 should have no messages");
            Assert.IsFalse(entry.TryGetMessage(channel2Name, out msg2Out), "Channel2 should have no messages");

            // Validation
            MockChannel<TestMessage1> channel1 = distributedMemoryManagerMock.CreatedChannels[channel1Name] as MockChannel<TestMessage1>;
            Assert.IsNotNull(channel1, "{0} was not found", channel1Name);
            Assert.AreEqual(1, channel1.PublishedMessages.Count); // Should still have only one message as it has been merged            

            SortedSet<int> msg1Data = new SortedSet<int>();
            msg1Data.UnionWith(msg11.Data);
            msg1Data.UnionWith(msg12.Data);

            Assert.AreEqual(msg1Data.Count, channel1.PublishedMessages[0].Data.Count);
            Assert.IsTrue(channel1.PublishedMessages[0].Data.SetEquals(msg1Data));                        

            MockChannel<TestMessage2> channel2 = distributedMemoryManagerMock.CreatedChannels[channel2Name] as MockChannel<TestMessage2>;
            Assert.IsNotNull(channel2, "{0} was not found", channel2Name);
            Assert.AreEqual(1, channel2.PublishedMessages.Count); // Should still have only one message as it has been merged

            SortedSet<int> msg2Data = new SortedSet<int>();
            msg2Data.UnionWith(msg21.Data);
            msg2Data.UnionWith(msg22.Data);

            Assert.AreEqual(msg2Data.Count, channel2.PublishedMessages[0].Data.Count);
            Assert.IsTrue(channel2.PublishedMessages[0].Data.SetEquals(msg2Data));
        }

        [Test]
        public void Test_TryGetMessage_Null_Channel()
        {
            TestMessage1 msg;
            DeferredChannelMessageContextEntry entry = new DeferredChannelMessageContextEntry();
            Assert.Throws<ArgumentNullException>(() => entry.TryGetMessage(null, out msg));
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

        /// <summary>
        /// Mock channel. Moq doesn't play well with the reflection calls made
        /// in DeferredChannelMessageContextEntry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class MockChannel<T> : IChannel<T>
        {
            #region Mocked Methods and Properties

            public MockChannel(string channelName)
            {
                ChannelName = channelName;
                PublishedMessages = new List<T>();
            }

            public string ChannelName
            {
                get;
                private set;
            }

            public List<T> PublishedMessages { get; private set; }

            public void Dispose()
            {
            }

            public void Publish(T message, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null)
            {
                PublishedMessages.Add(message);
            }

            public void Publish(T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null)
            {
                PublishedMessages.Add(message);
            }

			public System.Threading.Tasks.Task<long> PublishAsync( T message, PublishMethod publishMethod, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
			{
				PublishedMessages.Add( message );

				return Task.FromResult<long>( 0 );
			}

			public System.Threading.Tasks.Task<long> PublishAsync( T message, PublishOptions options = PublishOptions.None, bool publishToOriginator = false, Action<T, T> mergeAction = null )
			{
				PublishedMessages.Add( message );

				return Task.FromResult<long>( 0 );
			}

			#endregion Mocked Methods and Properties

#pragma warning disable 67
			public event EventHandler<MessageEventArgs<T>> MessageReceived;
#pragma warning restore 67

			public void Subscribe()
            {
                throw new NotImplementedException();
            }

            public void Unsubscribe()
            {
                throw new NotImplementedException();
            }
		}

        /// <summary>
        /// Mock distributedMemoryManager. Moq doesn't play well with the reflection calls made
        /// in DeferredChannelMessageContextEntry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class MockDistributedMemoryManager : IDistributedMemoryManager
        {
            #region Mocked Methods and Properties
            public MockDistributedMemoryManager()
            {
                CreatedChannels = new Dictionary<string, object>();
            }

            public void Dispose()
            {
                return;
            }

            public IChannel<T> GetChannel<T>(string channelName)
            {
                var channel = new MockChannel<T>(channelName);
                CreatedChannels[channelName] = channel;
                return channel;                
            }

            public Dictionary<string, object> CreatedChannels { get; private set; }

            #endregion Mocked Methods and Properties

            #region Ignored Methods and Properties

            public bool IsConnected
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void Connect(bool allowAdmin = false)
            {
                throw new NotImplementedException();
            }

            public void Disconnect()
            {
                throw new NotImplementedException();
            }

            public IMemoryStore GetMemoryStore()
            {
                throw new NotImplementedException();
            }

            public ISuppression Suppress()
            {
                throw new NotImplementedException();
            }

            #endregion Ignored Methods and Properties
        }

        private class TestMessage1
        {
            public TestMessage1()
            {
                Data = new SortedSet<int>();
            }

            public SortedSet<int> Data { get; private set; }
        }

        private class TestMessage2
        {
            public TestMessage2()
            {
                Data = new SortedSet<int>();
            }

            public SortedSet<int> Data { get; private set; }
        }
    }
}