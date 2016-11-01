// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Test.Complete
{
	public static class TenantMergeProcessorTestHelper
	{
		/// <summary>
		///     Confirms the entity has been published.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid PackageId;packageId
		///     or
		///     @Invalid EntityUid;entityUid
		/// </exception>
		public static bool ConfirmAppLibraryEntity( DatabaseContext context, Guid packageId, Guid entityUid )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( packageId == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid PackageId", "packageId" );
			}

			if ( entityUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid EntityUid", "entityUid" );
			}

			const string query = @"SELECT Id FROM AppEntity WHERE AppVerUid = @packageId AND EntityUid = @entityUid";

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@packageId", DbType.Guid, packageId );
				context.AddParameter( command, "@entityUid", DbType.Guid, entityUid );

				object result = command.ExecuteScalar( );

				if ( result != null && result != DBNull.Value )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Confirms the application library field value.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="dataTableType">Type of the data table.</param>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		/// <param name="fieldUid">The field upgrade id.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid TenantId;tenantId
		///     or
		///     @Invalid EntityUid;entityUid
		/// </exception>
		public static bool ConfirmAppLibraryFieldValue( DatabaseContext context, DataTableType dataTableType, Guid packageId, Guid entityUid, Guid fieldUid, object value )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( packageId == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid PackageId", "packageId" );
			}

			if ( entityUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid EntityUid", "entityUid" );
			}

			string query = string.Format( "SELECT d.Data FROM AppData_{0} d WHERE d.AppVerUid = @appVerUid AND d.EntityUid = @entityUid AND d.FieldUid = @fieldUid", dataTableType );

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@appVerUid", DbType.Guid, packageId );
				context.AddParameter( command, "@entityUid", DbType.Guid, entityUid );
				context.AddParameter( command, "@fieldUid", DbType.Guid, fieldUid );

				object result = command.ExecuteScalar( );

				if ( result == null || result == DBNull.Value )
				{
					if ( value == null || value == DBNull.Value )
					{
						return true;
					}

					return false;
				}

				return result.Equals( value );
			}
		}

		/// <summary>
		///     Confirms the application library relationship.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <param name="sourceUid">The source upgrade id.</param>
		/// <param name="targetUid">The target upgrade id.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid PackageId;packageId
		///     or
		///     @Invalid typeUid;typeUid
		///     or
		///     @Invalid sourceUid;sourceUid
		///     or
		///     @Invalid targetUid;targetUid
		/// </exception>
		public static bool ConfirmAppLibraryRelationship( DatabaseContext context, Guid packageId, Direction direction, Guid typeUid, Guid sourceUid, Guid targetUid )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( packageId == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid PackageId", "packageId" );
			}

			if ( typeUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid typeUid", "typeUid" );
			}

			if ( sourceUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid sourceUid", "sourceUid" );
			}

			if ( targetUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid targetUid", "targetUid" );
			}

			string query = direction == Direction.Forward ? "SELECT r.ToUid FROM AppRelationship r WHERE r.AppVerUid = @appVerUid AND r.TypeUid = @typeUid AND r.FromUid = @sourceUid" : "SELECT r.FromUid FROM AppRelationship r WHERE r.AppVerUid = @appVerUid AND r.TypeUid = @typeUid AND r.ToUid = @sourceUid";

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@appVerUid", DbType.Guid, packageId );
				context.AddParameter( command, "@typeUid", DbType.Guid, typeUid );
				context.AddParameter( command, "@sourceUid", DbType.Guid, sourceUid );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						object result = reader.GetValue( 0 );

						if ( result == null || result == DBNull.Value )
						{
							continue;
						}

						if ( result.Equals( targetUid ) )
						{
							return true;
						}
					}

					return false;
				}
			}
		}

		/// <summary>
		///     Confirms the dropped relationship.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <param name="sourceUid">The source upgrade id.</param>
		/// <param name="targetUid">The target upgrade id.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid PackageId;packageId
		///     or
		///     @Invalid TenantId;tenantId
		///     or
		///     @Invalid typeUid;typeUid
		///     or
		///     @Invalid sourceUid;sourceUid
		///     or
		///     @Invalid targetUid;targetUid
		/// </exception>
		public static bool ConfirmDroppedRelationship( DatabaseContext context, Guid packageId, long tenantId, Direction direction, Guid typeUid, Guid sourceUid, Guid targetUid )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( packageId == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid PackageId", "packageId" );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid TenantId", "tenantId" );
			}

			if ( typeUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid typeUid", "typeUid" );
			}

			if ( sourceUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid sourceUid", "sourceUid" );
			}

			if ( targetUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid targetUid", "targetUid" );
			}

			string query = direction == Direction.Forward ? "SELECT r.ToUid FROM AppDeploy_Relationship r WHERE r.AppVerUid = @appVerUid AND r.TenantId = @tenantId AND r.TypeUid = @typeUid AND r.FromUid = @sourceUid" : "SELECT r.FromUid FROM AppDeploy_Relationship r WHERE r.AppVerUid = @appVerUid AND r.TenantId = @tenantId AND r.TypeUid = @typeUid AND r.ToUid = @sourceUid";

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@appVerUid", DbType.Guid, packageId );
				context.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
				context.AddParameter( command, "@typeUid", DbType.Guid, typeUid );
				context.AddParameter( command, "@sourceUid", DbType.Guid, sourceUid );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						object result = reader.GetValue( 0 );

						if ( result == null || result == DBNull.Value )
						{
							continue;
						}

						if ( result.Equals( targetUid ) )
						{
							return true;
						}
					}

					return false;
				}
			}
		}

		/// <summary>
		///     Confirms the entity has been deployed.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid TenantId;tenantId
		///     or
		///     @Invalid EntityUid;entityUid
		/// </exception>
		public static bool ConfirmTenantEntity( DatabaseContext context, long tenantId, Guid entityUid )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid TenantId", "tenantId" );
			}

			if ( entityUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid EntityUid", "entityUid" );
			}

			const string query = @"SELECT Id FROM Entity WHERE TenantId = @tenantId AND UpgradeId = @entityUid";

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
				context.AddParameter( command, "@entityUid", DbType.Guid, entityUid );

				object result = command.ExecuteScalar( );

				if ( result != null && result != DBNull.Value )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Confirms the tenant field value.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="dataTableType">Type of the data table.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		/// <param name="fieldUid">The field upgrade id.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid TenantId;tenantId
		///     or
		///     @Invalid EntityId;entityId
		/// </exception>
		public static bool ConfirmTenantFieldValue( DatabaseContext context, long tenantId, DataTableType dataTableType, Guid entityUid, Guid fieldUid, object value )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid TenantId", "tenantId" );
			}

			if ( entityUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid EntityUid", "entityUid" );
			}

			string query = string.Format( "SELECT d.Data FROM Data_{0} d JOIN Entity e ON d.EntityId = e.Id AND d.TenantId = e.TenantId JOIN Entity f ON d.FieldId = f.Id AND d.TenantId = f.TenantId WHERE d.TenantId = @tenantId AND e.UpgradeId = @entityUid AND f.UpgradeId = @fieldUid", dataTableType );

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
				context.AddParameter( command, "@entityUid", DbType.Guid, entityUid );
				context.AddParameter( command, "@fieldUid", DbType.Guid, fieldUid );

				object result = command.ExecuteScalar( );

				if ( result == null || result == DBNull.Value )
				{
					if ( value == null || value == DBNull.Value )
					{
						return true;
					}

					return false;
				}

				return result.Equals( value );
			}
		}

		/// <summary>
		///     Confirms the tenant relationship.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <param name="sourceUid">The source upgrade id.</param>
		/// <param name="targetUid">The target upgrade id.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">context</exception>
		/// <exception cref="System.ArgumentException">
		///     @Invalid TenantId;tenantId
		///     or
		///     @Invalid typeUid;typeUid
		///     or
		///     @Invalid sourceUid;sourceUid
		///     or
		///     @Invalid targetUid;targetUid
		/// </exception>
		public static bool ConfirmTenantRelationship( DatabaseContext context, long tenantId, Direction direction, Guid typeUid, Guid sourceUid, Guid targetUid )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid TenantId", "tenantId" );
			}

			if ( typeUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid typeUid", "typeUid" );
			}

			if ( sourceUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid sourceUid", "sourceUid" );
			}

			if ( targetUid == Guid.Empty )
			{
				throw new ArgumentException( @"Invalid targetUid", "targetUid" );
			}

			string query = direction == Direction.Forward ? "SELECT d.UpgradeId FROM Relationship r JOIN Entity t ON r.TypeId = t.Id AND r.TenantId = t.TenantId JOIN Entity s ON r.FromId = s.Id AND r.TenantId = s.TenantId JOIN Entity d ON r.ToId = d.Id AND r.TenantId = d.TenantId WHERE r.TenantId = @tenantId AND t.UpgradeId = @typeUid AND s.UpgradeId = @sourceUid" : "SELECT d.UpgradeId FROM Relationship r JOIN Entity t ON r.TypeId = t.Id AND r.TenantId = t.TenantId JOIN Entity s ON r.ToId = s.Id AND r.TenantId = s.TenantId JOIN Entity d ON r.FromId = d.Id AND r.TenantId = d.TenantId WHERE r.TenantId = @tenantId AND t.UpgradeId = @typeUid AND s.UpgradeId = @sourceUid";

			using ( IDbCommand command = context.CreateCommand( query ) )
			{
				context.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
				context.AddParameter( command, "@typeUid", DbType.Guid, typeUid );
				context.AddParameter( command, "@sourceUid", DbType.Guid, sourceUid );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						object result = reader.GetValue( 0 );

						if ( result == null || result == DBNull.Value )
						{
							continue;
						}

						if ( result.Equals( targetUid ) )
						{
							return true;
						}
					}

					return false;
				}
			}
		}
	}
}