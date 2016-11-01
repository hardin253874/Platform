// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.MessageQueue;
using EDC.ReadiNow.Model;
using EDC.Remote;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.MessageQueue
{
    /// <summary>
    /// Tests for <see cref="MessageQueueSender"/>.
    /// </summary>
    [TestFixture]
    public class MessageQueueSenderTests
    {
        /// <summary>
        /// Basic test for <see cref="MessageQueueSender.Send{T}"/>.
        /// </summary>
        [Test]
        public void TestSend()
        {
            // arrange
            var mq = new MessageQueueMock();
            mq.Setup();

            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.RegisterInstance(mq.Object).As<IMessageQueue>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var sender = new MessageQueueSender();

                // act
                sender.Send("foo", "test");

                // assert
                mq.Receive("foo").Should().Be("\"test\"");
            }
        }

        /// <summary>
        /// Basic test for <see cref="MessageQueueSender.Publish{T}"/>.
        /// </summary>
        [Test]
        public void TestPublish()
        {
            // arrange
            var mq = new MessageQueueMock();
            mq.Setup();

            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.RegisterInstance(mq.Object).As<IMessageQueue>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var sender = new MessageQueueSender();

                // act
                sender.Publish("foo", "test");

                // assert
                mq.Subscribe("foo").Should().Be("\"test\"");
            }
        }

        /// <summary>
        /// Basic test for <see cref="MessageQueueSender.Request{T,TResult}"/>.
        /// </summary>
        [Test]
        [RunWithTransaction]
        [RunAsGlobalTenant]
        public void TestRequest()
        {
            // arrange
            var mq = new MessageQueueMock();
            mq.Setup();

            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.RegisterInstance(mq.Object).As<IMessageQueue>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var sender = new MessageQueueSender();

                var handler = new Mock<IRemoteResponseHandler<string, string>>();

                // act
                sender.Request("foo", "test", handler.Object);

                // assert
                var messageQueueRequest = Entity.GetByField<MessageQueueRequest>("reply_foo", MessageQueueRequest.MessageQueueRequestKey_Field)
                    .OrderByDescending(e => e.CreatedDate)
                    .FirstOrDefault();

                messageQueueRequest.Should().NotBeNull();

                mq.Respond("foo").Should().Be("\"test\"");
            }
        }
    }
}
