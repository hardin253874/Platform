// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Database;
using NUnit.Framework;

namespace EDC.Test.Database
{
	/// <summary>
	///     This is a test class for the  DatabaseTypeHelper type.
	/// </summary>
	[TestFixture]
	public class DatabaseTypeHelperTests
	{
		/// <summary>
		///     Helper method to cast the specified data using SQL server
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="databaseType"></param>
		/// <param name="length"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		private static T SqlServerCastData<T>( DatabaseType databaseType, int length, string data )
		{
			using ( DatabaseContext dc = DatabaseContext.GetContext( ) )
			{
				string typeName = databaseType.GetSqlDbType( ).ToString( );

				if ( databaseType == DatabaseType.BinaryType || databaseType == DatabaseType.StringType )
				{
					using ( IDbCommand cmd = dc.CreateCommand( string.Format( "SELECT CAST('{0}' AS {1}({2})) AS [Data]", data, typeName, length.ToString( CultureInfo.InvariantCulture ) ) ) )
					{
						return ( T ) cmd.ExecuteScalar( );
					}
				}

				using ( IDbCommand cmd = dc.CreateCommand( string.Format( "SELECT CAST('{0}' AS {1}) AS [Data]", data, typeName ) ) )
				{
					return ( T ) cmd.ExecuteScalar( );
				}
			}
		}

		/// <summary>
		///     Test that the ConvertFromSqlDbType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		public void ConvertFromSqlDbTypeEnum_InvalidTypes( )
		{
			var supportedTypes = new List<SqlDbType>
				{
					SqlDbType.BigInt,
					SqlDbType.Binary,
					SqlDbType.Bit,
					SqlDbType.Date,
					SqlDbType.DateTime,
					SqlDbType.DateTime2,
					SqlDbType.Decimal,
					SqlDbType.Int,
					SqlDbType.Money,
					SqlDbType.NVarChar,
					SqlDbType.SmallInt,
					SqlDbType.NText,
					SqlDbType.Real,
					SqlDbType.Time,
					SqlDbType.TinyInt,
					SqlDbType.UniqueIdentifier,
					SqlDbType.Xml
				};

			Array values = Enum.GetValues( typeof ( SqlDbType ) );
			foreach ( SqlDbType sqlDbType in values )
			{
				if ( !supportedTypes.Contains( sqlDbType ) )
				{
					bool exceptionThrown = false;

					try
					{
						DatabaseType.ConvertFromSqlDbType( sqlDbType );
					}
					catch ( InvalidOperationException )
					{
						exceptionThrown = true;
					}
					Assert.IsTrue( exceptionThrown, "The expected exception for SqlDbType {0} was not thrown", sqlDbType );
				}
			}
		}

		/// <summary>
		///     Test that the ConvertFromSqlDbType method correctly converts DatabaseType values.
		/// </summary>
		[Test]
		public void ConvertFromSqlDbTypeEnum_ValidTypes( )
		{
			var typeMappings = new Dictionary<SqlDbType, DatabaseType>
				{
					{
						SqlDbType.Binary, DatabaseType.BinaryType
					},
					{
						SqlDbType.Bit, DatabaseType.BoolType
					},
					{
						SqlDbType.Date, DatabaseType.DateType
					},
					{
						SqlDbType.DateTime, DatabaseType.DateTimeType
					},
					{
						SqlDbType.DateTime2, DatabaseType.DateTimeType
					},
					{
						SqlDbType.Decimal, DatabaseType.DecimalType
					},
					{
						SqlDbType.Int, DatabaseType.Int32Type
					},
					{
						SqlDbType.Money, DatabaseType.CurrencyType
					},
					{
						SqlDbType.NVarChar, DatabaseType.StringType
					},
					{
						SqlDbType.NText, DatabaseType.StringType
					},
					{
						SqlDbType.Time, DatabaseType.TimeType
					},
					{
						SqlDbType.UniqueIdentifier, DatabaseType.GuidType
					},
					{
						SqlDbType.Xml, DatabaseType.XmlType
					}
				};

			Array values = Enum.GetValues( typeof ( SqlDbType ) );
			foreach ( SqlDbType sqlDbType in values )
			{
				DatabaseType expectedType;

				if ( typeMappings.TryGetValue( sqlDbType, out expectedType ) )
				{
					DatabaseType actualType = DatabaseType.ConvertFromSqlDbType( sqlDbType );
					Assert.AreEqual( expectedType.GetType( ), actualType.GetType( ), "SqlDbType {0} failed to convert to DatabaseType", sqlDbType );
				}
			}
		}


		/// <summary>
		///     Test that the ConvertFromSqlDbType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( InvalidOperationException ) )]
		public void ConvertFromSqlDbTypeString_InvalidSqlDbType( )
		{
			DatabaseType.ConvertFromSqlDbType( "Not a SqlDbType" );
		}

