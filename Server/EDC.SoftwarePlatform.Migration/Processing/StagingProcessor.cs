// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Staging processor.
	/// </summary>
	internal class StagingProcessor
	{
		/// <summary>
		///     Processing context.
		/// </summary>
		private IProcessingContext _context;

		/// <summary>
		///     New application.
		/// </summary>
		private AppContents _newApp;

		/// <summary>
		///     Old application.
		/// </summary>
		private AppContents _oldApp;

		/// <summary>
		///     Initializes a new instance of the <see cref="StagingProcessor" /> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public StagingProcessor( IProcessingContext context )
		{
			Context = context;
		}

		/// <summary>
		///     Gets or sets the new version.
		/// </summary>
		/// <value>
		///     The new version.
		/// </value>
		public IDataSource NewVersion
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the old version.
		/// </summary>
		/// <value>
		///     The old version.
		/// </value>
		public IDataSource OldVersion
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
		///     Stages the data.
		/// </summary>
		public void StageData( )
		{
			IProcessingContext context = Context;

			NewVersion.Setup( context );
			OldVersion.Setup( context );

			_oldApp = AppContents.Load( OldVersion, context, true );
			_newApp = AppContents.Load( NewVersion, context );

			/////
			// Detect entity changes
			/////
			List<EntityEntry> addedEntities, removedEntities, changedEntities, unchangedEntities;

			Diff.DetectChanges( _oldApp.Entities, _newApp.Entities, null, AddAction, RemoveAction, UpdateAction, UnchangedAction, out addedEntities, out removedEntities, out changedEntities, out unchangedEntities );

			Context.Report.AddedEntities = addedEntities;
			Context.Report.RemovedEntities = removedEntities;
			Context.Report.UpdatedEntities = changedEntities;
			Context.Report.UnchangedEntities = unchangedEntities;

			if ( _oldApp.Entities != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Previous Application Entities", _oldApp.Entities.Count, StatisticsCountType.PreviousApplication ) );
			}

			if ( _newApp.Entities != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Current Application Entities", _newApp.Entities.Count, StatisticsCountType.CurrentApplication ) );
			}

			Context.Report.Counts.Add( new StatisticsCount( "Added Entities", addedEntities.Count, StatisticsCountType.Added ) );
			Context.Report.Counts.Add( new StatisticsCount( "Removed Entities", removedEntities.Count, StatisticsCountType.Removed ) );
			Context.Report.Counts.Add( new StatisticsCount( "Updated Entities", changedEntities.Count, StatisticsCountType.Updated ) );
			Context.Report.Counts.Add( new StatisticsCount( "Unchanged Entities", unchangedEntities.Count, StatisticsCountType.Unchanged ) );

			/////
			// Detect relationship changes
			/////
			List<RelationshipEntry> addedRelationships, removedRelationships, changedRelationships, unchangedRelationships;
			Diff.DetectChanges( _oldApp.Relationships, _newApp.Relationships, null, null, null, null, null, out addedRelationships, out removedRelationships, out changedRelationships, out unchangedRelationships );

			Context.Report.AddedRelationships = addedRelationships;
			Context.Report.RemovedRelationships = removedRelationships;
			Context.Report.UpdatedRelationships = changedRelationships;

			if ( _oldApp.Relationships != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Previous Application Relationships", _oldApp.Relationships.Count, StatisticsCountType.PreviousApplication ) );
			}

			if ( _newApp.Relationships != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Current Application Relationships", _newApp.Relationships.Count, StatisticsCountType.CurrentApplication ) );
			}

			Context.Report.Counts.Add( new StatisticsCount( "Added Relationships", addedRelationships.Count, StatisticsCountType.Added ) );
			Context.Report.Counts.Add( new StatisticsCount( "Removed Relationships", removedRelationships.Count, StatisticsCountType.Removed ) );
			Context.Report.Counts.Add( new StatisticsCount( "Updated Relationships", changedRelationships.Count, StatisticsCountType.Updated ) );
			Context.Report.Counts.Add( new StatisticsCount( "Unchanged Relationships", unchangedRelationships.Count, StatisticsCountType.Unchanged ) );

			Func<DataEntry, DataEntry, bool> changedAction = ( oldVal, newVal ) =>
			{
				newVal.ExistingData = oldVal.Data;

				return true;
			};

			/////
			// Detect and apply data changes
			/////
				foreach ( string dataTable in Helpers.FieldDataTables )
			{
				List<DataEntry> addedData, removedData, changedData, unchangedData;
				Dictionary<Tuple<Guid, Guid>, DataEntry> oldData = _oldApp.FieldData[ dataTable ];
				Dictionary<Tuple<Guid, Guid>, DataEntry> newData = _newApp.FieldData[ dataTable ];

				Diff.DetectChanges( oldData, newData, null, null, null, changedAction, null, out addedData, out removedData, out changedData, out unchangedData );

				context.Report.AddedEntityData[ dataTable ] = addedData;
				context.Report.RemovedEntityData[ dataTable ] = removedData;
				context.Report.UpdatedEntityData[ dataTable ] = changedData;

				if ( oldData != null )
				{
					Context.Report.Counts.Add( new StatisticsCount( $"Previous Application {dataTable} Data", oldData.Count, StatisticsCountType.PreviousApplication ) );
				}

				if ( newData != null )
				{
					Context.Report.Counts.Add( new StatisticsCount( $"Current Application {dataTable} Data", newData.Count, StatisticsCountType.CurrentApplication ) );
				}

				Context.Report.Counts.Add( new StatisticsCount( $"Added {dataTable} Data", addedData.Count, StatisticsCountType.Added ) );
				Context.Report.Counts.Add( new StatisticsCount( $"Removed {dataTable} Data", removedData.Count, StatisticsCountType.Removed ) );
				Context.Report.Counts.Add( new StatisticsCount( $"Updated {dataTable} Data", changedData.Count, StatisticsCountType.Updated ) );
				Context.Report.Counts.Add( new StatisticsCount( $"Unchanged {dataTable} Data", unchangedData.Count, StatisticsCountType.Unchanged ) );
			}

			/////
			// Detect binary data changes
			/////
			List<BinaryDataEntry> addedbinaryData, removedBinaryData, changedBinaryData, unchangedBinaryData;

			Diff.DetectChanges( _oldApp.BinaryData, _newApp.BinaryData, null, null, null, null, null, out addedbinaryData, out removedBinaryData, out changedBinaryData, out unchangedBinaryData );

			context.Report.AddedBinaryData = addedbinaryData;
			context.Report.RemovedBinaryData = removedBinaryData;
			context.Report.UpdatedBinaryData = changedBinaryData;

			if ( _oldApp.BinaryData != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Previous Application Binary Data", _oldApp.BinaryData.Count, StatisticsCountType.PreviousApplication ) );
			}

			if ( _newApp.BinaryData != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Current Application Binary Data", _newApp.BinaryData.Count, StatisticsCountType.CurrentApplication ) );
			}

			Context.Report.Counts.Add( new StatisticsCount( "Added Binary Data", addedbinaryData.Count, StatisticsCountType.Added ) );
			Context.Report.Counts.Add( new StatisticsCount( "Removed Binary Data", removedBinaryData.Count, StatisticsCountType.Removed ) );
			Context.Report.Counts.Add( new StatisticsCount( "Updated Binary Data", changedBinaryData.Count, StatisticsCountType.Updated ) );
			Context.Report.Counts.Add( new StatisticsCount( "Unchanged Binary Data", unchangedBinaryData.Count, StatisticsCountType.Unchanged ) );

			/////
			// Detect binary data changes
			/////
			List<DocumentDataEntry> addedDocumentData, removedDocumentData, changedDocumentData, unchangedDocumentData;

			Diff.DetectChanges( _oldApp.DocumentData, _newApp.DocumentData, null, null, null, null, null, out addedDocumentData, out removedDocumentData, out changedDocumentData, out unchangedDocumentData );

			context.Report.AddedDocumentData = addedDocumentData;
			context.Report.RemovedDocumentData = removedDocumentData;
			context.Report.UpdatedDocumentData = changedDocumentData;

			if ( _oldApp.DocumentData != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Previous Application Document Data", _oldApp.DocumentData.Count, StatisticsCountType.PreviousApplication ) );
			}

			if ( _newApp.DocumentData != null )
			{
				Context.Report.Counts.Add( new StatisticsCount( "Current Application Document Data", _newApp.DocumentData.Count, StatisticsCountType.CurrentApplication ) );
			}

			Context.Report.Counts.Add( new StatisticsCount( "Added Document Data", addedDocumentData.Count, StatisticsCountType.Added ) );
			Context.Report.Counts.Add( new StatisticsCount( "Removed Document Data", removedDocumentData.Count, StatisticsCountType.Removed ) );
			Context.Report.Counts.Add( new StatisticsCount( "Updated Document Data", changedDocumentData.Count, StatisticsCountType.Updated ) );
			Context.Report.Counts.Add( new StatisticsCount( "Unchanged Document Data", unchangedDocumentData.Count, StatisticsCountType.Unchanged ) );

			OldVersion.TearDown( context );
			NewVersion.TearDown( context );
		}

		/// <summary>
		///		Adds the action.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		private bool AddAction( EntityEntry entry )
		{
			entry.State = DataState.Added;

			return true;
		}

		/// <summary>
		///		Removes the action.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		private bool RemoveAction( EntityEntry entry )
		{
			entry.State = DataState.Removed;

			return true;
		}

		/// <summary>
		///		Updates the action.
		/// </summary>
		/// <param name="oldEntry">The old entry.</param>
		/// <param name="newEntry">The new entry.</param>
		/// <returns></returns>
		private bool UpdateAction( EntityEntry oldEntry, EntityEntry newEntry )
		{
			newEntry.State = DataState.Changed;

			return true;
		}

		/// <summary>
		///		Unchanged action.
		/// </summary>
		/// <param name="newEntry">The new entry.</param>
		/// <returns></returns>
		private bool UnchangedAction( EntityEntry newEntry )
		{
			newEntry.State = DataState.Unchanged;

			return true;
		}
	}
}