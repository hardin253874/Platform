// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

// ReSharper disable InconsistentNaming

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     The process monitor settings class.
	/// </summary>
	/// <seealso cref="ConfigurationElement" />
	public class ProcessMonitorSettings : ConfigurationElement
	{
		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="ProcessMonitorSettings" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty( "enabled", IsRequired = true )]
		public bool Enabled
		{
			get
			{
				return ( bool ) this[ "enabled" ];
			}
			set
			{
				this[ "enabled" ] = value;
			}
		}
	}
}