// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using NUnit.Framework;

namespace EDC.Test.Collections.Generic
{
	[TestFixture]
	public class ChangeTrackerTests
	{
		/// <summary>
		///     Tests the ChangeTracker class.
		/// </summary>
		[Test]
		public void ChangeTracker_BaseCollection( )
		{
			var baseCollection = new List<string>
				{
					"a",
					"b",
					"c"
				};

			var tracker = new ChangeTracker<string>( baseCollection )
				{
					"a",
					"b",
					"c"
				};

			Assert.AreEqual( 6, tracker.Count, "Invalid count." );
			Assert.IsTrue( tracker.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "c" ), "Invalid contents." );

			tracker.Remove( "a" );

			Assert.AreEqual( 5, tracker.Count, "Invalid count." );
			Assert.IsTrue( tracker.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "c" ), "Invalid contents." );

			tracker.Add( "a" );

			Assert.AreEqual( 6, tracker.Count, "Invalid count." );
			Assert.IsTrue( tracker.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "c" ), "Invalid contents." );

			tracker.Clear( );

			Assert.AreEqual( 0, tracker.Count, "Invalid count." );
			Assert.AreEqual( 0, tracker.Added.Count( ), "Invalid Added count." );
			Assert.IsTrue( tracker.Removed.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Removed.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Removed.Contains( "c" ), "Invalid contents." );
		}

		/// <summary>
		///     Tests the ChangeTracker class.
		/// </summary>
		[Test]
		public void ChangeTracker_NoBaseCollection( )
		{
			var tracker = new ChangeTracker<string>
				{
					"a",
					"b",
					"c"
				};

			Assert.AreEqual( 3, tracker.Count, "Invalid count." );
			Assert.IsTrue( tracker.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "c" ), "Invalid contents." );

			tracker.Remove( "a" );

			Assert.AreEqual( 2, tracker.Count, "Invalid count." );
			Assert.IsFalse( tracker.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "c" ), "Invalid contents." );

			tracker.Add( "a" );

			Assert.AreEqual( 3, tracker.Count, "Invalid count." );
			Assert.IsTrue( tracker.Contains( "a" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "b" ), "Invalid contents." );
			Assert.IsTrue( tracker.Contains( "c" ), "Invalid contents." );
		}
	}
}