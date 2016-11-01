// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     Field Info
	/// </summary>
	public class FieldInfo<T> : ViewModelBase, IFieldInfo
	{
		/// <summary>
		///     The database manager.
		/// </summary>
		private readonly DatabaseManager _databaseManager;

		/// <summary>
		///     Loading
		/// </summary>
		private readonly bool _loading;

		/// <summary>
		///     The data.
		/// </summary>
		private T _data;

		/// <summary>
		///     The focused
		/// </summary>
		private bool _focused;

		/// <summary>
		///     Edit/View mode.
		/// </summary>
		private EditViewMode _mode;

		/// <summary>
		///     Initializes a new instance of the <see cref="FieldInfo{T}" /> class.
		/// </summary>
		/// <param name="databaseManager">The database manager.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="data">The data.</param>
		/// <param name="description">The description.</param>
		public FieldInfo( DatabaseManager databaseManager, long entityId, long tenantId, long fieldId, Guid fieldUpgradeId, string name, string alias, T data, string description )
		{
			_loading = true;

			try
			{
				_databaseManager = databaseManager;

				EntityId = entityId;
				TenantId = tenantId;
				FieldId = fieldId;
				FieldUpgradeId = fieldUpgradeId;
				Name = name;
				Alias = alias;
				Data = data;
				Description = description;

				EditCommand = new DelegateCommand( EditClick );
				DoneCommand = new DelegateCommand( DoneClick );
				UndoCommand = new DelegateCommand<TextBox>( UndoClick );
				EnterCommand = new DelegateCommand( EnterPress );
				NavigateCommand = new DelegateCommand( NavigateClick );
			}
			finally
			{
				_loading = false;
			}
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		public T Data
		{
			get
			{
				return _data;
			}
			set
			{
				if ( !EqualityComparer<T>.Default.Equals( value, _data ) )
				{
					if ( NewField )
					{
						SetProperty( ref _data, value );
					}
					else
					{
						var result = MessageBoxResult.Yes;

						if ( !_loading )
						{
							result = MessageBox.Show( "Are you sure you wish to save this change?", "Confirm", MessageBoxButton.YesNo );
						}

						if ( result == MessageBoxResult.Yes )
						{
							SetProperty( ref _data, value );

							if ( !_loading )
							{
								try
								{
									string commandText = string.Format( @"--ReadiMon - FieldInfo.Data
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Entity Browser->Update Field' )
SET CONTEXT_INFO @contextInfo

UPDATE {0} SET Data = @data WHERE EntityId = @entityId AND TenantId = @tenantId AND FieldId = @fieldId", TableName );

									using ( var command = _databaseManager.CreateCommand( commandText ) )
									{
										_databaseManager.AddParameter( command, "@data", Data );
										_databaseManager.AddParameter( command, "@entityId", EntityId );
										_databaseManager.AddParameter( command, "@tenantId", TenantId );
										_databaseManager.AddParameter( command, "@fieldId", FieldId );

										command.ExecuteNonQuery( );
									}
								}
								catch ( Exception exc )
								{
									MessageBox.Show( "Failed to update field value. " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the done command.
		/// </summary>
		/// <value>
		///     The done command.
		/// </value>
		public ICommand DoneCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the edit command.
		/// </summary>
		/// <value>
		///     The edit command.
		/// </value>
		public ICommand EditCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the enter command.
		/// </summary>
		/// <value>
		///     The enter command.
		/// </value>
		public ICommand EnterCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="FieldInfo{T}" /> is focused.
		/// </summary>
		/// <value>
		///     <c>true</c> if focused; otherwise, <c>false</c>.
		/// </value>
		public bool Focused
		{
			get
			{
				return _focused;
			}
			set
			{
				SetProperty( ref _focused, value );
			}
		}

		/// <summary>
		///     Gets or sets the navigate command.
		/// </summary>
		/// <value>
		///     The navigate command.
		/// </value>
		public ICommand NavigateCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the old data.
		/// </summary>
		/// <value>
		///     The old data.
		/// </value>
		private T OldData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the tool-tip.
		/// </summary>
		/// <value>
		///     The tool-tip.
		/// </value>
		public string Tooltip
		{
			get
			{
				if ( string.IsNullOrEmpty( Description ) )
				{
					return string.Format( "Entity Id: {0}\nTenant Id: {1}\nField Id: {2}", EntityId, TenantId, FieldId );
				}

				return string.Format( "{0}\n\nEntity Id: {1}\nTenant Id: {2}\nField Id: {3}", Description, EntityId, TenantId, FieldId );
			}
		}

		/// <summary>
		///     Gets or sets the undo command.
		/// </summary>
		/// <value>
		///     The undo command.
		/// </value>
		public ICommand UndoCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [new field].
		/// </summary>
		/// <value>
		///     <c>true</c> if [new field]; otherwise, <c>false</c>.
		/// </value>
		public bool NewField
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the visibility.
		/// </summary>
		/// <value>
		///     The visibility.
		/// </value>
		public EditViewMode EditViewMode
		{
			get
			{
				return _mode;
			}
			set
			{
				SetProperty( ref _mode, value );
			}
		}

		/// <summary>
		///     Gets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		public string Alias
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the field upgrade identifier.
		/// </summary>
		/// <value>
		///     The field upgrade identifier.
		/// </value>
		public Guid FieldUpgradeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [read only].
		/// </summary>
		/// <value>
		///     <c>true</c> if [read only]; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		public long EntityId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the field identifier.
		/// </summary>
		/// <value>
		///     The field identifier.
		/// </value>
		public long FieldId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="IFieldInfo" /> is disabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if disabled; otherwise, <c>false</c>.
		/// </value>
		public bool Disabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		object IFieldInfo.Data
		{
			get
			{
				return Data;
			}
		}

		/// <summary>
		///     Gets the name of the table.
		/// </summary>
		/// <value>
		///     The name of the table.
		/// </value>
		/// <exception cref="System.InvalidOperationException">Needs to be overridden</exception>
		public virtual string TableName
		{
			get
			{
				throw new InvalidOperationException( "Needs to be overridden" );
			}
		}

		/// <summary>
		///     Done click.
		/// </summary>
		protected virtual void DoneClick( )
		{
			Focused = false;

			if ( !NewField )
			{
				EditViewMode = EditViewMode.View;
			}
		}

		/// <summary>
		///     Edits the click.
		/// </summary>
		protected virtual void EditClick( )
		{
			EditViewMode = EditViewMode.Edit;
			Focused = true;
			OldData = Data;
		}

		/// <summary>
		///     Enter press.
		/// </summary>
		private void EnterPress( )
		{
			if ( !NewField )
			{
				EditViewMode = EditViewMode.View;
			}
		}

		/// <summary>
		///     Navigates the click.
		/// </summary>
		private void NavigateClick( )
		{
			EntityNotificationService.NavigateTo( FieldId.ToString( CultureInfo.InvariantCulture ), TenantId );
		}

		/// <summary>
		///     Undoes the click.
		/// </summary>
		protected virtual void UndoClick( TextBox ctrl )
		{
			ctrl.Undo( );
			Focused = false;
			EditViewMode = EditViewMode.View;
			Data = OldData;
		}
	}
}