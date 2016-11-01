// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Messages;
using ReadiMon.Shared.Model;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Entity Browser View Model.
	/// </summary>
	public class EntityBrowserViewModel : EntityViewModel
	{
		/// <summary>
		///     The history.
		/// </summary>
		private readonly History<HistoryEntry> _history = new History<HistoryEntry>( );

		/// <summary>
		///     The add field enabled
		/// </summary>
		private bool _addFieldEnabled;

		/// <summary>
		/// The _delete entity enabled
		/// </summary>
		private bool _deleteEntityEnabled;

		/// <summary>
		///     The add forward relationship enabled
		/// </summary>
		private bool _addForwardRelationshipEnabled;

		/// <summary>
		///     The add reverse relationship enabled
		/// </summary>
		private bool _addReverseRelationshipEnabled;

		/// <summary>
		///     The can navigate back
		/// </summary>
		private bool _canNavigateBack;

		/// <summary>
		///     The can navigate forward
		/// </summary>
		private bool _canNavigateForward;

		/// <summary>
		///     The can reload
		/// </summary>
		private bool _canReload;

		/// <summary>
		///     The fields
		/// </summary>
		private List<IFieldInfo> _fields;

		/// <summary>
		///     The forward relationships
		/// </summary>
		private List<ForwardRelationship> _forwardRelationships;

		/// <summary>
		///     The loading flag.
		/// </summary>
		private bool _loading;

		/// <summary>
		///     The navigating
		/// </summary>
		private bool _navigating;

		/// <summary>
		///     The reverse relationships
		/// </summary>
		private List<ReverseRelationship> _reverseRelationships;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityBrowserViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public EntityBrowserViewModel( IPluginSettings settings ) : base( settings )
		{
			RemoveFieldCommand = new DelegateCommand<IFieldInfo>( RemoveFieldClick );
			RemoveReverseRelationshipCommand = new DelegateCommand<ReverseRelationship>( RemoveReverseRelationshipClick );
			RemoveForwardRelationshipCommand = new DelegateCommand<ForwardRelationship>( RemoveForwardRelationshipClick );
			CopyReverseRelationshipCommand = new DelegateCommand<ReverseRelationship>( CopyReverseRelationshipClick );
			CopyForwardRelationshipCommand = new DelegateCommand<ForwardRelationship>( CopyForwardRelationshipClick );
			BackCommand = new DelegateCommand( BackClick );
			ForwardCommand = new DelegateCommand( ForwardClick );
			ReloadCommand = new DelegateCommand( ReloadClick );
			CopyValueCommand = new DelegateCommand<IFieldInfo>( CopyValueClick );
			CopyCommand = new DelegateCommand<IFieldInfo>( CopyClick );
			AddFieldCommand = new DelegateCommand( AddFieldClick );
			DeleteEntityCommand = new DelegateCommand( DeleteEntityClick );
			AddReverseRelationshipCommand = new DelegateCommand( AddReverseRelationshipClick );
			AddForwardRelationshipCommand = new DelegateCommand( AddForwardRelationshipClick );

			_history.Add( new HistoryEntry( null, null ) );

			EntityNotificationService.Navigate += EntityNotificationService_Navigate;
		}

		/// <summary>
		///     Gets or sets the add field command.
		/// </summary>
		/// <value>
		///     The add field command.
		/// </value>
		public ICommand AddFieldCommand
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the delete entity command.
		/// </summary>
		/// <value>
		/// The delete entity command.
		/// </value>
		public ICommand DeleteEntityCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether adding of fields enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if fields can be added; otherwise, <c>false</c>.
		/// </value>
		public bool AddFieldEnabled
		{
			get
			{
				return _addFieldEnabled;
			}
			set
			{
				SetProperty( ref _addFieldEnabled, value );
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [delete entity enabled].
		/// </summary>
		/// <value>
		///   <c>true</c> if [delete entity enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool DeleteEntityEnabled
		{
			get
			{
				return _deleteEntityEnabled;
			}
			set
			{
				SetProperty( ref _deleteEntityEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets the add forward relationship command.
		/// </summary>
		/// <value>
		///     The add forward relationship command.
		/// </value>
		public ICommand AddForwardRelationshipCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [add forward relationship enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [add forward relationship enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool AddForwardRelationshipEnabled
		{
			get
			{
				return _addForwardRelationshipEnabled;
			}
			set
			{
				SetProperty( ref _addForwardRelationshipEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets the add reverse relationship command.
		/// </summary>
		/// <value>
		///     The add reverse relationship command.
		/// </value>
		public ICommand AddReverseRelationshipCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [add reverse relationship enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [add reverse relationship enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool AddReverseRelationshipEnabled
		{
			get
			{
				return _addReverseRelationshipEnabled;
			}
			set
			{
				SetProperty( ref _addReverseRelationshipEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets the back command.
		/// </summary>
		/// <value>
		///     The back command.
		/// </value>
		public ICommand BackCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance can navigate back.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can navigate back; otherwise, <c>false</c>.
		/// </value>
		public bool CanNavigateBack
		{
			get
			{
				return _canNavigateBack;
			}
			set
			{
				SetProperty( ref _canNavigateBack, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance can navigate forward.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can navigate forward; otherwise, <c>false</c>.
		/// </value>
		public bool CanNavigateForward
		{
			get
			{
				return _canNavigateForward;
			}
			set
			{
				SetProperty( ref _canNavigateForward, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance can reload.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can reload; otherwise, <c>false</c>.
		/// </value>
		public bool CanReload
		{
			get
			{
				return _canReload;
			}
			set
			{
				SetProperty( ref _canReload, value );
			}
		}

		/// <summary>
		///     Gets or sets the copy command.
		/// </summary>
		/// <value>
		///     The copy command.
		/// </value>
		public ICommand CopyCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the copy forward relationship command.
		/// </summary>
		/// <value>
		///     The copy forward relationship command.
		/// </value>
		public ICommand CopyForwardRelationshipCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the copy reverse relationship command.
		/// </summary>
		/// <value>
		///     The copy reverse relationship command.
		/// </value>
		public ICommand CopyReverseRelationshipCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the copy value command.
		/// </summary>
		/// <value>
		///     The copy value command.
		/// </value>
		public ICommand CopyValueCommand
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
		///     Gets or sets the forward command.
		/// </summary>
		/// <value>
		///     The forward command.
		/// </value>
		public ICommand ForwardCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the forward relationships.
		/// </summary>
		/// <value>
		///     The forward relationships.
		/// </value>
		public List<ForwardRelationship> ForwardRelationships
		{
			get
			{
				return _forwardRelationships;
			}
			set
			{
				SetProperty( ref _forwardRelationships, value );
			}
		}

		/// <summary>
		///     Gets or sets the reload command.
		/// </summary>
		/// <value>
		///     The reload command.
		/// </value>
		public ICommand ReloadCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove field command.
		/// </summary>
		/// <value>
		///     The remove field command.
		/// </value>
		public ICommand RemoveFieldCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove forward relationship command.
		/// </summary>
		/// <value>
		///     The remove forward relationship command.
		/// </value>
		public ICommand RemoveForwardRelationshipCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove reverse relationship command.
		/// </summary>
		/// <value>
		///     The remove reverse relationship command.
		/// </value>
		public ICommand RemoveReverseRelationshipCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the reverse relationships.
		/// </summary>
		/// <value>
		///     The reverse relationships.
		/// </value>
		public List<ReverseRelationship> ReverseRelationships
		{
			get
			{
				return _reverseRelationships;
			}
			set
			{
				SetProperty( ref _reverseRelationships, value );
			}
		}

		/// <summary>
		///     Gets or sets the selected tenant.
		/// </summary>
		/// <value>
		///     The selected tenant.
		/// </value>
		public override TenantInfo SelectedTenant
		{
			get
			{
				return base.SelectedTenant;
			}
			set
			{
				base.SelectedTenant = value;

				LoadEntity( SelectedText, true );
			}
		}

		/// <summary>
		///     Gets or sets the selected text.
		/// </summary>
		/// <value>
		///     The selected text.
		/// </value>
		public override string SelectedText
		{
			get
			{
				return base.SelectedText;
			}
			set
			{
				base.SelectedText = value;

				Debug.WriteLine( value );
				LoadEntity( value );
			}
		}

		/// <summary>
		/// Deletes the entity click.
		/// </summary>
		private void DeleteEntityClick( )
		{
			long id;

			if ( IsEntityValid( SelectedText, out id ) )
			{
				MessageBoxResult result = MessageBox.Show( $"Are you sure you wish to delete entity '{id}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning );

				if ( result == MessageBoxResult.Yes )
				{
					string commandText = @"-- ReadiMon: Delte Entity
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Entity Browser->Delete Entity (' + CAST( @entityId AS NVARCHAR(100) ) + ')' )
SET CONTEXT_INFO @contextInfo

DECLARE @tenantId BIGINT

SELECT @tenantId = TenantId FROM Entity WHERE Id = @entityId

EXEC spDelete @entityId, @tenantId";

					using ( var command = DatabaseManager.CreateCommand( commandText ) )
					{
						DatabaseManager.AddParameter( command, "@entityId", id );

						command.ExecuteNonQuery( );

						_history.RemoveCurrent( );

						BackClick( );
					}
				}
			}

			PluginSettings.Channel.SendMessage( new StatusTextMessage( $"Entity '{id}' deleted...", 5000 ).ToString( ) );
		}

		/// <summary>
		///     Adds the field click.
		/// </summary>
		private void AddFieldClick( )
		{
			long id;

			if ( IsEntityValid( SelectedText, out id ) )
			{
				AddFieldDialog dialog = new AddFieldDialog( PluginSettings, id );
				var result = dialog.ShowDialog( );

				if ( result == true )
				{
					AddFieldDialogViewModel vm = dialog.DataContext as AddFieldDialogViewModel;

					if ( vm != null )
					{
						if ( vm.SelectedField != null && !string.IsNullOrEmpty( vm.SelectedField.TableName ) )
						{
							var field = vm.SelectedField;

							string commandText = $@"--ReadiMon AddField
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Entity Browser->Add Field' )
SET CONTEXT_INFO @contextInfo

INSERT INTO {field.TableName} ( EntityId, TenantId, FieldId, Data{( field is DataAliasInfo ? ", Namespace, AliasMarkerId" : string.Empty )} ) VALUES ( @entityId, @tenantId, @fieldId, @data{( field is DataAliasInfo ? ", @nameSpace, @aliasMarkerId" : string.Empty )} )";

							using ( var command = DatabaseManager.CreateCommand( commandText ) )
							{
								try
								{
									DatabaseManager.AddParameter( command, "@entityId", field.EntityId );
									DatabaseManager.AddParameter( command, "@tenantId", field.TenantId );
									DatabaseManager.AddParameter( command, "@fieldId", field.FieldId );

									var aliasField = field as DataAliasInfo;

									if ( aliasField != null && !string.IsNullOrEmpty( aliasField.Data ) )
									{
										string alias;
										string nameSpace;

										if ( aliasField.Data.GetNamespaceAlias( out nameSpace, out alias ) )
										{
											DatabaseManager.AddParameter( command, "@data", alias );
											DatabaseManager.AddParameter( command, "@nameSpace", nameSpace );
											DatabaseManager.AddParameter( command, "@aliasMarkerId", field.FieldId == GetForwardAliasId( field.TenantId ) ? 0 : 1 );
										}
									}

									var bitField = field as DataBitInfo;

									if ( bitField != null )
									{
										DatabaseManager.AddParameter( command, "@data", bitField.Data );
									}

									var dateTimeField = field as DataDateTimeInfo;

									if ( dateTimeField != null )
									{
										DatabaseManager.AddParameter( command, "@data", dateTimeField.Data );
									}

									var decimalField = field as DataDecimalInfo;

									if ( decimalField != null )
									{
										DatabaseManager.AddParameter( command, "@data", decimalField.Data );
									}

									var guidField = field as DataGuidInfo;

									if ( guidField != null )
									{
										DatabaseManager.AddParameter( command, "@data", guidField.Data );
									}

									var intField = field as DataIntInfo;

									if ( intField != null )
									{
										DatabaseManager.AddParameter( command, "@data", intField.Data );
									}

									var nVarCharField = field as DataNVarCharInfo;

									if ( nVarCharField != null )
									{
										DatabaseManager.AddParameter( command, "@data", nVarCharField.Data );
									}

									var xmlField = field as DataXmlInfo;

									if ( xmlField != null )
									{
										DatabaseManager.AddParameter( command, "@data", xmlField.Data );
									}

									command.ExecuteNonQuery( );
								}
								catch ( Exception exc )
								{
									MessageBox.Show( exc.Message );
								}

								LoadEntity( SelectedText, true );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Adds the forward relationship click.
		/// </summary>
		public void AddForwardRelationshipClick( )
		{
			AddRelationship( true );
		}

		/// <summary>
		///     Adds the relationship.
		/// </summary>
		/// <param name="isForward">if set to <c>true</c> [is forward].</param>
		private void AddRelationship( bool isForward )
		{
			long id;

			if ( IsEntityValid( SelectedText, out id ) )
			{
				AddRelationshipDialog dialog = new AddRelationshipDialog( PluginSettings, id, isForward );
				var result = dialog.ShowDialog( );

				if ( result == true )
				{
					AddRelationshipDialogViewModel vm = dialog.DataContext as AddRelationshipDialogViewModel;

					if ( vm != null )
					{
						RelationshipPicker selectedRelationship = vm.SelectedRelationship;

						if ( selectedRelationship != null && selectedRelationship.SelectedInstance != null )
						{
							var instance = selectedRelationship.SelectedInstance;

							var dbManager = new DatabaseManager( PluginSettings.DatabaseSettings );

							string commandText = @"--ReadiMon: Insert Relationship
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Entity Browser->Add Relationship' )
SET CONTEXT_INFO @contextInfo

INSERT INTO Relationship (TenantId, TypeId, FromId, ToId) VALUES (@tenantId, @typeId, @fromId, @toId)";


							using ( var command = dbManager.CreateCommand( commandText ) )
							{
								dbManager.AddParameter( command, "@tenantId", selectedRelationship.TenantId );
								dbManager.AddParameter( command, "@typeId", selectedRelationship.Id );
								dbManager.AddParameter( command, "@fromId", selectedRelationship.IsForward ? SelectedEntityId : instance.Id );
								dbManager.AddParameter( command, "@toId", selectedRelationship.IsForward ? instance.Id : SelectedEntityId );

								command.ExecuteNonQuery( );

								LoadEntity( SelectedText, true );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Adds the reverse relationship click.
		/// </summary>
		public void AddReverseRelationshipClick( )
		{
			AddRelationship( false );
		}

		/// <summary>
		///     Backs the click.
		/// </summary>
		private void BackClick( )
		{
			_navigating = true;

			try
			{
				_history.MoveBack( );
				SelectedText = _history.Current.SelectedText;

				if ( _history.Current.SelectedTenant != null )
				{
					SelectedTenant = _history.Current.SelectedTenant;
				}

				CanNavigateBack = !_history.AtFront;
				CanNavigateForward = !_history.AtEnd;
			}
			finally
			{
				_navigating = false;
			}
		}

		/// <summary>
		///     Copies the click.
		/// </summary>
		/// <param name="field">The field.</param>
		private void CopyClick( IFieldInfo field )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, string.Format( "Entity Id: {0}\nTenant Id: {1}\nField Id: {2}\nValue: {3}", field.EntityId, field.TenantId, field.FieldId, field.Data ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Data copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the forward relationship click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void CopyForwardRelationshipClick( ForwardRelationship obj )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, string.Format( "Type Id: {0}\nTenant Id: {1}\nFrom Id: {2}\nTo Id: {3}", obj.TypeId, obj.TenantId, SelectedEntityId, obj.ToId ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Relationship data copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the reverse relationship click.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void CopyReverseRelationshipClick( ReverseRelationship obj )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, string.Format( "Type Id: {0}\nTenant Id: {1}\nFrom Id: {2}\nTo Id: {3}", obj.TypeId, obj.TenantId, obj.FromId, SelectedEntityId ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Relationship data copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Copies the value click.
		/// </summary>
		/// <param name="field">The field.</param>
		private void CopyValueClick( IFieldInfo field )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, field.Data.ToString( ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Value copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Deletes the relationship.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		private void DeleteRelationship( long typeId, long tenantId, long fromId, long toId )
		{
			var messageBoxResult = MessageBox.Show( "Are you sure you wish to remove this relationship?", "Confirm", MessageBoxButton.YesNo );

			if ( messageBoxResult == MessageBoxResult.Yes )
			{
				try
				{
					const string commandText = @"--ReadiMon - DeleteRelationship
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Entity Browser->Delete Relationship' )
SET CONTEXT_INFO @contextInfo

DELETE FROM Relationship WHERE TypeId = @typeId AND TenantId = @tenantId AND FromId = @fromId AND ToId = @toId";

					using ( var command = DatabaseManager.CreateCommand( commandText ) )
					{
						DatabaseManager.AddParameter( command, "@typeId", typeId );
						DatabaseManager.AddParameter( command, "@tenantId", tenantId );
						DatabaseManager.AddParameter( command, "@fromId", fromId );
						DatabaseManager.AddParameter( command, "@toId", toId );

						command.ExecuteNonQuery( );

						ReloadClick( );
					}
				}
				catch ( Exception exc )
				{
					PluginSettings.EventLog.WriteException( exc );
				}
			}
		}

		/// <summary>
		///     Handles the Navigate event of the EntityNotificationService control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EntityInfoEventArgs" /> instance containing the event data.</param>
		private void EntityNotificationService_Navigate( object sender, EntityInfoEventArgs e )
		{
			if ( SelectedTenant != null && SelectedTenant.Id != e.TenantId )
			{
				var tenantInfo = Tenants.FirstOrDefault( t => t.Id == e.TenantId );

				if ( tenantInfo != null )
				{
					SelectedTenant = tenantInfo;
				}
			}

			long id;
			string val = e.Id;

			if ( long.TryParse( e.Id, out id ) )
			{
				string alias;

				if ( AliasMap.TryGetValue( id, out alias ) )
				{
					long resolvedId = VerifyAliasMap( id, alias );

					if ( resolvedId != id )
					{
						if ( AliasMap.TryGetValue( resolvedId, out alias ) )
						{
							val = alias;
						}
						else
						{
							val = resolvedId.ToString( );
						}
					}
					else
					{
						val = alias;
					}
				}
			}

			SelectedText = val;
		}

		private long VerifyAliasMap( long id, string alias )
		{
			if ( string.IsNullOrEmpty( alias ) )
			{
				return id;
			}

			string ns;
			string a;

			alias.GetNamespaceAlias( out ns, out a );

			using ( var command = DatabaseManager.CreateCommand( @"--ReadiMon - VerifyAliasMap
SELECT TOP 1 EntityId FROM Data_Alias WHERE Data = @alias AND Namespace = @namespace" ) )
			{

				DatabaseManager.AddParameter( command, "@alias", a );
				DatabaseManager.AddParameter( command, "@namespace", ns );

				object resolvedIdObject = command.ExecuteScalar( );

				if ( resolvedIdObject != null && resolvedIdObject != DBNull.Value )
				{
					long resolvedId = ( long ) resolvedIdObject;

					if ( resolvedId != id )
					{
						LoadSearchStrings( );
					}

					return resolvedId;
				}

				return id;
			}
		}

		/// <summary>
		///     Forwards the click.
		/// </summary>
		private void ForwardClick( )
		{
			_navigating = true;

			try
			{
				_history.MoveForward( );
				SelectedText = _history.Current.SelectedText;

				if ( _history.Current.SelectedTenant != null )
				{
					SelectedTenant = _history.Current.SelectedTenant;
				}

				CanNavigateBack = !_history.AtFront;
				CanNavigateForward = !_history.AtEnd;
			}
			finally
			{
				_navigating = false;
			}
		}

		/// <summary>
		///     Loads the entity by identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="enforceTenantId">if set to <c>true</c> [enforce tenant identifier].</param>
		/// <returns></returns>
		private bool GetEntityById( long id, bool enforceTenantId = false )
		{
			Guid upgradeId;

			if ( !CheckEntityExists( id, out upgradeId, enforceTenantId ) )
			{
				return false;
			}

			const string aliasQuery = @"--ReadiMon - GetEntityById
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId );
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId );
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId );

SELECT
	d.EntityId,
	d.TenantId,
	d.FieldId,
	fe.UpgradeId,
	FieldName = fn.Data,
	FieldAlias = fa.Data,
	d.Namespace + ':' + d.Data,
	dd.Data
FROM
	Data_Alias d
JOIN
	Entity fe ON
		d.FieldId = fe.Id AND
		d.TenantId = fe.TenantId
LEFT JOIN
	Data_NVarChar fn ON
		d.FieldId = fn.EntityId AND
		d.TenantId = fn.TenantId AND
		fn.FieldId = @name
LEFT JOIN
	Data_Alias fa ON
		d.FieldId = fa.EntityId AND
		d.TenantId = fa.TenantId AND
		fa.FieldId = @alias
LEFT JOIN
	Data_NVarChar dd ON
		d.FieldId = dd.EntityId AND
		d.TenantId = dd.TenantId AND
		dd.FieldId = @description
WHERE
	d.TenantId = @tenantId
	AND d.EntityId = @id";

			var fields = new List<IFieldInfo>( );

			GetFieldData( id, "Data_Alias", reader => reader.GetString( 6, "" ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataAliasInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields, aliasQuery );
			GetFieldData( id, "Data_Bit", reader => reader.GetBoolean( 6, false ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataBitInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );
			GetFieldData( id, "Data_DateTime", reader => reader.GetDateTime( 6, DateTime.MinValue ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataDateTimeInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );
			GetFieldData( id, "Data_Decimal", reader => reader.GetDecimal( 6, decimal.MinValue ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataDecimalInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );
			GetFieldData( id, "Data_Guid", reader => reader.GetGuid( 6, Guid.Empty ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataGuidInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );
			GetFieldData( id, "Data_Int", reader => reader.GetInt32( 6, int.MinValue ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataIntInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );
			GetFieldData( id, "Data_NVarChar", reader => reader.GetString( 6, string.Empty ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataNVarCharInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );
			GetFieldData( id, "Data_Xml", reader => reader.GetString( 6, string.Empty ), ( eId, tId, fId, fuid, n, a, d, desc ) => new DataXmlInfo( DatabaseManager, eId, tId, fId, fuid, n, a, d, desc ), fields );

			fields.Sort( new FieldComparer( ) );

			var idField = new FieldInfo<long>( DatabaseManager, id, SelectedTenant.Id, -1, Guid.Empty, "Id", null, id, "The entity identifier." )
			{
				IsReadOnly = true
			};

			fields.Insert( 0, idField );

			var upgradeIdField = new FieldInfo<Guid>( DatabaseManager, id, SelectedTenant.Id, -1, Guid.Empty, "Upgrade Id", null, upgradeId, "The entity upgrade identifier." )
			{
				IsReadOnly = true
			};

			fields.Insert( 1, upgradeIdField );

			Fields = fields;

			List<ForwardRelationship> forwardRelationships = GetForwardRelationships( id );
			List<ReverseRelationship> reverseRelationships = GetReverseRelationships( id );

			var relComparer = new RelationshipComparer( );

			forwardRelationships.Sort( relComparer );
			reverseRelationships.Sort( relComparer );

			ForwardRelationships = forwardRelationships;
			ReverseRelationships = reverseRelationships;

			return true;
		}

		/// <summary>
		///     Gets the forward alias identifier.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <returns></returns>
		private long GetForwardAliasId( long tenantId )
		{
			try
			{
				using ( var command = DatabaseManager.CreateCommand( "SELECT dbo.fnAliasNsId( 'alias', 'core', @tenantId )" ) )
				{
					DatabaseManager.AddParameter( command, "@tenantId", tenantId );

					return ( long ) command.ExecuteScalar( );
				}
			}
			catch ( Exception )
			{
				return -1;
			}
		}

		/// <summary>
		///     Loads the entity.
		/// </summary>
		/// <param name="identifier">The alias or identifier.</param>
		/// <param name="enforceTenantId">if set to <c>true</c> [enforce tenant identifier].</param>
		private void LoadEntity( string identifier, bool enforceTenantId = false )
		{
			if ( !_loading )
			{
				_loading = true;

				try
				{
					bool valid = false;

					if ( !string.IsNullOrEmpty( identifier ) )
					{
						long id;

						Guid guid;

						if ( Guid.TryParse( identifier, out guid ) )
						{
							id = GetEntityIdByGuid( guid, true );

							if ( id < 0 )
							{
								id = GetEntityIdByGuid( guid );
							}
						}
						else
						{
							if ( !long.TryParse( identifier, out id ) )
							{
								id = GetEntityIdByAlias( identifier, true );

								if ( id < 0 )
								{
									id = GetEntityIdByAlias( identifier );

									if ( id < 0 )
									{
										id = GetEntityIdByName( identifier );

										if ( id < 0 )
										{
											id = GetEntityIdByName( identifier, true );
										}
									}
								}
							}
						}

						valid = id >= 0 && GetEntityById( id, enforceTenantId );

						SelectedEntityId = id;

						if ( !_navigating && valid )
						{
							_history.Add( new HistoryEntry( identifier, SelectedTenant ) );

							CanNavigateBack = !_history.AtFront;
							CanNavigateForward = !_history.AtEnd;
						}
					}

					CanReload = valid;
					AddFieldEnabled = valid;
					DeleteEntityEnabled = valid;
					AddForwardRelationshipEnabled = valid;
					AddReverseRelationshipEnabled = valid;

					if ( !valid )
					{
						SelectedEntityId = -1;

						Fields = new List<IFieldInfo>( );
						ForwardRelationships = new List<ForwardRelationship>( );
						ReverseRelationships = new List<ReverseRelationship>( );
					}
				}
				finally
				{
					_loading = false;
				}
			}
		}

		/// <summary>
		///     Reload click.
		/// </summary>
		private void ReloadClick( )
		{
			LoadEntity( SelectedText, true );
		}

		/// <summary>
		///     Removes field click.
		/// </summary>
		/// <param name="field">The field.</param>
		private void RemoveFieldClick( IFieldInfo field )
		{
			var messageBoxResult = MessageBox.Show( "Are you sure you wish to remove this field?", "Confirm", MessageBoxButton.YesNo );

			if ( messageBoxResult == MessageBoxResult.Yes )
			{
				try
				{
					string commandText = string.Format( @"--ReadiMon - RemoveFieldClick
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Entity Browser->Delete Field' )
SET CONTEXT_INFO @contextInfo

DELETE FROM {0} WHERE EntityId = @entityId AND TenantId = @tenantId AND FieldId = @fieldId", field.TableName );

					using ( var command = DatabaseManager.CreateCommand( commandText ) )
					{
						DatabaseManager.AddParameter( command, "@entityId", field.EntityId );
						DatabaseManager.AddParameter( command, "@tenantId", field.TenantId );
						DatabaseManager.AddParameter( command, "@fieldId", field.FieldId );

						command.ExecuteNonQuery( );

						ReloadClick( );
					}
				}
				catch ( Exception exc )
				{
					PluginSettings.EventLog.WriteException( exc );
				}
			}
		}

		/// <summary>
		///     Removes the forward relationship click.
		/// </summary>
		/// <param name="relationship">The relationship.</param>
		private void RemoveForwardRelationshipClick( ForwardRelationship relationship )
		{
			DeleteRelationship( relationship.TypeId, relationship.TenantId, SelectedEntityId, relationship.ToId );
		}

		/// <summary>
		///     Removes the reverse relationship click.
		/// </summary>
		/// <param name="relationship">The relationship.</param>
		private void RemoveReverseRelationshipClick( ReverseRelationship relationship )
		{
			DeleteRelationship( relationship.TypeId, relationship.TenantId, relationship.FromId, SelectedEntityId );
		}

		/// <summary>
		///     Sets the entity.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		public void SetEntity( string identifier )
		{
			long id;

			if ( IsEntityValid( identifier, out id ) )
			{
				SelectedText = id.ToString( CultureInfo.InvariantCulture );
			}
		}
	}
}