// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;

// Namespace declaration 

namespace EDC.ReadiNow.Test.Model.Relationships
{
	[TestFixture]
	[RunWithTransaction]
	[Ignore( "Too slow" )]
	public class CascadeDeleteTests
	{
		/// <summary>
		///     Cleans up this instance.
		/// </summary>
		[TearDown]
		public void Cleanup( )
		{
			using ( new TenantAdministratorContext( "EDC" ) )
			{
				foreach ( IEntity entity in _cleanUp )
				{
					try
					{
						entity.Delete( );
					}
						// ReSharper disable EmptyGeneralCatchClause
					catch
						// ReSharper restore EmptyGeneralCatchClause
					{
					}
				}
			}
		}

		private readonly List<IEntity> _cleanUp = new List<IEntity>( );

		// end OneToManyDelTargetHasInstanceTest

		// end twoWayTest

		/// <summary>
		///     Set-Up Diamond Test framework
		///     This will be tested in a number of scenarios
		///     set up 4 definitions & 5 relationships
		///     then one instance for each definition & relationship
		/// </summary>
		private List<EntityType> SetUpDiamondTest( out List<Relationship> relationshipList, out List<Entity> entityList,
		                                           out List<Entity> relEntityList, out long[] defIdListArray, out long[] relIdListArray, out long[] defInstanceIdArray,
		                                           out long[] relInstanceIdArray, out EntityRelationshipCollection<IEntity> relationshipsCol0,
		                                           out EntityRelationshipCollection<IEntity> relationshipsCol1,
		                                           out EntityRelationshipCollection<IEntity> relationshipsCol3 )
		{
			const int listLength = 4;

			//create definitions
			List<EntityType> definitionList = CreateDefinitionList( listLength );
			defIdListArray = BacklogTestHelper.LoadIdArray( definitionList );

			//create relationships for "Diamond Test Frame 1"
			relationshipList = CreateRelationshipList( definitionList );
			relIdListArray = BacklogTestHelper.LoadIdArray( relationshipList );

			// create instances for each definition
			entityList = CreateDefinitionInstance( definitionList );
			defInstanceIdArray = BacklogTestHelper.LoadIdArray( entityList );

			// create instances for each relationship
			relEntityList = CreateRelationshipInstance( relationshipList );
			relInstanceIdArray = BacklogTestHelper.LoadIdArray( relEntityList );

			//create relationships collection x 3 - one for each entity instance i assume???
			CreateRelationshipCollection( relationshipList, entityList, relEntityList, out relationshipsCol0,
			                              out relationshipsCol1, out relationshipsCol3 );
			return definitionList;
		}

		// end setUpDiamondTest

		/// <summary>
		///     Set-Up 'round the bags' framework
		///     This will be tested in a number of scenarios
		///     set up 1 definition type, 1 relationship type & several instances of each
		/// </summary>
		private void SetUpRoundTheBags( out Relationship relationshipThrow, out EntityType definitionBase, out List<Entity> baseList,
		                                out List<Entity> throwList, out long[] baseInstanceIdArray, out long[] throwInstanceIdArray,
		                                out EntityRelationshipCollection<IEntity> relationshipsCol0,
		                                out EntityRelationshipCollection<IEntity> relationshipsCol1,
		                                out EntityRelationshipCollection<IEntity> relationshipsCol2,
		                                out EntityRelationshipCollection<IEntity> relationshipsCol3, int collectionChoice )
		{
			relationshipsCol0 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol1 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol2 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol3 = new EntityRelationshipCollection<IEntity>( );
			const int baseCount = 4;
			const int throwCount = 12;

			//create definition
			definitionBase = BacklogTestHelper.CreateDefinition( "Diamond Bag" );
			_cleanUp.Add( definitionBase );
			//Console.WriteLine("Base Entity definition is: {1}; id: {0}", definitionBase.Id, definitionBase.Name);
			//Console.WriteLine("Base Entity field is: {1}; id: {0}", definitionBase.Fields.First().Id, definitionBase.Fields.First().Name);

			//create relationships for "Diamond Test Frame 1"
			relationshipThrow = BacklogTestHelper.CreateRelationship( definitionBase, definitionBase, "Base to Base Throw", "manyToMany",
			                                                          false, true ); // cascadeTo relationship
			_cleanUp.Add( relationshipThrow );
			//Console.WriteLine("Relationship Entity definition is: {1}; id: {0}", relationshipThrow.Id, relationshipThrow.Name);

			// create instances for each definition
			baseList = CreateInstanceList( baseCount, definitionBase );
			baseInstanceIdArray = BacklogTestHelper.LoadIdArray( baseList );

			// create instances for each relationship
			throwList = CreateRelationshipList2( throwCount, relationshipThrow );
			throwInstanceIdArray = BacklogTestHelper.LoadIdArray( throwList );

			if ( collectionChoice == 1 )
			{
				//create relationships collection x 4 - one for each entity object; type instance - whatever?
				CreateThrowCollection( relationshipThrow, baseList, throwList, out relationshipsCol0,
				                       out relationshipsCol1, out relationshipsCol2, out relationshipsCol3 );
				Console.WriteLine( @"collection choice one" );

				//check relationships collections - Assert statements//
				CheckRelCollection( relationshipsCol0, relationshipsCol1, relationshipsCol2, relationshipsCol3 );
			}
			else if ( collectionChoice == 2 )
			{
				CreateThrow2Collection( relationshipThrow, baseList, throwList, out relationshipsCol0,
				                        out relationshipsCol1, out relationshipsCol2, out relationshipsCol3 );
				Console.WriteLine( @"collection choice two" );
				CheckRel2Collection( relationshipsCol0, relationshipsCol1, relationshipsCol2, relationshipsCol3 );
			}
			else if ( collectionChoice == 3 )
			{
				CreateThrow3Collection( relationshipThrow, baseList, throwList, out relationshipsCol0,
				                        out relationshipsCol1, out relationshipsCol2, out relationshipsCol3 );
				Console.WriteLine( @"collection choice three" );
				CheckRel3Collection( relationshipsCol0, relationshipsCol1, relationshipsCol2, relationshipsCol3 );
			}
			else
			{
				Console.WriteLine( @"collection choice else" );
			}
		}

		// end setUpRoundTheBags

