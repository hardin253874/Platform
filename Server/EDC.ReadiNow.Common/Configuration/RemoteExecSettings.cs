// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     The remote execute settings class.
	/// </summary>
	/// <seealso cref="System.Configuration.ConfigurationElement" />
	public class RemoteExecSettings : ConfigurationElement
	{
		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="RemoteExecSettings" /> is enabled.
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