// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Entity model audit log configuration element.
    /// </summary>
    public class AuditLogEntityModelSettings : ConfigurationElement
    {
        /// <summary>
        ///     Gets or sets whether audit log messages are logged to the entity model.
        /// </summary>
        [ConfigurationProperty("isEnabled", DefaultValue = true, IsRequired = false)]
        public bool IsEnabled
        {
            get
            {
                return (bool)this["isEnabled"];
            }

            set
            {
                this["isEnabled"] = value;
            }
        }
    }
}
