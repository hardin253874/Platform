// Copyright 2011-2014 Global Software Innovation Pty Ltd

using System.Configuration;

namespace ReadiMon.AddinView.Configuration
{
	/// <summary>
	///     Plugin configuration base.
	/// </summary>
	public class PluginConfigurationBase : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets whether the plugin is enabled.
		/// </summary>
		/// <value>
		///     True if the plugin is enabled; false otherwise.
		/// </value>
		[ConfigurationProperty( "enabled", DefaultValue = "true", IsRequired = true )]
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

		/// <summary>
		///     Gets or sets the <see cref="System.Object" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public new object this[ string key ]
		{
			get
			{
				return base[ key.ToCamelCase( ) ];
			}
			set
			{
				base[ key.ToCamelCase( ) ] = value;
			}
		}
	}
}