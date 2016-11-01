// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Allows for creation of and connection to message queues managed by a configured RabbitMQ installation on the network.
    /// Used internally by <see cref="MessageQueueSender"/> and <see cref="MessageQueueListener"/>.
    /// </summary>
    public class MessageQueue : IMessageQueue
    {
        private static volatile IConnection _connection;
        private static readonly object Sync = new object();

        /// <summary>
        /// Creates and connects to a channel allowing messages to be sent and received remotely via the queue.
        /// </summary>
        /// <returns>A RabbitMQ channel object that allows for sending or receiving messages.</returns>
        public IModel Connect()
        {
            // Connections are per app. Channels are per thread. Generally. (IModel may be thread safe in Java but not .Net, i believe)
            return CreateChannel();
        }

        #region Private Static Methods

        private static IModel CreateChannel()
        {
            IModel model;

            try
            {
                model = Connection.CreateModel();
            }
            catch (AlreadyClosedException)
            {
                _connection = null; // try to prevent a stale connection on next request

                throw;
            }

            return model;
        }
        
        private static IConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    lock (Sync)
                    {
                        if (_connection == null)
                        {
                            var factory = GetConnectionFactory();
                            _connection = factory.CreateConnection();
                        }
                    }
                }

                return _connection;
            }
        }

        private static ConnectionFactory GetConnectionFactory()
        {
            var rabbitConfiguration = ConfigurationSettings.GetRabbitMqConfigurationSection();
            if (rabbitConfiguration == null)
            {
                throw new Exception("Cannot connect. Message Queue configuration not present.");
            }

            if (string.IsNullOrEmpty(rabbitConfiguration.RabbitMq.HostName))
            {
                throw new Exception("Cannot connect. Message Queue host name has not been specified.");
            }

            var settings = rabbitConfiguration.RabbitMq;

            //
            // TODO: SSL support
            //
            return new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = string.IsNullOrEmpty(settings.User) ? ConnectionFactory.DefaultUser : settings.User,
                Password = string.IsNullOrEmpty(settings.Password) ? ConnectionFactory.DefaultPass : settings.Password,
                Port = settings.Port > 0 ? settings.Port : AmqpTcpEndpoint.UseDefaultPort,
                VirtualHost = string.IsNullOrEmpty(settings.VirtualHost) ? ConnectionFactory.DefaultVHost : settings.VirtualHost
                //Protocol = Protocols.DefaultProtocol,
                //Ssl = new SslOption
                //{
                //    Enabled = true,
                //    ServerName = hostName,
                //    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors
                //}
            };
        }

        #endregion
    }
}
