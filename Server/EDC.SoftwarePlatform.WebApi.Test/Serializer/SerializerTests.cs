// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Runtime.Serialization;
using EDC.Database;
using Jil;
using NUnit.Framework;
using DateTimeFormat = Jil.DateTimeFormat;

namespace EDC.SoftwarePlatform.WebApi.Test.Serializer
{
	/// <summary>
	///     Object Container class.
	/// </summary>
	[DataContract]
	public class ObjectContainer
	{
		/// <summary>
		///     Gets or sets the type of the data.
		/// </summary>
		/// <value>
		///     The type of the data.
		/// </value>
		[DataMember( Name = "dataType" )]
		public string DataType
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[DataMember( Name = "value" )]
		public string Value
		{
			get;
			set;
		}
	}

	/// <summary>
	///     Serializer tests.
	/// </summary>
	[TestFixture]
	public class SerializerTests
	{
		/// <summary>
		///     Gets or sets the options.
		/// </summary>
		/// <value>
		///     The options.
		/// </value>
		public Options Options
		{
			get;
			set;
		}


		/// <summary>
		///     Initializes a new instance of the <see cref="SerializerTests" /> class.
		/// </summary>
		public SerializerTests( )
		{
			Options = new Options( dateFormat: DateTimeFormat.ISO8601, excludeNulls: false, includeInherited: true );
		}

		/// <summary>
		///     Tests the database type deserialization.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="value">The value.</param>
		[TestCase( "Bool", "true" )]
		[TestCase( "Currency", "123.33" )]
		[TestCase( "Date", "10/10/2014 12:00:00 AM" )]
		[TestCase( "DateTime", "10/10/2014 11:25:17 AM" )]
		[TestCase( "Decimal", "123.33" )]
		[TestCase( "Guid", "9553ED44-18FB-41CE-A761-84FC673CF95D" )]
		[TestCase( "Int32", "12345" )]
		[TestCase( "Identifier", "12345" )]
		[TestCase( "StructureLevels", "Hello World" )]
		[TestCase( "String", "Hello World" )]
		[TestCase( "ChoiceRelationship", "Hello World" )]
		[TestCase( "InlineRelationship", "Hello World" )]
        [TestCase( "Time", "1/1/1753 11:25:17 AM" )]
		[TestCase( "Unknown", "", ExpectedExceptionName = "System.InvalidOperationException" )]
		[TestCase( "Xml", "<a>Hello World</a>" )]
		[TestCase( "AutoIncrement", "0" )]
		public void TestDatabaseTypeDeserialization( string dataType, string value )
		{
			string json = string.Format( @"{{""dataType"":""{0}"",""value"":""{1}""}}", dataType, value );

			using ( var str = new StringReader( json ) )
			{
				var container = JSON.Deserialize<ObjectContainer>( str, Options );

				DatabaseType databaseType = DatabaseTypeHelper.ConvertFromDisplayName( dataType );

				string stringVal = container.Value;

				object val = databaseType.ConvertFromString( stringVal );

				if ( val is DateTime )
				{
					DateTime val1 = ( DateTime ) val;
					DateTime val2 = DateTime.Parse( value );

					Assert.AreEqual( val1, val2 );	
				}
				else
				{
					Assert.AreEqual( value.ToLowerInvariant( ), val.ToString( ).ToLowerInvariant( ) );	
				}
			}
		}
	}
}