// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Interprocess communications.
	/// </summary>
	public static partial class InterprocessCommunications
	{
		/// <summary>
		///     Represents an IPC bulk operation.
		/// </summary>
		[Serializable]
		public class BulkOperation : IDisposable
		{
			/// <summary>
			///     The call context key
			/// </summary>
			private const string CallContextKey = "InterprocessCommunicationBulkOperation";

			/// <summary>
			///     Whether this instance is disposed or not.
			/// </summary>
			private bool _disposed;

			/// <summary>
			///     Initializes a new instance of the <see cref="BulkOperation" /> class.
			/// </summary>
			/// <param name="key">The key.</param>
			/// <param name="bulkAction">The bulk action.</param>
			private BulkOperation( string key, Action<object> bulkAction )
			{
				BulkAction = bulkAction;
				Arguments = new List<object>( );
				Key = key;
			}

			/// <summary>
			///     Gets or sets the reference count.
			/// </summary>
			/// <value>
			///     The reference count.
			/// </value>
			private int RefCount
			{
				get;
				set;
			}

			/// <summary>
			///     Gets the bulk action.
			/// </summary>
			/// <value>
			///     The bulk action.
			/// </value>
			public Action<object> BulkAction
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets or sets the key.
			/// </summary>
			/// <value>
			///     The key.
			/// </value>
			private string Key
			{
				get;
				set;
			}

			/// <summary>
			///     Gets the arguments.
			/// </summary>
			/// <value>
			///     The arguments.
			/// </value>
			public List<object> Arguments
			{
				get;
				private set;
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
			///     Initializes a new instance of the <see cref="BulkOperation" /> class.
			/// </summary>
			/// <param name="key">The key.</param>
			/// <param name="bulkAction">The bulk action.</param>
			public static BulkOperation Register( string key, Action<object> bulkAction )
			{
				var data = CallContext.LogicalGetData( CallContextKey ) as Dictionary<string, BulkOperation>;

				if ( data == null )
				{
					data = new Dictionary<string, BulkOperation>( );

					CallContext.LogicalSetData( CallContextKey, data );
				}

				BulkOperation bulkOperation;

				if ( !data.TryGetValue( key, out bulkOperation ) )
				{
					bulkOperation = new BulkOperation( key, bulkAction );
					data[ key ] = bulkOperation;
				}

				bulkOperation.RefCount++;

				return bulkOperation;
			}

			/// <summary>
			///     Determines whether this instance is active.
			/// </summary>
			/// <param name="key">The key.</param>
			/// <returns></returns>
			public static bool IsActive( string key )
			{
				var data = CallContext.LogicalGetData( CallContextKey ) as Dictionary<string, BulkOperation>;

				return data != null && data.ContainsKey( key );
			}

			/// <summary>
			///     Adds the argument.
			/// </summary>
			/// <param name="key">The key.</param>
			/// <param name="argument">The argument.</param>
			/// <exception cref="System.InvalidOperationException">Invalid Interprocess Communications Bulk Operation stack.</exception>
			public static void AddArgument( string key, object argument )
			{
				var data = CallContext.LogicalGetData( CallContextKey ) as Dictionary<string, BulkOperation>;

				if ( data == null || data.Keys.Count <= 0 )
				{
					throw new InvalidOperationException( "Invalid Interprocess Communications Bulk Operation stack." );
				}

				BulkOperation bulkOperation;

				if ( data.TryGetValue( key, out bulkOperation ) )
				{
					bulkOperation.Arguments.Add( argument );
				}
			}

			/// <summary>
			///     Releases unmanaged and - optionally - managed resources.
			/// </summary>
			/// <param name="disposing">
			///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
			///     unmanaged resources.
			/// </param>
			protected virtual void Dispose( bool disposing )
			{
				if ( _disposed )
				{
					return;
				}

				if ( disposing )
				{
					var data = CallContext.LogicalGetData( CallContextKey ) as Dictionary<string, BulkOperation>;

					if ( data == null || data.Keys.Count <= 0 || !data.ContainsKey( Key ) )
					{
						throw new InvalidOperationException( "Invalid Interprocess Communications Bulk Operation structure." );
					}

					RefCount--;

					if ( RefCount <= 0 )
					{
						data.Remove( Key );

						if ( data.Keys.Count <= 0 )
						{
							CallContext.FreeNamedDataSlot( CallContextKey );
						}
					}

					if ( BulkAction != null && Arguments.Count > 0 )
					{
						Broadcast( BulkAction, Arguments );
					}
				}

				_disposed = true;
			}
		}
	}
}