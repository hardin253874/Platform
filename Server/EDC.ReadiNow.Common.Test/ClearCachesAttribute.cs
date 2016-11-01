// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Clear Caches attribute.
	/// </summary>
	public class ClearCachesAttribute : ReadiNowTestAttribute
	{
		/// <summary>
		///     Caches that can be cleared
		/// </summary>
		[Flags]
		public enum Caches
		{
			/// <summary>
			///     The entity cache
			/// </summary>
			EntityCache,

			/// <summary>
			///     The entity field cache
			/// </summary>
			EntityFieldCache,

			/// <summary>
			///     The entity relationship cache
			/// </summary>
			EntityRelationshipCache,

			/// <summary>
			/// The bulk result cache
			/// </summary>
			BulkResultCache
		}

		/// <summary>
		///     When to clear the caches.
		/// </summary>
		public enum Clear
		{
			/// <summary>
			///     Clear the caches before the test(s)
			/// </summary>
			BeforeTest,

			/// <summary>
			///     Clear the caches after the test(s)
			/// </summary>
			AfterTest
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ClearCachesAttribute" /> class.
		/// </summary>
		/// <param name="caches">The caches.</param>
		public ClearCachesAttribute( Caches caches ) : this( caches, Clear.BeforeTest )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ClearCachesAttribute" /> class.
		/// </summary>
		/// <param name="caches">The caches.</param>
		/// <param name="clear">The clear.</param>
		public ClearCachesAttribute( Caches caches, Clear clear )
		{
			Targets = ActionTargets.Test;

			CachesToClear = caches;
			WhenToClear = clear;
		}

		/// <summary>
		///     Gets or sets the caches.
		/// </summary>
		/// <value>
		///     The caches.
		/// </value>
		private Caches CachesToClear
		{
			get;
		}

		/// <summary>
		///     Gets or sets the when to clear.
		/// </summary>
		/// <value>
		///     The when to clear.
		/// </value>
		private Clear WhenToClear
		{
			get;
		}

		/// <summary>
		///     Executed after each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that has just been run.</param>
		public override void AfterTest( TestDetails testDetails )
		{
			if ( WhenToClear == Clear.AfterTest )
			{
				ClearCaches( );
			}
		}

		/// <summary>
		///     Executed before each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that is going to be run.</param>
		public override void BeforeTest( TestDetails testDetails )
		{
			if ( WhenToClear == Clear.BeforeTest )
			{
				ClearCaches( );
			}
		}

		/// <summary>
		///     Provides the target for the action attribute
		/// </summary>
		public override ActionTargets Targets
		{
			get;
		}

		/// <summary>
		///     Clears the caches.
		/// </summary>
		private void ClearCaches( )
		{
			if ( ( CachesToClear & Caches.EntityCache ) == Caches.EntityCache )
			{
				EntityCache.Instance.Clear( );
			}

			if ( ( CachesToClear & Caches.EntityCache ) == Caches.EntityFieldCache )
			{
				EntityFieldCache.Instance.Clear( );
			}

			if ( ( CachesToClear & Caches.EntityCache ) == Caches.EntityRelationshipCache )
			{
				EntityRelationshipCache.Instance.Clear( );
			}

			if ( ( CachesToClear & Caches.BulkResultCache ) == Caches.BulkResultCache )
			{
                using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
                {
                    BulkResultCache.Clear( );
                }
			}
		}
	}
}