// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Data;
using EDC.Database;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Solutions.DataUpgrade
{
	/// <summary>
	///     Handles AutoNumber upgrades from the old schema to the new.
	/// </summary>
	public static class AutoNumber
	{
		/// <summary>
		///     Upgrades this instance.
		/// </summary>
		/// <param name="solutions">The solutions.</param>
		public static void Upgrade( IEnumerable<Solution> solutions )
		{
		    long autoNumberValueId = WellKnownAliases.CurrentTenant.AutoNumberValue;

			long userId;
			RequestContext.TryGetUserId( out userId );

			/////
			// If the 'core:autoNumberValue' alias exists, convert all auto number instances
			// that are of the old type, regardless of solution that they belong to.
			/////

			using ( DatabaseContext context = DatabaseContext.GetContext( true ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "Upgrade autonumber instances" ) )
				using ( var command = context.CreateCommand( "spUpgradeAutoNumberInstance", CommandType.StoredProcedure ) )
				{
					context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					context.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					command.ExecuteNonQuery( );
				}

				using ( DatabaseContextInfo.SetContextInfo( "Upgrade autonumber start values" ) )
				using ( var command = context.CreateCommand( "spUpgradeAutoNumberStartingNumber", CommandType.StoredProcedure ) )
				{
					context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					context.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );
					var solutionIdParameter = context.AddParameter( command, "@solutionId", DbType.Int64 );

					foreach ( Solution solution in solutions )
					{
						solutionIdParameter.Value = solution.Id;

						command.ExecuteNonQuery( );
					}
				}

				context.CommitTransaction( );
			}
		}
	}
}