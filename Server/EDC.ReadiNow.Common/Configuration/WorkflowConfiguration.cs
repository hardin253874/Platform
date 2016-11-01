// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Defines the workflow configuration section
	/// </summary>
	public class WorkflowConfiguration : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets a value indicating whether security is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if security is enabled; otherwise, <c>false</c>.
		/// </value>
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
        [ConfigurationProperty("maxRunTimeSeconds", DefaultValue = 120, IsRequired = false)]
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


    }

}