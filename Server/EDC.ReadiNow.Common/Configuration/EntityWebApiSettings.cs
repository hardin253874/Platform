// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    ///     Represents the entity element within the primary configuration file.
    /// </summary>
    public class EntityWebApiSettings : ConfigurationElement
    {
        /// <summary>
        /// Gets the current configuration settings.
        /// </summary>
        public static EntityWebApiSettings Current
        {
            get
            {
                return ConfigurationSettings.GetServerConfigurationSection().EntityWebApi;
            }            
        }

        /// <summary>
        ///     Gets or sets the number of related entities that can be requested via the entity WebApi
        ///     while following any one relationship from any one entity.
        ///     If more than this many are found, then none are returned.
        /// </summary>
        [ConfigurationProperty("maxRelatedLimit", DefaultValue = 500, IsRequired = false)]
        public int MaxRelatedLimit
        {
            get
            {
                return (int)this["maxRelatedLimit"];
            }

            set
            {
                this["maxRelatedLimit"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets the warning threshold for the number of related entities that can be requested via the entity WebApi
        ///     while following any one relationship from any one entity.
        ///     If more than this many are found, then a warning is written to the log.
        /// </summary>
        [ConfigurationProperty("maxRelatedWarning", DefaultValue = 200, IsRequired = false)]
        public int MaxRelatedWarning
        {
            get
            {
                return (int)this["maxRelatedWarning"];
            }

            set
            {
                this["maxRelatedWarning"] = value;
            }
        }

        /// <summary>
        ///     The maximum amount of SQL CPU time a report is allows to use while running
        /// </summary>
        [ConfigurationProperty("reportCpuLimitSeconds", DefaultValue = 0, IsRequired = false)]
        public int ReportCpuLimitSeconds
        {
            get
            {
                return (int)this["reportCpuLimitSeconds"];
            }

            set
            {
                this["reportCpuLimitSeconds"] = value;
            }
        }
    }
}