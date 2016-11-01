// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RedisInit
{
	/// <summary>
	///     Database connection.
	/// </summary>
	public class DatabaseConnection : IDisposable
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseConnection" /> class.
		/// </summary>
		/// <param name="initFile">The initialize file.</param>
		public DatabaseConnection( InitFile initFile )
		{
			var connectionString = new SqlConnectionStringBuilder( );

			connectionString.ApplicationName = "Redis Init";
			connectionString.InitialCatalog = initFile.DatabaseCatalog;
			connectionString.IntegratedSecurity = initFile.DatabaseIntegratedSecurity;

			if ( !initFile.DatabaseIntegratedSecurity )
			{
				connectionString.UserID = initFile.DatabaseUsername;
				connectionString.Password = initFile.DatabasePassword;
			}

			Connection = new SqlConnection( connectionString.ToString( ) );
			Connection.Open( );
		}

		/// <summary>
		///     Gets or sets the connection.
		/// </summary>
		/// <value>
		///     The connection.
		/// </value>
		private SqlConnection Connection
		{
			get;
			set;
		}

		public void Dispose( )
		{
			if ( Connection != null )
			{
				Connection.Dispose( );
			}
		}

		/// <summary>
		///     Gets all tenants.
		/// </summary>
		/// <returns></returns>
		public List<TenantInfo> GetAllTenants( )
		{
			var tenants = new List<TenantInfo>( );

			tenants.Add( new TenantInfo( 0, "Global" ) );

			using ( SqlCommand command = Connection.CreateCommand( ) )
			{
				command.CommandText = @"SELECT n.EntityId, n.Data FROM Relationship r JOIN Data_NVarChar n ON r.FromId = n.EntityId WHERE r.TenantId = 0 AND r.TypeId = dbo.fnAliasNsId( 'isOfType', 'core', 0 ) AND r.ToId = dbo.fnAliasNsId( 'tenant', 'core', 0 ) AND n.FieldId = dbo.fnAliasNsId( 'name', 'core', 0 )";
				command.CommandType = CommandType.Text;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						tenants.Add( new TenantInfo( reader.GetInt64( 0 ), reader.GetString( 1 ) ) );
					}
				}
			}

			return tenants;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <returns></returns>
		public List<EntityInfo> GetEntities( long tenantId )
		{
			var entities = new List<EntityInfo>( );

			using ( SqlCommand command = Connection.CreateCommand( ) )
			{
				command.CommandText = string.Format( @"
SELECT Type = {0}, EntityId, FieldId, [Namespace] + ':' + Data, AliasMarkerId FROM Data_Alias WHERE TenantId = @tenantId
SELECT Type = {1}, EntityId, FieldId, Data, NULL FROM Data_Bit WHERE TenantId = @tenantId
SELECT Type = {2}, EntityId, FieldId, Data, NULL FROM Data_DateTime WHERE TenantId = @tenantId
SELECT Type = {3}, EntityId, FieldId, Data, NULL FROM Data_Decimal WHERE TenantId = @tenantId
SELECT Type = {4}, EntityId, FieldId, Data, NULL FROM Data_Guid WHERE TenantId = @tenantId
SELECT Type = {5}, EntityId, FieldId, Data, NULL FROM Data_Int WHERE TenantId = @tenantId
SELECT Type = {6}, EntityId, FieldId, Data, NULL FROM Data_NVarChar WHERE TenantId = @tenantId
SELECT Type = {7}, EntityId, FieldId, Data, NULL FROM Data_Xml WHERE TenantId = @tenantId
SELECT Type = {8}, FromId, TypeId, ToId, NULL FROM Relationship WHERE TenantId = @tenantId
", ( int ) Types.Alias, ( int ) Types.Bit, ( int ) Types.DateTime, ( int ) Types.Decimal, ( int ) Types.Guid, ( int ) Types.Int, ( int ) Types.NVarChar, ( int ) Types.Xml, ( int ) Types.Relationship );
				command.CommandType = CommandType.Text;

				var tenantParameter = command.CreateParameter( );
				tenantParameter.ParameterName = "@tenantId";
				tenantParameter.DbType = DbType.Int64;
				tenantParameter.Value = tenantId;
				command.Parameters.Add( tenantParameter );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					do
					{
						while ( reader.Read( ) )
						{
							object data = reader.IsDBNull( 4 ) ? null : reader.GetValue( 4 );

							entities.Add( new EntityInfo( (Types) reader.GetInt32( 0 ), tenantId, reader.GetInt64( 1 ), reader.GetInt64( 2 ), reader.GetValue( 3 ), data ) );
						}
					}
					while ( reader.NextResult( ) );
				}
			}

			return entities;
		}

		/// <summary>
		///     Gets the tenant.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public TenantInfo GetTenant( string name )
		{
			using ( SqlCommand command = Connection.CreateCommand( ) )
			{
				command.CommandText = @"SELECT n.EntityId, n.Data FROM Relationship r JOIN Data_NVarChar n ON r.FromId = n.EntityId WHERE r.TenantId = 0 AND r.TypeId = dbo.fnAliasNsId( 'isOfType', 'core', 0 ) AND r.ToId = dbo.fnAliasNsId( 'tenant', 'core', 0 ) AND n.FieldId = dbo.fnAliasNsId( 'name', 'core', 0 ) AND n.Data = @name";
				command.CommandType = CommandType.Text;

				var nameParameter = command.CreateParameter( );
				nameParameter.ParameterName = "@name";
				nameParameter.DbType = DbType.String;
				nameParameter.Value = name;
				command.Parameters.Add( nameParameter );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
						return new TenantInfo( reader.GetInt64( 0 ), reader.GetString( 1 ) );
					}
				}
			}

			return null;
		}
	}
}