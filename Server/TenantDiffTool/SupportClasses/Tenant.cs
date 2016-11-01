// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.SupportClasses
{
	/// <summary>
	///     Tenant class.
	/// </summary>
	public class Tenant : ViewModelBase, ISource, IEntityPropertySource
	{
		/// <summary>
		///     Id
		/// </summary>
		private long _id;

		/// <summary>
		///     Name
		/// </summary>
		private string _name;

		/// <summary>
		///     Initializes a new instance of the <see cref="Tenant" /> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="name">The name.</param>
		/// <param name="context">The context.</param>
		public Tenant( long id, string name, DatabaseContext context )
		{
			Id = id;
			Name = name;
			Context = context;
		}

		/// <summary>
		///     Gets or sets the id.
		/// </summary>
		/// <value>
		///     The id.
		/// </value>
		public long Id
		{
			get
			{
				return _id;
			}
			set
			{
				if ( _id != value )
				{
					_id = value;
					RaisePropertyChanged( "Id" );
				}
			}
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if ( _name != value )
				{
					_name = value;
					RaisePropertyChanged( "Name" );
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
			using ( IDbCommand command = Context.CreateCommand( ) )
			{
				command.CommandText = $"SELECT Id FROM Entity WHERE UpgradeId = '{( Guid ) state[ "entityUpgradeId" ]}' AND TenantId = {Id}";

				object entityIdObject = command.ExecuteScalar( );
				long entityId = -1;

				if ( entityIdObject != null )
				{
					entityId = ( long ) entityIdObject;
					state[ "entityId" ] = entityId;
				}

				/////
				// Id.
				/////
				var baseInfo = new EntityFieldInfo( "base0", "Entity", "Id", "The Entity Id." );
				baseInfo.SetValue( this, entityId.ToString( CultureInfo.InvariantCulture ) );
				props.Add( new FieldPropertyDescriptor( baseInfo ) );

				/////
				// Upgrade Id.
				/////
				baseInfo = new EntityFieldInfo( "base1", "Entity", "UpgradeId", "The Entity upgrade Id." );
				baseInfo.SetValue( this, ( ( Guid ) state[ "entityUpgradeId" ] ).ToString( "B" ) );
				props.Add( new FieldPropertyDescriptor( baseInfo ) );
			}
		}

		/// <summary>
		///     Gets the field properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var entityId = ( long ) state[ "entityId" ];

			using ( IDbCommand command = Context.CreateCommand( ) )
			{
				/////
				// Get all the field information in a single request.
				/////
				command.CommandText = string.Format( SqlQueries.GetTenantEntityViewerFields, entityId.ToString( CultureInfo.InvariantCulture ), Id.ToString( CultureInfo.InvariantCulture ) );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int fieldCounter = 0;

					while ( reader.Read( ) )
					{
						string value = reader.GetString( 0 );

						if ( !string.IsNullOrEmpty( value ) )
						{
							value = value.Replace( "\r\n", " " ).Replace( "\n", " " ).Replace( "\t", " " );

							while ( value.IndexOf( "  ", StringComparison.Ordinal ) >= 0 )
							{
								value = value.Replace( "  ", " " );
							}

							value = value.Trim( );
						}

						var field = new EntityFieldInfo( $"FLD{fieldCounter++}", "Fields", value, $"The '{value}' field value." );
						field.SetValue( this, reader.IsDBNull( 1 ) ? "<null>" : reader.GetString( 1 ) );
						props.Add( new FieldPropertyDescriptor( field ) );
					}
				}
			}
		}

		/// <summary>
		///     Gets the relationship properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var entityId = ( long ) state[ "entityId" ];

			using ( IDbCommand command = Context.CreateCommand( ) )
			{
				Context.AddParameter( command, "@tenantId", DbType.Int64, Id );
				Context.AddParameter( command, "@entityId", DbType.Int64, entityId );

				/////
				// Get the relationships that this entity is involved in.
				/////
				command.CommandText = SqlQueries.GetTenantEntityViewerRelationships;

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
			sb.AppendLine( SqlQueries.GetNames );
			sb.AppendLine( "SELECT * FROM (" );

			bool firstDataType = true;

			foreach ( string type in Diff.Data.DataTypes )
			{
				if ( !firstDataType )
				{
					sb.AppendLine( );
					sb.AppendLine( );
					sb.AppendLine( "UNION ALL" );
					sb.AppendLine( );
				}
				else
				{
					firstDataType = false;
				}

				sb.AppendFormat( SqlQueries.GetTenantData, $"Data_{type}", type, Id.ToString( CultureInfo.InvariantCulture ), type == "Alias" ? "d.Namespace, d.AliasMarkerId" : "NULL, NULL" );
			}

			sb.AppendLine( );
			sb.AppendLine( " ) x ORDER BY CAST(x.EntityUpgradeId AS NVARCHAR(100)), CAST(x.FieldUpgradeId AS NVARCHAR(100))" );

			using ( IDbCommand command = Context.CreateCommand( sb.ToString( ) ) )
			{
				Context.AddParameter( command, "@tenantId", DbType.Int64, Id );

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

						if ( type == "Alias" )
						{
							string nameSpace = reader.GetString( 6 );
							string aliasMarkerId = reader.GetInt32( 7 ).ToString( CultureInfo.InvariantCulture );
							value = nameSpace + ":" + value + ":" + aliasMarkerId;
						}

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
		/// <exception cref="System.NotImplementedException"></exception>
		public IList<Entity> GetEntities( bool excludeRelationshipInstances )
		{
			var data = new List<Entity>( );

			string query = string.Format( SqlQueries.GetTenantEntities, excludeRelationshipInstances ? "LEFT JOIN Relationship r ON e.Id = r.EntityId" : string.Empty, excludeRelationshipInstances ? "AND r.EntityId IS NULL" : string.Empty );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				Context.AddParameter( command, "@tenantId", DbType.Int64, Id );

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
		/// <exception cref="System.NotImplementedException"></exception>
		public IList<Relationship> GetRelationships( bool excludeRelationshipInstances )
		{
			var data = new List<Relationship>( );

			string query = string.Format( SqlQueries.GetTenantRelationships, excludeRelationshipInstances ? "LEFT JOIN Relationship r2 ON r.FromId = r2.EntityId" : string.Empty, excludeRelationshipInstances ? "AND r2.EntityId IS NULL" : string.Empty );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				Context.AddParameter( command, "@tenantId", DbType.Int64, Id );

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
				return $"tenant://{Context.Server}/{Name}";
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

		/// <summary>
		///     Gets the apps.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TenantApp> GetApps( )
		{
			var apps = new List<TenantApp>( );

			using ( IDbCommand command = Context.CreateCommand( SqlQueries.GetTenantApplications ) )
			{
				Context.AddParameter( command, "@tenantName", DbType.String, Name );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						string name = reader.GetString( 0, null );
						string description = reader.GetString( 1, null );
						string version = reader.GetString( 2, null );
						Int64 id = reader.GetInt64( 3 );

						apps.Add( new TenantApp( name, description, version, id, this, Context ) );
					}
				}
			}

			return apps;
		}
	}
}