		/// <summary>
		///     Set of Assert statements for checking the relationship collections have been set-up correctly
		/// </summary>
// ReSharper disable UnusedParameter.Local
		private static void CheckRelCollection( EntityRelationshipCollection<IEntity> relationshipsCol0, EntityRelationshipCollection<IEntity> relationshipsCol1, EntityRelationshipCollection<IEntity> relationshipsCol2, EntityRelationshipCollection<IEntity> relationshipsCol3 )
// ReSharper restore UnusedParameter.Local
		{
			//check relationships collections//
			Assert.IsTrue( relationshipsCol0 != null );
			Assert.IsTrue( relationshipsCol0.Count == 3 );
			Assert.IsTrue( relationshipsCol0.First( ).Entity != null );
			//Console.WriteLine("Home Plate Relationship Collection instance: {0}", relationshipsCol0.First().Instance);
			Assert.IsTrue( relationshipsCol0.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol0.Skip( 2 ).First( ).Entity != null );

			Assert.IsTrue( relationshipsCol1 != null );
			Assert.IsTrue( relationshipsCol1.Count == 3 );
			Assert.IsTrue( relationshipsCol1.First( ).Entity != null );
			//Console.WriteLine("1st bag Relationship Collection instance: {0}", relationshipsCol1.First().Instance);
			Assert.IsTrue( relationshipsCol1.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol1.Skip( 2 ).First( ).Entity != null );

			Assert.IsTrue( relationshipsCol2 != null );
			Assert.IsTrue( relationshipsCol2.Count == 3 );
			Assert.IsTrue( relationshipsCol2.First( ).Entity != null );
			//Console.WriteLine("2nd bag Relationship Collection instance: {0}", relationshipsCol2.First().Instance);
			Assert.IsTrue( relationshipsCol2.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol2.Skip( 2 ).First( ).Entity != null );

			Assert.IsTrue( relationshipsCol3 != null );
			Assert.IsTrue( relationshipsCol3.Count == 3 );
			Assert.IsTrue( relationshipsCol3.First( ).Entity != null );
			//Console.WriteLine("3rd bag Relationship Collection instance: {0}", relationshipsCol3.First().Instance);
			Assert.IsTrue( relationshipsCol3.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol3.Skip( 2 ).First( ).Entity != null );
		}

		//end checkRelCollection

		/// <summary>
		///     Set of Assert statements for checking the relationship collections have been set-up correctly
		/// </summary>
// ReSharper disable UnusedParameter.Local
		private static void CheckRel2Collection( EntityRelationshipCollection<IEntity> relationshipsCol0, EntityRelationshipCollection<IEntity> relationshipsCol1, EntityRelationshipCollection<IEntity> relationshipsCol2, EntityRelationshipCollection<IEntity> relationshipsCol3 )
// ReSharper restore UnusedParameter.Local
		{
			//check relationships collections//
			Assert.IsTrue( relationshipsCol0 != null );
			Assert.IsTrue( relationshipsCol0.Count == 1 );
			Assert.IsTrue( relationshipsCol0.First( ).Entity != null );

			Assert.IsTrue( relationshipsCol1 != null );
			Assert.IsTrue( relationshipsCol1.Count == 2 );
			Assert.IsTrue( relationshipsCol1.First( ).Entity != null );
			//Console.WriteLine("1st bag Relationship Collection instance: {0}", relationshipsCol1.First().Instance);
			Assert.IsTrue( relationshipsCol1.Skip( 1 ).First( ).Entity != null );

			Assert.IsTrue( relationshipsCol3 != null );
			Assert.IsTrue( relationshipsCol3.Count == 1 );
			Assert.IsTrue( relationshipsCol3.First( ).Entity != null );
		}

		//end checkRel2Collection

		/// <summary>
		///     Set of Assert statements for checking the relationship collections have been set-up correctly
		/// </summary>
// ReSharper disable UnusedParameter.Local
		private static void CheckRel3Collection( EntityRelationshipCollection<IEntity> relationshipsCol0, EntityRelationshipCollection<IEntity> relationshipsCol1, EntityRelationshipCollection<IEntity> relationshipsCol2, EntityRelationshipCollection<IEntity> relationshipsCol3 )
// ReSharper restore UnusedParameter.Local
		{
			//check relationships collections//
			Assert.IsTrue( relationshipsCol0 != null );
			Assert.IsTrue( relationshipsCol0.Count == 1 );
			Assert.IsTrue( relationshipsCol0.First( ).Entity != null );

			Assert.IsTrue( relationshipsCol1 != null );
			Assert.IsTrue( relationshipsCol1.Count == 1 );
			Assert.IsTrue( relationshipsCol1.First( ).Entity != null );

			Assert.IsTrue( relationshipsCol3 != null );
			Assert.IsTrue( relationshipsCol3.Count == 1 );
			Assert.IsTrue( relationshipsCol3.First( ).Entity != null );
		}

		//end checkRel3Collection

		/// <summary>
		///     Create a collection of relationships for the Diamond set-up
		/// </summary>
		private static void CreateRelationshipCollection( List<Relationship> relationshipList, List<Entity> entityList, List<Entity> relEntityList,
		                                                  out EntityRelationshipCollection<IEntity> relationshipsCol0,
		                                                  out EntityRelationshipCollection<IEntity> relationshipsCol1,
		                                                  out EntityRelationshipCollection<IEntity> relationshipsCol3 )
		{
			relationshipsCol0 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol1 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol3 = new EntityRelationshipCollection<IEntity>( );
			// add to the collection (instance, entity)
			relationshipsCol0.Add( entityList[ 1 ] );
			relationshipsCol0.Add( entityList[ 3 ] );
			relationshipsCol3.Add( entityList[ 1 ] );
			relationshipsCol1.Add( entityList[ 2 ] );
			relationshipsCol3.Add( entityList[ 2 ] );

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			entityList[ 0 ].SetRelationships( relationshipList[ 0 ], relationshipsCol0 );
			entityList[ 0 ].SetRelationships( relationshipList[ 1 ], relationshipsCol0 );
			entityList[ 3 ].SetRelationships( relationshipList[ 2 ], relationshipsCol3 );
			entityList[ 1 ].SetRelationships( relationshipList[ 3 ], relationshipsCol1 );
			entityList[ 3 ].SetRelationships( relationshipList[ 4 ], relationshipsCol3 );

			//Save source definitions
			entityList[ 0 ].Save( );
			entityList[ 1 ].Save( );
			entityList[ 3 ].Save( );
		}

		//end createRelationshipCollection

		/// <summary>
		///     Create a collection of relationships for the round-the-bags set-up
		/// </summary>
		private static void CreateThrowCollection( Relationship rType, List<Entity> entityList, List<Entity> relEntityList,
		                                           out EntityRelationshipCollection<IEntity> relationshipsCol0,
		                                           out EntityRelationshipCollection<IEntity> relationshipsCol1,
		                                           out EntityRelationshipCollection<IEntity> relationshipsCol2,
		                                           out EntityRelationshipCollection<IEntity> relationshipsCol3 )
		{
			relationshipsCol0 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol1 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol2 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol3 = new EntityRelationshipCollection<IEntity>( );
			// add to the collection (instance, entity) // rel_instance , target entity //
			relationshipsCol0.Add( entityList[ 1 ] );
			relationshipsCol1.Add( entityList[ 0 ] );
			relationshipsCol1.Add( entityList[ 2 ] );
			relationshipsCol2.Add( entityList[ 1 ] );
			relationshipsCol2.Add( entityList[ 3 ] );
			relationshipsCol3.Add( entityList[ 2 ] );
			relationshipsCol3.Add( entityList[ 0 ] );
			relationshipsCol0.Add( entityList[ 3 ] );
			relationshipsCol0.Add( entityList[ 2 ] );
			relationshipsCol2.Add( entityList[ 0 ] );
			relationshipsCol3.Add( entityList[ 1 ] );
			relationshipsCol1.Add( entityList[ 3 ] );

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			entityList[ 0 ].SetRelationships( rType, relationshipsCol0 );
			entityList[ 1 ].SetRelationships( rType, relationshipsCol1 );
			entityList[ 2 ].SetRelationships( rType, relationshipsCol2 );
			entityList[ 3 ].SetRelationships( rType, relationshipsCol3 );

			//Save source definitions
			entityList[ 0 ].Save( );
			entityList[ 1 ].Save( );
			entityList[ 2 ].Save( );
			entityList[ 3 ].Save( );
		}

