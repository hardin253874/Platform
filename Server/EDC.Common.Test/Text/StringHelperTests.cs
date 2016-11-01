// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Text;
using NUnit.Framework;

namespace EDC.Test.Text
{
	/// <summary>
	///     This is a test class for the StringHelper type
	/// </summary>
	[TestFixture]
	public class StringHelperTests
	{
		/// <summary>
		///     Tests that FromCSV returns a valid array for a valid comma-separated list, which contains additional spacing.
		/// </summary>
		[Test]
		public void FromCSV_ValidCSVWithSpaces_ReturnsCorrectArray( )
		{
			const string input = "1, 2,3,4,  5 , 6  ,7,8,9 ,10";

			object array = StringHelper.FromCsv( input, typeof ( Int16 ) );
			var values = ( Int16[] ) array;

			for ( int x = 1; x <= 10; ++x )
			{
				Assert.AreEqual( x, values[ x - 1 ] );
			}
		}

		/// <summary>
		///     Tests that FromCSV returns a valid array for a valid comma-separated list.
		/// </summary>
		[Test]
		public void FromCSV_ValidGuidCSV_ReturnsCorrectArray( )
		{
			var data = new[]
				{
					new Guid( "{9A795B9A-DE4C-4621-9716-6503798D7786}" ),
					new Guid( "{81901738-0F7C-4674-B4FE-13F603DBB8E9}" ),
					new Guid( "{2944E0D8-F29E-4B7E-AEA1-E594843B6004}" )
				};

			const string input = "{9A795B9A-DE4C-4621-9716-6503798D7786},{81901738-0F7C-4674-B4FE-13F603DBB8E9},{2944E0D8-F29E-4B7E-AEA1-E594843B6004}";

			object array = StringHelper.FromCsv( input, typeof ( Guid ) );
			var values = ( Guid[] ) array;

			for ( int x = 0; x < data.Length; ++x )
			{
				Assert.AreEqual( data[ x ], values[ x ] );
			}
		}

		/// <summary>
		///     Tests that FromCSV returns a valid array for a valid comma-separated list.
		/// </summary>
		[Test]
		public void FromCSV_ValidInt16CSV_ReturnsCorrectArray( )
		{
			const string input = "1,2,3,4,5,6,7,8,9,10";

			object array = StringHelper.FromCsv( input, typeof ( Int16 ) );
			var values = ( Int16[] ) array;

			for ( int x = 1; x <= 10; ++x )
			{
				Assert.AreEqual( x, values[ x - 1 ] );
			}
		}

		/// <summary>
		///     Tests that FromCSV returns a valid array for a valid comma-separated list.
		/// </summary>
		[Test]
		public void FromCSV_ValidInt32CSV_ReturnsCorrectArray( )
		{
			const string input = "1,2,3,4,5,6,7,8,9,10";

			object array = StringHelper.FromCsv( input, typeof ( Int32 ) );
			var values = ( Int32[] ) array;

			for ( int x = 1; x <= 10; ++x )
			{
				Assert.AreEqual( x, values[ x - 1 ] );
			}
		}

		/// <summary>
		///     Tests that FromCSV returns a valid array for a valid comma-separated list.
		/// </summary>
		[Test]
		public void FromCSV_ValidInt64CSV_ReturnsCorrectArray( )
		{
			const string input = "1,2,3,4,5,6,7,8,9,10";

			object array = StringHelper.FromCsv( input, typeof ( Int64 ) );
			var values = ( Int64[] ) array;

			for ( int x = 1; x <= 10; ++x )
			{
				Assert.AreEqual( x, values[ x - 1 ] );
			}
		}

		/// <summary>
		///     Tests that ToCSV returns a valid comma-separated list for a valid array of integers.
		/// </summary>
		[Test]
		public void ToCSV_ValidGuidArray_ReturnsCorrectList( )
		{
			var input = new[]
				{
					new Guid( "{9A795B9A-DE4C-4621-9716-6503798D7786}" ),
					new Guid( "{81901738-0F7C-4674-B4FE-13F603DBB8E9}" ),
					new Guid( "{2944E0D8-F29E-4B7E-AEA1-E594843B6004}" )
				};

			string csv = StringHelper.ToCSV( input, "B" );

			Assert.AreEqual( csv.ToUpper( ), "{9A795B9A-DE4C-4621-9716-6503798D7786},{81901738-0F7C-4674-B4FE-13F603DBB8E9},{2944E0D8-F29E-4B7E-AEA1-E594843B6004}" );
		}

		/// <summary>
		///     Tests that ToCSV returns a valid comma-separated list for a valid array of integers.
		/// </summary>
		[Test]
		public void ToCSV_ValidInt16Array_ReturnsCorrectList( )
		{
			var input = new Int16[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10
				};

			string csv = StringHelper.ToCSV( input, "" );

			Assert.AreEqual( csv, "1,2,3,4,5,6,7,8,9,10" );
		}

		/// <summary>
		///     Tests that ToCSV returns a valid comma-separated list for a valid array of integers.
		/// </summary>
		[Test]
		public void ToCSV_ValidInt32Array_ReturnsCorrectList( )
		{
			var input = new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10
				};

			string csv = StringHelper.ToCSV( input, "" );

			Assert.AreEqual( csv, "1,2,3,4,5,6,7,8,9,10" );
		}

		/// <summary>
		///     Tests that ToCSV returns a valid comma-separated list for a valid array of integers.
		/// </summary>
		[Test]
		public void ToCSV_ValidInt64Array_ReturnsCorrectList( )
		{
			var input = new Int64[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10
				};

			string csv = StringHelper.ToCSV( input, "" );

			Assert.AreEqual( csv, "1,2,3,4,5,6,7,8,9,10" );
		}
	}
}