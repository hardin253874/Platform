// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Configuration;
using ReadiMon.AddinView.Configuration;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     EntityBrowserPlugin configuration.
	/// </summary>
	public class EntityBrowserPluginConfiguration : PluginConfigurationBase
	{
		/// <summary>
		///     Gets or sets a value indicating whether [hide when minimized].
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide when minimized]; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty( "hideWhenMinimized", DefaultValue = "true", IsRequired = true )]
		public bool HideWhenMinimized
		{
			get
			{
				return ( bool ) this[ "hideWhenMinimized" ];
			}
			set
			{
				this[ "hideWhenMinimized" ] = value;
			}
		}
	}
}