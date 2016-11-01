// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     The history viewer view model class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class HistoryViewerViewModel : ViewModelBase
	{
		/// <summary>
		///     The data alias
		/// </summary>
		private List<HistAlias> _dataAlias;

		/// <summary>
		///     The data alias visible
		/// </summary>
		private bool _dataAliasVisible;

		/// <summary>
		///     The data bit
		/// </summary>
		private List<HistData> _dataBit;

		/// <summary>
		///     The data bit visible
		/// </summary>
		private bool _dataBitVisible;

		/// <summary>
		///     The data date time
		/// </summary>
		private List<HistData> _dataDateTime;

		/// <summary>
		///     The data date time visible
		/// </summary>
		private bool _dataDateTimeVisible;

		/// <summary>
		///     The data decimal
		/// </summary>
		private List<HistData> _dataDecimal;

		/// <summary>
		///     The data decimal visible
		/// </summary>
		private bool _dataDecimalVisible;

		/// <summary>
		///     The data unique identifier
		/// </summary>
		private List<HistData> _dataGuid;

		/// <summary>
		///     The data unique identifier visible
		/// </summary>
		private bool _dataGuidVisible;

		/// <summary>
		///     The data int
		/// </summary>
		private List<HistData> _dataInt;

		/// <summary>
		///     The data int visible
		/// </summary>
		private bool _dataIntVisible;

		/// <summary>
		///     The data n variable character
		/// </summary>
		private List<HistData> _dataNVarChar;

		/// <summary>
		///     The data n variable character visible
		/// </summary>
		private bool _dataNVarCharVisible;

		/// <summary>
		///     The data XML
		/// </summary>
		private List<HistData> _dataXml;

		/// <summary>
		///     The data XML visible
		/// </summary>
		private bool _dataXmlVisible;

		/// <summary>
		///     The entity
		/// </summary>
		private List<HistEntity> _entity;

		/// <summary>
		///     The entity visible
		/// </summary>
		private bool _entityVisible;

		/// <summary>
		///     The relationship
		/// </summary>
		private List<HistRelationship> _relationship;

		/// <summary>
		/// The selected tab index
		/// </summary>
		private int _selectedTabIndex = -1;

		/// <summary>
		///     The relationship visible
		/// </summary>
		private bool _relationshipVisible;

		/// <summary>
		///     Initializes a new instance of the <see cref="HistoryViewerViewModel" /> class.
		/// </summary>
		/// <param name="transactionId">The transaction identifier.</param>
		/// <param name="settings">The settings.</param>
		public HistoryViewerViewModel( long transactionId, IPluginSettings settings )
		{
			TransactionId = transactionId;
			PluginSettings = settings;

			LoadData( );
		}

		/// <summary>
		/// Gets or sets the index of the selected tab.
		/// </summary>
		/// <value>
		/// The index of the selected tab.
		/// </value>
		public int SelectedTabIndex
		{
			get
			{
				return _selectedTabIndex;
			}
			set
			{
				SetProperty( ref _selectedTabIndex, value );
			}
		}

		/// <summary>
		///     Gets or sets the data alias.
		/// </summary>
		/// <value>
		///     The data alias.
		/// </value>
		public List<HistAlias> DataAlias
		{
			get
			{
				return _dataAlias;
			}
			set
			{
				SetProperty( ref _dataAlias, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data alias visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data alias visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataAliasVisible
		{
			get
			{
				return _dataAliasVisible;
			}
			set
			{
				SetProperty( ref _dataAliasVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data bit.
		/// </summary>
		/// <value>
		///     The data bit.
		/// </value>
		public List<HistData> DataBit
		{
			get
			{
				return _dataBit;
			}
			set
			{
				SetProperty( ref _dataBit, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data bit visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data bit visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataBitVisible
		{
			get
			{
				return _dataBitVisible;
			}
			set
			{
				SetProperty( ref _dataBitVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data date time.
		/// </summary>
		/// <value>
		///     The data date time.
		/// </value>
		public List<HistData> DataDateTime
		{
			get
			{
				return _dataDateTime;
			}
			set
			{
				SetProperty( ref _dataDateTime, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data date time visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data date time visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataDateTimeVisible
		{
			get
			{
				return _dataDateTimeVisible;
			}
			set
			{
				SetProperty( ref _dataDateTimeVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data decimal.
		/// </summary>
		/// <value>
		///     The data decimal.
		/// </value>
		public List<HistData> DataDecimal
		{
			get
			{
				return _dataDecimal;
			}
			set
			{
				SetProperty( ref _dataDecimal, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data decimal visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data decimal visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataDecimalVisible
		{
			get
			{
				return _dataDecimalVisible;
			}
			set
			{
				SetProperty( ref _dataDecimalVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data unique identifier.
		/// </summary>
		/// <value>
		///     The data unique identifier.
		/// </value>
		public List<HistData> DataGuid
		{
			get
			{
				return _dataGuid;
			}
			set
			{
				SetProperty( ref _dataGuid, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data unique identifier visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data unique identifier visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataGuidVisible
		{
			get
			{
				return _dataGuidVisible;
			}
			set
			{
				SetProperty( ref _dataGuidVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data int.
		/// </summary>
		/// <value>
		///     The data int.
		/// </value>
		public List<HistData> DataInt
		{
			get
			{
				return _dataInt;
			}
			set
			{
				SetProperty( ref _dataInt, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data int visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data int visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataIntVisible
		{
			get
			{
				return _dataIntVisible;
			}
			set
			{
				SetProperty( ref _dataIntVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data n variable character.
		/// </summary>
		/// <value>
		///     The data n variable character.
		/// </value>
		public List<HistData> DataNVarChar
		{
			get
			{
				return _dataNVarChar;
			}
			set
			{
				SetProperty( ref _dataNVarChar, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data n variable character visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data n variable character visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataNVarCharVisible
		{
			get
			{
				return _dataNVarCharVisible;
			}
			set
			{
				SetProperty( ref _dataNVarCharVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the data XML.
		/// </summary>
		/// <value>
		///     The data XML.
		/// </value>
		public List<HistData> DataXml
		{
			get
			{
				return _dataXml;
			}
			set
			{
				SetProperty( ref _dataXml, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [data XML visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [data XML visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DataXmlVisible
		{
			get
			{
				return _dataXmlVisible;
			}
			set
			{
				SetProperty( ref _dataXmlVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the entity.
		/// </summary>
		/// <value>
		///     The entity.
		/// </value>
		public List<HistEntity> Entity
		{
			get
			{
				return _entity;
			}
			set
			{
				SetProperty( ref _entity, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [entity visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [entity visible]; otherwise, <c>false</c>.
		/// </value>
		public bool EntityVisible
		{
			get
			{
				return _entityVisible;
			}
			set
			{
				SetProperty( ref _entityVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the relationship.
		/// </summary>
		/// <value>
		///     The relationship.
		/// </value>
		public List<HistRelationship> Relationship
		{
			get
			{
				return _relationship;
			}
			set
			{
				SetProperty( ref _relationship, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [relationship visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [relationship visible]; otherwise, <c>false</c>.
		/// </value>
		public bool RelationshipVisible
		{
			get
			{
				return _relationshipVisible;
			}
			set
			{
				SetProperty( ref _relationshipVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the settings.
		/// </summary>
		/// <value>
		///     The settings.
		/// </value>
		private IPluginSettings PluginSettings
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the transaction identifier.
		/// </summary>
		/// <value>
		///     The transaction identifier.
		/// </value>
		private long TransactionId
		{
			get;
			set;
		}

		/// <summary>
		///     Loads the data.
		/// </summary>
		private void LoadData( )
		{
			LoadEntity( );
			LoadRelationship( );
			LoadDataAlias( );
			LoadData( "Bit" );
			LoadData( "DateTime" );
			LoadData( "Decimal" );
			LoadData( "Guid" );
			LoadData( "Int" );
			LoadData( "NVarChar" );
			LoadData( "Xml" );
		}

		private void LoadData( string tableName )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			string commandText = $@"--ReadiMon - Load Transaction {tableName}
SET NOCOUNT ON

SELECT Action, TenantId, EntityId, FieldId, Data FROM Hist_Data_{tableName}
WHERE TransactionId = @transactionId";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@transactionId", TransactionId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var datas = new List<HistData>( );

						while ( reader.Read( ) )
						{
							var data = new HistData( reader );

							datas.Add( data );
						}

						switch ( tableName )
						{
							case "Bit":
								DataBit = datas;
								DataBitVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataBitVisible )
								{
									SelectedTabIndex = 3;
								}
								break;
							case "DateTime":
								DataDateTime = datas;
								DataDateTimeVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataDateTimeVisible )
								{
									SelectedTabIndex = 4;
								}
								break;
							case "Decimal":
								DataDecimal = datas;
								DataDecimalVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataDecimalVisible )
								{
									SelectedTabIndex = 5;
								}
								break;
							case "Guid":
								DataGuid = datas;
								DataGuidVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataGuidVisible )
								{
									SelectedTabIndex = 6;
								}
								break;
							case "Int":
								DataInt = datas;
								DataIntVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataIntVisible )
								{
									SelectedTabIndex = 7;
								}
								break;
							case "NVarChar":
								DataNVarChar = datas;
								DataNVarCharVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataNVarCharVisible )
								{
									SelectedTabIndex = 8;
								}
								break;
							case "Xml":
								DataXml = datas;
								DataXmlVisible = datas.Count > 0;

								if ( SelectedTabIndex < 0 && DataXmlVisible )
								{
									SelectedTabIndex = 9;
								}
								break;
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		private void LoadDataAlias( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - Load Transaction Alias
SET NOCOUNT ON

SELECT Action, TenantId, EntityId, FieldId, Data, Namespace, AliasMarkerId FROM Hist_Data_Alias
WHERE TransactionId = @transactionId";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@transactionId", TransactionId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var aliases = new List<HistAlias>( );

						while ( reader.Read( ) )
						{
							var alias = new HistAlias( reader );

							aliases.Add( alias );
						}

						DataAlias = aliases;

						DataAliasVisible = aliases.Count > 0;

						if ( SelectedTabIndex < 0 && DataAliasVisible )
						{
							SelectedTabIndex = 2;
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		private void LoadEntity( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - Load Transaction Entities
SET NOCOUNT ON

SELECT Action, TenantId, Id, UpgradeId FROM Hist_Entity
WHERE TransactionId = @transactionId";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@transactionId", TransactionId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var entities = new List<HistEntity>( );

						while ( reader.Read( ) )
						{
							var entity = new HistEntity( reader );

							entities.Add( entity );
						}

						Entity = entities;

						EntityVisible = entities.Count > 0;

						if ( EntityVisible )
						{
							SelectedTabIndex = 0;
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		private void LoadRelationship( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - Load Transaction Relationships
SET NOCOUNT ON

SELECT Action, TenantId, TypeId, FromId, ToId FROM Hist_Relationship
WHERE TransactionId = @transactionId";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@transactionId", TransactionId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var rels = new List<HistRelationship>( );

						while ( reader.Read( ) )
						{
							var rel = new HistRelationship( reader );

							rels.Add( rel );
						}

						Relationship = rels;

						RelationshipVisible = rels.Count > 0;

						if ( SelectedTabIndex < 0 && RelationshipVisible )
						{
							SelectedTabIndex = 1;
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
		///     The hist entity class.
		/// </summary>
		/// <seealso cref="ReadiMon.Plugin.Database.HistoryViewerViewModel.HistBase" />
		public class HistEntity : HistBase
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="HistEntity" /> class.
			/// </summary>
			/// <param name="reader">The reader.</param>
			public HistEntity( IDataReader reader ) : base( reader )
			{
				Id = reader.GetInt64( 2 );
				UpgradeId = reader.GetGuid( 3 );
			}

			/// <summary>
			///     Gets or sets the identifier.
			/// </summary>
			/// <value>
			///     The identifier.
			/// </value>
			public long Id
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the upgrade identifier.
			/// </summary>
			/// <value>
			///     The upgrade identifier.
			/// </value>
			public Guid UpgradeId
			{
				get;
				set;
			}
		}

		/// <summary>
		///     The hist relationship class.
		/// </summary>
		/// <seealso cref="ReadiMon.Plugin.Database.HistoryViewerViewModel.HistBase" />
		public class HistRelationship : HistBase
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="HistRelationship" /> class.
			/// </summary>
			/// <param name="reader">The reader.</param>
			public HistRelationship( IDataReader reader ) : base( reader )
			{
				TypeId = reader.GetInt64( 2 );
				FromId = reader.GetInt64( 3 );
				ToId = reader.GetInt64( 4 );
			}

			/// <summary>
			///     Gets or sets from identifier.
			/// </summary>
			/// <value>
			///     From identifier.
			/// </value>
			public long FromId
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets to identifier.
			/// </summary>
			/// <value>
			///     To identifier.
			/// </value>
			public long ToId
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the type identifier.
			/// </summary>
			/// <value>
			///     The type identifier.
			/// </value>
			public long TypeId
			{
				get;
				set;
			}
		}

		/// <summary>
		///     The hist data class.
		/// </summary>
		/// <seealso cref="ReadiMon.Plugin.Database.HistoryViewerViewModel.HistBase" />
		public class HistData : HistBase
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="HistData" /> class.
			/// </summary>
			/// <param name="reader">The reader.</param>
			public HistData( IDataReader reader ) : base( reader )
			{
				EntityId = reader.GetInt64( 2 );
				FieldId = reader.GetInt64( 3 );
				Data = reader.GetValue( 4 ).ToString( );
			}

			/// <summary>
			///     Gets or sets the data.
			/// </summary>
			/// <value>
			///     The data.
			/// </value>
			public string Data
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the entity identifier.
			/// </summary>
			/// <value>
			///     The entity identifier.
			/// </value>
			public long EntityId
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the field identifier.
			/// </summary>
			/// <value>
			///     The field identifier.
			/// </value>
			public long FieldId
			{
				get;
				set;
			}
		}

		/// <summary>
		///     The hist alias class.
		/// </summary>
		/// <seealso cref="string" />
		public class HistAlias : HistData
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="HistAlias" /> class.
			/// </summary>
			/// <param name="reader">The reader.</param>
			public HistAlias( IDataReader reader ) : base( reader )
			{
				Namespace = reader.GetString( 5 );
				AliasMarkerId = reader.GetInt32( 6 );
			}

			/// <summary>
			///     Gets or sets the alias marker identifier.
			/// </summary>
			/// <value>
			///     The alias marker identifier.
			/// </value>
			public int AliasMarkerId
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the namespace.
			/// </summary>
			/// <value>
			///     The namespace.
			/// </value>
			public string Namespace
			{
				get;
				set;
			}
		}

		/// <summary>
		///     The hist base class.
		/// </summary>
		public class HistBase
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="HistBase" /> class.
			/// </summary>
			/// <param name="reader">The reader.</param>
			public HistBase( IDataReader reader )
			{
				Action = reader.GetByte( 0 ) == 0 ? "Deleted" : "Added";
				TenantId = reader.GetInt64( 1 );
			}

			/// <summary>
			///     Gets or sets the action.
			/// </summary>
			/// <value>
			///     The action.
			/// </value>
			public string Action
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the tenant identifier.
			/// </summary>
			/// <value>
			///     The tenant identifier.
			/// </value>
			public long TenantId
			{
				get;
				set;
			}
		}
	}
}