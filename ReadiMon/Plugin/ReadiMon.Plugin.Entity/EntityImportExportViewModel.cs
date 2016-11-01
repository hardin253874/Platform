// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using ReadiMon.Shared;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Model;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Entity Import/Export view model.
	/// </summary>
	public class EntityImportExportViewModel : EntityViewModel
	{
		/// <summary>
		///     The initial text
		/// </summary>
		private const string InitialText = @"<readiMon version=""1.0.0.0"">
	<entityImportExport>
		<description>

			This is the Entity Xml Import/Export tool.

		</description>
	</entityImportExport>
</readiMon>";

		/// <summary>
		///     The changed action
		/// </summary>
		private readonly Action _changedAction;

		/// <summary>
		///     The import export text
		/// </summary>
		private string _importExportText;

		/// <summary>
		///     The loading flag.
		/// </summary>
		private bool _loading;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityImportExportViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="changedAction">The changed action.</param>
		public EntityImportExportViewModel( IPluginSettings settings, Action changedAction ) : base( settings )
		{
			_changedAction = changedAction;

			ImportExportText = InitialText;
		}

		/// <summary>
		///     Gets or sets the import export text.
		/// </summary>
		/// <value>
		///     The import export text.
		/// </value>
		public string ImportExportText
		{
			get
			{
				return _importExportText;
			}
			set
			{
				if ( value != InitialText )
				{
					_changedAction( );
				}

				SetProperty( ref _importExportText, value );
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

				Export( value );
			}
		}

		/// <summary>
		///     Exports this instance.
		/// </summary>
		private void Export( string identifier, bool enforceTenantId = false )
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
								}
							}
						}

						valid = id >= 0 && GetEntityById( id, enforceTenantId );

						SelectedEntityId = id;
					}

					if ( !valid )
					{
						SelectedEntityId = -1;
					}
				}
				finally
				{
					_loading = false;
				}
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

			const string aliasQuery = @"
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
	d.EntityId = @id";

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

			List<ForwardRelationship> forwardRelationships = GetForwardRelationships( id );
			List<ReverseRelationship> reverseRelationships = GetReverseRelationships( id );

			var settings = new XmlWriterSettings
			{
				Indent = true,
				ConformanceLevel = ConformanceLevel.Fragment,
				OmitXmlDeclaration = true
			};

			bool includeLongs = Settings.Default.IncludeLongs;
			bool includeComments = Settings.Default.IncludeComments;

			var builder = new StringBuilder( );

			using ( XmlWriter writer = XmlWriter.Create( builder, settings ) )
			{
				writer.WriteStartElement( "entity" );

				if ( includeLongs )
				{
					writer.WriteAttributeString( "id", id.ToString( CultureInfo.InvariantCulture ) );
				}

				writer.WriteAttributeString( "upgradeId", upgradeId.ToString( "B" ) );

				writer.WriteStartElement( "fields" );

				foreach ( IFieldInfo field in fields )
				{
					if ( field.FieldUpgradeId == Guid.Empty )
					{
						continue;
					}

					if ( !string.IsNullOrEmpty( field.Alias ) && includeComments )
					{
						writer.WriteComment( string.Format( " {0} ", field.Alias ) );
					}

					writer.WriteStartElement( "field" );

					if ( includeLongs )
					{
						writer.WriteAttributeString( "id", field.FieldId.ToString( CultureInfo.InvariantCulture ) );
					}

					writer.WriteAttributeString( "upgradeId", field.FieldUpgradeId.ToString( "B" ) );

					writer.WriteString( field.Data.ToString( ) );

					writer.WriteEndElement( ); // field
				}

				writer.WriteEndElement( ); // fields

				writer.WriteStartElement( "forwardRelationships" );

				var fwdGroupBy = forwardRelationships.GroupBy( fr => fr.TypeUpgradeId );

				foreach ( IGrouping<Guid, ForwardRelationship> group in fwdGroupBy )
				{
					if ( group.Any( ) )
					{
						var relationship = @group.FirstOrDefault( );

						if ( relationship != null && includeComments )
						{
							writer.WriteComment( string.Format( " {0} ", relationship.Type ) );
						}
					}

					writer.WriteStartElement( "relationship" );

					if ( includeLongs )
					{
						var relationship = @group.FirstOrDefault( );

						if ( relationship != null )
						{
							writer.WriteAttributeString( "id", relationship.TypeId.ToString( CultureInfo.InvariantCulture ) );
						}
					}

					writer.WriteAttributeString( "upgradeId", group.Key.ToString( "B" ) );

					foreach ( ForwardRelationship rel in group )
					{
						if ( !string.IsNullOrEmpty( rel.To ) && includeComments )
						{
							writer.WriteComment( string.Format( " {0} ", rel.To ) );
						}

						writer.WriteStartElement( "instance" );

						if ( includeLongs )
						{
							writer.WriteAttributeString( "id", rel.ToId.ToString( CultureInfo.InvariantCulture ) );
						}

						writer.WriteAttributeString( "upgradeId", rel.ToUpgradeId.ToString( "B" ) );

						writer.WriteEndElement( ); // instance
					}

					writer.WriteEndElement( ); // relationship
				}

				writer.WriteEndElement( ); // forwardRelationships

				var revGroupBy = reverseRelationships.GroupBy( rr => rr.TypeUpgradeId );

				writer.WriteStartElement( "reverseRelationships" );

				foreach ( IGrouping<Guid, ReverseRelationship> group in revGroupBy )
				{
					if ( group.Any( ) )
					{
						var relationship = @group.FirstOrDefault( );

						if ( relationship != null && includeComments )
						{
							writer.WriteComment( string.Format( " {0} ", relationship.Type ) );
						}
					}

					writer.WriteStartElement( "relationship" );

					if ( includeLongs )
					{
						var relationship = @group.FirstOrDefault( );

						if ( relationship != null )
						{
							writer.WriteAttributeString( "id", relationship.TypeId.ToString( CultureInfo.InvariantCulture ) );
						}
					}

					writer.WriteAttributeString( "upgradeId", group.Key.ToString( "B" ) );

					foreach ( ReverseRelationship rel in group )
					{
						if ( ! string.IsNullOrEmpty( rel.From ) && includeComments )
						{
							writer.WriteComment( string.Format( " {0} ", rel.From ) );
						}

						writer.WriteStartElement( "instance" );

						if ( includeLongs )
						{
							writer.WriteAttributeString( "id", rel.FromId.ToString( CultureInfo.InvariantCulture ) );
						}

						writer.WriteAttributeString( "upgradeId", rel.FromUpgradeId.ToString( "B" ) );

						writer.WriteEndElement( ); // instance
					}

					writer.WriteEndElement( ); // relationship
				}

				writer.WriteEndElement( ); // reverseRelationships

				writer.WriteEndElement( ); // entity
			}

			ImportExportText = builder.ToString( );

			return true;
		}
	}
}