		//end createThrowCollection

		/// <summary>
		///     Create a collection of relationships for the round-the-bags set-up
		/// </summary>
		private static void CreateThrow2Collection( Relationship rType, List<Entity> entityList, List<Entity> relEntityList,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol0,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol1,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol2,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol3 )
		{
			relationshipsCol0 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol1 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol2 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol3 = new EntityRelationshipCollection<IEntity>( );
			// add to the collection (entity) // rel_instance , target entity //
			relationshipsCol1.Add( entityList[ 0 ] );
			relationshipsCol3.Add( entityList[ 0 ] );
			relationshipsCol0.Add( entityList[ 2 ] );
			relationshipsCol1.Add( entityList[ 3 ] );

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			entityList[ 0 ].SetRelationships( rType, relationshipsCol0 );
			entityList[ 1 ].SetRelationships( rType, relationshipsCol1 );
			entityList[ 3 ].SetRelationships( rType, relationshipsCol3 );

			//Save source definitions
			entityList[ 0 ].Save( );
			entityList[ 1 ].Save( );
			entityList[ 3 ].Save( );
		}

		//end createThrow2Collection

		/// <summary>
		///     Create a collection of relationships for the round-the-bags set-up
		/// </summary>
		private static void CreateThrow3Collection( Relationship rType, List<Entity> entityList, List<Entity> relEntityList,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol0,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol1,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol2,
		                                            out EntityRelationshipCollection<IEntity> relationshipsCol3 )
		{
			relationshipsCol0 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol1 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol2 = new EntityRelationshipCollection<IEntity>( );
			relationshipsCol3 = new EntityRelationshipCollection<IEntity>
				{
					entityList[ 0 ]
				};
			// add to the collection (instance, entity) // rel_instance , target entity //
			relationshipsCol0.Add( entityList[ 2 ] );
			relationshipsCol1.Add( entityList[ 3 ] );

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			entityList[ 0 ].SetRelationships( rType, relationshipsCol0 );
			entityList[ 1 ].SetRelationships( rType, relationshipsCol1 );
			entityList[ 3 ].SetRelationships( rType, relationshipsCol3 );

			//Save source definitions
			entityList[ 0 ].Save( );
			entityList[ 1 ].Save( );
			entityList[ 3 ].Save( );
		}

		//end createThrow3Collection

		/// <summary>
		///     Create a list of definition instances for Round-the-bags set-up
		/// </summary>
		private List<Entity> CreateInstanceList( int instanceCount, EntityType eType )
		{
			var entityList = new List<Entity>( );

			for ( int i = 0; i < instanceCount; i++ )
			{
				string nameTag = "Name is Base " + i;
				var tempEnt = new Entity( eType.Id );
				// set string field to some arbitrary value nameTag
				tempEnt.SetField( eType.Fields.First( ).Id, nameTag );
				tempEnt.Save( );
				entityList.Add( tempEnt );
				_cleanUp.Add( tempEnt );
				//assert
				Assert.IsTrue( Entity.Exists( entityList[ i ].Id ), "Base {0} exists", i );
				//Console.WriteLine("Base Instance {2} Name: {1}; Id: {0}", entityList[i].Id, entityList[i].GetField(eType.Fields.First().Id),i);
			}
			return entityList;
		}

		// end createInstanceList

		/// <summary>
		///     Create a list of definition instances for Diamond Set-Up
		/// </summary>
		private List<Entity> CreateDefinitionInstance( IEnumerable<EntityType> defList )
		{
			int loop = 0;
			var entityList = new List<Entity>( );

			foreach ( EntityType eType in defList )
			{
				string nameTag = "Name is Base " + loop;
				var tempEnt = new Entity( eType.Id );
				// set string field to some arbitrary value nameTag
				tempEnt.SetField( eType.Fields.First( ).Id, nameTag );
				tempEnt.Save( );
				entityList.Add( tempEnt );
				_cleanUp.Add( tempEnt );
				//assert
				Assert.IsTrue( Entity.Exists( entityList[ loop ].Id ), "Base {0} exists", loop );
				loop++;
			}
			return entityList;
		}

		// end createDefinitionInstance

		/// <summary>
		///     Create a list of relationship instances for Diamond Set-Up
		/// </summary>
		private List<Entity> CreateRelationshipInstance( IEnumerable<Relationship> relList )
		{
			var relEntityList = new List<Entity>( );
			int loop = 0;

			foreach ( Relationship rShip in relList )
			{
				string nameTag = "Name is Relationship Instance " + loop;
				var tempRel = new Entity( rShip.Id );
				tempRel.Save( );
				relEntityList.Add( tempRel );
				_cleanUp.Add( tempRel );
				//assert
				Assert.IsTrue( Entity.Exists( relEntityList[ loop ].Id ), "Relationship Instance: {0} exits", nameTag );
				loop++;
			}
			return relEntityList;
		}

		//end createRelationshipInstance

		/// <summary>
		///     Create a list of relationship instances for Round-the-bags set-up
		/// </summary>
		private List<Entity> CreateRelationshipList2( int instanceCount, Relationship rType )
		{
			var relEntityList = new List<Entity>( );

			for ( int i = 0; i < instanceCount; i++ )
			{
				string nameTag = "Name is Throw Instance " + i;
				var tempRel = new Entity( rType.Id );
				tempRel.Save( );
				relEntityList.Add( tempRel );
				_cleanUp.Add( tempRel );
				//assert
				Assert.IsTrue( Entity.Exists( relEntityList[ i ].Id ), "Relationship Instance: {0} exits", nameTag );
				//Console.WriteLine("Throw Instance {1}; Id: {0}", relEntityList[i].Id, i);
			}
			return relEntityList;
		}

		//end createRelationshipList

		/// <summary>
		///     Create a list of relationships
		///     Set up a baseball-diamond like set of relationships for several cascade delete tests //
		///     Home Plate to first; home to third; first to second; third to second & third to first - total 5
		///     All cascadeTo relationships, delete source & target will follow
		/// </summary>
		private List<Relationship> CreateRelationshipList( List<EntityType> definitionList )
		{
			// a list of definition types 
			var relList = new List<Relationship>( );

			// create relationships 
			relList.Insert( 0, BacklogTestHelper.CreateRelationship( definitionList[ 0 ], definitionList[ 1 ],
			                                                         "homeToFirstRelationship", "oneToMany", cascade: false, cascadeTo: true ) );
			relList.Insert( 1, BacklogTestHelper.CreateRelationship( definitionList[ 0 ], definitionList[ 3 ],
			                                                         "homeToThirdRelationship", "oneToMany", cascade: false, cascadeTo: true ) );
			relList.Insert( 2, BacklogTestHelper.CreateRelationship( definitionList[ 3 ], definitionList[ 1 ],
			                                                         "thirdToFirstRelationship", "oneToMany", cascade: false, cascadeTo: true ) );
			relList.Insert( 3, BacklogTestHelper.CreateRelationship( definitionList[ 1 ], definitionList[ 2 ],
			                                                         "firstToSecondRelationship", "oneToMany", cascade: false, cascadeTo: true ) );
			relList.Insert( 4, BacklogTestHelper.CreateRelationship( definitionList[ 3 ], definitionList[ 2 ],
			                                                         "thirdToSecondRelationship", "oneToMany", cascade: false, cascadeTo: true ) );

			//relList.Add(BacklogTestHelper.CreateRelationship(definitionList[2], definitionList[0],
			//"secondToHomeRelationship", "oneToMany", cascade: false, cascadeTo: true));

			foreach ( Relationship relation in relList )
			{
				//Console.WriteLine("Relationship: {1}; Id: {0}", relation.Id,relation.Name);
				_cleanUp.Add( relation );
				//assert
				Assert.IsTrue( Entity.Exists( relation.Id ), "Relationship: {0} exists", relation.Name );
			}

			return relList;
		}

