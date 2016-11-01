// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure;
using EDC.SoftwarePlatform.Migration.Processing;

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics
{
	/// <summary>
	///     Statistics Report class.
	/// </summary>
	public class StatisticsReport
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="StatisticsReport" /> class.
		/// </summary>
		public StatisticsReport( )
		{
			Arguments = new List<KeyValuePair<string, string>>( );

			FailedEntity = new ObservableCollection<EntityFailure>( );
			FailedRelationship = new ObservableCollection<RelationshipFailure>( );
			FailedEntityData = new ObservableCollection<EntityDataFailure>( );

			Counts = new List<StatisticsCount>( );
			MissingDependencies = new List<MissingDependency<object>>( );

			AddedEntityData = new Dictionary<string, IEnumerable<DataEntry>>( );
			RemovedEntityData = new Dictionary<string, IEnumerable<DataEntry>>( );
			UpdatedEntityData = new Dictionary<string, IEnumerable<DataEntry>>( );

			FailedEntity.CollectionChanged += Failures_CollectionChanged;
			FailedRelationship.CollectionChanged += Failures_CollectionChanged;
			FailedEntityData.CollectionChanged += Failures_CollectionChanged;
            
		    try
		    {
                var req = RequestContext.GetContext();
                UserName = req?.Identity?.Name;
            }
		    catch (Exception)
		    {
		        // ignored
		    }
		}

		/// <summary>
		/// Gets or sets the action.
		/// </summary>
		/// <value>
		/// The action.
		/// </value>
		public AppLibraryAction Action
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
	    public string UserName
	    {
	        get;
        }

		/// <summary>
		/// Gets or sets the arguments.
		/// </summary>
		/// <value>
		/// The arguments.
		/// </value>
		public List<KeyValuePair<string, string>> Arguments
		{
			get;
			set;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StatisticsReport" /> class.
		/// </summary>
		/// <param name="errorMessageHandler">The error message handler.</param>
		/// <param name="warningMessageHandler">The warning message handler.</param>
		/// <param name="infoMessageHandler">The information message handler.</param>
		public StatisticsReport( Action<string> errorMessageHandler, Action<string> warningMessageHandler, Action<string> infoMessageHandler )
			: this( )
		{
			ErrorMessageHandler = errorMessageHandler;
			WarningMessageHandler = warningMessageHandler;
			InfoMessageHandler = infoMessageHandler;
		}

		/// <summary>
		///     Gets or sets the added binary data.
		/// </summary>
		/// <value>
		///     The added binary data.
		/// </value>
		public IEnumerable<BinaryDataEntry> AddedBinaryData
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the cardinality violations.
		/// </summary>
		/// <value>
		/// The cardinality violations.
		/// </value>
		public IEnumerable<RelationshipEntry> CardinalityViolations
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the added binary data.
		/// </summary>
		/// <value>
		///     The added binary data.
		/// </value>
		public IEnumerable<DocumentDataEntry> AddedDocumentData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the added entities.
		/// </summary>
		/// <value>
		///     The added entities.
		/// </value>
		public IEnumerable<EntityEntry> AddedEntities
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the added entity data.
		/// </summary>
		/// <value>
		///     The added entity data.
		/// </value>
		public Dictionary<string, IEnumerable<DataEntry>> AddedEntityData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the added relationships.
		/// </summary>
		/// <value>
		///     The added relationships.
		/// </value>
		public IEnumerable<RelationshipEntry> AddedRelationships
		{
			get;
			set;
		}

        /// <summary>
        ///     Gets or sets the secureData entries.
        /// </summary>
        /// <value>
        ///     The added relationships.
        /// </value>
        public IEnumerable<SecureDataEntry> AddedSecureData
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the DoNotRemove entries.
        /// </summary>
        /// <value>
        ///     The added entries.
        /// </value>
        public IEnumerable<Guid> AddedDoNotRemoveData
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the counts.
        /// </summary>
        /// <value>
        ///     The counts.
        /// </value>
        public List<StatisticsCount> Counts
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the details.
		/// </summary>
		/// <value>
		///     The details.
		/// </value>
		public string Details
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the end time.
		/// </summary>
		/// <value>
		///     The end time.
		/// </value>
		public DateTime EndTime
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the error message handler.
		/// </summary>
		/// <value>
		///     The error message handler.
		/// </value>
		private Action<string> ErrorMessageHandler
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the exception.
		/// </summary>
		/// <value>
		///     The exception.
		/// </value>
		public Exception Exception
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the entity failures.
		/// </summary>
		/// <value>
		///     The entity failures.
		/// </value>
		public ObservableCollection<EntityFailure> FailedEntity
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entity data failures.
		/// </summary>
		/// <value>
		///     The entity data failures.
		/// </value>
		public ObservableCollection<EntityDataFailure> FailedEntityData
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationship failures.
		/// </summary>
		/// <value>
		///     The relationship failures.
		/// </value>
		public ObservableCollection<RelationshipFailure> FailedRelationship
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the information message handler.
		/// </summary>
		/// <value>
		///     The information message handler.
		/// </value>
		private Action<string> InfoMessageHandler
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the missing dependencies.
		/// </summary>
		/// <value>
		///     The missing dependencies.
		/// </value>
		public List<MissingDependency<object>> MissingDependencies
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the removed binary data.
		/// </summary>
		/// <value>
		///     The removed binary data.
		/// </value>
		public IEnumerable<BinaryDataEntry> RemovedBinaryData
		{
			get;
			set;
		}

		public IEnumerable<DocumentDataEntry> RemovedDocumentData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the removed entities.
		/// </summary>
		/// <value>
		///     The removed entities.
		/// </value>
		public IEnumerable<EntityEntry> RemovedEntities
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the removed entity data.
		/// </summary>
		/// <value>
		///     The removed entity data.
		/// </value>
		public Dictionary<string, IEnumerable<DataEntry>> RemovedEntityData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the removed relationships.
		/// </summary>
		/// <value>
		///     The removed relationships.
		/// </value>
		public IEnumerable<RelationshipEntry> RemovedRelationships
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the solution dependencies.
		/// </summary>
		/// <value>
		///     The solution dependencies.
		/// </value>
		public IList<SolutionDependency> SolutionDependencies
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the start time.
		/// </summary>
		/// <value>
		///     The start time.
		/// </value>
		public DateTime StartTime
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the unchanged entities.
		/// </summary>
		/// <value>
		///     The unchanged entities.
		/// </value>
		public IEnumerable<EntityEntry> UnchangedEntities
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the updated binary data.
		/// </summary>
		/// <value>
		///     The updated binary data.
		/// </value>
		public IEnumerable<BinaryDataEntry> UpdatedBinaryData
		{
			get;
			set;
		}

		public IEnumerable<DocumentDataEntry> UpdatedDocumentData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the updated entities.
		/// </summary>
		/// <value>
		///     The updated entities.
		/// </value>
		public IEnumerable<EntityEntry> UpdatedEntities
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the updated entity data.
		/// </summary>
		/// <value>
		///     The updated entity data.
		/// </value>
		public Dictionary<string, IEnumerable<DataEntry>> UpdatedEntityData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the updated relationships.
		/// </summary>
		/// <value>
		///     The updated relationships.
		/// </value>
		public IEnumerable<RelationshipEntry> UpdatedRelationships
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the warning message handler.
		/// </summary>
		/// <value>
		///     The warning message handler.
		/// </value>
		private Action<string> WarningMessageHandler
		{
			get;
			set;
		}

		/// <summary>
		///     Adds the line.
		/// </summary>
		/// <param name="sb">The sb.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="padding">The padding.</param>
		private void AddLine( StringBuilder sb, string key, string value, int padding )
		{
			sb.AppendLineFormat( "{0} {1}", ( key + ":" ).PadRight( padding ), value );
		}

		/// <summary>
		///     Handles the CollectionChanged event of all the Failure controls.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event
		///     data.
		/// </param>
		private void Failures_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null )
			{
				foreach ( object newItem in e.NewItems )
				{
					var failure = newItem as MigrationFailure;

					if ( failure != null )
					{
						switch ( failure.Level )
						{
							case FailureLevel.Info:
								if ( InfoMessageHandler != null )
								{
									InfoMessageHandler( failure.ToString( ) );
								}

								break;
							case FailureLevel.Warning:
								if ( WarningMessageHandler != null )
								{
									WarningMessageHandler( failure.ToString( ) );
								}

								break;
							case FailureLevel.Error:
								if ( ErrorMessageHandler != null )
								{
									ErrorMessageHandler( failure.ToString( ) );
								}

								break;
						}
					}
				}
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			var sb = new StringBuilder( );

			if ( Action != AppLibraryAction.Unknown )
			{
				string args = string.Empty;

				foreach ( KeyValuePair<string, string> pair in Arguments )
				{
					if ( args != string.Empty )
					{
						args += ", ";
					}

					args += string.Format( "{0}: {1}", pair.Key, pair.Value );
				}

				sb.Append( string.Format( "{0} Application", Action ) );

				if ( args != string.Empty )
				{
					sb.Append( " - " + args );
				}

				sb.AppendLine( );
			}

			sb.AppendLine( new String( '-', sb.Length ) );
			sb.AppendLine( );

			int padding = 0;

            // Put into a list to isolate from background modifications.
		    IList<StatisticsCount> countsList = Counts.ToList();
            if (countsList.Count > 0)
			{
                padding = countsList.Select(p => p.Name.Length).Max() + 1;
			}

			AddLine( sb, "Start Time", StartTime.ToString( CultureInfo.InvariantCulture ), padding );
			AddLine( sb, "Duration", ( EndTime - StartTime ).ToString( ), padding );
			sb.AppendLine( );

			var statisticsCountType = StatisticsCountType.PreviousApplication;

            foreach (StatisticsCount pair in countsList.OrderBy(pair => pair.CountType).ThenBy(pair => pair.Name))
			{
				if ( pair.CountType != statisticsCountType )
				{
					sb.AppendLine( );
					statisticsCountType = pair.CountType;
				}

				AddLine( sb, pair.Name, pair.Count.ToString( CultureInfo.InvariantCulture ), padding );
			}

			sb.AppendLine( );

			if ( MissingDependencies.Count > 0 )
			{
				sb.AppendLine( "Dropped Rows:" );

				foreach ( MissingDependency<object> missingDependency in MissingDependencies )
				{
					sb.AppendLine( missingDependency.GenerateLogMessage( missingDependency.Entry, missingDependency.Result ) );
				}

				sb.AppendLine( );
			}

			if ( FailedEntity.Count > 0 )
			{
				sb.AppendLine( "Entity Failures:" );

				foreach ( EntityFailure failure in FailedEntity )
				{
					sb.AppendLine( failure.ToString( ) );
				}

				sb.AppendLine( );
			}

			if ( FailedRelationship.Count > 0 )
			{
				sb.AppendLine( "Relationship Failures:" );

				foreach ( RelationshipFailure failure in FailedRelationship )
				{
					sb.AppendLine( failure.ToString( ) );
				}

				sb.AppendLine( );
			}

			if ( FailedEntityData.Count > 0 )
			{
				foreach ( string dataTable in Helpers.FieldDataTables )
				{
					if ( FailedEntityData.Any( f => f.DataTable == dataTable ) )
					{
						sb.AppendLineFormat( "Entity Data Failures ({0}):", dataTable );

						string table = dataTable;

						foreach ( EntityDataFailure failure in FailedEntityData.Where( f => f.DataTable == table ) )
						{
							sb.AppendLine( failure.ToString( ) );
							sb.AppendLine( );
						}
					}
				}
			}

			if ( Exception != null )
			{
				sb.AppendLine( "Exception:" );
				sb.AppendLine( Exception.ToString( ) );
			}

			return sb.ToString( );
		}
	}
}