// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Holds configuration settings relevant for communicating with a RabbitMQ installation.
    /// </summary>
    public class RabbitMqSettings : ConfigurationElement
    {
        /// <summary>
        /// The host name of the machine where RabbitMQ is installed.
        /// </summary>
        [ConfigurationProperty("hostName", DefaultValue = "", IsRequired = false)]
        public string HostName
        {
            get { return (string)this["hostName"]; }
            set { this["hostName"] = value; }
        }

        /// <summary>
        /// The port that the RabbitMQ installation is listening on.
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = -1, IsRequired = false)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }

        /// <summary>
        /// The user name of the RabbitMQ account to use when connecting.
        /// </summary>
        [ConfigurationProperty("user", DefaultValue = "", IsRequired = false)]
        public string User
        {
            get { return (string)this["user"]; }
            set { this["user"] = value; }
        }

        /// <summary>
        /// The password of the RabbitMQ account to use when connecting.
        /// </summary>
        [ConfigurationProperty("password", DefaultValue = "", IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        /// <summary>
        /// The virtual path to use when connecting to the RabbitMQ installation.
        /// </summary>
        [ConfigurationProperty("vhost", DefaultValue = "", IsRequired = false)]
        public string VirtualHost
        {
            get { return (string)this["vhost"]; }
            set { this["vhost"] = value; }
        }
    }
}
