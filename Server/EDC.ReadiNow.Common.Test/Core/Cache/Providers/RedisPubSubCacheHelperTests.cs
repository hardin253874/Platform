// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Core.Cache.Providers;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
    public class RedisPubSubCacheHelperTests
    {
        [Test]
        public void GetChannelName_Null()
        {
            Assert.That(
                () => RedisPubSubCacheHelpers.GetChannelName(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("cacheName")
            );
        }

        [Test]
        [TestCase("a", "a Cache")]
        [TestCase("a b", "a b Cache")]
        public void GetChannelName(string cacheName, string expectedCacheName)
        {
            Assert.That(
                RedisPubSubCacheHelpers.GetChannelName(cacheName),
                Is.EqualTo(expectedCacheName)
            );
        }

        [Test]
        public void MergeAction_NullExistingMessage()
        {
            Assert.That(
                () => RedisPubSubCacheHelpers.MergeAction(null, new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove)),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("existingMessage")
            );    
        }

        [Test]
        public void MergeAction_NullNewMessage()
        {
            Assert.That(
                () => RedisPubSubCacheHelpers.MergeAction(new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("newMessage")
            );
        }

        [Test]
        [TestCaseSource("MergeAction_TestCaseSource")]
        public void MergeAction(RedisPubSubCacheMessage<int> existingMessage, RedisPubSubCacheMessage<int> newMessage,
            RedisPubSubCacheMessage<int> expectedMergedMessage)
        {
            RedisPubSubCacheHelpers.MergeAction(existingMessage, newMessage);

            Assert.That(existingMessage, Is.EqualTo(expectedMergedMessage));
        }

        private IEnumerable<TestCaseData> MergeAction_TestCaseSource()
        {
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 2, 3),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Clear)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1)
            );
            yield return new TestCaseData(
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 2),
                new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, 1, 2)
            );
        }
    }
}
