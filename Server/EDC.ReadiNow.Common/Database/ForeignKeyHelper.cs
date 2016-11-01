// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	/// Foreign Key Helper.
	/// </summary>
	public static class ForeignKeyHelper
	{
		/// <summary>
		///     Ensure all foreign keys are trusted.
		/// </summary>
		public static void Trust( )
		{
			EventLog.Application.WriteInformation( "Enabling CHECK constraints..." );

			try
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( IDbCommand command = ctx.CreateCommand( "spTrustForeignKeys", CommandType.StoredProcedure ) )
					{
						command.ExecuteNonQuery( );
					}
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to enable CHECK constraints. {0}", exc.Message );
			}
		}
	}
}