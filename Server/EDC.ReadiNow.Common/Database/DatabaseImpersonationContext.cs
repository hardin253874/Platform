// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net;
using EDC.Security;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Database impersonation context.
	/// </summary>
	public class DatabaseImpersonationContext : IDisposable
	{
		/// <summary>
		///     The impersonation context
		/// </summary>
		private readonly ImpersonationContext _impersonationContext;

		/// <summary>
		///     Whether this instance is disposed of or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseImpersonationContext" /> class.
		/// </summary>
		public DatabaseImpersonationContext( )
		{
			NetworkCredential credential;

			using ( var context = DatabaseContext.GetContext( ) )
			{
				credential = context.DatabaseInfo.Credentials;
			}

			if ( credential != null )
			{
				_impersonationContext = ImpersonationContext.GetContext( credential );
			}
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

			if ( _impersonationContext != null )
			{
				_impersonationContext.Dispose( );
			}

			_disposed = true;
		}
	}
}