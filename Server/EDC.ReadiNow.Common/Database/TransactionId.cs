// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Handles creation of transaction ids.
	/// </summary>
	public static class TransactionId
	{
		/// <summary>
		///     Creates a new transaction id.
		/// </summary>
		/// <returns>A new transaction id.</returns>
		public static string Create( )
		{
			return ( "TRAN_" + Guid.NewGuid( ).ToString( "N" ) ).Substring( 0, 32 ).ToUpperInvariant( );
		}
	}
}