// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using EDC.Common;

namespace EDC.ReadiNow.Model
{
	public class EntityTypeContext : IDisposable
	{
		/// <summary>
		///     The call context key
		/// </summary>
		private const string CallContextKey = "EntityTypeContext";

		/// <summary>
		///     Whether this instance is disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityTypeContext" /> class.
		/// </summary>
		public EntityTypeContext( )
		{
			var state = CallContext.LogicalGetData( CallContextKey ) as EntityTypeDictionary;

			if ( state == null )
			{
				state = new EntityTypeDictionary( );
				CallContext.LogicalSetData( CallContextKey, state );
			}

			state.Increment( );

			State = state;
		}

		/// <summary>
		///     Gets or sets the state.
		/// </summary>
		/// <value>
		///     The state.
		/// </value>
		private EntityTypeDictionary State
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
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		/// <exception cref="System.InvalidOperationException">CallContext corrupt. Expected valid SuppressionState.</exception>
		protected virtual void Dispose( bool disposing )
		{
			if ( !_disposed )
			{
				if ( disposing )
				{
					var state = CallContext.LogicalGetData( CallContextKey ) as EntityTypeDictionary;

					if ( state == null )
					{
						throw new InvalidOperationException( "CallContext corrupt. Expected valid SuppressionState." );
					}

					state.Decrement( );

					if ( state.RefCount == 0 )
					{
						CallContext.FreeNamedDataSlot( CallContextKey );
					}
				}
			}

			_disposed = true;
		}

		/// <summary>
		///     Gets the specified entity identifier.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <returns></returns>
		public HashSet<long> Get( long entityId )
		{
			HashSet<long> typeIds;

			State.Dictionary.TryGetValue( entityId, out typeIds );

			return typeIds;
		}

		/// <summary>
		///     Merges the specified entityId.
		/// </summary>
		/// <param name="entityId">The entityId.</param>
		/// <param name="typeIds">The type ids.</param>
		public void Merge( long entityId, IEnumerable<long> typeIds )
		{
			HashSet<long> typeIdList;

			if ( !State.Dictionary.TryGetValue( entityId, out typeIdList ) )
			{
				typeIdList = new HashSet<long>( );
				State.Dictionary[ entityId ] = typeIdList;
			}

			typeIdList.AddRange( typeIds );
		}

		/// <summary>
		///     Merges the specified entityId.
		/// </summary>
		/// <param name="entityId">The entityId.</param>
		/// <param name="typeId">The type identifier.</param>
		public void Merge( long entityId, long typeId )
		{
			HashSet<long> typeIdList;

			if ( !State.Dictionary.TryGetValue( entityId, out typeIdList ) )
			{
				typeIdList = new HashSet<long>( );
				State.Dictionary[ entityId ] = typeIdList;
			}

			typeIdList.Add( typeId );
		}

		/// <summary>
		///     Reference counted Entity Type dictionary.
		/// </summary>
		public class EntityTypeDictionary
		{
			/// <summary>
			///     The reference count
			/// </summary>
			private int _refCount;

			/// <summary>
			///     Initializes a new instance of the <see cref="EntityTypeDictionary" /> class.
			/// </summary>
			public EntityTypeDictionary( )
			{
				Dictionary = new Dictionary<long, HashSet<long>>( );
			}

			/// <summary>
			///     Gets the dictionary.
			/// </summary>
			/// <value>
			///     The dictionary.
			/// </value>
			public IDictionary<long, HashSet<long>> Dictionary
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets the reference count.
			/// </summary>
			/// <value>
			///     The reference count.
			/// </value>
			public int RefCount
			{
				get
				{
					return _refCount;
				}
			}

			/// <summary>
			///     Decrements this instance.
			/// </summary>
			public void Decrement( )
			{
				Interlocked.Decrement( ref _refCount );
			}

			/// <summary>
			///     Increments this instance.
			/// </summary>
			public void Increment( )
			{
				Interlocked.Increment( ref _refCount );
			}
		}
	}
}