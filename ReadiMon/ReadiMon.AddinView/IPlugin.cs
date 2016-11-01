// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using System.Windows;
using ReadiMon.Shared;

namespace ReadiMon.AddinView
{
	/// <summary>
	///     IPlugin interface.
	/// </summary>
	[AddInBase]
	public interface IPlugin
	{
		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="IPlugin" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		bool Enabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the name of the entry.
		/// </summary>
		/// <value>
		///     The name of the entry.
		/// </value>
		string EntryName
		{
			get;
		}

		/// <summary>
		///     Gets the entry ordinal.
		/// </summary>
		/// <value>
		///     The entry ordinal.
		/// </value>
		int EntryOrdinal
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has options user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has options user interface; otherwise, <c>false</c>.
		/// </value>
		bool HasOptionsUserInterface
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has a tool bar.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has tool bar; otherwise, <c>false</c>.
		/// </value>
		bool HasToolBar
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has user interface; otherwise, <c>false</c>.
		/// </value>
		bool HasUserInterface
		{
			get;
		}

		/// <summary>
		///     Gets the name of the section.
		/// </summary>
		/// <value>
		///     The name of the section.
		/// </value>
		string SectionName
		{
			get;
		}

		/// <summary>
		///     Gets the section ordinal.
		/// </summary>
		/// <value>
		///     The section ordinal.
		/// </value>
		int SectionOrdinal
		{
			get;
		}

		/// <summary>
		///     Gets the tool bar ordinal.
		/// </summary>
		/// <value>
		///     The tool bar ordinal.
		/// </value>
		int ToolBarOrdinal
		{
			get;
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		FrameworkElement GetOptionsUserInterface( );

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		FrameworkElement GetToolBar( );

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		FrameworkElement GetUserInterface( );

		/// <summary>
		///     Invokes the specified argument.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <returns></returns>
		string Invoke( string argument );

		/// <summary>
		///     Called when the plugin is first initialized.
		/// </summary>
		/// <param name="settings">The settings.</param>
		void OnInitialize( IPluginSettings settings );

		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <returns></returns>
		bool OnMessageReceived( string message );

		/// <summary>
		///     Called when the plugin shuts down.
		/// </summary>
		void OnShutdown( );

		/// <summary>
		///     Called when the plugin is started.
		/// </summary>
		void OnStartup( );

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		void OnStartupComplete( );

		/// <summary>
		///     Called when updating the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		void OnUpdateSettings( IPluginSettings settings );

		/// <summary>
		///     Saves the options.
		/// </summary>
		void SaveOptions( );
	}
}