// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReadiNow.TenantHealth.Test.Infrastructure
{
	/// <summary>
	///     Memory Logger class.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class MemoryLogger : IDisposable
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MemoryLogger" /> class.
		/// </summary>
		/// <param name="arguments">The arguments.</param>
		public MemoryLogger( params KeyValuePair<string, object>[ ] arguments )
		{
			Arguments = arguments;
			InitialPrivateBytes = Process.GetCurrentProcess( ).PrivateMemorySize64;
		}

		/// <summary>
		///     Gets the arguments.
		/// </summary>
		/// <value>
		///     The arguments.
		/// </value>
		private KeyValuePair<string, object>[ ] Arguments
		{
			get;
		}

		/// <summary>
		///     Gets or sets the final private bytes.
		/// </summary>
		/// <value>
		///     The final private bytes.
		/// </value>
		private long FinalPrivateBytes
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the initial private bytes.
		/// </summary>
		/// <value>
		///     The initial private bytes.
		/// </value>
		private long InitialPrivateBytes
		{
			get;
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose( )
		{
			FinalPrivateBytes = Process.GetCurrentProcess( ).PrivateMemorySize64;

			string message = string.Empty;

			if ( Arguments != null && Arguments.Length > 0 )
			{
				message = string.Join( ", ", Arguments.Select( arg => $"{arg.Key}: {arg.Value}" ) );
				message += ", ";
			}

			message += $"Initial Memory: {TenantHealthHelpers.ToPrettySize( InitialPrivateBytes, 2 )}, Final Memory: {TenantHealthHelpers.ToPrettySize( FinalPrivateBytes, 2 )}, Difference: {TenantHealthHelpers.ToPrettySize( FinalPrivateBytes - InitialPrivateBytes, 2 )}";

			Trace.WriteLine( message );
		}
	}
}