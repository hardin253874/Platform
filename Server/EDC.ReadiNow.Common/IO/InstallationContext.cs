// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Installation context.
	/// </summary>
	public class InstallationContext : IDisposable
	{
		/// <summary>
		///     The reference count
		/// </summary>
		[ThreadStatic]
		private static int _refCount;

		/// <summary>
		///     Initializes a new instance of the <see cref="InstallationContext" /> class.
		/// </summary>
		public InstallationContext( )
		{
			Interlocked.Increment( ref _refCount );
		}


		/// <summary>
		///     Gets a value indicating whether this <see cref="InstallationContext" /> is active.
		/// </summary>
		/// <value>
		///     <c>true</c> if active; otherwise, <c>false</c>.
		/// </value>
		public static bool Active
		{
			get
			{
				return _refCount > 0;
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Interlocked.Decrement( ref _refCount );
		}
	}
}