		/// <summary>
		///     Test that the ConvertFromSqlDbType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		public void ConvertFromSqlDbTypeString_InvalidTypes( )
		{
			var supportedTypes = new List<SqlDbType>
				{
					SqlDbType.BigInt,
					SqlDbType.Binary,
					SqlDbType.Bit,
					SqlDbType.Date,
					SqlDbType.DateTime,
					SqlDbType.DateTime2,
					SqlDbType.Decimal,
					SqlDbType.Int,
					SqlDbType.Money,
					SqlDbType.NVarChar,
					SqlDbType.SmallInt,
					SqlDbType.NText,
					SqlDbType.Real,
					SqlDbType.Time,
					SqlDbType.TinyInt,
					SqlDbType.UniqueIdentifier,
					SqlDbType.Xml
				};

			Array values = Enum.GetValues( typeof ( SqlDbType ) );
			foreach ( SqlDbType sqlDbType in values )
			{
				if ( !supportedTypes.Contains( sqlDbType ) )
				{
					bool exceptionThrown = false;

					try
					{
						DatabaseType.ConvertFromSqlDbType( sqlDbType.ToString( ) );
					}
					catch ( InvalidOperationException )
					{
						exceptionThrown = true;
					}
					Assert.IsTrue( exceptionThrown, "The expected exception for SqlDbType {0} was not thrown", sqlDbType );
				}
			}
		}

