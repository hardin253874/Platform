// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
    public class RedisPubSubCacheTests
    {
        public readonly TimeSpan DefaultRedisWaitTime = TimeSpan.FromSeconds(2.5);

        [Test]
        public void Ctor_NotConnected()
        {
            MockRepository mockRepository;
            const string name = "Name";
            DictionaryCache<long, string> dictionaryCache;
            Mock<IDistributedMemoryManager> memoryManagerMock;
            Mock<IChannel<RedisPubSubCacheMessage<long>>> channelMock;

            mockRepository = new MockRepository(MockBehavior.Strict);
            dictionaryCache = new DictionaryCache<long, string>();
            channelMock = mockRepository.Create<IChannel<RedisPubSubCacheMessage<long>>>();
            channelMock.Setup(c => c.Subscribe());
            channelMock.Setup(c => c.Dispose());
            memoryManagerMock = mockRepository.Create<IDistributedMemoryManager>();
            memoryManagerMock.Setup(mm => mm.IsConnected).Returns(false);
            memoryManagerMock.Setup(mm => mm.Connect(It.IsAny<bool>()));
            memoryManagerMock.Setup(mm => mm.GetChannel<RedisPubSubCacheMessage<long>>(
                RedisPubSubCacheHelpers.GetChannelName(name))).Returns(channelMock.Object);

            using (RedisPubSubCache<long, string> redisCache = 
                new RedisPubSubCache<long, string>(dictionaryCache, name, memoryManagerMock.Object))
            {
                Assert.That(redisCache, Has.Property("Name").EqualTo(name));
                Assert.That(redisCache, Has.Property("InnerCache").SameAs(dictionaryCache));
                Assert.That(redisCache.MemoryManager, Is.SameAs(memoryManagerMock.Object));
                Assert.That(redisCache.Channel, Is.SameAs(channelMock.Object));
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void Ctor_Connected()
        {
            MockRepository mockRepository;
            const string name = "Name";
            DictionaryCache<long, string> dictionaryCache;
            Mock<IDistributedMemoryManager> memoryManagerMock;
            Mock<IChannel<RedisPubSubCacheMessage<long>>> channelMock;

            mockRepository = new MockRepository(MockBehavior.Strict);
            dictionaryCache = new DictionaryCache<long, string>();
            channelMock = mockRepository.Create<IChannel<RedisPubSubCacheMessage<long>>>();
            channelMock.Setup(c => c.Subscribe());
            channelMock.Setup(c => c.Dispose());
            memoryManagerMock = mockRepository.Create<IDistributedMemoryManager>();
            memoryManagerMock.Setup(mm => mm.IsConnected).Returns(true);
            memoryManagerMock.Setup(mm => mm.GetChannel<RedisPubSubCacheMessage<long>>(
                RedisPubSubCacheHelpers.GetChannelName(name))).Returns(channelMock.Object);

            using (RedisPubSubCache<long, string> redisCache =
				new RedisPubSubCache<long, string>( dictionaryCache, name, memoryManagerMock.Object ) )
            {
                Assert.That(redisCache, Has.Property("Name").EqualTo(name));
                Assert.That(redisCache, Has.Property("InnerCache").SameAs(dictionaryCache));
                Assert.That(redisCache.MemoryManager, Is.SameAs(memoryManagerMock.Object));
                Assert.That(redisCache.Channel, Is.SameAs(channelMock.Object));
            }

            mockRepository.VerifyAll();
        }

        [Test]
        public void Ctor_NullInnerCache()
        {
            Assert.That(
				( ) => new RedisPubSubCache<long, string>(
                    null, 
                    "name",
                    new Mock<IDistributedMemoryManager>().Object
                ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("innerCache"));
        }

        [Test]
        public void Ctor_NullName()
        {
            Assert.That(
				( ) => new RedisPubSubCache<long, string>(
                    new Mock<ICache<long, string>>().Object,
					null,
                    new Mock<IDistributedMemoryManager>().Object
                ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cacheName"));
        }

        [Test]
        public void Ctor_NullMemoryManager()
        {
            Assert.That(
				( ) => new RedisPubSubCache<long, string>(
                    new Mock<ICache<long, string>>().Object,
					"name",
                    null
                ),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("memoryManager"));
        }

        [Test]
        public void Remove_NoSuppression()
        {
            const string cacheName = "My Precious";
            Tuple<long, string>[] testData =
            {
                new Tuple<long, string>(144, "foo"), 
                new Tuple<long, string>(96, "bar")
            };
            string value;

            using (IDistributedMemoryManager memoryManager = new RedisManager())
			using ( RedisPubSubCache<long, string> redisPubSubCache1 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
			using ( RedisPubSubCache<long, string> redisPubSubCache2 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
            {
                foreach (Tuple<long, string> testDatum in testData)
                {
                    redisPubSubCache1.Add(testDatum.Item1, testDatum.Item2);
                    redisPubSubCache2.Add(testDatum.Item1, testDatum.Item2);
                }

                redisPubSubCache1.Remove(testData[0].Item1);

                // Wait for message
                Thread.Sleep(DefaultRedisWaitTime);

                Assert.That(redisPubSubCache1.TryGetValue(testData[0].Item1, out value), Is.False, "Item not removed from cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[0].Item1, out value), Is.False, "Item not removed from cache 2");
                Assert.That(redisPubSubCache1.TryGetValue(testData[1].Item1, out value), Is.True, "Item 2 removed from cache 1");
                Assert.That(value, Is.EqualTo(testData[1].Item2), "Item 2 incorrect value in cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[1].Item1, out value), Is.True, "Item 2 removed from cache 2");
                Assert.That(value, Is.EqualTo(testData[1].Item2), "Item 2 incorrect value in cache 2");
            }
        }

        [Test]
        public void Remove_Suppression()
        {
            const string cacheName = "My Precious";
            Tuple<long, string>[] testData =
            {
                new Tuple<long, string>(144, "foo"), 
                new Tuple<long, string>(96, "bar")
            };
            string value;

            using (IDistributedMemoryManager memoryManager = new RedisManager())
			using ( RedisPubSubCache<long, string> redisPubSubCache1 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
			using ( RedisPubSubCache<long, string> redisPubSubCache2 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
            {
                foreach (Tuple<long, string> testDatum in testData)
                {
                    redisPubSubCache1.Add(testDatum.Item1, testDatum.Item2);
                    redisPubSubCache2.Add(testDatum.Item1, testDatum.Item2);
                }

                using (new RedisCacheMessageSuppressionContext(cacheName))
                {
                    redisPubSubCache1.Remove(testData[0].Item1);
                }

                // Wait for message, just in case
                Thread.Sleep(DefaultRedisWaitTime);

                Assert.That(redisPubSubCache1.TryGetValue(testData[0].Item1, out value), Is.False, "Item not removed from cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[0].Item1, out value), Is.True, "Item removed from cache 2");
                Assert.That(redisPubSubCache1.TryGetValue(testData[1].Item1, out value), Is.True, "Item 2 removed from cache 1");
                Assert.That(value, Is.EqualTo(testData[1].Item2), "Item 2 incorrect value in cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[1].Item1, out value), Is.True, "Item 2 removed from cache 2");
                Assert.That(value, Is.EqualTo(testData[1].Item2), "Item 2 incorrect value in cache 2");
            }
        }

        [Test]
        public void Clear_NoSupression()
        {
            const string cacheName = "My Precious";
            Tuple<long, string>[] testData =
            {
                new Tuple<long, string>(26, "foo"), 
                new Tuple<long, string>(986, "bar")
            };
            string value;

            using (IDistributedMemoryManager memoryManager = new RedisManager())
			using ( RedisPubSubCache<long, string> redisPubSubCache1 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
			using ( RedisPubSubCache<long, string> redisPubSubCache2 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
            {
                foreach (Tuple<long, string> testDatum in testData)
                {
                    redisPubSubCache1.Add(testDatum.Item1, testDatum.Item2);
                    redisPubSubCache2.Add(testDatum.Item1, testDatum.Item2);
                }

                redisPubSubCache1.Clear();

                // Wait for message
                Thread.Sleep(DefaultRedisWaitTime);

                Assert.That(redisPubSubCache1.TryGetValue(testData[0].Item1, out value), Is.False, "Item 1 not removed from cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[0].Item1, out value), Is.False, "Item 1 not removed from cache 2");
                Assert.That(redisPubSubCache1.TryGetValue(testData[1].Item1, out value), Is.False, "Item 2 not removed from cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[1].Item1, out value), Is.False, "Item 2 not removed from cache 2");
                Assert.That(redisPubSubCache1, Is.Empty, "Cache 1 not empty");
                Assert.That(redisPubSubCache2, Is.Empty, "Cache 2 not empty");
            }
        }

        [Test]
        public void Clear_Supression()
        {
            const string cacheName = "My Precious";
            Tuple<long, string>[] testData =
            {
                new Tuple<long, string>(26, "foo"), 
                new Tuple<long, string>(986, "bar")
            };
            string value;

            using (IDistributedMemoryManager memoryManager = new RedisManager())
			using ( RedisPubSubCache<long, string> redisPubSubCache1 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
			using ( RedisPubSubCache<long, string> redisPubSubCache2 = new RedisPubSubCache<long, string>(
				new DictionaryCache<long, string>( ), cacheName, memoryManager ) )
            {
                foreach (Tuple<long, string> testDatum in testData)
                {
                    redisPubSubCache1.Add(testDatum.Item1, testDatum.Item2);
                    redisPubSubCache2.Add(testDatum.Item1, testDatum.Item2);
                }

                using (new RedisCacheMessageSuppressionContext(cacheName))
                {
                    redisPubSubCache1.Clear();
                }

                // Wait for message
                Thread.Sleep(DefaultRedisWaitTime);

                Assert.That(redisPubSubCache1.TryGetValue(testData[0].Item1, out value), Is.False, "Item 1 not removed from cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[0].Item1, out value), Is.True, "Item 1 removed from cache 2");
                Assert.That(redisPubSubCache1.TryGetValue(testData[1].Item1, out value), Is.False, "Item 2 not removed from cache 1");
                Assert.That(redisPubSubCache2.TryGetValue(testData[1].Item1, out value), Is.True, "Item 2 removed from cache 2");
                Assert.That(redisPubSubCache1, Is.Empty, "Cache 1 not empty");
                Assert.That(redisPubSubCache2, Is.Not.Empty, "Cache 2 not empty");
            }
        }
    }
}
