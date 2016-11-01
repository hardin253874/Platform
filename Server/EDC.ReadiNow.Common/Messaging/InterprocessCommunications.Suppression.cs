// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Interprocess communications.
	/// </summary>
	public static partial class InterprocessCommunications
	{
		/// <summary>
		///     Suppression class.
		/// </summary>
		public sealed class Suppression : IDisposable
		{
			/// <summary>
			///     Key
			/// </summary>
			private const string SuppressionKey = "InterprocessCommunicationsSuppression";

			/// <summary>
			///     Whether this has been disposed or not.
			/// </summary>
			private bool _disposed;

			/// <summary>
			///     Initializes a new instance of the <see cref="Suppression" /> class.
			/// </summary>
			public Suppression( )
			{
				object existing = CallContext.LogicalGetData( SuppressionKey );

				Stack<Suppression> stack;

				if ( existing == null )
				{
					stack = new Stack<Suppression>( );
					CallContext.LogicalSetData( SuppressionKey, stack );
				}
				else
				{
					stack = existing as Stack<Suppression>;
				}

				if ( stack != null )
				{
					stack.Push( this );
				}
			}

			/// <summary>
			///     Initializes a new instance of the <see cref="Suppression" /> class.
			/// </summary>
			/// <param name="source">The source.</param>
			public Suppression( Delegate source )
				: this( )
			{
				SourceDelegate = source;
			}

			/// <summary>
			///     Gets or sets the source delegate.
			/// </summary>
			/// <value>
			///     The source delegate.
			/// </value>
			private Delegate SourceDelegate
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
			///     Determines whether the specified delegate is active.
			/// </summary>
			/// <param name="del">The delegate.</param>
			/// <returns>
			///     <c>true</c> if the specified delegate is active; otherwise, <c>false</c>.
			/// </returns>
			public static bool IsActive( Delegate del )
			{
				object existing = CallContext.LogicalGetData( SuppressionKey );

				if ( existing != null )
				{
					var stack = existing as Stack<Suppression>;

					if ( stack != null )
					{
						return stack.Any( s => s.SourceDelegate == del || s.SourceDelegate == null );
					}
				}

				return false;
			}

			/// <summary>
			///     Releases unmanaged and - optionally - managed resources.
			/// </summary>
			/// <param name="disposing">
			///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
			/// </param>
			/// <exception cref="System.InvalidOperationException">The suppression stack is corrupt.</exception>
			private void Dispose( bool disposing )
			{
				if ( !_disposed )
				{
					if ( disposing )
					{
						object existing = CallContext.LogicalGetData( SuppressionKey );

						if ( existing != null )
						{
							var stack = existing as Stack<Suppression>;

							if ( stack != null )
							{
								Suppression top = stack.Pop( );

								if ( top != this )
								{
									throw new InvalidOperationException( "The suppression stack is corrupt." );
								}

								if ( stack.Count == 0 )
								{
									CallContext.FreeNamedDataSlot( SuppressionKey );
								}
							}
							else
							{
								throw new InvalidOperationException( "The suppression stack is corrupt." );
							}
						}
						else
						{
							throw new InvalidOperationException( "The suppression stack is corrupt." );
						}
					}

					_disposed = true;
				}
			}

			/// <summary>
			///     Finalizes an instance of the <see cref="Subscriber" /> class.
			/// </summary>
			~Suppression( )
			{
				Dispose( false );
			}
		}
	}
}