		// end CreateRelationshipList

		/// <summary>
		///     Create a list of definitions
		///     To be later related in a complex way for cascade testing
		///     Baseball diamond: Home Plate (0); first base; second base & third base - total 4
		///     all with one string field - why?, because I can
		/// </summary>
		private List<EntityType> CreateDefinitionList( int uBound )
		{
			// a list of definition types 
			var defList = new List<EntityType>( );

			if ( uBound > 0 )
			{
				int x = 0;
				while ( x < uBound )
				{
					var defElement = new EntityType
						{
							Name = "definition" + x
						};
					//add a string field to the definition
					var strField = new StringField
						{
							Name = "String field " + x
						};
					strField.Save( );
					//// cast to type to "Add" field & save definition
					defElement.Fields.Add( strField.As<Field>( ) );
					defElement.Save( );
					//add to cleanup
					_cleanUp.Add( defElement );
					_cleanUp.Add( strField );
					//add to list
					defList.Insert( x, defElement );
					//assert
					Assert.IsTrue( Entity.Exists( defElement.Id ), "Definition: {0} exists", defElement.Name );
					Assert.IsTrue( Entity.Exists( defElement.Fields.First( ).Id ), "Definition String field: {0} exists", defElement.Fields.First( ).Name );
					//comment
					//Console.WriteLine("Assert definition: {0}; sField: {2}; iteration: {1}", defElement.Name, x, //defElement.Fields.First().Name
					//       defElement.Fields.ElementAt(0).Name ); // both methods work //

					x++;
				}
			}
			return defList;
		}

