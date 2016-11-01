// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     EntityAlias tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class EntityAliasTests
	{
		/// <summary>
		///     Core namespace.
		/// </summary>
		private const string CoreNamespace = "core";

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ empty.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntityAlias_Constructor_StringString_Empty( )
		{
// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( string.Empty, string.Empty );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ empty namespace.
		/// </summary>
		[Test]
		public void TestEntityAlias_Constructor_StringString_EmptyNamespace( )
		{
			const string aliasString = "abc123";

			var alias = new EntityAlias( string.Empty, aliasString );

			Assert.AreEqual( CoreNamespace, alias.Namespace );
			Assert.AreEqual( aliasString, alias.Alias );
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ empty namespace empty alias.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntityAlias_Constructor_StringString_EmptyNamespaceEmptyAlias( )
		{
// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( string.Empty, string.Empty );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ null.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void TestEntityAlias_Constructor_StringString_Null( )
		{
// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( null, null );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ random namespace.
		/// </summary>
		[Test]
		public void TestEntityAlias_Constructor_StringString_RandomNamespace( )
		{
			const string namespaceString = "xyz789";
			const string aliasString = "abc123";

			var alias = new EntityAlias( namespaceString, aliasString );

			Assert.AreEqual( namespaceString, alias.Namespace );
			Assert.AreEqual( aliasString, alias.Alias );
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ random namespace empty alias.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntityAlias_Constructor_StringString_RandomNamespaceEmptyAlias( )
		{
			const string namespaceString = "xyz789";

// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( namespaceString, string.Empty );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ random.
		/// </summary>
		[Test]
		public void TestEntityAlias_Constructor_StringString_RandomNoNamespace( )
		{
			const string aliasString = "abc123";

			var alias = new EntityAlias( null, aliasString );

			Assert.AreEqual( CoreNamespace, alias.Namespace );
			Assert.AreEqual( aliasString, alias.Alias );
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ empty.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntityAlias_Constructor_String_Empty( )
		{
// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( string.Empty );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ empty namespace.
		/// </summary>
		[Test]
		public void TestEntityAlias_Constructor_String_EmptyNamespace( )
		{
			const string aliasString = ":abc123";

			string[] parts = aliasString.Split( new[]
				{
					':'
				}, StringSplitOptions.RemoveEmptyEntries );

			var alias = new EntityAlias( aliasString );

			Assert.AreEqual( parts[ 0 ], alias.Alias );
			Assert.AreEqual( CoreNamespace, alias.Namespace );
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ empty namespace empty alias.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntityAlias_Constructor_String_EmptyNamespaceEmptyAlias( )
		{
			const string aliasString = ":";

// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( aliasString );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ null.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void TestEntityAlias_Constructor_String_Null( )
		{
// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( null );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ random namespace.
		/// </summary>
		[Test]
		public void TestEntityAlias_Constructor_String_RandomNamespace( )
		{
			const string aliasString = "xyz789:abc123";

			string[] parts = aliasString.Split( new[]
				{
					':'
				}, StringSplitOptions.RemoveEmptyEntries );

			var alias = new EntityAlias( aliasString );

			Assert.AreEqual( parts[ 1 ], alias.Alias );
			Assert.AreEqual( parts[ 0 ], alias.Namespace );
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ random namespace empty alias.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntityAlias_Constructor_String_RandomNamespaceEmptyAlias( )
		{
			const string aliasString = "xyz789:";

// ReSharper disable ObjectCreationAsStatement
			new EntityAlias( aliasString );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity alias_ constructor_ string_ random.
		/// </summary>
		[Test]
		public void TestEntityAlias_Constructor_String_RandomNoNamespace( )
		{
			const string aliasString = "abc123";

			var alias = new EntityAlias( aliasString );

			Assert.AreEqual( aliasString, alias.Alias );
			Assert.AreEqual( CoreNamespace, alias.Namespace );
		}

		/// <summary>
		///     Tests the entity alias_ equality.
		/// </summary>
		[Test]
		public void TestEntityAlias_Equality( )
		{
			var alias1 = new EntityAlias( "abc123" );
			var alias2 = new EntityAlias( "abc123" );

			Assert.AreEqual( alias1, alias2 );
			Assert.IsTrue( alias1.Equals( alias2 ) );
			Assert.IsTrue( alias1 == alias2 );
			Assert.IsFalse( alias1 != alias2 );

			alias1 = new EntityAlias( "xyz789:abc123" );
			alias2 = new EntityAlias( "xyz789:abc123" );

			Assert.AreEqual( alias1, alias2 );
			Assert.IsTrue( alias1.Equals( alias2 ) );
			Assert.IsTrue( alias1 == alias2 );
			Assert.IsFalse( alias1 != alias2 );

			alias1 = new EntityAlias( "xyz789", "abc123" );

			Assert.AreEqual( alias1, alias2 );
			Assert.IsTrue( alias1.Equals( alias2 ) );
			Assert.IsTrue( alias1 == alias2 );
			Assert.IsFalse( alias1 != alias2 );
		}

		/// <summary>
		///     Tests the entity alias_ hash code.
		/// </summary>
		[Test]
		public void TestEntityAlias_HashCode( )
		{
			var alias1 = new EntityAlias( "abc123" );
			var alias2 = new EntityAlias( "abc123" );

			Assert.AreEqual( alias1.GetHashCode( ), alias2.GetHashCode( ) );

			alias1 = new EntityAlias( "xyz789:abc123" );
			alias2 = new EntityAlias( "xyz789:abc123" );

			Assert.AreEqual( alias1.GetHashCode( ), alias2.GetHashCode( ) );

			alias1 = new EntityAlias( "xyz789", "abc123" );

			Assert.AreEqual( alias1.GetHashCode( ), alias2.GetHashCode( ) );
		}

		/// <summary>
		///     Tests the entity alias_ to string.
		/// </summary>
		[Test]
		public void TestEntityAlias_ToString( )
		{
			var alias1 = new EntityAlias( "xyz789:abc123" );
			var alias2 = new EntityAlias( "xyz789", "abc123" );

			Assert.IsTrue( alias1.ToString( ) == alias2.ToString( ) );
			Assert.IsTrue( alias1.ToString( ) == "xyz789:abc123" );
			Assert.IsTrue( alias2.ToString( ) == "xyz789:abc123" );
		}
	}
}