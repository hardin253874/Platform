// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Data;
using System.Windows;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Model;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     The AddRelationshipDialogViewModel class.
	/// </summary>
	/// <seealso cref="ViewModelBase" />
	public class AddRelationshipDialogViewModel : ViewModelBase
	{
		private bool? _dialogResult;
		private bool _okEnabled;
		private List<RelationshipPicker> _relationships;
		private RelationshipPicker _selectedRelationship;
		private string _title;
		private string _titleDescription;

		/// <summary>
		///     Initializes a new instance of the <see cref="AddFieldDialogViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="selectedEntityId">The selected entity identifier.</param>
		/// <param name="forward">if set to <c>true</c> [forward].</param>
		public AddRelationshipDialogViewModel( IPluginSettings settings, long selectedEntityId, bool forward )
		{
			PluginSettings = settings;
			SelectedEntityId = selectedEntityId;
			IsForward = forward;

			LoadRelationships( );

			CancelCommand = new DelegateCommand( CancelClicked );
			OkCommand = new DelegateCommand( OkClicked );

			Title = "Add " + ( IsForward ? "Forward" : "Reverse" ) + " Relationship";
			TitleDescription = "Add a new " + ( IsForward ? "Forward" : "Reverse" ) + " Relationship to the selected entity...";
		}

		/// <summary>
		///     Gets or sets the cancel command.
		/// </summary>
		/// <value>
		///     The cancel command.
		/// </value>
		public DelegateCommand CancelCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [dialog result].
		/// </summary>
		/// <value>
		///     <c>true</c> if [dialog result]; otherwise, <c>false</c>.
		/// </value>
		public bool? DialogResult
		{
			get
			{
				return _dialogResult;
			}
			private set
			{
				SetProperty( ref _dialogResult, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is forward.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is forward; otherwise, <c>false</c>.
		/// </value>
		public bool IsForward
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the ok command.
		/// </summary>
		/// <value>
		///     The ok command.
		/// </value>
		public DelegateCommand OkCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [ok enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ok enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool OkEnabled
		{
			get
			{
				return _okEnabled;
			}
			set
			{
				SetProperty( ref _okEnabled, value );
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
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the fields.
		/// </summary>
		/// <value>
		///     The fields.
		/// </value>
		public List<RelationshipPicker> Relationships
		{
			get
			{
				return _relationships;
			}
			set
			{
				SetProperty( ref _relationships, value );
			}
		}

		/// <summary>
		///     Gets or sets the selected entity identifier.
		/// </summary>
		/// <value>
		///     The selected entity identifier.
		/// </value>
		private long SelectedEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selected field.
		/// </summary>
		/// <value>
		///     The selected field.
		/// </value>
		public RelationshipPicker SelectedRelationship
		{
			get
			{
				return _selectedRelationship;
			}
			set
			{
				foreach ( RelationshipPicker rel in Relationships )
				{
					rel.Visible = rel == value ? Visibility.Visible : Visibility.Hidden;
				}

				SetProperty( ref _selectedRelationship, value );

				OkEnabled = value != null && value.SelectedInstance != null;
			}
		}

		/// <summary>
		///     Gets or sets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				SetProperty( ref _title, value );
			}
		}

		/// <summary>
		///     Gets or sets the title description.
		/// </summary>
		/// <value>
		///     The title description.
		/// </value>
		public string TitleDescription
		{
			get
			{
				return _titleDescription;
			}
			set
			{
				SetProperty( ref _titleDescription, value );
			}
		}

		/// <summary>
		///     Cancels the clicked.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		private void CancelClicked( )
		{
			DialogResult = false;
		}

		/// <summary>
		///     Loads the fields.
		/// </summary>
		private void LoadRelationships( )
		{
			var dbManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			string commandText;

			if ( IsForward )
			{
				commandText = @"-- ReadiMon: GetForwardRelationships
SET NOCOUNT ON

DECLARE @tenantId BIGINT
SELECT @tenantId = TenantId FROM Entity WHERE Id = @entityId

DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @from BIGINT = dbo.fnAliasNsId( 'fromType', 'core', @tenantId )
DECLARE @to BIGINT = dbo.fnAliasNsId( 'toType', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @cardinality BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenantId )

DECLARE @typeId BIGINT
SELECT @typeId = ToId FROM Relationship WHERE TenantId = @tenantId AND FromId = @entityId AND TypeId = @isOfType

SELECT DISTINCT
	[TenantId] = @tenantId,
	[TypeId] = r.FromId,
	[TypeUpgradeId] = e.UpgradeId,
	[Name] = n.Data,
	[Description] = d.Data,
	[FromId] = r.ToId,
	[From] = yna.Data,
	[ToId] = o.ToId,
	[To] = xna.Data,
	[Cardinality] = ca.Data,
	[Alias] = ta.Namespace + ':' + ta.Data
FROM
	dbo.fnAncestorsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON r.TenantId = @tenantId AND r.ToId = a.Id AND r.TypeId = @from
JOIN
	Relationship o ON o.TenantId = @tenantId AND r.FromId = o.FromId AND o.TypeId = @to
LEFT JOIN
	Data_Alias ta ON ta.TenantId = @tenantId AND r.FromId = ta.EntityId AND ta.FieldId = @alias AND ta.AliasMarkerId = 0
LEFT JOIN
	Data_NVarChar xna ON xna.TenantId = @tenantId AND o.ToId = xna.EntityId AND xna.FieldId = @name
LEFT JOIN
	Data_NVarChar yna ON yna.TenantId = @tenantId AND r.ToId = yna.EntityId AND yna.FieldId = @name
LEFT JOIN
	Relationship c ON r.TenantId = c.TenantId AND r.FromId = c.FromId AND c.TypeId = @cardinality
LEFT JOIN
	Data_Alias ca ON r.TenantId = ca.TenantId AND ca.EntityId = c.ToId AND ca.FieldId = @alias
JOIN
	Entity e ON r.FromId = e.Id AND e.TenantId = @tenantId
LEFT JOIN
	Data_NVarChar n ON n.TenantId = @tenantId AND r.FromId = n.EntityId AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar d ON d.TenantId = @tenantId AND r.FromId = d.EntityId AND d.FieldId = @description
ORDER BY
	n.Data

SELECT
	TenantId,
	TypeId,
	FromId,
	ToId
FROM
	Relationship
WHERE
	TenantId = @tenantId AND
	FromId = @entityId";
			}
			else
			{
				commandText = @"-- ReadiMon: GetReverseRelationships
SET NOCOUNT ON

DECLARE @tenantId BIGINT
SELECT @tenantId = TenantId FROM Entity WHERE Id = @entityId

DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @from BIGINT = dbo.fnAliasNsId( 'fromType', 'core', @tenantId )
DECLARE @to BIGINT = dbo.fnAliasNsId( 'toType', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )
DECLARE @reverseAlias BIGINT = dbo.fnAliasNsId( 'reverseAlias', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @cardinality BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenantId )

DECLARE @typeId BIGINT
SELECT @typeId = ToId FROM Relationship WHERE TenantId = @tenantId AND FromId = @entityId AND TypeId = @isOfType

SELECT DISTINCT
	[TenantId] = @tenantId,
	[TypeId] = r.FromId,
	[TypeUpgradeId] = e.UpgradeId,
	[Name] = n.Data,
	[Description] = d.Data,
	[FromId] = o.ToId,
	[From] = xna.Data,
	[ToId] = r.ToId,
	[To] = yna.Data,
	[Cardinality] = ca.Data,
	[Alias] = ta.Namespace + ':' + ta.Data
FROM
	dbo.fnAncestorsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON r.TenantId = @tenantId AND r.ToId = a.Id AND r.TypeId = @to
JOIN
	Relationship o ON o.TenantId = @tenantId AND r.FromId = o.FromId AND o.TypeId = @from
LEFT JOIN
	Data_Alias ta ON ta.TenantId = @tenantId AND r.FromId = ta.EntityId AND ta.FieldId = @reverseAlias AND ta.AliasMarkerId = 1
LEFT JOIN
	Data_NVarChar xna ON xna.TenantId = @tenantId AND o.ToId = xna.EntityId AND xna.FieldId = @name
LEFT JOIN
	Data_NVarChar yna ON yna.TenantId = @tenantId AND r.ToId = yna.EntityId AND yna.FieldId = @name
LEFT JOIN
	Relationship c ON r.TenantId = c.TenantId AND r.FromId = c.FromId AND c.TypeId = @cardinality
LEFT JOIN
	Data_Alias ca ON r.TenantId = ca.TenantId AND ca.EntityId = c.ToId AND ca.FieldId = @alias
JOIN
	Entity e ON r.FromId = e.Id AND e.TenantId = @tenantId
LEFT JOIN
	Data_NVarChar n ON n.TenantId = @tenantId AND r.FromId = n.EntityId AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar d ON d.TenantId = @tenantId AND r.FromId = d.EntityId AND d.FieldId = @description
ORDER BY
	n.Data

SELECT
	TenantId,
	TypeId,
	FromId,
	ToId
FROM
	Relationship
WHERE
	TenantId = @tenantId AND
	ToId = @entityId";
			}

			var command = dbManager.CreateCommand( commandText );

			dbManager.AddParameter( command, "@entityId", SelectedEntityId );

			using ( IDataReader reader = command.ExecuteReader( ) )
			{
				var relationships = new List<RelationshipPicker>( );

				Dictionary<long, RelationshipPicker> map = new Dictionary<long, RelationshipPicker>( );

				while ( reader.Read( ) )
				{
					var tenantId = reader.GetInt64( 0 );
					var id = reader.GetInt64( 1 );
					var upgradeId = reader.GetGuid( 2 );
					var name = reader.GetString( 3, "<Unnamed>" );
					var description = reader.GetString( 4, "No description specified" );
					var fromId = reader.GetInt64( 5 );
					var from = reader.GetString( 6, "<Unnamed>" );
					var toId = reader.GetInt64( 7 );
					var to = reader.GetString( 8, "<Unnamed>" );
					var cardinality = reader.GetString( 9, null );
					var alias = reader.GetString( 10, null );

					RelationshipPicker rel = new RelationshipPicker( id, tenantId, upgradeId, name + ( alias == null ? "" : $" - ({alias})" ), description, fromId, from, toId, to, cardinality, IsForward, PluginSettings );

					relationships.Add( rel );
					map[ rel.Id ] = rel;
				}

				if ( reader.NextResult( ) )
				{
					while ( reader.Read( ) )
					{
						var existingRelationshipId = reader.GetInt64( 1 );

						RelationshipPicker relationship;

						if ( map.TryGetValue( existingRelationshipId, out relationship ) )
						{
							if ( relationship.Cardinality == "oneToOne" || relationship.Cardinality == "oneToMany" )
							{
								relationship.Disabled = true;
							}
						}
					}
				}

				Relationships = relationships;
			}
		}

		/// <summary>
		///     Oks the clicked.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		private void OkClicked( )
		{
			DialogResult = true;
		}
	}
}