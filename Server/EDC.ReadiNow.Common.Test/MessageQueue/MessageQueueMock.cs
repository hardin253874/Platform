// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Text;
using EDC.ReadiNow.MessageQueue;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;

namespace EDC.ReadiNow.Test.MessageQueue
{
    /// <summary>
    /// Delegate handler for a mock consume event fired by <see cref="MessageQueueMock"/>. Can't factor out the consumers properly
    /// so this was the best I could come up with.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event args.</param>
    public delegate void MessageQueueMockConsumeEventHandler(object sender, MessageQueueMockConsumeEventArgs args);
    
    /// <summary>
    /// A mock object used to simulate connecting to a RabbitMQ installation for the purposes of unit testing.
    /// </summary>
    public class MessageQueueMock
    {
        private Mock<IMessageQueue> _mock;

        private readonly IDictionary<string, Queue> _queueProps = new Dictionary<string, Queue>();

        private readonly IDictionary<string, Exchange> _exchangeProps = new Dictionary<string, Exchange>();

        private readonly IDictionary<string, string> _bindings = new Dictionary<string, string>();

        private readonly Dictionary<string, Stack<Tuple<IBasicProperties, byte[]>>> _queue = new Dictionary<string, Stack<Tuple<IBasicProperties, byte[]>>>();

        /// <summary>
        /// The mock object.
        /// </summary>
        public IMessageQueue Object
        {
            get { return _mock != null ? _mock.Object : default(IMessageQueue); }
        }

        /// <summary>
        /// Fires when an internal call to start consuming from a queue has occurred.
        /// </summary>
        public event MessageQueueMockConsumeEventHandler Consume;

        /// <summary>
        /// Setup the mock definition and handlers.
        /// </summary>
        public void Setup()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            basicPropertiesMock.SetupAllProperties();

            var channelMock = new Mock<IModel>();

