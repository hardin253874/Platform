// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core.Cache.Providers;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Core.Cache.Providers
{
    [TestFixture]
    public class RedisPubSubMessageTests
    {
        [Test]
        public void Ctor_NullArray()
        {
            Assert.That(
                () => new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, (int[]) null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("keys")
            );
        }

        [Test]
        public void Ctor_NullIEnumerable()
        {
            Assert.That(
                () => new RedisPubSubCacheMessage<int>(RedisPubSubCacheMessageAction.Remove, (IEnumerable<int>)null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("keys")
            );
        }

        [Test]
        public void Ctor()
        {
            RedisPubSubCacheMessage<int> message;
            const RedisPubSubCacheMessageAction action = RedisPubSubCacheMessageAction.Remove;
            int[] keys = new[] {1, 2};

            message = new RedisPubSubCacheMessage<int>(action, keys);
            Assert.That(message, Has.Property("Action").EqualTo(action));
            Assert.That(message, Has.Property("Keys").EqualTo(keys));
        }
    }
}
