// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Database;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Represents a reader for loading a specific version of an application from the application library.
	/// </summary>
	internal class LibraryAppStagingSource : LibraryAppSource
	{
		/// <summary>
		///     Load entities.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override IEnumerable<EntityEntry> GetEntities( IProcessingContext context )
		{
			/////
			// Query entities that are part of the solution
			/////
			const string sql = @"
DECLARE @name UNIQUEIDENTIFIER
DECLARE @isOfType UNIQUEIDENTIFIER

SELECT @name = EntityUid FROM AppData_Alias WHERE Data = 'name' AND Namespace = 'core'
SELECT @isOfType = EntityUid FROM AppData_Alias WHERE Data = 'isOfType' AND Namespace = 'core'

SELECT DISTINCT e.EntityUid, en.Data, etn.Data
FROM AppEntity e
LEFT JOIN AppData_NVarChar en ON e.EntityUid = en.EntityUid AND e.AppVerUid = en.AppVerUid AND en.FieldUid = @name
LEFT JOIN AppRelationship et ON e.EntityUid = et.FromUid AND e.AppVerUid = et.AppVerUid AND et.TypeUid = @isOfType
LEFT JOIN AppData_NVarChar etn ON et.ToUid = etn.EntityUid AND etn.FieldUid = @name
WHERE e.AppVerUid = @appVer";

			var map = new Dictionary<Guid, EntityStagingEntry>( );

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = sql;
				command.AddParameterWithValue( "@appVer", AppVerId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader != null )
					{
						while ( reader.Read( ) )
						{
							if ( reader.IsDBNull( 0 ) )
							{
								context.WriteWarning( "Unexpected null UpgradeId in Entity." );
								continue;
							}

							EntityStagingEntry entry;

							if ( !map.TryGetValue( reader.GetGuid( 0 ), out entry ) )
							{
								entry = new EntityStagingEntry
									{
										EntityId = reader.GetGuid( 0 ),
										EntityName = reader.IsDBNull( 1 ) ? null : reader.GetString( 1 ),
										EntityTypeName = reader.IsDBNull( 2 ) ? null : reader.GetString( 2 )
									};

								map[ entry.EntityId ] = entry;
							}
							else
							{
								if ( entry.EntityTypeName != null && !reader.IsDBNull( 2 ) )
								{
									string[] split = entry.EntityTypeName.Split( new[]
										{
											','
										} );

									string type = reader.GetString( 2 );

									if ( split.All( s => s.Trim( ).ToLowerInvariant( ) != type ) )
									{
										entry.EntityTypeName += ", " + type;
									}
								}
							}
						}
					}
				}
			}

			return map.Values;
		}
	}
}