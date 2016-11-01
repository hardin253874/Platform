// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Threading;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Interprocess communications.
	/// </summary>
	public static partial class InterprocessCommunications
	{
		/// <summary>
		///     Interprocess communications subscriber.
		/// </summary>
		private sealed class Subscriber : IDisposable
		{
			/// <summary>
			///     Buffer.
			/// </summary>
			private volatile Queue<byte[ ]> _buffer;

			/// <summary>
			///     Whether this instance has been disposed of or not.
			/// </summary>
			private bool _disposed;

			/// <summary>
			///     Event wait handle.
			/// </summary>
			private volatile EventWaitHandle _event;

			/// <summary>
			///     File.
			/// </summary>
			private volatile MemoryMappedFile _file;

			/// <summary>
			///     View.
			/// </summary>
			private volatile MemoryMappedViewAccessor _view;

			/// <summary>
			///     Initializes a new instance of the <see cref="Subscriber" /> class.
			/// </summary>
			/// <param name="name">The name.</param>
			public Subscriber( string name )
			{
				SubscriberSyncRoot = new object( );
				Name = name;
			}

			/// <summary>
			///     Gets the buffer.
			/// </summary>
			/// <value>
			///     The buffer.
			/// </value>
			public Queue<byte[ ]> Buffer
			{
				get
				{
					if ( _buffer == null )
					{
						lock ( SubscriberSyncRoot )
						{
							if ( _buffer == null )
							{
								_buffer = new Queue<byte[ ]>( );
							}
						}
					}

					return _buffer;
				}
			}

			/// <summary>
			///     Gets the event.
			/// </summary>
			/// <value>
			///     The event.
			/// </value>
			public EventWaitHandle Event
			{
				get
				{
					if ( _event == null )
					{
						lock ( SyncRoot )
						{
							if ( _event == null )
							{
								EventWaitHandle eventWaitHandle;

								EventWaitHandle.TryOpenExisting( GetEventName( Name ), EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize, out eventWaitHandle );

								_event = eventWaitHandle;
							}
						}
					}

					return _event;
				}
			}

			/// <summary>
			///     Gets or sets the last successful send.
			/// </summary>
			/// <value>
			///     The last successful send.
			/// </value>
			public DateTime LastSuccessfulSend
			{
				get;
				set;
			}

			/// <summary>
			///     Gets the sync root.
			/// </summary>
			/// <value>
			///     The sync root.
			/// </value>
			public object SubscriberSyncRoot
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets or sets the view.
			/// </summary>
			/// <value>
			///     The view.
			/// </value>
			public MemoryMappedViewAccessor View
			{
				get
				{
					if ( _view == null )
					{
						lock ( SyncRoot )
						{
							if ( _view == null )
							{
								MemoryMappedFile file = File;

								if ( file == null )
								{
									return null;
								}

								_view = OpenView( file, 0, RemoteMemoryMappedFileSize, Name );
							}
						}
					}

					return _view;
				}
			}

			/// <summary>
			///     Gets or sets the file.
			/// </summary>
			/// <value>
			///     The file.
			/// </value>
			private MemoryMappedFile File
			{
				get
				{
					if ( _file == null )
					{
						lock ( SyncRoot )
						{
							if ( _file == null )
							{
								try
								{
									_file = OpenMemoryMappedFile( GetGlobalName( Name ), RemoteMemoryMappedFileSize );
								}
								catch ( FileNotFoundException ex )
								{
									/////
									// If the memory mapped file doesn't exist, the application domain may have gone down so unregister it.
									/////
									UnregisterAppDomain( Name );

									EventLog.Application.WriteError( "Failed to open memory mapped file '{0}'.\r\n{1}", GetGlobalName( Name ), ex );
								}
								catch ( UnauthorizedAccessException ex )
								{
									/////
									// Unable to access the memory mapped file so unregister it and let the owning application domain attempt to recreate it.
									/////
									UnregisterAppDomain( Name );

									EventLog.Application.WriteError( "Failed to open memory mapped file '{0}'.\r\n{1}", GetGlobalName( Name ), ex );
								}
							}
						}
					}

					return _file;
				}
			}

			/// <summary>
			///     Gets or sets the name.
			/// </summary>
			/// <value>
			///     The name.
			/// </value>
			private string Name
			{
				get;
				set;
			}

			/// <summary>
			///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose( )
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}

			/// <summary>
			///     Releases unmanaged and - optionally - managed resources.
			/// </summary>
			/// <param name="disposing">
			///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
			/// </param>
			private void Dispose( bool disposing )
			{
				if ( !_disposed )
				{
					if ( disposing )
					{
						if ( _view != null )
						{
							_view.Dispose( );
							_view = null;
						}

						if ( _file != null )
						{
							_file.Dispose( );
							_file = null;
						}

						if ( _event != null )
						{
							_event.Dispose( );
							_event = null;
						}
					}

					_disposed = true;
				}
			}

			/// <summary>
			///     Finalizes an instance of the <see cref="Subscriber" /> class.
			/// </summary>
			~Subscriber( )
			{
				Dispose( false );
			}
		}
	}
}