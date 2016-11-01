// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Model;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     The AddFieldDialogViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class AddFieldDialogViewModel : ViewModelBase
	{
		private bool? _dialogResult;
		private List<IFieldInfo> _fields;
		private bool _okEnabled;
		private IFieldInfo _selectedField;

		/// <summary>
		///     Initializes a new instance of the <see cref="AddFieldDialogViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="selectedEntityId">The selected entity identifier.</param>
		public AddFieldDialogViewModel( IPluginSettings settings, long selectedEntityId )
		{
			PluginSettings = settings;
			SelectedEntityId = selectedEntityId;

			LoadFields( );

			CancelCommand = new DelegateCommand( CancelClicked );
			OkCommand = new DelegateCommand( OkClicked );
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
		///     Gets or sets the fields.
		/// </summary>
		/// <value>
		///     The fields.
		/// </value>
		public List<IFieldInfo> Fields
		{
			get
			{
				return _fields;
			}
			set
			{
				SetProperty( ref _fields, value );
			}
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
		public IFieldInfo SelectedField
		{
			get
			{
				return _selectedField;
			}
			set
			{
				if ( _selectedField != null )
				{
					_selectedField.EditViewMode = EditViewMode.View;
				}

				if ( value != null )
				{
					value.EditViewMode = EditViewMode.Edit;
				}

				SetProperty( ref _selectedField, value );

				OkEnabled = value != null;
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
		private void LoadFields( )
		{
			var dbManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			var command = dbManager.CreateCommand( @"-- ReadiMon: Get Fields
SET NOCOUNT ON

DECLARE @tenantId BIGINT
SELECT @tenantId = TenantId FROM Entity WHERE Id = @entityId

DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @fields BIGINT = dbo.fnAliasNsId( 'fieldIsOnType', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @dbFieldTable BIGINT = dbo.fnAliasNsId( 'dbFieldTable', 'core', @tenantId )

DECLARE @typeId BIGINT
SELECT @typeId = ToId FROM Relationship WHERE TenantId = @tenantId AND FromId = @entityId AND TypeId = @isOfType

SELECT DISTINCT
	[Id] = @entityId,
	[TenantId] = @tenantId,
	[FieldId] = r.FromId,
	[FieldUpgradeId] = e.UpgradeId,
	[Name] = n.Data,
	[Alias] = fa.Namespace + ':' + fa.Data,
	[Description] = d.Data,
	[FieldType] = t.ToId,
	[Table] = ft.Data
FROM
	dbo.fnAncestorsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON r.TenantId = @tenantId AND r.ToId = a.Id AND r.TypeId = @fields
JOIN
	Entity e ON r.FromId = e.Id AND e.TenantId = @tenantId
LEFT JOIN
	Data_NVarChar n ON n.TenantId = @tenantId AND r.FromId = n.EntityId AND n.FieldId = @name
LEFT JOIN
	Data_Alias fa ON fa.TenantId = @tenantId AND r.FromId = fa.EntityId AND fa.FieldId = @alias
LEFT JOIN
	Data_NVarChar d ON d.TenantId = @tenantId AND r.FromId = d.EntityId AND d.FieldId = @description
JOIN
	Relationship t ON t.TenantId = @tenantId AND t.FromId = r.FromId AND t.TypeId = @isOfType
JOIN
	Data_NVarChar ft ON ft.TenantId = @tenantId AND t.ToId = ft.EntityId AND ft.FieldId = @dbFieldTable
ORDER BY
	n.Data

SELECT FieldId FROM Data_Alias WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_Bit WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_DateTime WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_Decimal WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_Guid WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_Int WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_NVarChar WHERE TenantId = @tenantId AND EntityId = @entityId
UNION ALL
SELECT FieldId FROM Data_Xml WHERE TenantId = @tenantId AND EntityId = @entityId" );

			dbManager.AddParameter( command, "@entityId", SelectedEntityId );

			using ( IDataReader reader = command.ExecuteReader( ) )
			{
				var fields = new List<IFieldInfo>( );

				Dictionary<long, IFieldInfo> map = new Dictionary<long, IFieldInfo>( );

				while ( reader.Read( ) )
				{
					var entityId = reader.GetInt64( 0 );
					var tenantId = reader.GetInt64( 1 );
					var fieldId = reader.GetInt64( 2 );
					var fieldUpgradeId = reader.GetGuid( 3 );
					var name = reader.GetString( 4, "<Unnamed>" );
					var alias = reader.GetString( 5, string.Empty );
					var description = reader.GetString( 6, string.Empty );
					var fieldType = reader.GetInt64( 7 );
					var tableType = reader.GetString( 8, "String" );

					IFieldInfo field = null;

					switch ( tableType )
					{
						case "Data_Alias":
							field = new DataAliasInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, null, description );
							break;
						case "Data_Bit":
							field = new DataBitInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, false, description );
							break;
						case "Data_DateTime":
							field = new DataDateTimeInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, DateTime.MinValue, description );
							break;
						case "Data_Decimal":
							field = new DataDecimalInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, 0, description );
							break;
						case "Data_Guid":
							field = new DataGuidInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, Guid.Empty, description );
							break;
						case "Data_Int":
							field = new DataIntInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, 0, description );
							break;
						case "Data_NVarChar":
							field = new DataNVarCharInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, null, description );
							break;
						case "Data_Xml":
							field = new DataXmlInfo( dbManager, entityId, tenantId, fieldId, fieldUpgradeId, name + ( alias == null ? "" : $" - ({alias})" ), alias, null, description );
							break;
					}

					if ( field != null )
					{
						field.NewField = true;
						fields.Add( field );
						map[ field.FieldId ] = field;
					}
				}

				if ( reader.NextResult( ) )
				{
					while ( reader.Read( ) )
					{
						var existingFieldId = reader.GetInt64( 0 );

						IFieldInfo field;

						if ( map.TryGetValue( existingFieldId, out field ) )
						{
							field.Disabled = true;
						}
					}
				}

				Fields = fields;
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