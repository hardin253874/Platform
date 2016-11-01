// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Text;
using EDC.SoftwarePlatform.Migration.Processing;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.SupportClasses
{
	/// <summary>
	///     Class representing the SqliteFile type.
	/// </summary>
	/// <seealso cref="TenantDiffTool.SupportClasses.File" />
	public class SqliteFile : File
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqliteFile" /> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="context">The context.</param>
		public SqliteFile( string path, DatabaseContext context ) : base( path, context )
		{
		}

		/// <summary>
		///     The database connection.
		/// </summary>
		private SQLiteConnection Connection
		{
			get;
			set;
		}

		/// <summary>
		///     Create a new database command.
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateCommand( )
		{
			if ( Connection == null )
			{
				Connection = CreateConnection( );
			}

			return Connection.CreateCommand( );
		}

		/// <summary>
		///     Creates a new database connection to this database.
		/// </summary>
		public SQLiteConnection CreateConnection( )
		{
			string connectionString = CreateConnectionString( Path );

			var sqliteConnection = new SQLiteConnection( connectionString );
			sqliteConnection.Open( );

			using ( var cmd = new SQLiteCommand( ) )
			{
				/////
				// Set the database journaling mode to Memory to avoid SqLite Disk IO errors (well documented).
				// May also set this to OFF if the memory pressure becomes too much.
				/////
				cmd.Connection = sqliteConnection;
				const string pragma = "PRAGMA journal_mode = MEMORY";
				cmd.CommandText = pragma;
				cmd.ExecuteNonQuery( );
			}

			return sqliteConnection;
		}

		/// <summary>
		///     Clean up
		/// </summary>
		public override void Dispose( )
		{
			Connection?.Close( );
			base.Dispose( );
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <returns></returns>
		public override IList<Data> GetData( )
		{
			var data = new List<Data>( );

			var sb = new StringBuilder( );
			sb.AppendLine( "SELECT * FROM (" );

			bool firstDataType = true;

			foreach ( string type in Diff.Data.DataTypes )
			{
				if ( !firstDataType )
				{
					sb.AppendLine( "UNION ALL" );
				}
				else
				{
					firstDataType = false;
				}

				sb.AppendFormat( SqlQueries.GetFileData, $"_Data_{type}", type, type == "Alias" ? "d.Namespace, d.AliasMarkerId" : "NULL, NULL" );
			}

			sb.AppendLine( " ) x ORDER BY CAST(x.EntityUpgradeId AS NVARCHAR), CAST(x.FieldUpgradeId AS NVARCHAR)" );

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sb.ToString( );

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

						switch ( type )
						{
							case Helpers.AliasName:
								string nameSpace = reader.GetString( 6 );
								string aliasMarkerId = reader.GetInt32( 7 ).ToString( CultureInfo.InvariantCulture );
								value = nameSpace + ":" + value + ":" + aliasMarkerId;
								break;
							case Helpers.DateTimeName:

								if ( value != null && value != DBNull.Value )
								{
									DateTime dt;

									if ( value is DateTime )
									{
										value = ( ( DateTime ) value ).ToUniversalTime( ).ToString( "u" );
									}
									else if ( DateTime.TryParse( value.ToString( ), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dt ) )
									{
										value = dt.ToString( "u" );
									}
								}
								break;
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
		/// <param name="excludeRelationshipInstances">if set to <c>true</c> [exclude relationship instances].</param>
		/// <returns></returns>
		public override IList<Entity> GetEntities( bool excludeRelationshipInstances )
		{
			var data = new List<Entity>( );

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = string.Format( SqlQueries.GetFileEntities, excludeRelationshipInstances ? "LEFT JOIN _Relationship r ON e.Uid = r.EntityUid WHERE r.EntityUid IS NULL" : string.Empty );

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
		///     Gets the entity field properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public override void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var entityUid = ( Guid ) state[ "entityUpgradeId" ];

			var sb = new StringBuilder( );

			bool firstDataType = true;

			foreach ( string type in Diff.Data.DataTypes )
			{
				if ( !firstDataType )
				{
					sb.AppendLine( "UNION ALL" );
				}
				else
				{
					firstDataType = false;
				}

				sb.AppendLine( string.Format( SqlQueries.GetFileEntityViewerFields, $"_Data_{type}", entityUid.ToString( ).ToLower( ) ) );
			}

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sb.ToString( );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int fieldCounter = 0;

					while ( reader.Read( ) )
					{
						string fieldName = reader.GetString( 0, null );
						object value = reader.GetValue( 1 );
						Guid fieldUid = reader.GetGuid( 2, Guid.Empty );

						ReferToUpgradeIdLookup( ref fieldName, fieldUid );

						if ( !string.IsNullOrEmpty( fieldName ) )
						{
							fieldName = fieldName.Replace( "\r\n", " " ).Replace( "\n", " " ).Replace( "\t", " " );

							while ( fieldName.IndexOf( "  ", StringComparison.Ordinal ) >= 0 )
							{
								fieldName = fieldName.Replace( "  ", " " );
							}

							fieldName = fieldName.Trim( );
						}
						else
						{
							fieldName = fieldUid.ToString( "B" );
						}

						var field = new EntityFieldInfo( $"FLD{fieldCounter++}", "Fields", fieldName, $"The '{fieldName}' field value." );
						field.SetValue( this, value.ToString( ) );
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
		public override void GetEntityProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
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
		public override void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var entityUid = ( Guid ) state[ "entityUpgradeId" ];

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = string.Format( SqlQueries.GetFileEntityViewerRelationships, entityUid.ToString( ).ToLower( ) );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					int relationshipCounter = 0;

					while ( reader.Read( ) )
					{
						string direction = reader.GetString( 0 );
						Guid typeId = reader.GetGuid( 1, Guid.Empty );
						string typeName = reader.GetString( 2, null );
						Guid toId = reader.GetGuid( 3, Guid.Empty );
						string toName = reader.GetString( 4, null );

						ReferToUpgradeIdLookup( ref typeName, typeId );
						ReferToUpgradeIdLookup( ref toName, toId );

						var field = new EntityFieldInfo( $"RLN{relationshipCounter++.ToString( "0000" )}", direction + " relationships", typeName ?? typeId.ToString( "B" ), $"The '{typeName ?? typeId.ToString( "B" )}' field value." );
						field.SetValue( this, toName ?? toId.ToString( "B" ) );
						props.Add( new FieldPropertyDescriptor( field ) );
					}
				}
			}
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="excludeRelationshipInstances">if set to <c>true</c> [exclude relationship instances].</param>
		/// <returns></returns>
		public override IList<Relationship> GetRelationships( bool excludeRelationshipInstances )
		{
			var data = new List<Relationship>( );

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = string.Format( SqlQueries.GetFileRelationships, excludeRelationshipInstances ? "LEFT JOIN _Relationship rIn ON r.FromUid = rIn.EntityUid WHERE rIn.EntityUid IS NULL" : string.Empty );

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
		///     Returns the connection string for this provider.
		/// </summary>
		private static string CreateConnectionString( string path )
		{
			var connectionBuilder = new SQLiteConnectionStringBuilder( );

			if ( string.IsNullOrEmpty( path ) )
			{
				throw new InvalidOperationException( "Database path not specified." );
			}

			connectionBuilder.DataSource = path;

			return connectionBuilder.ConnectionString;
		}
	}
}