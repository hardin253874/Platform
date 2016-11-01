// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Holds the entire data contents of an application in memory.
	///     This is used to perform diffs.
	/// </summary>
	internal class AppContents
	{
		/// <summary>
		///     All binary data entries.
		///     A map from the DataHash to the BinaryData.
		/// </summary>
		public Dictionary<string, BinaryDataEntry> BinaryData
		{
			get;
			set;
		}

		public Dictionary<string, DocumentDataEntry> DocumentData
		{
			get;
			set;
		}

		/// <summary>
		///     All entities.
		///     A map from the EntityId to the EntityEntry.
		/// </summary>
		public Dictionary<Guid, EntityEntry> Entities
		{
			get;
			set;
		}

		/// <summary>
		///     All field data.
		///     Dictionary of dictionaries. Outer dictionary is keyed on field type.
		///     Inner dictionary contains a map from the EntityId/FieldId tuple to the DataEntry.
		/// </summary>
		public Dictionary<string, Dictionary<Tuple<Guid, Guid>, DataEntry>> FieldData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the missing field data.
		/// </summary>
		/// <value>
		///     The missing field data.
		/// </value>
		public Dictionary<Tuple<Guid, Guid>, DataEntry> MissingFieldData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the missing relationships.
		/// </summary>
		/// <value>
		///     The missing relationships.
		/// </value>
		public IDictionary<RelationshipEntryKey, RelationshipEntry> MissingRelationships
		{
			get;
			set;
		}

		/// <summary>
		///     All relationships.
		///     A map from the Type/From/To tuple to the RelationshipEntry.
		/// </summary>
		public IDictionary<RelationshipEntryKey, RelationshipEntry> Relationships
		{
			get;
			set;
        }

        /// <summary>
        ///     Set of entity guids that an application declares should not be deleted when the app is upgraded.
        /// </summary>
        public ISet<Guid> DoNotRemove
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the application data.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="context">The context.</param>
        /// <param name="ignoreCardinalityViolations">if set to <c>true</c> [ignore cardinality violations].</param>
        /// <returns></returns>
        public static AppContents Load( IDataSource source, IProcessingContext context, bool ignoreCardinalityViolations = false )
		{
			/////
			// Load entities, relationships and field data.
			/////
			Dictionary<string, DocumentDataEntry> documentData = source.GetDocumentData( context ) != null && source.GetDocumentData( context ).Any( ) ? source.GetDocumentData( context ).ToDictionary( d => d.GetKey( ) ) : new Dictionary<string, DocumentDataEntry>( );

            var app = new AppContents
            {
                Entities = source.GetEntities( context ).ToDictionary( e => e.GetKey( ) ),
                Relationships = ProcessRelationships( source.GetRelationships( context ), context, ignoreCardinalityViolations ),
                MissingRelationships = ProcessRelationships( source.GetMissingRelationships( context ), context, true ),
                FieldData = new Dictionary<string, Dictionary<Tuple<Guid, Guid>, DataEntry>>( ),
                MissingFieldData = source.GetMissingFieldData( context ).ToDictionary( d => d.GetKey( ) ),
                BinaryData = source.GetBinaryData( context ).ToDictionary( b => b.GetKey( ) ),
                DocumentData = documentData,
                DoNotRemove = new HashSet<Guid>( source.GetDoNotRemove( context ) )
            };

			foreach ( string dataTable in Helpers.FieldDataTables )
			{
				app.FieldData[ dataTable ] = source.GetFieldData( dataTable, context ).ToDictionary( d => d.GetKey( ) );
			}

			return app;
		}

		/// <summary>
		/// Processes the relationships.
		/// </summary>
		/// <param name="relationships">The relationships.</param>
		/// <param name="context">The context.</param>
		/// <param name="ignoreCardinalityViolations">if set to <c>true</c> [ignore cardinality violations].</param>
		/// <returns></returns>
		private static IDictionary<RelationshipEntryKey, RelationshipEntry> ProcessRelationships( IEnumerable<RelationshipEntry> relationships, IProcessingContext context, bool ignoreCardinalityViolations = false )
		{
			var results = new CardinalityAwareDictionary( );

			var cardinalityViolations = new List<RelationshipEntry>( );

			foreach ( RelationshipEntry entry in relationships )
			{
				RelationshipEntryKey key = entry.GetKey( );

				RelationshipEntry existingEntry;

				if ( !results.TryGetValue( key, out existingEntry ) )
				{
					results.Add( key, entry );
				}
				else
				{
					if ( entry.Cardinality == CardinalityEnum_Enumeration.ManyToMany )
					{
						/////
						// Duplicate record.
						/////
						EventLog.Application.WriteWarning( string.Format( "Detected duplicate relationship.\n\nExisting Type: {0}\nExisting From: {1}\nExisting To: {2}\n\nDropped Type: {3}\nDropped From: {4}\nDropped To: {5}\n",
							existingEntry.TypeId.ToString( "B" ),
							existingEntry.FromId.ToString( "B" ),
							existingEntry.ToId.ToString( "B" ),
							entry.TypeId.ToString( "B" ),
							entry.FromId.ToString( "B" ),
							entry.ToId.ToString( "B" ) ) );
					}
					else
					{
						if ( !ignoreCardinalityViolations )
						{
							/////
							// Cardinality violation.
							/////
							cardinalityViolations.Add( entry );

							EventLog.Application.WriteWarning( string.Format( "Detected cardinality violation ({0}).\n\nExisting Type: {1}\nExisting From: {2}\nExisting To: {3}\n\nDropped Type: {4}\nDropped From: {5}\nDropped To: {6}\n",
								entry.Cardinality,
								existingEntry.TypeId.ToString( "B" ),
								existingEntry.FromId.ToString( "B" ),
								existingEntry.ToId.ToString( "B" ),
								entry.TypeId.ToString( "B" ),
								entry.FromId.ToString( "B" ),
								entry.ToId.ToString( "B" ) ) );
						}
					}
				}
			}

			if ( !ignoreCardinalityViolations && cardinalityViolations.Count > 0 )
			{
				context.Report.CardinalityViolations = cardinalityViolations;
			}

			return results;
		}
	}
}