// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Defines the server configuration section
	/// </summary>
	public class ServerConfiguration : ConfigurationSection
    {
        /// <summary>
        ///     Gets or sets the security settings configuration element.
        /// </summary>
        [ConfigurationProperty( "security" )]
        public SecuritySettings Security
		{
			get
			{
				return ( SecuritySettings ) this[ "security" ];
			}

			set
			{
				this[ "security" ] = value;
			}
		}

        /// <summary>
        ///     Gets or sets the entity settings configuration element.
        /// </summary>
        [ConfigurationProperty("entityWebApi")]
        public EntityWebApiSettings EntityWebApi
        {
            get
            {
                return (EntityWebApiSettings)this["entityWebApi"];
            }

            set
            {
                this["entityWebApi"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the file manager related settings configuration element.
        /// </summary>
        [ConfigurationProperty( "uploadDirectory" )]
        public UploadDirectorySettings UploadDirectory
        {
            get
            {
                return ( UploadDirectorySettings ) this [ "uploadDirectory" ];
            }

            set
            {
                this [ "uploadDirectory" ] = value;
            }
        }


        /// <summary>
        /// Gets or sets the client settings.
        /// </summary>
        [ConfigurationProperty("client")]
        public ClientSettings Client
        {
            get
            {
                return (ClientSettings)this["client"];
            }
            set
            {
                this["client"] = value;
            }
        }

		/// <summary>
		///     Gets or sets the process monitor.
		/// </summary>
		/// <value>
		///     The process monitor.
		/// </value>
		[ConfigurationProperty( "processMonitor" )]
		public ProcessMonitorSettings ProcessMonitor
		{
			get
			{
				return ( ProcessMonitorSettings ) this [ "processMonitor" ];
			}
			set
			{
				this[ "processMonitor" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the system information.
		/// </summary>
		/// <value>
		///     The system information.
		/// </value>
		[ConfigurationProperty( "systemInfo" )]
		public SystemInfoSettings SystemInfo
		{
			get
			{
				return ( SystemInfoSettings ) this[ "systemInfo" ];
			}
			set
			{
				this[ "systemInfo" ] = value;
			}
		}

		/// <summary>
		/// Gets or sets the remote execute.
		/// </summary>
		/// <value>
		/// The remote execute.
		/// </value>
		[ConfigurationProperty( "remoteExec" )]
		public RemoteExecSettings RemoteExec
		{
			get
			{
				return ( RemoteExecSettings ) this [ "remoteExec" ];
			}
			set
			{
				this [ "remoteExec" ] = value;
			}
		}

	}
}