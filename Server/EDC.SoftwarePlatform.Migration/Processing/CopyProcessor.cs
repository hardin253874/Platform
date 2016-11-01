// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Worker class to perform the actual coping of data from a source to a target.
	/// </summary>
	internal class CopyProcessor
	{
		/// <summary>
		///     Processing context.
		/// </summary>
		private IProcessingContext _context;

		/// <summary>
		///     Constructor
		/// </summary>
		public CopyProcessor( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CopyProcessor" /> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="target">The target.</param>
		public CopyProcessor( IDataSource source, IDataTarget target )
			: this( source, target, null )
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="target">The target.</param>
		/// <param name="context">The context.</param>
		public CopyProcessor( IDataSource source, IDataTarget target, IProcessingContext context )
		{
			DataSource = source;
			DataTarget = target;
			Context = context;
		}

		/// <summary>
		///     Data source provider - for example, SQLite database, App Library, or Tenant, etc
		/// </summary>
		/// <value>
		///     The data source.
		/// </value>
		public IDataSource DataSource
		{
			get;
			set;
		}

		/// <summary>
		///     Data target provider - for example, SQLite database, App Library, or Tenant, etc
		/// </summary>
		/// <value>
		///     The data target.
		/// </value>
		public IDataTarget DataTarget
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		private IProcessingContext Context
		{
			get
			{
				return _context ?? ( _context = new ProcessingContext( ) );
			}
			set
			{
				_context = value;
			}
		}

		/// <summary>
		///     Migrates the data.
		/// </summary>
		public void MigrateData( )
		{
			IProcessingContext context = Context;

			/////
			// Run any setup logic.
			/////
			DataSource.Setup( context );
			DataTarget.Setup( context );

			/////
			// Migrate metadata
			/////
			context.WriteInfo( "Copying metadata..." );
			Metadata metadata = DataSource.GetMetadata( context );
			DataTarget.SetMetadata( metadata, context );

			/////
			// Migrate entities
			/////
			context.WriteInfo( "Copying Entity data..." );
			IEnumerable<EntityEntry> entities = DataSource.GetEntities( context );
			IList<EntityEntry> addedEntities = entities as IList<EntityEntry> ?? entities.ToList( );
			context.Report.AddedEntities = addedEntities;
			Context.Report.Counts.Add( new StatisticsCount( "Current Application Entities", addedEntities.Count, StatisticsCountType.CurrentApplication ) );
			DataTarget.WriteEntities( addedEntities, context );

			/////
			// Migrate relationships
			/////
			context.WriteInfo( "Copying Relationship data..." );
			IEnumerable<RelationshipEntry> relationships = DataSource.GetRelationships( context );
			IList<RelationshipEntry> relationshipEntries = relationships as IList<RelationshipEntry> ?? relationships.ToList( );
			context.Report.AddedRelationships = relationshipEntries;
			Context.Report.Counts.Add( new StatisticsCount( "Current Application Relationships", relationshipEntries.Count, StatisticsCountType.CurrentApplication ) );
			DataTarget.WriteRelationships( relationshipEntries, context );

			/////
			// Migrate field data
			/////
			foreach ( string fieldDataTable in Helpers.FieldDataTables )
			{
				context.WriteInfo( string.Format( "Copying {0} Field data...", fieldDataTable ) );
				IEnumerable<DataEntry> fieldData = DataSource.GetFieldData( fieldDataTable, context );
				IList<DataEntry> dataEntries = fieldData as IList<DataEntry> ?? fieldData.ToList( );
				if ( dataEntries.Count > 0 )
				{
					context.Report.AddedEntityData[ fieldDataTable ] = dataEntries;
				}

				Context.Report.Counts.Add( new StatisticsCount( string.Format( "Current Application {0} Data", fieldDataTable ), dataEntries.Count, StatisticsCountType.CurrentApplication ) );
				DataTarget.WriteFieldData( fieldDataTable, dataEntries, context );
			}

			context.WriteInfo( "Copying Binary File data..." );
			IEnumerable<BinaryDataEntry> binaryData = DataSource.GetBinaryData( context );
			IList<BinaryDataEntry> binaryDataEntries = binaryData as IList<BinaryDataEntry> ?? binaryData.ToList( );
			context.Report.AddedBinaryData = binaryDataEntries;
			Context.Report.Counts.Add( new StatisticsCount( "Current Application Binary Data", binaryDataEntries.Count, StatisticsCountType.CurrentApplication ) );
			DataTarget.WriteBinaryData( binaryDataEntries, context );

			context.WriteInfo( "Copying Document File data..." );
			IEnumerable<DocumentDataEntry> documentData = DataSource.GetDocumentData( context );
			IList<DocumentDataEntry> documentDataEntries = documentData as IList<DocumentDataEntry> ?? documentData.ToList( );
			context.Report.AddedDocumentData = documentDataEntries;
			Context.Report.Counts.Add( new StatisticsCount( "Current Application Document Data", documentDataEntries.Count, StatisticsCountType.CurrentApplication ) );
			DataTarget.WriteDocumentData( documentDataEntries, context );

            
            context.WriteInfo("Copying Secure data...");
            IEnumerable<SecureDataEntry> secureData = DataSource.GetSecureData(context);
            IList<SecureDataEntry> secureDataEntries = secureData as IList<SecureDataEntry> ?? secureData.ToList();
            context.Report.AddedSecureData = secureDataEntries;
            Context.Report.Counts.Add(new StatisticsCount("Current Application Secure Data", secureDataEntries.Count, StatisticsCountType.CurrentApplication));
            DataTarget.WriteSecureData(secureDataEntries, context);


            context.WriteInfo( "Copying DoNotRemove data..." );
            IList<Guid> doNotRemove = DataSource.GetDoNotRemove( context ).ToList( );
            context.Report.AddedDoNotRemoveData = doNotRemove;
            Context.Report.Counts.Add( new StatisticsCount( "Current Application DoNotRemove records", doNotRemove.Count, StatisticsCountType.CurrentApplication ) );
            DataTarget.WriteDoNotRemove( doNotRemove, context );

            /////
            // Run any teardown logic.
            /////
            DataTarget.TearDown(context);
            DataSource.TearDown( context );
		}
	}
}