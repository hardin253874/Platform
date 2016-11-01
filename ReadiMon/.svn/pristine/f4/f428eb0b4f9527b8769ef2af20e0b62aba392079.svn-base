// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Windows;
using ReadiMon.Contract;
using ReadiMon.HostView;
using ReadiMon.Shared;

namespace ReadiMon.HostSideAdapter
{
	/// <summary>
	///     The Plugin contract to Host adapter.
	/// </summary>
	[HostAdapter]
	public class PluginContractToViewHostAdapter : IPlugin, IDisposable
	{
		/// <summary>
		///     The plugin contract.
		/// </summary>
		private readonly IPluginContract _contract;

		/// <summary>
		///     The contract handle.
		/// </summary>
		private readonly ContractHandle _handle;

		/// <summary>
		///     Initializes a new instance of the <see cref="PluginContractToViewHostAdapter" /> class.
		/// </summary>
		/// <param name="contract">The contract.</param>
		public PluginContractToViewHostAdapter( IPluginContract contract )
		{
			_contract = contract;
			_handle = new ContractHandle( contract );
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( _handle != null )
			{
				_handle.Dispose( );
			}
		}

		/// <summary>
		///     Gets the name of the section.
		/// </summary>
		/// <value>
		///     The name of the section.
		/// </value>
		public string SectionName
		{
			get
			{
				return _contract.SectionName;
			}
		}

		/// <summary>
		///     Called when the plugin is first initialized.
		/// </summary>
		public void OnStartup( )
		{
			_contract.OnStartup( );
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public void OnStartupComplete( )
		{
			_contract.OnStartupComplete( );
		}

		/// <summary>
		///     Called when the plugin is shutdown.
		/// </summary>
		public void OnShutdown( )
		{
			_contract.OnShutdown( );
		}


		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public FrameworkElement GetUserInterface( )
		{
			// Convert the INativeHandleContract that was passed from the add-in side 
			// of the isolation boundary to a FrameworkElement
			INativeHandleContract inhc = _contract.GetUserInterface( );
			FrameworkElement fe = FrameworkElementAdapters.ContractToViewAdapter( inhc );
			return fe;
		}

		/// <summary>
		///     Gets the name of the entry.
		/// </summary>
		/// <value>
		///     The name of the entry.
		/// </value>
		public string EntryName
		{
			get
			{
				return _contract.EntryName;
			}
		}


		/// <summary>
		///     Gets the entry ordinal.
		/// </summary>
		/// <value>
		///     The entry ordinal.
		/// </value>
		public int EntryOrdinal
		{
			get
			{
				return _contract.EntryOrdinal;
			}
		}

		/// <summary>
		///     Gets the section ordinal.
		/// </summary>
		/// <value>
		///     The section ordinal.
		/// </value>
		public int SectionOrdinal
		{
			get
			{
				return _contract.SectionOrdinal;
			}
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public FrameworkElement GetOptionsUserInterface( )
		{
			/////
			// Convert the INativeHandleContract that was passed from the add-in side 
			// of the isolation boundary to a FrameworkElement
			/////
			INativeHandleContract inhc = _contract.GetOptionsUserInterface( );

			FrameworkElement fe = FrameworkElementAdapters.ContractToViewAdapter( inhc );

			return fe;
		}


		/// <summary>
		///     Gets a value indicating whether this instance has options user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has options user interface; otherwise, <c>false</c>.
		/// </value>
		public bool HasOptionsUserInterface
		{
			get
			{
				return _contract.HasOptionsUserInterface;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has user interface; otherwise, <c>false</c>.
		/// </value>
		public bool HasUserInterface
		{
			get
			{
				return _contract.HasUserInterface;
			}
		}


		/// <summary>
		///     Gets a value indicating whether this instance has a tool bar.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has tool bar; otherwise, <c>false</c>.
		/// </value>
		public bool HasToolBar
		{
			get
			{
				return _contract.HasToolBar;
			}
		}

		/// <summary>
		///     Gets the tool bar ordinal.
		/// </summary>
		/// <value>
		///     The tool bar ordinal.
		/// </value>
		public int ToolBarOrdinal
		{
			get
			{
				return _contract.ToolBarOrdinal;
			}
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public FrameworkElement GetToolBar( )
		{
			/////
			// Convert the INativeHandleContract that was passed from the add-in side 
			// of the isolation boundary to a FrameworkElement
			/////
			INativeHandleContract inhc = _contract.GetToolBar( );

			FrameworkElement toolbar = FrameworkElementAdapters.ContractToViewAdapter( inhc );

			return toolbar;
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
				return _contract.Enabled;
			}
			set
			{
				_contract.Enabled = value;
			}
		}


		/// <summary>
		///     Called when the plugin is first initialized.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void OnInitialize( IPluginSettings settings )
		{
			_contract.OnInitialize( new PluginSettingsContractToViewHostAdapter( settings ) );
		}


		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public bool OnMessageReceived( string message )
		{
			return _contract.OnMessageReceived( message );
		}


		/// <summary>
		///     Saves the options.
		/// </summary>
		public void SaveOptions( )
		{
			_contract.SaveOptions( );
		}


		/// <summary>
		///     Called when updating the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void OnUpdateSettings( IPluginSettings settings )
		{
			_contract.OnUpdateSettings( new PluginSettingsContractToViewHostAdapter( settings ) );
		}

		/// <summary>
		/// Invokes the specified argument.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <returns></returns>
		public string Invoke( string argument )
		{
			return _contract.Invoke( argument );
		}
	}
}