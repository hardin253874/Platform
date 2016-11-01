// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReadiMon.HostView;
using ReadiMon.Shared.Core;

namespace ReadiMon.Core
{
	/// <summary>
	///     Plugin Manager
	/// </summary>
	public static class PluginManager
	{
		/// <summary>
		///     The plugin instances
		/// </summary>
		private static readonly Lazy<List<PluginWrapper>> PluginInstances = new Lazy<List<PluginWrapper>>( LoadPlugins, false );

		/// <summary>
		///     Gets the plugins.
		/// </summary>
		/// <value>
		///     The plugins.
		/// </value>
		public static List<PluginWrapper> Plugins
		{
			get
			{
				return PluginInstances.Value;
			}
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public static void Dispose( )
		{
			if ( PluginInstances.IsValueCreated )
			{
				foreach ( PluginWrapper plugin in PluginInstances.Value )
				{
					plugin.Plugin.OnShutdown( );
				}
			}
		}

		/// <summary>
		///     Loads the plugins.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Invalid assembly path.</exception>
		private static List<PluginWrapper> LoadPlugins( )
		{
			/////
			// Get the current path.
			/////
			string currentPath = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );

			if ( currentPath == null )
			{
				throw new ArgumentException( "Invalid assembly path." );
			}

			string addInRoot = Path.Combine( currentPath, "Pipeline" );

			/////
			// Update the cache files of the pipeline segments and add-ins. 
			/////
			string[ ] warnings = AddInStore.Update( addInRoot );

			foreach ( string warning in warnings )
			{
				EventLog.Instance.WriteWarning( warning );
			}

			/////
			// Search for add-ins of type IPlugin (the host view of the add-in).
			/////
			List<AddInToken> tokens = AddInStore.FindAddIns( typeof ( IPlugin ), addInRoot ).ToList( );

			var plugins = new List<PluginWrapper>( );

			foreach ( AddInToken token in tokens )
			{
				EventLog.Instance.WriteTrace( "Activating '{0}' from assembly '{1}'...", token.Name, token.AssemblyName );

				var plugin = token.Activate<IPlugin>( AddInSecurityLevel.FullTrust );

				plugins.Add( new PluginWrapper( token, plugin ) );
			}

			return plugins;
		}
	}
}