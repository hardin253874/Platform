// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	public class EntityRelationshipCacheTests
	{
		private ISuppression _suppression;
 
		[TestFixtureSetUp]
		public void TestFixtureSetup( )
		{
			_suppression = Entity.DistributedMemoryManager.Suppress( );
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown( )
		{
			if ( _suppression != null )
			{
				_suppression.Dispose( );
			}
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 1, direction ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRemoveRecordDifferentSource( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 4, direction ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordSameType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 2, direction ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordSameDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 3, direction ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordDifferentDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 1, direction ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordDifferentType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 1, direction ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 1, direction, 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRemoveRecordDifferentSourceWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 4, direction, 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordSameTypeWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 1, direction, 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordSameDestinationWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 3, direction, 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRemoveRecordDifferentDestinationWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 1, direction, 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRemoveRecordDifferentTypeWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 1, direction, 3 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 0 )]
		public void AddEmptyRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 2, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 2, 1, 1 )]
		public void AddMultipleRecordsDifferentSource( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 3, 4 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 2, direction ), CreateCacheValue( 3, 4 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 2, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 2 )]
		public void AddMultipleRecordsDifferentDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 4 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 2, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 2, 1 )]
		public void AddMultipleRecordsDifferentType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 4 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 3, 4 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddDuplicateRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 1, 1, 1 )]
		public void AddRecordAddRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 0, 0, 0 )]
		public void AddRecordAddRecordDifferentSource( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, 4 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 1, 1, 2 )]
		[TestCase( Direction.Reverse, 1, 1, 2, 1, 1, 1 )]
		public void AddIsOfTypeRecordAddInstancesRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, CreateEnumerable( 1, 4 ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 2, 1, 1, 1, 1, 2 )]
		[TestCase( Direction.Reverse, 1, 1, 2, 2, 1, 1 )]
		public void AddMultipleIsOfTypeRecordAddMultipleInstancesRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, CreateEnumerable( 1, 4 ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 2 )]
		[TestCase( Direction.Reverse, 1, 1, 2, 0, 0, 0 )]
		public void AddMultipleIsOfTypeRecordAddDifferentMultipleInstancesRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, CreateEnumerable( 5, 6 ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 1, 1, 2 )]
		[TestCase( Direction.Reverse, 1, 1, 2, 1, 1, 1 )]
		public void AddMultipleIsOfTypeRecordAddDifferentMultipleInstancesRecordOverlap( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, CreateEnumerable( 4, 5 ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 0, 0, 0 )]
		public void AddRecordAddRecordDifferentDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, Flip( direction ) ), CreateCacheValue( 2, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 1, 1, 1 )]
		public void AddRecordAddRecordDifferentType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 4, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 0, 0, 0 )]
		public void AddEmptyRecordAddRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 0, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 1, 1, 0 )]
		public void AddEmptyRecordAddRecordDifferentSource( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, Flip( direction ) ), CreateCacheValue( 2, 3 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 0, 0, 0 )]
		public void AddEmptyRecordAddRecordDifferentDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, Flip( direction ) ), CreateCacheValue( 2, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 0, 1, 1, 1 )]
		[TestCase( Direction.Reverse, 1, 1, 1, 1, 1, 0 )]
		public void AddEmptyRecordAddRecordDifferentType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, Flip( direction ) ), CreateCacheValue( 3, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 2, 1, 1 )]
		[TestCase( Direction.Reverse, 2, 1, 1, 0, 0, 0 )]
		public void AddEmptyRecordAddMultipleRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ), CreateCacheValue( 2, 1 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 4, Flip( direction ) ), CreateCacheValue( 2, 1 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 0, 1, 1, 0 )]
		[TestCase( Direction.Reverse, 1, 1, 0, 1, 1, 0 )]
		public void AddEmptyRecordAddEmptyRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, Flip( direction ) ), CreateCacheValue( 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 0 )]
		[TestCase( Direction.Reverse, 1, 1, 0, 0, 0, 0 )]
		public void AddEmptyRecordAddEmptyRecordDifferentSource( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 2, Flip( direction ) ), CreateCacheValue( 3 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 1, 1, 0 )]
		[TestCase( Direction.Reverse, 1, 1, 0, 0, 0, 0 )]
		public void AddEmptyRecordAddEmptyRecordDifferentDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 2, Flip( direction ) ), CreateCacheValue( 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 0, 1, 1, 0 )]
		[TestCase( Direction.Reverse, 1, 1, 0, 1, 1, 0 )]
		public void AddEmptyRecordAddEmptyRecordDifferentType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, Flip( direction ) ), CreateCacheValue( 3 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRecordRemoveRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRecordRemoveDifferentSource( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 4, Flip( direction ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRecordRemoveDifferentDestination( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRecordRemoveRecordWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 3, Flip( direction ), 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRecordRemoveDifferentSourceWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 4, Flip( direction ), 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRecordRemoveDifferentDestinationWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 3, Flip( direction ), 2 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 1, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 1 )]
		public void AddRecordRemoveDifferentTypeWithType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( 3, Flip( direction ), 4 ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 0, 0, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 0, 0, 0 )]
		public void AddRecordRemoveDifferentType( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2, 3 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		[TestCase( Direction.Forward, 1, 1, 0, 0, 0, 0 )]
		[TestCase( Direction.Reverse, 0, 0, 0, 1, 1, 0 )]
		public void AddEmptyRecordRemoveOppositeRecord( Direction direction, int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			EntityRelationshipCache.Instance.Clear( );

			EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( 1, direction ), CreateCacheValue( 2 ) );
			EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( 3, Flip( direction ) ) );

			Expect( forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		/// <summary>
		///     Creates the cache value.
		/// </summary>
		/// <returns></returns>
		[DebuggerStepThrough]
		private IDictionary<long, ISet<long>> CreateCacheValue( )
		{
			return new Dictionary<long, ISet<long>>( );
		}

		/// <summary>
		///     Creates the cache value.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		private IDictionary<long, ISet<long>> CreateCacheValue( long typeId )
		{
			IDictionary<long, ISet<long>> dic = CreateCacheValue( );
			dic[ typeId ] = new HashSet<long>( );
			return dic;
		}

		/// <summary>
		///     Creates the cache value.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		private IDictionary<long, ISet<long>> CreateCacheValue( long typeId, IEnumerable<long> values )
		{
			IDictionary<long, ISet<long>> dic = CreateCacheValue( typeId );
			dic[ typeId ].UnionWith( values );
			return dic;
		}

		/// <summary>
		///     Creates the cache value.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		private IDictionary<long, ISet<long>> CreateCacheValue( long typeId, long value )
		{
			return CreateCacheValue( typeId, value.ToEnumerable( ) );
		}

		/// <summary>
		///     Flips the specified direction.
		/// </summary>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		private Direction Flip( Direction direction )
		{
			return direction == Direction.Forward ? Direction.Reverse : Direction.Forward;
		}

		/// <summary>
		///     Creates the enumerable.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		private IEnumerable<long> CreateEnumerable( params long[ ] args )
		{
			return args;
		}

		/// <summary>
		///     Expects the specified forward count.
		/// </summary>
		/// <param name="forwardCacheCount">The forward cache count.</param>
		/// <param name="forwardTypeCacheCount">The forward type cache count.</param>
		/// <param name="forwardReverseCacheCount">The forward reverse cache count.</param>
		/// <param name="reverseCacheCount">The reverse cache count.</param>
		/// <param name="reverseTypeCacheCount">The reverse type cache count.</param>
		/// <param name="reverseReverseCacheCount">The reverse reverse cache count.</param>
		private void Expect( int forwardCacheCount, int forwardTypeCacheCount, int forwardReverseCacheCount, int reverseCacheCount, int reverseTypeCacheCount, int reverseReverseCacheCount )
		{
			DirectionalEntityRelationshipCache forwardCache;
			DirectionalEntityRelationshipCache reverseCache;

			EntityRelationshipCache.Instance.Debug( out forwardCache, out reverseCache );

			Action<DirectionalEntityRelationshipCache, int, int, int> test = ( instance, count, typeCount, revCount ) =>
			{
				IDictionary<long, IDictionary<long, ISet<long>>> cache;
				IDictionary<long, ISet<long>> typeCache;
				IDictionary<long, IDictionary<long, ISet<long>>> revCache;

				instance.Debug( out cache, out typeCache, out revCache );

				Assert.AreEqual( count, cache.Count, "Cache count mismatch" );
				Assert.AreEqual( typeCount, typeCache.Count, "TypeCache count mismatch" );
				Assert.AreEqual( revCount, revCache.Count, "ReverseCache count mismatch" );
			};

			test( forwardCache, forwardCacheCount, forwardTypeCacheCount, forwardReverseCacheCount );
			test( reverseCache, reverseCacheCount, reverseTypeCacheCount, reverseReverseCacheCount );
		}

		/// <summary>
		///     Tests the clear method.
		/// </summary>
		[Test]
		public void Clear( )
		{
			EntityRelationshipCache.Instance.Clear( );

			Expect( 0, 0, 0, 0, 0, 0 );
		}
	}
}