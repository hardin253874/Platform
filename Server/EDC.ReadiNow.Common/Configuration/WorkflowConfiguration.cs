// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Defines the workflow configuration section
	/// </summary>
	public class WorkflowConfiguration : ConfigurationSection
	{
		[ConfigurationProperty( "triggers" )]
        public TriggerSettings Triggers
		{
			get
			{
                return (TriggerSettings)this["triggers"];
			}

			set
			{
				this[ "triggers" ] = value;
			}
		}

        [ConfigurationProperty("backgroundTasks")]
        public BackgroundTaskSettings BackgroundTasks
        {
            get
            {
                return (BackgroundTaskSettings)this["backgroundTasks"];
            }

            set
            {
                this["backgroundTasks"] = value;
            }
        }

    }

    /// <summary>
    ///     Trigger settigns
    /// </summary>
    public class TriggerSettings : ConfigurationElement
    {
        /// <summary>
        /// The maximum depth of triggers that is supportedwithin a workflow. If this depth is exceeded the triggered workflow will fail.
        /// </summary>
        [ConfigurationProperty("maxDepth", DefaultValue = 10, IsRequired = false)]
        public int MaxDepth
        {
            get
            {
                return (int)this["maxDepth"];
            }

            set
            {
                this["maxDepth"] = value;
            }
        }


        /// <summary>
        /// The number of days to keep a compelted workflow run before it is automatically cleaned up.
        /// </summary>
        [ConfigurationProperty("keepCompletedRunDays", DefaultValue = 5, IsRequired = false)]     
        public int KeepCompletedRunDays
        {
            get
            {
                return (int)this["keepCompletedRunDays"];
            }

            set
            {
                this["keepCompletedRunDays"] = value;
            }
        }


        /// <summary>
        /// The maximum length of time a workflow can run in seconds before it is killed.
        /// </summary>
        [ConfigurationProperty("maxRunTimeSeconds", DefaultValue = 600, IsRequired = false)]
        public int MaxRunTimeSeconds
        {
            get
            {
                return (int)this["maxRunTimeSeconds"];
            }

            set
            {
                this["maxRunTimeSeconds"] = value;
            }
        }

        /// <summary>
        /// The maximum number of steps that a workflow can run before it is killed.
        /// </summary>
        [ConfigurationProperty("maxSteps", DefaultValue = 100000, IsRequired = false)]
        public int MaxSteps
        {
            get
            {
                return (int)this["maxSteps"];
            }

            set
            {
                this["maxSteps"] = value;
            }
        }

    }

    /// <summary>
    ///     Trigger settigns
    /// </summary>
    public class BackgroundTaskSettings : ConfigurationElement
    {
        /// <summary>
        /// The maximum number of background tasks running at once per tenant
        /// </summary>
        [ConfigurationProperty("perTenantConcurrency", DefaultValue = 10, IsRequired = false)]
        public int PerTenantConcurrency
        {
            get
            {
                return (int)this["perTenantConcurrency"];
            }

            set
            {
                this["perTenantConcurrency"] = value;
            }
        }

        /// <summary>
        /// The amount of time a workflow will run before being suspended and put back on the queue.
        /// </summary>
        [ConfigurationProperty("suspendTimeoutSeconds", DefaultValue = 10, IsRequired = false)]
        public int SuspendTimeoutSeconds
        {
            get
            {
                return (int)this["suspendTimeoutSeconds"];
            }

            set
            {
                this["suspendTimeoutSeconds"] = value;
            }
        }
    }

}