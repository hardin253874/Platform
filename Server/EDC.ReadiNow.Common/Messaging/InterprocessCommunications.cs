// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.Tracing;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Interprocess communications.
	/// </summary>
	public static partial class InterprocessCommunications
	{
		/// <summary>
		///     Delegate map name.
		/// </summary>
		private const string DelegateMapName = @"Global\SoftwarePlatformDelegateMap";

		/// <summary>
		///     Delegate mutex.
		/// </summary>
		private const string DelegateMutexName = @"Global\SoftwarePlatformDelegateMutex";

		/// <summary>
		///     Mutex timeout.
		/// </summary>
		private const int MutexTimeout = 2500;

		/// <summary>
		///     Registration map name.
		/// </summary>
		private const string RegistrationMapName = @"Global\SoftwarePlatformRegistrationMap";

		/// <summary>
		///     Registration mutex name.
		/// </summary>
		private const string RegistrationMutexName = @"Global\SoftwarePlatformRegistrationMutex";

		/// <summary>
		///     Size of the delegate memory map.
		/// </summary>
		private const int DelegateMemoryMappedFileSize = 1024 * 1024;

		/// <summary>
		///     Size of the local memory map.
		/// </summary>
		private const int LocalMemoryMappedFileSize = 1024 * 1024;

		/// <summary>
		///     Registered Application Domain Delimiter.
		/// </summary>
		private const char RegisteredAppDomainDelimiterMember = '\t';

		/// <summary>
		///     Size of the registration memory map.
		/// </summary>
		private const int RegistrationMemoryMappedFileSize = 1024 * 1024;

		/// <summary>
		///     Remote memory mapped file size.
		/// </summary>
		private const int RemoteMemoryMappedFileSize = 1024 * 1024;

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private static readonly object SyncRoot = new object( );

		/// <summary>
		///     AppDomain identifier.
		/// </summary>
		private static readonly string CurrentAppDomainId = Guid.NewGuid( ).ToString( );

		/// <summary>
		///     Delegate map.
		/// </summary>
		private static MemoryMappedFile _delegateMemoryMappedFile;

		/// <summary>
		///     Delegate map view.
		/// </summary>
		private static volatile MemoryMappedViewAccessor _delegateMemoryMappedFileView;

		/// <summary>
		///     Delegate mutex.
		/// </summary>
		private static volatile Mutex _delegateMutex;

		/// <summary>
		///     Delegates.
		/// </summary>
		private static volatile BidirectionalDictionary<Delegate, short> _delegates;

		/// <summary>
		///     Delimiters.
		/// </summary>
		private static char[ ] _delimiters;

		/// <summary>
		///     Encoding.
		/// </summary>
		private static volatile UTF8Encoding _encoding;

		/// <summary>
		///     Binary formatter.
		/// </summary>
		private static volatile BinaryFormatter _formatter;

		/// <summary>
		///     Whether this application domain is currently registered or not.
		/// </summary>
		private static bool _isRegistered;

		/// <summary>
		///     The last time the list of subscribers was refreshed.
		/// </summary>
		private static DateTime _lastSubscriberRefresh = DateTime.MinValue;

		/// <summary>
		///     Local event.
		/// </summary>
		private static volatile EventWaitHandle _localEvent;

		/// <summary>
		///     Local memory map.
		/// </summary>
		private static volatile MemoryMappedFile _localMemoryMappedFile;

		/// <summary>
		///     Local memory map view.
		/// </summary>
		private static volatile MemoryMappedViewAccessor _localMemoryMappedFileView;

		/// <summary>
		///     Registration memory map.
		/// </summary>
		private static MemoryMappedFile _registrationMemoryMappedFile;

		/// <summary>
		///     Registration memory map view.
		/// </summary>
		private static volatile MemoryMappedViewAccessor _registrationMemoryMappedFileView;

		/// <summary>
		///     Registration mutex.
		/// </summary>
		private static volatile Mutex _registrationMutex;

		/// <summary>
		///     Send event.
		/// </summary>
		private static volatile AutoResetEvent _sendEvent;

		/// <summary>
		///     Send queue.
		/// </summary>
		private static volatile Queue<ActionContainer> _sendQueue;

		/// <summary>
		///     Subscribers.
		/// </summary>
		private static volatile Dictionary<string, Subscriber> _subscribers;

		/// <summary>
		///     Initializes the <see cref="InterprocessCommunications" /> class.
		/// </summary>
		static InterprocessCommunications( )
		{
			/////
			// Static initialization indicates the domain is not currently registered.
			/////
			IsRegistered = false;

			/////
			// Setup ETW.
			/////
			Trace = PlatformTrace.Instance;

			/////
			// Create the send/receive thread.
			/////
			var receiverThread = new Thread( SendReceive )
			{
				IsBackground = true
			};

			receiverThread.Start( );
		}

		/// <summary>
		///     Gets the application domain id.
		/// </summary>
		/// <value>
		///     The application domain id.
		/// </value>
		private static string AppDomainId
		{
			get
			{
				return CurrentAppDomainId;
			}
		}

		/// <summary>
		///     Gets the delegate memory mapped file.
		/// </summary>
		/// <value>
		///     The delegate memory mapped file.
		/// </value>
		private static MemoryMappedFile DelegateMemoryMappedFile
		{
			get
			{
				if ( _delegateMemoryMappedFile == null )
				{
					if ( !DelegateMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
					{
						return null;
					}

					try
					{
						if ( _delegateMemoryMappedFile == null )
						{
							_delegateMemoryMappedFile = OpenMemoryMappedFile( DelegateMapName, DelegateMemoryMappedFileSize, true );
						}
					}
					finally
					{
						DelegateMutex.ReleaseMutex( );
					}
				}

				return _delegateMemoryMappedFile;
			}
		}

		/// <summary>
		///     Gets the registration memory mapped file view.
		/// </summary>
		/// <value>
		///     The registration memory mapped file view.
		/// </value>
		private static MemoryMappedViewAccessor DelegateMemoryMappedFileView
		{
			get
			{
				if ( _delegateMemoryMappedFileView == null )
				{
					lock ( SyncRoot )
					{
						if ( _delegateMemoryMappedFileView == null )
						{
							MemoryMappedViewAccessor viewAccessor;

							MemoryMappedFile delegateMap = DelegateMemoryMappedFile;

							try
							{
								viewAccessor = OpenView( delegateMap, 0, DelegateMemoryMappedFileSize, DelegateMapName );
							}
							catch ( Exception ex )
							{
								EventLog.Application.WriteWarning( "Failed to open the delegate memory mapped file '{0}'.\r\n{1}", DelegateMapName, ex );

								viewAccessor = null;
							}

							_delegateMemoryMappedFileView = viewAccessor;
						}
					}
				}

				return _delegateMemoryMappedFileView;
			}
		}

		/// <summary>
		///     Gets the delegate mutex.
		/// </summary>
		/// <value>
		///     The delegate mutex.
		/// </value>
		private static Mutex DelegateMutex
		{
			get
			{
				if ( _delegateMutex == null )
				{
					lock ( SyncRoot )
					{
						if ( _delegateMutex == null )
						{
							bool createdNew;

							var everyoneSid = new SecurityIdentifier( WellKnownSidType.WorldSid, null );

							var mutexSecurity = new MutexSecurity( );
							mutexSecurity.AddAccessRule( new MutexAccessRule( everyoneSid, MutexRights.FullControl, AccessControlType.Allow ) );

							_delegateMutex = new Mutex( false, DelegateMutexName, out createdNew, mutexSecurity );
						}
					}
				}

				return _delegateMutex;
			}
		}

		/// <summary>
		///     Gets the delegates.
		/// </summary>
		/// <value>
		///     The delegates.
		/// </value>
		private static BidirectionalDictionary<Delegate, short> Delegates
		{
			get
			{
				if ( _delegates == null )
				{
					lock ( SyncRoot )
					{
						if ( _delegates == null )
						{
							byte[ ] serializedObject = ReadFromView( DelegateMemoryMappedFileView, DelegateMapName );

							BidirectionalDictionary<Delegate, short> map;

							if ( serializedObject == null || serializedObject.Length <= 0 )
							{
								map = new BidirectionalDictionary<Delegate, short>( );

								if ( DelegateMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
								{
									try
									{
										WriteToView( DelegateMemoryMappedFileView, DelegateMapName, BinaryEncode( map ) );
									}
									finally
									{
										DelegateMutex.ReleaseMutex( );
									}
								}
							}
							else
							{
								map = BinaryDecode( serializedObject ) as BidirectionalDictionary<Delegate, short>;
							}

							_delegates = map;
						}
					}
				}

				return _delegates;
			}
			set
			{
				_delegates = value;
			}
		}

		/// <summary>
		///     Gets the formatter.
		/// </summary>
		/// <value>
		///     The formatter.
		/// </value>
		private static BinaryFormatter Formatter
		{
			get
			{
				if ( _formatter == null )
				{
					lock ( SyncRoot )
					{
						if ( _formatter == null )
						{
							_formatter = new BinaryFormatter( );
						}
					}
				}

				return _formatter;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this application domain is currently registered.
		/// </summary>
		/// <value>
		///     <c>true</c> if this application domain is registered; otherwise, <c>false</c>.
		/// </value>
		private static bool IsRegistered
		{
			get
			{
				lock ( SyncRoot )
				{
					return _isRegistered;
				}
			}
			set
			{
				lock ( SyncRoot )
				{
					_isRegistered = value;
				}
			}
		}

		/// <summary>
		///     Gets the local event.
		/// </summary>
		/// <value>
		///     The local event.
		/// </value>
		private static EventWaitHandle LocalEvent
		{
			get
			{
				if ( _localEvent == null )
				{
					lock ( SyncRoot )
					{
						if ( _localEvent == null )
						{
							var everyoneSid = new SecurityIdentifier( WellKnownSidType.WorldSid, null );

							var eventSecurity = new EventWaitHandleSecurity( );
							eventSecurity.AddAccessRule( new EventWaitHandleAccessRule( everyoneSid, EventWaitHandleRights.FullControl, AccessControlType.Allow ) );

							bool createdNew;

							string eventName = GetEventName( AppDomainId );

							_localEvent = new EventWaitHandle( false, EventResetMode.ManualReset, eventName, out createdNew, eventSecurity );

							Trace.TraceIpcCreateEvent( eventName, AppDomainId );
						}
					}
				}

				return _localEvent;
			}
		}

		/// <summary>
		///     Gets the local memory mapped file.
		/// </summary>
		/// <value>
		///     The local memory mapped file.
		/// </value>
		private static MemoryMappedFile LocalMemoryMappedFile
		{
			get
			{
				if ( _localMemoryMappedFile == null )
				{
					lock ( SyncRoot )
					{
						if ( _localMemoryMappedFile == null )
						{
							string name = GetGlobalName( AppDomainId );

							_localMemoryMappedFile = OpenMemoryMappedFile( name, LocalMemoryMappedFileSize, true );

							Trace.TraceIpcCreateMemoryMappedFile( name, AppDomainId );
						}
					}
				}

				return _localMemoryMappedFile;
			}
		}

		/// <summary>
		///     Gets the local memory mapped file view.
		/// </summary>
		/// <value>
		///     The local memory mapped file view.
		/// </value>
		private static MemoryMappedViewAccessor LocalMemoryMappedFileView
		{
			get
			{
				if ( _localMemoryMappedFileView == null )
				{
					lock ( SyncRoot )
					{
						if ( _localMemoryMappedFileView == null )
						{
							MemoryMappedViewAccessor viewAccessor;

							MemoryMappedFile localMap = LocalMemoryMappedFile;

							try
							{
								viewAccessor = OpenView( localMap, 0, LocalMemoryMappedFileSize, AppDomainId );
							}
							catch ( Exception ex )
							{
								EventLog.Application.WriteWarning( "Failed to open the local memory mapped file '{0}'.\r\n{1}", AppDomainId, ex );

								viewAccessor = null;
							}

							_localMemoryMappedFileView = viewAccessor;
						}
					}
				}

				return _localMemoryMappedFileView;
			}
		}

		/// <summary>
		///     Gets the delimiters.
		/// </summary>
		/// <value>
		///     The delimiters.
		/// </value>
		private static char RegisteredAppDomainDelimiter
		{
			get
			{
				return RegisteredAppDomainDelimiterMember;
			}
		}

		/// <summary>
		///     Gets the delimiters.
		/// </summary>
		/// <value>
		///     The delimiters.
		/// </value>
		private static char[ ] RegisteredAppDomainDelimiters
		{
			get
			{
				return _delimiters ?? ( _delimiters = new[ ]
				{
					RegisteredAppDomainDelimiter
				} );
			}
		}

		/// <summary>
		///     Gets the registered application domains.
		/// </summary>
		/// <value>
		///     The registered application domains.
		/// </value>
		private static HashSet<string> RegisteredAppDomains
		{
			get
			{
				var registeredDomainNames = new HashSet<string>( );

				try
				{
					byte[ ] buffer = ReadFromView( RegistrationMemoryMappedFileView, RegistrationMapName );

					if ( buffer != null && buffer.Length > 0 )
					{
						string registeredAppDomains = Utf8Decode( buffer );

						if ( !string.IsNullOrEmpty( registeredAppDomains ) )
						{
							string[ ] domainNames = registeredAppDomains.Split( RegisteredAppDomainDelimiters, StringSplitOptions.RemoveEmptyEntries );

							/////
							// Append each of the domains to the set.
							/////
							foreach ( string domainName in domainNames )
							{
								registeredDomainNames.Add( domainName );
							}
						}
					}
				}
				catch ( Exception ex )
				{
					EventLog.Application.WriteWarning( "Failed to retrieve the list of registered application domains.\r\n{0}", ex );
				}

				return registeredDomainNames;
			}
		}

		/// <summary>
		///     Gets the registration memory mapped file.
		/// </summary>
		/// <value>
		///     The registration memory mapped file.
		/// </value>
		private static MemoryMappedFile RegistrationMemoryMappedFile
		{
			get
			{
				if ( _registrationMemoryMappedFile == null )
				{
					if ( !RegistrationMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
					{
						return null;
					}

					try
					{
						if ( _registrationMemoryMappedFile == null )
						{
							_registrationMemoryMappedFile = OpenMemoryMappedFile( RegistrationMapName, RegistrationMemoryMappedFileSize, true );
						}
					}
					finally
					{
						RegistrationMutex.ReleaseMutex( );
					}
				}

				return _registrationMemoryMappedFile;
			}
		}

		/// <summary>
		///     Gets the registration memory mapped file view.
		/// </summary>
		/// <value>
		///     The registration memory mapped file view.
		/// </value>
		private static MemoryMappedViewAccessor RegistrationMemoryMappedFileView
		{
			get
			{
				if ( _registrationMemoryMappedFileView == null )
				{
					lock ( SyncRoot )
					{
						if ( _registrationMemoryMappedFileView == null )
						{
							MemoryMappedViewAccessor viewAccessor;

							MemoryMappedFile registrationMap = RegistrationMemoryMappedFile;

							try
							{
								viewAccessor = OpenView( registrationMap, 0, RegistrationMemoryMappedFileSize, RegistrationMapName );
							}
							catch ( Exception ex )
							{
								EventLog.Application.WriteWarning( "Failed to open the registration memory mapped file '{0}'.\r\n{1}", RegistrationMapName, ex );

								viewAccessor = null;
							}

							_registrationMemoryMappedFileView = viewAccessor;
						}
					}
				}

				return _registrationMemoryMappedFileView;
			}
		}

		/// <summary>
		///     Gets the registration mutex.
		/// </summary>
		/// <value>
		///     The registration mutex.
		/// </value>
		private static Mutex RegistrationMutex
		{
			get
			{
				if ( _registrationMutex == null )
				{
					lock ( SyncRoot )
					{
						if ( _registrationMutex == null )
						{
							bool createdNew;

							var everyoneSid = new SecurityIdentifier( WellKnownSidType.WorldSid, null );

							var mutexSecurity = new MutexSecurity( );
							mutexSecurity.AddAccessRule( new MutexAccessRule( everyoneSid, MutexRights.FullControl, AccessControlType.Allow ) );

							_registrationMutex = new Mutex( false, RegistrationMutexName, out createdNew, mutexSecurity );
						}
					}
				}

				return _registrationMutex;
			}
		}

		/// <summary>
		///     Gets the send event.
		/// </summary>
		/// <value>
		///     The send event.
		/// </value>
		private static AutoResetEvent SendEvent
		{
			get
			{
				if ( _sendEvent == null )
				{
					lock ( SyncRoot )
					{
						if ( _sendEvent == null )
						{
							_sendEvent = new AutoResetEvent( false );
						}
					}
				}

				return _sendEvent;
			}
		}

		/// <summary>
		///     Gets the send queue.
		/// </summary>
		/// <value>
		///     The send queue.
		/// </value>
		private static Queue<ActionContainer> SendQueue
		{
			get
			{
				if ( _sendQueue == null )
				{
					lock ( SyncRoot )
					{
						if ( _sendQueue == null )
						{
							_sendQueue = new Queue<ActionContainer>( );
						}
					}
				}

				return _sendQueue;
			}
		}

		/// <summary>
		///     Gets the subscriber memory maps.
		/// </summary>
		/// <value>
		///     The subscriber memory maps.
		/// </value>
		private static Dictionary<string, Subscriber> Subscribers
		{
			get
			{
				if ( _subscribers == null )
				{
					lock ( SyncRoot )
					{
						if ( _subscribers == null )
						{
							var map = new Dictionary<string, Subscriber>( );

							HashSet<string> registeredAppDomains = RegisteredAppDomains;

                            var now = DateTime.Now;

							foreach ( string registeredAppDomain in registeredAppDomains )
							{
								/////
								// Ignore the current application domain.
								/////
								if ( registeredAppDomain == AppDomainId )
								{
									continue;
								}

                                var subscriber = new Subscriber( registeredAppDomain );
                                subscriber.LastSuccessfulSend = now;

                                map[registeredAppDomain] = subscriber;
							}

							_subscribers = map;
						}
					}
				}

				return _subscribers;
			}
		}

		/// <summary>
		///     Gets or sets the trace.
		/// </summary>
		/// <value>
		///     The trace.
		/// </value>
		private static PlatformTrace Trace
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the encoder.
		/// </summary>
		/// <value>
		///     The encoder.
		/// </value>
		private static UTF8Encoding Utf8Encoder
		{
			get
			{
				if ( _encoding == null )
				{
					lock ( SyncRoot )
					{
						if ( _encoding == null )
						{
							_encoding = new UTF8Encoding( );
						}
					}
				}

				return _encoding;
			}
		}

		/// <summary>
		///     Bulks the operation active.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static bool BulkOperationActive( string key )
		{
			return BulkOperation.IsActive( key );
		}

		/// <summary>
		///     Begins the bulk operation.
		/// </summary>
		/// <returns></returns>
		public static BulkOperation BeginBulkOperation( string key, Action<object> bulkAction )
		{
			return BulkOperation.Register( key, bulkAction );
		}

		/// <summary>
		///     Adds the bulk operation argument.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public static void AddBulkOperationArgument( string key, object value )
		{
			BulkOperation.AddArgument( key, value );
		}

		/// <summary>
		///     Broadcasts the specified action.
		/// </summary>
		/// <param name="action">The action.</param>
		public static void Broadcast( Action action )
		{
			if ( Suppression.IsActive( action ) )
			{
				return;
			}

			EnsureAppDomainIsRegistered( );

			Trace.TraceIpcBroadcast( AppDomainId );

			lock ( SendQueue )
			{
				SendQueue.Enqueue( new ActionContainer( action ) );
			}

			SendEvent.Set( );
		}

		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="payload">The payload.</param>
		public static void Broadcast( Action<object> action, object payload )
		{
			Broadcast( action, payload, EncodingMethod.Binary );
		}

		/// <summary>
		///     Broadcasts the specified action.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="payload">The payload.</param>
		/// <param name="method">The method.</param>
		public static void Broadcast( Action<object> action, object payload, EncodingMethod method )
		{
			if ( Suppression.IsActive( action ) )
			{
				return;
			}

			EnsureAppDomainIsRegistered( );

			Trace.TraceIpcBroadcast( AppDomainId );

			lock ( SendQueue )
			{
				SendQueue.Enqueue( new ActionContainer( action, payload, method ) );
			}

			SendEvent.Set( );
		}

		/// <summary>
		///     Flushes this instance.
		/// </summary>
		/// <param name="waitForSubscribers">
		///     if set to <c>true</c> waits for subscribers to complete.
		/// </param>
		public static void Flush( bool waitForSubscribers = false )
		{
			DateTime now = DateTime.UtcNow;

			List<KeyValuePair<string, Subscriber>> nonResponsiveSubscribers = null;

			/////
			// Get the current list of subscribers.
			/////
			KeyValuePair<string, Subscriber>[ ] subscribers = Subscribers.ToArray( );

			foreach ( var subscriber in subscribers )
			{
				lock ( subscriber.Value.SubscriberSyncRoot )
				{
					/////
					// Determine if there are any messages pending.
					/////
					if ( subscriber.Value.Buffer.Count > 0 )
					{
						if ( subscriber.Value.Event.WaitOne( 0 ) )
						{
							/////
							// Subscriber is busy.
							/////
							if ( now - subscriber.Value.LastSuccessfulSend > TimeSpan.FromSeconds( 10 ) )
							{
								if ( nonResponsiveSubscribers == null )
								{
									nonResponsiveSubscribers = new List<KeyValuePair<string, Subscriber>>( );
								}

								nonResponsiveSubscribers.Add( subscriber );
							}
						}
						else
						{
							/////
							// Subscriber is responsive.
							/////
							subscriber.Value.LastSuccessfulSend = now;

							if ( subscriber.Value.Buffer.Count > 0 )
							{
								var jaggedArray = new byte[subscriber.Value.Buffer.Count + 1][ ];

								int payloadSize = 0;
								int index = 0;

								/////
								// Construct the message.
								/////
								while ( subscriber.Value.Buffer.Count > 0 && ( payloadSize < LocalMemoryMappedFileSize - 5 * 1024 ) )
								{
									byte[ ] queuedPayload = subscriber.Value.Buffer.Dequeue( );
									jaggedArray[ index++ ] = queuedPayload;
									payloadSize += queuedPayload.Length;
								}

								/////
								// Write the message.
								/////
								WriteToView( subscriber.Value.View, subscriber.Key, BinaryEncode( jaggedArray ) );

								/////
								// Pulse the subscriber.
								/////
								subscriber.Value.Event.Set( );
							}
						}
					}
				}
			}

			/////
			// Remove non-responsive subscribers.
			/////
			if ( nonResponsiveSubscribers != null )
			{
				foreach ( var nonResponsiveSubscriber in nonResponsiveSubscribers )
				{
					Trace.TraceIpcUnregister( nonResponsiveSubscriber.Key, AppDomainId );

					UnregisterAppDomain( nonResponsiveSubscriber.Key );
				}
			}

			if ( waitForSubscribers )
			{
				/////
				// Wait for a short period for the subscribers to complete.
				/////
				foreach ( var subscriber in subscribers )
				{
					/////
					// Ignore non-responsive subscribers.
					/////
					if ( nonResponsiveSubscribers != null && nonResponsiveSubscribers.Contains( subscriber ) )
					{
						continue;
					}

					TimeSpan timeout = TimeSpan.FromSeconds( 5 );
					DateTime timeoutStart = DateTime.UtcNow;

					while ( DateTime.UtcNow - timeoutStart < timeout )
					{
						if ( subscriber.Value.Event.WaitOne( 0 ) )
						{
							Thread.Sleep( 100 );
						}
						else
						{
							break;
						}
					}
				}
			}
		}

		/// <summary>
		///     Creates the local memory mapped file.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">Unable to read from the local memory mapped file.</exception>
		public static void Register( )
		{
			/////
			// This is here to ensure that the memory mapped file is created prior to the application domain being registered with the registrar.
			/////
			MemoryMappedViewAccessor accessor = LocalMemoryMappedFileView;

			if ( !accessor.CanRead )
			{
				throw new InvalidOperationException( "Unable to read from the local memory mapped file." );
			}

			if ( !accessor.CanWrite )
			{
				throw new InvalidOperationException( "Unable to write to the local memory mapped file." );
			}

			/////
			// This is here to ensure that the event wait handle is created prior to the application domain being registered with the registrar.
			/////
			EventWaitHandle local = LocalEvent;

			if ( local.SafeWaitHandle.IsInvalid )
			{
				throw new InvalidOperationException( "Unable to create the local event wait handle." );
			}

			RegisterCurrentAppDomain( );
		}

		/// <summary>
		///     Suppresses the broadcasts.
		/// </summary>
		/// <returns></returns>
		public static IDisposable SuppressBroadcasts( )
		{
			return new Suppression( );
		}

		/// <summary>
		///     Closes the local memory mapped file.
		/// </summary>
		public static void Unregister( )
		{
			lock ( SyncRoot )
			{
				/////
				// Unregister the current application domain.
				/////
				UnregisterCurrentAppDomain( );

				/////
				// Close the view.
				/////
				if ( _localMemoryMappedFileView != null )
				{
					_localMemoryMappedFileView.Dispose( );
					_localMemoryMappedFileView = null;
				}

				/////
				// Close the file.
				/////
				if ( _localMemoryMappedFile != null )
				{
					_localMemoryMappedFile.Dispose( );
					_localMemoryMappedFile = null;
				}
			}
		}

		/// <summary>
		///     Binaries the decode.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>
		///     The decoded object.
		/// </returns>
		private static object BinaryDecode( byte[ ] data )
		{
			if ( data == null )
			{
				return null;
			}

			using ( var ms = new MemoryStream( data ) )
			{
				return BinaryDecode( ms );
			}
		}

		/// <summary>
		///     Decodes the object on the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns>
		///     The decoded object.
		/// </returns>
		private static object BinaryDecode( Stream stream )
		{
			if ( stream == null )
			{
				return null;
			}

			return Formatter.Deserialize( stream );
		}

		/// <summary>
		///     Encodes the specified object.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>
		///     The encoded byte stream.
		/// </returns>
		private static byte[ ] BinaryEncode( object data )
		{
			if ( data == null )
			{
				return null;
			}

			using ( var ms = new MemoryStream( ) )
			{
				BinaryEncode( data, ms );

				return ms.ToArray( );
			}
		}

		/// <summary>
		///     Encodes the specified object to the specified stream.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="stream">The stream.</param>
		private static void BinaryEncode( object data, Stream stream )
		{
			if ( data == null )
			{
				return;
			}

			Formatter.Serialize( stream, data );
		}

		/// <summary>
		///     Ensures the application domain is registered.
		/// </summary>
		private static void EnsureAppDomainIsRegistered( )
		{
			if ( !IsRegistered )
			{
				Register( );
			}
		}

		/// <summary>
		///     Gets the delegate by id.
		/// </summary>
		/// <param name="delegateId">The delegate id.</param>
		/// <returns>
		///     The delegate that corresponds to the specified id.
		/// </returns>
		private static Delegate GetDelegateById( short delegateId )
		{
			Delegate del;

			if ( !Delegates.TryGetByValue( delegateId, out del ) )
			{
				if ( DelegateMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
				{
					try
					{
						if ( !Delegates.TryGetByValue( delegateId, out del ) )
						{
							byte[ ] serializedObject = ReadFromView( DelegateMemoryMappedFileView, DelegateMapName );

							BidirectionalDictionary<Delegate, short> map;

							if ( serializedObject == null || serializedObject.Length <= 0 )
							{
								map = new BidirectionalDictionary<Delegate, short>( );
							}
							else
							{
								map = BinaryDecode( serializedObject ) as BidirectionalDictionary<Delegate, short>;
							}

							if ( map != null )
							{
								map.TryGetByValue( delegateId, out del );

								Delegates = map;
							}
						}
					}
					finally
					{
						DelegateMutex.ReleaseMutex( );
					}
				}
			}

			return del;
		}

		/// <summary>
		///     Gets the delegate id.
		/// </summary>
		/// <param name="del">The delegate.</param>
		/// <returns>
		///     The id of the specified delegate.
		/// </returns>
		private static short GetDelegateId( Delegate del )
		{
			short id;

			if ( !Delegates.TryGetValue( del, out id ) )
			{
				if ( DelegateMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
				{
					try
					{
						if ( !Delegates.TryGetValue( del, out id ) )
						{
							byte[ ] delegateDictionary = ReadFromView( DelegateMemoryMappedFileView, DelegateMapName );

							BidirectionalDictionary<Delegate, short> map;

							if ( delegateDictionary != null )
							{
								map = BinaryDecode( delegateDictionary ) as BidirectionalDictionary<Delegate, short>;
							}
							else
							{
								map = new BidirectionalDictionary<Delegate, short>( );
							}

							if ( map != null && !map.TryGetValue( del, out id ) )
							{
								if ( map.Count > 0 )
								{
									id = map.Values.Max( );
									id++;
								}

								map[ del ] = id;

								WriteToView( DelegateMemoryMappedFileView, DelegateMapName, BinaryEncode( map ) );
							}

							Delegates = map;
						}
					}
					finally
					{
						DelegateMutex.ReleaseMutex( );
					}
				}
			}

			return id;
		}

		/// <summary>
		///     Gets the name of the event.
		/// </summary>
		/// <param name="appDomain">The application domain.</param>
		/// <returns>
		///     The event name.
		/// </returns>
		private static string GetEventName( string appDomain )
		{
			return string.Format( @"{0}_Event", GetGlobalName( appDomain ) );
		}

		/// <summary>
		///     Gets the name of the global.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>
		///     The global version of the specified name.
		/// </returns>
		private static string GetGlobalName( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				return name;
			}

			if ( name.ToLower( ).StartsWith( @"global\" ) )
			{
				return name;
			}

			return string.Format( @"Global\{0}", name );
		}

		/// <summary>
		///     Opens the memory mapped file.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="size">The size.</param>
		/// <param name="createIfNotExists">
		///     if set to <c>true</c> [create if not exists].
		/// </param>
		/// <returns>
		///     The memory mapped file.
		/// </returns>
		private static MemoryMappedFile OpenMemoryMappedFile( string filename, int size, bool createIfNotExists = false )
		{
			if ( createIfNotExists )
			{
				var everyoneSid = new SecurityIdentifier( WellKnownSidType.WorldSid, null );

				var security = new MemoryMappedFileSecurity( );
				security.AddAccessRule( new AccessRule<MemoryMappedFileRights>( everyoneSid, MemoryMappedFileRights.FullControl, AccessControlType.Allow ) );

				return MemoryMappedFile.CreateOrOpen( filename, size, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, security, HandleInheritability.None );
			}

			return MemoryMappedFile.OpenExisting( filename, MemoryMappedFileRights.ReadWrite, HandleInheritability.None );
		}

		/// <summary>
		///     Opens the view.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="size">The size.</param>
		/// <param name="mapName">Name of the map.</param>
		/// <returns>
		///     The memory mapped file view accessor.
		/// </returns>
		private static MemoryMappedViewAccessor OpenView( MemoryMappedFile file, int offset, int size, string mapName )
		{
			MemoryMappedViewAccessor accessor = null;

			try
			{
				accessor = file.CreateViewAccessor( offset, size );
			}
			catch ( Exception ex )
			{
				/////
				// If there is an exception opening the view, unregister it.
				/////
				UnregisterAppDomain( mapName );

				EventLog.Application.WriteWarning( "Failed to open memory mapped file view.\r\n{0}", ex );
			}

			return accessor;
		}

		/// <summary>
		///     Reads from view.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="mapName">Name of the map.</param>
		/// <returns>
		///     The bytes read from the view accessor.
		/// </returns>
		private static byte[ ] ReadFromView( MemoryMappedViewAccessor view, string mapName )
		{
			byte[ ] buffer = null;

			try
			{
				int capacity = view.ReadInt32( 0 );

				if ( capacity > 0 )
				{
					buffer = new byte[capacity];

					view.ReadArray( Marshal.SizeOf( typeof ( int ) ), buffer, 0, capacity );
				}
			}
			catch ( Exception ex )
			{
				/////
				// If there is an exception reading from the view, unregister it.
				/////
				UnregisterAppDomain( mapName );

				EventLog.Application.WriteWarning( "Failed to read from memory mapped file.\r\n{0}", ex );
			}

			return buffer;
		}

		/// <summary>
		///     Receives this instance.
		/// </summary>
		private static void Receive( )
		{
			Trace.TraceIpcReceive( AppDomainId );

			/////
			// Receive handle was signaled.
			/////
			byte[ ] data = ReadFromView( LocalMemoryMappedFileView, AppDomainId );

			LocalEvent.Reset( );

			var jaggedArray = BinaryDecode( data ) as byte[ ][ ];

			if ( jaggedArray != null )
			{
				foreach ( var element in jaggedArray )
				{
					if ( element == null )
					{
						continue;
					}

					short delegateId;
					object payload = null;

					using ( var ms = new MemoryStream( element ) )
					{
						delegateId = BitConverter.ToInt16( element, 0 );
						ms.Position += 2;

						if ( ms.Position < ms.Length )
						{
							var method = ( EncodingMethod ) BitConverter.ToInt16( element, 2 );
							ms.Position += 2;

							payload = method == EncodingMethod.Binary ? BinaryDecode( ms ) : Utf8Decode( ms );
						}
					}

					if ( delegateId >= 0 )
					{
						Delegate del = GetDelegateById( delegateId );

						if ( del != null )
						{
							var argumentDelegate = del as Action<object>;

							if ( argumentDelegate != null )
							{
								using ( new Suppression( del ) )
								{
									argumentDelegate( payload );
								}
							}
							else
							{
								var nonArgumentDelegate = del as Action;

								if ( nonArgumentDelegate != null )
								{
									using ( new Suppression( del ) )
									{
										nonArgumentDelegate( );
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Refreshes the subscribers.
		/// </summary>
		private static void RefreshSubscribers( )
		{
			lock ( SyncRoot )
			{
				_subscribers = null;
				_lastSubscriberRefresh = DateTime.UtcNow;
			}
		}

		/// <summary>
		///     Registers the current application domain.
		/// </summary>
		private static void RegisterCurrentAppDomain( )
		{
			lock ( SyncRoot )
			{
				if ( RegistrationMemoryMappedFileView == null )
				{
					return;
				}

				if ( !RegistrationMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
				{
					return;
				}

				try
				{
					/////
					// Get the currently registered application domains.
					/////
					HashSet<string> currentlyRegisteredDomainNames = RegisteredAppDomains;

					if ( currentlyRegisteredDomainNames.Contains( AppDomainId ) )
					{
						/////
						// The current appDomain is already registered.
						/////
						return;
					}

					if ( !RegistrationMemoryMappedFileView.CanWrite )
					{
						return;
					}

					currentlyRegisteredDomainNames.Add( AppDomainId );

					string delimiteredDomainNames = string.Join( RegisteredAppDomainDelimiter.ToString( CultureInfo.InvariantCulture ), currentlyRegisteredDomainNames.ToArray( ) );

					byte[ ] bytes = Utf8Encode( delimiteredDomainNames );

					WriteToView( RegistrationMemoryMappedFileView, RegistrationMapName, bytes );

					Trace.TraceIpcRegister( AppDomainId );
				}
				finally
				{
					RegistrationMutex.ReleaseMutex( );
				}

				/////
				// Current domain is now registered.
				/////
				IsRegistered = true;

				Broadcast( RefreshSubscribers );
			}
		}

		/// <summary>
		///     Sends this instance.
		/// </summary>
		private static void Send( )
		{
			/////
			// Send handle was signaled.
			/////
			ActionContainer payload = null;

			lock ( SendQueue )
			{
				if ( SendQueue.Count > 0 )
				{
					payload = SendQueue.Dequeue( );
				}
			}

			while ( payload != null )
			{
				/////
				// Get the delegate id.
				/////
				short delegateId = GetDelegateId( payload.Action );

				byte[ ] encodedPayload;

				using ( var ms = new MemoryStream( ) )
				{
					ms.Write( BitConverter.GetBytes( delegateId ), 0, Marshal.SizeOf( typeof ( short ) ) );

					if ( payload.Payload != null )
					{
						ms.Write( BitConverter.GetBytes( ( short ) payload.EncodingMethod ), 0, Marshal.SizeOf( typeof ( short ) ) );

						if ( payload.EncodingMethod == EncodingMethod.Binary )
						{
							BinaryEncode( payload.Payload, ms );
						}
						else
						{
							byte[ ] textBuffer = Utf8Encode( payload.Payload.ToString( ) );

							ms.Write( textBuffer, 0, textBuffer.Length );
						}
					}

					encodedPayload = ms.ToArray( );
				}

				List<KeyValuePair<string, Subscriber>> nonResponsiveSubscribers = null;

				DateTime now = DateTime.UtcNow;

				if ( now - _lastSubscriberRefresh > TimeSpan.FromSeconds( 10 ) )
				{
					RefreshSubscribers( );
				}

				/////
				// Considered using Parallel.ForEach here but it slowed things down.
				/////
				foreach ( var subscriber in Subscribers.ToArray( ) )
				{
					Trace.TraceIpcSend( AppDomainId, subscriber.Key );

					if ( subscriber.Value.Event != null )
					{
						if ( subscriber.Value.Event.WaitOne( 0 ) )
						{
                            if ( now - subscriber.Value.LastSuccessfulSend > TimeSpan.FromMinutes(1))
							{
								if ( nonResponsiveSubscribers == null )
								{
									nonResponsiveSubscribers = new List<KeyValuePair<string, Subscriber>>( );
								}

								nonResponsiveSubscribers.Add( subscriber );
							}
							else
							{
								lock ( subscriber.Value.SubscriberSyncRoot )
								{
									/////
									// Buffer the payload.
									/////
									subscriber.Value.Buffer.Enqueue( encodedPayload );
								}
							}
						}
						else
						{
							subscriber.Value.LastSuccessfulSend = now;

							byte[ ][ ] jaggedArray;

							lock ( subscriber.Value.SubscriberSyncRoot )
							{
								if ( subscriber.Value.Buffer.Count > 0 )
								{
									jaggedArray = new byte[subscriber.Value.Buffer.Count + 1][ ];

									int payloadSize = 0;
									int index = 0;

									while ( subscriber.Value.Buffer.Count > 0 && ( payloadSize < LocalMemoryMappedFileSize - 5 * 1024 ) )
									{
										byte[ ] queuedPayload = subscriber.Value.Buffer.Dequeue( );
										jaggedArray[ index++ ] = queuedPayload;
										payloadSize += queuedPayload.Length;
									}
								}
								else
								{
									jaggedArray = new byte[1][ ];
								}
							}

							jaggedArray[ jaggedArray.Length - 1 ] = encodedPayload;

							WriteToView( subscriber.Value.View, subscriber.Key, BinaryEncode( jaggedArray ) );

							subscriber.Value.Event.Set( );
						}
					}
				}

				if ( nonResponsiveSubscribers != null )
				{
					foreach ( var nonResponsiveSubscriber in nonResponsiveSubscribers )
					{
						Trace.TraceIpcUnregister( nonResponsiveSubscriber.Key, AppDomainId );

						UnregisterAppDomain( nonResponsiveSubscriber.Key );
					}
				}

				payload = null;

				lock ( SendQueue )
				{
					if ( SendQueue.Count > 0 )
					{
						payload = SendQueue.Dequeue( );
					}
				}
			}
		}

		/// <summary>
		///     Sends this instance.
		/// </summary>
		private static void SendReceive( )
		{
			int failures = 0;

			while ( true )
			{
				try
				{
					var handles = new WaitHandle[ ]
					{
						SendEvent, LocalEvent
					};

					while ( true )
					{
						int signalledHandle = WaitHandle.WaitAny( handles, TimeSpan.FromSeconds( 1 ) );

						if ( signalledHandle == 0 )
						{
							/////
							// Send.
							/////
							Send( );
						}
						else if ( signalledHandle == 1 )
						{
							/////
							// Receive.
							/////
							Receive( );
						}
						else if ( signalledHandle == WaitHandle.WaitTimeout )
						{
							/////
							// Timeout occurred.
							/////
							RegisterCurrentAppDomain( );

							/////
							// Flush any receivers that have buffered data.
							/////
							Flush( );
						}
					}
				}
				catch ( ThreadAbortException )
				{
					//EventLog.Application.WriteInformation( "Interprocess communications Send/Receive thread is being aborted.\r\n{0}", ex );

					throw;
				}
				catch ( Exception ex )
				{
					/////
					// Handle exceptions and continue.
					/////
					EventLog.Application.WriteWarning( "Failed to send/receive payload between application boundaries.\r\n{0}", ex );
				}

				failures++;

				if ( failures > 5 )
				{
					break;
				}
			}
		}

		/// <summary>
		///     Unregisters the application domain.
		/// </summary>
		/// <param name="appDomain">The application domain.</param>
		private static void UnregisterAppDomain( string appDomain )
		{
			lock ( SyncRoot )
			{
                if (_registrationMemoryMappedFileView == null)
				{
					return;
				}

				lock ( SyncRoot )
				{
					Subscriber subscriber;

					if ( Subscribers.TryGetValue( appDomain, out subscriber ) )
					{
						Subscribers.Remove( appDomain );

						subscriber.Dispose( );
					}
				}

				if ( !RegistrationMutex.WaitOne( TimeSpan.FromMilliseconds( MutexTimeout ) ) )
				{
					return;
				}

				try
				{
					/////
					// Get the currently registered application domains.
					/////
					HashSet<string> currentlyRegisteredDomainNames = RegisteredAppDomains;

					if ( !currentlyRegisteredDomainNames.Contains( appDomain ) )
					{
						/////
						// The current appDomain is already registered.
						/////
						return;
					}

					if ( !RegistrationMemoryMappedFileView.CanWrite )
					{
						return;
					}

					currentlyRegisteredDomainNames.Remove( appDomain );

					string delimiteredDomainNames = string.Join( RegisteredAppDomainDelimiter.ToString( CultureInfo.InvariantCulture ), currentlyRegisteredDomainNames.ToArray( ) );

					byte[ ] bytes = Utf8Encode( delimiteredDomainNames );

					WriteToView( RegistrationMemoryMappedFileView, RegistrationMapName, bytes );
				}
				finally
				{
					RegistrationMutex.ReleaseMutex( );
				}
			}
		}

		/// <summary>
		///     Unregisters the current application domain.
		/// </summary>
		private static void UnregisterCurrentAppDomain( )
		{
			UnregisterAppDomain( AppDomainId );

			/////
			// Domain is no longer registered.
			/////
			IsRegistered = false;
		}

		/// <summary>
		///     Decodes the specified data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>
		///     The UTF8 decoded string.
		/// </returns>
		private static string Utf8Decode( byte[ ] data )
		{
			if ( data == null )
			{
				return null;
			}

			return Utf8Encoder.GetString( data );
		}

		/// <summary>
		///     UTs the f8 decode.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns>
		///     The UTF8 decoded string.
		/// </returns>
		private static string Utf8Decode( Stream stream )
		{
			if ( stream == null )
			{
				return null;
			}

			var buffer = new byte[stream.Length - stream.Position];

			stream.Read( buffer, 0, ( int ) stream.Length - ( int ) stream.Position );

			return Utf8Encoder.GetString( buffer );
		}

		/// <summary>
		///     Encodes the specified data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>
		///     The UTF8 encoded string.
		/// </returns>
		private static byte[ ] Utf8Encode( string data )
		{
			if ( data == null )
			{
				return null;
			}

			return Utf8Encoder.GetBytes( data );
		}

		/// <summary>
		///     Writes to view.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="mapName">Name of the map.</param>
		/// <param name="data">The data.</param>
		private static void WriteToView( MemoryMappedViewAccessor view, string mapName, byte[ ] data )
		{
			try
			{
				view.Write( 0, data.Length );
				view.WriteArray( Marshal.SizeOf( typeof ( int ) ), data, 0, data.Length );
			}
			catch ( Exception ex )
			{
				/////
				// If there are any exceptions writing to the view, unregister it.
				/////
				UnregisterAppDomain( mapName );

				EventLog.Application.WriteWarning( "Failed to write to memory mapped file.\r\n{0}", ex );
			}
		}
	}
}