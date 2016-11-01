// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Collections.Generic;
using NUnit.Framework;

namespace EDC.Test.Collections.Generic
{
	/// <summary>
	///     This is a test class for the Pair type
	/// </summary>
	[TestFixture]
	public class PairTests
	{
		/// <summary>
		///     Tests that the overloaded constructor for a Guid pair causes the First property to return the correct value.
		/// </summary>
		[Test]
		public void Constructor_ValidGuidPair_VerifyFirstIsCorrect( )
		{
			Guid first = Guid.NewGuid( );
			Guid second = Guid.NewGuid( );

			var pair = new Pair<Guid, Guid>( first, second );
			Assert.AreEqual( pair.First, first );
		}

		/// <summary>
		///     Tests that the overloaded constructor for a Guid pair causes the Second property to return the correct value.
		/// </summary>
		[Test]
		public void Constructor_ValidGuidPair_VerifySecondIsCorrect( )
		{
			Guid first = Guid.NewGuid( );
			Guid second = Guid.NewGuid( );

			var pair = new Pair<Guid, Guid>( first, second );
			Assert.AreEqual( pair.Second, second );
		}

		/// <summary>
		///     Tests that the overloaded constructor for a string pair causes the First property to return the correct value.
		/// </summary>
		[Test]
		public void Constructor_ValidStringPair_VerifyFirstIsCorrect( )
		{
			var pair = new Pair<string, string>( "string1", "string2" );
			Assert.AreEqual( pair.First, "string1" );
		}

		/// <summary>
		///     Tests that the overloaded constructor for a string pair causes the Second property to return the correct value.
		/// </summary>
		[Test]
		public void Constructor_ValidStringPair_VerifySecondIsCorrect( )
		{
			var pair = new Pair<string, string>( "string1", "string2" );
			Assert.AreEqual( pair.Second, "string2" );
		}

		/// <summary>
		///     Tests Pair equality.
		/// </summary>
		[Test]
		public void EqualityTests( )
		{
			var pair = new Pair<string, string>( null, null );
			Assert.AreNotEqual( pair, null );
			Assert.AreEqual( pair, new Pair<string, string>( null, null ) );

			pair = new Pair<string, string>( null, "hello" );
			Assert.AreNotEqual( pair, new Pair<string, string>( null, null ) );
			Assert.AreNotEqual( pair, new Pair<string, string>( "hello", null ) );
			Assert.AreNotEqual( pair, new Pair<string, string>( "hello", "hello" ) );
			Assert.AreEqual( pair, new Pair<string, string>( null, "hello" ) );

			pair = new Pair<string, string>( "hello", null );
			Assert.AreNotEqual( pair, new Pair<string, string>( null, null ) );
			Assert.AreNotEqual( pair, new Pair<string, string>( "hello", "hello" ) );
			Assert.AreNotEqual( pair, new Pair<string, string>( null, "hello" ) );
			Assert.AreEqual( pair, new Pair<string, string>( "hello", null ) );

			pair = new Pair<string, string>( "hello", "hello" );
			Assert.AreNotEqual( pair, new Pair<string, string>( null, null ) );
			Assert.AreNotEqual( pair, new Pair<string, string>( null, "hello" ) );
			Assert.AreNotEqual( pair, new Pair<string, string>( "hello", null ) );
			Assert.AreEqual( pair, new Pair<string, string>( "hello", "hello" ) );
		}

		/// <summary>
		///     Tests Pair hash codes.
		/// </summary>
		[Test]
		public void HashCodeTests( )
		{
			Assert.IsTrue( new Pair<string, string>( null, null ).GetHashCode( ) == 17 );
			Assert.IsTrue( new Pair<string, string>( null, "hello" ).GetHashCode( ) != 0 );
			Assert.IsTrue( new Pair<string, string>( "hello", null ).GetHashCode( ) != 0 );
			Assert.IsTrue( new Pair<string, string>( null, "hello" ).GetHashCode( ) != 0 );
			Assert.IsTrue( new Pair<string, string>( "hello", "hello" ).GetHashCode( ) != 0 );
		}
	}
}