// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Entity tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class EntityTests
	{
		[Test]
		[RunAsDefaultTenant]
		public void RelationshipSaveProblemDemo2( )
		{
			using ( DatabaseContext.GetContext( true ) )
			{
				var u1 = new UserAccount( );
				var u3 = new UserAccount( );


				// Failing version
				u1.SecurityOwnerOf.Add( u3.As<Resource>( ) );
				u3.SecurityOwnerOf.Add( u1.As<Resource>( ) );

				// Working version
				//u3.SecurityOwner = u1;
				//u1.SecurityOwner = u3;

				//u1.Save( );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestCreateEntity( )
		{
			var c = new Report
				{
					Name = "EDC"
				};
			c.Save( );

			var p = new ReportColumn
				{
					Name = "Col1",
					ColumnForReport = c
				};
			p.Save( );

			Assert.IsNotNull( c );
			Assert.IsNotNull( p );

			p.Delete( );
			c.Delete( );

            p = (ReportColumn)Entity.Get(p.Id, false);
            c = (Report)Entity.Get(c.Id, false);

			Assert.IsNull( p );
			Assert.IsNull( c );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestDeleteField( )
		{
			using ( DatabaseContext.GetContext( true ) )
			{
				var a = new UserAccount
					{
						Name = "Test2"
					};
				a.Save( );

				a.Name = null;
				a.Save( );

				a = Entity.Get<UserAccount>( a.Id );

				Assert.AreEqual( a.Name, null );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEntity_Clone( )
		{
			var p = new Person
				{
					FirstName = "Pete"
				};
			p.Save( );

			var p2 = p.Clone<Person>( );
			Assert.IsNotNull( p2 );
			Assert.AreNotSame( p.Id, p2.Id );
			Assert.AreEqual( p.FirstName, p2.FirstName );

			p.Delete( );
		}

		/// <summary>
		///     Tests the entity_ constructor_ type_ null.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestEntity_Constructor_ActivationData_Empty( )
		{
			ActivationData activationData = ActivationData.Empty;
// ReSharper disable ObjectCreationAsStatement
			new Entity( activationData );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the type of the entity_ constructor_ type_ invalid.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestEntity_Constructor_ActivationData_InvalidType( )
		{
			var activationData = new ActivationData( -1, RequestContext.TenantId );
// ReSharper disable ObjectCreationAsStatement
			new Entity( activationData );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the type of the entity_ constructor_ type_ valid.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestEntity_Constructor_ActivationData_ValidType( )
		{
			ActivationData activationData = ActivationData.Empty;

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "SELECT TOP 1 Id FROM Entity WHERE TenantId = @tenantId" ) )
				{
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						if ( reader.Read( ) )
						{
							activationData = new ActivationData( reader.GetInt64( 0 ), RequestContext.TenantId );
						}
					}
				}
			}


			var entity = new Entity( activationData );

			Assert.IsNotNull( entity );
			Assert.AreEqual( activationData.Id, entity.Id );
		}

		/// <summary>
		///     Tests the entity_ constructor_ type id_ invalid type id.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( InvalidTypeException ) )]
		public void TestEntity_Constructor_TypeId_InvalidTypeId( )
		{
			const long typeId = -1;
// ReSharper disable ObjectCreationAsStatement
			new Entity( typeId );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity_ constructor_ type id_ invalid type id.
		/// </summary>
		[Test]
        [Ignore]
		[RunAsDefaultTenant]
		public void TestEntity_Constructor_TypeId_ValidTypeId( )
		{
			long typeId = EntityIdentificationCache.GetId( new EntityAlias( "core", "type" ) );

			Entity entity = null;

			try
			{
				entity = new Entity( typeId );

				entity.Save( );
			}
			finally
			{
				entity?.Delete( );
			}
		}

		/// <summary>
		///     Tests the type of the entity_ constructor_ type_ invalid.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( InvalidTypeException ) )]
		public void TestEntity_Constructor_Type_InvalidType( )
		{
			Type type = typeof ( string );
// ReSharper disable ObjectCreationAsStatement
			new Entity( type );
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the entity_ constructor_ type_ null.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof ( ArgumentException ) )]
		public void TestEntity_Constructor_Type_Null( )
		{
			Type type = null;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable ExpressionIsAlwaysNull
			new Entity( type );
// ReSharper restore ExpressionIsAlwaysNull
// ReSharper restore ObjectCreationAsStatement
		}

		/// <summary>
		///     Tests the type of the entity_ constructor_ type_ valid.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestEntity_Constructor_Type_ValidType( )
		{
			Type type = typeof ( EntityType );
			var entity = new Entity( type );

			Assert.IsNotNull( entity );
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestFieldLifetime( )
		{
			using ( DatabaseContext.GetContext( true ) )
			{
				var eRef = new EntityRef( "core:name" );

				var writableEntity = Entity.Get<StringField>( eRef, true );
				string currentValue = writableEntity.Description;
				writableEntity.Description = "new value";
				// note: there is *no* save here

				var readableEntity = Entity.Get<StringField>( eRef, false );
				Assert.AreEqual( currentValue, readableEntity.Description );
			}
		}

        /// <summary>
        /// Tests that a large entity graph can save.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestLargeEntityGraphCanSave()
        {            
            var resources = new List<Resource>();            
            for (int i = 0; i < 1000; i++)
            {
                resources.Add(new Resource { Name = Guid.NewGuid().ToString() });
            }

            Assert.DoesNotThrow(() => Entity.Save(resources));    
        }

		/////
		// Tests that entity refs of different types can be processed (id/alias/entity).
		/////
		[Test]
		[RunAsDefaultTenant]
		public void GetFieldTests( )
		{
			var entities = new List<IEntityRef>
			{
				new EntityRef( -1 ),
				new EntityRef( "core:description" ),
				new EntityRef( Entity.Get( new EntityRef( "core:modifiedDate" ) ) )
			};

			IDictionary<IEntityRef, string> fields = Entity.GetField<string>( entities, new EntityRef( "core:name" ) );

			Assert.IsNotNull( fields, "Returned 'fields' dictionary is null" );
			Assert.AreEqual( 3, fields.Count, "Size of the returned dictionary is incorrect" );
		}

        /// <summary>
        /// Tests that a large entity graph can save.
        /// </summary>
        [TestCase( false )]
        [TestCase( true )]
        [RunAsDefaultTenant]
        public void TestRelationshipLoadsType( bool isReverse )
        {
            // Setup schema
            var t1 = new EntityType
            {
	            Name = "Type 1 " + Guid.NewGuid( ).ToString( )
            };
	        t1.Save( );

            var t2 = new EntityType
            {
	            Name = "Type 2 " + Guid.NewGuid( ).ToString( )
            };
	        t2.Save( );

            var r = new Relationship
            {
	            Name = "Rel " + Guid.NewGuid( ).ToString( ),
	            FromType = t1,
	            ToType = t2,
	            Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
            };
	        r.Save( );

            // Setup data
            IEntity inst2 = Entity.Create( t2.Id );
            inst2.Save( );
            IEntity inst1 = Entity.Create( t1.Id );
            inst1.SetRelationships( r.Id, new EntityRelationship<IEntity>( inst2 ).ToEntityRelationshipCollection( ), Direction.Forward );
            inst1.Save( );

            // Ensure cache is empty
            EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( inst1.Id, Direction.Forward ) );
            EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( inst2.Id, Direction.Forward ) );
            EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( inst1.Id, Direction.Reverse ) );
            EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheKey.Create( inst2.Id, Direction.Reverse ) );

            // Follow relationship
            IReadOnlyDictionary<long, ISet<long>> cache;
            long expectType;
            if ( !isReverse )
            {
                IEntity inst1Again = Entity.Get( inst2.Id );
                inst1Again.GetRelationships( r.Id, Direction.Reverse );

                // Verify that target type data has appeared in cache
                cache = EntityRelationshipCache.Instance[ EntityRelationshipCacheKey.Create( inst1.Id, Direction.Forward ) ];
                expectType = t1.Id;
            }
            else
            {
                IEntity inst1Again = Entity.Get( inst1.Id );
                inst1Again.GetRelationships( r.Id, Direction.Forward );

                // Verify that target type data has appeared in cache
                cache = EntityRelationshipCache.Instance[ EntityRelationshipCacheKey.Create( inst2.Id, Direction.Forward ) ];
                expectType = t2.Id;
            }

            Assert.That( cache, Is.Not.Null );
            long isOfType = ( new EntityRef( "core:isOfType" ) ).Id;
            Assert.That( cache.ContainsKey( isOfType ), Is.True, "Cache should have isOfType" );
            Assert.That( cache [ isOfType ].Single( ), Is.EqualTo( expectType ), "Cache isOfType value" );
        }

        [TestCase( 20 )]
        [TestCase( 200 )]
        [RunAsDefaultTenant]
        public void GetByName( int lengthOfName )
        {
            string name = "Test name " + new String( 'z', lengthOfName - 10 );

            var entity = Entity.Create<Resource>( );
            entity.Name = name;
            entity.Save( );

            var result = Entity.GetByName( name ).ToList( );
            Assert.That( result.Count, Is.EqualTo( 1 ) );
            var id = result [ 0 ].Id;

            Assert.That( id, Is.EqualTo( entity.Id ) );
        }

        [TestCase( 20 )]
        [TestCase( 200 )]
        [RunAsDefaultTenant]
        public void GetByField( int lengthOfValue )
        {
            string desc = "Test desc " + new String( 'z', lengthOfValue - 10 );

            var entity = Entity.Create<Resource>( );
            entity.Name = "Test";
            entity.Description = desc;
            entity.Save( );

            var result = Entity.GetByField( desc, new EntityRef("core:description") ).ToList( );
            Assert.That( result.Count, Is.EqualTo( 1 ) );
            var id = result [ 0 ].Id;

            Assert.That( id, Is.EqualTo( entity.Id ) );
        }

		[Test]
		[RunAsDefaultTenant]
		public void GetByNameInsensitiveExact( )
		{
			var entities = Entity.GetByName( "class name", false, true, CaseComparisonOption.Insensitive, StringComparisonOption.Exact );

			Assert.IsNotNull( entities );
			Assert.GreaterOrEqual( 1, entities.Count( ) );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetByNameInsensitivePartial( )
		{
			var entities = Entity.GetByName( "class name", false, true, CaseComparisonOption.Insensitive, StringComparisonOption.Partial );

			Assert.IsNotNull( entities );
			Assert.GreaterOrEqual( 1, entities.Count( ) );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetByNameSensitiveExact( )
		{
			var entities = Entity.GetByName( "class name", false, true, CaseComparisonOption.Sensitive, StringComparisonOption.Exact );

			Assert.IsNotNull( entities );
			Assert.GreaterOrEqual( 1, entities.Count( ) );
		}

		[Test]
		[RunAsDefaultTenant]
		public void GetByNameSensitivePartial( )
		{
			var entities = Entity.GetByName( "class name", false, true, CaseComparisonOption.Sensitive, StringComparisonOption.Partial );

			Assert.IsNotNull( entities );
			Assert.GreaterOrEqual( 1, entities.Count( ) );
		}

		[Test]
		[RunAsDefaultTenant]
		public void EntityExistsInOtherTenant( )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				Entity.Get( 20 );
			}

			bool exists = Entity.Exists( 20 );

			Assert.IsFalse( exists );
		}

		[Test]
		[RunAsDefaultTenant]
		[ExpectedException(typeof( FieldException ) )]
		public void SetInvalidFieldOnEntity( )
		{
			Person p = new Person( );
			p.SetField( new EntityRef( "core:solutionPublisher" ), "DQ" );
		}

		/// <summary>
		/// Ensures the save graph exceptions get raised.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( CardinalityViolationException ) )]
		public void EnsureSaveGraphExceptionsGetRaised( )
		{
			var drinkInstance = Entity.Get( new EntityRef( "test:aaTonicWater" ) );

			var consoleSolution = Entity.Get( new EntityRef( "core:consoleSolution" ) );

			var clone = drinkInstance.Clone( );

			/////
			// InSolution relationship is many-to-one so this will cause a cardinality violation
			/////
			clone.GetRelationships( WellKnownAliases.CurrentTenant.InSolution ).Add( consoleSolution );

			clone.Save( );
		}

		/// <summary>
		/// Ensures the no save graph exceptions get raised.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void EnsureNoSaveGraphExceptionsGetRaised( )
		{
			var drinkInstance = Entity.Get( new EntityRef( "test:aaTonicWater" ) );

			var clone = drinkInstance.Clone( );

			clone.Save( );
		}
	}
}