// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Entity tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class TemporaryEntityTests
	{
		/// <summary>
		///     Tests the type of the entity_ constructor_ type_ valid.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestTemporaryEntity_Constructor_Type_ValidType( )
		{
			Type type = typeof ( Person );
			var person = new Entity( type ).As<Person>( );

			Assert.IsTrue( person.IsOfType.Count == 1, "Entity should have a type" );
			Assert.AreEqual( "core:person", person.IsOfType.First( ).Alias );
		}

		/// <summary>
		///     Tests the type of the entity_ constructor_ type_ valid.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestTemporaryEntity_TypeConstructor( )
		{
			var person = new Person( );

			Assert.IsTrue( person.IsOfType.Count == 1, "Entity should have a type" );
			Assert.AreEqual( "core:person", person.IsOfType.First( ).Alias );
		}
	}
}