		/// <summary>
		///     Test for Diamond Framework One
		///     Test for delete Home Plate definition & see what happens
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteHomePlateDefnTest( )
		{
			List<Relationship> relationshipList;
			List<Entity> entityList;
			List<Entity> relEntityList;
			EntityRelationshipCollection<IEntity> relationshipsCol0;
			EntityRelationshipCollection<IEntity> relationshipsCol1;
			EntityRelationshipCollection<IEntity> relationshipsCol3;
			long[] defIdListArray;
			long[] relIdListArray;
			long[] defInstanceIdArray;
			long[] relInstanceIdArray;

			//Set-Up
			List<EntityType> definitionList = SetUpDiamondTest( out relationshipList, out entityList, out relEntityList, out defIdListArray,
			                                                    out relIdListArray, out defInstanceIdArray, out relInstanceIdArray, out relationshipsCol0, out relationshipsCol1,
			                                                    out relationshipsCol3 );

			//check relationships collections//
			Assert.IsTrue( relationshipsCol0 != null );
			Assert.IsTrue( relationshipsCol0.Count == 2 );
			Assert.IsTrue( relationshipsCol0.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol0.Skip( 1 ).First( ).Entity != null );

			Assert.IsTrue( relationshipsCol1 != null );
			Assert.IsTrue( relationshipsCol1.Count == 1 );
			Assert.IsTrue( relationshipsCol1.First( ).Entity != null );

			Assert.IsTrue( relationshipsCol3 != null );
			Assert.IsTrue( relationshipsCol3.Count == 2 );
			Assert.IsTrue( relationshipsCol3.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol3.Skip( 1 ).First( ).Entity != null );

			//Action
			//home plate definition is deleted
			IEntity sourceDefinition0 = Entity.Get( definitionList[ 0 ].Id, true ); //writeable //
			sourceDefinition0.Delete( );

			//Assert - home plate definition & instance are gone
			Assert.IsFalse( Entity.Exists( defIdListArray[ 0 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( defInstanceIdArray[ 0 ] ), "Deleted Entities" );
			// check cascade
			// object definitions - all other bases should remain
			Assert.IsTrue( Entity.Exists( defIdListArray[ 1 ] ), "Deleted Entities" );
			Assert.IsTrue( Entity.Exists( defIdListArray[ 2 ] ), "Deleted Entities" );
			Assert.IsTrue( Entity.Exists( defIdListArray[ 3 ] ), "Deleted Entities" );
			//object instances - all should go due to two level cascade 
			Assert.IsFalse( Entity.Exists( defInstanceIdArray[ 1 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( defInstanceIdArray[ 2 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( defInstanceIdArray[ 3 ] ), "Deleted Entities" );
			// relationship definitions - the two with one end at home go; the rest remain
			Assert.IsFalse( Entity.Exists( relIdListArray[ 0 ] ), "Deleted Entities" ); //home to first
			Assert.IsFalse( Entity.Exists( relIdListArray[ 1 ] ), "Deleted Entities" ); //home to third
			Assert.IsTrue( Entity.Exists( relIdListArray[ 2 ] ), "Deleted Entities" ); //third to first
			Assert.IsTrue( Entity.Exists( relIdListArray[ 3 ] ), "Deleted Entities" ); //first to second
			Assert.IsTrue( Entity.Exists( relIdListArray[ 4 ] ), "Deleted Entities" ); //third to second
			//relationship instances - all go (as above for object instances)
			Assert.IsFalse( Entity.Exists( relInstanceIdArray[ 0 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( relInstanceIdArray[ 1 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( relInstanceIdArray[ 2 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( relInstanceIdArray[ 3 ] ), "Deleted Entities" );
			Assert.IsFalse( Entity.Exists( relInstanceIdArray[ 4 ] ), "Deleted Entities" );
		}

		/// <summary>
		///     Test for delete source definition & see what happens to the target objects with no relationship instance & 'cascade to' set
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteSourceDefNoInstanceTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			long sourceDefnId = sourceDefn.Id;

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			long targetDefnId = targetDefn.Id;

			/////
			// Create a relationship between the two new types
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: false, cascadeTo: true );
			_cleanUp.Add( sourceToTargetRelationship );
			long sourceToTargetRelationshipId = sourceToTargetRelationship.Id;

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// assert 
			Assert.IsNotNull( sourceInstance1 );

			var targetInstance1 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// save source instances & add to cleanup
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );
			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;

			// get writable copy
			Entity.Get( sourceInstance1Id, true );

			////
			// get down to test - delete source resource definition -
			////
			sourceDefn.Delete( );

			Assert.IsFalse( Entity.Exists( sourceDefnId ), "Deleted entities" );
			// Check the cascade delete
			// the target definition should remain, relationship definition and source instance should go, target instances should remain
			Assert.IsTrue( Entity.Exists( targetDefnId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( sourceToTargetRelationshipId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( targetInstance2Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Definition Tests
		///     Test for delete source definition & see what happens to the target objects with 'cascade to' set
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteSourceDefinitionTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			long sourceDefnId = sourceDefn.Id;

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			long targetDefnId = targetDefn.Id;

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: false, cascadeTo: true );
			_cleanUp.Add( sourceToTargetRelationship );
			long sourceToTargetRelationshipId = sourceToTargetRelationship.Id;

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// assert 
			Assert.IsNotNull( sourceInstance1 );

			var targetInstance1 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance );

			var relationshipInstance2 = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance2 );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1,
					targetInstance2
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instances & add to cleanup
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );
			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );
			relationshipInstance.Save( );
			_cleanUp.Add( relationshipInstance );
			relationshipInstance2.Save( );
			_cleanUp.Add( relationshipInstance2 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;
			long relationshipInstanceId = relationshipInstance.Id;
			long relationshipInstance2Id = relationshipInstance2.Id;

			// update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 2 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 1 ).First( ).Entity != null );

			////
			// get down to test - delete source resource definition -
			////
			sourceDefn.Delete( );

			Assert.IsFalse( Entity.Exists( sourceDefnId ), "Deleted entities" );
			// Check the cascade delete
			// the target definition should remain, but all relationships & instances should go
			Assert.IsTrue( Entity.Exists( targetDefnId ), "Deleted entities" );

			Assert.IsFalse( Entity.Exists( sourceToTargetRelationshipId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstanceId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstance2Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Instance Tests
		///     Test for delete source instance & see what happens to target instance
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteSourceInstanceTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "my string field"
				}; // new object of whatever type
			stField.Save( );
			var decField = new DecimalField
				{
					Name = "my decimal field"
				};
			decField.Save( );
			var boolField = new BoolField
				{
					Name = "my boolean field"
				};
			boolField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Fields.Add( decField.As<Field>( ) );
			sourceDefn.Fields.Add( boolField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			var strField2 = new StringField
				{
					Name = "target string field Two"
				}; // new object of whatever type
			strField2.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Fields.Add( strField2.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToOne", cascade: false, cascadeTo: true );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			const decimal decimal1 = 99.8572m;
			sourceInstance1.SetField( decField, decimal1 );
			//sourceInstance1.SetField(decField, 99.8572m);  // use 'm' construct as instructed
			sourceInstance1.SetField( boolField, true );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.SetField( strField2, "Description is left behind after test" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 1 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );

			////
			// get down to test - delete target resource instance -
			////
			//IEntity targetInstance1B = Entity.Get(targetInstance1Id, true); //obtain writable entity, hence the bool flag; default is read-only
			//targetInstance1B.Delete();

			sourceInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );

			// Check the cascade delete 
			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
		}

		/// <summary>
		///     Test for delete source definition & see what happens to the target objects with no relationship instance & 'cascade' set
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteTargetDefNoInstanceTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			long sourceDefnId = sourceDefn.Id;

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			long targetDefnId = targetDefn.Id;

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );
			long sourceToTargetRelationshipId = sourceToTargetRelationship.Id;

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// assert 
			Assert.IsNotNull( sourceInstance1 );

			var targetInstance1 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// save source instances & add to cleanup
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );
			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;

			// get writable copy
			//IEntity sourceInstance1B = Entity.Get(sourceInstance1Id, true);

			////
			// get down to test - delete source resource definition -
			////
			targetDefn.Delete( );

			Assert.IsFalse( Entity.Exists( targetDefnId ), "Deleted entities" );
			// Check the cascade delete
			// the source definition & instance should remain, relationship definition and target instances should go
			Assert.IsTrue( Entity.Exists( sourceDefnId ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( sourceToTargetRelationshipId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
		}

		/// <summary>
		///     Target Definition tests
		///     Test for delete target definition & see what happens to the source objects with 'cascade' set
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteTargetDefinitionTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			long sourceDefnId = sourceDefn.Id;

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			long targetDefnId = targetDefn.Id;

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );
			long sourceToTargetRelationshipId = sourceToTargetRelationship.Id;

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// assert 
			Assert.IsNotNull( sourceInstance1 );

			var targetInstance1 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance );

			// instantiate the relationship instance 
			var relationshipInstance2 = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance2 );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1,
					targetInstance2
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instances & add to cleanup
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );
			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );
			relationshipInstance.Save( );
			_cleanUp.Add( relationshipInstance );
			relationshipInstance2.Save( );
			_cleanUp.Add( relationshipInstance2 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;
			long relationshipInstanceId = relationshipInstance.Id;
			long relationshipInstance2Id = relationshipInstance2.Id;

			// update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 2 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 1 ).First( ).Entity != null );

			////
			// get down to test - delete target resource definition -
			////
			targetDefn.Delete( );

			Assert.IsFalse( Entity.Exists( targetDefnId ), "Deleted entities" );
			// Check the cascade delete
			// the source definition should remain, but all relationships & instances should go
			Assert.IsTrue( Entity.Exists( sourceDefnId ), "Deleted entities" );

			Assert.IsFalse( Entity.Exists( sourceToTargetRelationshipId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstanceId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstance2Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Instance Tests
		///     Test for
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteTargetInstanceNoCascadeTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "my string field"
				}; // new object of whatever type
			stField.Save( );
			var decField = new DecimalField
				{
					Name = "my decimal field"
				};
			decField.Save( );
			var boolField = new BoolField
				{
					Name = "my boolean field"
				};
			boolField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Fields.Add( decField.As<Field>( ) );
			sourceDefn.Fields.Add( boolField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			var strField2 = new StringField
				{
					Name = "target string field Two"
				}; // new object of whatever type
			strField2.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Fields.Add( strField2.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToOne", cascade: false, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			const decimal decimal1 = 99.8572m;
			sourceInstance1.SetField( decField, decimal1 );
			//sourceInstance1.SetField(decField, 99.8572m);  // use 'm' construct as instructed
			sourceInstance1.SetField( boolField, true );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );
			Assert.AreEqual( "Name is Source Instance One", sourceInstance1.GetField( stField ), "source String field" );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.SetField( strField2, "Description is left behind after test" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity>  relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long relationshipInstanceId = relationshipInstance.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 1 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );

			////
			// get down to test - delete target resource instance -
			////
			IEntity targetInstance1B = Entity.Get( targetInstance1Id, true ); //obtain writable entity, hence the bool flag; default is read-only
			targetInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );

			// Check the cascade delete                 
			Assert.IsTrue( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstanceId ), "Deleted entities" );
		}

		// end DeleteTargetInstanceNoCascadeTest

		/// <summary>
		///     Source Instance Tests
		///     Test for
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteTargetInstanceNoRelInstanceTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "source string field One"
				}; // new object of whatever type
			stField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToOne", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );
			Assert.AreEqual( "Name is Source Instance One", sourceInstance1.GetField( stField ), "source String field" );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity>  relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 1 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );

			////
			// get down to test - delete target resource instance -
			////
			IEntity targetInstance1B = Entity.Get( targetInstance1Id, true ); //obtain writable entity, hence the bool flag; default is read-only
			targetInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );

			// Check the cascade delete                 
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Instance Tests
		///     Test for
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DeleteTargetInstanceTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "my string field"
				}; // new object of whatever type
			stField.Save( );
			var decField = new DecimalField
				{
					Name = "my decimal field"
				};
			decField.Save( );
			var boolField = new BoolField
				{
					Name = "my boolean field"
				};
			boolField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Fields.Add( decField.As<Field>( ) );
			sourceDefn.Fields.Add( boolField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			var strField2 = new StringField
				{
					Name = "target string field Two"
				}; // new object of whatever type
			strField2.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Fields.Add( strField2.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToOne", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			const decimal decimal1 = 99.8572m;
			sourceInstance1.SetField( decField, decimal1 );
			//sourceInstance1.SetField(decField, 99.8572m);  // use 'm' construct as instructed
			sourceInstance1.SetField( boolField, true );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );
			Assert.AreEqual( "Name is Source Instance One", sourceInstance1.GetField( stField ), "source String field" );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.SetField( strField2, "Description is left behind after test" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 1 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );

			////
			// get down to test - delete target resource instance -
			////
			IEntity targetInstance1B = Entity.Get( targetInstance1Id, true ); //obtain writable entity, hence the bool flag; default is read-only
			targetInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );

			// Check the cascade delete                 
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Instance Tests
		///     Test for
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void OneToManyDelTargetHasInstanceTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "source string field One"
				}; // new object of whatever type
			stField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );
			Assert.AreEqual( "Name is Source Instance One", sourceInstance1.GetField( stField ), "source String field" );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			targetInstance2.SetField( strField1, "Name is Target Instance Two" );
			targetInstance2.Save( );
			// assert 
			Assert.IsNotNull( targetInstance2 );

