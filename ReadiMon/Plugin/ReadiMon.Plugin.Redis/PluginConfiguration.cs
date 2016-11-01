// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Configuration;
using ReadiMon.AddinView.Configuration;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Plugin configuration.
	/// </summary>
	public class PluginConfiguration : PluginConfigurationBase
	{
		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="PluginConfiguration" /> is monitor.
		/// </summary>
		/// <value>
		///     <c>true</c> if monitor; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty( "monitor", DefaultValue = "false", IsRequired = false )]
		public bool Monitor
		{
			get
			{
				return ( bool ) this[ "monitor" ];
			}
			set
			{
				this[ "monitor" ] = value;
			}
		}
	}
}