// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EDC.Common;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Entity tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ResourceKeyTests
	{
		/// <summary>
		///     Clears the caches.
		/// </summary>
		private static void ClearCaches( )
		{
			EntityCache.Instance.Clear( );

			Type cacheType = Type.GetType( "EDC.ReadiNow.Model.EntityFieldCache, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral" );
			if ( cacheType != null )
			{
				PropertyInfo pi = cacheType.GetProperty( "Instance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public );
				object cache = pi.GetValue( null, null );
				cacheType.InvokeMember( "Clear", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, cache, null );

				cacheType = Type.GetType( "EDC.ReadiNow.Model.EntityFieldModificationCache, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral" );
				if ( cacheType != null )
				{
					pi = cacheType.GetProperty( "Instance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public );
				}
				cache = pi.GetValue( null, null );
				if ( cacheType != null )
				{
					cacheType.InvokeMember( "Clear", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, cache, null );
				}

				cacheType = Type.GetType( "EDC.ReadiNow.Model.EntityIdentificationCache, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral" );
				if ( cacheType != null )
				{
                    cacheType.InvokeMember("Clear", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, null, cache, null);
				}

				cacheType = Type.GetType( "EDC.ReadiNow.Model.EntityRelationshipCache, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral" );
				if ( cacheType != null )
				{
					pi = cacheType.GetProperty( "Instance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public );
				}
				cache = pi.GetValue( null, null );
				if ( cacheType != null )
				{
					cacheType.InvokeMember( "Clear", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, cache, null );
				}

				cacheType = Type.GetType( "EDC.ReadiNow.Model.EntityRelationshipModificationCache, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral" );
				if ( cacheType != null )
				{
					pi = cacheType.GetProperty( "Instance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public );
				}
				cache = pi.GetValue( null, null );
				if ( cacheType != null )
				{
					cacheType.InvokeMember( "Clear", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, cache, null );
				}

				//cacheType = Type.GetType( "EDC.ReadiNow.Model.EntityTypeCache, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral" );
				//if ( cacheType != null )
				//{
				//	pi = cacheType.GetProperty( "Instance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public );
				//}
				//cache = pi.GetValue( null, null );
				//if ( cacheType != null )
				//{
				//	cacheType.InvokeMember( "Clear", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, cache, null );
				//}
			}
		}


		/// <summary>
		///     Verifies the resource key hashes.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="resourceKey">The resource key.</param>
		/// <param name="expectedHashes">The expected hashes.</param>
		/// <param name="name">The name.</param>
		private void VerifyResourceKeyHashes( Resource resource, ResourceKey resourceKey, int expectedHashes, string name )
		{
			VerifyResourceKeyHashes( resource, Delegates.ListOfOne( resourceKey ), expectedHashes, name );
		}


		/// <summary>
		///     Verifies the resource keys.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <param name="resourceKeys">The resource keys.</param>
		/// <param name="expectedHashes">The expected hashes.</param>
		/// <param name="name">The name.</param>
		private void VerifyResourceKeyHashes( Resource resource, List<ResourceKey> resourceKeys, int expectedHashes, string name )
		{
			Assert.AreEqual( expectedHashes, resource.ResourceHasResourceKeyDataHashes.Count, "{0}: The number of key data hashes is invalid.", name );

			for ( int i = 0; i < expectedHashes; i++ )
			{
				ResourceKeyDataHash dataHash = resource.ResourceHasResourceKeyDataHashes[ i ];

				ResourceKey resourceKey = resourceKeys.FirstOrDefault( rk => rk.Id == dataHash.ResourceKeyDataHashAppliesToResourceKey.Id );
				Assert.IsNotNull( resourceKey, "{0}: The resource key was not found. Index:{1}", name, i );
				Assert.AreEqual( resourceKey.Id, dataHash.ResourceKeyDataHashAppliesToResourceKey.Id, "{0}: The data hash key is invalid. Index:{1}", name, i );
				Assert.AreEqual( resource.Id, dataHash.ResourceKeyDataHashAppliesToResource.Id, "{0}: The data hash resource is invalid. Index:{1}", name, i );
				Assert.IsNotNullOrEmpty( dataHash.DataHash, "{0}: The data hash is invalid. Index:{1}", name, i );
			}
		}

		private class TestSetupData
		{
			public EntityType CelestialBodyEntityType
			{
				get;
				private set;
			}

			public IntField CelestialBodyIdField
			{
				get;
				private set;
			}

			public ResourceKey CelestialBodyKey
			{
				get;
				private set;
			}

			public EntityType GalaxyEntityType
			{
				get;
				private set;
			}

			public StringField GalaxyNameField
			{
				get;
				private set;
			}

            public IntField GalaxyIdField
            {
                get;
                private set;
            }

            public Relationship GalaxyToPlanetRelationship
			{
				get;
				private set;
			}

		    public ResourceKey GalaxyKey
		    {
		        get;
                private set;
		    }

			public EntityType PlanetEntityType
			{
				get;
				private set;
			}

            public EntityType UniverseEntityType
            {
                get;
                private set;
            }

			public IntField PlanetIdField
			{
				get;
				private set;
			}

			public ResourceKey PlanetIdKey
			{
				get;
				private set;
			}

			public ResourceKey PlanetKey
			{
				get;
				private set;
			}

			public StringField PlanetNameField
			{
				get;
				private set;
			}

			public Relationship PlanetToGalaxyRelationship
			{
				get;
				private set;
			}

            public Relationship PlanetToUniverseRelationship
            {
                get;
                private set;
            }

            /// <summary>
            /// Sets up the following model.
            /// Planet -> Galaxy (many relationship)
            /// Planet -> Universe (many relationship)
            /// Resource key on Planet (name) (Planet -> Galaxy) (Planet -> Universe)            
            /// </summary>
            /// <returns></returns>
		    public static TestSetupData CreatePlanetTypeToManyRelationshipKeyField()
		    {
                var setupData = new TestSetupData();

                // Create the entity type
                Console.WriteLine(@"Creating Planet entity type");
                setupData.PlanetEntityType = new EntityType();
                setupData.PlanetEntityType.Inherits.Add(UserResource.UserResource_Type);
                setupData.PlanetEntityType.Name = "Planet";
                setupData.PlanetEntityType.Save();

                Console.WriteLine(@"Creating Galaxy entity type");
                setupData.GalaxyEntityType = new EntityType();
                setupData.GalaxyEntityType.Inherits.Add(UserResource.UserResource_Type);
                setupData.GalaxyEntityType.Name = "Galaxy";
                setupData.GalaxyEntityType.Save();

                setupData.UniverseEntityType = new EntityType();
                setupData.UniverseEntityType.Inherits.Add(UserResource.UserResource_Type);
                setupData.UniverseEntityType.Name = "Universe";
                setupData.UniverseEntityType.Save();

                // Create the planet name key
                Console.WriteLine(@"Creating Planet name key");
                setupData.PlanetKey = new ResourceKey
                {
                    KeyAppliesToType = setupData.PlanetEntityType
                };
                setupData.PlanetKey.Save();

                // Relationship between planet and galaxy
                // Some sort of parallel universe where a planet
                // can be in multiple galaxies
                setupData.PlanetToGalaxyRelationship = new Relationship
                {
                    FromType = setupData.PlanetEntityType,
                    ToType = setupData.GalaxyEntityType,
                    Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                };
                setupData.PlanetToGalaxyRelationship.Save();

                // Create another relationship from planet to universe
                setupData.PlanetToUniverseRelationship = new Relationship
                {
                    FromType = setupData.PlanetEntityType,
                    ToType = setupData.UniverseEntityType,
                    Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany
                };
                setupData.PlanetToUniverseRelationship.Save();

                var planetToGalaxyRelationshipKey = new ResourceKeyRelationship
                {
                    KeyRelationshipDirection_Enum = DirectionEnum_Enumeration.Forward,
                    KeyRelationship = setupData.PlanetToGalaxyRelationship
                };
                planetToGalaxyRelationshipKey.Save();

                setupData.PlanetKey.ResourceKeyRelationships.Add(planetToGalaxyRelationshipKey);

                var planetToUniverseRelationshipKey = new ResourceKeyRelationship
                {
                    KeyRelationshipDirection_Enum = DirectionEnum_Enumeration.Forward,
                    KeyRelationship = setupData.PlanetToUniverseRelationship
                };
                planetToUniverseRelationshipKey.Save();

                setupData.PlanetKey.ResourceKeyRelationships.Add(planetToUniverseRelationshipKey);

                setupData.PlanetKey.Save();

                // Create the planet name field
                Console.WriteLine(@"Creating Planet name field");
                setupData.PlanetNameField = new StringField
                {
                    Name = "PlanetName",
                    FieldIsOnType = setupData.PlanetEntityType
                };
                setupData.PlanetNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.PlanetNameField.Save();

                // Create the galaxy name field
                Console.WriteLine(@"Creating Galaxy name field");
                setupData.GalaxyNameField = new StringField
                {
                    Name = "GalaxyName",
                    FieldIsOnType = setupData.GalaxyEntityType
                };                
                setupData.GalaxyNameField.Save();

                return setupData;
		    }


			/// <summary>
			///     Creates the planet type name galaxy key fields.
			/// </summary>
			/// <param name="mergeDuplicates">
			///     if set to <c>true</c> [merge duplicates].
			/// </param>
			/// <returns></returns>
			public static TestSetupData CreateInheritedPlanetTypeNameGalaxyKeyFields( bool mergeDuplicates = false )
			{
				var setupData = new TestSetupData( );

				// Create the entity type
				Console.WriteLine( @"Creating Celestial Body entity type" );
				setupData.CelestialBodyEntityType = new EntityType( );
				setupData.CelestialBodyEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.CelestialBodyEntityType.Name = "Celestial Body";
				setupData.CelestialBodyEntityType.Save( );

				Console.WriteLine( @"Creating Celestial Body key" );
				setupData.CelestialBodyKey = new ResourceKey
					{
						KeyAppliesToType = setupData.CelestialBodyEntityType,
						MergeDuplicates = mergeDuplicates
					};
				setupData.CelestialBodyKey.Save( );

				Console.WriteLine( @"Creating Celestial Body id field" );
				setupData.CelestialBodyIdField = new IntField
					{
						Name = "Celestial Body Id",
						FieldIsOnType = setupData.CelestialBodyEntityType
					};
			    setupData.CelestialBodyIdField.FieldInKey.Add(setupData.CelestialBodyKey);
				setupData.CelestialBodyIdField.Save( );

				// Create the entity type
				Console.WriteLine( @"Creating Planet entity type" );
				setupData.PlanetEntityType = new EntityType( );
				setupData.PlanetEntityType.Inherits.Add( setupData.CelestialBodyEntityType );
				setupData.PlanetEntityType.Name = "Planet";
				setupData.PlanetEntityType.Save( );

				// Create the planet name key
				Console.WriteLine( @"Creating Planet name key" );
				setupData.PlanetKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType,
						MergeDuplicates = mergeDuplicates
					};
				setupData.PlanetKey.Save( );

				// Create the planet name field
				Console.WriteLine( @"Creating Planet name field" );
				setupData.PlanetNameField = new StringField
					{
						Name = "PlanetName",
						FieldIsOnType = setupData.PlanetEntityType
					};
			    setupData.PlanetNameField.FieldInKey.Add(setupData.PlanetKey);
				setupData.PlanetNameField.Save( );

				// Create the galaxy name field
				Console.WriteLine( @"Creating Planet Galaxy field" );
				setupData.GalaxyNameField = new StringField
					{
						Name = "GalaxyName",
						FieldIsOnType = setupData.PlanetEntityType
					};
			    setupData.GalaxyNameField.FieldInKey.Add(setupData.PlanetKey);
				setupData.GalaxyNameField.Save( );

				return setupData;
			}

			/// <summary>
			///     Creates the planet type with a forward relationship key field.
			/// </summary>
			/// <returns></returns>
			public static TestSetupData CreatePlanetTypeForwardRelationshipKeyField( )
			{
				var setupData = new TestSetupData( );

				// Create the entity type
				Console.WriteLine( @"Creating Planet entity type" );
				setupData.PlanetEntityType = new EntityType( );
				setupData.PlanetEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.PlanetEntityType.Name = "Planet";
				setupData.PlanetEntityType.Save( );

				Console.WriteLine( @"Creating Galaxy entity type" );
				setupData.GalaxyEntityType = new EntityType( );
				setupData.GalaxyEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.GalaxyEntityType.Name = "Galaxy";
				setupData.GalaxyEntityType.Save( );

				// Create the planet name key
				Console.WriteLine( @"Creating Planet name key" );
				setupData.PlanetKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType
					};
				setupData.PlanetKey.Save( );

				// Relationship between planet and galaxy
				setupData.PlanetToGalaxyRelationship = new Relationship
					{
						FromType = setupData.PlanetEntityType,
						ToType = setupData.GalaxyEntityType,
						Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne
					};
				setupData.PlanetToGalaxyRelationship.Save( );

				var planetToGalaxyRelationshipKey = new ResourceKeyRelationship
					{
						KeyRelationshipDirection_Enum = DirectionEnum_Enumeration.Forward,
                        KeyRelationship = setupData.PlanetToGalaxyRelationship
					};
                planetToGalaxyRelationshipKey.Save();
				
                setupData.PlanetKey.ResourceKeyRelationships.Add(planetToGalaxyRelationshipKey);
				setupData.PlanetKey.Save( );

				// Create the planet name field
				Console.WriteLine( @"Creating Planet name field" );
				setupData.PlanetNameField = new StringField
					{
						Name = "PlanetName",
						FieldIsOnType = setupData.PlanetEntityType
					};
				setupData.PlanetNameField.Save( );

				// Create the galaxy name field
				Console.WriteLine( @"Creating Galaxy name field" );
				setupData.GalaxyNameField = new StringField
					{
						Name = "GalaxyName",
						FieldIsOnType = setupData.GalaxyEntityType
					};
				setupData.GalaxyNameField.Save( );

				return setupData;
			}


			/// <summary>
			///     Creates the planet type name galaxy id key fields.
			/// </summary>
			/// <returns></returns>
			public static TestSetupData CreatePlanetTypeNameGalaxyIdKeyFields( )
			{
				var setupData = new TestSetupData( );

				// Create the entity type
				Console.WriteLine( @"Creating Planet entity type" );
				setupData.PlanetEntityType = new EntityType( );
				setupData.PlanetEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.PlanetEntityType.Name = "Planet";
				setupData.PlanetEntityType.Save( );

				// Create the planet name key
				Console.WriteLine( @"Creating Planet name key" );
				setupData.PlanetKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType
					};
				setupData.PlanetKey.Save( );

				// Create the planet id key
				Console.WriteLine( @"Creating Planet Id key" );
				setupData.PlanetIdKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType
					};
				setupData.PlanetIdKey.Save( );

				// Create the planet name field
				Console.WriteLine( @"Creating Planet name field" );
				setupData.PlanetNameField = new StringField
					{
						Name = "PlanetName",
						FieldIsOnType = setupData.PlanetEntityType
					};
                setupData.PlanetNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.PlanetNameField.Save();

				// Create the galaxy name field
				Console.WriteLine( @"Creating Planet Galaxy field" );
				setupData.GalaxyNameField = new StringField
					{
						Name = "GalaxyName",
						FieldIsOnType = setupData.PlanetEntityType
					};
                setupData.GalaxyNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.GalaxyNameField.Save();

				// Create the Planet Id field
				Console.WriteLine( @"Creating Planet Id" );
				setupData.PlanetIdField = new IntField
					{
						Name = "PlanetId",
						FieldIsOnType = setupData.PlanetEntityType
					};
                setupData.PlanetIdField.FieldInKey.Add(setupData.PlanetIdKey);
                setupData.PlanetIdField.Save();

				return setupData;
			}

			/// <summary>
			///     Creates the planet type name galaxy key fields.
			/// </summary>
			/// <param name="mergeDuplicates">
			///     if set to <c>true</c> [merge duplicates].
			/// </param>
			/// <returns></returns>
			public static TestSetupData CreatePlanetTypeNameGalaxyKeyFields( bool mergeDuplicates = false )
			{
				var setupData = new TestSetupData( );

				// Create the entity type
				Console.WriteLine( @"Creating Planet entity type" );
				setupData.PlanetEntityType = new EntityType( );
				setupData.PlanetEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.PlanetEntityType.Name = "Planet";
				setupData.PlanetEntityType.Save( );

				// Create the planet name key
				Console.WriteLine( @"Creating Planet name key" );
				setupData.PlanetKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType,
						MergeDuplicates = mergeDuplicates
					};
				setupData.PlanetKey.Save( );

				// Create the planet name field
				Console.WriteLine( @"Creating Planet name field" );
				setupData.PlanetNameField = new StringField
					{
						Name = "PlanetName",
						FieldIsOnType = setupData.PlanetEntityType
					};
                setupData.PlanetNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.PlanetNameField.Save();

				// Create the galaxy name field
				Console.WriteLine( @"Creating Planet Galaxy field" );
				setupData.GalaxyNameField = new StringField
					{
						Name = "GalaxyName",
						FieldIsOnType = setupData.PlanetEntityType
					};
                setupData.GalaxyNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.GalaxyNameField.Save();

				return setupData;
			}


			/// <summary>
			///     Creates the planet type name key fields and a forward relationship key field.
			/// </summary>
			/// <returns></returns>
			public static TestSetupData CreatePlanetTypeNameKeyFieldsForwardRelationshipKeyField( )
			{
				var setupData = new TestSetupData( );

				// Create the entity type
				Console.WriteLine( @"Creating Planet entity type" );
				setupData.PlanetEntityType = new EntityType( );
				setupData.PlanetEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.PlanetEntityType.Name = "Planet";
				setupData.PlanetEntityType.Save( );

				Console.WriteLine( @"Creating Galaxy entity type" );
				setupData.GalaxyEntityType = new EntityType( );
				setupData.GalaxyEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.GalaxyEntityType.Name = "Galaxy";
				setupData.GalaxyEntityType.Save( );

				// Create the planet name key
				Console.WriteLine( @"Creating Planet name key" );
				setupData.PlanetKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType
					};
				setupData.PlanetKey.Save( );

				// Relationship between planet and galaxy
				setupData.PlanetToGalaxyRelationship = new Relationship
					{
						FromType = setupData.PlanetEntityType,
						ToType = setupData.GalaxyEntityType,
						Cardinality_Enum = CardinalityEnum_Enumeration.ManyToOne
					};
				setupData.PlanetToGalaxyRelationship.Save( );

				var planetToGalaxyRelationshipKey = new ResourceKeyRelationship
					{
						KeyRelationshipDirection_Enum = DirectionEnum_Enumeration.Forward,
                        KeyRelationship = setupData.PlanetToGalaxyRelationship
					};
                planetToGalaxyRelationshipKey.Save();

                setupData.PlanetKey.ResourceKeyRelationships.Add(planetToGalaxyRelationshipKey);
				setupData.PlanetKey.Save( );

				// Create the planet name field
				Console.WriteLine( @"Creating Planet name field" );
				setupData.PlanetNameField = new StringField
					{
						Name = "PlanetName",
						FieldIsOnType = setupData.PlanetEntityType
					};
                setupData.PlanetNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.PlanetNameField.Save();

				// Create the galaxy name field
				Console.WriteLine( @"Creating Galaxy name field" );
				setupData.GalaxyNameField = new StringField
					{
						Name = "GalaxyName",
						FieldIsOnType = setupData.GalaxyEntityType
					};
				setupData.GalaxyNameField.Save( );

				return setupData;
			}


			/// <summary>
			///     Creates the planet type reverse relationship key field.
			/// </summary>
			/// <returns></returns>
			public static TestSetupData CreatePlanetTypeReverseRelationshipKeyField( )
			{
				var setupData = new TestSetupData( );

				// Create the entity type
				Console.WriteLine( @"Creating Planet entity type" );
				setupData.PlanetEntityType = new EntityType( );
				setupData.PlanetEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.PlanetEntityType.Name = "Planet";
				setupData.PlanetEntityType.Save( );

				Console.WriteLine( @"Creating Galaxy entity type" );
				setupData.GalaxyEntityType = new EntityType( );
				setupData.GalaxyEntityType.Inherits.Add( Entity.Get<EntityType>( "core:userResource" ) );
				setupData.GalaxyEntityType.Name = "Galaxy";
				setupData.GalaxyEntityType.Save( );

				// Create the planet name key
				Console.WriteLine( @"Creating Planet name key" );
				setupData.PlanetKey = new ResourceKey
					{
						KeyAppliesToType = setupData.PlanetEntityType
					};
				setupData.PlanetKey.Save( );

				// Relationship between planet and galaxy
				setupData.GalaxyToPlanetRelationship = new Relationship
					{
						FromType = setupData.GalaxyEntityType,
						ToType = setupData.PlanetEntityType,
						Cardinality_Enum = CardinalityEnum_Enumeration.OneToMany
					};
				setupData.GalaxyToPlanetRelationship.Save( );

				var galaxyToPlanetRelationshipKey = new ResourceKeyRelationship
					{
						KeyRelationshipDirection_Enum = DirectionEnum_Enumeration.Reverse,
                        KeyRelationship = setupData.GalaxyToPlanetRelationship
					};
                galaxyToPlanetRelationshipKey.Save();

                setupData.PlanetKey.ResourceKeyRelationships.Add(galaxyToPlanetRelationshipKey);
				setupData.PlanetKey.Save( );

				// Create the planet name field
				Console.WriteLine( @"Creating Planet name field" );
				setupData.PlanetNameField = new StringField
					{
						Name = "PlanetName",
						FieldIsOnType = setupData.PlanetEntityType
					};
				setupData.PlanetNameField.Save( );

				// Create the galaxy name field
				Console.WriteLine( @"Creating Galaxy name field" );
				setupData.GalaxyNameField = new StringField
					{
						Name = "GalaxyName",
						FieldIsOnType = setupData.GalaxyEntityType
					};
				setupData.GalaxyNameField.Save( );

				return setupData;
			}

		    public static TestSetupData CreatePlanetNameGalaxyIdTypeReverseRelationshipWithFieldsKey(bool mergeDuplicates = false)
		    {
                var setupData = new TestSetupData();

                // Create the galaxy type
                Console.WriteLine(@"Creating Galaxy entity type");
                setupData.GalaxyEntityType = new EntityType();
                setupData.GalaxyEntityType.Inherits.Add(Entity.Get<EntityType>("core:userResource"));
                setupData.GalaxyEntityType.Name = "Galaxy";
                setupData.GalaxyEntityType.Save();

                // Create the galaxy key
                Console.WriteLine(@"Creating Galaxy key");
                setupData.GalaxyKey = new ResourceKey
                {
                    KeyAppliesToType = setupData.GalaxyEntityType,
                    MergeDuplicates = mergeDuplicates
                };
                setupData.GalaxyKey.Save();

                // Create the galaxy name field
                Console.WriteLine(@"Creating Galaxy name field");
                setupData.GalaxyNameField = new StringField
                {
                    Name = "GalaxyName",
                    FieldIsOnType = setupData.GalaxyEntityType
                };
                setupData.GalaxyNameField.Save();

                // Create the galaxy id field
                Console.WriteLine(@"Creating Galaxy id field");
                setupData.GalaxyIdField = new IntField
                {
                    Name = "GalaxyId",
                    FieldIsOnType = setupData.GalaxyEntityType
                };
                setupData.GalaxyIdField.FieldInKey.Add(setupData.GalaxyKey);
                setupData.GalaxyIdField.Save();

                // Create the planet type
                Console.WriteLine(@"Creating Planet entity type");
                setupData.PlanetEntityType = new EntityType();
                setupData.PlanetEntityType.Inherits.Add(Entity.Get<EntityType>("core:userResource"));
                setupData.PlanetEntityType.Name = "Planet";
                setupData.PlanetEntityType.Save();

                // Relationship between planet and galaxy
                setupData.GalaxyToPlanetRelationship = new Relationship
                {
                    FromType = setupData.GalaxyEntityType,
                    ToType = setupData.PlanetEntityType,
                    Cardinality_Enum = CardinalityEnum_Enumeration.OneToMany
                };
                setupData.GalaxyToPlanetRelationship.Save();

                // Create the planet name and reverse relationship key to id field
                Console.WriteLine(@"Creating Planet key");
                var galaxyToPlanetRelationshipKey = new ResourceKeyRelationship
                {
                    KeyRelationshipDirection_Enum = DirectionEnum_Enumeration.Reverse,
                    KeyRelationship = setupData.GalaxyToPlanetRelationship
                };
                galaxyToPlanetRelationshipKey.FieldsInRelationshipKey.Add(setupData.GalaxyIdField.As<Field>());

                setupData.PlanetKey = new ResourceKey
                {
                    KeyAppliesToType = setupData.PlanetEntityType,
                    MergeDuplicates = mergeDuplicates
                };
                setupData.PlanetKey.ResourceKeyRelationships.Add(galaxyToPlanetRelationshipKey);
                setupData.PlanetKey.Save();

                // Create the planet name field
                Console.WriteLine(@"Creating Planet name field");
                setupData.PlanetNameField = new StringField
                {
                    Name = "PlanetName",
                    FieldIsOnType = setupData.PlanetEntityType
                };
                setupData.PlanetNameField.FieldInKey.Add(setupData.PlanetKey);
                setupData.PlanetNameField.Save();

                return setupData;
            }
		}


		/// <summary>
		///     Test that creating a non-duplicate entity with a key succeeds.
		///     Then ensure that changing the value of the key field of the non-duplicate
		///     to a duplicate will fail.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestChangingNonDuplicateEntityToDuplicateFails( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetField( setupData.GalaxyNameField, "Milky Way" );
			pluto.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Pluto" );

			var plutoResource = Entity.Get<Resource>( pluto.Id );
			VerifyResourceKeyHashes( plutoResource, setupData.PlanetKey, 1, "Pluto" );

			Assert.AreNotEqual( plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			// Now change Pluto's name to be Saturn
			var plutoToSaturn = Entity.Get<Entity>( pluto.Id, true );
			plutoToSaturn.SetField( setupData.PlanetNameField, "Saturn" );
			Assert.Throws<DuplicateKeyException>( () => plutoToSaturn.Save() );

			#endregion
		}

		/// <summary>
		///     Test that clearing the key fields on a key
		///     deletes all the resource key data hashes
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestClearKeyFields( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetField( setupData.GalaxyNameField, "Milky Way" );
			pluto.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			long saturnResourceKeyDataHashId = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].Id;
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			var plutoResource = Entity.Get<Resource>( pluto.Id );
			long plutoResourceKeyDataHashId = plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].Id;
			VerifyResourceKeyHashes( plutoResource, setupData.PlanetKey, 1, "Pluto" );

			Assert.AreNotEqual( plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			setupData.PlanetKey.KeyFields.Clear( );
			setupData.PlanetKey.Save( );

			// Verify that all the hashes are gone
			ClearCaches( );

			saturnResource = Entity.Get<Resource>( saturn.Id );
			Assert.AreEqual( 0, saturnResource.ResourceHasResourceKeyDataHashes.Count, "The Saturn resource should not have any key data hashes." );
			Assert.IsFalse( Entity.Exists( saturnResourceKeyDataHashId ), "The Saturn resource key data hash should not exist." );

			plutoResource = Entity.Get<Resource>( pluto.Id );
			Assert.AreEqual( 0, plutoResource.ResourceHasResourceKeyDataHashes.Count, "The Pluto resource should not have any key data hashes." );
			Assert.IsFalse( Entity.Exists( plutoResourceKeyDataHashId ), "The Pluto resource key data hash should not exist." );

			#endregion
		}


		/// <summary>
		///     Test that clearing the key field of an entity with a key clears the hash.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestClearingEntityKeyFields( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet with a key
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			long datahashId = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].Id;
			string hash = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;
			Assert.IsNotNullOrEmpty( hash, "Saturn: The data hash is invalid." );

			// Clear the field
			Console.WriteLine( @"Clearing planet Saturn key fields" );
			saturn.SetField( setupData.PlanetNameField, null );
			saturn.SetField( setupData.GalaxyNameField, null );

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                saturn.Save();

                ctx.CommitTransaction();
            }

			ClearCaches( );

			saturnResource = Entity.Get<Resource>( saturn.Id );

			Assert.AreEqual( 0, saturnResource.ResourceHasResourceKeyDataHashes.Count, "Saturn: The number of resource key data hashes is invalid." );
			Assert.AreEqual( 0, setupData.PlanetKey.ResourceKeyDataHashes.Count, "Planet key: The number of key key data hashes is invalid." );
			Assert.IsFalse( Entity.Exists( new EntityRef( datahashId ) ), "The resource data hash should not exist." );

			#endregion
		}

		/// <summary>
		///     Test that creating a key that would result in duplicates fails.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreateKeyForExistingDuplicates( )
		{
			#region Setup

			// Create the entity type
			Console.WriteLine( @"Creating Planet entity type" );
			var planetEntityType = new EntityType( );
			planetEntityType.Inherits.Add( Entity.Get<EntityType>( "core:resource" ) );
			planetEntityType.Name = "Planet";
			planetEntityType.Save( );

			// Create the planet name field
			Console.WriteLine( @"Creating Planet name field" );
			var planetNameField = new StringField
				{
					Name = "PlanetName",
					FieldIsOnType = planetEntityType
				};
			planetNameField.Save( );

			// Create the galaxy name field
			Console.WriteLine( @"Creating Planet Galaxy field" );
			var galaxyNameField = new StringField
				{
					Name = "GalaxyName",
					FieldIsOnType = planetEntityType
				};
			galaxyNameField.Save( );

			#endregion

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( planetEntityType );
			saturn.SetField( planetNameField, "Saturn" );
			saturn.SetField( galaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Saturn2" );
			var saturn2 = new Entity( planetEntityType );
			saturn2.SetField( planetNameField, "Saturn" );
			saturn2.SetField( galaxyNameField, "Milky Way" );
			saturn2.Save( );

			// Create a key on the planet name field
			var planetKey = new ResourceKey
				{
					KeyAppliesToType = planetEntityType
				};
			planetKey.KeyFields.Add( planetNameField.As<Field>( ) );

			Assert.Throws<DuplicateKeyException>( () => planetKey.Save() );

			#endregion
		}

		/// <summary>
		///     Test that creating a duplicate entity that hash a key fails.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityFails( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Try and create a duplicate.             
			Console.WriteLine( @"Creating duplicate planet Saturn" );
			var saturnDuplicate = new Entity( setupData.PlanetEntityType );
			saturnDuplicate.SetField( setupData.PlanetNameField, "Saturn" );
			saturnDuplicate.SetField( setupData.GalaxyNameField, "Milky Way" );
			// This should fail.                        
			Assert.Throws<DuplicateKeyException>( () => saturnDuplicate.Save() );

			#endregion
		}


		/// <summary>
		///     Test that creating a duplicate entity that hash a key fails.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityFailsDifferByCase( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Try and create a duplicate.             
			Console.WriteLine( @"Creating duplicate planet Saturn" );
			var saturnDuplicate = new Entity( setupData.PlanetEntityType );
			saturnDuplicate.SetField( setupData.PlanetNameField, "SATURN" );
			saturnDuplicate.SetField( setupData.GalaxyNameField, "Milky Way" );
			// This should fail.                        
			Assert.Throws<DuplicateKeyException>( () => saturnDuplicate.Save() );

			#endregion
		}


		/// <summary>
		///     Test that creating a duplicate entity with an inherited key fails.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityInheritedKeyBaseFirst( )
		{
			TestSetupData setupData = TestSetupData.CreateInheritedPlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a celestial body
			Console.WriteLine( @"Creating saturn celestial body" );
			var saturnCelestialBody = new Entity( setupData.CelestialBodyEntityType );
			saturnCelestialBody.SetField( setupData.CelestialBodyIdField, 100 );
			saturnCelestialBody.Save( );

			// Create a saturn planet type with the same celestial id
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.CelestialBodyIdField, 100 );

			Assert.Throws<DuplicateKeyException>( () => saturn.Save() );

			#endregion
		}


		/// <summary>
		///     Test that creating a duplicate entity with an inherited key fails.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityInheritedKeyDerivedFirst( )
		{
			TestSetupData setupData = TestSetupData.CreateInheritedPlanetTypeNameGalaxyKeyFields( );

			#region Test

			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.CelestialBodyIdField, 100 );
			saturn.Save( );

			// Create a celestial body with the same celestial id
			Console.WriteLine( @"Creating saturn celestial body" );
			var saturnCelestialBody = new Entity( setupData.CelestialBodyEntityType );
			saturnCelestialBody.SetField( setupData.CelestialBodyIdField, 100 );

			Assert.Throws<DuplicateKeyException>( () => saturnCelestialBody.Save() );

			#endregion
		}

		/// <summary>
		///     Test that creating a duplicate entity with multiple keys fails
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityMultipleKeys( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyIdKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.SetField( setupData.PlanetIdField, 100 );
			saturn.Save( );

			// Try and create a duplicate.             
			Console.WriteLine( @"Creating duplicate planet Saturn" );
			var saturnDuplicate = new Entity( setupData.PlanetEntityType );
			saturnDuplicate.SetField( setupData.PlanetNameField, "Saturn" );
			saturnDuplicate.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturnDuplicate.SetField( setupData.PlanetIdField, 200 );
			// This should fail.                        
			Assert.Throws<DuplicateKeyException>( () => saturnDuplicate.Save() );

			#endregion
		}


		/// <summary>
		///     Test that creating a duplicate entity keyed off a field and
		///     a relationship fails
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityRelationshipKeyFails( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameKeyFieldsForwardRelationshipKeyField( );

			#region Test

			// Create a Galaxy
			Console.WriteLine( @"Creating galaxy Milky Way" );
			var milkyWay = new Entity( setupData.GalaxyEntityType );
			milkyWay.SetField( setupData.GalaxyNameField, "Milky Way" );
			milkyWay.Save( );

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn in Milky Way" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			saturn.Save( );

			// Try and create a duplicate.             
			Console.WriteLine( @"Creating duplicate planet Saturn" );
			var saturnDuplicate = new Entity( setupData.PlanetEntityType );
			saturnDuplicate.SetField( setupData.PlanetNameField, "Saturn" );
			saturnDuplicate.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			// This should fail.                        
			Assert.Throws<DuplicateKeyException>( () => saturnDuplicate.Save() );

			#endregion
		}


		/// <summary>
		///     Test that creating a duplicate entity keyed off a relationship only fails
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingDuplicateEntityRelationshipKeyOnlyFails( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeForwardRelationshipKeyField( );

			#region Test

			// Create a Galaxy
			Console.WriteLine( @"Creating galaxy Milky Way" );
			var milkyWay = new Entity( setupData.GalaxyEntityType );
			milkyWay.SetField( setupData.GalaxyNameField, "Milky Way" );
			milkyWay.Save( );

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn in Milky Way" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "SaturnA" );
			saturn.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			saturn.Save( );

			// Try and create a duplicate.             
			Console.WriteLine( @"Creating duplicate planet Saturn" );
			var saturnDuplicate = new Entity( setupData.PlanetEntityType );
			saturnDuplicate.SetField( setupData.PlanetNameField, "SaturnB" );
			saturnDuplicate.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			// This should fail because both planets share the same galaxy
			Assert.Throws<DuplicateKeyException>( () => saturnDuplicate.Save() );

			#endregion
		}

		/// <summary>
		///     Test that creating a non-duplicate entity with a key succeeds.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingNonDuplicateEntity( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetField( setupData.GalaxyNameField, "Milky Way" );
			pluto.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			var plutoResource = Entity.Get<Resource>( pluto.Id );
			VerifyResourceKeyHashes( plutoResource, setupData.PlanetKey, 1, "Pluto" );

			Assert.AreNotEqual( plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			#endregion
		}

		/// <summary>
		///     Test that creating a duplicate entity with an inherited key succeeds.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingNonDuplicateEntityInheritedKey( )
		{
			TestSetupData setupData = TestSetupData.CreateInheritedPlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a celestial body
			Console.WriteLine( @"Creating saturn celestial body" );
			var saturnCelestialBody = new Entity( setupData.CelestialBodyEntityType );
			saturnCelestialBody.SetField( setupData.CelestialBodyIdField, 100 );
			saturnCelestialBody.Save( );

			// Create a saturn planet type with a different celestial id
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.CelestialBodyIdField, 200 );
			saturn.Save( );

			VerifyResourceKeyHashes( saturnCelestialBody.As<Resource>( ), setupData.CelestialBodyKey, 1, "Saturn Celestial Body" );

			VerifyResourceKeyHashes( saturn.As<Resource>( ), new List<ResourceKey>
				{
					setupData.PlanetKey,
					setupData.CelestialBodyKey
				}, 2, "Saturn" );

			#endregion
		}

		/// <summary>
		///     Test that creating a non-duplicate entity with multiple key succeeds.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingNonDuplicateEntityMultipleKeys( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyIdKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.SetField( setupData.PlanetIdField, 100 );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetField( setupData.GalaxyNameField, "Milky Way" );
			pluto.SetField( setupData.PlanetIdField, 200 );
			pluto.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, new List<ResourceKey>
				{
					setupData.PlanetKey,
					setupData.PlanetIdKey
				}, 2, "Saturn" );

			var plutoResource = Entity.Get<Resource>( pluto.Id );
			VerifyResourceKeyHashes( plutoResource, new List<ResourceKey>
				{
					setupData.PlanetKey,
					setupData.PlanetIdKey
				}, 2, "Pluto" );

			#endregion
		}

		/// <summary>
		///     Test that creating a non-duplicate entity keyed off a field and
		///     a relationship
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingNonDuplicateEntityRelationshipKey( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameKeyFieldsForwardRelationshipKeyField( );

			#region Test

			// Create a Galaxy
			Console.WriteLine( @"Creating galaxy Milky Way" );
			var milkyWay = new Entity( setupData.GalaxyEntityType );
			milkyWay.SetField( setupData.GalaxyNameField, "Milky Way" );
			milkyWay.Save( );

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn in Milky Way" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			saturn.Save( );
			VerifyResourceKeyHashes( saturn.As<Resource>( ), setupData.PlanetKey, 1, "Saturn" );

			// Create another planet
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			pluto.Save( );
			VerifyResourceKeyHashes( pluto.As<Resource>( ), setupData.PlanetKey, 1, "Pluto" );

			#endregion
		}


		/// <summary>
		///     Test that creating a non-duplicate entity keyed off a relationship only
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingNonDuplicateEntityRelationshipOnlyKey( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeForwardRelationshipKeyField( );

			#region Test

			// Create a Galaxy
			Console.WriteLine( @"Creating galaxy Milky Way" );
			var milkyWay = new Entity( setupData.GalaxyEntityType );
			milkyWay.SetField( setupData.GalaxyNameField, "Milky Way" );
			milkyWay.Save( );

			Console.WriteLine( @"Creating galaxy Andromeda" );
			var andromeda = new Entity( setupData.GalaxyEntityType );
			andromeda.SetField( setupData.GalaxyNameField, "Andromeda" );
			andromeda.Save( );

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn in Milky Way" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Forward );
			saturn.Save( );
			VerifyResourceKeyHashes( saturn.As<Resource>( ), setupData.PlanetKey, 1, "Saturn" );

			// Create another planet
			Console.WriteLine( @"Creating planet Saturn in Andromeda" );
			var saturnAndromeda = new Entity( setupData.PlanetEntityType );
			saturnAndromeda.SetField( setupData.PlanetNameField, "Saturn" );
			saturnAndromeda.SetRelationships( setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( andromeda ).ToEntityRelationshipCollection( ), Direction.Forward );
			saturnAndromeda.Save( );
			VerifyResourceKeyHashes( saturnAndromeda.As<Resource>( ), setupData.PlanetKey, 1, "Saturn" );

			#endregion
		}


		/// <summary>
		///     Test that creating a non-duplicate entity keyed off a relationship only
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingNonDuplicateEntityReverseRelationshipOnlyKey( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeReverseRelationshipKeyField( );

			#region Test

			// Create a Galaxy
			Console.WriteLine( @"Creating galaxy Milky Way" );
			var milkyWay = new Entity( setupData.GalaxyEntityType );
			milkyWay.SetField( setupData.GalaxyNameField, "Milky Way" );
			milkyWay.Save( );

			Console.WriteLine( @"Creating galaxy Andromeda" );
			var andromeda = new Entity( setupData.GalaxyEntityType );
			andromeda.SetField( setupData.GalaxyNameField, "Andromeda" );
			andromeda.Save( );

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn in Milky Way" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetRelationships( setupData.GalaxyToPlanetRelationship, new EntityRelationship<IEntity>( milkyWay ).ToEntityRelationshipCollection( ), Direction.Reverse );
			saturn.Save( );
			VerifyResourceKeyHashes( saturn.As<Resource>( ), setupData.PlanetKey, 1, "Saturn" );

			// Create another planet
			Console.WriteLine( @"Creating planet Saturn in Andromeda" );
			var saturnAndromeda = new Entity( setupData.PlanetEntityType );
			saturnAndromeda.SetField( setupData.PlanetNameField, "Saturn" );
			saturnAndromeda.SetRelationships( setupData.GalaxyToPlanetRelationship, new EntityRelationship<IEntity>( andromeda ).ToEntityRelationshipCollection( ), Direction.Reverse );
			saturnAndromeda.Save( );
			VerifyResourceKeyHashes( saturnAndromeda.As<Resource>( ), setupData.PlanetKey, 1, "Saturn" );

			#endregion
		}

		/// <summary>
		///     Tests creating the other end of a relationship field resulting in a duplicate key.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestCreatingOtherEndOfARelationshipFieldDuplicates( )
		{
			string reportName = "My Test Report " + Guid.NewGuid( );
			string folderName = "My Test Folder " + Guid.NewGuid( );

			// Create a report
			var report1 = new Report
			{
				Name = reportName
			};
			report1.Save( );

			// Create a folder and save it.
			var folder1 = new Folder
			{
				Name = folderName
			};
			folder1.Save( );

			// Add the report to folder 1 and save
			folder1.FolderContents.Add( report1.As<Resource>( ) );
			folder1.Save( );

			// Create a second report with the same name as the first
			var report2 = new Report
			{
				Name = reportName
			};
			report2.Save( );

			// Add the second report to the folder.
			folder1.FolderContents.Add( report2.As<Resource>( ) );

			// This should not fail because the resource key has been deleted
			Assert.DoesNotThrow( () => folder1.Save() );
		}


		/// <summary>
		///     Test that deleting a key
		///     deletes all the resource key data hashes
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestDeleteKey( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetField( setupData.GalaxyNameField, "Milky Way" );
			pluto.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			long saturnResourceKeyDataHashId = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].Id;
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			var plutoResource = Entity.Get<Resource>( pluto.Id );
			long plutoResourceKeyDataHashId = plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].Id;
			VerifyResourceKeyHashes( plutoResource, setupData.PlanetKey, 1, "Pluto" );

			Assert.AreNotEqual( plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			// Delete the resource key
			Entity.Delete( setupData.PlanetKey.Id );

			// Verify that all the hashes are gone
			ClearCaches( );

			saturnResource = Entity.Get<Resource>( saturn.Id );
			Assert.AreEqual( 0, saturnResource.ResourceHasResourceKeyDataHashes.Count, "The Saturn resource should not have any key data hashes." );
			Assert.IsFalse( Entity.Exists( saturnResourceKeyDataHashId ), "The Saturn resource key data hash should not exist." );

			plutoResource = Entity.Get<Resource>( pluto.Id );
			Assert.AreEqual( 0, plutoResource.ResourceHasResourceKeyDataHashes.Count, "The Pluto resource should not have any key data hashes." );
			Assert.IsFalse( Entity.Exists( plutoResourceKeyDataHashId ), "The Pluto resource key data hash should not exist." );

			#endregion
		}

		/// <summary>
		///     Test that deleting a resource with a key deletes the hashes.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestDeletingResourceWithKey( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			long saturnResourceDataHashId = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].Id;
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			// Delete Saturn
			Entity.Delete( saturnResource.Id );

			ClearCaches( );

			// Verify that the hashes are gone            
			var planetKey = Entity.Get<ResourceKey>( setupData.PlanetKey.Id );

			Assert.AreEqual( 0, planetKey.ResourceKeyDataHashes.Count, "The resource key should not have any data hashes." );
			Assert.IsFalse( Entity.Exists( saturnResourceDataHashId ), "The data hash resource should not exist" );

			#endregion
		}

		/// <summary>
		///     Merge Duplicates Test
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestMergeDuplicateEntity( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( true );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			var saturnResource = saturn.As<Resource>( );
			saturnResource.SetField( "core:name", "Saturn" );
			saturnResource.SetField( setupData.PlanetNameField, "Saturn" );
			saturnResource.SetField( setupData.GalaxyNameField, "Milky Way" );
            saturnResource.ResourceInFolder.Add(Entity.Get<NavContainer>("console:homeSection"));

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                saturnResource.Save();

                ctx.CommitTransaction();
            }                

			// Try and create a duplicate.             
			Console.WriteLine( @"Creating duplicate planet Saturn" );
			var saturnDuplicate = new Entity( setupData.PlanetEntityType );
			var saturnDuplicateResource = saturnDuplicate.As<Resource>( );
			saturnDuplicateResource.SetField( setupData.PlanetNameField, "Saturn" );
			saturnDuplicateResource.SetField( setupData.GalaxyNameField, "Milky Way" );
			//This should merge the old resource data                
			
            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                saturnDuplicateResource.Save();

                ctx.CommitTransaction();
            }

            ClearCaches( );

			saturnDuplicateResource = Entity.Get<Resource>( saturnDuplicateResource.Id );
			Assert.AreEqual( "Saturn", saturnDuplicateResource.Name );
			Assert.AreEqual( "Saturn", saturnDuplicateResource.GetField( setupData.PlanetNameField ) );
			Assert.AreEqual( "Milky Way", saturnDuplicateResource.GetField( setupData.GalaxyNameField ) );
			Assert.AreEqual( Entity.Get<NavContainer>( "console:homeSection" ).Id, saturnDuplicateResource.ResourceInFolder.FirstOrDefault().Id );

			// Ensure that the duplicate is removed.
			Assert.IsFalse( Entity.Exists( new EntityRef( saturnResource.Id ) ) );

			#endregion
		}

		/// <summary>
		///     Test that creating a duplicate entity with an inherited key succeeds
		///     merges successfully.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestMergingDuplicateEntityInheritedKeyDemote( )
		{
			TestSetupData setupData = TestSetupData.CreateInheritedPlanetTypeNameGalaxyKeyFields( true );

			#region Test

			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( "core:name", "Saturn Planet" );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.CelestialBodyIdField, 100 );
			saturn.Save( );

			// Create a celestial body
			Console.WriteLine( @"Creating saturn celestial body" );
			var saturnCelestialBody = new Entity( setupData.CelestialBodyEntityType );
			saturnCelestialBody.SetField( setupData.CelestialBodyIdField, 100 );
			// This should fail because the merging will cause a demotion which
			// result in a loss of data.
			Assert.Throws<DuplicateKeyException>( () => saturnCelestialBody.Save() );

			#endregion
		}

		/// <summary>
		///     Test that creating a duplicate entity with an inherited key succeeds
		///     merges successfully.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestMergingDuplicateEntityInheritedKeyPromote( )
		{
			TestSetupData setupData = TestSetupData.CreateInheritedPlanetTypeNameGalaxyKeyFields( true );

			#region Test

			// Create a celestial body
			Console.WriteLine( @"Creating saturn celestial body" );
			var saturnCelestialBody = new Entity( setupData.CelestialBodyEntityType );
			saturnCelestialBody.SetField( "core:name", "Saturn Celestial Body" );
			saturnCelestialBody.SetField( setupData.CelestialBodyIdField, 100 );
			saturnCelestialBody.Save( );

			// Create a saturn planet type with the same celestial id
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.CelestialBodyIdField, 100 );

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                // This should merge the celestial body to be saturn
                saturn.Save();

                ctx.CommitTransaction();
            }

			ClearCaches( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			// Verify that saturn is still a planet
			EntityType firstOrDefault = saturnResource.EntityTypes.Cast<EntityType>( ).FirstOrDefault( );
			Assert.IsTrue( firstOrDefault != null && firstOrDefault.Id == setupData.PlanetEntityType.Id,
			               "The saturn resource is of the incorrect type." );
			// Verify that the name field from the saturn celestial body was merged
			Assert.AreEqual( "Saturn Celestial Body", saturnResource.GetField( "core:name" ) );
			Assert.IsFalse( Entity.Exists( saturnCelestialBody.Id ), "The celestial body should not exist." );

			VerifyResourceKeyHashes( saturnResource, new List<ResourceKey>
				{
					setupData.PlanetKey,
					setupData.CelestialBodyKey
				}, 2, "Saturn" );

			#endregion
		}

		/// <summary>
		///     Test that saving an entity without updating the
		///     key fields works
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestSaveEntityNoKeyFieldChanges( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyIdKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			string hash = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;

			// Update the name field and ensure that the hash does not change
			Console.WriteLine( @"Save planet Saturn field name" );
			var updatedSaturn = Entity.Get<Resource>( saturn.Id, true );
			updatedSaturn.Save( );

			VerifyResourceKeyHashes( updatedSaturn, setupData.PlanetKey, 1, "Updated Saturn" );

			string updatedHash = updatedSaturn.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;
			Assert.AreEqual( hash, updatedHash, "The data hash was updated" );

			#endregion
		}


		/// <summary>
		///     Tests saving the other end of a relationship field resulting in a duplicate.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestSavingOtherEndOfARelationshipFieldDuplicate( )
		{
			using ( DatabaseContext.GetContext( true ) )
			{
				string reportName = "My Test Report " + Guid.NewGuid( );
				string folderName = "My Test Folder " + Guid.NewGuid( );

				// Create a report
				var report1 = new Report
				{
					Name = reportName
				};
				report1.Save( );

				// Create a folder and add report1 to it.
				var folder1 = new Folder
				{
					Name = folderName
				};
				folder1.FolderContents.Add( report1.As<Resource>( ) );
				folder1.Save( );

				// Create a second report with the same name as the first
				var report2 = new Report
				{
					Name = reportName
				};
				report2.Save( );

				// Add the second report to the folder.
				folder1.FolderContents.Add( report2.As<Resource>( ) );

                // This should not fail because the resource key has been deleted
                Assert.DoesNotThrow(() => folder1.Save());
			}
		}

		/// <summary>
		///     Test that updating a resource key that does not
		///     result in duplicates succeeds.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestUpdateResourceKeyNoDuplicates( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Pluto" );
			var pluto = new Entity( setupData.PlanetEntityType );
			pluto.SetField( setupData.PlanetNameField, "Pluto" );
			pluto.SetField( setupData.GalaxyNameField, "Milky Way" );
			pluto.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );
			string saturnHash1 = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;

			var plutoResource = Entity.Get<Resource>( pluto.Id );
			VerifyResourceKeyHashes( plutoResource, setupData.PlanetKey, 1, "Pluto" );
			string plutoHash1 = plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;

			Assert.AreNotEqual( plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			// Update the key so that only the name field is a key            
			setupData.PlanetKey.KeyFields.Remove( setupData.GalaxyNameField.As<Field>( ) );

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                setupData.PlanetKey.Save();

                ctx.CommitTransaction();
            }

			saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );
			Assert.AreNotEqual( saturnHash1, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "Saturn: The data hashes should not match." );

			plutoResource = Entity.Get<Resource>( pluto.Id );
			VerifyResourceKeyHashes( plutoResource, setupData.PlanetKey, 1, "Pluto" );
			Assert.AreNotEqual( plutoHash1, plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "Pluto: The data hashes should not match." );

			Assert.AreNotEqual( plutoResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			#endregion
		}

		/// <summary>
		///     Test that updating a resource key by removing a key field
		///     that results in duplicates fails.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestUpdateResourceKeyResultingInDuplicates( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			// Create another planet 
			Console.WriteLine( @"Creating planet Saturn in Andromeda" );
			var saturnAndromeda = new Entity( setupData.PlanetEntityType );
			saturnAndromeda.SetField( setupData.PlanetNameField, "Saturn" );
			saturnAndromeda.SetField( setupData.GalaxyNameField, "Andromeda" );
			saturnAndromeda.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			var saturnAndromedaResource = Entity.Get<Resource>( saturnAndromeda.Id );
			VerifyResourceKeyHashes( saturnAndromedaResource, setupData.PlanetKey, 1, "Saturn Andromeda" );

			Assert.AreNotEqual( saturnAndromedaResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash, "The data hashes should not match." );

			// Update the key so that only the name field is a key
			// This should fail as this will result in multiple Saturn resources
			setupData.PlanetKey.KeyFields.Remove( setupData.GalaxyNameField.As<Field>( ) );
			Assert.Throws<DuplicateKeyException>( ( ) => setupData.PlanetKey.Save( ) );

			#endregion
		}

		/// <summary>
		///     Test that updating the key field of an entity with a key updates the hash.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestUpdatingEntityKeyField( )
		{
			TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields( );

			#region Test

			// Create a planet
			Console.WriteLine( @"Creating planet Saturn" );
			var saturn = new Entity( setupData.PlanetEntityType );
			saturn.SetField( setupData.PlanetNameField, "Saturn" );
			saturn.SetField( setupData.GalaxyNameField, "Milky Way" );
			saturn.Save( );

			var saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "Saturn" );

			string hash = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;

			// Update the name field and ensure that the hash changes
			Console.WriteLine( @"Updating planet Saturn field name" );
			saturn.SetField( setupData.PlanetNameField, "SATURN2" );

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                saturn.Save();

                ctx.CommitTransaction();
            }            

			ClearCaches( );

			saturnResource = Entity.Get<Resource>( saturn.Id );
			VerifyResourceKeyHashes( saturnResource, setupData.PlanetKey, 1, "SATURN" );

			string updatedHash = saturnResource.ResourceHasResourceKeyDataHashes[ 0 ].DataHash;
			Assert.AreNotEqual( hash, updatedHash, "The data hash was not updated" );

			#endregion
		}


        /// <summary>
		/// </summary>
		[Test]
        [RunAsDefaultTenant]
        public void TestDoesNotDeleteHashWhenUpdatingKeyFieldWithDuplicateInBatch()
        {
            TestSetupData setupData = TestSetupData.CreatePlanetTypeNameGalaxyKeyFields();

            #region Test

            // Create a planet
            Console.WriteLine(@"Creating planet Saturn");
            var saturn = new Entity(setupData.PlanetEntityType);
            saturn.SetField(setupData.PlanetNameField, "Saturn");
            saturn.SetField(setupData.GalaxyNameField, "Milky Way");
            saturn.Save();

            var saturnResource = Entity.Get<Resource>(saturn.Id);
            VerifyResourceKeyHashes(saturnResource, setupData.PlanetKey, 1, "Saturn");

            string hash1 = saturnResource.ResourceHasResourceKeyDataHashes[0].DataHash;

            // Create a planet
            Console.WriteLine(@"Creating planet Jupiter");
            var jupiter = new Entity(setupData.PlanetEntityType);
            jupiter.SetField(setupData.PlanetNameField, "Jupiter");
            jupiter.SetField(setupData.GalaxyNameField, "Milky Way");
            jupiter.Save();

            var jupiteResource = Entity.Get<Resource>(jupiter.Id);
            VerifyResourceKeyHashes(jupiteResource, setupData.PlanetKey, 1, "Jupiter");

            string hash2 = jupiteResource.ResourceHasResourceKeyDataHashes[0].DataHash;

            // Update the name field of Saturn and create a duplicate Jupiter in a batch            
            saturn.SetField(setupData.PlanetNameField, "SATURN2");

            var jupiter2 = new Entity(setupData.PlanetEntityType);
            jupiter2.SetField(setupData.PlanetNameField, "Jupiter");
            jupiter2.SetField(setupData.GalaxyNameField, "Milky Way");

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                Assert.Throws<DuplicateKeyException>(() => Entity.Save(new List<IEntity> { saturn, jupiter2 }));
                
                ctx.CommitTransaction();
            }

            ClearCaches();

            saturnResource = Entity.Get<Resource>(saturn.Id);
            VerifyResourceKeyHashes(saturnResource, setupData.PlanetKey, 1, "Saturn");

            string originalHash = saturnResource.ResourceHasResourceKeyDataHashes[0].DataHash;
            Assert.AreEqual(hash1, originalHash, "The data hash was updated");

            #endregion
        }


        /// <summary>
        ///     Tests updating the other end of a relationship field resulting in a duplicate key.
        /// </summary>
        [Test]
		[RunAsDefaultTenant]
		public void TestUpdatingOtherEndOfARelationshipFieldDuplicate( )
		{
			using ( DatabaseContext.GetContext( true ) )
			{
				string reportName = "My Test Report " + Guid.NewGuid( );
				string folerName = "My Test Folder " + Guid.NewGuid( );

				// Create a report
				var report1 = new Report
				{
					Name = reportName
				};
				report1.Save( );

				// Create a folder and save it.
				var folder1 = new Folder
				{
					Name = folerName
				};
				folder1.Save( );

				// Add the report to folder 1 and save
				folder1.FolderContents.Add( report1.As<Resource>( ) );
				folder1.Save( );

				// Create a second report with the same name as the first
				var report2 = new Report
				{
					Name = reportName
				};
				report2.Save( );

				// Add the second report to the folder.
				folder1.FolderContents.Add( report2.As<Resource>( ) );

                // This should not fail because the resource key has been deleted
                Assert.DoesNotThrow(() => folder1.Save());
			}
		}


		/// <summary>
		///     Tests updating the other end of a relationship field resulting in
		///     no duplicates.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestUpdatingOtherEndOfARelationshipFieldNoDuplicate( )
		{
			string report1Name = "My Test Report1 " + Guid.NewGuid( );
			string report2Name = "My Test Report2 " + Guid.NewGuid( );
			string folderName = "My Test Folder " + Guid.NewGuid( );

			// Create a report
			var report1 = new Report
			{
				Name = report1Name
			};
			report1.Save( );

			// Create a folder and save it.
			var folder1 = new Folder
			{
				Name = folderName
			};
			folder1.Save( );

			// Add the report to folder 1 and save
			folder1.FolderContents.Add( report1.As<Resource>( ) );

			// Create a second report with the same name as the first
			var report2 = new Report
			{
				Name = report2Name
			};
			report2.Save( );

			// Add the second report to the folder.
			folder1.FolderContents.Add( report2.As<Resource>( ) );
			folder1.Save( );
		}


        /// <summary>
        /// Execute tests to verify to many relationship key field.
        /// </summary>
	    [Test]
	    [RunAsDefaultTenant]
	    public void TestToManyRelationshipKeyField()
	    {
            // Setup the model for the tests
            TestSetupData setupData = TestSetupData.CreatePlanetTypeToManyRelationshipKeyField();

            var galaxy1 = new Entity(setupData.GalaxyEntityType);            
            galaxy1.Save();

            var galaxy2 = new Entity(setupData.GalaxyEntityType);
            galaxy2.Save();

            var galaxy3 = new Entity(setupData.GalaxyEntityType);
            galaxy3.Save();

            var universe1 = new Entity(setupData.UniverseEntityType);
            universe1.Save();

            var universe2 = new Entity(setupData.UniverseEntityType);
            universe2.Save();

            var universe3 = new Entity(setupData.UniverseEntityType);
            universe3.Save();

            // Create a planet which is in multiple galaxies and single universe.
            var saturn = new Entity(setupData.PlanetEntityType);
            var saturnResource = saturn.As<Resource>();
            saturnResource.SetField("core:name", "Saturn");
            saturnResource.SetField(setupData.PlanetNameField, "Saturn");
            saturnResource.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationshipCollection<IEntity>() { galaxy1, galaxy2 }, Direction.Forward);
            saturnResource.SetRelationships(setupData.PlanetToUniverseRelationship, new EntityRelationship<IEntity>(universe1).ToEntityRelationshipCollection(), Direction.Forward);

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                saturnResource.Save();

                ctx.CommitTransaction();
            }

            // We should have two hashes as there are two relationships
            VerifyResourceKeyHashes(saturnResource, setupData.PlanetKey, 2, "Saturn");

            // Create a planet which is in multiple galaxies and multiple universes.
            var pluto = new Entity(setupData.PlanetEntityType);
            var plutoResource = pluto.As<Resource>();
            plutoResource.SetField("core:name", "Pluto");
            plutoResource.SetField(setupData.PlanetNameField, "Pluto");            
            plutoResource.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationshipCollection<IEntity>() { galaxy1, galaxy2 }, Direction.Forward);            
            plutoResource.SetRelationships(setupData.PlanetToUniverseRelationship, new EntityRelationshipCollection<IEntity>() { universe1, universe2 }, Direction.Forward);            

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                plutoResource.Save();

                ctx.CommitTransaction();
            }

            // We should have four hashes. (g1,u1), (g1,u2), (g2,u1), (g2,u2)
            VerifyResourceKeyHashes(plutoResource, setupData.PlanetKey, 4, "Pluto");

            // Create a saturn in the same galaxy and universe as above. This is a duplicate.
            var saturnEntity = new Entity(setupData.PlanetEntityType);
            var saturnDuplicate = saturnEntity.As<Resource>();
            saturnDuplicate.SetField("core:name", "Saturn");
            saturnDuplicate.SetField(setupData.PlanetNameField, "Saturn");
            saturnDuplicate.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>(galaxy1).ToEntityRelationshipCollection(), Direction.Forward);
            saturnDuplicate.SetRelationships(setupData.PlanetToUniverseRelationship, new EntityRelationship<IEntity>(universe1).ToEntityRelationshipCollection(), Direction.Forward);
            Assert.Throws<DuplicateKeyException>(() => saturnDuplicate.Save() );

            // Create a saturn in the same galaxies and universe as above. This is a duplicate.
            saturnEntity = new Entity(setupData.PlanetEntityType);
            saturnDuplicate = saturnEntity.As<Resource>();
            saturnDuplicate.SetField("core:name", "Saturn");
            saturnDuplicate.SetField(setupData.PlanetNameField, "Saturn");            
            saturnDuplicate.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationshipCollection<IEntity>() { galaxy1, galaxy2 }, Direction.Forward);
            saturnDuplicate.SetRelationships(setupData.PlanetToUniverseRelationship, new EntityRelationship<IEntity>(universe1).ToEntityRelationshipCollection(), Direction.Forward);
            Assert.Throws<DuplicateKeyException>(() => saturnDuplicate.Save() );

            // Create another Saturn in the same galaxy but different universe. This is not a duplicate.
            var saturnNonDuplicateEntity = new Entity(setupData.PlanetEntityType);
            var saturnNonDuplicate = saturnNonDuplicateEntity.As<Resource>();
            saturnNonDuplicate.SetField("core:name", "Saturn");
            saturnNonDuplicate.SetField(setupData.PlanetNameField, "Saturn");
            saturnNonDuplicate.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>( galaxy1).ToEntityRelationshipCollection(), Direction.Forward);
            saturnNonDuplicate.SetRelationships(setupData.PlanetToUniverseRelationship, new EntityRelationship<IEntity>(universe3).ToEntityRelationshipCollection(), Direction.Forward);

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                saturnNonDuplicate.Save();

                ctx.CommitTransaction();
            }            

            // We should have 1 hash
            VerifyResourceKeyHashes(saturnNonDuplicate, setupData.PlanetKey, 1, "Saturn");

            // Create a new pluto planet which is in galaxy1 only. This is not duplicate.
            var plutoNonDuplicate = new Entity(setupData.PlanetEntityType);
            var plutoNonDuplicateResource = plutoNonDuplicate.As<Resource>();
            plutoNonDuplicateResource.SetField("core:name", "Pluto");
            plutoNonDuplicateResource.SetField(setupData.PlanetNameField, "Pluto");
            plutoNonDuplicateResource.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationship<IEntity>(galaxy1).ToEntityRelationshipCollection(), Direction.Forward);

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                plutoNonDuplicateResource.Save();

                ctx.CommitTransaction();
            }
            
            VerifyResourceKeyHashes(plutoNonDuplicateResource, setupData.PlanetKey, 1, "Pluto");

            // Create a planet which is in multiple galaxies
            var jupiter = new Entity(setupData.PlanetEntityType);
            var jupiterResource = jupiter.As<Resource>();
            jupiterResource.SetField("core:name", "Jupiter");
            jupiterResource.SetField(setupData.PlanetNameField, "Jupiter");
            jupiterResource.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationshipCollection<IEntity>() { galaxy1, galaxy2 }, Direction.Forward);

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                jupiterResource.Save();

                ctx.CommitTransaction();
            }            

            VerifyResourceKeyHashes(jupiterResource, setupData.PlanetKey, 2, "Jupiter");

            ISet<string> jupiterHashes = jupiterResource.ResourceHasResourceKeyDataHashes.Select(dh => dh.DataHash).ToSet();
            Assert.AreEqual(2, jupiterHashes.Count);

            // Save the container and ensure that the contained elements hashes are updated
            galaxy3.SetRelationships(setupData.PlanetToGalaxyRelationship, new EntityRelationshipCollection<IEntity>() { jupiterResource }, Direction.Reverse);

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                galaxy3.Save();

                ctx.CommitTransaction();
            }            

            jupiterResource = Entity.Get<Resource>(jupiter.Id, true);

            // Jupiter should now have 3 hashes
            VerifyResourceKeyHashes(jupiterResource, setupData.PlanetKey, 3, "Jupiter");

            // Remove the relationships and save
            jupiterResource.SetRelationships(setupData.PlanetToGalaxyRelationship, null, Direction.Forward);

            using (DatabaseContext ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                jupiterResource.Save();

                ctx.CommitTransaction();
            }            

            jupiterResource = Entity.Get<Resource>(jupiter.Id);

            // Should have one hash, which is due to the name field
            VerifyResourceKeyHashes(jupiterResource, setupData.PlanetKey, 1, "Jupiter");
	    }

        /// <summary>
        /// Execute tests to verify use of optional fields on a relationship resource key.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestRelationshipKeyWithFields()
	    {
            TestSetupData setupData = TestSetupData.CreatePlanetNameGalaxyIdTypeReverseRelationshipWithFieldsKey(true);

            #region Test

            // Create a Galaxy
            Console.WriteLine(@"Creating galaxy Milky Way");
            var milkyWay = new Entity(setupData.GalaxyEntityType);
            milkyWay.SetField(setupData.GalaxyNameField, "Milky Way");
            milkyWay.SetField(setupData.GalaxyIdField, 100);
            milkyWay.Save();
            VerifyResourceKeyHashes(milkyWay.As<Resource>(), setupData.GalaxyKey, 1, "Milky Way");

            var milkyWayId = milkyWay.Id;

            // Create a planet
            Console.WriteLine(@"Creating planet Earth in Milky Way");
            var earth = new Entity(setupData.PlanetEntityType);
            earth.SetField(setupData.PlanetNameField, "Earth");
            earth.SetRelationships(setupData.GalaxyToPlanetRelationship, new EntityRelationship<IEntity>(milkyWay).ToEntityRelationshipCollection(), Direction.Reverse);
            earth.Save();
            VerifyResourceKeyHashes(earth.As<Resource>(), setupData.PlanetKey, 1, "Earth");

            var earthId = earth.Id;

            // Save over the top of the Milky Way, force a merge so it receives new Entity.Id.
            Console.WriteLine(@"Force merge galaxies");
            var winterStreet = new Entity(setupData.GalaxyEntityType);
            winterStreet.SetField(setupData.GalaxyNameField, "Winter Street");
            winterStreet.SetField(setupData.GalaxyIdField, 100);
            winterStreet.Save();
            VerifyResourceKeyHashes(winterStreet.As<Resource>(), setupData.GalaxyKey, 1, "Winter Street");

            Assert.AreNotEqual(milkyWayId, winterStreet.Id, "The merge should have resolved to the newest resource.");
            Assert.AreEqual(winterStreet.GetRelationships(setupData.GalaxyToPlanetRelationship).Count, 1);
            Assert.Null(Entity.Get(milkyWayId));

            // Check that no duplicate of Earth can sneak in
            Console.WriteLine(@"Creating Earth in the Milky Way");
            var earth2 = new Entity(setupData.PlanetEntityType);
            earth2.SetField(setupData.PlanetNameField, "Earth");
            earth2.SetRelationships(setupData.GalaxyToPlanetRelationship, new EntityRelationship<IEntity>(winterStreet).ToEntityRelationshipCollection(), Direction.Reverse);
            earth2.Save();
            VerifyResourceKeyHashes(earth2.As<Resource>(), setupData.PlanetKey, 1, "Earth 2");

            Assert.AreNotEqual(earthId, earth2.Id);
            Assert.Null(Entity.Get(earthId));

            var galaxy = Entity.Get(winterStreet.Id);
            Assert.NotNull(galaxy);
            Assert.AreEqual(galaxy.GetField(setupData.GalaxyNameField), "Winter Street");
            Assert.AreEqual(galaxy.GetRelationships(setupData.GalaxyToPlanetRelationship).Count, 1);

            // Create a different planet 
            Console.WriteLine(@"Creating planet Mars");
            var mars = new Entity(setupData.PlanetEntityType);
            mars.SetField(setupData.PlanetNameField, "Mars");
            mars.SetRelationships(setupData.GalaxyToPlanetRelationship, new EntityRelationship<IEntity>(winterStreet).ToEntityRelationshipCollection(), Direction.Reverse);
            mars.Save();
            VerifyResourceKeyHashes(mars.As<Resource>(), setupData.PlanetKey, 1, "Mars");

            galaxy = Entity.Get(winterStreet.Id);
            Assert.NotNull(galaxy);
            Assert.AreEqual(galaxy.GetField(setupData.GalaxyNameField), "Winter Street");
            Assert.AreEqual(galaxy.GetRelationships(setupData.GalaxyToPlanetRelationship).Count, 2);

            #endregion
        }
    }
}