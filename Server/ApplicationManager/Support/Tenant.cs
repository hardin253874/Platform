// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Data;
using ApplicationManager.Core;
using EDC.Database;
using EDC.ReadiNow.Database;

namespace ApplicationManager.Support
{
	/// <summary>
	///     Tenant.
	/// </summary>
	public class Tenant
	{
		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private static readonly object SyncRoot = new object( );

		/// <summary>
		///     Cached tenants.
		/// </summary>
		private static volatile List< Tenant > _tenants;

		/// <summary>
		///     Gets or sets the entity id.
		/// </summary>
		/// <value>
		///     The entity id.
		/// </value>
		public long EntityId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable< Tenant > GetTenants( )
		{
			if ( _tenants == null )
			{
				lock ( SyncRoot )
				{
					if ( _tenants == null )
					{
						var tenants = new List< Tenant >( );

						var databaseInfo = new SqlDatabaseInfo( Config.ServerName, Config.DatabaseName, DatabaseAuthentication.Integrated, null, 60, 30, 300 );

						/////
						// Set the base properties
						/////

						using ( DatabaseContext context = DatabaseContext.GetContext( databaseInfo: databaseInfo ) )
						{
							using ( IDbCommand command = context.CreateCommand( ) )
							{
								command.CommandText = @"
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', 0 )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', 0 )
DECLARE @tenant BIGINT = dbo.fnAliasNsId( 'tenant', 'core', 0 )

SELECT
	EntityId = n.EntityId, Name = n.Data
FROM
	Relationship r
JOIN
	Data_NVarChar n ON
		n.TenantId = r.TenantId AND
		r.FromId = n.EntityId AND
		n.FieldId = @name
WHERE
	r.TypeId = @isOfType AND
	r.ToId = @tenant AND
	r.TenantId = 0";

								using ( IDataReader reader = command.ExecuteReader( ) )
								{
									if ( reader != null )
									{
										while ( reader.Read( ) )
										{
											tenants.Add( new Tenant
												{
													EntityId = reader.GetInt64( 0 ),
													Name = reader.GetString( 1 )
												} );
										}
									}
								}
							}
						}

						return _tenants = tenants;
					}
				}
			}

			return _tenants;
		}
	}
}