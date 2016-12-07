// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace ReadiNow.TenantHealth.Test.Infrastructure
{
	/// <summary>
	///     Memory Guard class.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class MemoryGuard : IDisposable
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MemoryGuard" /> class.
		/// </summary>
		/// <param name="threshold">The memory threshold in bytes.</param>
		/// <param name="action">The action to perform if memory exceeds the specified threshold.</param>
		/// <param name="collectGarbage">if set to <c>true</c> [collect garbage].</param>
		/// <exception cref="System.ArgumentException">Invalid memory threshold.</exception>
		/// <exception cref="System.ArgumentNullException">action</exception>
		public MemoryGuard( long threshold, Action action, bool collectGarbage = true )
		{
			if ( threshold <= 0 )
			{
				throw new ArgumentException( "Invalid memory threshold." );
			}

			if ( action == null )
			{
				throw new ArgumentNullException( nameof( action ) );
			}

			Threshold = threshold;
			Action = action;
			CollectGarbage = collectGarbage;
		}

		/// <summary>
		///     Gets or sets the action.
		/// </summary>
		/// <value>
		///     The action.
		/// </value>
		private Action Action
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [collect garbage].
		/// </summary>
		/// <value>
		///     <c>true</c> if [collect garbage]; otherwise, <c>false</c>.
		/// </value>
		private bool CollectGarbage
		{
			get;
		}

		/// <summary>
		///     Gets or sets the threshold.
		/// </summary>
		/// <value>
		///     The threshold.
		/// </value>
		private long Threshold
		{
			get;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			long completePrivateBytes = Process.GetCurrentProcess( ).PrivateMemorySize64;

			if ( completePrivateBytes > Threshold )
			{
				Trace.WriteLine( $"Private bytes exceeded {TenantHealthHelpers.ToPrettySize( Threshold, 2 )}. Running cleanup action..." );

				Action.Invoke( );

				if ( CollectGarbage )
				{
					GC.Collect( );
				}
			}

			Action = null;
		}
	}
}