// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Collections.Generic;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;
using EDC.SoftwarePlatform.Migration.Storage;
using EDC.SoftwarePlatform.Migration.Targets;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Applies a three-way merge.
	/// </summary>
	internal class MergeProcessor : SqlBase
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
        ///     Initializes a new instance of the <see cref="MergeProcessor" /> class.
        /// </summary>
        public MergeProcessor( )
			: this( null )
		{
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergeProcessor" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MergeProcessor( IProcessingContext context )
		{
			Context = context;
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
		///     The 'new application' version to install.
		/// </summary>
		public IDataSource NewVersion
		{
			get;
			set;
		}

		/// <summary>
		///     The 'currently installed' version, participating in the diff.
		/// </summary>
		public IDataSource OldVersion
		{
			get;
			set;
		}

		/// <summary>
		///     The 'new application' version to install.
		/// </summary>
		public IMergeTarget Target
		{
			get;
			set;
		}

		/// <summary>
		///     Merges the data.
		/// </summary>
		public void MergeData( )
		{
			IProcessingContext context = Context;

			OldVersion.Setup( context );
			NewVersion.Setup( context );
			Target.Setup( context );

			context.WriteInfo( "Loading package..." );
			_oldApp = AppContents.Load( OldVersion, context );

			context.WriteInfo( "Loading tenant data..." );
			_newApp = AppContents.Load( NewVersion, context );

			context.WriteInfo( "Processing entities..." );

            // Entities that the application explicitly does not want to delete
            ISet<Guid> doNotRemove = _newApp.DoNotRemove;

            /////
            // Detect entity changes
            /////
            List<EntityEntry> addedEntities,
				removedEntities,
				changedEntities,
				unchangedEntities;
            HashSet<Guid> removedEntitiesSet;

			long tenantId = -1;

			TenantMergeTarget tenantMergeTarget = Target as TenantMergeTarget;

			if ( tenantMergeTarget != null )
			{
				tenantId = tenantMergeTarget.TenantId;
			}

			Guid isTenantDisabled = new Guid( "11209a07-9189-4099-b609-80ed1d4f3e56" );

			Func<EntityEntry, bool> removeEntityAction = e =>
			{
				if ( tenantId == 0 && e.EntityId == isTenantDisabled )
				{
					EventLog.Application.WriteWarning( "Attempt to delete the entity 'isTenantDisabled' from the global tenant has been prevented." );

					return false;
				}

				return true;
			};

			Diff.DetectChanges( _oldApp.Entities, _newApp.Entities, null, null, removeEntityAction, null, null, out addedEntities, out removedEntities, out changedEntities, out unchangedEntities );

            // Suppress removal of entites that are flagged as 'do not remove'
            // removedEntities contains list of entities that we will actually remove
            removedEntities.RemoveAll( entityEntry => doNotRemove.Contains( entityEntry.EntityId ) );

            // Capture raw set of entities that are being removed (excluding the doNotRemove ones)
            removedEntitiesSet = new HashSet<Guid>( removedEntities.Select( entityEntry => entityEntry.EntityId ) );

            Context.Report.AddedEntities = addedEntities;
			Context.Report.RemovedEntities = removedEntities;
			Context.Report.UpdatedEntities = changedEntities;

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
			// Apply entity changes
			// note: 'changedEntities' is always empty
			/////
			Target.WriteEntities( addedEntities, context );

			context.WriteInfo( "Processing relationships..." );

			/////
			// Detect relationship changes
			/////
			List<RelationshipEntry> addedRelationships,
				removedRelationships,
				changedRelationships,
				unchangedRelationships;

			var changeAction = new Func<RelationshipEntry, RelationshipEntry, bool>( ( o, n ) =>
			{
				if ( n.Cardinality == CardinalityEnum_Enumeration.ManyToOne )
				{
					n.PreviousValue = o.ToId;
					return true;
				}

				if ( n.Cardinality == CardinalityEnum_Enumeration.OneToMany )
				{
					n.PreviousValue = o.FromId;
					return true;
				}

				if ( n.Cardinality == CardinalityEnum_Enumeration.OneToOne )
				{
					if ( o.FromId == n.FromId )
					{
						n.PreviousValue = o.ToId;
						n.UpdateTo = true;
						n.UpdateFrom = false;
					}
					else
					{
						n.PreviousValue = o.FromId;
						n.UpdateTo = false;
						n.UpdateFrom = true;
					}
				}

				return true;
			} );

			Func<RelationshipEntry, bool> removeRelationshipAction = e =>
			{
				if ( tenantId == 0 )
				{
					if ( e.FromId == isTenantDisabled )
					{
						EventLog.Application.WriteWarning( "Attempt to delete relatiosnhip from 'isTenantDisabled' in the global tenant has been prevented." );

						return false;
					}

					if ( e.ToId == isTenantDisabled )
					{
						EventLog.Application.WriteWarning( "Attempt to delete relationship to 'isTenantDisabled' in the global tenant has been prevented." );

						return false;
					}
				}

				return true;
			};

			Diff.DetectChanges( _oldApp.Relationships, _newApp.Relationships, _oldApp.MissingRelationships, null, removeRelationshipAction, changeAction, null, out addedRelationships, out removedRelationships, out changedRelationships, out unchangedRelationships );

            // Don't remove relationship content for entities that are flagged as 'do not delete' (unless the other end is being deleted)
            removedRelationships.RemoveAll( relationshipEntry =>
            {
                 if ( !( doNotRemove.Contains( relationshipEntry.ToId ) || doNotRemove.Contains( relationshipEntry.FromId ) ) )
                     return false;  // neither end is in the 'do not remove' list, so just carry on with the deletion (by leaving the row in the removal collection)

                 // Always allow apps to remove their association to an entity
                 if ( relationshipEntry.TypeId == Guids.InSolution || relationshipEntry.TypeId == Guids.IndirectInSolution )
                    return false;

                 // If the other end of the relationship (or the relationship type) is being deleted, then delete the relationship instance anyway
                 if ( removedEntitiesSet.Contains( relationshipEntry.ToId ) )
                     return false;
                 if ( removedEntitiesSet.Contains( relationshipEntry.FromId ) )
                     return false;
                 if ( removedEntitiesSet.Contains( relationshipEntry.TypeId ) )
                     return false;

                 return true;   // the relationship is partly in the do-not-remove list, and the other end is not being removed, so don't delete this relationship
            } );


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

			/////
			// Apply relationship changes
			/////
			Target.UpdateRelationships( changedRelationships, context );
			Target.WriteRelationships( addedRelationships, context );

			Target.PreDeleteEntities( removedEntities, context );

			Target.DeleteRelationships( removedRelationships, context );
			Target.DeleteEntities( removedEntities, context );

			/////
			// Invalidate all per-tenant caches at this point.
			// This is to ensure caches that are holding onto type information are flushed.
			/////
			InvalidatePerTenantCaches( tenantId );

			Func<DataEntry, bool> removedAction = de =>
			{
				if ( de != null )
				{
					if ( tenantId == 0 && de.EntityId == isTenantDisabled )
					{
						EventLog.Application.WriteWarning( "Attempt to delete field on entity 'isTenantDisabled' in the global tenant has been prevented." );

						return false;
					}
				}

				return true;
			};

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
				context.WriteInfo( $"Processing '{dataTable}'..." );

				List<DataEntry> addedData,
					removedData,
					changedData,
					unchangedData;
				Dictionary<Tuple<Guid, Guid>, DataEntry> oldData = _oldApp.FieldData[ dataTable ];
				Dictionary<Tuple<Guid, Guid>, DataEntry> missingData = _oldApp.MissingFieldData;
				Dictionary<Tuple<Guid, Guid>, DataEntry> newData = _newApp.FieldData[ dataTable ];

				Diff.DetectChanges( oldData, newData, missingData, null, removedAction, changedAction, null, out addedData, out removedData, out changedData, out unchangedData );

                // Don't remove content for entities that are tagged as 'do not delete'
                removedData.RemoveAll( dataEntry => _newApp.DoNotRemove.Contains( dataEntry.EntityId ) );

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

				Target.WriteFieldData( dataTable, addedData, context );
				Target.DeleteFieldData( dataTable, removedData, context );
				Target.UpdateFieldData( dataTable, changedData, context );
			}

			context.WriteInfo( "Processing binary data..." );

			/////
			// Detect binary data changes
			/////
			List<BinaryDataEntry> addedbinaryData,
				removedBinaryData,
				changedBinaryData,
				unchangedBinaryData;

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
			// Apply binary changes     
			/////
			Target.WriteBinaryData( addedbinaryData, context );
			// Update before delete. This is to ensure that if any files have the IsReferencedExternally flag set
			// that they will not be deleted.
			Target.UpdateBinaryData( changedBinaryData, context );
			Target.DeleteBinaryData( removedBinaryData, context );

			context.WriteInfo( "Processing document data..." );

			/////
			// Detect binary data changes
			/////
			List<DocumentDataEntry> addedDocumentData,
				removedDocumentData,
				changedDocumentData,
				unchangedDocumentData;

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

			/////
			// Apply binary changes     
			/////
			Target.WriteDocumentData( addedDocumentData, context );
			Target.UpdateDocumentData( changedDocumentData, context );
			Target.DeleteDocumentData( removedDocumentData, context );

            Target.TearDown( context );
            NewVersion.TearDown( context );
            OldVersion.TearDown( context );
        }

        /// <summary>
        /// Invalidates the per tenant caches.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        private void InvalidatePerTenantCaches( long tenantId )
		{
			// Invalidate all per-tenant cache entries.
			// Note: this only works for the PerTenantNonSharingCache, not the PerTenantCache.
			Factory.Current.Resolve<IPerTenantCacheInvalidator>( ).InvalidateTenant( tenantId );
		}
	}
}