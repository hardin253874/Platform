// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Test
{
	/// <summary>
	///     App Details.
	/// </summary>
	public class AppDetails
	{
		public static Guid AdministratorUserAccountUid = new Guid( "d223c5fb-1e3f-4fba-b45c-2df451eb85c7" );
		public static Guid AppPackageTypeUid = new Guid( "AF6C9DEE-DB41-4C19-91D1-008B966CBB38" );
		public static Guid AppTypeUid = new Guid( "46B58C2E-2EFB-4EF3-9275-8BFDF99FAAE1" );
		public static Guid AppVerTypeUid = new Guid( "6efb1485-9862-4630-bda9-3c34ec4cd091" );
		public static Guid AppVersionStringUid = new Guid( "AAD0F45F-8C60-47C4-9B13-2F7158CC6078" );
		public static Guid ApplicationIdUid = new Guid( "3D6B891F-6F7C-45AB-9CAE-503C5DE79972" );
		public static Guid CardinalityUid = new Guid( "23b14828-dcd7-432e-b8c1-d23ce6df16c0" );
		public static Guid CascadeDeleteToUid = new Guid( "6b0e7501-0e23-4073-8370-54b97d022d9b" );
		public static Guid CascadeDeleteUid = new Guid( "44adfff2-b654-487d-87f1-92d63be8be38" );
		public static Guid CloneActionUid = new Guid( "830f4a8d-6e51-44ad-a543-7dd26a5333cd" );
		public static Guid ConsoleOrderUid = new Guid( "ea15d71a-bc34-4b57-82dc-37346d9b7735" );
		public static Guid DefinitionUid = new Guid( "22f52cc0-ecc7-49c6-9f71-2500fb832579" );
		public static Guid DescriptionUid = new Guid( "A6907C5A-19DB-48CA-B9BE-85D81CD9081F" );
		public static Guid FromTypeTypeUid = new Guid( "3d841747-0690-4832-9ba2-5116afef4032" );
		public static Guid HideOnDesktopUid = new Guid( "b884b515-5157-465a-819f-ab5aca7b24d2" );
		public static Guid HideOnMobileUid = new Guid( "de601ccc-cf41-4d6d-8c60-0d48d6bf31e1" );
		public static Guid HideOnTabletUid = new Guid( "fa429df1-47a7-464e-9a48-0f904acb3afc" );
		public static Guid ImplicitInSolutionUid = new Guid( "cd706403-83dc-44d7-8445-2ae956c76088" );
		public static Guid InDirectInSolutionUid = new Guid( "54e16d01-c53f-407f-add8-4c906b6ca5dc" );
		public static Guid InSolutionUid = new Guid( "7c77c3a0-75b5-4c59-99f6-3ba9229e6a55" );
		public static Guid InheritsUid = new Guid( "50fe1b39-bb32-4825-8ab5-807a7df9b5b8" );
		public static Guid IsAppTabUid = new Guid( "96263B3C-8F97-483F-A2AD-B7C86ED728EA" );
		public static Guid IsOfTypeUid = new Guid( "e1afc9e2-a526-4dc6-b90f-e2271e130f24" );
		public static Guid IsTopMenuVisibleUid = new Guid( "2702d98e-bae9-438b-ae1a-25fbf652f0f5" );
		public static Guid ManyToManyUid = new Guid( "8DAEA64B-3BAC-42A9-ABB1-D223EDB938DA" );
		public static Guid ManyToOneUid = new Guid( "206CB34F-A006-43E5-BFCB-AD1FD28067C8" );
		public static Guid NameUid = new Guid( "f8def406-90a1-4580-94f4-1b08beac87af" );
		public static Guid NavSectionTypeUid = new Guid( "1baec7ee-bf1c-464a-9acc-95938b8bdb26" );
		public static Guid OneToManyUid = new Guid( "455106C7-19F1-4AB7-8329-5CFA8F3D7F7C" );
		public static Guid OneToOneUid = new Guid( "23fcdeca-0731-4e2e-907e-800fa8b64efe" );
		public static Guid PackageForApplicationUid = new Guid( "31BCA990-4675-4F62-A1D2-E217A4AD5CA8" );
		public static Guid PackageIdUid = new Guid( "1670eb43-6568-4e41-97c4-71b2bd1fba61" );
		public static Guid RelationshipTypeUid = new Guid( "82716909-4456-4dfd-a2c6-335c909c0f30" );
		public static Guid ResourceInFolderUid = new Guid( "989e4a92-e177-480b-8df4-9a997ac0c62d" );
		public static Guid ResourceTypeUid = new Guid( "69224903-ca05-48cb-a13d-786c49f08e18" );
		public static Guid ReverseCloneActionUid = new Guid( "d2cfc438-e7d6-4cd7-8eeb-eb753d094254" );
		public static Guid ReverseImplicitInSolutionUid = new Guid( "e039f615-ba3c-44ed-962c-e3a43aa5ef61" );
		public static Guid SecurityOwnerUid = new Guid( "a715539d-0425-4488-a950-54dca3d88925" );
		public static Guid SolutionTypeUid = new Guid( "f6ebea8a-0989-4ae0-be53-5d6541cb0908" );
		public static Guid SolutionVersionString = new Guid( "3b9d1ad1-f3ef-45ae-8ebb-78a5fb2f2ad9" );
		public static Guid ToTypeTypeUid = new Guid( "f96303ce-6ec6-4e56-8c7a-ad740a350f7a" );
		public static Guid TopMenuTypeUid = new Guid( "ef71e8f0-3cc8-4da6-86d3-a66c06d1d52b" );
		public static Guid TypeUid = new Guid( "c0c6de86-2c65-47d2-bd5d-1751eed77378" );
		public static Guid UserResourceTypeUid = new Guid( "401A7EFD-BEAE-41EE-A179-AE8846685F04" );

		/// <summary>
		///     Initializes a new instance of the <see cref="AppDetails" /> class.
		/// </summary>
		public AppDetails( )
		{
			AppVerUid = Guid.NewGuid( );
			NavSectionUid = Guid.NewGuid( );
			TopMenuUid = Guid.NewGuid( );
			SolutionUid = Guid.NewGuid( );
			FromTypeUid = Guid.NewGuid( );
			ToTypeUid = Guid.NewGuid( );
			RelationshipUid = Guid.NewGuid( );
			FromInstanceUid = Guid.NewGuid( );
			ToInstanceUid = Guid.NewGuid( );

			AppPackageUid = Guid.NewGuid( );
			AppUid = Guid.NewGuid( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="AppDetails" /> class.
		/// </summary>
		/// <param name="data">The data.</param>
		public AppDetails( DataSet data ) : this( )
		{
			Data = data;

			Create( );
		}

		/// <summary>
		///     Gets the application package upgrade id.
		/// </summary>
		/// <value>
		///     The application package upgrade id.
		/// </value>
		public Guid AppPackageUid
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the application upgrade id.
		/// </summary>
		/// <value>
		///     The application upgrade id.
		/// </value>
		public Guid AppUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the application version upgrade id.
		/// </summary>
		/// <value>
		///     The application version upgrade id.
		/// </value>
		public Guid AppVerUid
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
		public DataSet Data
		{
			get;
			set;
		}

		/// <summary>
		///     Gets from instance upgrade id.
		/// </summary>
		/// <value>
		///     From instance upgrade id.
		/// </value>
		public Guid FromInstanceUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets from type upgrade id.
		/// </summary>
		/// <value>
		///     From type upgrade id.
		/// </value>
		public Guid FromTypeUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the navigation section upgrade id.
		/// </summary>
		/// <value>
		///     The navigation section upgrade id.
		/// </value>
		public Guid NavSectionUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationship upgrade id.
		/// </summary>
		/// <value>
		///     The relationship upgrade id.
		/// </value>
		public Guid RelationshipUid
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="AppDetails" /> is saved.
		/// </summary>
		/// <value>
		///     <c>true</c> if saved; otherwise, <c>false</c>.
		/// </value>
		public bool Saved
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the solution upgrade id.
		/// </summary>
		/// <value>
		///     The solution upgrade id.
		/// </value>
		public Guid SolutionUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the tenant upgrade identifier to entity identifier map.
		/// </summary>
		/// <value>
		///     The tenant upgrade identifier to entity identifier map.
		/// </value>
		private static Dictionary<long, Dictionary<Guid, long>> TenantUpgradeIdToEntityIdMap
		{
			get;
			set;
		}

		/// <summary>
		///     Gets to instance upgrade id.
		/// </summary>
		/// <value>
		///     To instance upgrade id.
		/// </value>
		public Guid ToInstanceUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets to type upgrade id.
		/// </summary>
		/// <value>
		///     To type upgrade id.
		/// </value>
		public Guid ToTypeUid
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the top menu upgrade id.
		/// </summary>
		/// <value>
		///     The top menu upgrade id.
		/// </value>
		public Guid TopMenuUid
		{
			get;
			private set;
		}

		/// <summary>
		/// Sets the cardinality.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		public void SetCardinality( CardinalityEnum_Enumeration cardinality )
		{
			DataTable appRelationship = Data.Tables[ TableNames.AppRelationship ];

			var cardinalityRow = appRelationship.Rows.Find( new object[ ]
			{
				AppVerUid,
				CardinalityUid,
				RelationshipUid,
				OneToOneUid
			} );

			if ( cardinalityRow != null )
			{
				cardinalityRow[ "ToUid" ] = ConvertCardinalityEnumToUpgradeId( cardinality );
			}
		}

		/// <summary>
		/// Sets the version.
		/// </summary>
		/// <param name="version">The version.</param>
		public void SetVersion( string version )
		{
			DataTable appDataNVarChar = Data.Tables [ TableNames.AppDataNVarChar ];

			DataRow versionRow = appDataNVarChar.Rows.Find( new object [ ]
			{
				AppVerUid,
				SolutionUid,
				SolutionVersionString
			} );

			if ( versionRow != null )
			{
				versionRow [ "Data" ] = version;
			}
		}

		/// <summary>
		///     Converts the cardinality enumeration to upgrade identifier.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Invalid cardinality value</exception>
		public static Guid ConvertCardinalityEnumToUpgradeId( CardinalityEnum_Enumeration cardinality )
		{
			switch ( cardinality )
			{
				case CardinalityEnum_Enumeration.OneToOne:
					return OneToOneUid;
				case CardinalityEnum_Enumeration.OneToMany:
					return OneToManyUid;
				case CardinalityEnum_Enumeration.ManyToOne:
					return ManyToOneUid;
				case CardinalityEnum_Enumeration.ManyToMany:
					return ManyToManyUid;
				default:
					throw new ArgumentException( "Invalid cardinality value" );
			}
		}

		/// <summary>
		///     Creates this instance.
		/// </summary>
		internal void Create( )
		{
			DataTable appEntityTable = Data.Tables[ TableNames.AppEntity ];

			CreateAppEntityRow( appEntityTable, AppVerUid, NavSectionUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, TopMenuUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, SolutionUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, FromTypeUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, ToTypeUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, RelationshipUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, FromInstanceUid );
			CreateAppEntityRow( appEntityTable, AppVerUid, ToInstanceUid );

			DataTable appDataBit = Data.Tables[ TableNames.AppDataBit ];

			CreateAppDataRow( appDataBit, AppVerUid, NavSectionUid, IsAppTabUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, TopMenuUid, HideOnMobileUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, TopMenuUid, HideOnTabletUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, TopMenuUid, IsTopMenuVisibleUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, TopMenuUid, HideOnDesktopUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, SolutionUid, HideOnMobileUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, SolutionUid, HideOnTabletUid, true );
			CreateAppDataRow( appDataBit, AppVerUid, SolutionUid, HideOnDesktopUid, true );

			DataTable appDataGuid = Data.Tables[ TableNames.AppDataGuid ];

			CreateAppDataRow( appDataGuid, AppVerUid, SolutionUid, PackageIdUid, Guid.NewGuid( ) );

			DataTable appDataInt = Data.Tables[ TableNames.AppDataInt ];

			CreateAppDataRow( appDataInt, AppVerUid, TopMenuUid, ConsoleOrderUid, 11 );

			DataTable appDataNVarChar = Data.Tables[ TableNames.AppDataNVarChar ];

			CreateAppDataRow( appDataNVarChar, AppVerUid, NavSectionUid, NameUid, "UnitTest Application" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, TopMenuUid, NameUid, "UnitTest Application" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, SolutionUid, NameUid, "UnitTest Application" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, SolutionUid, SolutionVersionString, "1.0" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, FromTypeUid, NameUid, "UnitTest From Type" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, ToTypeUid, NameUid, "UnitTest To Type" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, RelationshipUid, NameUid, "UnitTest Relationship" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, FromInstanceUid, NameUid, "UnitTest From Instance" );
			CreateAppDataRow( appDataNVarChar, AppVerUid, ToInstanceUid, NameUid, "UnitTest To Instance" );

			DataTable appRelationship = Data.Tables[ TableNames.AppRelationship ];

			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, NavSectionUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, TopMenuUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, SolutionUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, NavSectionUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, TopMenuUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, SolutionUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, ResourceInFolderUid, NavSectionUid, TopMenuUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, NavSectionUid, NavSectionTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, TopMenuUid, TopMenuTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, SolutionUid, SolutionTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, FromTypeUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, FromTypeUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InheritsUid, FromTypeUid, UserResourceTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, FromTypeUid, DefinitionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, ToTypeUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, ToTypeUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InheritsUid, ToTypeUid, UserResourceTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, ToTypeUid, DefinitionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, RelationshipUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, FromTypeTypeUid, RelationshipUid, FromTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, RelationshipUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InheritsUid, RelationshipUid, ResourceTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, ToTypeTypeUid, RelationshipUid, ToTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, CardinalityUid, RelationshipUid, OneToOneUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, RelationshipUid, RelationshipTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, FromInstanceUid, FromTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, FromInstanceUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, FromInstanceUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, ToInstanceUid, ToTypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, ToInstanceUid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, ToInstanceUid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, RelationshipUid, FromInstanceUid, ToInstanceUid );

			DataTable entity = Data.Tables[ TableNames.Entity ];

			CreateEntityRow( entity, 0, AppPackageUid );
			CreateEntityRow( entity, 0, AppUid );
		}

		/// <summary>
		/// Creates the instance.
		/// </summary>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <returns></returns>
		public Guid CreateInstance( Guid typeUid, string name = null )
		{
			var guid = Guid.NewGuid( );

			DataTable appEntity = Data.Tables [ TableNames.AppEntity ];

			CreateAppEntityRow( appEntity, AppVerUid, guid );

			if ( name != null )
			{
				DataTable appNvarChar = Data.Tables [ TableNames.AppDataNVarChar ];
				CreateAppDataRow( appNvarChar, AppVerUid, guid, NameUid, name );
			}

			DataTable appRelationship = Data.Tables [ TableNames.AppRelationship ];

			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, guid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, guid, typeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, guid, AdministratorUserAccountUid );

			return guid;
		}

		/// <summary>
		/// Creates the type.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public Guid CreateType( string name )
		{
			var guid = Guid.NewGuid( );

			DataTable appEntity = Data.Tables [ TableNames.AppEntity ];
			CreateAppEntityRow( appEntity, AppVerUid, guid );

			DataTable appNvarChar = Data.Tables [ TableNames.AppDataNVarChar ];
			CreateAppDataRow( appNvarChar, AppVerUid, guid, NameUid, name );

			DataTable appRelationship = Data.Tables [ TableNames.AppRelationship ];
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, guid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, guid, TypeUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, guid, AdministratorUserAccountUid );

			return guid;
		}

		/// <summary>
		/// Changes the relationship cardinality.
		/// </summary>
		/// <param name="relationshipUid">The relationship uid.</param>
		/// <param name="oldCardinality">The old cardinality.</param>
		/// <param name="newCardinality">The new cardinality.</param>
		public void ChangeRelationshipCardinality( Guid relationshipUid, Guid oldCardinality, Guid newCardinality )
		{
			DataTable appRelationship = Data.Tables [ TableNames.AppRelationship ];

			DeleteRelationship( CardinalityUid, relationshipUid, oldCardinality );
			CreateAppRelationshipRow( appRelationship, AppVerUid, CardinalityUid, relationshipUid, newCardinality );
		}

		/// <summary>
		/// Creates the relationship.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="fromUid">From uid.</param>
		/// <param name="toUid">To uid.</param>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="cascadeDelete">if set to <c>true</c> [cascade delete].</param>
		/// <param name="cascadeDeleteTo">if set to <c>true</c> [cascade delete to].</param>
		/// <returns></returns>
		public Guid CreateRelationship( string name, Guid fromUid, Guid toUid, Guid cardinality, bool cascadeDelete, bool cascadeDeleteTo )
		{
			var guid = Guid.NewGuid( );

			DataTable appEntity = Data.Tables [ TableNames.AppEntity ];
			CreateAppEntityRow( appEntity, AppVerUid, guid );

			DataTable appNvarChar = Data.Tables [ TableNames.AppDataNVarChar ];
			CreateAppDataRow( appNvarChar, AppVerUid, guid, NameUid, name );

			DataTable appRelationship = Data.Tables [ TableNames.AppRelationship ];
			CreateAppRelationshipRow( appRelationship, AppVerUid, InSolutionUid, guid, SolutionUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, IsOfTypeUid, guid, RelationshipUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, SecurityOwnerUid, guid, AdministratorUserAccountUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, FromTypeTypeUid, guid, fromUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, ToTypeTypeUid, guid, toUid );
			CreateAppRelationshipRow( appRelationship, AppVerUid, CardinalityUid, guid, cardinality );

			if ( cascadeDelete || cascadeDeleteTo )
			{
				DataTable appBit = Data.Tables [ TableNames.AppDataBit ];

				if ( cascadeDelete )
				{
					CreateAppDataRow( appBit, AppVerUid, guid, CascadeDeleteUid, true );
				}

				if ( cascadeDeleteTo )
				{
					CreateAppDataRow( appBit, AppVerUid, guid, CascadeDeleteToUid, true );
				}
			}

			return guid;
		}

		/// <summary>
		/// Deletes the entity.
		/// </summary>
		/// <param name="entityUid">The entity uid.</param>
		public void DeleteEntity( Guid entityUid )
		{
			foreach ( DataTable table in Data.Tables )
			{
				for ( int i = 0; i < table.Rows.Count; i++ )
				{
					DataRow row = table.Rows[ i ];

					foreach ( DataColumn column in table.Columns )
					{
						if ( column.DataType == typeof( Guid ) )
						{
							object colVal = row [ column ];

							if ( colVal != null && colVal != DBNull.Value )
							{
								if ( ( ( Guid ) row [ column ] ) == entityUid )
								{
									table.Rows.Remove( row );
									i--;
									break;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Deletes the relationship.
		/// </summary>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <param name="fromUid">From upgrade id.</param>
		/// <param name="toUid">To upgrade id.</param>
		public void DeleteRelationship( Guid typeUid, Guid fromUid, Guid toUid )
		{
			DataTable table = Data.Tables[ TableNames.AppRelationship ];

			var row = table.Rows.Find( new object [ ]
			{
				AppVerUid,
				typeUid,
				fromUid,
				toUid
			} );

			if ( row != null )
			{
				table.Rows.Remove( row );
			}
		}

		/// <summary>
		/// Sets the field value.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="entityUid">The entity uid.</param>
		/// <param name="fieldUid">The field uid.</param>
		/// <param name="value">The value.</param>
		public void SetFieldValue( string tableName, Guid entityUid, Guid fieldUid, object value )
		{
			DataTable table = Data.Tables [ tableName ];

			var row = table.Rows.Find( new object [ ]
			{
				AppVerUid,
				entityUid,
				fieldUid
			} );

			if ( row != null )
			{
				row[ "Data" ] = value;
			}
		}

		/// <summary>
		///     Creates the application data row.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="appVerUid">The application version upgrade id.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		/// <param name="fieldUid">The field upgrade id.</param>
		/// <param name="data">The data.</param>
		public static void CreateAppDataRow( DataTable dataTable, Guid appVerUid, Guid entityUid, Guid fieldUid, object data )
		{
			var row = dataTable.NewRow( );

			row[ "AppVerUid" ] = appVerUid;
			row[ "EntityUid" ] = entityUid;
			row[ "FieldUid" ] = fieldUid;
			row[ "Data" ] = data;

			dataTable.Rows.Add( row );
		}

		/// <summary>
		///     Creates the application data row.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="appVerUid">The application version upgrade id.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		/// <param name="fieldUid">The field upgrade id.</param>
		/// <param name="data">The data.</param>
		public void CreateAppDataRow( string tableName, Guid appVerUid, Guid entityUid, Guid fieldUid, object data )
		{
			DataTable table = Data.Tables[ tableName ];

			CreateAppDataRow( table, appVerUid, entityUid, fieldUid, data );
		}

		/// <summary>
		///     Creates the application entity row.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="appVerUid">The application version upgrade id.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		public static void CreateAppEntityRow( DataTable dataTable, Guid appVerUid, Guid entityUid )
		{
			var row = dataTable.NewRow( );

			row[ "AppVerUid" ] = appVerUid;
			row[ "EntityUid" ] = entityUid;

			dataTable.Rows.Add( row );
		}

		/// <summary>
		///     Creates the application entity row.
		/// </summary>
		/// <param name="appVerUid">The application version upgrade id.</param>
		/// <param name="entityUid">The entity upgrade id.</param>
		public void CreateAppEntityRow( Guid appVerUid, Guid entityUid )
		{
			DataTable table = Data.Tables[ TableNames.AppEntity ];

			CreateAppEntityRow( table, appVerUid, entityUid );
		}

		/// <summary>
		///     Creates the application relationship row.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="appVerUid">The application version upgrade id.</param>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <param name="fromUid">From upgrade id.</param>
		/// <param name="toUid">To upgrade id.</param>
		public static void CreateAppRelationshipRow( DataTable dataTable, Guid appVerUid, Guid typeUid, Guid fromUid, Guid toUid )
		{
			var row = dataTable.NewRow( );

			row[ "AppVerUid" ] = appVerUid;
			row[ "TypeUid" ] = typeUid;
			row[ "FromUid" ] = fromUid;
			row[ "ToUid" ] = toUid;

			dataTable.Rows.Add( row );
		}

		/// <summary>
		///     Creates the application relationship row.
		/// </summary>
		/// <param name="appVerUid">The application version upgrade id.</param>
		/// <param name="typeUid">The type upgrade id.</param>
		/// <param name="fromUid">From upgrade id.</param>
		/// <param name="toUid">To upgrade id.</param>
		public void CreateAppRelationshipRow( Guid appVerUid, Guid typeUid, Guid fromUid, Guid toUid )
		{
			DataTable table = Data.Tables[ TableNames.AppRelationship ];

			CreateAppRelationshipRow( table, appVerUid, typeUid, fromUid, toUid );
		}

		/// <summary>
		///     Creates the data row.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		public static void CreateDataRow( DataTable dataTable, long tenantId, long entityId, long fieldId, object data )
		{
			var row = dataTable.NewRow( );

			row[ "TenantId" ] = tenantId;
			row[ "EntityId" ] = entityId;
			row[ "FieldId" ] = fieldId;
			row[ "Data" ] = data;

			dataTable.Rows.Add( row );
		}

		/// <summary>
		///     Creates the data row.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		public void CreateDataRow( string tableName, long tenantId, long entityId, long fieldId, object data )
		{
			DataTable table = Data.Tables[ tableName ];

			CreateDataRow( table, tenantId, entityId, fieldId, data );
		}

		/// <summary>
		///     Creates the entity row.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="upgradeId">The upgrade identifier.</param>
		public static void CreateEntityRow( DataTable dataTable, long tenantId, Guid upgradeId )
		{
			var row = dataTable.NewRow( );

			row[ "TenantId" ] = tenantId;
			row[ "UpgradeId" ] = upgradeId;

			dataTable.Rows.Add( row );
		}

		/// <summary>
		///     Creates the entity row.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="upgradeId">The upgrade identifier.</param>
		public void CreateEntityRow( long tenantId, Guid upgradeId )
		{
			DataTable table = Data.Tables[ TableNames.Entity ];

			CreateEntityRow( table, tenantId, upgradeId );
		}

		/// <summary>
		///     Creates the relationship row.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		public static void CreateRelationshipRow( DataTable dataTable, long tenantId, long typeId, long fromId, long toId )
		{
			var row = dataTable.NewRow( );

			row[ "TenantId" ] = tenantId;
			row[ "TypeId" ] = typeId;
			row[ "FromId" ] = fromId;
			row[ "ToId" ] = toId;

			dataTable.Rows.Add( row );
		}

		/// <summary>
		///     Creates the relationship row.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		public void CreateRelationshipRow( long tenantId, long typeId, long fromId, long toId )
		{
			DataTable table = Data.Tables[ TableNames.Relationship ];

			CreateRelationshipRow( table, tenantId, typeId, fromId, toId );
		}

		/// <summary>
		///     Flushes the tenant data.
		/// </summary>
		internal void FlushTenantData( )
		{
			const long tenantId = 0;

			if ( Saved )
			{
				DataTable table = Data.Tables[ TableNames.Entity ];
				table.Rows.Clear( );
				CreateEntityRow( table, tenantId, AppPackageUid );

				table = Data.Tables[ TableNames.DataGuid ];
				table.Rows.Clear( );
				table = Data.Tables[ TableNames.DataNVarChar ];
				table.Rows.Clear( );
				table = Data.Tables[ TableNames.Relationship ];
				table.Rows.Clear( );
			}
		}

		/// <summary>
		///     Gets the tenant entity identifier from upgrade identifier.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <returns></returns>
		public static long GetTenantEntityIdFromUpgradeId( long tenantId, Guid upgradeId )
		{
			if ( TenantUpgradeIdToEntityIdMap == null )
			{
				TenantUpgradeIdToEntityIdMap = new Dictionary<long, Dictionary<Guid, long>>( );
			}

			Dictionary<Guid, long> tenantMap;

			if ( !TenantUpgradeIdToEntityIdMap.TryGetValue( tenantId, out tenantMap ) )
			{
				tenantMap = new Dictionary<Guid, long>( );
				TenantUpgradeIdToEntityIdMap[ tenantId ] = tenantMap;
			}

			long entityId;

			if ( !tenantMap.TryGetValue( upgradeId, out entityId ) )
			{
				using ( DatabaseContext context = DatabaseContext.GetContext( ) )
				{
					var command = context.CreateCommand( string.Format( @"SELECT Id FROM Entity WHERE TenantId = {0} AND UpgradeId = '{1}'", tenantId, upgradeId ) );

					var id = command.ExecuteScalar( );

					if ( id == null || id == DBNull.Value )
					{
						throw new ArgumentException( "Unable to find specified upgrade id." );
					}

					entityId = ( long ) id;

					tenantMap[ upgradeId ] = entityId;
				}
			}

			return entityId;
		}

		/// <summary>
		///     Populates the tenant data.
		/// </summary>
		internal void PopulateTenantData( )
		{
			const long tenantId = 0;

			DataTable dataGuid = Data.Tables[ TableNames.DataGuid ];

			CreateDataRow( dataGuid, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppPackageUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppVerTypeUid ), AppVerUid );

			if ( !Saved )
			{
				CreateDataRow( dataGuid, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppUid ), GetTenantEntityIdFromUpgradeId( tenantId, ApplicationIdUid ), SolutionUid );
			}

			DataTable dataNVarChar = Data.Tables[ TableNames.DataNVarChar ];

			CreateDataRow( dataNVarChar, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppPackageUid ), GetTenantEntityIdFromUpgradeId( tenantId, NameUid ), "UnitTest Application Package 1.0" );
			CreateDataRow( dataNVarChar, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppPackageUid ), GetTenantEntityIdFromUpgradeId( tenantId, DescriptionUid ), "Application Package for version 1.0 of UnitTest Application." );
			CreateDataRow( dataNVarChar, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppPackageUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppVersionStringUid ), "1.0" );

			if ( !Saved )
			{
				CreateDataRow( dataNVarChar, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppUid ), GetTenantEntityIdFromUpgradeId( tenantId, NameUid ), "UnitTest Application" );
				CreateDataRow( dataNVarChar, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, AppUid ), GetTenantEntityIdFromUpgradeId( tenantId, DescriptionUid ), "Application Package for version 1.0 of UnitTest Application." );
			}

			DataTable relationship = Data.Tables[ TableNames.Relationship ];

			CreateRelationshipRow( relationship, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, PackageForApplicationUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppPackageUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppUid ) );
			CreateRelationshipRow( relationship, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, IsOfTypeUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppPackageUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppPackageTypeUid ) );

			if ( ! Saved )
			{
				CreateRelationshipRow( relationship, tenantId, GetTenantEntityIdFromUpgradeId( tenantId, IsOfTypeUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppUid ), GetTenantEntityIdFromUpgradeId( tenantId, AppTypeUid ) );
			}
		}

		/// <summary>
		///     Replaces the upgrade identifier.
		/// </summary>
		/// <param name="oldUpgradeId">The old upgrade identifier.</param>
		/// <param name="newUpgradeId">The new upgrade identifier.</param>
		internal void ReplaceUpgradeId( Guid oldUpgradeId, Guid newUpgradeId )
		{
			foreach ( DataTable table in Data.Tables )
			{
				foreach ( DataRow row in table.Rows )
				{
					foreach ( DataColumn column in table.Columns )
					{
						if ( column.DataType == typeof ( Guid ) )
						{
							object colVal = row[ column ];

							if ( colVal != null && colVal != DBNull.Value )
							{
								if ( ( ( Guid ) row[ column ] ) == oldUpgradeId )
								{
									row[ column ] = newUpgradeId;
								}
							}
						}
					}
				}
			}
		}
	}
}