// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace EDC.Test.DelegatesTests
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     Tests for EDC.Common.Delegates class
	/// </summary>
	[TestFixture]
	public class DelegatesTests
	{
		/// <summary>
		///     Tests the IEnumerable[T].To extension method that converts an enumeration to a manually defined collection type.
		/// </summary>
		[Test]
		public void Test_Copy( )
		{
			Guid guid = Guid.NewGuid( );
			var source = new HashSet<Guid>
				{
					guid
				};

			// Test copy
			List<Guid> target = source.To<List<Guid>, Guid>( );
            target.Should().BeEquivalentTo(guid);

			// Test inequality
			var source2 = new List<Guid>( );
			target = source2.Copy<List<Guid>, Guid>( );
            target.As<object>().Should().NotBeSameAs(source2);
		}

		/// <summary>
		///     Tests the IList.Copy extension method.
		/// </summary>
		[Test]
		public void Test_ListCopy( )
		{
			var list1 = new List<int>
				{
					1,
					2
				};
			List<int> list2 = list1.Copy( );

		    list2.Should().BeEquivalentTo(list1); // data has been copied
            list2.As<object>().Should().NotBeSameAs(list1); // but its not the same list
		}

		/// <summary>
		///     Tests the Delegates.ListOfOne method.
		/// </summary>
		[Test]
		public void Test_ListOfOne( )
		{
			Guid id = Guid.NewGuid( );
			List<Guid> list = EDC.Common.Delegates.ListOfOne( id );

			Assert.AreEqual( 1, list.Count, "Count" );
            Assert.AreEqual(id, list.First(), "Value comparison");

            list.Should().BeEquivalentTo(id);
		}

		/// <summary>
		///     Tests the Delegates.ListOfOne method with a null value
		/// </summary>
		[Test]
		public void Test_ListOfOne_Null( )
		{
			object obj = null;
// ReSharper disable ExpressionIsAlwaysNull
			List<object> list = EDC.Common.Delegates.ListOfOne( obj );
// ReSharper restore ExpressionIsAlwaysNull

		    list.Should().HaveCount(1);
		    list.First().Should().BeNull();
		}

		/// <summary>
		///     Tests the Delegates.RemoveAll method with a null value
		/// </summary>
		[Test]
		public void Test_RemoveAll( )
		{
			IList<int> list = new List<int>
				{
					1,
					2,
					1
				};
			list.RemoveAll( x => x == 1 );

            list.Should().BeEquivalentTo(2);
		}

		/// <summary>
		///     Tests the IEnumerable[T].To extension method that converts an enumeration to a manually defined collection type.
		/// </summary>
		[Test]
		public void Test_To( )
		{
			// The difference between 'To' and 'Copy' is that 'To' may pass the original object through if of the correct type.

			Guid guid = Guid.NewGuid( );
			var source = new HashSet<Guid>
				{
					guid
				};

			// Test copy
			List<Guid> target = source.To<List<Guid>, Guid>( );
			Assert.AreEqual( guid, target[ 0 ], "Copy values" );

			// Test inequality
			var source2 = new List<Guid>( );
			target = source2.To<List<Guid>, Guid>( );

		    target.As<object>().Should().BeSameAs(source2);
		}

		/// <summary>
		///     Tests the Delegates.TryCatch method to ensure it executes the action and catches any exception.
		/// </summary>
		[Test]
		public void Test_TryCatch( )
		{
			bool passed = false;

			Action<bool> work = delegate( bool b )
				{
					passed = b;
					throw new Exception( "This should get caught." );
				};

			Action<bool> safeWork = work.TryCatch( );
			safeWork( true );
		    passed.Should().BeTrue();
		}

        /// <summary>
        ///     Tests WhereType
        /// </summary>
        [Test]
        public void Test_WhereType( )
        {
            Child child = new Child();
            Parent parent = new Parent( );
            List<Parent> list = new List<Parent>( ) { parent, child };

            IEnumerable<Child> filtered = list.WhereType<Child>( );
            
            var res = filtered.ToList( );
            Assert.That( res, Has.Count.EqualTo( 1 ).And.Contains( child ) );
        }

        class Parent
        {
        }

        class Child : Parent
        {
        }


        [Test]
        public void Test_WhereNotNull( )
        {
            List<string> list = new List<string>() { null, "test", null };
            var res = list.WhereNotNull( );
            Assert.That( res, Is.Not.Null );
            Assert.That( res.Count( ), Is.EqualTo( 1 ) );
            Assert.That( res.First( ), Is.EqualTo( "test" ) );
        }
	}
}