            channelMock.Setup(m => m.CreateBasicProperties()).Returns(basicPropertiesMock.Object);
            channelMock.Setup(m => m.QueueBind(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Callback<string, string, string>((q, e, r) =>
                {
                    if (_bindings.ContainsKey(q))
                        return;

                    _bindings.Add(q, e);
                });
            channelMock.Setup(m => m.QueueDeclare()).Returns(() =>
            {
                var temp = "temp_" + Guid.NewGuid().ToString("N");
                return new QueueDeclareOk(temp, 0, 0);
            });
            channelMock.Setup(m => m.QueueDeclare(It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>()))
                .Callback<string, bool, bool, bool, IDictionary<string, object>>(QueueDeclare);
            channelMock.Setup(m => m.ExchangeDeclare(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>()))
                .Callback<string, string, bool, bool, IDictionary<string, object>>(ExchangeDeclare);
            channelMock.Setup(m => m.BasicPublish(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<byte[]>()))
                .Callback<string, string, IBasicProperties, byte[]>(BasicPublish);
            channelMock.Setup(m => m.BasicConsume(It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicConsumer>()))
                .Callback<string, bool, IBasicConsumer>(BasicConsume);
            
            _mock = new Mock<IMessageQueue>();
            
            _mock.Setup(m => m.Connect()).Returns(channelMock.Object);
        }

        /// <summary>
        /// Simulate the receipt of a message sent through the mock object.
        /// </summary>
        /// <param name="key">The key to listen with.</param>
        /// <param name="guarantee">True if message delivery was expected to be guaranteed.</param>
        /// <returns>The message received.</returns>
        public string Receive(string key, bool guarantee = true)
        {
            _queueProps.Should().ContainKey(key, "Queue wasn't declared.");

            var queueProps = _queueProps[key];
            queueProps.Should().NotBeNull();
            queueProps.Durable.Should().Be(guarantee, "Queue declarations didn't match.");
            queueProps.Exclusive.Should().BeFalse("Queue declarations didn't match.");
            queueProps.AutoDelete.Should().BeFalse("Queue declarations didn't match.");

            var q = string.Format("exchange:|routingKey:{0}", key);

            if (_queue.ContainsKey(q))
            {
                var pair = _queue[q].Pop();

                pair.Should().NotBeNull();

                var props = pair.Item1;

                props.Persistent.Should().Be(guarantee, "The 'guarantee' argument must match to the original call.");

                return Encoding.UTF8.GetString(pair.Item2);
            }

            return null;
        }

        /// <summary>
        /// Simulate a message received on subscription published to through the mock object.
        /// </summary>
        /// <param name="key">The key to listen with.</param>
        /// <returns>The message received.</returns>
        public string Subscribe(string key)
        {
            _exchangeProps.Should().ContainKey(key, "Exchange wasn't declared.");

            var exchangeProps = _exchangeProps[key];
            exchangeProps.Should().NotBeNull();
            exchangeProps.Type.Should().Be(ExchangeType.Fanout, "Exchange declarations didn't match.");
            exchangeProps.Durable.Should().BeFalse("Exchange declarations didn't match.");
            exchangeProps.AutoDelete.Should().BeFalse("Exchange declarations didn't match.");

            var q = string.Format("exchange:{0}|routingKey:", key);

            if (_queue.ContainsKey(q))
            {
                var pair = _queue[q].Pop();

                pair.Should().NotBeNull();
                pair.Item1.Should().BeNull();

                return Encoding.UTF8.GetString(pair.Item2);
            }
            
            return null;
        }

        /// <summary>
        /// Simulate the receipt of a message request sent through the mock object.
        /// </summary>
        /// <param name="key">The key to listen with.</param>
        /// <returns>The message received.</returns>
        public string Respond(string key)
        {
            _queueProps.Should().ContainKey(key, "Queue wasn't declared.");

            var queueProps = _queueProps[key];
            queueProps.Should().NotBeNull();
            queueProps.Durable.Should().BeTrue("Queue declarations didn't match.");
            queueProps.Exclusive.Should().BeFalse("Queue declarations didn't match.");
            queueProps.AutoDelete.Should().BeFalse("Queue declarations didn't match.");

            var replyKey = "reply_" + key;

            _queueProps.Should().ContainKey(replyKey, "Reply queue wasn't declared.");
            var replyQueueProps = _queueProps[replyKey];
            replyQueueProps.Should().NotBeNull();
            replyQueueProps.Durable.Should().BeTrue("Reply queue declarations didn't match.");
            replyQueueProps.Exclusive.Should().BeFalse("Reply queue declarations didn't match.");
            replyQueueProps.AutoDelete.Should().BeFalse("Reply queue declarations didn't match.");

            var q = string.Format("exchange:|routingKey:{0}", key);

            if (_queue.ContainsKey(q))
            {
                var pair = _queue[q].Pop();

                pair.Should().NotBeNull();

                var props = pair.Item1;

                props.Persistent.Should().BeTrue("RPC messages should persist.");

                return Encoding.UTF8.GetString(pair.Item2);
            }

            return null;
        }

        /// <summary>
        /// Simulate a call to <see cref="MessageQueueSender.Send{T}"/> that may be listened for by the mock object.
        /// </summary>
        /// <param name="key">The key to listen with.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="guarantee">True if message delivery should be guaranteed.</param>
        public void Send(string key, string message, bool guarantee = true)
        {
            QueueDeclare(key, guarantee, false, false, null);

            var basicPropertiesMock = new Mock<IBasicProperties>();
            basicPropertiesMock.SetupAllProperties();

            var basicProperties = basicPropertiesMock.Object;
            basicProperties.Persistent = guarantee;

            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish("", key, basicProperties, body);
        }

        /// <summary>
        /// Simulate a call to <see cref="MessageQueueSender.Publish{T}"/> that may be listened for by the mock object.
        /// </summary>
        /// <param name="key">The key to listen with.</param>
        /// <param name="message">The message to send.</param>
        public void Publish(string key, string message)
        {
            ExchangeDeclare(key, ExchangeType.Fanout, false, false, null);

            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish(key, "", null, body);
        }

        /// <summary>
        /// Simulate a call to <see cref="MessageQueueSender.Request{T,TResult}"/> that may be listened for by the mock object.
        /// </summary>
        /// <param name="key">The key to listen with.</param>
        /// <param name="message">The message to send.</param>
        public void Request(string key, string message)
        {
            QueueDeclare(key, true, false, false, null);

            var basicPropertiesMock = new Mock<IBasicProperties>();
            basicPropertiesMock.SetupAllProperties();

            var basicProperties = basicPropertiesMock.Object;
            basicProperties.ReplyTo = "reply_" + key;
            basicProperties.CorrelationId = Guid.NewGuid().ToString("N");
            basicProperties.Persistent = true;

            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish("", key, basicProperties, body);
        }

        #region Private

        private void QueueDeclare(string q, bool d, bool e, bool ad, IDictionary<string, object> args)
        {
            args.Should().BeNull();

            var props = new Queue
            {
                Durable = d,
                Exclusive = e,
                AutoDelete = ad
            };

            if (_queueProps.ContainsKey(q))
            {
                _queueProps[q].ShouldBeEquivalentTo(props, "Queue already declared with different properties.");
            }
            else
            {
                _queueProps.Add(q, props);
            }
        }

        private void ExchangeDeclare(string e, string t, bool d, bool ad, IDictionary<string, object> args)
        {
            args.Should().BeNull();

            var props = new Exchange
            {
                Type = t,
                Durable = d,
                AutoDelete = ad
            };

            if (_exchangeProps.ContainsKey(e))
            {
                _exchangeProps[e].ShouldBeEquivalentTo(props, "Exchange already declared with different properties.");
            }
            else
            {
                _exchangeProps.Add(e, props);
            }
        }

        private void BasicPublish(string e, string r, IBasicProperties p, byte[] b)
        {
            var q = string.Format("exchange:{0}|routingKey:{1}", e, r);
            var pair = Tuple.Create(p, b);
            if (!_queue.ContainsKey(q))
            {
                _queue.Add(q, new Stack<Tuple<IBasicProperties, byte[]>>());
            }
            _queue[q].Push(pair);
        }

        /// <summary>
        /// Mock callback for <see cref="IModel.BasicConsume(string,bool,IBasicConsumer)"/> that throws a <see cref="Consume"/>
        /// event as a means for this object to simulate receiving a message.
        /// </summary>
        /// <param name="q">The queue name.</param>
        /// <param name="noAck">True if no acknowledgement of receipt is expected.</param>
        /// <param name="consumer">The consumer that would normally retrieve the message.</param>
        private void BasicConsume(string q, bool noAck, IBasicConsumer consumer)
        {
            var result = _bindings.ContainsKey(q) ? Subscribe(_bindings[q]) : Receive(q);
            if (Consume != null)
            {
                Consume.Invoke(this, new MessageQueueMockConsumeEventArgs(result));
            }
        }

        /// <summary>
        /// Properties of a queue.
        /// </summary>
        private struct Queue
        {
            internal bool Durable;
            internal bool Exclusive;
            internal bool AutoDelete;
        }

        /// <summary>
        /// Properties of an exchange.
        /// </summary>
        private struct Exchange
        {
            internal string Type;
            internal bool Durable;
            internal bool AutoDelete;
        }

        #endregion
    }
}
