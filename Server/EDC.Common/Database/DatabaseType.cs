// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using EDC.Core;
using EDC.Database.Types;
using EDC.Xml;
using ProtoBuf;

namespace EDC.Database
{
	/// <summary>
	///     Describes the supported logical Readinow database types.
	///     For SQL data types, use System.Data.DbType.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[KnownType( typeof ( BinaryType ) )]
	[XmlInclude( typeof ( BinaryType ) )]
	[ProtoInclude( 200, typeof( BinaryType ) )]
	[KnownType( typeof ( BoolType ) )]
	[XmlInclude( typeof ( BoolType ) )]
	[ProtoInclude( 201, typeof( BoolType ) )]
	[KnownType( typeof ( GuidType ) )]
	[XmlInclude( typeof ( GuidType ) )]
	[ProtoInclude( 202, typeof( GuidType ) )]
	[KnownType( typeof ( StringType ) )]
	[XmlInclude( typeof ( StringType ) )]
	[ProtoInclude( 203, typeof( StringType ) )]
	[KnownType( typeof ( StructureLevelsType ) )]
	[XmlInclude( typeof ( StructureLevelsType ) )]
	[ProtoInclude( 204, typeof( StructureLevelsType ) )]
	[KnownType( typeof ( ChoiceRelationshipType ) )]
	[XmlInclude( typeof ( ChoiceRelationshipType ) )]
	[ProtoInclude( 205, typeof( ChoiceRelationshipType ) )]
	[KnownType( typeof ( InlineRelationshipType ) )]
	[XmlInclude( typeof ( InlineRelationshipType ) )]
	[ProtoInclude( 206, typeof( InlineRelationshipType ) )]
	[KnownType( typeof ( XmlType ) )]
	[XmlInclude( typeof ( XmlType ) )]
	[ProtoInclude( 207, typeof( XmlType ) )]
	[KnownType( typeof ( UnknownType ) )]
	[XmlInclude( typeof ( UnknownType ) )]
	[ProtoInclude( 208, typeof( UnknownType ) )]
	[KnownType( typeof ( DateTimeType ) )]
	[XmlInclude( typeof ( DateTimeType ) )]
	[ProtoInclude( 209, typeof( DateTimeType ) )]
	[KnownType( typeof ( DateType ) )]
	[XmlInclude( typeof ( DateType ) )]
	[ProtoInclude( 210, typeof( DateType ) )]
	[KnownType( typeof ( TimeType ) )]
	[XmlInclude( typeof ( TimeType ) )]
	[ProtoInclude( 211, typeof( TimeType ) )]
	[KnownType( typeof ( Int32Type ) )]
	[XmlInclude( typeof ( Int32Type ) )]
	[ProtoInclude( 212, typeof( Int32Type ) )]
	[KnownType( typeof ( IdentifierType ) )]
	[XmlInclude( typeof ( IdentifierType ) )]
	[ProtoInclude( 213, typeof( IdentifierType ) )]
	[KnownType( typeof ( DecimalType ) )]
	[XmlInclude( typeof ( DecimalType ) )]
	[ProtoInclude( 214, typeof( DecimalType ) )]
	[KnownType( typeof ( CurrencyType ) )]
	[XmlInclude( typeof ( CurrencyType ) )]
	[ProtoInclude( 215, typeof( CurrencyType ) )]
	[KnownType( typeof( AutoIncrementType ) )]
	[XmlInclude( typeof( AutoIncrementType ) )]
	[ProtoInclude( 217, typeof( AutoIncrementType ) )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	[ProtoContract]
	public abstract class DatabaseType
	{
		/// <summary>
		///     Gets the binary.
		/// </summary>
		public static BinaryType BinaryType
		{
			get
			{
				return new BinaryType( );
			}
		}

		/// <summary>
		///     Gets the bool.
		/// </summary>
		public static BoolType BoolType
		{
			get
			{
				return new BoolType( );
			}
		}

		/// <summary>
		///     Returns whether it is possible to transform this value in a manner that places values on a relative linear scale.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can linear scale; otherwise, <c>false</c>.
		/// </value>
		public virtual bool CanLinearScale
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		///     Gets the Choice Relationship.
		/// </summary>
		public static ChoiceRelationshipType ChoiceRelationshipType
		{
			get
			{
				return new ChoiceRelationshipType( );
			}
		}

		/// <summary>
		///     Gets the currency.
		/// </summary>
		public static CurrencyType CurrencyType
		{
			get
			{
				return new CurrencyType( );
			}
		}

		/// <summary>
		///     Gets the date time.
		/// </summary>
		public static DateTimeType DateTimeType
		{
			get
			{
				return new DateTimeType( );
			}
		}

		/// <summary>
		///     Gets the date.
		/// </summary>
		public static DateType DateType
		{
			get
			{
				return new DateType( );
			}
		}

		/// <summary>
		///     Gets the decimal.
		/// </summary>
		public static DecimalType DecimalType
		{
			get
			{
				return new DecimalType( );
			}
		}

		/// <summary>
		///     Gets the GUID.
		/// </summary>
		public static GuidType GuidType
		{
			get
			{
				return new GuidType( );
			}
		}


		/// <summary>
		///     Gets the int32.
		/// </summary>
		public static IdentifierType IdentifierType
		{
			get
			{
				return new IdentifierType( );
			}
		}

		/// <summary>
		///     Gets the Inline Relationship.
		/// </summary>
		public static InlineRelationshipType InlineRelationshipType
		{
			get
			{
				return new InlineRelationshipType( );
			}
		}

		/// <summary>
		///     Gets the int32.
		/// </summary>
		public static Int32Type Int32Type
		{
			get
			{
				return new Int32Type( );
			}
		}

		/// <summary>
		/// Gets the type of the auto increment.
		/// </summary>
		/// <value>
		/// The type of the auto increment.
		/// </value>
		public static AutoIncrementType AutoIncrementType
		{
			get
			{
				return new AutoIncrementType( );
			}
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		public static StringType StringType
		{
			get
			{
				return new StringType( );
			}
		}

		/// <summary>
		///     Gets the structure levels.
		/// </summary>
		public static StructureLevelsType StructureLevelsType
		{
			get
			{
				return new StructureLevelsType( );
			}
		}

		/// <summary>
		///     Gets the time.
		/// </summary>
		public static TimeType TimeType
		{
			get
			{
				return new TimeType( );
			}
		}

		/// <summary>
		///     Gets the unknown.
		/// </summary>
		public static UnknownType UnknownType
		{
			get
			{
				return new UnknownType( );
			}
		}

		/// <summary>
		///     Gets the XML.
		/// </summary>
		public static XmlType XmlType
		{
			get
			{
				return new XmlType( );
			}
		}

		/// <summary>
		///     Converts a SQL DbType to a generic database type.
		/// </summary>
		/// <param name="type">
		///     An enumeration describing a generic database type.
		/// </param>
		/// <returns>
		///     An enumeration describing the SQL equivalent type.
		/// </returns>
		public static DatabaseType ConvertFromDbType( string type )
		{
			DbType dbType;
			if ( Enum.TryParse( type, true, out dbType ) )
			{
				return ConvertFromSqlDbType( dbType );
			}

			throw new InvalidOperationException( "The specified database type cannot be converted." );
		}

		/// <summary>
		///     Converts from display name.
		/// </summary>
		/// <param name="displayName">The display name.</param>
		/// <returns></returns>
		public static DatabaseType ConvertFromDisplayName( string displayName )
		{
			switch ( displayName )
			{
				case "Binary":
					return new BinaryType( );

				case "Bool":
					return new BoolType( );

				case "Boolean":
					return new BoolType( );

				case "Currency":
					return new CurrencyType( );

				case "DateTime":
					return new DateTimeType( );

				case "Date":
					return new DateType( );

				case "Decimal":
					return new DecimalType( );

				case "Guid":
					return new GuidType( );

				case "Int32":
					return new Int32Type( );

				case "String":
					return new StringType( );

				case "StructureLevels":
					return new StructureLevelsType( );

				case "ChoiceRelationship":
					return new ChoiceRelationshipType( );

				case "InlineRelationship":
					return new InlineRelationshipType( );

				case "Time":
					return new TimeType( );

				case "Unknown":
					return new UnknownType( );

				case "Xml":
					return new XmlType( );

				case "Identifier":
					return new IdentifierType( );

				case "AutoIncrement":
					return new AutoIncrementType( );

				default:
					throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", displayName ) );
			}
		}

		/// <summary>
		///     Converts a SQL DbType to a generic database type.
		/// </summary>
		/// <param name="sqlDbType">
		///     An enumeration describing a generic database type.
		/// </param>
		/// <returns>
		///     An enumeration describing the SQL equivalent type.
		/// </returns>
		public static DatabaseType ConvertFromSqlDbType( SqlDbType sqlDbType )
		{
			switch ( sqlDbType )
			{
				case SqlDbType.Binary:
					return new BinaryType( );

				case SqlDbType.Bit:
					return new BoolType( );

				case SqlDbType.Date:
					return new DateType( );

				case SqlDbType.DateTime:
				case SqlDbType.DateTime2:
					return new DateTimeType( );

				case SqlDbType.Decimal:
					return new DecimalType( );

				case SqlDbType.Int:
					return new Int32Type( );

				case SqlDbType.Money:
					return new CurrencyType( );

				case SqlDbType.NVarChar:
				case SqlDbType.NText:
					return new StringType( );

				case SqlDbType.Time:
					return new TimeType( );

				case SqlDbType.UniqueIdentifier:
					return new GuidType( );

				case SqlDbType.Xml:
					return new XmlType( );

				default:
					throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", sqlDbType ) );
			}
		}

		/// <summary>
		///     Converts a SQL DbType to a generic database type.
		/// </summary>
		/// <param name="dbType">
		///     An enumeration describing a generic database type.
		/// </param>
		/// <returns>
		///     An enumeration describing the SQL equivalent type.
		/// </returns>
		public static DatabaseType ConvertFromSqlDbType( DbType dbType )
		{
			switch ( dbType )
			{
				case DbType.Binary:
					return new BinaryType( );

				case DbType.Boolean:
					return new BoolType( );

				case DbType.Date:
					return new DateType( );

				case DbType.DateTime:
				case DbType.DateTime2:
					return new DateTimeType( );

				case DbType.Decimal:
					return new DecimalType( );

				case DbType.Int32:
					return new Int32Type( );

				case DbType.Currency:
					return new CurrencyType( );

				case DbType.String:
				case DbType.StringFixedLength:
					return new StringType( );

				case DbType.Time:
					return new TimeType( );

				case DbType.Guid:
					return new GuidType( );

				case DbType.Xml:
					return new XmlType( );

				default:
					throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", dbType ) );
			}
		}

		/// <summary>
		///     Converts a SQL DbType to a generic database type.
		/// </summary>
		/// <param name="type">
		///     An enumeration describing a generic database type.
		/// </param>
		/// <returns>
		///     An enumeration describing the SQL equivalent type.
		/// </returns>
		//[Obsolete("We should be using DbType not SqlDbType")]
		public static DatabaseType ConvertFromSqlDbType( string type )
		{
			SqlDbType sqlDbType;
			if ( Enum.TryParse( type, true, out sqlDbType ) )
			{
				return ConvertFromSqlDbType( sqlDbType );
			}

			throw new InvalidOperationException( "The specified database type cannot be converted." );
		}

		/// <summary>
		///     Converts a database value from a string encoding to its native type.
		/// </summary>
		/// <param name="value">
		///     A string containing the database value.
		/// </param>
		/// <returns>
		///     An object representing the database value.
		/// </returns>
		public abstract object ConvertFromString( string value );

		/// <summary>
		///     Converts a runtime type to a generic database type
		/// </summary>
		/// <param name="type">
		///     An enumeration describing a runtime type.
		/// </param>
		/// <returns>
		///     An enumeration describing the equivalent database type.
		/// </returns>
		public static DatabaseType ConvertFromType( Type type )
		{
			if ( type == typeof ( bool ) )
			{
				return new BoolType( );
			}

			if ( type == typeof ( DateTime ) )
			{
				return new DateTimeType( );
			}

			if ( type == typeof ( Decimal ) )
			{
				return new DecimalType( );
			}

			if ( type == typeof ( Guid ) )
			{
				return new GuidType( );
			}

			if ( type == typeof ( Int32 ) )
			{
				return new Int32Type( );
			}

			if ( type == typeof ( Int64 ) )
			{
				return new IdentifierType( );
			}

			if ( type == typeof ( String ) )
			{
				return new StringType( );
			}

			if ( type == typeof ( TimeSpan ) )
			{
				return new TimeType( );
			}

			if ( type == typeof ( Double ) )
			{
				return new DecimalType( );
			}

			if ( type == typeof ( Enum ) )
			{
				return new UnknownType( );
			}

			throw new InvalidOperationException( "The specified runtime type cannot be converted." );
		}

		/// <summary>
		///     Converts the value to a type that can be embedded directly in a SQL query, inclusive of any applicable quotes and escaping.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string literal representation of the database value.
		/// </returns>
		public abstract string ConvertToSqlLiteral( object value );

		/// <summary>
		///     Converts the database value from its native type to a string encoding
		///     supported by SQL server, that can be used in queries.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public abstract string ConvertToSqlString( object value );

		/// <summary>
		///     Converts the database value from its native type to a string encoding.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public abstract string ConvertToString( object value );

		/// <summary>
		///     Reads the XML.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static DatabaseType FromXml( XmlNode xml )
		{
			string typeName = XmlHelper.EvaluateSingleNode( xml, "@name" ) ? XmlHelper.ReadAttributeString( xml, "@name" ) : xml.InnerXml;


			if ( !string.IsNullOrEmpty( typeName ) )
			{
				DatabaseType databaseType = ConvertFromDisplayName( typeName );
				databaseType.OnFromXml( xml );
				return databaseType;
			}

			throw new Exception( "Unknown type: " + typeName );
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent DbType.
		/// </summary>
		public virtual DbType GetDbType( )
		{
			throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", GetType( ).Name ) );
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		/// <returns></returns>
		public virtual string GetDisplayName( )
		{
			throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", GetType( ).Name ) );
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent runtime type.
		/// </summary>
		/// <returns></returns>
		public virtual Type GetRunTimeType( )
		{
			throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", GetType( ).Name ) );
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public virtual SqlDbType GetSqlDbType( )
		{
			throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", GetType( ).Name ) );
		}


		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType string.
		/// </summary>
		public abstract string GetSqlDbTypeString( );

		/// <summary>
		///     Toes the XML.
		/// </summary>
		/// <param name="xml">The XML.</param>
		public void ToXml( XmlWriter xml )
		{
			xml.WriteStartElement( "type" );
			xml.WriteAttributeString( "name", GetDisplayName( ) );
			OnToXml( xml );
			xml.WriteEndElement( );
		}

		/// <summary>
		///     Called when [from XML].
		/// </summary>
		/// <param name="xmlNode">The XML node.</param>
		protected virtual void OnFromXml( XmlNode xmlNode )
		{
			//do nothing
		}

		/// <summary>
		///     Called when [to XML].
		/// </summary>
		/// <param name="xml">The XML.</param>
		protected virtual void OnToXml( XmlWriter xml )
		{
			//do nothing
		}
	}
}