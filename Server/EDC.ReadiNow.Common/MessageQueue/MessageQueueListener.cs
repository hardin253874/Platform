// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics;
using EDC.Remote;
using Jil;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using EDC.ReadiNow.Core;
using Autofac;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Provides a means to listen for incoming messages directed via a RabbitMQ installation.
    /// </summary>
    public class MessageQueueListener : IRemoteListener
    {
        private volatile bool _stopping;
        private volatile Thread _worker;
        private readonly object _sync = new object();

        private IMessageQueue MessageQueue { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public MessageQueueListener()
        {
            MessageQueue = Factory.Current.Resolve<IMessageQueue>();
        }

        /// <summary>
        /// Listens for messages that have been sent remotely via <see cref="MessageQueueSender.Send{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the message expected.</typeparam>
        /// <param name="key">The key that the message was sent with.</param>
        /// <param name="handler">A handler function that can act on any messages received while listening.</param>
        /// <param name="guarantee">Must match the argument that the message was sent with.</param>
        public void Receive<T>(string key, Action<T> handler, bool guarantee = true)
        {
            if (_worker == null)
            {
                lock (_sync)
                {
                    if (_worker == null)
                    {
                        EventLog.Application.WriteInformation("Starting Message Queue Receiver ({0})...", key);

                        _worker = new Thread(() =>
                        {
                            try
                            {
                                using (var channel = MessageQueue.Connect())
                                {
                                    QueueingBasicConsumer consumer;

                                    try
                                    {
                                        channel.QueueDeclare(queue: key,
                                            durable: guarantee,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                                        // fair dispatch
                                        channel.BasicQos(prefetchSize: 0,
                                            prefetchCount: 1,
                                            global: false);

                                        // ordering
                                        consumer = new QueueingBasicConsumer(channel);

                                        channel.BasicConsume(queue: key,
                                            noAck: !guarantee,
                                            consumer: consumer);
                                    }
                                    catch (Exception e)
                                    {
                                        EventLog.Application.WriteError("Message Queue ({0}): Failed to establish connection. {1}", key, e.ToString());
                                        return;
                                    }

                                    while (!_stopping)
                                    {
                                        BasicDeliverEventArgs ea;

                                        // This call will block until there is a message. The timeout will mean a max wait before being able to stop.
                                        if (!consumer.Queue.Dequeue(1000, out ea))
                                        {
                                            Thread.Sleep(10000);
                                            continue;
                                        }

                                        EventLog.Application.WriteTrace("Message Queue ({0}): Message Received.", key);

                                        try
                                        {
                                            var response = Encoding.UTF8.GetString(ea.Body);

                                            var body = Deserialize<T>(response);

                                            handler(body);
                                        }
                                        catch (Exception e)
                                        {
                                            EventLog.Application.WriteError("Message Queue ({0}): Failed to process message. {1}", key, e.ToString());
                                        }
                                        finally
                                        {
                                            if (guarantee)
                                            {
                                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                            }
                                        }
                                    }

                                    EventLog.Application.WriteInformation("Message Queue Listener Stopped ({0}).", key);
                                }
                            }
                            catch (BrokerUnreachableException bue)
                            {
                                EventLog.Application.WriteError("Message Queue ({0}): Could not reach host. {1}", key, bue.ToString());
                            }
                            catch (Exception err)
                            {
                                EventLog.Application.WriteError("Message Queue ({0}): Unexpected error. Exiting. {1}", key, err.ToString());
                            }
                        })
                        {
                            IsBackground = true,
                            Name = key
                        };

                        _worker.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes to messages that have been broadcast via <see cref="MessageQueueSender.Publish{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the message expected.</typeparam>
        /// <param name="key">The key that the message was published with.</param>
        /// <param name="handler">A handler function that can act on any messages received while listening.</param>
        public void Subscribe<T>(string key, Action<T> handler)
        {
            if (_worker == null)
            {
                lock (_sync)
                {
                    if (_worker == null)
                    {
                        EventLog.Application.WriteInformation("Starting Message Queue Subscriber ({0})...", key);

                        _worker = new Thread(() =>
                        {
                            try
                            {
                                using (var channel = MessageQueue.Connect())
                                {
                                    try
                                    {
                                        channel.ExchangeDeclare(exchange: key,
                                            type: ExchangeType.Fanout);

                                        // temporary queues
                                        var queueName = channel.QueueDeclare().QueueName;
                                        channel.QueueBind(queue: queueName,
                                            exchange: key,
                                            routingKey: "");

                                        var consumer = new EventingBasicConsumer(channel);
                                        consumer.Received += (o, ea) =>
                                        {
                                            EventLog.Application.WriteTrace("Message Queue ({0}): Message Received.", key);

                                            try
                                            {
                                                var response = Encoding.UTF8.GetString(ea.Body);

                                                var body = Deserialize<T>(response);

                                                handler(body);
                                            }
                                            catch (Exception e)
                                            {
                                                EventLog.Application.WriteError("Message Queue ({0}): Failed to process message. {1}", key, e.ToString());
                                            }
                                        };

                                        channel.BasicConsume(queue: queueName,
                                            noAck: true,
                                            consumer: consumer);
                                    }
                                    catch (Exception e)
                                    {
                                        EventLog.Application.WriteError("Message Queue ({0}): Failed to establish connection. {1}", key, e.ToString());
                                        return;
                                    }

                                    while (!_stopping)
                                    {
                                        // run
                                        Thread.Sleep(10000);
                                    }

                                    EventLog.Application.WriteInformation("Message Queue Subscriber Stopped ({0}).", key);
                                }
                            }
                            catch (BrokerUnreachableException bue)
                            {
                                EventLog.Application.WriteError("Message Queue ({0}): Could not reach host. {1}", key, bue.ToString());
                            }
                            catch (Exception err)
                            {
                                EventLog.Application.WriteError("Message Queue ({0}): Unexpected error. Exiting. {1}", key,
                                    err.ToString());
                            }
                        })
                        {
                            IsBackground = true,
                            Name = key
                        };

                        _worker.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Waits for messages sent with <see cref="MessageQueueSender.Request{T,TResult}"/> and provides a response.
        /// </summary>
        /// <typeparam name="T">The type of the message expected.</typeparam>
        /// <typeparam name="TResult">The type of the response message to provide.</typeparam>
        /// <param name="key">The key that the message was sent with.</param>
        /// <param name="handler">A handler function that can responsd to any messages received while listening.</param>
        public void Respond<T, TResult>(string key, Func<T, TResult> handler)
        {
            if (_worker == null)
            {
                lock (_sync)
                {
                    if (_worker == null)
                    {
                        EventLog.Application.WriteInformation("Starting Message Queue Responder ({0})...", key);

                        _worker = new Thread(() =>
                        {
                            try
                            {
                                using (var channel = MessageQueue.Connect())
                                {
                                    QueueingBasicConsumer consumer;

                                    try
                                    {
                                        channel.QueueDeclare(queue: key,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                                        channel.BasicQos(prefetchSize: 0,
                                            prefetchCount: 1,
                                            global: false);

                                        consumer = new QueueingBasicConsumer(channel);

                                        channel.BasicConsume(queue: key,
                                            noAck: false,
                                            consumer: consumer);
                                    }
                                    catch (Exception e)
                                    {
                                        EventLog.Application.WriteError("Message Queue ({0}): Failed to establish connection. {1}", key, e.ToString());
                                        return;
                                    }

                                    while (!_stopping)
                                    {
                                        var result = default(TResult);

                                        BasicDeliverEventArgs ea;

                                        if (!consumer.Queue.Dequeue(1000, out ea))
                                        {
                                            Thread.Sleep(10000);
                                            continue;
                                        }

                                        EventLog.Application.WriteTrace("Message Queue ({0}): Message Received.", key);

                                        try
                                        {
                                            var response = Encoding.UTF8.GetString(ea.Body);

                                            var body = Deserialize<T>(response);
                                            
                                            result = handler(body);
                                        }
                                        catch (Exception e)
                                        {
                                            EventLog.Application.WriteError("Message Queue ({0}): Failed to process message. {1}", key, e.ToString());
                                        }
                                        finally
                                        {
                                            try
                                            {
                                                var replyKey = ea.BasicProperties.ReplyTo;
                                                var correlationId = ea.BasicProperties.CorrelationId;

                                                EventLog.Application.WriteTrace("Responding ({0}) [{1}]...", key, correlationId);

                                                var replyProps = channel.CreateBasicProperties();
                                                replyProps.CorrelationId = correlationId;
                                                replyProps.Persistent = true;

                                                // serialize the response
                                                var raw = Serialize(result);
                                                
                                                var body = Encoding.UTF8.GetBytes(raw);

                                                channel.QueueDeclare(queue: replyKey,
                                                    durable: true,
                                                    exclusive: false,
                                                    autoDelete: false,
                                                    arguments: null);

                                                channel.BasicPublish(exchange: "",
                                                    routingKey: replyKey,
                                                    basicProperties: replyProps,
                                                    body: body);
                                            }
                                            catch (Exception ex)
                                            {
                                                EventLog.Application.WriteError("Message Queue ({0}): Failed to prepare the response. {1}", key, ex.ToString());
                                                throw;
                                            }
                                            finally
                                            {
                                                // doubly ensure we acknowledge the original call
                                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                            }
                                        }
                                    }

                                    EventLog.Application.WriteInformation("Message Queue Responder Stopped ({0}).", key);
                                }
                            }
                            catch (BrokerUnreachableException bue)
                            {
                                EventLog.Application.WriteError("Message Queue ({0}): Could not reach host. {1}", key, bue.ToString());
                            }
                            catch (Exception err)
                            {
                                EventLog.Application.WriteError("Message Queue ({0}): Unexpected error. Exiting. {1}", key,
                                    err.ToString());
                            }
                        })
                        {
                            IsBackground = true,
                            Name = key
                        };

                        _worker.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Stops any listening threads that may be running as a result of the calls above.
        /// </summary>
        public void Stop()
        {
            _stopping = true;
            if (_worker != null)
            {
                lock (_sync)
                {
                    if (_worker != null)
                    {
                        _worker.Join();
                    }
                }
            }
        }

        #region Private Static Methods

        private static string Serialize<T>(T value)
        {
            var options = new Options(includeInherited: true);
            var result = JSON.Serialize(value, options);

            var valueTypeAware = value as IMessageQueueTypeAware;
            if (valueTypeAware != null && !string.IsNullOrEmpty(valueTypeAware.Type))
            {
                var type = Type.GetType(valueTypeAware.Type);
                if (type != null)
                {
                    result = JSON.Serialize(Convert.ChangeType(value, type), options);
                }
            }

            return result;
        }

        private static T Deserialize<T>(string value)
        {
            var options = new Options(includeInherited: true);
            var result = JSON.Deserialize<T>(value, options);

            var resultTypeAware = result as IMessageQueueTypeAware;
            if (resultTypeAware != null && !string.IsNullOrEmpty(resultTypeAware.Type))
            {
                var type = Type.GetType(resultTypeAware.Type);
                if (type != null)
                {
                    result = (T)JSON.Deserialize(value, type, options);
                }
            }

            return result;
        }

        #endregion
    }
}
