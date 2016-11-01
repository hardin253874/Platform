// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.MessageQueue;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.MessageQueue
{
    /// <summary>
    /// Tests for <see cref="MessageQueueListener"/>.
    /// </summary>
    [TestFixture]
    public class MessageQueueListenerTests
    {
        /// <summary>
        /// Basic test for <see cref="MessageQueueListener.Receive{T}"/>.
        /// </summary>
        [Test]
        public void TestReceive()
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
                var listener = new MessageQueueListener();

                string consumed = null;

                mq.Send("foo", "\"test\"");
                mq.Consume += (s, e) =>
                {
                    consumed = e.Message;
                };

                // act
                listener.Receive<string>("foo", null);
                listener.Stop();

                // assert
                consumed.Should().Be("\"test\"");
            }
        }

        /// <summary>
        /// Basic test for <see cref="MessageQueueListener.Subscribe{T}"/>.
        /// </summary>
        [Test]
        public void TestSubscribe()
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
                var listener = new MessageQueueListener();

                string consumed = null;

                mq.Publish("foo", "\"test\"");
                mq.Consume += (s, e) =>
                {
                    consumed = e.Message;
                };

                // act
                listener.Subscribe<string>("foo", null);
                listener.Stop();

                // assert
                consumed.Should().Be("\"test\"");
            }
        }

        /// <summary>
        /// Basic test for <see cref="MessageQueueListener.Respond{T,TResult}"/>.
        /// </summary>
        [Test]
        public void TestRespond()
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
                var listener = new MessageQueueListener();

                string consumed = null;

                mq.Request("foo", "\"test\"");
                mq.Consume += (s, e) =>
                {
                    consumed = e.Message;
                };

                // act
                listener.Respond<string, string>("foo", null);
                listener.Stop();

                // assert
                consumed.Should().Be("\"test\"");
            }
        }
    }
}
