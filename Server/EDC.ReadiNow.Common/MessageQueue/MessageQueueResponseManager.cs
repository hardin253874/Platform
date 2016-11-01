// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Autofac;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Remote;
using Jil;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Intended to run as a single global instance to manage the processing of responses received after initiating a remote
    /// call with <see cref="MessageQueueSender.Request{T,TResult}"/>. Information about the request is persisted to the database as
    /// a <see cref="MessageQueueRequest"/> entity and subsequently watched in conjunction with RabbitMQ allowing for durable RPC handling.
    /// </summary>
    public sealed class MessageQueueResponseManager
    {
        private static volatile MessageQueueResponseManager _instance;
        private static readonly object Sync = new object();

        private readonly ConcurrentDictionary<string, MessageQueueRequest> _requests;
        private readonly ConcurrentDictionary<string, EventingBasicConsumer> _consumers;
        private volatile IModel _channel;
        private volatile bool _stopping;
        private volatile bool _listening;
        private volatile Thread _worker;
        private readonly object _sync = new object();

        private IMessageQueue MessageQueue { get; set; }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private MessageQueueResponseManager()
        {
            _requests = new ConcurrentDictionary<string, MessageQueueRequest>();
            _consumers = new ConcurrentDictionary<string, EventingBasicConsumer>();

            MessageQueue = Factory.Current.Resolve<IMessageQueue>();
        }

        /// <summary>
        /// Indicates to the static manager that the worker thread has initialized and has got to the point where
        /// it is ready to receive messages.
        /// </summary>
        public bool IsListening
        {
            get { return _listening; }
        }

        /// <summary>
        /// Starts the global message queue response manager listening for the responses to any registered requests.
        /// </summary>
        public static void Start()
        {
            try
            {
                if (_instance != null)
                    return;

                var config = ConfigurationSettings.GetRabbitMqConfigurationSection();
                if (config == null)
                    return;

                var settings = config.RabbitMq;
                if (settings == null)
                    return;
                
                if (string.IsNullOrEmpty(settings.HostName))
                    return;

                lock (Sync)
                {
                    if (_instance == null)
                    {
                        _instance = new MessageQueueResponseManager();
                        _instance.StartInternal();
                    }
                }

                var maxWait = TimeSpan.FromMinutes(1);
                var sw = Stopwatch.StartNew();
                while (!_instance.IsListening && sw.Elapsed < maxWait)
                {
                    // wait... but only so long
                    Thread.Sleep(100);
                }

                if (!_instance.IsListening)
                {
                    EventLog.Application.WriteError("Message Queue Response Manager: Thread did not start in time. Exiting.");
                }

                using (new AdministratorContext())
                {
                    var requests = Entity.GetInstancesOfType<MessageQueueRequest>();
                    foreach (var request in requests)
                    {
                        //
                        // TODO: ? Maybe if the persisted entity is too old, it should just be deleted?
                        //

                        Add(request);
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure starting the message queue response manager. {0}", e);
            }
        }

        /// <summary>
        /// Adds a message queue request to the response manager's watch list.
        /// </summary>
        /// <param name="request">The message queue request.</param>
        public static void Add(MessageQueueRequest request)
        {
            if (request == null)
                return;

            try
            {
                // unless Start has been called, ignore. Any saved requests should be picked up on Start anyway.
                if (_instance != null && _instance.IsListening)
                {
                    _instance.AddInternal(request);
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure adding a request to the message queue response manager. {0}", e);
            }
        }

        /// <summary>
        /// Removes a message queue request from the response manager's watch list.
        /// </summary>
        /// <param name="request"></param>
        public static void Remove(MessageQueueRequest request)
        {
            if (request == null)
                return;

            try
            {
                if (_instance != null && _instance.IsListening)
                {
                    _instance.RemoveInternal(request);
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure removing a request from the message queue response manager. {0}", e);
            }
        }

        /// <summary>
        /// Requests a stop to any processing by the message queue response manager.
        /// </summary>
        public static void Stop()
        {
            try
            {
                if (_instance != null)
                {
                    _instance.StopInternal();
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Unexpected failure stopping the message queue response manager. {0}", e);
            }
        }

        #region Private Methods

        /// <summary>
        /// Starts the manager listening for responses to all existing <see cref="MessageQueueRequest"/> objects in the system.
        /// </summary>
        private void StartInternal()
        {
            if (_worker == null)
            {
                lock (_sync)
                {
                    if (_worker == null)
                    {
                        EventLog.Application.WriteTrace("Starting Message Queue Response Manager...");

                        _worker = new Thread(() =>
                        {
                            try
                            {
                                _channel = MessageQueue.Connect();
                                _channel.BasicQos(prefetchSize: 0,
                                    prefetchCount: 1,
                                    global: false);

                                while (!_stopping)
                                {
                                    // keep alive
                                    _listening = true;

                                    Thread.Sleep(10000);
                                }

                                _listening = false;

                                EventLog.Application.WriteTrace("Message Queue Response Manager Stopped.");
                            }
                            catch (BrokerUnreachableException bue)
                            {
                                EventLog.Application.WriteError(
                                    "Message Queue Response Manager: Could not reach host. {0}", bue.ToString());
                            }
                            catch (Exception err)
                            {
                                EventLog.Application.WriteError(
                                    "Message Queue Response Manager: Unexpected error. Exiting. {0}", err.ToString());
                            }
                        })
                        {
                            IsBackground = true,
                            Name = "Message Queue Response Manager"
                        };

                        _worker.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Adds a message queue request to the active list used in listening for and processing responses.
        /// </summary>
        /// <param name="request">The message queue request to add.</param>
        private void AddInternal(MessageQueueRequest request)
        {
            // if this request is not yet known to us then listen out for the reply
            if (_requests.TryAdd(request.MessageQueueRequestToken, request))
            {
                var key = request.MessageQueueRequestKey;

                // if this queue is not yet known to us then declare it, and begin consuming
                EventingBasicConsumer consumer;
                if (!_consumers.TryGetValue(key, out consumer))
                {
                    try
                    {
                        consumer = new EventingBasicConsumer(_channel);
                        if (!_consumers.TryAdd(key, consumer))
                        {
                            // has someone added in the mean time?
                            if (!_consumers.TryGetValue(key, out consumer))
                            {
                                throw new Exception("Failed to create or retrieve a consumer.");
                            }
                        }

                        _channel.QueueDeclare(queue: key,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        consumer.Received += ConsumerOnReceived;

                        _channel.BasicConsume(queue: key,
                            noAck: false,
                            consumer: consumer);
                    }
                    catch (Exception e)
                    {
                        EventLog.Application.WriteError("Message Queue Response Manager: Failed to establish connection. {0}", e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Removes a message queue request from being involved in the processing of responses.
        /// </summary>
        /// <param name="request">The message queue request to remove.</param>
        private void RemoveInternal(MessageQueueRequest request)
        {
            MessageQueueRequest remove;
            _requests.TryRemove(request.MessageQueueRequestToken, out remove);
        }

        /// <summary>
        /// Handles a message response received for any of the consumers started by the message queue response manager. The
        /// response is processed and passed to an instance of the <see cref="IRemoteResponseHandler{T,TResult}"/> type that was specified
        /// on the original request, after which the matching <see cref="MessageQueueRequest"/> is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="ea">The event args.</param>
        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                EventLog.Application.WriteTrace("Message Queue Response Manager: Message Received.");

                var response = Encoding.UTF8.GetString(ea.Body);
                if (string.IsNullOrEmpty(response))
                    throw new Exception("Message queue response handler received an empty response.");

                var correlationId = ea.BasicProperties.CorrelationId;
                if (string.IsNullOrEmpty(correlationId))
                    throw new Exception("Message queue response handler was called with an invalid token.");

                MessageQueueRequest request;
                if (_requests.TryRemove(correlationId, out request))
                {
                    EventLog.Application.WriteTrace("Message Queue Response Manager: Finalizing [{0}]...", correlationId);

                    // process the response
                    try
                    {
                        string raw;
                        object handler;
                        Type handlerType, bodyType, resultType;

                        using (new AdministratorContext())
                        {
                            handlerType = Type.GetType(request.MessageQueueRequestHandlerType);
                            if (handlerType == null)
                                throw new Exception("Message queue response handler type was unknown.");

                            if (!handlerType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRemoteResponseHandler<,>)))
                                throw new Exception("Message queue response handler does not appear to conform to the expected interface.");

                            bodyType = Type.GetType(request.MessageQueueRequestBodyType);
                            if (bodyType == null)
                                throw new Exception("Message queue original request body type was unknown.");

                            raw = request.MessageQueueRequestBody;

                            resultType = Type.GetType(request.MessageQueueRequestResultType);
                            if (resultType == null)
                                throw new Exception("Message queue result type was unknown.");

                            handler = Activator.CreateInstance(handlerType);
                            if (handler == null)
                                throw new Exception("Message queue response handler could not be instantiated.");
                        }

                        var body = JSON.Deserialize(raw, bodyType, new Options(includeInherited: true));
                        if (body == null)
                            throw new Exception("Message queue original request did not deserialize correctly.");

                        var result = JSON.Deserialize(response, resultType, new Options(includeInherited: true));
                        if (result == null)
                            throw new Exception("Message queue result did not deserialize correctly.");

                        // Reflection. But it makes it nicer for the api.
                        var process = handlerType.GetMethod("Process");

                        process.Invoke(handler, new[] { body, result });
                    }
                    finally
                    {
                        using (new AdministratorContext())
                        {
                            request.AsWritable().Delete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.Application.WriteError("Message Queue Response Manager: Failed to process message. {0}", e.ToString());
            }
            finally
            {
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }

        /// <summary>
        /// Stops the worker thread from listening and waits for it to finish anything in progress.
        /// </summary>
        private void StopInternal()
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

        #endregion
    }
}
