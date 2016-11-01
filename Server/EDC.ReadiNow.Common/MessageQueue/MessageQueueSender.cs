// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Text;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Remote;
using Jil;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Provides a means to send messages remotely via a RabbitMQ installation.
    /// </summary>
    public class MessageQueueSender : IRemoteSender
    {
        private IMessageQueue MessageQueue { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public MessageQueueSender()
        {
            MessageQueue = Factory.Current.Resolve<IMessageQueue>();
        }

        /// <summary>
        /// Sends a message remotely that may optionally be durable. The initiating half of <see cref="MessageQueueListener.Receive{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize and send.</typeparam>
        /// <param name="key">The key used to retrieve the message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="guarantee">If true, the message will be durable and will wait for acknowledgement of receipt.</param>
        public void Send<T>(string key, T message, bool guarantee = true)
        {
            try
            {
                EventLog.Application.WriteTrace("Sending message ({0})...", key);

                using (var channel = MessageQueue.Connect())
                {
                    try
                    {
                        var raw = Serialize(message);

                        var body = Encoding.UTF8.GetBytes(raw);

                        channel.QueueDeclare(queue: key,
                            durable: guarantee,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = guarantee;

                        channel.BasicPublish(exchange: "",
                            routingKey: key,
                            basicProperties: properties,
                            body: body);
                    }
                    catch (Exception e)
                    {
                        EventLog.Application.WriteError("Message Queue ({0}): Failed to send message. {1}", key, e.ToString());
                        throw;
                    }
                }
            }
            catch (BrokerUnreachableException bue)
            {
                EventLog.Application.WriteError("Message Queue ({0}): Could not reach host. {1}", key, bue.ToString());
                throw;
            }
            catch (Exception err)
            {
                EventLog.Application.WriteError("Message Queue ({0}): Unexpected error. {1}", key, err.ToString());
                throw;
            }
        }

        /// <summary>
        /// Broadcasts a message remotely to none or more listeners. No guarantees about delivery and if not listening others will
        /// miss the message. The push side of the pub/sub pairing with <see cref="MessageQueueListener.Subscribe{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize and publish.</typeparam>
        /// <param name="key">The key used to retrieve the message.</param>
        /// <param name="message">The message to send.</param>
        public void Publish<T>(string key, T message)
        {
            try
            {
                EventLog.Application.WriteTrace("Publishing message ({0})...", key);

                using (var channel = MessageQueue.Connect())
                {
                    try
                    {
                        var raw = Serialize(message);

                        var body = Encoding.UTF8.GetBytes(raw);

                        channel.ExchangeDeclare(exchange: key,
                            type: ExchangeType.Fanout,
                            durable: false,
                            autoDelete: false,
                            arguments: null); // broadcast to all

                        channel.BasicPublish(exchange: key,
                            routingKey: "",
                            basicProperties: null,
                            body: body);
                    }
                    catch (Exception e)
                    {
                        EventLog.Application.WriteError("Message Queue ({0}): Failed to broadcast message. {1}", key, e.ToString());
                        throw;
                    }
                }
            }
            catch (BrokerUnreachableException bue)
            {
                EventLog.Application.WriteError("Message Queue ({0}): Could not reach host. {1}", key, bue.ToString());
                throw;
            }
            catch (Exception err)
            {
                EventLog.Application.WriteError("Message Queue ({0}): Unexpected error. {1}", key, err.ToString());
                throw;
            }
        }

        /// <summary>
        /// Sends a message remotely while anticipating a response in return. Effectively an RPC coupling used 
        /// together with <see cref="MessageQueueListener.Respond{T,TResult}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize and send.</typeparam>
        /// <typeparam name="TResult">The type of message expected in response.</typeparam>
        /// <param name="key">The key used to retrieve the message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="handler">A handler object that may be instantiated to process the return message.</param>
        public void Request<T, TResult>(string key, T message, IRemoteResponseHandler<T, TResult> handler)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N");
                var replyKey = "reply_" + key;

                EventLog.Application.WriteTrace("Requesting response ({0}) [{1}]...", key, correlationId);
               
                using (var channel = MessageQueue.Connect())
                {
                    try
                    {
                        var raw = Serialize(message);

                        var body = Encoding.UTF8.GetBytes(raw);

                        // declare the request queue
                        channel.QueueDeclare(queue: key,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        // declare the reply queue
                        channel.QueueDeclare(queue: replyKey,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        var properties = channel.CreateBasicProperties();
                        properties.ReplyTo = replyKey;
                        properties.CorrelationId = correlationId;
                        properties.Persistent = true;

                        // have the system first hold a record of the impending request
                        using (new AdministratorContext())
                        {
                            var requestEntity = Entity.Create<MessageQueueRequest>();
                            requestEntity.Name = "mq_req_" + correlationId;
                            requestEntity.MessageQueueRequestKey = replyKey;
                            requestEntity.MessageQueueRequestToken = correlationId;
                            requestEntity.MessageQueueRequestHandlerType = handler.GetType().AssemblyQualifiedName;
                            requestEntity.MessageQueueRequestBody = raw;
                            requestEntity.MessageQueueRequestBodyType = typeof(T).AssemblyQualifiedName;
                            requestEntity.MessageQueueRequestResultType = typeof(TResult).AssemblyQualifiedName;
                            requestEntity.Save();
                        }

                        channel.BasicPublish(exchange: "",
                            routingKey: key,
                            basicProperties: properties,
                            body: body);
                    }
                    catch (Exception e)
                    {
                        EventLog.Application.WriteError("Message Queue ({0}): Failed to send message. {1}", key, e.ToString());
                        throw;
                    }
                }
            }
            catch (BrokerUnreachableException bue)
            {
                EventLog.Application.WriteError("Message Queue ({0}): Could not reach host. {1}", key, bue.ToString());
                throw;
            }
            catch (Exception err)
            {
                EventLog.Application.WriteError("Message Queue ({0}): Unexpected error. {1}", key, err.ToString());
                throw;
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

        #endregion
    }
}
