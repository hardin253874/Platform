// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Hosting;
using ReadiMon.HostView;
using ReadiMon.Shared.Core;

namespace ReadiMon.Core
{
	/// <summary>
	///     Plugin wrapper.
	/// </summary>
	public class PluginWrapper : ViewModelBase
	{
		/// <summary>
		///     Whether the plugin is enabled or not.
		/// </summary>
		private bool _enabled;

		/// <summary>
		///     Initializes a new instance of the <see cref="PluginWrapper" /> class.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="plugin">The plugin.</param>
		/// <exception cref="System.ArgumentNullException">
		///     token
		///     or
		///     plugin
		/// </exception>
		public PluginWrapper( AddInToken token, IPlugin plugin )
		{
			if ( token == null )
			{
				throw new ArgumentNullException( "token" );
			}

			if ( plugin == null )
			{
				throw new ArgumentNullException( "plugin" );
			}

			Token = token;
			Plugin = plugin;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="PluginWrapper" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				SetProperty( ref _enabled, value );

				Plugin.Enabled = value;

				if ( value && OnEnable != null )
				{
					OnEnable( this );
				}
				else if ( !value && OnDisable != null )
				{
					OnDisable( this );
				}
			}
		}

		/// <summary>
		///     Gets or sets the on disable.
		/// </summary>
		/// <value>
		///     The on disable.
		/// </value>
		public Action<PluginWrapper> OnDisable
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the on enable.
		/// </summary>
		/// <value>
		///     The on enable.
		/// </value>
		public Action<PluginWrapper> OnEnable
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the plugin.
		/// </summary>
		/// <value>
		///     The plugin.
		/// </value>
		public IPlugin Plugin
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the token.
		/// </summary>
		/// <value>
		///     The token.
		/// </value>
		public AddInToken Token
		{
			get;
			private set;
		}

		/// <summary>
		///     Initializes the specified settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void Initialize( PluginSettings settings )
		{
			if ( Plugin == null )
			{
				return;
			}

			Plugin.OnInitialize( settings );

			_enabled = Plugin.Enabled;
		}
	}
}