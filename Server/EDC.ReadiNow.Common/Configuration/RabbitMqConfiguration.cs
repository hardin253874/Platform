// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;
using System.Diagnostics;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Defines the RabbitMQ configuration section
    /// </summary>
    [DebuggerStepThrough]
    public class RabbitMqConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the RabbitMq settings configuration element.
        /// </summary>
        [ConfigurationProperty("rabbitMq")]
        public RabbitMqSettings RabbitMq
        {
            get { return (RabbitMqSettings)this["rabbitMq"]; }
            set { this["rabbitMq"] = value; }
        }
    }
}
