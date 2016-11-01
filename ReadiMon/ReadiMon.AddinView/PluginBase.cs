// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using ReadiMon.AddinView.Configuration;
using ReadiMon.Shared;

namespace ReadiMon.AddinView
{
	/// <summary>
	///     Plugin Base.
	/// </summary>
	public abstract class PluginBase : IPlugin
	{
		/// <summary>
		///     Gets or sets the configuration map.
		/// </summary>
		/// <value>
		///     The configuration map.
		/// </value>
		private ExeConfigurationFileMap ConfigurationMap
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the configuration section.
		/// </summary>
		/// <value>
		///     The name of the configuration section.
		/// </value>
		private string ConfigurationSectionName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the settings.
		/// </summary>
		/// <value>
		///     The settings.
		/// </value>
		protected IPluginSettings Settings
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the entry.
		/// </summary>
		/// <value>
		///     The name of the entry.
		/// </value>
		public string EntryName
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the entry ordinal.
		/// </summary>
		/// <value>
		///     The entry ordinal.
		/// </value>
		public int EntryOrdinal
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has options user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has options user interface; otherwise, <c>false</c>.
		/// </value>
		public bool HasOptionsUserInterface
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has user interface; otherwise, <c>false</c>.
		/// </value>
		public bool HasUserInterface
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the name of the section.
		/// </summary>
		/// <value>
		///     The name of the section.
		/// </value>
		public string SectionName
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the section ordinal.
		/// </summary>
		/// <value>
		///     The section ordinal.
		/// </value>
		public int SectionOrdinal
		{
			get;
			protected set;
		}

		/// <summary>
		///     Saves the options.
		/// </summary>
		public virtual void SaveOptions( )
		{
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public virtual FrameworkElement GetOptionsUserInterface( )
		{
			return null;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public virtual FrameworkElement GetUserInterface( )
		{
			return null;
		}

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		public virtual void OnShutdown( )
		{
		}

		/// <summary>
		///     Called when the plugin first initializes.
		/// </summary>
		public virtual void OnStartup( )
		{
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public virtual void OnStartupComplete( )
		{
		}

		/// <summary>
		///     Gets a value indicating whether this instance has a tool bar.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has tool bar; otherwise, <c>false</c>.
		/// </value>
		public bool HasToolBar
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the tool bar ordinal.
		/// </summary>
		/// <value>
		///     The tool bar ordinal.
		/// </value>
		public int ToolBarOrdinal
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public virtual FrameworkElement GetToolBar( )
		{
			return null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="IPlugin" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled
		{
			get
			{
				return GetConfigurationValue<bool>( );
			}
			set
			{
				SetConfigurationValue( value );
			}
		}

		/// <summary>
		///     Called when the plugin is first initialized.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public virtual void OnInitialize( IPluginSettings settings )
		{
			Settings = settings;

			InitializeConfiguration( );
		}

		/// <summary>
		///     Called when updating the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void OnUpdateSettings( IPluginSettings settings )
		{
			Settings = settings;

			OnUpdateSettings( );
		}

		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public virtual bool OnMessageReceived( string message )
		{
			return false;
		}

		/// <summary>
		/// Invokes the specified argument.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <returns></returns>
		public virtual string Invoke( string argument )
		{
			return null;
		}

		/// <summary>
		///     Creates the configuration section.
		/// </summary>
		/// <returns></returns>
		protected virtual PluginConfigurationBase CreateConfigurationSection( )
		{
			return new PluginConfigurationBase( );
		}

		/// <summary>
		///     Gets the configuration value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		protected T GetConfigurationValue<T>( [CallerMemberName] string propertyName = null )
		{
			System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration( ConfigurationMap, ConfigurationUserLevel.None );

			if ( !configuration.Sections.Contains( ConfigurationSectionName ) )
			{
				throw new InvalidOperationException( string.Format( "Invalid section name '{0}' in configuration file '{1}'.", ConfigurationSectionName, Settings.ConfigurationFile ) );
			}

			var section = configuration.GetSection( ConfigurationSectionName ) as PluginConfigurationBase;

			if ( section == null )
			{
				throw new InvalidOperationException( string.Format( "Invalid section '{0}' in configuration file '{1}'.", ConfigurationSectionName, Settings.ConfigurationFile ) );
			}

			object val = null;

			if ( section.ElementInformation.Properties.Contains( propertyName ) )
			{
				val = section[ propertyName ];
			}

			if ( val != null )
			{
				return ( T ) val;
			}

			return default(T);
		}

		/// <summary>
		///     Initializes the configuration.
		/// </summary>
		private void InitializeConfiguration( )
		{
			ConfigurationSectionName = GetType( ).FullName;

			ConfigurationMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = Settings.ConfigurationFile
			};

			System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration( ConfigurationMap, ConfigurationUserLevel.None );

			if ( !configuration.Sections.Contains( ConfigurationSectionName ) )
			{
				PluginConfigurationBase section = CreateConfigurationSection( );

				if ( section != null )
				{
					section.Enabled = true;
					configuration.Sections.Add( ConfigurationSectionName, section );

					configuration.Save( ConfigurationSaveMode.Modified );
				}
			}
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected virtual void OnUpdateSettings( )
		{
		}

		/// <summary>
		///     Sets the configuration value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value.</param>
		/// <param name="propertyName">Name of the property.</param>
		protected void SetConfigurationValue<T>( T value, [CallerMemberName] string propertyName = null )
		{
			System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration( ConfigurationMap, ConfigurationUserLevel.None );

			if ( !configuration.Sections.Contains( ConfigurationSectionName ) )
			{
				throw new InvalidOperationException( string.Format( "Invalid section name '{0}' in configuration file '{1}'.", ConfigurationSectionName, Settings.ConfigurationFile ) );
			}

			var section = configuration.GetSection( ConfigurationSectionName ) as PluginConfigurationBase;

			if ( section == null )
			{
				throw new InvalidOperationException( string.Format( "Invalid section '{0}' in configuration file '{1}'.", ConfigurationSectionName, Settings.ConfigurationFile ) );
			}

			section[ propertyName ] = value;

			configuration.Save( ConfigurationSaveMode.Modified );
		}
	}
}