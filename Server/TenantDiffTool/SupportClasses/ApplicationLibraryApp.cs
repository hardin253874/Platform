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
	///     Application Library App
	/// </summary>
	public class ApplicationLibraryApp : ViewModelBase, ISource, IEntityPropertySource
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ApplicationLibraryApp" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="version">The version.</param>
		/// <param name="id">The id.</param>
		/// <param name="context">The context.</param>
		public ApplicationLibraryApp( string name, string description, string version, Guid id, DatabaseContext context )
		{
			Name = name;
			Description = description;
			Version = version;
			Id = id;
			Context = context;

			/////
			// Refresh the source string.
			/////
			SourceString = string.Empty;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		[Browsable( false )]
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
		[Browsable( false )]
		public Guid Id
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
		[Browsable( false )]
		public string Name
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
		[Browsable( false )]
		public string Version
		{
			get;
			private set;
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

		public void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var upgradeId = ( Guid ) state[ "entityUpgradeId" ];

			var sb = new StringBuilder( );
			sb.AppendLine( "CREATE TABLE #Names ( EntityUid UNIQUEIDENTIFIER PRIMARY KEY, Data NVARCHAR( MAX ) )" );
			sb.AppendLine( );
			sb.AppendLine( @"
INSERT INTO #Names
SELECT
	n.EntityUid, n.Data
FROM
	AppData_NVarChar n
WHERE
	n.FieldUid = 'f8def406-90a1-4580-94f4-1b08beac87af'
	AND
		n.AppVerUid = @appVerId" );

			sb.AppendLine( "SELECT x.Type, x.EntityUpgradeId, x.FieldUpgradeId, x.Data, e.Data, f.Data, x.Namespace, x.Additional FROM (" );

			bool firstDataType = true;

			foreach ( string type in Diff.Data.DataTypes )
			{
				if ( !firstDataType )
				{
					sb.AppendLine( );
					sb.AppendLine( "UNION ALL" );
				}
				else
				{
					firstDataType = false;
				}

				sb.AppendFormat( SqlQueries.GetAppLibEntityViewerData, $"AppData_{type}", type, type == "Alias" ? "d.Namespace, d.AliasMarkerId" : "NULL, NULL" );
			}

			sb.AppendLine( );
			sb.AppendLine( @"
) x
LEFT JOIN
	#Names e ON x.EntityUpgradeId = e.EntityUid
LEFT JOIN
	#Names f ON x.FieldUpgradeId = f.EntityUid
ORDER BY CAST(x.EntityUpgradeId AS NVARCHAR(100)), CAST(x.FieldUpgradeId AS NVARCHAR(100))

DROP TABLE #Names" );

			using ( IDbCommand command = Context.CreateCommand( sb.ToString( ) ) )
			{
				Context.AddParameter( command, "@appVerId", DbType.Guid, Id );
				Context.AddParameter( command, "@entityUid", DbType.Guid, upgradeId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int fieldCounter = 0;

					while ( reader.Read( ) )
					{
						string type = reader.GetString( 0, null );

						Guid fieldUpgradeId = reader.GetGuid( 2 );
						object value = reader.GetValue( 3 );
						string fieldName = reader.GetString( 5, null );

						ReferToUpgradeIdLookup( ref fieldName, fieldUpgradeId );

						if ( type == "Alias" )
						{
							string nameSpace = reader.GetString( 6 );
							string aliasMarkerId = reader.GetInt32( 7 ).ToString( CultureInfo.InvariantCulture );
							value = nameSpace + ":" + value + ":" + aliasMarkerId;
						}

						var field = new EntityFieldInfo( $"FLD{fieldCounter++}", "Fields", fieldName ?? fieldUpgradeId.ToString( "B" ), $"The '{fieldName ?? fieldUpgradeId.ToString( "B" )}' field value." );
						field.SetValue( this, value );
						props.Add( new FieldPropertyDescriptor( field ) );
					}
				}
			}
		}

		/// <summary>
		///     Gets the entity relationship properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var upgradeId = ( Guid ) state[ "entityUpgradeId" ];

			using ( IDbCommand command = Context.CreateCommand( SqlQueries.GetAppLibraryEntityViewerRelationships ) )
			{
				Context.AddParameter( command, "@appVerId", DbType.Guid, Id );
				Context.AddParameter( command, "@entityUid", DbType.Guid, upgradeId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int relationshipCounter = 0;

					while ( reader.Read( ) )
					{
						string name = reader.GetString( 0, string.Empty );
						Guid id = reader.GetGuid( 3, Guid.Empty );
						string val = reader.GetString( 1, string.Empty );
						Guid valId = reader.GetGuid( 4, Guid.Empty );

						ReferToUpgradeIdLookup( ref name, id );
						ReferToUpgradeIdLookup( ref val, valId );

						var field = new EntityFieldInfo( $"RLN{relationshipCounter++.ToString( "0000" )}", reader.GetString( 2 ) + " relationships", name, $"The '{name}' field value." );
						field.SetValue( this, val );
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
		[Browsable( false )]
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
			sb.AppendLine( "CREATE TABLE #Names ( EntityUid UNIQUEIDENTIFIER PRIMARY KEY, Data NVARCHAR( MAX ) COLLATE SQL_Latin1_General_CP1_CS_AS )" );
			sb.AppendLine( );
			sb.AppendLine( @"
INSERT INTO #Names
SELECT
	n.EntityUid, n.Data
FROM
	AppData_NVarChar n
WHERE
	n.FieldUid = 'f8def406-90a1-4580-94f4-1b08beac87af'
	AND
		n.AppVerUid = @appVerId" );

			sb.AppendLine( "SELECT x.Type, x.EntityUpgradeId, x.FieldUpgradeId, x.Data, e.Data, f.Data, x.Namespace, x.Additional FROM (" );

			bool firstDataType = true;

			foreach ( string type in Diff.Data.DataTypes )
			{
				if ( !firstDataType )
				{
					sb.AppendLine( );
					sb.AppendLine( "UNION ALL" );
				}
				else
				{
					firstDataType = false;
				}

				sb.AppendFormat( SqlQueries.GetAppLibData, $"AppData_{type}", type, type == "Alias" ? "d.Namespace, d.AliasMarkerId" : "NULL, NULL" );
			}

			sb.AppendLine( );
			sb.AppendLine( @"
) x
LEFT JOIN
	#Names e ON x.EntityUpgradeId = e.EntityUid
LEFT JOIN
	#Names f ON x.FieldUpgradeId = f.EntityUid
ORDER BY CAST(x.EntityUpgradeId AS NVARCHAR(100)), CAST(x.FieldUpgradeId AS NVARCHAR(100))

DROP TABLE #Names" );

			using ( IDbCommand command = Context.CreateCommand( sb.ToString( ) ) )
			{
				Context.AddParameter( command, "@appVerId", DbType.Guid, Id );

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
		public IList<Entity> GetEntities( bool excludeRelationshipInstances )
		{
			var data = new List<Entity>( );

			string query = string.Format( SqlQueries.GetAppLibEntities, excludeRelationshipInstances ? "LEFT JOIN AppRelationship r ON e.EntityUid = r.EntityUid" : string.Empty, excludeRelationshipInstances ? "AND r.EntityUid IS NULL" : string.Empty );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				Context.AddParameter( command, "@appVerId", DbType.Guid, Id );

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

			string query = string.Format( SqlQueries.GetAppLibRelationships, excludeRelationshipInstances ? "LEFT JOIN AppRelationship r2 ON r.FromUid = r2.EntityUid" : string.Empty, excludeRelationshipInstances ? "AND r2.EntityUid IS NULL" : string.Empty );

			using ( IDbCommand command = Context.CreateCommand( query ) )
			{
				Context.AddParameter( command, "@appVerId", DbType.Guid, Id );

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
				return $"appLib://{Context.Server}/{Name + ":" + Version}";
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
		///     Gets the application library apps.
		/// </summary>
		/// <param name="ctx">The db info.</param>
		/// <returns></returns>
		public static IEnumerable<ApplicationLibraryApp> GetApplicationLibraryApps( DatabaseContext ctx )
		{
			var apps = new List<ApplicationLibraryApp>( );

			using ( IDbCommand command = ctx.CreateCommand( SqlQueries.GetApplicationLibraryApplications ) )
			{
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						string name = reader.GetString( 0, null );
						string description = reader.GetString( 1, null );
						string version = reader.GetString( 2, null );
						Guid applicationVersionId = reader.GetGuid( 3 );

						apps.Add( new ApplicationLibraryApp( name, description, version, applicationVersionId, ctx ) );
					}
				}
			}

			return apps;
		}

		/// <summary>
		///     Refers to upgrade identifier lookup.
		/// </summary>
		/// <param name="val">The value.</param>
		/// <param name="id">The identifier.</param>
		private void ReferToUpgradeIdLookup( ref string val, Guid id )
		{
			if ( string.IsNullOrEmpty( val ) )
			{
				string tempVal;
				if ( UpgradeIdCache.Instance.TryGetValue( id, out tempVal ) )
				{
					val = tempVal;
				}
			}
		}
	}
}