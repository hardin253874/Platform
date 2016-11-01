// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Data.SqlClient;

namespace EDC.Database
{
	/// <summary>
	///     Table-Valued Parameters
	/// </summary>
	public static class TableValuedParameter
	{
		/// <summary>
		///     Adds the column.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataTable">The data table.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">dataTable</exception>
		private static DataTable AddColumn<T>( this DataTable dataTable, string name )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			dataTable.Columns.Add( new DataColumn( name, typeof ( T ) ) );

			return dataTable;
		}

		/// <summary>
		///     Adds the data column.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataTable">The data table.</param>
		/// <returns></returns>
		private static DataTable AddDataColumn<T>( this DataTable dataTable )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			dataTable.AddColumn<T>( "Data" );

			return dataTable;
		}

		/// <summary>
		///     Adds the default data columns.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">dataTable</exception>
		private static DataTable AddDefaultDataColumns( this DataTable dataTable )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			dataTable.Columns.AddRange( new[ ]
			{
				new DataColumn( "EntityId", typeof ( long ) ),
				new DataColumn( "TenantId", typeof ( long ) ),
				new DataColumn( "FieldId", typeof ( long ) )
			} );

			return dataTable;
		}

		/// <summary>
		///     Adds the default relationship columns.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">dataTable</exception>
		private static DataTable AddDefaultRelationshipColumns( this DataTable dataTable )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			dataTable.Columns.AddRange( new[ ]
			{
				new DataColumn( "TenantId", typeof ( long ) ),
				new DataColumn( "TypeId", typeof ( long ) )
			} );

			return dataTable;
		}

		/// <summary>
		///     Adds from identifier column.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <exception cref="System.ArgumentNullException">dataTable</exception>
		private static DataTable AddFromIdColumn( this DataTable dataTable )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			dataTable.AddColumn<long>( "FromId" );

			return dataTable;
		}

		/// <summary>
		///     Adds the table valued parameter.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="typeName">Name of the type.</param>
		/// <exception cref="System.ArgumentNullException">
		///     command
		///     or
		///     name
		///     or
		///     value
		///     or
		///     typeName
		/// </exception>
		public static IDbDataParameter AddTableValuedParameter( this IDbCommand command, string name, DataTable value, string typeName = null )
		{
			if ( command == null )
			{
				throw new ArgumentNullException( "command" );
			}

			if ( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( typeName == null )
			{
				if ( value.ExtendedProperties.ContainsKey( "dataType" ) )
				{
					typeName = ( string ) value.ExtendedProperties[ "dataType" ];
				}
			}

			if ( typeName == null )
			{
				throw new ArgumentNullException( "typeName" );
			}

			var parameter = new SqlParameter( name, SqlDbType.Structured )
			{
				TypeName = typeName,
				Value = value
			};

			command.Parameters.Add( parameter );

			return parameter;
		}

		/// <summary>
		///     Adds to identifier column.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <exception cref="System.ArgumentNullException">dataTable</exception>
		private static DataTable AddToIdColumn( this DataTable dataTable )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			dataTable.AddColumn<long>( "ToId" );

			return dataTable;
		}

		/// <summary>
		///     Converts the type of the table name to table valued parameter.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">
		///     Invalid tableName
		///     or
		///     Invalid tableName
		/// </exception>
		public static TableValuedParameterType ConvertTableNameToTableValuedParameterType( string tableName )
		{
			if ( string.IsNullOrWhiteSpace( tableName ) )
			{
				throw new ArgumentException( "Invalid tableName" );
			}

			switch ( tableName )
			{
				case "Data_Alias":
					return TableValuedParameterType.DataAlias;
				case "Data_Bit":
					return TableValuedParameterType.DataBit;
				case "Data_DateTime":
					return TableValuedParameterType.DataDateTime;
				case "Data_Decimal":
					return TableValuedParameterType.DataDecimal;
				case "Data_Guid":
					return TableValuedParameterType.DataGuid;
				case "Data_Int":
					return TableValuedParameterType.DataInt;
				case "Data_NVarChar":
					return TableValuedParameterType.DataNVarChar;
				case "Data_Xml":
					return TableValuedParameterType.DataXml;
				case "Relationship":
					return TableValuedParameterType.Relationship;
				default:
					throw new ArgumentException( "Invalid tableName" );
			}
		}

		/// <summary>
		///     Creates the table valued parameter.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">@Invalid table valued parameter type;type</exception>
		public static DataTable Create( TableValuedParameterType type )
		{
			var dt = new DataTable( );

			switch ( type )
			{
				case TableValuedParameterType.BigInt:
					dt.AddColumn<long>( "Id" )
						.SetType( "dbo.UniqueIdListType" );
					break;
                case TableValuedParameterType.Guid:
                    dt.AddColumn<Guid>("Id")
                        .SetType("dbo.UniqueGuidListType");
                    break;
                case TableValuedParameterType.GuidList:
                    dt.AddColumn<Guid>( "Id" )
                        .SetType( "dbo.GuidListType" );
                    break;
                case TableValuedParameterType.Int:
                    dt.AddColumn<int>( "Data" )
                        .SetType( "dbo.IntListType" );
                    break;
                case TableValuedParameterType.Decimal:
                    dt.AddColumn<decimal>( "Data" )
                        .SetType( "dbo.DecimalListType" );
                    break;
                case TableValuedParameterType.DateTime:
                    dt.AddColumn<DateTime>( "Data" )
                        .SetType( "dbo.DateTimeListType" );
                    break;
                case TableValuedParameterType.DataAlias:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<string>( )
						.AddColumn<string>( "Namespace" )
						.AddColumn<int>( "AliasMarkerId" )
						.SetType( "dbo.Data_AliasType" );
					break;
				case TableValuedParameterType.DataBit:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<bool>( )
						.SetType( "dbo.Data_BitType" );
					break;
				case TableValuedParameterType.DataDateTime:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<DateTime>( )
						.SetType( "dbo.Data_DateTimeType" );
					break;
				case TableValuedParameterType.DataDecimal:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<decimal>( )
						.SetType( "dbo.Data_DecimalType" );
					break;
				case TableValuedParameterType.DataGuid:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<Guid>( )
						.SetType( "dbo.Data_GuidType" );
					break;
				case TableValuedParameterType.DataInt:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<int>( )
						.SetType( "dbo.Data_IntType" );
					break;
				case TableValuedParameterType.DataNVarChar:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<string>( )
						.SetType( "dbo.Data_NVarCharType" );
					break;
				case TableValuedParameterType.DataXml:
					dt.AddDefaultDataColumns( )
						.AddDataColumn<string>( )
						.SetType( "dbo.Data_XmlType" );
					break;
				case TableValuedParameterType.Relationship:
					dt.AddDefaultRelationshipColumns( )
						.AddFromIdColumn( )
						.AddToIdColumn( )
						.SetType( "dbo.RelationshipType" );
					break;
				case TableValuedParameterType.EntityBatch:
					dt.AddColumn<long>( "BatchId" )
						.AddColumn<long>( "EntityId" )
						.SetType( "dbo.EntityBatchType" );
					break;
				case TableValuedParameterType.EntityClone:
					dt.AddColumn<long>( "SourceId" )
						.AddColumn<long>( "DestinationId" )
						.AddColumn<int>( "CloneOption" )
						.SetType( "dbo.EntityCloneType" );
					break;
				case TableValuedParameterType.EntityMap:
					dt.AddColumn<long>( "SourceId" )
						.AddColumn<long>( "DestinationId" )
						.SetType( "dbo.EntityMapType" );
					break;
				case TableValuedParameterType.InputEntityType:
					dt.AddColumn<long>( "Id" )
						.AddColumn<long>( "TypeId" )
						.AddColumn<int>( "IsClone" )
						.SetType( "dbo.InputEntityType" );
					break;
				case TableValuedParameterType.LookupFieldKey:
					dt.AddDefaultDataColumns( )
						.SetType( "dbo.FieldKeyType" );
					break;
				case TableValuedParameterType.LookupAliasData:
					dt.AddColumn<long>( "TenantId" )
						.AddColumn<string>( "Namespace" )
						.AddColumn<string>( "Data" )
						.SetType( "dbo.AliasLookupType" );
					break;
				case TableValuedParameterType.LookupRelationshipForward:
					dt.AddDefaultRelationshipColumns( )
						.AddFromIdColumn( )
						.SetType( "dbo.ForwardRelationshipType" );
					break;
				case TableValuedParameterType.LookupRelationshipReverse:
					dt.AddDefaultRelationshipColumns( )
						.AddToIdColumn( )
						.SetType( "dbo.ReverseRelationshipType" );
					break;
				case TableValuedParameterType.BulkFldType:
					dt.AddColumn<int>( "NodeTag" )
						.AddColumn<int>( "FieldId" )
						.SetType( "dbo.BulkFldType" );
					break;
				case TableValuedParameterType.BulkRelType:
					dt.AddColumn<int>( "NodeTag" )
						.AddColumn<long>( "RelTypeId" )
						.AddColumn<int>( "NextTag" )
						.SetType( "dbo.BulkRelType" );
					break;
                case TableValuedParameterType.NVarCharMaxListType:
                    dt.AddColumn<string>( "Data" )
                        .SetType( "dbo.NVarCharMaxListType" );
                    break;
                
				default:
					throw new ArgumentException( @"Invalid table valued parameter type", "type" );
			}

			return dt;
		}

		/// <summary>
		///     Creates the table valued parameter.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">command</exception>
		/// <exception cref="System.ArgumentException">@Invalid table valued parameter type;type</exception>
		public static DataTable CreateTableValuedParameter( this IDbCommand command, TableValuedParameterType type )
		{
			if ( command == null )
			{
				throw new ArgumentNullException( "command" );
			}

			return Create( type );
		}

		/// <summary>
		///     Adds from identifier column.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">dataTable</exception>
		/// <exception cref="System.ArgumentException">@Invalid type specified;type</exception>
		private static DataTable SetType( this DataTable dataTable, string type )
		{
			if ( dataTable == null )
			{
				throw new ArgumentNullException( "dataTable" );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			if ( string.IsNullOrWhiteSpace( type ) )
			{
				throw new ArgumentException( @"Invalid type specified", "type" );
			}

			dataTable.ExtendedProperties[ "dataType" ] = type;

			return dataTable;
		}
	}
}