// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.SupportClasses
{
	/// <summary>
	///     Tenant app.
	/// </summary>
	public class TenantApp : ViewModelBase, ISource, IEntityPropertySource
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantApp" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="version">The version.</param>
		/// <param name="id">The id.</param>
		/// <param name="tenant">The tenant.</param>
		/// <param name="context">The context.</param>
		public TenantApp( string name, string description, string version, long id, Tenant tenant, DatabaseContext context )
		{
			Name = name;
			Description = description;
			Version = version;
			Id = id;
			Tenant = tenant;
			Context = context;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the id.
		/// </summary>
		/// <value>
		///     The id.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant.
		/// </summary>
		/// <value>
		///     The tenant.
		/// </value>
		public Tenant Tenant
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public string Version
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entity field properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var upgradeId = ( Guid ) state[ "entityUpgradeId" ];

			var sb = new StringBuilder( );
			sb.AppendLine( string.Format( SqlQueries.GetTenantAppData, $"AND e.UpgradeId = '{upgradeId}'" ) );

			using ( IDbCommand command = Context.CreateCommand( ) )
			{
				command.CommandText = sb.ToString( );

				Context.AddParameter( command, "@tenant", DbType.Int64, Tenant.Id );
				Context.AddParameter( command, "@solutionId", DbType.Int64, Id );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int fieldCounter = 0;

					while ( reader.Read( ) )
					{
						Guid fieldUpgradeId = reader.GetGuid( 2 );
						object value = reader.GetValue( 3 );
						string fieldName = reader.GetString( 5, null );

						var field = new EntityFieldInfo( $"FLD{fieldCounter++}", "Fields", fieldName ?? fieldUpgradeId.ToString( "B" ), $"The '{fieldName ?? fieldUpgradeId.ToString( "B" )}' field value." );
						field.SetValue( this, value );
						props.Add( new FieldPropertyDescriptor( field ) );
					}
				}
			}
		}

		/// <summary>
		///     Gets the entity properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public void GetEntityProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			/////
			// Upgrade Id.
			/////
			var baseInfo = new EntityFieldInfo( "base1", "Entity", "UpgradeId", "The Entity upgrade Id." );
			baseInfo.SetValue( this, ( ( Guid ) state[ "entityUpgradeId" ] ).ToString( "B" ) );
			props.Add( new FieldPropertyDescriptor( baseInfo ) );
		}

		/// <summary>
		///     Gets the entity relationship properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var upgradeId = ( Guid ) state[ "entityUpgradeId" ];

			string query = string.Format( SqlQueries.GetTenantAppRelationshipsViewer, Id, Tenant.Id, upgradeId );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int relationshipCounter = 0;

					while ( reader.Read( ) )
					{
						string name = reader.GetString( 0 );

						var field = new EntityFieldInfo( $"RLN{relationshipCounter++.ToString( "0000" )}", reader.GetString( 2 ) + " relationships", name, $"The '{name}' field value." );
						field.SetValue( this, reader.GetString( 1 ) );
						props.Add( new FieldPropertyDescriptor( field ) );
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		public DatabaseContext Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <returns></returns>
		public IList<Data> GetData( )
		{
			var data = new List<Data>( );

			var sb = new StringBuilder( );
			sb.AppendLine( string.Format( SqlQueries.GetTenantAppData, string.Empty ) );

			using ( IDbCommand command = Context.CreateCommand( ) )
			{
				command.CommandText = sb.ToString( );

				Context.AddParameter( command, "@tenant", DbType.Int64, Tenant.Id );
				Context.AddParameter( command, "@solutionId", DbType.Int64, Id );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						string type = reader.GetString( 0, null );

						Guid entityUpgradeId = reader.GetGuid( 1 );
						Guid fieldUpgradeId = reader.GetGuid( 2 );
						object value = reader.GetValue( 3 );
						string entityName = reader.GetString( 4, null );
						string fieldName = reader.GetString( 5, null );

						data.Add( new Data
						{
							EntityUpgradeId = entityUpgradeId,
							FieldUpgradeId = fieldUpgradeId,
							Value = value,
							EntityName = entityName,
							FieldName = fieldName,
							Type = type
						} );
					}
				}
			}

			Data = data;

			return Data;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		public IList<Entity> GetEntities( bool excludeRelationshipInstances )
		{
			var data = new List<Entity>( );

			string query = string.Format( SqlQueries.GetTenantAppEntities, Id, Tenant.Id, excludeRelationshipInstances ? "LEFT JOIN ( SELECT ee.UpgradeId FROM Entity ee JOIN Relationship rr ON ee.Id = rr.EntityId ) r ON e.EntityUid = r.UpgradeId WHERE r.UpgradeId IS NULL" : string.Empty );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						Guid entityUpgradeId = reader.GetGuid( 0 );
						string entityName = reader.GetString( 1, null );

						data.Add( new Entity
						{
							EntityUpgradeId = entityUpgradeId,
							Name = entityName
						} );
					}
				}
			}

			Entities = data;

			return Entities;
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		public IList<Relationship> GetRelationships( bool excludeRelationshipInstances )
		{
			var data = new List<Relationship>( );

			string query = string.Format( SqlQueries.GetTenantAppRelationships, Id, Tenant.Id, excludeRelationshipInstances ? "LEFT JOIN ( SELECT ee.UpgradeId FROM Entity ee JOIN Relationship rr ON ee.Id = rr.EntityId ) r2 ON r.FromId = r2.UpgradeId WHERE r2.UpgradeId IS NULL" : string.Empty );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						Guid fromId = reader.GetGuid( 0, Guid.Empty );
						string fromName = reader.GetString( 1, null );
						Guid typeId = reader.GetGuid( 2, Guid.Empty );
						string typeName = reader.GetString( 3, null );
						Guid toId = reader.GetGuid( 4, Guid.Empty );
						string toName = reader.GetString( 5, null );

						data.Add( new Relationship
						{
							FromUpgradeId = fromId,
							FromName = fromName,
							TypeUpgradeId = typeId,
							TypeName = typeName,
							ToUpgradeId = toId,
							ToName = toName
						} );
					}
				}
			}

			Relationships = data;

			return Relationships;
		}

		/// <summary>
		///     Gets the source string.
		/// </summary>
		/// <returns></returns>
		public string SourceString
		{
			get
			{
				return $"tenantApp://{Context.Server}/{Tenant.Name + ":" + Name}";
			}
			set
			{
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Context != null )
			{
				Context.Dispose( );
				Context = null;
			}
		}


		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		public IList<Data> Data
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		public IList<Entity> Entities
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		public IList<Relationship> Relationships
		{
			get;
			private set;
		}
	}
}