// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Windows;
using ReadiMon.AddinView;
using ReadiMon.Contract;

namespace ReadiMon.AddinSideAdapter
{
	/// <summary>
	///     The Addin View to contract addin side adapter.
	/// </summary>
	[AddInAdapter]
	public class AddinViewToContractAddinSideAdapter : ContractBase, IPluginContract
	{
		/// <summary>
		///     The plugin view.
		/// </summary>
		private readonly IPlugin _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="AddinViewToContractAddinSideAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public AddinViewToContractAddinSideAdapter( IPlugin view )
		{
			_view = view;
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
				return _view.SectionName;
			}
		}

		/// <summary>
		///     Called when the plugin is first initialized.
		/// </summary>
		public void OnStartup( )
		{
			_view.OnStartup( );
		}

		/// <summary>
		///     Called when startup is complete.
		/// </summary>
		public void OnStartupComplete( )
		{
			_view.OnStartupComplete( );
		}

		/// <summary>
		///     Called when the plugin is shutdown.
		/// </summary>
		public void OnShutdown( )
		{
			_view.OnShutdown( );
		}


		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public INativeHandleContract GetUserInterface( )
		{
			FrameworkElement fe = _view.GetUserInterface( );
			INativeHandleContract inhc = FrameworkElementAdapters.ViewToContractAdapter( fe );
			return inhc;
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
				return _view.Enabled;
			}
			set
			{
				_view.Enabled = value;
			}
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
				return _view.EntryName;
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
				return _view.SectionOrdinal;
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
				return _view.EntryOrdinal;
			}
		}


		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public INativeHandleContract GetOptionsUserInterface( )
		{
			FrameworkElement fe = _view.GetOptionsUserInterface( );
			INativeHandleContract inhc = FrameworkElementAdapters.ViewToContractAdapter( fe );
			return inhc;
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
				return _view.HasOptionsUserInterface;
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
				return _view.HasUserInterface;
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
				return _view.HasToolBar;
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
				return _view.ToolBarOrdinal;
			}
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public INativeHandleContract GetToolBar( )
		{
			FrameworkElement fe = _view.GetToolBar( );
			INativeHandleContract inhc = FrameworkElementAdapters.ViewToContractAdapter( fe );
			return inhc;
		}

		/// <summary>
		///     Called when the plugin is first initialized.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void OnInitialize( IPluginSettingsContract settings )
		{
			_view.OnInitialize( new PluginSettingsContractToViewAddinAdapter( settings ) );
		}


		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public bool OnMessageReceived( string message )
		{
			return _view.OnMessageReceived( message );
		}


		/// <summary>
		///     Saves the options.
		/// </summary>
		public void SaveOptions( )
		{
			_view.SaveOptions( );
		}


		/// <summary>
		///     Called when updating the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void OnUpdateSettings( IPluginSettingsContract settings )
		{
			_view.OnUpdateSettings( new PluginSettingsContractToViewAddinAdapter( settings ) );
		}

		/// <summary>
		/// Invokes the specified argument.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <returns></returns>
		public string Invoke( string argument )
		{
			return _view.Invoke( argument );
		}
	}
}