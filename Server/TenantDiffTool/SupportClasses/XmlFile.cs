// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Processing;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version1;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.SupportClasses
{
	public class XmlFile : File
	{
		/// <summary>
		///     The _data
		/// </summary>
		private IList<Data> _data;

		/// <summary>
		///     The entities
		/// </summary>
		private IList<Entity> _entities;

		/// <summary>
		///     The relationships
		/// </summary>
		private IList<Relationship> _relationships;

		/// <summary>
		///     The serializer
		/// </summary>
		private XmlDeserializer _serializer;

		/// <summary>
		///     Initializes a new instance of the <see cref="XmlFile" /> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="context">The context.</param>
		public XmlFile( string path, DatabaseContext context ) : base( path, context )
		{
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <returns></returns>
		public override IList<Data> GetData( )
		{
			Parse( );

			if ( _data == null )
			{
				_data = new List<Data>( );

				List<Data> unsorted = new List<Data>( );

				foreach ( string dataType in _serializer.Data.Keys )
				{
					IList<DataEntry> entryList = _serializer.Data[ dataType ];

					foreach ( DataEntry entry in entryList )
					{
						Data data = new Data
						{
							EntityUpgradeId = entry.EntityId,
							FieldUpgradeId = entry.FieldId,
							Value = entry.Data,
							Type = dataType
						};

						switch ( dataType )
						{
							case Helpers.AliasName:
								data.Value = entry.Namespace + ":" + entry.Data + ":" + entry.AliasMarkerId;
								break;
							case Helpers.BitName:
								bool b;

								if ( bool.TryParse( entry.Data.ToString( ), out b ) )
								{
									data.Value = b ? "1" : "0";
								}
								break;
							case Helpers.DateTimeName:
								if ( entry.Data == null )
								{
									data.Value = entry.Data;
								}
								else
								{
									DateTime dt;

									if ( entry.Data is DateTime )
									{
										data.Value = ( ( DateTime ) entry.Data ).ToUniversalTime( ).ToString( "u" );
									}
									else if ( DateTime.TryParse( entry.Data.ToString( ), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out dt ) )
									{
										data.Value = dt.ToString( "u" );
									}
								}
								break;
							case Helpers.GuidName:
								data.Value = entry.Data.ToString( );
								break;
							case Helpers.IntName:
								data.Value = entry.Data.ToString( );
								break;
						}

						unsorted.Add( data );
					}
				}

				_data = unsorted.OrderBy( e => e.EntityUpgradeId ).ThenBy( e => e.FieldUpgradeId ).ToList( );
			}

			return _data;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <param name="excludeRelationshipInstances">if set to <c>true</c> [exclude relationship instances].</param>
		/// <returns></returns>
		public override IList<Entity> GetEntities( bool excludeRelationshipInstances )
		{
			Parse( );

			return _entities ?? ( _entities = _serializer.Entities.OrderBy( e => e.EntityId ).Select( e => new Entity
			{
				EntityUpgradeId = e.EntityId
			} ).ToList( ) );
		}

		/// <summary>
		///     Gets the entity field properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public override void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var entityUid = ( Guid ) state[ "entityUpgradeId" ];

			int fieldCounter = 0;

			foreach ( var fieldData in _data.Where( d => d.EntityUpgradeId == entityUid ) )
			{
				var fieldName = GetFieldName( fieldData.FieldUpgradeId );

				string value = fieldData.Value.ToString( );

				var field = new EntityFieldInfo( $"FLD{fieldCounter++}", "Fields", fieldName, $"The '{fieldName}' field value." );
				field.SetValue( this, value );
				props.Add( new FieldPropertyDescriptor( field ) );
			}
		}

		/// <summary>
		///     Gets the entity properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public override void GetEntityProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			/////
			// Upgrade Id.
			/////
			var baseInfo = new EntityFieldInfo( "base1", "Entity", "UpgradeId", "The Entity upgrade Id." );
			baseInfo.SetValue( this, ( ( Guid ) state[ "entityUpgradeId" ] ).ToString( "B" ) );
			props.Add( new FieldPropertyDescriptor( baseInfo ) );
		}

		/// <summary>
		///     Gets the entity relationship properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public override void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state )
		{
			var entityUid = ( Guid ) state[ "entityUpgradeId" ];

			int relationshipCounter = 0;

			foreach ( var relData in _relationships.Where( r => r.FromUpgradeId == entityUid || r.ToUpgradeId == entityUid ) )
			{
				string direction = relData.FromUpgradeId == entityUid ? "Forward" : "Reverse";
				Guid typeId = relData.TypeUpgradeId;
				string typeName = GetFieldName( relData.TypeUpgradeId );
				Guid toId = relData.ToUpgradeId;
				string toName = GetFieldName( relData.ToUpgradeId );

				var field = new EntityFieldInfo( $"RLN{relationshipCounter++.ToString( "0000" )}", direction + " relationships", typeName ?? typeId.ToString( "B" ), $"The '{typeName ?? typeId.ToString( "B" )}' field value." );
				field.SetValue( this, toName ?? toId.ToString( "B" ) );
				props.Add( new FieldPropertyDescriptor( field ) );
			}
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="excludeRelationshipInstances">if set to <c>true</c> [exclude relationship instances].</param>
		/// <returns></returns>
		public override IList<Relationship> GetRelationships( bool excludeRelationshipInstances )
		{
			Parse( );

			return _relationships ?? ( _relationships = _serializer.Relationships.OrderBy( r => r.FromId ).ThenBy( r => r.TypeId ).ThenBy( r => r.ToId ).Select( r => new Relationship
			{
				FromUpgradeId = r.FromId,
				TypeUpgradeId = r.TypeId,
				ToUpgradeId = r.ToId
			} ).ToList( ) );
		}

		/// <summary>
		///     Gets the name of the field.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		private string GetFieldName( Guid id )
		{
			Guid nameGuid = new Guid( "f8def406-90a1-4580-94f4-1b08beac87af" );

			string fieldName = null;

			ReferToUpgradeIdLookup( ref fieldName, id );

			if ( !string.IsNullOrEmpty( fieldName ) )
			{
				fieldName = fieldName.Replace( "\r\n", " " ).Replace( "\n", " " ).Replace( "\t", " " );

				while ( fieldName.IndexOf( "  ", StringComparison.Ordinal ) >= 0 )
				{
					fieldName = fieldName.Replace( "  ", " " );
				}

				fieldName = fieldName.Trim( );
			}
			else
			{
				var name = _data.FirstOrDefault( d => d.EntityUpgradeId == id && d.FieldUpgradeId == nameGuid );

				if ( name?.Value != null )
				{
					fieldName = name.Value.ToString( );
				}

				if ( string.IsNullOrEmpty( fieldName ) )
				{
					fieldName = id.ToString( "B" );
				}
			}
			return fieldName;
		}

		/// <summary>
		///     Parses this instance.
		/// </summary>
		private void Parse( )
		{
			if ( _serializer != null )
			{
				return;
			}

			using ( XmlTextReader xmlTextReader = new XmlTextReader( Path ) )
			{
				_serializer = new XmlDeserializer( );
				_serializer.Deserialize( xmlTextReader );
			}
		}
	}
}