// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Orphan Detection view model.
	/// </summary>
	/// <seealso cref="EntityViewModel" />
	public class OrphanDetectionViewModel : ViewModelBase
	{
		/// <summary>
		///     The applications
		/// </summary>
		private List<ApplicationInfo> _applications;

		/// <summary>
		///     The database manager.
		/// </summary>
		private DatabaseManager _databaseManager;

		/// <summary>
		///     The delete enabled
		/// </summary>
		private bool _deleteEnabled;

		/// <summary>
		///     The filter count
		/// </summary>
		private string _filterCount;

		/// <summary>
		///     The filtered application
		/// </summary>
		private ApplicationInfo _filteredApplication;

		/// <summary>
		///     The filtered applications
		/// </summary>
		private List<ApplicationInfo> _filteredApplications;

		/// <summary>
		///     The filtered instances
		/// </summary>
		private List<InstanceInfo> _filteredInstances;

		/// <summary>
		///     The filter system instances
		/// </summary>
		private bool _filterSystemInstances;

		/// <summary>
		///     The forward relationships
		/// </summary>
		private List<RelationshipInfo> _forwardRelationships;

		/// <summary>
		///     The instances
		/// </summary>
		private List<InstanceInfo> _instances;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The reverse relationships
		/// </summary>
		private List<RelationshipInfo> _reverseRelationships;

		/// <summary>
		///     The save enabled
		/// </summary>
		private bool _saveEnabled;

		/// <summary>
		///     The selected application
		/// </summary>
		private ApplicationInfo _selectedApplication;

		/// <summary>
		///     The selected tenant.
		/// </summary>
		private TenantInfo _selectedTenant;

		/// <summary>
		///     The selected type.
		/// </summary>
		private TypeInfo _selectedType;

		/// <summary>
		///     The tenants
		/// </summary>
		private List<TenantInfo> _tenants;

		/// <summary>
		///     The types
		/// </summary>
		private List<TypeInfo> _types;

		/// <summary>
		///     Initializes a new instance of the <see cref="OrphanDetectionViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public OrphanDetectionViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			SaveCommand = new DelegateCommand( SaveClick );
			DeleteCommand = new DelegateCommand( DeleteClick );
		}

		/// <summary>
		///     Gets the applications.
		/// </summary>
		/// <value>
		///     The applications.
		/// </value>
		public List<ApplicationInfo> Applications
		{
			get
			{
				return _applications;
			}
		}

		/// <summary>
		///     Gets the delete command.
		/// </summary>
		/// <value>
		///     The delete command.
		/// </value>
		public ICommand DeleteCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [delete enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [delete enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool DeleteEnabled
		{
			get
			{
				return _deleteEnabled;
			}
			set
			{
				SetProperty( ref _deleteEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [filter count].
		/// </summary>
		/// <value>
		///     <c>true</c> if [filter count]; otherwise, <c>false</c>.
		/// </value>
		public string FilterCount
		{
			get
			{
				return _filterCount;
			}
			set
			{
				SetProperty( ref _filterCount, value );
			}
		}

		/// <summary>
		///     Gets or sets the filtered application.
		/// </summary>
		/// <value>
		///     The filtered application.
		/// </value>
		public ApplicationInfo FilteredApplication
		{
			get
			{
				return _filteredApplication;
			}
			set
			{
				SetProperty( ref _filteredApplication, value );

				if ( _filteredApplication != null )
				{
					FilterInstances( );
				}
			}
		}

		/// <summary>
		///     Gets the filtered applications.
		/// </summary>
		/// <value>
		///     The filtered applications.
		/// </value>
		public List<ApplicationInfo> FilteredApplications
		{
			get
			{
				return _filteredApplications;
			}
		}

		/// <summary>
		///     Gets the filtered instances.
		/// </summary>
		/// <value>
		///     The filtered instances.
		/// </value>
		public List<InstanceInfo> FilteredInstances
		{
			get
			{
				return _filteredInstances;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [filter system instances].
		/// </summary>
		/// <value>
		///     <c>true</c> if [filter system instances]; otherwise, <c>false</c>.
		/// </value>
		public bool FilterSystemInstances
		{
			get
			{
				return _filterSystemInstances;
			}
			set
			{
				SetProperty( ref _filterSystemInstances, value );

				FilterInstances( );
			}
		}

		/// <summary>
		///     Gets the forward relationships.
		/// </summary>
		/// <value>
		///     The forward relationships.
		/// </value>
		public List<RelationshipInfo> ForwardRelationships
		{
			get
			{
				return _forwardRelationships;
			}
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		public IPluginSettings PluginSettings
		{
			get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				_databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

				LoadTenants( );

				TypeInfo report = Types.FirstOrDefault( t => t.Alias == "report" );

				if ( report != null )
				{
					SelectedType = report;
				}

				var filteredApplications = new List<ApplicationInfo>
				{
					new ApplicationInfo( -1, string.Empty, "All" )
				};

				LoadApplications_Impl( filteredApplications );

				_filteredApplications = filteredApplications;
				OnPropertyChanged( "FilteredApplications" );

				FilteredApplication = filteredApplications[ 0 ];
			}
		}

		/// <summary>
		///     Gets the reverse relationships.
		/// </summary>
		/// <value>
		///     The reverse relationships.
		/// </value>
		public List<RelationshipInfo> ReverseRelationships
		{
			get
			{
				return _reverseRelationships;
			}
		}

		/// <summary>
		///     Gets or sets the save command.
		/// </summary>
		/// <value>
		///     The save command.
		/// </value>
		public ICommand SaveCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [save enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [save enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool SaveEnabled
		{
			get
			{
				return _saveEnabled;
			}
			set
			{
				SetProperty( ref _saveEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets the selected application.
		/// </summary>
		/// <value>
		///     The selected application.
		/// </value>
		public ApplicationInfo SelectedApplication
		{
			get
			{
				return _selectedApplication;
			}
			set
			{
				SetProperty( ref _selectedApplication, value );

				if ( _selectedApplication != null )
				{
					LoadTypes( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected tenant.
		/// </summary>
		/// <value>
		///     The selected tenant.
		/// </value>
		public TenantInfo SelectedTenant
		{
			get
			{
				return _selectedTenant;
			}
			set
			{
				SetProperty( ref _selectedTenant, value );

				if ( _selectedTenant != null )
				{
					LoadApplications( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected type.
		/// </summary>
		/// <value>
		///     The selected type.
		/// </value>
		public TypeInfo SelectedType
		{
			get
			{
				return _selectedType;
			}
			set
			{
				SetProperty( ref _selectedType, value );

				if ( _selectedType != null )
				{
					LoadRelationships( );
					LoadInstances( );
				}
			}
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public List<TenantInfo> Tenants
		{
			get
			{
				return _tenants;
			}
			set
			{
				SetProperty( ref _tenants, value );
			}
		}

		/// <summary>
		///     Gets the types.
		/// </summary>
		/// <value>
		///     The types.
		/// </value>
		public List<TypeInfo> Types
		{
			get
			{
				return _types;
			}
		}

		/// <summary>
		///     Deletes the click.
		/// </summary>
		private void DeleteClick( )
		{
			MessageBoxResult result = MessageBox.Show( "This will perform a cascade delete on all the listed instances.\nThis operation CANNOT be reversed.\n\nDQ assumes no responsibility if you proceed.\n\nAre you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Exclamation );

			if ( result == MessageBoxResult.Yes )
			{
				result = MessageBox.Show( "Are you REALLY sure you want to delete these?", "Last Warning", MessageBoxButton.YesNo, MessageBoxImage.Stop );

				if ( result == MessageBoxResult.Yes )
				{
					long batchId = -1;

					using ( var command = _databaseManager.CreateCommand( @"--ReadiMon BatchDelete
DECLARE @batchGuid UNIQUEIDENTIFIER = NEWID( )

INSERT INTO
	Batch ( BatchGuid )
VALUES
	( @batchGuid )

SELECT @@IDENTITY" ) )
					{
						var batchIdObject = command.ExecuteScalar( );

						if ( batchIdObject != null )
						{
							batchId = long.Parse( batchIdObject.ToString( ) );
						}
					}

					if ( batchId >= 0 )
					{
						using ( var command = _databaseManager.CreateCommand( @"--ReadiMon BatchDelete
INSERT INTO
		EntityBatch
VALUES
	( @batchId, @entityId )" ) )
						{
							_databaseManager.AddParameter( command, "@batchId", batchId );

							SqlParameter param = null;

							foreach ( InstanceInfo instance in FilteredInstances )
							{
								if ( param == null )
								{
									param = _databaseManager.AddParameter( command, "@entityId", instance.Id );
								}
								else
								{
									param.Value = instance.Id;
								}

								command.ExecuteNonQuery( );
							}
						}

						using ( var command = _databaseManager.CreateCommand( @"--ReadiMon BatchDelete
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Orphan Detection->Delete Batch' )
SET CONTEXT_INFO @contextInfo

EXEC spDeleteBatch @batchId, @tenantId" ) )
						{
							_databaseManager.AddParameter( command, "@batchId", batchId );
							_databaseManager.AddParameter( command, "@tenantId", SelectedTenant.Id );

							command.ExecuteNonQuery( );
						}
					}
				}

				_filteredInstances = new List<InstanceInfo>( );
				OnPropertyChanged( "FilteredInstances" );

				SaveEnabled = false;
				DeleteEnabled = false;
			}
		}

		/// <summary>
		///     Filters the instances.
		/// </summary>
		private void FilterInstances( )
		{
			var instances = new List<InstanceInfo>( );

			if ( _instances != null )
			{
				var fwdRelationships = new HashSet<long>( );

				if ( _forwardRelationships != null )
				{
					foreach ( RelationshipInfo rel in _forwardRelationships )
					{
						if ( rel.Selected )
						{
							fwdRelationships.Add( rel.Id );
						}
					}
				}

				var revRelationships = new HashSet<long>( );

				if ( _reverseRelationships != null )
				{
					foreach ( RelationshipInfo rel in _reverseRelationships )
					{
						if ( rel.Selected )
						{
							revRelationships.Add( rel.Id );
						}
					}
				}

				string filteredApplication = null;

				if ( FilteredApplication.Id >= 0 )
				{
					filteredApplication = FilteredApplication.Name;
				}

				foreach ( InstanceInfo instance in _instances )
				{
					if ( FilterSystemInstances && !string.IsNullOrEmpty( instance.Alias ) )
					{
						continue;
					}

					if ( filteredApplication != null && instance.Applications != null )
					{
						var apps = instance.Applications.Split( ',' );

						bool found = false;

						foreach ( string app in apps )
						{
							if ( app.Trim( ) == filteredApplication )
							{
								found = true;
							}
						}

						if ( !found )
						{
							continue;
						}
					}

					if ( instance.ForwardRelationships.Intersect( fwdRelationships ).Any( ) )
					{
						continue;
					}

					if ( instance.ReverseRelationships.Intersect( revRelationships ).Any( ) )
					{
						continue;
					}

					instances.Add( instance );
				}
			}

			_filteredInstances = instances;
			OnPropertyChanged( "FilteredInstances" );

			FilterCount = FilteredInstances.Count.ToString( CultureInfo.InvariantCulture );
		}

		/// <summary>
		///     Loads the applications.
		/// </summary>
		private void LoadApplications( )
		{
			var applications = new List<ApplicationInfo>
			{
				new ApplicationInfo( -1, string.Empty, "All", "Show all types regardless of which application they belong to.", ApplicationType.All ),
				new ApplicationInfo( -1, string.Empty, "Unassigned", "Only show types that are not assigned to any solution.", ApplicationType.Unassigned )
			};

			LoadApplications_Impl( applications );

			_applications = applications;
			OnPropertyChanged( "Applications" );

			SelectedApplication = _applications[ 0 ];
		}

		private void LoadApplications_Impl( List<ApplicationInfo> applications )
		{
			try
			{
				const string commandText = @"--ReadiMon - LoadApplications
SELECT Id, alias, name FROM _vSolution WHERE TenantId = @tenantId ORDER BY name";

				using ( var command = _databaseManager.CreateCommand( commandText ) )
				{
					_databaseManager.AddParameter( command, "@tenantId", SelectedTenant.Id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string alias = reader.GetString( 1, null );
							string name = reader.GetString( 2, null );

							applications.Add( new ApplicationInfo( id, alias, name, string.Format( "Show types that are either unassigned or belong to the '{0}' application.", name ), ApplicationType.Named ) );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Loads the forward relationships.
		/// </summary>
		private void LoadForwardRelationships( )
		{
			var relationships = new List<RelationshipInfo>( );

			try
			{
				const string commandText = @"--ReadiMon - LoadRelationships
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @fromType BIGINT = dbo.fnAliasNsId( 'fromType', 'core', @tenantId )
DECLARE @toType BIGINT = dbo.fnAliasNsId( 'toType', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @relationship BIGINT = dbo.fnAliasNsId( 'relationship', 'core', @tenantId )
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @cardinality BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenantId )

SELECT DISTINCT
	Id = rt.FromId,
	[Name] = n.Data,
	[Description] = d.Data,
	[FromName] = fn.Data,
	[ToName] = tn.Data,
	[Cardinality] = cn.Data,
	[Solution] = sn.Data
FROM
	dbo.fnAncestorsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON
		r.TenantId = @tenantId
		AND a.Id = r.ToId
		AND r.TypeId = @fromType
JOIN
	Relationship rt ON
		r.TenantId = rt.TenantId
		AND r.FromId = rt.FromId
		AND rt.TypeId = @isOfType
		AND rt.ToId = @relationship
LEFT JOIN
	Data_NVarChar n ON
		rt.TenantId = n.TenantId
		AND rt.FromId = n.EntityId
		AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar d ON
		rt.TenantId = d.TenantId
		AND rt.FromId = d.EntityId
		AND d.FieldId = @description
LEFT JOIN
	Data_NVarChar fn ON
		rt.TenantId = fn.TenantId
		AND r.ToId = fn.EntityId
		AND fn.FieldId = @name
LEFT JOIN
	Relationship tr ON
		tr.TenantId = r.TenantId
		AND tr.FromId = rt.FromId
		AND tr.TypeId = @toType
LEFT JOIN
	Data_NVarChar tn ON
		tr.TenantId = tn.TenantId
		AND tr.ToId = tn.EntityId
		AND tn.FieldId = @name
LEFT JOIN
	Relationship c ON
		rt.TenantId = c.TenantId
		AND rt.FromId = c.FromId
		AND c.TypeId = @cardinality
LEFT JOIN
	Data_NVarChar cn ON
		c.TenantId = cn.TenantId
		AND c.ToId = cn.EntityId
		AND cn.FieldId = @name
LEFT JOIN
	Relationship s ON
		rt.TenantId = s.TenantId
		AND rt.FromId = s.FromId
		AND s.TypeId = @inSolution
LEFT JOIN
	Data_NVarChar sn ON
		s.TenantId = sn.TenantId
		AND s.ToId = sn.EntityId
		AND sn.FieldId = @name
ORDER BY
	n.Data";

				using ( var command = _databaseManager.CreateCommand( commandText ) )
				{
					_databaseManager.AddParameter( command, "@tenantId", SelectedTenant.Id );
					_databaseManager.AddParameter( command, "@typeId", SelectedType.Id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string name = reader.GetString( 1, null );
							string description = reader.GetString( 2, null );
							string fromName = reader.GetString( 3, null );
							string toName = reader.GetString( 4, null );
							string cardinality = reader.GetString( 5, null );
							string solution = reader.GetString( 6, null );

							relationships.Add( new RelationshipInfo( id, name, description, fromName, toName, cardinality, solution, FilterInstances ) );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			_forwardRelationships = relationships;
			OnPropertyChanged( "ForwardRelationships" );
		}

		/// <summary>
		///     Loads the instances.
		/// </summary>
		private void LoadInstances( )
		{
			var instances = new List<InstanceInfo>( );

			try
			{
				const string commandText = @"--ReadiMon - LoadTypes
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @createdDate BIGINT = dbo.fnAliasNsId( 'createdDate', 'core', @tenantId )
DECLARE @modifiedDate BIGINT = dbo.fnAliasNsId( 'modifiedDate', 'core', @tenantId )

SELECT DISTINCT
	[Id] = r.FromId,
	[Name] = n.Data,
	[Description] = d.Data,
	[Alias] = ia.Data,
	[TypeName] = tn.Data,
	[CreatedDate] = cd.Data,
	[ModifiedDate] = md.Data,
	[Solutions] = SUBSTRING(
		(
			SELECT
				',' + sn.Data AS [text()]
			FROM
				Relationship iis
			LEFT JOIN
				Data_NVarChar sn ON
				iis.TenantId = sn.TenantId
				AND sn.EntityId = iis.ToId
				AND sn.FieldId = @name
			WHERE
				r.TenantId = iis.TenantId
				AND iis.FromId = r.FromId
				AND iis.TypeId = @inSolution
			ORDER BY
				sn.Data
			FOR XML PATH('')
		), 2, 10000 ),
	[ForwardRelationships] = SUBSTRING(
		(
			SELECT DISTINCT 
				',' + CAST( fwd.TypeId AS NVARCHAR( 100 ) ) AS [text()]
			FROM
				Relationship fwd
			WHERE
				r.TenantId = fwd.TenantId
				AND fwd.FromId = r.FromId
			FOR XML PATH('')
		), 2, 10000 ),
	[ReverseRelationships] = SUBSTRING(
		(
			SELECT DISTINCT
				',' + CAST( rev.TypeId AS NVARCHAR( 100 ) ) AS [text()]
			FROM
				Relationship rev
			WHERE
				r.TenantId = rev.TenantId
				AND rev.ToId = r.FromId
			FOR XML PATH('')
		), 2, 10000 )
FROM
	dbo.fnDescendantsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON
		r.TenantId = @tenantId
		AND a.Id = r.ToId
		AND r.TypeId = @isOfType
LEFT JOIN
	Data_Alias ia ON
		r.TenantId = ia.TenantId
		AND ia.EntityId = r.FromId
		AND ia.FieldId = @alias
LEFT JOIN
	Data_NVarChar n ON
		r.TenantId = n.TenantId
		AND n.EntityId = r.FromId
		AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar d ON
		r.TenantId = d.TenantId
		AND d.EntityId = r.FromId
		AND d.FieldId = @description
LEFT JOIN
	Data_NVarChar tn ON
		r.TenantId = tn.TenantId
		AND tn.EntityId = r.ToId
		AND tn.FieldId = @name
LEFT JOIN
	Data_DateTime cd ON
		r.TenantId = cd.TenantId
		AND cd.EntityId = r.FromId
		AND cd.FieldId = @createdDate
LEFT JOIN
	Data_DateTime md ON
		r.TenantId = md.TenantId
		AND md.EntityId = r.FromId
		AND md.FieldId = @modifiedDate
ORDER BY
	n.Data";

				using ( var command = _databaseManager.CreateCommand( commandText ) )
				{
					_databaseManager.AddParameter( command, "@tenantId", SelectedTenant.Id );
					_databaseManager.AddParameter( command, "@typeId", SelectedType.Id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string name = reader.GetString( 1, "<Unnamed>" );
							string description = reader.GetString( 2, null );
							string alias = reader.GetString( 3, null );
							string typeName = reader.GetString( 4, null );
							DateTime createdDate = reader.GetDateTime( 5, DateTime.MinValue );
							DateTime modifiedDate = reader.GetDateTime( 6, DateTime.MinValue );
							string applications = reader.GetString( 7, null );
							string forwardRelationships = reader.GetString( 8, null );
							string reverseRelationships = reader.GetString( 9, null );

							var instance = new InstanceInfo( id, SelectedTenant.Id, name, description, alias, typeName, createdDate, modifiedDate, applications, forwardRelationships, reverseRelationships, PluginSettings );

							instances.Add( instance );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			_instances = instances;

			_filteredInstances = instances;
			OnPropertyChanged( "FilteredInstances" );

			SaveEnabled = instances.Count > 0;
			DeleteEnabled = instances.Count > 0;

			FilterCount = FilteredInstances.Count.ToString( CultureInfo.InvariantCulture );
		}

		/// <summary>
		///     Loads the relationships.
		/// </summary>
		private void LoadRelationships( )
		{
			LoadReverseRelationships( );
			LoadForwardRelationships( );
		}

		/// <summary>
		///     Loads the reverse relationships.
		/// </summary>
		private void LoadReverseRelationships( )
		{
			var relationships = new List<RelationshipInfo>( );

			try
			{
				const string commandText = @"--ReadiMon - LoadRelationships
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @fromType BIGINT = dbo.fnAliasNsId( 'fromType', 'core', @tenantId )
DECLARE @toType BIGINT = dbo.fnAliasNsId( 'toType', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @relationship BIGINT = dbo.fnAliasNsId( 'relationship', 'core', @tenantId )
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @cardinality BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenantId )

SELECT DISTINCT
	Id = rt.FromId,
	[Name] = n.Data,
	[Description] = d.Data,
	[FromName] = fn.Data,
	[ToName] = tn.Data,
	[Cardinality] = cn.Data,
	[Solution] = sn.Data
FROM
	dbo.fnAncestorsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON
		r.TenantId = @tenantId
		AND a.Id = r.ToId
		AND r.TypeId = @toType
JOIN
	Relationship rt ON
		r.TenantId = rt.TenantId
		AND r.FromId = rt.FromId
		AND rt.TypeId = @isOfType
		AND rt.ToId = @relationship
LEFT JOIN
	Data_NVarChar n ON
		rt.TenantId = n.TenantId
		AND rt.FromId = n.EntityId
		AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar d ON
		rt.TenantId = d.TenantId
		AND rt.FromId = d.EntityId
		AND d.FieldId = @description
LEFT JOIN
	Relationship fr ON
		fr.TenantId = r.TenantId
		AND fr.FromId = rt.FromId
		AND fr.TypeId = @fromType
LEFT JOIN
	Data_NVarChar fn ON
		fr.TenantId = fn.TenantId
		AND fr.ToId = fn.EntityId
		AND fn.FieldId = @name
LEFT JOIN
	Data_NVarChar tn ON
		rt.TenantId = tn.TenantId
		AND r.ToId = tn.EntityId
		AND tn.FieldId = @name
LEFT JOIN
	Relationship c ON
		rt.TenantId = c.TenantId
		AND rt.FromId = c.FromId
		AND c.TypeId = @cardinality
LEFT JOIN
	Data_NVarChar cn ON
		c.TenantId = cn.TenantId
		AND c.ToId = cn.EntityId
		AND cn.FieldId = @name
LEFT JOIN
	Relationship s ON
		rt.TenantId = s.TenantId
		AND rt.FromId = s.FromId
		AND s.TypeId = @inSolution
LEFT JOIN
	Data_NVarChar sn ON
		s.TenantId = sn.TenantId
		AND s.ToId = sn.EntityId
		AND sn.FieldId = @name
ORDER BY
	n.Data";

				using ( var command = _databaseManager.CreateCommand( commandText ) )
				{
					_databaseManager.AddParameter( command, "@tenantId", SelectedTenant.Id );
					_databaseManager.AddParameter( command, "@typeId", SelectedType.Id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string name = reader.GetString( 1, null );
							string description = reader.GetString( 2, null );
							string fromName = reader.GetString( 3, null );
							string toName = reader.GetString( 4, null );
							string cardinality = reader.GetString( 5, null );
							string solution = reader.GetString( 6, null );

							relationships.Add( new RelationshipInfo( id, name, description, fromName, toName, cardinality, solution, FilterInstances ) );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			_reverseRelationships = relationships;
			OnPropertyChanged( "ReverseRelationships" );
		}

		/// <summary>
		///     Gets the search strings.
		/// </summary>
		/// <value>
		///     The search strings.
		/// </value>
		private void LoadTenants( )
		{
			var tenants = new List<TenantInfo>
			{
				new TenantInfo( 0, "Global", "The global tenant" )
			};

			try
			{
				const string commandText = @"--ReadiMon - LoadTenants
SELECT Id, name, description FROM _vTenant";

				using ( var command = _databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string name = reader.GetString( 1, null );
							string description = reader.GetString( 2, null );

							if ( string.IsNullOrEmpty( name ) )
							{
								continue;
							}

							tenants.Add( new TenantInfo( id, name, description ) );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			Tenants = tenants;

			SelectedTenant = _tenants[ _tenants.Count - 1 ];
		}

		/// <summary>
		///     Loads the types.
		/// </summary>
		private void LoadTypes( )
		{
			var types = new List<TypeInfo>( );

			var dict = new Dictionary<long, TypeInfo>( );

			try
			{
				const string commandText = @"--ReadiMon - LoadTypes
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )

SELECT
	t.Id,
	t.TypeId,
	n.Data,
	t.name,
	t.description,
	t.createdDate,
	t.modifiedDate,
	t.alias,
	t.isAbstract,
	t.isSealed,
	r.ToId,
	r.[To],
	an.Data
FROM
	_vType t
LEFT JOIN
	dbgRelationship r ON
		t.TenantId = r.TenantId
		AND t.Id = r.FromId
		AND r.TypeId = @inSolution
LEFT JOIN
	Data_NVarChar n ON
		t.TenantId = n.TenantId
		AND t.TypeId = n.EntityId
		AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar an ON
		t.TenantId = an.TenantId
		AND r.ToId = an.EntityId
		AND an.FieldId = @name
WHERE
	t.TenantId = @tenantId
	AND t.name IS NOT NULL
	{0}
ORDER BY
	t.name";

				string formattedCommand = null;

				if ( SelectedApplication.Type == ApplicationType.All )
				{
					formattedCommand = string.Format( commandText, string.Empty );
				}
				else if ( SelectedApplication.Type == ApplicationType.Unassigned )
				{
					formattedCommand = string.Format( commandText, "AND r.ToId IS NULL" );
				}
				else if ( SelectedApplication.Type == ApplicationType.Named )
				{
					formattedCommand = string.Format( commandText, string.Format( "AND r.ToId IS NULL OR r.ToId = {0}", SelectedApplication.Id ) );
				}

				using ( var command = _databaseManager.CreateCommand( formattedCommand ) )
				{
					_databaseManager.AddParameter( command, "@tenantId", SelectedTenant.Id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							long typeId = reader.GetInt64( 1 );
							string typeName = reader.GetString( 2 );
							string name = reader.GetString( 3 );
							string description = reader.GetString( 4, null );
							DateTime createdDate = reader.GetDateTime( 5, DateTime.MinValue );
							DateTime modifiedDate = reader.GetDateTime( 6, DateTime.MinValue );
							string alias = reader.GetString( 7, null );
							bool isAbstract = reader.GetBoolean( 8, false );
							bool isSealed = reader.GetBoolean( 9, false );
							long applicationId = reader.GetInt64( 10, -1 );
							string applicationAlias = reader.GetString( 11, string.Empty );
							string applicationName = reader.GetString( 12, string.Empty );

							TypeInfo type;

							if ( !dict.TryGetValue( id, out type ) )
							{
								type = new TypeInfo( id, typeId, typeName, name, description, createdDate, modifiedDate, alias, isAbstract, isSealed );
								types.Add( type );
								dict[ id ] = type;
							}

							if ( applicationId >= 0 )
							{
								type.Applications.Add( new ApplicationInfo( applicationId, applicationAlias, applicationName ) );
							}
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			_types = types;
			OnPropertyChanged( "Types" );

			SelectedType = _types.Count > 0 ? _types[ 0 ] : null;
		}

		/// <summary>
		///     Saves the click.
		/// </summary>
		private void SaveClick( )
		{
			var dlg = new SaveFileDialog
			{
				DefaultExt = ".txt",
				Filter = "Text documents (*.txt)|*.txt",
				Title = "Save Orphan List"
			};

			bool? result = dlg.ShowDialog( );

			if ( result == true )
			{
				string filename = dlg.FileName;

				using ( var writer = File.CreateText( filename ) )
				{
					int maxId = "Id".Length;
					int maxName = "Name".Length;
					int maxDescription = "Description".Length;
					int maxAlias = "Alias".Length;
					int maxCreatedDate = "Created Date".Length;
					int maxModifiedDate = "Modified Date".Length;
					int maxTenantId = "Tenant Id".Length;
					int maxTypeName = "Type Name".Length;
					int maxApplications = "Applications".Length;

					const int gap = 5;

					foreach ( InstanceInfo instance in FilteredInstances )
					{
						if ( instance.Id.ToString( CultureInfo.InvariantCulture ).Length > maxId )
						{
							maxId = instance.Id.ToString( CultureInfo.InvariantCulture ).Length;
						}

						if ( instance.Name != null && instance.Name.Length > maxName )
						{
							maxName = instance.Name.Length;
						}

						if ( instance.Description != null && instance.Description.Length > maxDescription )
						{
							maxDescription = instance.Description.Length;
						}

						if ( instance.Alias != null && instance.Alias.Length > maxAlias )
						{
							maxAlias = instance.Alias.Length;
						}

						if ( instance.CreatedDate != null && instance.CreatedDate.Length > maxCreatedDate )
						{
							maxCreatedDate = instance.CreatedDate.Length;
						}

						if ( instance.ModifiedDate != null && instance.ModifiedDate.Length > maxModifiedDate )
						{
							maxModifiedDate = instance.ModifiedDate.Length;
						}

						if ( instance.TenantId.ToString( CultureInfo.InvariantCulture ).Length > maxTenantId )
						{
							maxTenantId = instance.TenantId.ToString( CultureInfo.InvariantCulture ).Length;
						}

						if ( instance.TypeName != null && instance.TypeName.Length > maxTypeName )
						{
							maxTypeName = instance.TypeName.Length;
						}

						if ( instance.Applications != null && instance.Applications.Length > maxApplications )
						{
							maxApplications = instance.Applications.Length;
						}
					}

					writer.Write( "Id".PadRight( maxId + gap, ' ' ) );
					writer.Write( "TenantId".PadRight( maxTenantId + gap, ' ' ) );
					writer.Write( "Name".PadRight( maxName + gap, ' ' ) );
					writer.Write( "Description".PadRight( maxDescription + gap, ' ' ) );
					writer.Write( "Alias".PadRight( maxAlias + gap, ' ' ) );
					writer.Write( "Created Date".PadRight( maxCreatedDate + gap, ' ' ) );
					writer.Write( "Modified Date".PadRight( maxModifiedDate + gap, ' ' ) );
					writer.Write( "Type Name".PadRight( maxTypeName + gap, ' ' ) );
					writer.Write( "Applications".PadRight( maxApplications + gap, ' ' ) );
					writer.WriteLine( );

					writer.Write( "--".PadRight( maxId + gap, ' ' ) );
					writer.Write( "--------".PadRight( maxTenantId + gap, ' ' ) );
					writer.Write( "----".PadRight( maxName + gap, ' ' ) );
					writer.Write( "-----------".PadRight( maxDescription + gap, ' ' ) );
					writer.Write( "-----".PadRight( maxAlias + gap, ' ' ) );
					writer.Write( "------------".PadRight( maxCreatedDate + gap, ' ' ) );
					writer.Write( "-------------".PadRight( maxModifiedDate + gap, ' ' ) );
					writer.Write( "---------".PadRight( maxTypeName + gap, ' ' ) );
					writer.Write( "------------".PadRight( maxApplications + gap, ' ' ) );
					writer.WriteLine( );

					foreach ( InstanceInfo instance in FilteredInstances )
					{
						writer.Write( instance.Id.ToString( CultureInfo.InvariantCulture ).PadRight( maxId + gap, ' ' ) );
						writer.Write( instance.TenantId.ToString( CultureInfo.InvariantCulture ).PadRight( maxTenantId + gap, ' ' ) );
						writer.Write( instance.Name != null ? instance.Name.PadRight( maxName + gap, ' ' ) : "".PadRight( maxName + gap, ' ' ) );
						writer.Write( instance.Description != null ? instance.Description.PadRight( maxDescription + gap, ' ' ) : "".PadRight( maxDescription + gap, ' ' ) );
						writer.Write( instance.Alias != null ? instance.Alias.PadRight( maxAlias + gap, ' ' ) : "".PadRight( maxAlias + gap, ' ' ) );
						writer.Write( instance.CreatedDate != null ? instance.CreatedDate.PadRight( maxCreatedDate + gap, ' ' ) : "".PadRight( maxCreatedDate + gap, ' ' ) );
						writer.Write( instance.ModifiedDate != null ? instance.ModifiedDate.PadRight( maxModifiedDate + gap, ' ' ) : "".PadRight( maxModifiedDate + gap, ' ' ) );
						writer.Write( instance.TypeName != null ? instance.TypeName.PadRight( maxTypeName + gap, ' ' ) : "".PadRight( maxTypeName + gap, ' ' ) );
						writer.Write( instance.Applications != null ? instance.Applications.PadRight( maxApplications + gap, ' ' ) : "".PadRight( maxApplications + gap, ' ' ) );
						writer.WriteLine( );
					}
				}
			}
		}
	}
}