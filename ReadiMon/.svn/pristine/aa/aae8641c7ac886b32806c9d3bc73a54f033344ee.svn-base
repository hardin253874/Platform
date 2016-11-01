// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Configuration;
using ReadiMon.AddinView.Configuration;

namespace ReadiMon.Plugin.Misc
{
	/// <summary>
	///     Plugin configuration.
	/// </summary>
	public class ClipboardMonitorPluginConfiguration : PluginConfigurationBase
	{
		/// <summary>
		///     Gets or sets a value indicating whether to monitor the clipboard.
		/// </summary>
		/// <value>
		///     <c>true</c> if monitoring the clipboard; otherwise, <c>false</c>.
		/// </value>
		[ConfigurationProperty( "monitorClipboard", DefaultValue = "true", IsRequired = true )]
		public bool MonitorClipboard
		{
			get
			{
				return ( bool ) this[ "monitorClipboard" ];
			}
			set
			{
				this[ "monitorClipboard" ] = value;
			}
		}
	}
}