			var targetInstance3 = new Entity( targetDefn.Id );
			targetInstance3.SetField( strField1, "Name is Target Instance Three" );
			targetInstance3.Save( );
			// assert 
			Assert.IsNotNull( targetInstance3 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			Assert.IsNotNull( relationshipInstance );
			var relationshipInstance2 = new Entity( sourceToTargetRelationship.Id );
			Assert.IsNotNull( relationshipInstance2 );
			var relationshipInstance3 = new Entity( sourceToTargetRelationship.Id );
			Assert.IsNotNull( relationshipInstance3 );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
			{
				targetInstance1,
				targetInstance2,
				targetInstance3
			};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );
			targetInstance3.Save( );
			_cleanUp.Add( targetInstance3 );
			relationshipInstance.Save( );
			_cleanUp.Add( relationshipInstance );
			relationshipInstance2.Save( );
			_cleanUp.Add( relationshipInstance2 );
			relationshipInstance3.Save( );
			_cleanUp.Add( relationshipInstance3 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;
			long targetInstance3Id = targetInstance3.Id;
			long relationshipInstanceId = relationshipInstance.Id;
			long relationshipInstance2Id = relationshipInstance2.Id;
			long relationshipInstance3Id = relationshipInstance3.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 3 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 2 ).First( ).Entity != null );

			////
			// get down to test - delete target instance 1 -
			////
			IEntity targetInstance1B = Entity.Get( targetInstance1Id, true );
			targetInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			// Check the cascade delete 
			Assert.IsTrue( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstanceId ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( relationshipInstance2Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( relationshipInstance3Id ), "Deleted entities" );

			// next step
			IEntity targetInstance2B = Entity.Get( targetInstance2Id, true );
			targetInstance2B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			// Check the cascade delete 
			Assert.IsTrue( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstanceId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstance2Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( relationshipInstance3Id ), "Deleted entities" );

			//next step
			IEntity targetInstance3B = Entity.Get( targetInstance3Id, true );
			targetInstance3B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			// Check the cascade delete                
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstanceId ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstance2Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( relationshipInstance3Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Instance Tests
		///     Test for
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void OneToManyDelTargetTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "source string field One"
				}; // new object of whatever type
			stField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );
			Assert.AreEqual( "Name is Source Instance One", sourceInstance1.GetField( stField ), "source String field" );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			targetInstance2.SetField( strField1, "Name is Target Instance Two" );
			targetInstance2.Save( );
			// assert 
			Assert.IsNotNull( targetInstance2 );