		/// <summary>
		///     Test that the ConvertFromSqlDbType method correctly converts DatabaseType values.
		/// </summary>
		[Test]
		public void ConvertFromSqlDbTypeString_ValidTypes( )
		{
			var typeMappings = new Dictionary<SqlDbType, DatabaseType>
				{
					{
						SqlDbType.Binary, DatabaseType.BinaryType
					},
					{
						SqlDbType.Bit, DatabaseType.BoolType
					},
					{
						SqlDbType.Date, DatabaseType.DateType
					},
					{
						SqlDbType.DateTime, DatabaseType.DateTimeType
					},
					{
						SqlDbType.DateTime2, DatabaseType.DateTimeType
					},
					{
						SqlDbType.Decimal, DatabaseType.DecimalType
					},
					{
						SqlDbType.Int, DatabaseType.Int32Type
					},
					{
						SqlDbType.Money, DatabaseType.CurrencyType
					},
					{
						SqlDbType.NVarChar, DatabaseType.StringType
					},
					{
						SqlDbType.NText, DatabaseType.StringType
					},
					{
						SqlDbType.Time, DatabaseType.TimeType
					},
					{
						SqlDbType.UniqueIdentifier, DatabaseType.GuidType
					},
					{
						SqlDbType.Xml, DatabaseType.XmlType
					}
				};

			Array values = Enum.GetValues( typeof ( SqlDbType ) );
			foreach ( SqlDbType sqlDbType in values )
			{
				DatabaseType expectedType;

				if ( typeMappings.TryGetValue( sqlDbType, out expectedType ) )
				{
					string sqlDbTypeAsString = sqlDbType.ToString( );
					DatabaseType actualType = DatabaseType.ConvertFromSqlDbType( sqlDbTypeAsString );
					Assert.AreEqual( expectedType.GetType( ), actualType.GetType( ), "SqlDbType {0} failed to convert to DatabaseType", sqlDbTypeAsString );

					string sqlDbTypeAsStringUpperCase = sqlDbTypeAsString.ToUpper( );
					DatabaseType actualTypeUpper = DatabaseType.ConvertFromSqlDbType( sqlDbTypeAsStringUpperCase );
					Assert.AreEqual( expectedType.GetType( ), actualTypeUpper.GetType( ), "SqlDbType {0} failed to convert to DatabaseType", sqlDbTypeAsStringUpperCase );
				}
			}
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Binary value.
		/// </summary>
		[Test]
		public void ConvertFromString_Binary( )
		{
			var originalData = new byte[]
				{
					1, 2, 3, 4, 0x8
				};
			string originalStringData = Convert.ToBase64String( originalData );

			object objectData = DatabaseType.BinaryType.ConvertFromString( originalStringData );
			var convertedData = ( byte[] ) objectData;

			Assert.AreEqual( originalData.Length, convertedData.Length, "The converted data length is invalid." );
			for ( int i = 0; i < originalData.Length; i++ )
			{
				Assert.AreEqual( originalData[ i ], convertedData[ i ], "The converted data is invalid." );
			}
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Bool value.
		/// </summary>
		[Test]
		public void ConvertFromString_Bool( )
		{
			const bool originalData = true;
			string originalStringData = originalData.ToString( );

			object objectData = DatabaseType.BoolType.ConvertFromString( originalStringData );
			var convertedData = ( bool ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		///// <summary>
		///// Verifies that the ConvertFromString method correctly converts a Byte value.
		///// </summary>
		//[Test]
		//public void ConvertFromString_Byte()
		//{
		//    byte originalData = 123;
		//    string originalStringData = originalData.ToString();

		//    object objectData = DatabaseType.ByteType.ConvertFromString(originalStringData);
		//    byte convertedData = (byte)objectData;

		//    Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
		//}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Binary value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Binary( )
		{
			var originalData = new byte[]
				{
					1, 2, 3, 4, 0x8
				};
			string originalStringData = Convert.ToBase64String( originalData );

			object objectData = DatabaseType.BinaryType.ConvertFromString( originalStringData );
			string convertedStringData = DatabaseType.BinaryType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Bool value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Bool( )
		{
			const bool originalData = true;
			string originalStringData = originalData.ToString( );

			object objectData = DatabaseType.BoolType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.BoolType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Currency value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Currency( )
		{
			const decimal originalData = 74845739.345M;
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.CurrencyType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.CurrencyType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Date value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Date( )
		{
			var originalData = new DateTime( 2011, 4, 4 );
			string originalStringData = originalData.ToString( DateType.DateFormatString );

			object objectData = DatabaseType.DateType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.DateType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a DateTime value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_DateTime( )
		{
			var originalData = new DateTime( 2011, 4, 4, 4, 6, 7, DateTimeKind.Utc );
			string originalStringData = originalData.ToString( DateTimeType.DateTimeFormatString );

			object objectData = DatabaseType.DateTimeType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.DateTimeType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Decimal value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Decimal( )
		{
			const decimal originalData = 83673838.3335M;
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.DecimalType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.DecimalType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Guid value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Guid( )
		{
			Guid originalData = Guid.NewGuid( );
			string originalStringData = originalData.ToString( "B" );

			object objectData = DatabaseType.GuidType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.GuidType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Int32 value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Int32( )
		{
			const int originalData = 12387352;
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.Int32Type.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.Int32Type.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a String value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_String_Empty( )
		{
			const string originalData = "";

			object objectData = DatabaseType.StringType.ConvertFromString( originalData );
			object convertedStringData = DatabaseType.StringType.ConvertToString( objectData );

			Assert.AreEqual( originalData, convertedStringData, "The converted data is invalid." );
		}


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_String_Null()
        {
            const string originalData = null;

            object objectData = DatabaseType.StringType.ConvertFromString(originalData);
            object convertedStringData = DatabaseType.StringType.ConvertToString(objectData);

            // This is the current behavior, but is it correct??
            Assert.AreEqual(originalData, convertedStringData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_ConvertToString_String()
        {
            const string originalData = "string data";

            object objectData = DatabaseType.StringType.ConvertFromString(originalData);
            object convertedStringData = DatabaseType.StringType.ConvertToString(objectData);

            Assert.AreEqual(originalData, convertedStringData, "The converted data is invalid.");
        }


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Time value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Time( )
		{
			var originalData = new TimeSpan( 13, 4, 13 );
			string originalStringData = originalData.ToString( "c" );

			object objectData = DatabaseType.TimeType.ConvertFromString( originalStringData );
			object convertedStringData = DatabaseType.TimeType.ConvertToString( objectData );

			Assert.AreEqual( originalStringData, convertedStringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString and ConvertToString methods correctly convert a Xml value.
		/// </summary>
		[Test]
		public void ConvertFromString_ConvertToString_Xml( )
		{
			const string originalData = "<xml></xml>";

			object objectData = DatabaseType.XmlType.ConvertFromString( originalData );
			object convertedStringData = DatabaseType.XmlType.ConvertToString( objectData );

			Assert.AreEqual( originalData, convertedStringData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Currency value.
		/// </summary>
		[Test]
		public void ConvertFromString_Currency( )
		{
			const decimal originalData = 74845739.345M;
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.CurrencyType.ConvertFromString( originalStringData );
			var convertedData = ( Decimal ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Date value.
		/// </summary>
		[Test]
		public void ConvertFromString_Date( )
		{
			var originalData = new DateTime( 2011, 4, 4 );
			string originalStringData = originalData.ToString( DateType.DateFormatString );

			object objectData = DatabaseType.DateType.ConvertFromString( originalStringData );
			var convertedData = ( DateTime ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a DateTime value.
		/// </summary>
		[Test]
		public void ConvertFromString_DateTime( )
		{
			var originalData = new DateTime( 2011, 4, 4, 4, 6, 7 );
			string originalStringData = originalData.ToString( DateTimeType.DateTimeFormatString );

			object objectData = DatabaseType.DateTimeType.ConvertFromString( originalStringData );
			var convertedData = ( DateTime ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a DateTime value.
		/// </summary>
		[Test]
		public void ConvertFromString_DateTime_NotCustomFormat( )
		{
			var originalData = new DateTime( 2011, 4, 4, 4, 6, 7, DateTimeKind.Utc );
			string originalStringData = originalData.ToString( "u" );

			object objectData = DatabaseType.DateTimeType.ConvertFromString( originalStringData );
			var convertedData = ( DateTime ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Date value.
		/// </summary>
		[Test]
		public void ConvertFromString_Date_NotCustomFormat( )
		{
			var originalData = new DateTime( 2011, 4, 4 );
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.DateType.ConvertFromString( originalStringData );
			var convertedData = ( DateTime ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Decimal value.
		/// </summary>
		[Test]
		public void ConvertFromString_Decimal( )
		{
			const decimal originalData = 83673838.3335M;
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.DecimalType.ConvertFromString( originalStringData );
			var convertedData = ( Decimal ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Guid value.
		/// </summary>
		[Test]
		public void ConvertFromString_Guid( )
		{
			Guid originalData = Guid.NewGuid( );
			string originalStringData = originalData.ToString( );

			object objectData = DatabaseType.GuidType.ConvertFromString( originalStringData );
			var convertedData = ( Guid ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Int32 value.
		/// </summary>
		[Test]
		public void ConvertFromString_Int32( )
		{
			const int originalData = 12387352;
			string originalStringData = originalData.ToString( CultureInfo.InvariantCulture );

			object objectData = DatabaseType.Int32Type.ConvertFromString( originalStringData );
			var convertedData = ( Int32 ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertFromString method throws the expected expection
		///     when invalid databasetypes are specified.
		/// </summary>
		[Test]
		public void ConvertFromString_InvalidDatabaseType( )
		{
			var supportedTypes = new List<Type>
				{
					typeof ( AutoIncrementType ),
					typeof ( BinaryType ),
					typeof ( BoolType ),
					typeof ( CurrencyType ),
					typeof ( DateType ),
					typeof ( DateTimeType ),
					typeof ( DecimalType ),
					typeof ( GuidType ),
					typeof ( Int32Type ),
					typeof ( StructureLevelsType ),
					typeof ( ChoiceRelationshipType ),
					typeof ( InlineRelationshipType ),
					typeof ( StringType ),
					typeof ( TimeType ),
					typeof ( XmlType ),
					typeof ( IdentifierType )
				};

			IEnumerable<Type> list = typeof ( DatabaseType ).Assembly.GetTypes( ).Where( a => typeof ( DatabaseType ).IsAssignableFrom( a ) && !a.IsAbstract );

			foreach ( Type databaseType in list )
			{
				if ( !supportedTypes.Contains( databaseType ) )
				{
					bool exceptionThrown = false;

					try
					{
						var dbType = Activator.CreateInstance( databaseType ) as DatabaseType;
						if ( dbType != null )
						{
							dbType.ConvertFromString( "value" );
						}
					}
					catch ( InvalidOperationException )
					{
						exceptionThrown = true;
					}

					Assert.IsTrue( exceptionThrown, "The expected exception for DatabaseType {0} was not thrown", databaseType );
				}
			}
		}

		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a null value.
		/// </summary>
		[Test]
		public void ConvertFromString_Null( )
		{
			object obj = DatabaseType.XmlType.ConvertFromString( null );
			Assert.IsNull( obj, "Failed to convert null value" );
		}

	    /// <summary>
	    ///     Verifies that the ConvertFromString method correctly converts a String value.
	    /// </summary>
	    [Test]
	    public void ConvertFromString_String()
	    {
	        const string originalData = "string data";

	        object objectData = DatabaseType.StringType.ConvertFromString(originalData);
	        var convertedData = (string) objectData;

	        Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
	    }

	    /// <summary>
		///     Verifies that the ConvertFromString method correctly converts a String value.
		/// </summary>
		[Test]
		public void ConvertFromString_String_Empty( )
		{
			const string originalData = "";

			object objectData = DatabaseType.StringType.ConvertFromString( originalData );
			var convertedData = ( string ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

        /// <summary>
        ///     Verifies that the ConvertFromString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertFromString_String_Null()
        {
            const string originalData = null;

            object objectData = DatabaseType.StringType.ConvertFromString(originalData);
            var convertedData = (string)objectData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }

		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Time value.
		/// </summary>
		[Test]
		public void ConvertFromString_Time( )
		{
			var originalData = new TimeSpan( 13, 4, 13 );
			var originalStringData = originalData.ToString( "c" );

            // Trying to rid TimeType of the DateTime/Timespan duality. Always deal in DateTime now.
			var objectData = DatabaseType.TimeType.ConvertFromString( originalStringData );
			var convertedData = (DateTime) objectData;

			Assert.AreEqual( originalData, convertedData.TimeOfDay, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertFromString method correctly converts a Xml value.
		/// </summary>
		[Test]
		public void ConvertFromString_Xml( )
		{
			const string originalData = "<xml></xml>";

			object objectData = DatabaseType.XmlType.ConvertFromString( originalData );
			var convertedData = ( string ) objectData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Test that the ConvertFromType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( InvalidOperationException ) )]
		public void ConvertFromType_InvalidType( )
		{
			DatabaseType.ConvertFromType( typeof ( Type ) );
		}

		/// <summary>
		///     Test that the ConvertFromType method correctly converts Type values.
		/// </summary>
		[Test]
		public void ConvertFromType_ValidTypes( )
		{
			var typeMappings = new Dictionary<Type, DatabaseType>
				{
					{
						typeof ( bool ), DatabaseType.BoolType
					},
					//{typeof(byte), DatabaseType.ByteType},
					{
						typeof ( DateTime ), DatabaseType.DateTimeType
					},
					{
						typeof ( Decimal ), DatabaseType.DecimalType
					},
					{
						typeof ( Guid ), DatabaseType.GuidType
					},
					{
						typeof ( Int32 ), DatabaseType.Int32Type
					},
					{
						typeof ( String ), DatabaseType.StringType
					},
					{
						typeof ( TimeSpan ), DatabaseType.TimeType
					}
				};

			foreach ( var kvp in typeMappings )
			{
				DatabaseType expectedType = kvp.Value;
				DatabaseType actualType = DatabaseType.ConvertFromType( kvp.Key );

				Assert.AreEqual( expectedType.GetType( ), actualType.GetType( ), "Type {0} failed to convert to DatabaseType", kvp.Key.FullName );
			}
		}

		/// <summary>
		///     Test that the ConvertToDbType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( InvalidOperationException ) )]
		public void ConvertToDbType_InvalidType( )
		{
			DatabaseType.UnknownType.GetDbType( );
		}

		/// <summary>
		///     Test that the ConvertToDbType method correctly converts DatabaseType values.
		/// </summary>
		[Test]
		public void ConvertToDbType_ValidTypes( )
		{
			var typeMappings = new Dictionary<Type, DbType>
				{
					{
						typeof ( AutoIncrementType ), DbType.Int32
					},
					{
						typeof ( BinaryType ), DbType.Binary
					},
					{
						typeof ( BoolType ), DbType.Boolean
					},
					{
						typeof ( CurrencyType ), DbType.Currency
					},
					{
						typeof ( DateType ), DbType.Date
					},
					{
						typeof ( DateTimeType ), DbType.DateTime2
					},
					{
						typeof ( DecimalType ), DbType.Decimal
					},
					{
						typeof ( GuidType ), DbType.Guid
					},
					{
						typeof ( Int32Type ), DbType.Int32
					},
					{
						typeof ( StringType ), DbType.String
					},
					{
						typeof ( TimeType ), DbType.Time
					},
					{
						typeof ( XmlType ), DbType.Xml
					},
					{
						typeof ( IdentifierType ), DbType.Int64
					}
				};

			IEnumerable<Type> list = typeof ( DatabaseType ).Assembly.GetTypes( ).Where( a => typeof ( DatabaseType ).IsAssignableFrom( a ) && !a.IsAbstract );

			foreach ( Type databaseType in list )
			{
				if ( databaseType == typeof ( UnknownType ) || databaseType == typeof ( StructureLevelsType ) || databaseType == typeof ( ChoiceRelationshipType ) || databaseType == typeof ( InlineRelationshipType ) )
				{
					continue;
				}

				//cretae an instance
				var dbType = Activator.CreateInstance( databaseType ) as DatabaseType;

				DbType expectedDbType = typeMappings[ databaseType ];
				if ( dbType != null )
				{
					DbType actualDbType = dbType.GetDbType( );
					Assert.AreEqual( expectedDbType, actualDbType, "DatabaseType {0} failed to convert to DbType", databaseType );
				}
			}
		}

		/// <summary>
		///     Test that the ConvertToSqlDbType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( InvalidOperationException ) )]
		public void ConvertToSqlDbType_InvalidType( )
		{
			DatabaseType.UnknownType.GetSqlDbType( );
		}

		/// <summary>
		///     Test that the ConvertToSqlDbType method correctly converts DatabaseType values.
		/// </summary>
		[Test]
		public void ConvertToSqlDbType_ValidTypes( )
		{
			var typeMappings = new Dictionary<Type, SqlDbType>
				{
					{
						typeof ( AutoIncrementType ), SqlDbType.Int
					},
					{
						typeof ( BinaryType ), SqlDbType.Binary
					},
					{
						typeof ( BoolType ), SqlDbType.Bit
					},
					{
						typeof ( CurrencyType ), SqlDbType.Money
					},
					{
						typeof ( DateType ), SqlDbType.Date
					},
					{
						typeof ( DateTimeType ), SqlDbType.DateTime2
					},
					{
						typeof ( DecimalType ), SqlDbType.Decimal
					},
					{
						typeof ( GuidType ), SqlDbType.UniqueIdentifier
					},
					{
						typeof ( Int32Type ), SqlDbType.Int
					},
					{
						typeof ( StringType ), SqlDbType.NVarChar
					},
					{
						typeof ( TimeType ), SqlDbType.Time
					},
					{
						typeof ( XmlType ), SqlDbType.Xml
					},
					{
						typeof ( IdentifierType ), SqlDbType.BigInt
					}
				};

			IEnumerable<Type> list = typeof ( DatabaseType ).Assembly.GetTypes( ).Where( a => typeof ( DatabaseType ).IsAssignableFrom( a ) && !a.IsAbstract );

			foreach ( Type databaseType in list )
			{
				if ( databaseType == typeof ( UnknownType ) || databaseType == typeof ( StructureLevelsType ) || databaseType == typeof ( ChoiceRelationshipType ) || databaseType == typeof ( InlineRelationshipType ) )
				{
					continue;
				}

				//create instance
				var dbType = Activator.CreateInstance( databaseType ) as DatabaseType;

				SqlDbType expectedSqlDbType = typeMappings[ databaseType ];
				if ( dbType != null )
				{
					SqlDbType actualSqlDbType = dbType.GetSqlDbType( );
					Assert.AreEqual( expectedSqlDbType, actualSqlDbType, "DatabaseType {0} failed to convert to SqlDbType", databaseType );
				}
			}
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Binary value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Binary( )
		{
			// TODO - This may change if binary type is no longer stored as a base64 string
			var originalData = new byte[]
				{
					1, 2, 3, 4, 5, 0xF
				};
			string stringData = DatabaseType.BinaryType.ConvertToSqlString( originalData );
			byte[] convertedData = Convert.FromBase64String( stringData );

			Assert.AreEqual( originalData.Length, convertedData.Length, "The length of the converted data is invalid." );
			for ( int i = 0; i < originalData.Length; i++ )
			{
				Assert.AreEqual( originalData[ i ], convertedData[ i ], "The converted data is invalid." );
			}
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Bool value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Bool( )
		{
			string stringData = DatabaseType.BoolType.ConvertToSqlString( true );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<bool>( DatabaseType.BoolType, 0, stringData );

			Assert.AreEqual( true, convertedData, "The converted data is invalid." );

			stringData = DatabaseType.BoolType.ConvertToSqlString( false );

			// Verify that SQL server can convert the above string back to the specified type
			convertedData = SqlServerCastData<bool>( DatabaseType.BoolType, 0, stringData );

			Assert.AreEqual( false, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Currency value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Currency( )
		{
			const decimal originalData = 8726387463.4562M;
			string stringData = DatabaseType.CurrencyType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<Decimal>( DatabaseType.CurrencyType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Date value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Date( )
		{
			var originalData = new DateTime( 2011, 5, 23 );
			string stringData = DatabaseType.DateType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<DateTime>( DatabaseType.DateType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a DateTime value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_DateTime( )
		{
			var originalData = new DateTime( 2011, 5, 23, 5, 4, 3 );
			string stringData = DatabaseType.DateTimeType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<DateTime>( DatabaseType.DateTimeType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a DbNull value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_DbNull( )
		{
			string stringData = DatabaseType.BinaryType.ConvertToSqlString( DBNull.Value );
			Assert.AreEqual( string.Empty, stringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Decimal value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Decimal( )
		{
			const decimal originalData = 8722387463;
			string stringData = DatabaseType.DecimalType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<Decimal>( DatabaseType.DecimalType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Guid value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Guid( )
		{
			Guid originalData = Guid.NewGuid( );
			string stringData = DatabaseType.GuidType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<Guid>( DatabaseType.GuidType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Int32 value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Int32( )
		{
			const int originalData = 476685;
			string stringData = DatabaseType.Int32Type.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<Int32>( DatabaseType.Int32Type, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertToSqlString method throws the expected expection
		///     when invalid databasetypes are specified.
		/// </summary>
		[Test]
		public void ConvertToSqlString_InvalidDatabaseType( )
		{
			var supportedTypes = new List<Type>
				{
					typeof ( AutoIncrementType ),
					typeof ( BinaryType ),
					typeof ( BoolType ),
					typeof ( CurrencyType ),
					typeof ( DateType ),
					typeof ( DateTimeType ),
					typeof ( DecimalType ),
					typeof ( GuidType ),
					typeof ( Int32Type ),
					typeof ( StructureLevelsType ),
					typeof ( ChoiceRelationshipType ),
					typeof ( InlineRelationshipType ),
					typeof ( StringType ),
					typeof ( TimeType ),
					typeof ( XmlType ),
					typeof ( IdentifierType )
				};

			//Array values = Enum.GetValues(typeof(DatabaseType));
			IEnumerable<Type> list = typeof ( DatabaseType ).Assembly.GetTypes( ).Where( a => typeof ( DatabaseType ).IsAssignableFrom( a ) && !a.IsAbstract );

			foreach ( Type databaseType in list )
			{
				if ( !supportedTypes.Contains( databaseType ) )
				{
					bool exceptionThrown = false;

					try
					{
						var dbType = Activator.CreateInstance( databaseType ) as DatabaseType;
						if ( dbType != null )
						{
							dbType.ConvertToSqlString( "" );
						}
					}
					catch ( InvalidOperationException )
					{
						exceptionThrown = true;
					}

					Assert.IsTrue( exceptionThrown, "The expected exception for DatabaseType {0} was not thrown", databaseType );
				}
			}
		}

		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a null value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Null( )
		{
			string stringData = DatabaseType.BinaryType.ConvertToSqlString( null );
			Assert.AreEqual( string.Empty, stringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a String value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_String( )
		{
			const string originalData = "original data";
			string stringData = DatabaseType.StringType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<string>( DatabaseType.StringType, 20, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Time value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Time( )
		{
			var originalData = new TimeSpan( 10, 34, 45 );
			string stringData = DatabaseType.TimeType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<TimeSpan>( DatabaseType.TimeType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToSqlString method correctly converts a Xml value.
		/// </summary>
		[Test]
		public void ConvertToSqlString_Xml( )
		{
			const string originalData = "<originalXml>This is a test</originalXml>";
			string stringData = DatabaseType.XmlType.ConvertToSqlString( originalData );

			// Verify that SQL server can convert the above string back to the specified type
			var convertedData = SqlServerCastData<string>( DatabaseType.XmlType, 0, stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Binary value.
		/// </summary>
		[Test]
		public void ConvertToString_Binary( )
		{
			var originalData = new byte[]
				{
					1, 2, 3, 4, 5, 0xF
				};
			string stringData = DatabaseType.BinaryType.ConvertToString( originalData );
			byte[] convertedData = Convert.FromBase64String( stringData );

			Assert.AreEqual( originalData.Length, convertedData.Length, "The length of the converted data is invalid." );
			for ( int i = 0; i < originalData.Length; i++ )
			{
				Assert.AreEqual( originalData[ i ], convertedData[ i ], "The converted data is invalid." );
			}
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Bool value.
		/// </summary>
		[Test]
		public void ConvertToString_Bool( )
		{
			string stringData = DatabaseType.BoolType.ConvertToString( true );
			bool convertedData = bool.Parse( stringData );

			Assert.AreEqual( true, convertedData, "The converted data is invalid." );

			stringData = DatabaseType.BoolType.ConvertToString( false );
			convertedData = bool.Parse( stringData );

			Assert.AreEqual( false, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Currency value.
		/// </summary>
		[Test]
		public void ConvertToString_Currency( )
		{
			const decimal originalData = 8726387463.4562M;
			string stringData = DatabaseType.CurrencyType.ConvertToString( originalData );
			Decimal convertedData = Decimal.Parse( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Date value.
		/// </summary>
		[Test]
		public void ConvertToString_Date( )
		{
			var originalData = new DateTime( 2011, 5, 23 );
			string stringData = DatabaseType.DateType.ConvertToString( originalData );
			DateTime convertedData = DateTime.Parse( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a DateTime value.
		/// </summary>
		[Test]
		public void ConvertToString_DateTime( )
		{
			var originalData = new DateTime( 2011, 5, 23, 5, 4, 3 );
			string stringData = DatabaseType.DateTimeType.ConvertToString( originalData );
			DateTime convertedData = DateTime.Parse( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a DbNull value.
		/// </summary>
		[Test]
		public void ConvertToString_DbNull( )
		{
			string stringData = DatabaseType.BinaryType.ConvertToString( DBNull.Value );
			Assert.AreEqual( string.Empty, stringData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Decimal value.
		/// </summary>
		[Test]
		public void ConvertToString_Decimal( )
		{
			const decimal originalData = 8722387463.4562M;
			string stringData = DatabaseType.DecimalType.ConvertToString( originalData );
			Decimal convertedData = Decimal.Parse( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Guid value.
		/// </summary>
		[Test]
		public void ConvertToString_Guid( )
		{
			Guid originalData = Guid.NewGuid( );
			string stringData = DatabaseType.GuidType.ConvertToString( originalData );
			var convertedData = new Guid( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Int32 value.
		/// </summary>
		[Test]
		public void ConvertToString_Int32( )
		{
			const int originalData = 476685;
			string stringData = DatabaseType.Int32Type.ConvertToString( originalData );
			Int32 convertedData = Int32.Parse( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Verifies that the ConvertToString method throws the expected expection
		///     when invalid databasetypes are specified.
		/// </summary>
		[Test]
		public void ConvertToString_InvalidDatabaseType( )
		{
			var supportedTypes = new List<Type>
				{
					typeof ( AutoIncrementType ),
					typeof ( BinaryType ),
					typeof ( BoolType ),
					typeof ( CurrencyType ),
					typeof ( DateType ),
					typeof ( DateTimeType ),
					typeof ( DecimalType ),
					typeof ( GuidType ),
					typeof ( Int32Type ),
					typeof ( StructureLevelsType ),
					typeof ( ChoiceRelationshipType ),
					typeof ( InlineRelationshipType ),
					typeof ( StringType ),
					typeof ( TimeType ),
					typeof ( XmlType ),
					typeof ( IdentifierType )
				};

			IEnumerable<Type> list = typeof ( DatabaseType ).Assembly.GetTypes( ).Where( a => typeof ( DatabaseType ).IsAssignableFrom( a ) && !a.IsAbstract );

			//Array values = list.ToArray();
			foreach ( Type databaseType in list )
			{
				if ( !supportedTypes.Contains( databaseType ) )
				{
					bool exceptionThrown = false;

					try
					{
						var dbType = Activator.CreateInstance( databaseType ) as DatabaseType;
						if ( dbType != null )
						{
							dbType.ConvertToString( " " );
						}
					}
					catch ( InvalidOperationException )
					{
						exceptionThrown = true;
					}

					Assert.IsTrue( exceptionThrown, "The expected exception for DatabaseType {0} was not thrown", databaseType );
				}
			}
		}

		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a null value.
		/// </summary>
		[Test]
		public void ConvertToString_Null( )
		{
			string stringData = DatabaseType.BinaryType.ConvertToString( null );
			Assert.AreEqual( string.Empty, stringData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a String value.
		/// </summary>
		[Test]
		public void ConvertToString_String( )
		{
			const string originalData = "original data";
			string stringData = DatabaseType.StringType.ConvertToString( originalData );
			string convertedData = stringData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertToString_String_Empty()
        {
            const string originalData = "";
            string stringData = DatabaseType.StringType.ConvertToString(originalData);
            string convertedData = stringData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


        /// <summary>
        ///     Verifies that the ConvertToString method correctly converts a String value.
        /// </summary>
        [Test]
        public void ConvertToString_String_Null()
        {
            const string originalData = null;
            string stringData = DatabaseType.StringType.ConvertToString(originalData);
            string convertedData = stringData;

            Assert.AreEqual(originalData, convertedData, "The converted data is invalid.");
        }


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Time value.
		/// </summary>
		[Test]
		public void ConvertToString_Time( )
		{
			var originalData = new TimeSpan( 10, 34, 45 );
			string stringData = DatabaseType.TimeType.ConvertToString( originalData );
			TimeSpan convertedData = TimeSpan.Parse( stringData );

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}


		/// <summary>
		///     Verifies that the ConvertToString method correctly converts a Xml value.
		/// </summary>
		[Test]
		public void ConvertToString_Xml( )
		{
			const string originalData = "<originalXml></originalXml>";
			string stringData = DatabaseType.XmlType.ConvertToString( originalData );
			string convertedData = stringData;

			Assert.AreEqual( originalData, convertedData, "The converted data is invalid." );
		}

		/// <summary>
		///     Test that the ConvertToType method throws the correct exception
		///     for invalid types.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( InvalidOperationException ) )]
		public void ConvertToType_InvalidType( )
		{
			DatabaseType.UnknownType.GetRunTimeType( );
		}

		/// <summary>
		///     Test that the ConvertToType method correctly converts DatabaseType values.
		/// </summary>
		[Test]
		public void ConvertToType_ValidTypes( )
		{
			var typeMappings = new Dictionary<Type, Type>
				{
					{
						typeof( AutoIncrementType ), typeof( Int32 )
					},
					{
						typeof ( BinaryType ), typeof ( byte[] )
					},
					{
						typeof ( BoolType ), typeof ( bool )
					},
					{
						typeof ( CurrencyType ), typeof ( Decimal )
					},
					{
						typeof ( DateType ), typeof ( DateTime )
					},
					{
						typeof ( DateTimeType ), typeof ( DateTime )
					},
					{
						typeof ( DecimalType ), typeof ( Decimal )
					},
					{
						typeof ( GuidType ), typeof ( Guid )
					},
					{
						typeof ( Int32Type ), typeof ( Int32 )
					},
					{
						typeof ( StringType ), typeof ( string )
					},
					{
						typeof ( TimeType ), typeof ( TimeSpan )
					},
					{
						typeof ( XmlType ), typeof ( string )
					},
					{
						typeof ( IdentifierType ), typeof ( long )
					}
				};

			//Array values = Enum.GetValues(typeof(DatabaseType));
			IEnumerable<Type> list = typeof ( DatabaseType ).Assembly.GetTypes( ).Where( a => typeof ( DatabaseType ).IsAssignableFrom( a ) && !a.IsAbstract );

			foreach ( Type databaseType in list )
			{
				if ( databaseType == typeof ( UnknownType ) || databaseType == typeof ( StructureLevelsType ) || databaseType == typeof ( ChoiceRelationshipType ) || databaseType == typeof ( InlineRelationshipType ) )
				{
					continue;
				}

				var dbType = Activator.CreateInstance( databaseType ) as DatabaseType;

				Type expectedType = typeMappings[ databaseType ];
				if ( dbType != null )
				{
					Type actualType = dbType.GetRunTimeType( );
					Assert.AreEqual( expectedType, actualType, "DatabaseType {0} failed to convert to Type", databaseType );
				}
			}
		}

        [Test]
        public void GetEntityXmlId_Null( )
        {
            Assert.AreEqual( 0, DatabaseTypeHelper.GetEntityXmlId( null ) );
            Assert.AreEqual( 0, DatabaseTypeHelper.GetEntityXmlId( "" ) );
        }

        [Test]
        public void GetEntityXmlId_Canonical( )
        {
            Assert.AreEqual( 123, DatabaseTypeHelper.GetEntityXmlId( @"<e id=""123"" text=""whatever"" />" ) );
        }

        [Test]
        public void GetEntityXmlId_General( )
        {
            Assert.AreEqual( 123, DatabaseTypeHelper.GetEntityXmlId( @"<e text=""whatever"" id=""123"" />" ) );
        }
	}
}