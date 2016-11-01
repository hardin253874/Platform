// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache.Providers;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
	/// <summary>
	///     The "source of truth" for cache invalidators.
	/// </summary>
	public class CacheInvalidatorFactory
	{
		/// <summary>
		///     The collection of cache invalidators
		/// </summary>
		private static readonly List<ICacheInvalidator> Invalidators = new List<ICacheInvalidator>( );

		/// <summary>
		///     Thread synchronization object
		/// </summary>
		private static readonly object SyncRoot = new object( );

		/// <summary>
		///     Static constructor
		/// </summary>
		static CacheInvalidatorFactory( )
		{
			/////
			// Ensure new assemblies being loaded into the app domain cause this to be refreshed.
			/////
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

			LoadInvalidators( );
		}

		/// <summary>
		///     The cache invalidators in use.
		/// </summary>
		public IEnumerable<ICacheInvalidator> CacheInvalidators
		{
			get
			{
				lock ( SyncRoot )
				{
					return Invalidators.AsReadOnly( );
				}
			}
		}

		/// <summary>
		///     Expose mutable list to support unit tests.
		///     Ideally it would be good to do away with this - it will involve moving the cache invalidator factory and entity
		///     model over to Autofac.
		/// </summary>
		internal List<ICacheInvalidator> CacheInvalidatorsList_TestOnly
		{
			get
			{
				lock ( SyncRoot )
				{
					return Invalidators;
				}
			}
		}

		/// <summary>
		///     Handles the AssemblyLoad event of the CurrentDomain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="AssemblyLoadEventArgs" /> instance containing the event data.</param>
		private static void CurrentDomain_AssemblyLoad( object sender, AssemblyLoadEventArgs args )
		{
			LoadInvalidators( );
		}

		/// <summary>
		///     Loads the invalidators.
		/// </summary>
		private static void LoadInvalidators( )
		{
			lock ( SyncRoot )
			{
				Invalidators.Clear( );

				Invalidators.AddRange( Factory.Current.Resolve<IEnumerable<ICacheService>>( ).Select( acc => acc.CacheInvalidator ) );
				Invalidators.Add( PerTenantEntityTypeCache.Instance.CacheInvalidator );
				Invalidators.Add( MetadataCacheInvalidator.Instance );

				// Add new cache invalidators to cacheInvalidators here.
			}
		}
	}
}