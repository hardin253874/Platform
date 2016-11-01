// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Input;
using ReadiMon.Core;
using ReadiMon.Shared.Core;

namespace ReadiMon
{
	/// <summary>
	///     AboutWindow view model.
	/// </summary>
	public class AboutWindowViewModel : ViewModelBase
	{
		/// <summary>
		///     The loaded assemblies
		/// </summary>
		private ObservableCollection<PluginWrapper> _loadedAssemblies = new ObservableCollection<PluginWrapper>( );

		/// <summary>
		///     The selected plugin
		/// </summary>
		private PluginWrapper _selectedPlugin;

		/// <summary>
		///     Initializes a new instance of the <see cref="AboutWindowViewModel" /> class.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">parentWindow</exception>
		public AboutWindowViewModel( )
		{
			LoadAssemblies( );

			EnableCommand = new DelegateCommand( ( ) => EnableDisableClick( true ) );
			DisableCommand = new DelegateCommand( ( ) => EnableDisableClick( false ) );
		}

		/// <summary>
		///     Gets or sets the disable command.
		/// </summary>
		/// <value>
		///     The disable command.
		/// </value>
		public ICommand DisableCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the dot net copyright.
		/// </summary>
		/// <value>
		///     The dot net copyright.
		/// </value>
		public string DotNetCopyright
		{
			get
			{
				return "(c) 2014 Microsoft Corporation";
			}
		}

		/// <summary>
		///     Gets the dot net rights.
		/// </summary>
		/// <value>
		///     The dot net rights.
		/// </value>
		public string DotNetRights
		{
			get
			{
				return "All rights reserved.";
			}
		}

		/// <summary>
		///     Gets the dot net title.
		/// </summary>
		/// <value>
		///     The dot net title.
		/// </value>
		public string DotNetTitle
		{
			get
			{
				return "Microsoft .NET Framework";
			}
		}

		/// <summary>
		///     Gets the dot net version.
		/// </summary>
		/// <value>
		///     The dot net version.
		/// </value>
		public string DotNetVersion
		{
			get
			{
				object[ ] targetFw = Assembly.GetExecutingAssembly( ).GetCustomAttributes( typeof ( TargetFrameworkAttribute ), false );

				return ( ( TargetFrameworkAttribute ) targetFw[ 0 ] ).FrameworkDisplayName;
			}
		}

		/// <summary>
		///     Gets or sets the enable command.
		/// </summary>
		/// <value>
		///     The enable command.
		/// </value>
		public ICommand EnableCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the loaded assemblies.
		/// </summary>
		/// <value>
		///     The loaded assemblies.
		/// </value>
		public ObservableCollection<PluginWrapper> LoadedAssemblies
		{
			get
			{
				return _loadedAssemblies;
			}
			set
			{
				SetProperty( ref _loadedAssemblies, value );
			}
		}

		/// <summary>
		///     Gets the product copyright.
		/// </summary>
		/// <value>
		///     The product copyright.
		/// </value>
		public string ProductCopyright
		{
			get
			{
				return "(c) 2015 ReadiNow Corporation Pty Ltd";
			}
		}

		/// <summary>
		///     Gets the product rights.
		/// </summary>
		/// <value>
		///     The product rights.
		/// </value>
		public string ProductRights
		{
			get
			{
				return "All rights reserved.";
			}
		}

		/// <summary>
		///     Gets the product title.
		/// </summary>
		/// <value>
		///     The product title.
		/// </value>
		public string ProductTitle
		{
			get
			{
				return "ReadiMon " + DateTime.Now.Year;
			}
		}

		/// <summary>
		///     Gets the product version.
		/// </summary>
		/// <value>
		///     The product version.
		/// </value>
		public string ProductVersion
		{
			get
			{
				Assembly assembly = Assembly.GetEntryAssembly( );
				FileVersionInfo info = FileVersionInfo.GetVersionInfo( assembly.Location );

				return "Version " + info.ProductVersion;
			}
		}

		/// <summary>
		///     Gets or sets the selected plugin.
		/// </summary>
		/// <value>
		///     The selected plugin.
		/// </value>
		public PluginWrapper SelectedPlugin
		{
			get
			{
				return _selectedPlugin;
			}
			set
			{
				SetProperty( ref _selectedPlugin, value );
			}
		}

		/// <summary>
		///     Enable/Disable click.
		/// </summary>
		private void EnableDisableClick( bool enabled )
		{
			if ( SelectedPlugin == null )
			{
				return;
			}

			SelectedPlugin.Enabled = enabled;
		}

		/// <summary>
		///     Loads the assemblies.
		/// </summary>
		private void LoadAssemblies( )
		{
			foreach ( var plugin in PluginManager.Plugins )
			{
				LoadedAssemblies.Add( plugin );
			}
		}
	}
}