			var targetInstance3 = new Entity( targetDefn.Id );
			targetInstance3.SetField( strField1, "Name is Target Instance Three" );
			targetInstance3.Save( );
			// assert 
			Assert.IsNotNull( targetInstance3 );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1,
					targetInstance2,
					targetInstance3
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );
			targetInstance3.Save( );
			_cleanUp.Add( targetInstance3 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;
			long targetInstance3Id = targetInstance3.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 3 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 2 ).First( ).Entity != null );

			////
			// get down to test - delete target instance 1 -
			////
			IEntity targetInstance1B = Entity.Get( targetInstance1Id, true );
			targetInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			// Check the cascade delete    
			Assert.IsTrue( Entity.Exists( sourceInstance1Id ), "Deleted entities" );

			// next step
			IEntity targetInstance2B = Entity.Get( targetInstance2Id, true );
			targetInstance2B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsTrue( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			// Check the cascade delete                 
			Assert.IsTrue( Entity.Exists( sourceInstance1Id ), "Deleted entities" );

			// next step
			IEntity targetInstance3B = Entity.Get( targetInstance3Id, true );
			targetInstance3B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			// Check the cascade delete                 
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
		}

		/// <summary>
		///     Source Instance Tests
		///     Test for
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void OneToManyTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );
			// add some columns to the definition
			var stField = new StringField
				{
					Name = "source string field One"
				}; // new object of whatever type
			stField.Save( );
			// cast to type to "Add" field & save definition
			sourceDefn.Fields.Add( stField.As<Field>( ) );
			sourceDefn.Save( );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );
			// add some columns to the definition
			var strField1 = new StringField
				{
					Name = "target string field One"
				}; // new object of whatever type
			strField1.Save( );
			// cast to type to "Add" field & save definition
			targetDefn.Fields.Add( strField1.As<Field>( ) );
			targetDefn.Save( );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToMany", cascade: false, cascadeTo: true );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// include some data in this instance
			sourceInstance1.SetField( stField, "Name is Source Instance One" );
			sourceInstance1.Save( );
			// assert 
			Assert.IsNotNull( sourceInstance1 );
			Assert.AreEqual( "Name is Source Instance One", sourceInstance1.GetField( stField ), "source String field" );

			var targetInstance1 = new Entity( targetDefn.Id );
			targetInstance1.SetField( strField1, "Name is Target Instance One" );
			targetInstance1.Save( );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			var targetInstance2 = new Entity( targetDefn.Id );
			targetInstance2.SetField( strField1, "Name is Target Instance Two" );
			targetInstance2.Save( );
			// assert 
			Assert.IsNotNull( targetInstance2 );

			var targetInstance3 = new Entity( targetDefn.Id );
			targetInstance3.SetField( strField1, "Name is Target Instance Three" );
			targetInstance3.Save( );
			// assert 
			Assert.IsNotNull( targetInstance3 );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1,
					targetInstance2,
					targetInstance3
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );
			targetInstance2.Save( );
			_cleanUp.Add( targetInstance2 );
			targetInstance3.Save( );
			_cleanUp.Add( targetInstance3 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;
			long targetInstance2Id = targetInstance2.Id;
			long targetInstance3Id = targetInstance3.Id;

			// get writable copy & update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id, true );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 3 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 1 ).First( ).Entity != null );
			Assert.IsTrue( relationshipsCol.Skip( 2 ).First( ).Entity != null );

			////
			// get down to test - delete source resource instance -
			////
			sourceInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance2Id ), "Deleted entities" );
			Assert.IsFalse( Entity.Exists( targetInstance3Id ), "Deleted entities" );
			// Check the cascade delete                 
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
		}

		/// <summary>
		///     Test for round-the-bags framework
		///     Test for delete First base instance with one way relationship set-up
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void OneWayTest( )
		{
			EntityType definitionBase;
			Relationship relationshipThrow;
			List<Entity> baseList;
			List<Entity> throwList;
			EntityRelationshipCollection<IEntity> relationshipsCol0;
			EntityRelationshipCollection<IEntity> relationshipsCol1;
			EntityRelationshipCollection<IEntity> relationshipsCol3;
			const int collectionChoice = 2; //one way relationship set-up
			long[] baseInstanceIdArray;
			long[] throwInstanceIdArray;

			//Set-Up
			EntityRelationshipCollection<IEntity> relationshipsCol2;
			SetUpRoundTheBags( out relationshipThrow, out definitionBase, out baseList, out throwList, out baseInstanceIdArray,
			                   out throwInstanceIdArray, out relationshipsCol0, out relationshipsCol1, out relationshipsCol2, out relationshipsCol3,
			                   collectionChoice );

			//Action
			//delete first base
			IEntity firstBase = Entity.Get( baseInstanceIdArray[ 1 ], true ); //writeable //
			firstBase.Delete( );

			//Assert - third base instance is gone
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 0 ] ), "Home plate" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 1 ] ), "1st base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 2 ] ), "2nd base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 3 ] ), "3rd base" );
			// check cascade
			// object definition
			Assert.IsTrue( Entity.Exists( definitionBase ), "Base Entity" );
			// relationship definition
			Assert.IsTrue( Entity.Exists( relationshipThrow ), "Relationship 'Throw'" );
			//relationship instances - all go (as above for object instances)
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 1 ] ), "1st to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 6 ] ), "3rd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 8 ] ), "home to 2nd" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 11 ] ), "1st to 3rd" );
		}

		/// <summary>
		///     Simple test for proof of concept
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void SimpleCascadeDeleteTest( )
		{
			/////
			// Create a type.
			/////
			EntityType sourceDefn = BacklogTestHelper.CreateDefinition( "testSourceDefinition" );
			_cleanUp.Add( sourceDefn );

			/////
			// Create another type.
			/////
			EntityType targetDefn = BacklogTestHelper.CreateDefinition( "testTargetDefinition" );
			_cleanUp.Add( targetDefn );

			/////
			// Create a relationship between the two new types.
			/////
			Relationship sourceToTargetRelationship = BacklogTestHelper.CreateRelationship( sourceDefn, targetDefn, "testSourceToTargetRelationship",
			                                                                                "oneToOne", cascade: true, cascadeTo: false );
			_cleanUp.Add( sourceToTargetRelationship );

			/////
			// instantiate two new instances of targetDefn & sourceDefn 
			/////
			var sourceInstance1 = new Entity( sourceDefn.Id );
			// assert 
			Assert.IsNotNull( sourceInstance1 );

			var targetInstance1 = new Entity( targetDefn.Id );
			// assert 
			Assert.IsNotNull( targetInstance1 );

			// instantiate the relationship instance 
			var relationshipInstance = new Entity( sourceToTargetRelationship.Id );
			// assert 
			Assert.IsNotNull( relationshipInstance );

			/////
			// Create relationships collection       
			/////
			IEntityRelationshipCollection<IEntity> relationshipsCol = new EntityRelationshipCollection<IEntity>
				{
					targetInstance1
				};

			// SetRelationships method of entity instance - associate relationship definition with instance collection
			sourceInstance1.SetRelationships( sourceToTargetRelationship, relationshipsCol );

			// save source instance
			sourceInstance1.Save( );
			_cleanUp.Add( sourceInstance1 );

			targetInstance1.Save( );
			_cleanUp.Add( targetInstance1 );

			// store the instance id's in a local
			long sourceInstance1Id = sourceInstance1.Id;
			long targetInstance1Id = targetInstance1.Id;

			// update collection
			IEntity sourceInstance1B = Entity.Get( sourceInstance1Id );
			relationshipsCol = sourceInstance1B.GetRelationships( sourceToTargetRelationship );
			_cleanUp.Add( sourceInstance1B );

			// assert
			Assert.IsTrue( relationshipsCol != null );
			Assert.IsTrue( relationshipsCol.Count == 1 );
			Assert.IsTrue( relationshipsCol.First( ).Entity != null );

			////
			// get down to test - delete target resource instance -
			////
			IEntity targetInstance1B = Entity.Get( targetInstance1Id, true ); //obtain writable entity, hence the bool flag; default is read-only
			targetInstance1B.Delete( );

			Assert.IsFalse( Entity.Exists( targetInstance1Id ), "Deleted entities" );

			// Check the cascade delete
			Assert.IsFalse( Entity.Exists( sourceInstance1Id ), "Deleted entities" );
		}

		/// <summary>
		///     Test for round-the-bags framework
		///     Test for delete 3rd base instance & see what happens
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TwoLevelCascadeTest( )
		{
			EntityType definitionBase;
			Relationship relationshipThrow;
			List<Entity> baseList;
			List<Entity> throwList;
			EntityRelationshipCollection<IEntity> relationshipsCol0;
			EntityRelationshipCollection<IEntity> relationshipsCol1;
			EntityRelationshipCollection<IEntity> relationshipsCol2;
			EntityRelationshipCollection<IEntity> relationshipsCol3;
			const int collectionChoice = 1;
			long[] baseInstanceIdArray;
			long[] throwInstanceIdArray;

			//Set-Up
			SetUpRoundTheBags( out relationshipThrow, out definitionBase, out baseList, out throwList, out baseInstanceIdArray,
			                   out throwInstanceIdArray, out relationshipsCol0, out relationshipsCol1, out relationshipsCol2, out relationshipsCol3,
			                   collectionChoice );

			//Action
			//delete third base
			IEntity thirdBase = Entity.Get( baseInstanceIdArray[ 3 ], true ); //writeable //
			thirdBase.Delete( );

			//Assert - third base instance is gone
			Assert.IsTrue( Entity.Exists( baseInstanceIdArray[ 0 ] ), "Home plate" );
			Assert.IsTrue( Entity.Exists( baseInstanceIdArray[ 1 ] ), "1st base" );
			Assert.IsTrue( Entity.Exists( baseInstanceIdArray[ 2 ] ), "2nd base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 3 ] ), "3rd base" );
			// check cascade
			// object definition
			Assert.IsTrue( Entity.Exists( definitionBase ), "Base Entity" );
			// relationship definition
			Assert.IsTrue( Entity.Exists( relationshipThrow ), "Relationship 'Throw'" ); //home to first
			//relationship instances - all go (as above for object instances)
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 0 ] ), "home to first" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 1 ] ), "1st to home" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 2 ] ), "first to second" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 3 ] ), "second to first" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 4 ] ), "second to third" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 5 ] ), "third to second" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 6 ] ), "3rd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 7 ] ), "home to 3rd" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 8 ] ), "home to 2nd" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 9 ] ), "2nd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 10 ] ), "3rd to 1st" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 11 ] ), "1st to 3rd" );
		}

		/// <summary>
		///     Test for round-the-bags framework
		///     Test for delete 3rd base instance & First base instance & see what happens
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TwoLevelThreeBagDeleteTest( )
		{
			EntityType definitionBase;
			Relationship relationshipThrow;
			List<Entity> baseList;
			List<Entity> throwList;
			EntityRelationshipCollection<IEntity> relationshipsCol0;
			EntityRelationshipCollection<IEntity> relationshipsCol1;
			EntityRelationshipCollection<IEntity> relationshipsCol2;
			EntityRelationshipCollection<IEntity> relationshipsCol3;
			const int collectionChoice = 1;
			long[] baseInstanceIdArray;
			long[] throwInstanceIdArray;

			//Set-Up
			SetUpRoundTheBags( out relationshipThrow, out definitionBase, out baseList, out throwList, out baseInstanceIdArray,
			                   out throwInstanceIdArray, out relationshipsCol0, out relationshipsCol1, out relationshipsCol2, out relationshipsCol3,
			                   collectionChoice );

			//Action
			//delete third base
			IEntity thirdBase = Entity.Get( baseInstanceIdArray[ 3 ], true ); //writeable //
			thirdBase.Delete( );
			IEntity firstBase = Entity.Get( baseInstanceIdArray[ 1 ], true ); //writeable //
			firstBase.Delete( );
			IEntity secondBase = Entity.Get( baseInstanceIdArray[ 2 ], true ); //writeable //
			secondBase.Delete( );

			//Assert - third base instance is gone
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 0 ] ), "Home plate" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 1 ] ), "1st base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 2 ] ), "2nd base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 3 ] ), "3rd base" );
			// check cascade
			// object definition
			Assert.IsTrue( Entity.Exists( definitionBase ), "Base Entity" );
			// relationship definition
			Assert.IsTrue( Entity.Exists( relationshipThrow ), "Relationship 'Throw'" ); //home to first
			//relationship instances - all go (as above for object instances)
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 0 ] ), "home to first" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 1 ] ), "1st to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 2 ] ), "first to second" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 3 ] ), "second to first" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 4 ] ), "second to third" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 5 ] ), "third to second" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 6 ] ), "3rd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 7 ] ), "home to 3rd" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 8 ] ), "home to 2nd" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 9 ] ), "2nd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 10 ] ), "3rd to 1st" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 11 ] ), "1st to 3rd" );
		}

		/// <summary>
		///     Test for round-the-bags framework
		///     Test for delete 3rd base instance & First base instance & see what happens
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TwoLevelTwoBagDeleteTest( )
		{
			EntityType definitionBase;
			Relationship relationshipThrow;
			List<Entity> baseList;
			List<Entity> throwList;
			EntityRelationshipCollection<IEntity> relationshipsCol0;
			EntityRelationshipCollection<IEntity> relationshipsCol1;
			EntityRelationshipCollection<IEntity> relationshipsCol2;
			EntityRelationshipCollection<IEntity> relationshipsCol3;
			const int collectionChoice = 1;
			long[] baseInstanceIdArray;
			long[] throwInstanceIdArray;

			//Set-Up
			SetUpRoundTheBags( out relationshipThrow, out definitionBase, out baseList, out throwList, out baseInstanceIdArray,
			                   out throwInstanceIdArray, out relationshipsCol0, out relationshipsCol1, out relationshipsCol2, out relationshipsCol3,
			                   collectionChoice );

			//Action
			//delete third base
			IEntity thirdBase = Entity.Get( baseInstanceIdArray[ 3 ], true ); //writeable //
			thirdBase.Delete( );
			IEntity firstBase = Entity.Get( baseInstanceIdArray[ 1 ], true ); //writeable //
			firstBase.Delete( );

			//Assert - third base instance is gone
			Assert.IsTrue( Entity.Exists( baseInstanceIdArray[ 0 ] ), "Home plate" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 1 ] ), "1st base" );
			Assert.IsTrue( Entity.Exists( baseInstanceIdArray[ 2 ] ), "2nd base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 3 ] ), "3rd base" );
			// check cascade
			// object definition
			Assert.IsTrue( Entity.Exists( definitionBase ), "Base Entity" );
			// relationship definition
			Assert.IsTrue( Entity.Exists( relationshipThrow ), "Relationship 'Throw'" ); //home to first
			//relationship instances - all go (as above for object instances)
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 0 ] ), "home to first" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 1 ] ), "1st to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 2 ] ), "first to second" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 3 ] ), "second to first" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 4 ] ), "second to third" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 5 ] ), "third to second" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 6 ] ), "3rd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 7 ] ), "home to 3rd" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 8 ] ), "home to 2nd" );
			Assert.IsTrue( Entity.Exists( throwInstanceIdArray[ 9 ] ), "2nd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 10 ] ), "3rd to 1st" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 11 ] ), "1st to 3rd" );
		}

		/// <summary>
		///     Test for round-the-bags framework
		///     Test for delete First base instance with one way relationship set-up
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TwoWayTest( )
		{
			EntityType definitionBase;
			Relationship relationshipThrow;
			List<Entity> baseList;
			List<Entity> throwList;
			EntityRelationshipCollection<IEntity> relationshipsCol0;
			EntityRelationshipCollection<IEntity> relationshipsCol1;
			EntityRelationshipCollection<IEntity> relationshipsCol2;
			EntityRelationshipCollection<IEntity> relationshipsCol3;
			const int collectionChoice = 3; //two way relationship set-up
			long[] baseInstanceIdArray;
			long[] throwInstanceIdArray;

			//Set-Up
			SetUpRoundTheBags( out relationshipThrow, out definitionBase, out baseList, out throwList, out baseInstanceIdArray,
			                   out throwInstanceIdArray, out relationshipsCol0, out relationshipsCol1, out relationshipsCol2, out relationshipsCol3,
			                   collectionChoice );

			//Action
			//delete first base
			IEntity firstBase = Entity.Get( baseInstanceIdArray[ 1 ], true ); //writeable //
			firstBase.Delete( );

			//Assert - third base instance is gone
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 0 ] ), "Home plate" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 1 ] ), "1st base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 2 ] ), "2nd base" );
			Assert.IsFalse( Entity.Exists( baseInstanceIdArray[ 3 ] ), "3rd base" );
			// check cascade
			// object definition
			Assert.IsTrue( Entity.Exists( definitionBase ), "Base Entity" );
			// relationship definition
			Assert.IsTrue( Entity.Exists( relationshipThrow ), "Relationship 'Throw'" );
			//relationship instances - all go (as above for object instances)
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 6 ] ), "3rd to home" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 8 ] ), "home to 2nd" );
			Assert.IsFalse( Entity.Exists( throwInstanceIdArray[ 11 ] ), "1st to 3rd" );
		}

		// end CreateDefinitionList
	}

	// end of class CascadeDeleteTests      
}

// end of namespace