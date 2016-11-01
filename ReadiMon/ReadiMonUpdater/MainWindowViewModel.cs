// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ReadiMonUpdater
{
	/// <summary>
	///     Main Window view model.
	/// </summary>
	public class MainWindowViewModel : ViewModelBase
	{
		/// <summary>
		///     The maximum
		/// </summary>
		private int _maximum = 100;

		/// <summary>
		///     The progress value
		/// </summary>
		private int _progress;

		/// <summary>
		///     Gets or sets the maximum.
		/// </summary>
		/// <value>
		///     The maximum.
		/// </value>
		public int Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				SetProperty( ref _maximum, value );
			}
		}

		/// <summary>
		///     Gets or sets the progress.
		/// </summary>
		/// <value>
		///     The progress.
		/// </value>
		public int Progress
		{
			get
			{
				return _progress;
			}
			set
			{
				SetProperty( ref _progress, value );
			}
		}

		/// <summary>
		///     Copies the directory.
		/// </summary>
		/// <param name="directoryInfo">The directory information.</param>
		private void CopyDirectory( object directoryInfo )
		{
			var details = directoryInfo as Tuple<string, string>;

			if ( details != null )
			{
				var parent = new DirectoryInfo( details.Item2 ).Name;

				DirectoryCopy( details.Item2, Path.Combine( details.Item1, parent ), true );
			}
		}

		/// <summary>
		///     Copies the file.
		/// </summary>
		/// <param name="fileInfo">The file information.</param>
		private void CopyFile( object fileInfo )
		{
			var details = fileInfo as Tuple<string, string>;

			if ( details != null )
			{
				var filename = Path.GetFileName( details.Item2 );

				if ( !string.IsNullOrEmpty( filename ) )
				{
					Retry( ( ) => File.Copy( details.Item2, Path.Combine( details.Item1, filename ), true ), 5 );
				}
			}
		}

		/// <summary>
		///     Directories the copy.
		/// </summary>
		/// <param name="sourceDirectoryName">Name of the source directory.</param>
		/// <param name="destinationDirectoryName">Name of the destination directory.</param>
		/// <param name="copySubDirectories">if set to <c>true</c> copy sub directories.</param>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		///     Source directory does not exist or could not be found:
		///     + sourceDirName
		/// </exception>
		private void DirectoryCopy( string sourceDirectoryName, string destinationDirectoryName, bool copySubDirectories )
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo( sourceDirectoryName );
			DirectoryInfo[ ] dirs = dir.GetDirectories( );

			if ( !dir.Exists )
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirectoryName );
			}

			// If the destination directory doesn't exist, create it. 
			if ( !Directory.Exists( destinationDirectoryName ) )
			{
				Retry( ( ) => Directory.CreateDirectory( destinationDirectoryName ), 5 );
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[ ] files = dir.GetFiles( );
			foreach ( FileInfo file in files )
			{
				string temppath = Path.Combine( destinationDirectoryName, file.Name );

				FileInfo file1 = file;
				Retry( ( ) => file1.CopyTo( temppath, true ), 5 );
			}

			// If copying subdirectories, copy them and their contents to new location. 
			if ( copySubDirectories )
			{
				foreach ( DirectoryInfo subdir in dirs )
				{
					string temppath = Path.Combine( destinationDirectoryName, subdir.Name );
					DirectoryCopy( subdir.FullName, temppath, true );
				}
			}
		}

		/// <summary>
		/// Called when [complete].
		/// </summary>
		/// <param name="dispatcher">The dispatcher.</param>
		/// <param name="primaryFileLocalPath">The primary file local path.</param>
		/// <param name="restart">if set to <c>true</c> [restart].</param>
		private void OnComplete( Dispatcher dispatcher, string primaryFileLocalPath, bool restart )
		{
			new Timer( o =>
			{
				if ( restart )
				{
					Process.Start( primaryFileLocalPath );
				}

				dispatcher.InvokeShutdown( );
			}, null, 1000, Timeout.Infinite );
		}

		/// <summary>
		///     Retries the specified action.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="count">The count.</param>
		private void Retry( Action action, int count )
		{
			int retryCount = 0;

			while ( retryCount < count )
			{
				try
				{
					action( );

					break;
				}
				catch
				{
					retryCount++;
					Thread.Sleep( 1000 );
				}
			}
		}

		/// <summary>
		/// Runs the update.
		/// </summary>
		/// <param name="localPath">The local path.</param>
		/// <param name="remotePath">The remote path.</param>
		/// <param name="primaryFileLocalPath">The primary file local path.</param>
		/// <param name="restart">if set to <c>true</c> [restart].</param>
		private async Task RunUpdate( string localPath, string remotePath, string primaryFileLocalPath, bool restart )
		{
			var files = Directory.GetFiles( remotePath );
			var directories = Directory.GetDirectories( remotePath );
            var dispatcher = Application.Current.Dispatcher;

            Maximum = files.Length + directories.Length - 1;

			var tasks = new List<Task>( );

			foreach ( string file in files )
			{
				var task = new Task( CopyFile, new Tuple<string, string>( localPath, file ) );

				tasks.Add( task );

                task.Start();
			    await task.ContinueWith(new Action<Task>(t => Progress++));
			}

			foreach ( string directory in directories )
			{
				var task = new Task( CopyDirectory, new Tuple<string, string>( localPath, directory ) );

				tasks.Add( task );

                task.Start();
			    await task.ContinueWith(new Action<Task>(t => Progress++));
			}

		    await Task.WhenAll(tasks.ToArray());            

            OnComplete( dispatcher, primaryFileLocalPath, restart );
		}

		/// <summary>
		///     Updates this instance.
		/// </summary>
		public async Task Update( )
		{
			string[ ] args = Environment.GetCommandLineArgs( );

			if ( args.Length < 2 )
			{
				Application.Current.Shutdown( );
				return;
			}

			string updateLocation = args[ 1 ];

			if ( ! Directory.Exists( updateLocation ) )
			{
				Application.Current.Shutdown( );
				return;
			}

			bool restart = args.Any( a => a.ToLowerInvariant( ).Trim( ) == "restart" );

			const string primaryFile = "ReadiMon.exe";

			var primaryFileNetworkPath = Path.Combine( updateLocation, primaryFile );

			if ( ! File.Exists( primaryFileNetworkPath ) )
			{
				Application.Current.Shutdown( );
				return;
			}

			Version networkVersion = AssemblyName.GetAssemblyName( primaryFileNetworkPath ).Version;

			var localDirectory = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );

			if ( localDirectory != null )
			{
				var primaryFileLocalPath = Path.Combine( localDirectory, primaryFile );

				if ( ! File.Exists( primaryFileLocalPath ) )
				{
					Application.Current.Shutdown( );
					return;
				}

				Version localVersion = AssemblyName.GetAssemblyName( primaryFileLocalPath ).Version;

				if ( networkVersion <= localVersion )
				{
					Application.Current.Shutdown( );
					return;
				}

				await RunUpdate( localDirectory, updateLocation, primaryFileLocalPath, restart );
			}
		}
	}
}