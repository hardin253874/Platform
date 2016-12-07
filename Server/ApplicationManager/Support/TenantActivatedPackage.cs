// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using ApplicationManager.Core;
using EDC.Database;
using EDC.ReadiNow.Database;

namespace ApplicationManager.Support
{
	/// <summary>
	///     Tenant activations.
	/// </summary>
	public class TenantActivatedPackage
	{
		/// <summary>
		///     Gets or sets the app version id.
		/// </summary>
		/// <value>
		///     The app version id.
		/// </value>
		public Guid AppVerId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tenant id.
		/// </summary>
		/// <value>
		///     The tenant id.
		/// </value>
		public long TenantId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the tenant activated packages.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<TenantActivatedPackage> GetTenantActivatedPackages( )
		{
			var activations = new List<TenantActivatedPackage>( );

			var databaseInfo = new SqlDatabaseInfo( Config.ServerName, Config.DatabaseName, DatabaseAuthentication.Integrated, null, 60, 30, 300 );

			/////
			// Set the base properties
			/////

			using ( DatabaseContext context = DatabaseContext.GetContext( databaseInfo: databaseInfo ) )
			{
				using ( IDbCommand command = context.CreateCommand( ) )
				{
					command.CommandText = @"
SELECT
	Tenant = r.TenantId,
	PackageId = pid.Data
FROM
	Relationship r
JOIN
	Data_Alias a1 ON
		r.TypeId = a1.EntityId AND
		a1.Data = 'isOfType' AND
		a1.Namespace = 'core' AND
		r.TenantId = a1.TenantId
JOIN
	Data_Alias a2 ON
		r.ToId = a2.EntityId AND
		a2.Data = 'solution' AND
		a2.Namespace = 'core' AND
		r.TenantId = a2.TenantId
JOIN
	Data_Guid pid ON
		pid.TenantId = r.TenantId AND
		r.FromId = pid.EntityId
JOIN
	Data_Alias a3 ON
		pid.FieldId = a3.EntityId AND
		a3.Data = 'packageId' AND
		a3.Namespace = 'core' AND
		pid.TenantId = a3.TenantId
WHERE
	r.TenantId <> 0";

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							activations.Add( new TenantActivatedPackage
							{
								AppVerId = reader.GetGuid( 1 ),
								TenantId = reader.GetInt64( 0 )
							} );
						}
					}
				}
			}

			return activations;
		}
	}
}