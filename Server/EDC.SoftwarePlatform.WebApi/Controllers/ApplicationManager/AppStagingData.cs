// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EntityM = EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager
{
	/// <summary>
	///     App Staging Data
	/// </summary>
	[DataContract]
	public class AppStagingData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="AppStagingData" /> class.
		/// </summary>
		public AppStagingData( StatisticsReport report )
		{
			if ( report == null )
			{
				throw new ArgumentNullException( "report" );
			}

			var entities = new List<EntityEntry>( );

			if ( report.AddedEntities != null && report.AddedEntities.Any( ) )
			{
				entities.AddRange( report.AddedEntities );
			}

			if ( report.RemovedEntities != null && report.RemovedEntities.Any( ) )
			{
				entities.AddRange( report.RemovedEntities );
			}

			if ( report.UpdatedEntities != null && report.UpdatedEntities.Any( ) )
			{
				entities.AddRange( report.UpdatedEntities );
			}

			if ( report.UnchangedEntities != null && report.UnchangedEntities.Any( ) )
			{
				entities.AddRange( report.UnchangedEntities );
			}

			Entities = entities;

			if ( report.CardinalityViolations != null && report.CardinalityViolations.Any( ) )
			{
				CardinalityViolations = LoadViolationData( report.CardinalityViolations );
			}
		}

		/// <summary>
		///     Gets or sets the cardinality violations.
		/// </summary>
		/// <value>
		///     The cardinality violations.
		/// </value>
		[DataMember( Name = "cardinalityViolations" )]
		public IList<CardinalityViolation> CardinalityViolations
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		[DataMember( Name = "entities" )]
		public IList<EntityEntry> Entities
		{
			get;
			set;
		}

		/// <summary>
		///     Loads the violation data.
		/// </summary>
		/// <param name="violations">The violations.</param>
		/// <returns></returns>
		private static List<CardinalityViolation> LoadViolationData( IEnumerable<RelationshipEntry> violations )
		{
			var result = new List<CardinalityViolation>( );

			const string queryTemplate = @"
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )

SELECT
	UpgradeId = e.UpgradeId, Name = ISNULL( ISNULL( n.Data, a.Data ), '<unnamed>' )
FROM
	Entity e
LEFT JOIN
	Data_NVarChar n ON e.TenantId = n.TenantId AND e.Id = n.EntityId AND n.FieldId = @name
LEFT JOIN
	Data_Alias a ON e.TenantId = a.TenantId AND e.Id = a.EntityId AND a.FieldId = @alias
WHERE
	e.UpgradeId IN ({0}) AND e.TenantId = @tenantId";

			var uniqueIds = new HashSet<Guid>( );

			var relationshipEntries = violations as IList<RelationshipEntry> ?? violations.ToList( );

			foreach ( RelationshipEntry violation in relationshipEntries )
			{
				uniqueIds.Add( violation.TypeId );
				uniqueIds.Add( violation.FromId );
				uniqueIds.Add( violation.ToId );
			}

			string query = string.Format( queryTemplate, string.Join( ",", uniqueIds.Select( id => "'" + id.ToString( ) + "'" ) ) );

			var map = new Dictionary<Guid, string>( );

			using ( var context = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = context.CreateCommand( query ) )
				{
					command.CommandType = CommandType.Text;
					context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							map[ reader.GetGuid( 0 ) ] = reader.GetString( 1 );
						}
					}
				}
			}

			const string unnamed = "<unnamed>";

			result.AddRange( relationshipEntries.Select( violation =>
			{
				string type;

				if ( !map.TryGetValue( violation.TypeId, out type ) )
				{
					type = unnamed;
				}

				string from;

				if ( !map.TryGetValue( violation.FromId, out from ) )
				{
					from = unnamed;
				}

				string to;

				if ( !map.TryGetValue( violation.ToId, out to ) )
				{
					to = unnamed;
				}

				return new CardinalityViolation( type, from, to );
			} ) );

			return result;
		}
	}
}