// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
	/// <summary>
	///     Test App Domain class.
	/// </summary>
	public class TestAppDomain : IDisposable
	{
		/// <summary>
		///     The domain
		/// </summary>
		private readonly AppDomain _domain;

		/// <summary>
		///     The injected disposables
		/// </summary>
		private readonly List<IDisposable> _injectedDisposables = new List<IDisposable>( );

		/// <summary>
		///     Whether this instance has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="TestAppDomain" /> class.
		/// </summary>
		/// <param name="friendlyName">Name of the friendly.</param>
		public TestAppDomain( string friendlyName = "RedisManagerTest Domain" )
		{
			AppDomain callingDomain = Thread.GetDomain( );

			var setup = new AppDomainSetup
			{
				ApplicationBase = callingDomain.SetupInformation.ApplicationBase
			};

			AppDomain domain = AppDomain.CreateDomain( friendlyName, null, setup );

			_domain = domain;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( _disposed )
			{
				return;
			}

			if ( _injectedDisposables != null && _injectedDisposables.Count > 0 )
			{
				foreach ( IDisposable disposable in _injectedDisposables )
				{
					disposable.Dispose( );
				}

				_injectedDisposables.Clear( );
			}

			if ( _domain != null )
			{
				try
				{
					AppDomain.Unload( _domain );
				}
				catch
				{
					Trace.WriteLine( "Failed to unload app domain." );
				}
			}

			_disposed = true;
		}

		/// <summary>
		///     Injects the type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channelName">Name of the channel.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public T InjectType<T>( string channelName, string tenantName, Func<MessageEventArgs<TestMessage>, bool> filter )
		{
			return InjectType<T, TestMessage>( channelName, tenantName, filter );
		}

		/// <summary>
		/// Injects the type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <param name="channelName">Name of the channel.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public T InjectType<T, U>( string channelName, string tenantName, Func<MessageEventArgs<U>, bool> filter )
		{
			var type = typeof( T );

			var constructorArgs = new object [ ]
			{
				channelName,
				tenantName,
				filter
			};

			var instance = ( T ) _domain.CreateInstanceAndUnwrap( type.Assembly.FullName, type.FullName, false, BindingFlags.Default, null, constructorArgs, CultureInfo.CurrentCulture, null );

			var disposable = instance as IDisposable;

			if ( disposable != null )
			{
				_injectedDisposables.Add( disposable );
			}

			return instance;
		}
	}
}