// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Specialized;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.Core.Test
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     This is a test class for the CallbackArgs type
	/// </summary>
	[TestFixture]
	public class CallbackArgsTests
	{
		/// <summary>
		///     Tests that the default constructor creates an empty parameters collection
		/// </summary>
		[Test]
		public void Constructor_EmptyDictionary_CreatesEmptyParameters( )
		{
			var args = new CallbackArgs( );
			Assert.IsTrue( args.Parameters.Count == 0 );
		}

		/// <summary>
		///     Tests that the overloaded constructor creates a non-empty parameters collection
		/// </summary>
		[Test]
		public void Constructor_NonEmptyDictionary_CreatesNonEmptyParameters( )
		{
			var parameters = new StringDictionary
				{
					{
						"key1", "value1"
					},
					{
						"key2", "value2"
					}
				};
			var args = new CallbackArgs( parameters );
			Assert.IsTrue( args.Parameters.Count > 0 );
		}

		/// <summary>
		///     Tests that the parameters property returns a valid collection
		/// </summary>
		[Test]
		public void GetParameters_NonEmptyCollection_ReturnsCorrectKeyValuePair( )
		{
			var parameters = new StringDictionary
				{
					{
						"key1", "value1"
					},
					{
						"key2", "value2"
					}
				};
			var args = new CallbackArgs( parameters );
			Assert.IsTrue( args.Parameters.Count > 0 );
			Assert.AreEqual( args.Parameters[ "key1" ], "value1" );
			Assert.AreEqual( args.Parameters[ "key2" ], "value2" );
		}
	}
}